/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Windows.Kinect;
using System.Net.Sockets;
using System.Net;
using System.Text;

// ReSharper disable once CheckNamespace
public enum CalibrationProcess
{
	None,
	FindCenter,
	FindForward,
	GetPlane,
	CalcNormal
}

public class Tracker : MonoBehaviour
{
    public Dictionary<string, Sensor> Sensors { get; private set; }

    private Dictionary<int, Human> _humans;

    public List<int>    IdIntList;
    public List<string> IdList;

    private List<Human> _deadHumans;
    private List<Human> _humansToKill;
    
    public CalibrationProcess CalibrationStatus { get; set; }

    private UdpBroadcast _udpBroadcast;

    private SafeWriteFile _safeWriteFile;

    //private OptitrackManager _localOptitrackManager;

    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public Material WhiteMaterial;

    private TrackerUI _localTrackerUi;

    private GameObject _centroGameObject;


    public string[] UnicastClients
    {
		get
        {
			return _udpBroadcast.UnicastClients;
		}
	}


    public int ShowHumanBodies = -1;
    public int CountHuman;

    public bool ColorHumans;
    public bool colorHumans;

    private bool _setUpCentro;


    public Vector3 Centro;

    public bool SendKnees;
    //public List<KneesInfo> RightKneesInfo;
    //public List<KneesInfo> LeftKneesInfo;

    public Dictionary<string, KneesInfo> RightKneesInfo;
    public Dictionary<string, KneesInfo> LeftKneesInfo;

    private Dictionary<string, KneesInfo> _allKneesInfo;
    private Vector3? _lastRigthKneePosition;
    private Vector3? _lastLeftKneePosition;


    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start ()
	{
		Sensors           = new Dictionary<string, Sensor> ();
		_humans           = new Dictionary<int,     Human> ();
		_deadHumans       = new List<Human> ();
		_humansToKill     = new List<Human> ();
		CalibrationStatus = CalibrationProcess.FindCenter;

		_udpBroadcast  = new UdpBroadcast (TrackerProperties.Instance.BroadcastPort);
        _safeWriteFile = new SafeWriteFile();

        //_localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
	    _localTrackerUi = gameObject.GetComponent<TrackerUI>();
        
	    _loadConfig();
	    _loadSavedSensors();

	    IdList         = new List<string>();
	    IdIntList      = new List<int>();

	    CountHuman = 0;


        RightKneesInfo = new Dictionary<string, KneesInfo>();
        LeftKneesInfo  = new Dictionary<string, KneesInfo>();


    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void FixedUpdate ()
	{
	    //IdIntList      = new List<int>();
	    IdList = new List<string>();
	    var  idIntList = new List<int>();

	    if (Input.GetKeyDown (KeyCode.C)) ColorHumans = !ColorHumans;

		foreach (var s in Sensors.Values)
        {
			s.UpdateBodies ();
		}

		MergeHumans();

		var deadHumansToRemove = _deadHumans.Where(h => DateTime.Now > h.TimeOfDeath.AddMilliseconds(0)).ToList();

	    //foreach (var h in _deadHumans)
        //{
        //   if (DateTime.Now > h.timeOfDeath.AddMilliseconds (1000)) deadHumansToRemove.Add (h);
        //}

		foreach (var h in deadHumansToRemove)
        {
			Destroy (h.gameObject);
			_deadHumans.Remove (h);
		}

		foreach (var h in _humansToKill)
        {
			Destroy (h.gameObject);
		}

		_humansToKill.Clear ();

		// udp broadcast
		var strToSend = "" + _humans.Count;


	    CountHuman = _humans.Count;

        foreach (var h in _humans.Values)
        {
            IdList.Add(h.ID.ToString()); // "SpecialHuman " +
            idIntList.Add(h.ID);
             
            // udpate Human Skeleton
            h.UpdateSkeleton ();
            
            // get PDU
            try
            {
                strToSend += MessageSeparators.L1 + h.GetPdu();
                //GetKnees(h);
                // strToSend += SetCentro();
            }
			catch (Exception e)
            {
                Debug.Log(e.Message + "\n" + e.StackTrace + "\n");
            }
		}

		foreach (var h in _deadHumans)
        {
            try
            {
                strToSend += MessageSeparators.L1 + h.GetPdu(); 
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "\n" + e.StackTrace);
            }
		}

        // Debug.Log("Send = " + strToSend);
        _udpBroadcast.Send (strToSend);
        SaveRecordServer(strToSend);

		// set human material

		foreach (var h in _humans.Values)
        {
			if (h.SeenBySensor != null && colorHumans)
				CommonUtils.ChangeGameObjectMaterial (h.gameObject, Sensors [h.SeenBySensor].Material);
			else if (!ColorHumans)
				CommonUtils.ChangeGameObjectMaterial (h.gameObject, WhiteMaterial);
		}

		// show / hide human bodies

		if (ShowHumanBodies != -1 && !_humans.ContainsKey (ShowHumanBodies))
			ShowHumanBodies = -1;

		foreach (var h in _humans.Values)
        {
			var humanCollider = h.gameObject.GetComponent<CapsuleCollider> ();
			if (humanCollider != null)
				humanCollider.enabled = (ShowHumanBodies == -1);

			foreach (Transform child in h.gameObject.transform)
            {
				if (child.gameObject.GetComponent<Renderer> () != null)
					child.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == -1);
			}

			foreach (var b in h.bodies) {
				b.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == h.ID);
			}
		}
	    IdIntList = idIntList;

	}
    
    public Dictionary<int, Human> GetHumans()
    {
        return _humans;
    }

    public Human GetHuman(int id)
    {
        return _humans.ContainsKey(id) ? _humans[id] : null;
    }

    // ReSharper disable once UnusedMember.Local
    private static Vector3 GetLastPosition(Human human, Side thisSide, Vector3? lastPosition)
    {
        // ReSharper disable once MergeConditionalExpression
        var position = lastPosition != null ? lastPosition.Value : human.Skeleton.GetKnee(thisSide);
        return position;
    }
    
    // < Change >
    /// <summary>
    /// (Pt) Guarda as mensagens enviadas pelo servidor em ficheiros txt
    /// (En) Store messages sent by the server in txt files
    /// </summary>
    /// <param name="strToSend"> Mensagem para Guardar </param>
    private void SaveRecordServer(string strToSend)
    {
        const string noneMessage = "0";
        if (strToSend == noneMessage)
        {
            _safeWriteFile.StopRecording("Terminated Because No Messages");
            // _safeWriteFile.StopRecording();
        }
        else
        {
            if (_localTrackerUi == null || _localTrackerUi.UseRecord)
            {
                _safeWriteFile.IsRecording = true;
                _safeWriteFile.Recording(strToSend);
            }
            else if (!_localTrackerUi.UseRecord)
                _safeWriteFile.IsRecording = false;
        }

        foreach (var sensor in Sensors)
        {
            var start   = sensor.Value.SensorGameObject.transform.position;
            var forward = sensor.Value.SensorGameObject.transform.forward;

            Debug.DrawLine(start, start + forward, Color.black);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void SetHumansVisibility(bool invisible)
    {
        foreach (var h in _humans.Values)
        {
            h.gameObject.SetActive(!invisible);
        }
    }

    private void MergeHumans ()
	{
		var aloneBodies = new List<SensorBody> ();

		// refresh existing bodies
		foreach (Sensor s in Sensors.Values)
        {
			if (s.Active)
            {
				foreach (KeyValuePair<string, SensorBody> sensorBody in s.bodies)
                {
					bool alone = true;

					foreach (KeyValuePair<int, Human> h in _humans)
					{
					    SensorBody bodyToRemove = null;
						foreach (SensorBody humanBody in h.Value.bodies)
                        {
							if (sensorBody.Value.sensorID == humanBody.sensorID && sensorBody.Value.ID == humanBody.ID)
                            {
                                //************* NEW DM STUFF *******************
                                // check body distance from other human bodies
                                int nFarBodies = 0;
                                foreach (SensorBody b in h.Value.bodies)
                                {
                                    if (CalcHorizontalDistance(b.WorldPosition, humanBody.WorldPosition) >
                                        TrackerProperties.Instance.MergeDistance)
                                    {
                                        nFarBodies++;
                                    }
                                }
                                if (h.Value.bodies.Count > 1 && nFarBodies == (h.Value.bodies.Count - 1))
                                {
                                    bodyToRemove = humanBody;
                                    alone = false;
                                    break;
                                }
                                else
                                //************* END DM STUFF *******************
                                {

                                    humanBody.LocalPosition = sensorBody.Value.LocalPosition;
                                    humanBody.lastUpdated = sensorBody.Value.lastUpdated;
                                    humanBody.updated = sensorBody.Value.updated;

                                    alone = false;
                                    break;
                                }
                            }
						}
                        //************* NEW DM STUFF *******************
					    if (bodyToRemove != null)
					    {
					        h.Value.bodies.Remove(bodyToRemove);
					        break;
					    }
                        //************* END DM STUFF *******************
                        if (!alone)
							break;
					}

					if (alone)
						aloneBodies.Add (sensorBody.Value);
				}
			}
		}

		// refresh existing humans
		foreach (KeyValuePair<int, Human> h in _humans)
        {
			Vector3 position = new Vector3 ();
			int numberOfBodies = 0;
			List<SensorBody> deadBodies = new List<SensorBody> ();

			foreach (SensorBody b in h.Value.bodies)
            {
				if (b.updated && Sensors [b.sensorID].Active)
					position = (position * (float)numberOfBodies++ + b.WorldPosition) / (float) numberOfBodies;
				else
					deadBodies.Add (b);
			}

			foreach (SensorBody b in deadBodies) {
				h.Value.bodies.Remove (b);
			}

			if (h.Value.bodies.Count == 0) {
				h.Value.TimeOfDeath = DateTime.Now;
				_deadHumans.Add (h.Value);
			} else {
				h.Value.Position = position;
			}
		}
		foreach (Human h in _deadHumans) {
			_humans.Remove (h.ID);
		}

		// new bodies
		foreach (var b in aloneBodies)
        {
			var hasHuman = false;

			// try to fit in existing humans
			foreach (KeyValuePair<int, Human> h in _humans)
            {
                //************* NEW DM STUFF *******************
                // if the human has a body from the same sensor it cannot have another
                var canHaveThisBody = true;
                foreach (SensorBody humanBody in h.Value.bodies)
                {
                    if (humanBody.sensorID == b.sensorID)
                    {
                        canHaveThisBody = false;
                        break;
                    }
                }
                if (!canHaveThisBody)
                    continue;
                //************* END DM STUFF *******************

                if (CalcHorizontalDistance (b.WorldPosition, h.Value.Position) < TrackerProperties.Instance.MergeDistance)
                {
					h.Value.Position = (h.Value.Position * (float)h.Value.bodies.Count + b.WorldPosition) / (float)(h.Value.bodies.Count + 1);
					h.Value.bodies.Add (b);
					hasHuman = true;
					break;
				}
			}

			if (!hasHuman) {
				// try to fit in dead humans
				foreach (Human h in _deadHumans) {
					if (CalcHorizontalDistance (b.WorldPosition, h.Position) < TrackerProperties.Instance.MergeDistance) {
						h.Position = (h.Position * (float)h.bodies.Count + b.WorldPosition) / (float)(h.bodies.Count + 1);
						h.bodies.Add (b);
						hasHuman = true;
						break;
					}
				}

				if (!hasHuman) {
					// create new human
					Human h = new Human ((GameObject)Instantiate (Resources.Load ("Prefabs/Human")), this);

					h.bodies.Add (b);
					h.Position = b.WorldPosition;

					_humans [h.ID] = h;
				}
			}
		}

		// bring back to life selected dead humans
		List<Human> undeadHumans = new List<Human> ();
		foreach (Human h in _deadHumans) {
			if (h.bodies.Count > 0) {
				_humans [h.ID] = h;
				undeadHumans.Add (h);
			}
		}
		foreach (Human h in undeadHumans) {
			_deadHumans.Remove (h);
		}

		// merge humans
		List<Human> mergedHumans = new List<Human> ();
		foreach (KeyValuePair<int, Human> h1 in _humans)
        {
			foreach (KeyValuePair<int, Human> h2 in _humans)
            {
				if (h1.Value.ID != h2.Value.ID && !mergedHumans.Contains (h2.Value))
                {
					if (CalcHorizontalDistance (h1.Value.Position, h2.Value.Position) < TrackerProperties.Instance.MergeDistance)
                    {
                        //************* NEW DM STUFF *******************
                        // if a human has body from a sensor, the other cannot have a body from the same sensor
                        bool commonSensor = false;
                        foreach (SensorBody h1body in h1.Value.bodies)
                        {
                            foreach (SensorBody h2body in h2.Value.bodies)
                            {
                                if (h1body.sensorID == h2body.sensorID)
                                {
                                    commonSensor = true;
                                    break;
                                }
                            }
                            if(commonSensor)
                                break;
                        }
                        if (commonSensor)
                        {
                            continue;
                        }
                        //************* END DM STUFF *******************

                        var position = (h1.Value.Position * (float) h1.Value.bodies.Count + h2.Value.Position * (float)h2.Value.bodies.Count) / (float)(h1.Value.bodies.Count + h2.Value.bodies.Count);

						if (h1.Value.ID < h2.Value.ID) {
							h1.Value.Position = position;
							foreach (var b in h2.Value.bodies)
                            {
								h1.Value.bodies.Add (b);
							}
							mergedHumans.Add (h2.Value);
						}
                        else
                        {
							h2.Value.Position = position;
							foreach (var b in h1.Value.bodies)
                            {
								h2.Value.bodies.Add (b);
							}
							mergedHumans.Add (h1.Value);
						}
						break;
					}
				}
			}
		}
		foreach (var h in mergedHumans)
        {
			_humansToKill.Add (h);
			_humans.Remove (h.ID);
		}
	}

    private static float CalcHorizontalDistance (Vector3 a, Vector3 b)
	{
		var c = new Vector3 (a.x, 0, a.z);
		var d = new Vector3 (b.x, 0, b.z);
		return Vector3.Distance (c, d);
	}

    internal void AddUnicast (string address, string port)
	{
		_udpBroadcast.AddUnicast (address, int.Parse (port));
	}

	internal void RemoveUnicast (string key)
	{
		_udpBroadcast.RemoveUnicast (key);
	}
    //FOR TCP
    internal void SetNewCloud(string KinectID, byte[] data, int size, uint id)
    {
        // tirar o id da mensagem que é um int
        
        if (Sensors.ContainsKey(KinectID))
        {
            Sensors[KinectID].lastCloud.setPoints(data, 0, id, size);
            Sensors[KinectID].lastCloud.setToView();
        }
    }

    internal void SetNewCloud (CloudMessage cloud)
	{
        //if (!Sensors.ContainsKey (cloud.KinectId)) {
        //	Vector3 position = new Vector3 (Mathf.Ceil (Sensors.Count / 2.0f) * (Sensors.Count % 2 == 0 ? -1.0f : 1.0f), 1, 0);
        //          Sensors [cloud.KinectId] = new Sensor (cloud.KinectId, (GameObject)Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, Quaternion.identity));
        //}
        //      Sensors[cloud.KinectId].lastCloudupdateCloud (cloud);

        int step = cloud.HeaderSize+3; // TMA: Size in bytes of heading: "CloudMessage" + L0 + 2 * L1. Check the UDPListener.cs from the Client.
        string[] pdu = cloud.Message.Split(MessageSeparators.L1);
       

        string kinectId = pdu[0]; 
        uint  id = uint.Parse(pdu[1]);
        step += pdu[0].Length + pdu[1].Length;

        if (Sensors.ContainsKey(kinectId))
        {
            if (pdu[2] == "") {
                Sensors[kinectId].lastCloud.setToView();
            }
            else
                Sensors[kinectId].lastCloud.setPoints(cloud.ReceivedBytes,step,id,cloud.ReceivedBytes.Length);
        }
	}

    internal void SetCloudToView(string KinectID)
    {
        Sensors[KinectID].lastCloud.setToView();
    }

    internal void SetNewFrame (BodiesMessage bodies)
	{
        //Debug.Log("bodies = " + bodies.Message + ", KinectId = " + bodies.KinectId);
        if (!Sensors.ContainsKey (bodies.KinectId))
        {
			var position = new Vector3 (Mathf.Ceil (Sensors.Count / 2.0f) * (Sensors.Count % 2 == 0 ? -1.0f : 1.0f), 1, 0);
            Sensors [bodies.KinectId] = new Sensor (bodies.KinectId, (GameObject) Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, Quaternion.identity));
		}
		Sensors [bodies.KinectId].lastBodiesMessage = bodies;
	}

	internal bool CalibrationStep1 ()
	{
		var canNotCalibrate = false;
		foreach (var sensor in Sensors.Values)
        {
			if (sensor.Active)
            {
				if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1)
                {
					sensor.CalibrationStep1 ();
				}
                else
					canNotCalibrate = true;
			}
		}

		if (canNotCalibrate)
        {
			var n = gameObject.GetComponent<DoNotify> ();
			n.notifySend (NotificationLevel.IMPORTANT, "Calibration error", "Incorrect user placement!", 5000);
		}

		return !canNotCalibrate;
	}

	internal void CalibrationStep2 ()
	{

		var avgCenter = new Vector3 ();
		var sensorCount = 0;

		foreach (var sensor in Sensors.Values)
        {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.CalibrationStep2 ();

				avgCenter += sensor.PointSensorToScene (sensor.CalibAuxPoint);
				sensorCount += 1;
			}
		}

		avgCenter /= sensorCount;

		foreach (var sensor in Sensors.Values)
        {
			if (sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active)
            {
				sensor.move (avgCenter - sensor.PointSensorToScene (sensor.CalibAuxPoint));   
			}
		}

		_saveConfig();

		var n = gameObject.GetComponent<DoNotify> ();
		n.notifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
	}

	internal void CalibrationStep3 ()
	{
		foreach (var sensor in Sensors.Values) {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active)
            {
				sensor.CalibrationStep3 ();
			}
		}
	}

	internal void CalibrationStep4 ()
	{
		foreach (var sensor in Sensors.Values)
        {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.CalibrationStep4();
			}
		}

		_saveConfig ();

		var n = gameObject.GetComponent<DoNotify> ();
		n.notifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
	}

	internal Vector3 GetJointPosition (int id, JointType joint)
	{
		var h = _humans [id];

		var bestBody = h.bodies [0];
		var confidence = bestBody.Confidence;
		var lastSensorConfidence = 0;
		SensorBody lastSensorBody = null;

		foreach (var b in h.bodies)
        {
			var bConfidence = b.Confidence;
			if (bConfidence > confidence)
            {
				confidence = bConfidence;
				bestBody = b;
			}

			if (b.sensorID == h.SeenBySensor) {
				lastSensorConfidence = bConfidence;
				lastSensorBody = b;
			}
		}

		if (lastSensorBody == null || (bestBody.sensorID != h.SeenBySensor && confidence > (lastSensorConfidence + 1)))
			h.SeenBySensor = bestBody.sensorID;
		else
			bestBody = lastSensorBody;

        return Sensors [bestBody.sensorID].PointSensorToScene (CommonUtils.PointKinectToUnity (bestBody.skeleton.JointsPositions [joint]));
	}

    internal Vector3 GetJointPosition(int id, JointType joint, Vector3 garbage)
    {
        var h = _humans[id]; 
        var bestBody = h.bodies[0];
        var confidence = bestBody.Confidence;
        var lastSensorConfidence = 0;
        SensorBody lastSensorBody = null;

            foreach (var b in h.bodies)
            {
                var bConfidence = b.Confidence;
                   
                if (bConfidence > confidence)
                {
                    confidence = bConfidence; 
                    bestBody = b; 
                }
                    
                if (b.sensorID == h.SeenBySensor)
                {
                    lastSensorConfidence = bConfidence;
                    lastSensorBody = b;
                }
            }
            
        if (lastSensorBody == null || (bestBody.sensorID != h.SeenBySensor && confidence > (lastSensorConfidence + 1)))
            h.SeenBySensor = bestBody.sensorID;
        else
            bestBody = lastSensorBody;

        return Sensors[bestBody.sensorID].PointSensorToScene(CommonUtils.PointKinectToUnity(bestBody.skeleton.JointsPositions[joint]));
        
    }

    internal string GetHandState(int id, Side side)
    {
        var h = _humans[id];
        var bestBody = h.bodies[0];
        var confidence = bestBody.Confidence;
        var lastSensorConfidence = 0;
        SensorBody lastSensorBody = null;

        foreach (var b in h.bodies)
        {
            var bConfidence = b.Confidence;

            if (bConfidence > confidence)
            {
                confidence = bConfidence;
                bestBody = b;
            }

            if (b.sensorID == h.SeenBySensor)
            {
                lastSensorConfidence = bConfidence;
                lastSensorBody = b;
            }
        }

        if (lastSensorBody == null || (bestBody.sensorID != h.SeenBySensor && confidence > (lastSensorConfidence + 1)))
            h.SeenBySensor = bestBody.sensorID;
        else
            bestBody = lastSensorBody;

        var st = GetHandStateFromBody(side, bestBody);

        // body.HandLeftState = HandState.Closed;
        // body.HandLeftState = HandState.Open;
        // body.HandLeftState = HandState.Unknown;
        if (st == "Closed" || st == "Open")
        {
            return st;
        }
        foreach (var b in h.bodies)
        {
            st = GetHandStateFromBody(side, b);
            if (st == "Closed") return st;
        }
        return "Unknown";

        //return _sensors[bestBody.sensorID].PointSensorToScene(CommonUtils.pointKinectToUnity(body.skeleton.JointsPositions[joint]));
    }

    private static string GetHandStateFromBody(Side side, SensorBody body)
    {
        switch (side)
        {
            case Side.Right:
                return body.skeleton.BodyProperties[BodyPropertiesTypes.HandRightState];
            case Side.Left:
                return body.skeleton.BodyProperties[BodyPropertiesTypes.HandLeftState];
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }
    }

    internal bool HumanHasBodies (int id)
	{
		return _humans.ContainsKey (id) && _humans [id].bodies.Count > 0;
	}

    private void _saveConfig ()
	{
		var filePath = Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename;
		ConfigProperties.clear (filePath);

		ConfigProperties.writeComment (filePath, "Config File created in " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss"));

		// save properties
		ConfigProperties.save (filePath, "udp.listenport",              "" + TrackerProperties.Instance.ListenPort);
		ConfigProperties.save (filePath, "udp.broadcastport",           "" + TrackerProperties.Instance.BroadcastPort);
		ConfigProperties.save (filePath, "udp.sendinterval",            "" + TrackerProperties.Instance.SendInterval);
		ConfigProperties.save (filePath, "tracker.mergedistance",       "" + TrackerProperties.Instance.MergeDistance);
		ConfigProperties.save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.ConfidenceTreshold);
//		ConfigProperties.save (filePath, "tracker.filtergain", "" + AdaptiveDoubleExponentialFilterFloat.Gain);

		// save sensors
		foreach (Sensor s in Sensors.Values)
        {
			if (s.Active)
            {
				Vector3 p = s.SensorGameObject.transform.position;
				Quaternion r = s.SensorGameObject.transform.rotation;
				ConfigProperties.save (filePath, "kinect." + s.SensorID, "" + s.SensorID + ";" + p.x + ";" + p.y + ";" + p.z + ";" + r.x + ";" + r.y + ";" + r.z + ";" + r.w);
			}
		}
	}

    private void _loadConfig ()
	{
		var filePath = Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename;

		var port = ConfigProperties.Load (filePath, "udp.listenport");
		if (port != "") {
			TrackerProperties.Instance.ListenPort = int.Parse (port);
		}
		ResetListening ();

		port = ConfigProperties.Load (filePath, "udp.broadcastport");
		if (port != "") {
			TrackerProperties.Instance.BroadcastPort = int.Parse (port);
		}
		ResetBroadcast ();

		string aux = ConfigProperties.Load (filePath, "tracker.mergedistance");
		if (aux != "") {
			TrackerProperties.Instance.MergeDistance = float.Parse (aux);
		}

		aux = ConfigProperties.Load (filePath, "tracker.confidencethreshold");
		if (aux != "") {
			TrackerProperties.Instance.ConfidenceTreshold = int.Parse (aux);
		}

		aux = ConfigProperties.Load (filePath, "udp.sendinterval");
		if (aux != "") {
			TrackerProperties.Instance.SendInterval = int.Parse (aux);
		}

		/*
            aux = ConfigProperties.load (filePath, "tracker.filtergain");
		    if (aux != "") {
			    KalmanFilterFloat.Gain = float.Parse (aux);
		    }
        */
	}

    private void _loadSavedSensors ()
	{
		foreach (var line in ConfigProperties.loadKinects(Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename)) {
			var values = line.Split (';');

			var id = values [0];

			var position = new Vector3 (
				                   float.Parse (values [1].Replace (',', '.')),
				                   float.Parse (values [2].Replace (',', '.')),
				                   float.Parse (values [3].Replace (',', '.')));

			Quaternion rotation = new Quaternion (
				                      float.Parse (values [4].Replace (',', '.')),
				                      float.Parse (values [5].Replace (',', '.')),
				                      float.Parse (values [6].Replace (',', '.')),
				                      float.Parse (values [7].Replace (',', '.')));

			Sensors [id] = new Sensor (id, (GameObject)Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, rotation));
		}
	}

    public void ResetBroadcast ()
	{
		_udpBroadcast.Reset (TrackerProperties.Instance.BroadcastPort);
	}

    public void ResetListening ()
	{
		gameObject.GetComponent<UdpListener> ().UdpRestart ();
	}

    public void Save ()
	{
		_saveConfig ();
	}

    public void HideAllClouds ()
	{
		foreach (var s in Sensors.Values)
        {
			s.lastCloud.hideFromView ();
		}

		UdpClient udp = new UdpClient ();
		string message = CloudMessage.CreateRequestMessage (2,Network.player.ipAddress, TrackerProperties.Instance.ListenPort); 
		byte[] data = Encoding.UTF8.GetBytes(message);
		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
		udp.Send(data, data.Length, remoteEndPoint);
	}
    
    // ReSharper disable once MemberCanBeMadeStatic.Global
	public void BroadCastCloudRequests (bool continuous)
	{
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.CreateRequestMessage (continuous ? 1 : 0, Network.player.ipAddress, TrackerProperties.Instance.ListenPort); 
		byte[] data = Encoding.UTF8.GetBytes (message);
		IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
		udp.Send (data, data.Length, remoteEndPoint);
	}
    
    // ReSharper disable once UnusedMember.Local
    private string GetKnees(Human h)
    {
        // CommonUtils.convertVectorToStringRPC
        // if (!_humans.ContainsKey(h1)) return null;

        if (h == null || !_humans.ContainsValue(h)) return null;

        // Human h = _humans[h1];
        // SensorBody bestBody = h.bodies[0];
        var mensagem = "";
        mensagem += MessageSeparators.L4;

        //SaveTheKnees(h);

        var meanKneeRight = GetMeanList(RightKneesInfo);
        var meanKneeLeft  = GetMeanList(LeftKneesInfo);

        var stringMeanKneeRight = meanKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeRight.Value);
        var stringMeanKneeLeft  = meanKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeLeft.Value);

        mensagem +=
            "MeanKneeRight" + MessageSeparators.SET + stringMeanKneeRight +
            MessageSeparators.L2 +
            "MeanKneeLeft" + MessageSeparators.SET + stringMeanKneeLeft;

        var meanTrackKneeRight = GetMeanTrackList(RightKneesInfo);
        var meanTrackKneeLeft = GetMeanTrackList(LeftKneesInfo);

        var stringMeanTrackKneeRight = meanTrackKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeRight.Value);
        var stringMeanTrackKneeLeft = meanTrackKneeLeft == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeLeft.Value);

        mensagem +=
            MessageSeparators.L2 +
            "MeanTrackKneeRight" + MessageSeparators.SET + stringMeanTrackKneeRight +
            MessageSeparators.L2 +
            "MeanTrackKneeLeft"  + MessageSeparators.SET + stringMeanTrackKneeLeft;
        return mensagem;
    }

    private static Vector3? GetMeanList(Dictionary<string, KneesInfo> meanList)
    {
        if (meanList.Count == 0)
        {
            Debug.Log("meanList.Count == 0");
            return null;
        }

        var meanResult = meanList.Aggregate(Vector3.zero, (current, pair) => current + pair.Value.Pos);
        meanResult /= meanList.Count;
        return meanResult;
    }

    private static Vector3? GetMeanTrackList(Dictionary<string, KneesInfo> meanList)
    {
        if (meanList.Count == 0)
        {
            Debug.Log("meanList.Count == 0");
            return null;
        }

        var meanResult = Vector3.zero;
        var trackCount = 0;
        foreach (var info in meanList)
        {
            if (!info.Value.Track) continue;
            trackCount++;
            meanResult = meanResult + info.Value.Pos;
        }

        if (trackCount == 0) return GetMeanList(meanList);

        meanResult /= trackCount;
        return meanResult;
    }


    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    void OnGUI ()
    {
        //int n = 1;

        if (ShowHumanBodies == -1)
        {
            foreach (Human h in _humans.Values)
            {
                //GUI.Label(new Rect(10, Screen.height - (n++ * 50), 1000, 50), "Human " + h.ID + " as seen by " + h.seenBySensor);
                var p = Camera.main.WorldToScreenPoint (h.Skeleton.GetHead () + new Vector3 (0, 0.2f, 0));
                if (p.z > 0)
                {
                    GUI.Label (new Rect (p.x, Screen.height - p.y - 25, 100, 25), "" + h.ID);
                }
            }
        }

        foreach (Sensor s in Sensors.Values)
        {
            if (s.Active)
            {
                var p = Camera.main.WorldToScreenPoint (s.SensorGameObject.transform.position + new Vector3 (0, 0.05f, 0));
                if (p.z > 0)
                {
                    GUI.Label (new Rect (p.x, Screen.height - p.y - 25, 100, 25), "" + s.SensorID);
                }
            }
        }
    }

    public void ProcessAvatarMessage(AvatarMessage av)
    {
        UdpClient udp = new UdpClient();
        //Calibration
       // string message = av.createCalibrationMessage(_sensors);
        string message = av.createCalibrationMessage(Sensors);
        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(av.ReplyIpAddress, av.Port);
        Debug.Log("Sent reply with calibration data " + message);
        udp.Send(data, data.Length, remoteEndPoint);
        //broadcast
        string message2 = CloudMessage.CreateRequestMessage(av.Mode, av.ReplyIpAddress.ToString(), av.Port);
        byte[] data2 = Encoding.UTF8.GetBytes(message2);
        IPEndPoint remoteEndPoint2 = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        udp.Send(data2, data2.Length, remoteEndPoint2);
        Debug.Log("Forwarded request to clients " + message2);
    }
    
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/*
 *
 * 
    //private GameObject _centroGameObject;

    //public Vector3 Centro;

    //private bool _setUpCentro;
 * 
 * 
 * 
	    //if (_localTrackerUi.SetUpCenter) {}

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    if (_localOptitrackManager != null)
        //    {
        //        _setUpCentro = true;

        //        _centroGameObject.transform.position = Centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
        //    }
        //}

        //_centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;

 * 
 * 
 * 
 * 
        //_setUpCentro = false;
        //_centroGameObject = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, "Centro", Vector3.zero, transform, false);
        // _centroGameObject.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f); 
 * 
// if (_localOptitrackManager.IsOn) _safeWriteFile.Recording(strToSend, _localOptitrackManager.GetPositionVector(), _localOptitrackManager.GetRotationQuaternion());
// else
//_safeWriteFile.Recording(strToSend, Centro, _setUpCentro);




     private string SetCentro()
    {

        var mensagem = "";
        mensagem += MessageSeparators.L4;
        
        mensagem +=
            "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(Centro) + MessageSeparators.L2 +
            "Use"       + MessageSeparators.SET + _setUpCentro.ToString();
     
        return mensagem;
    }
    
    /////////////////////////////////////////////////////////////////////////////////
 //, _localOptitrackManager.PositionVector
        //////////////////////////////////////////////////////////////////////////////

    // public bool SendKnees;
    // public List<KneesInfo> RightKneesInfo;
    // public List<KneesInfo> LeftKneesInfo;

    // public Dictionary<string, KneesInfo> RightKneesInfo;
    // public Dictionary<string, KneesInfo> LeftKneesInfo;

    // private Dictionary<string, KneesInfo> _allKneesInfo;
    // private Vector3? _lastRigthKneePosition;
    // private Vector3? _lastLeftKneePosition;
        // RightKneesInfo = new Dictionary<string, KneesInfo>();
        // LeftKneesInfo  = new Dictionary<string, KneesInfo>();
        // _allKneesInfo  = new Dictionary<string, KneesInfo>();


       // RightKneesInfo = new Dictionary<string, KneesInfo>();
	    // LeftKneesInfo  = new Dictionary<string, KneesInfo>();
	    // _allKneesInfo  = new Dictionary<string, KneesInfo>();
	    // _lastRigthKneePosition = null;
	    // _lastLeftKneePosition  = null;
	    // SendKnees = false;

    //private Vector3 GetCentrePosition()
    //{
    //    if (_localTrackerUi.UseOptiTrack)
    //    {
    //        return _localOptitrackManager.GetUnityPositionVector();
    //    }
    //    else
    //    {


    //        return
    //    }


    //    return Vector3.zero;
    //}
 * 
 * 
 * 
 * 
 * 
//public struct KneesInfo
//{
//    public int IdHuman;
//    public string IdBody;
//    public Vector3 Pos;
//    public bool Track;
//}
 * 
 * 

private string GetKnees(Human h) {
        
        // CommonUtils.convertVectorToStringRPC
        // if (!_humans.ContainsKey(h1)) return null;

        if(h == null || !_humans.ContainsValue(h)) return null;

        // Human h = _humans[h1];
        // SensorBody bestBody = h.bodies[0];
        var mensagem = "";
        mensagem += MessageSeparators.L4;

        //SaveTheKnees(h);

        var meanKneeRight = GetMeanList(RightKneesInfo);
        var meanKneeLeft  = GetMeanList(LeftKneesInfo);

        var stringMeanKneeRight = meanKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeRight.Value);
        var stringMeanKneeLeft  = meanKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeLeft.Value);

        mensagem +=
            "MeanKneeRight" + MessageSeparators.SET + stringMeanKneeRight +
            MessageSeparators.L2 +
            "MeanKneeLeft"  + MessageSeparators.SET + stringMeanKneeLeft;

        var meanTrackKneeRight = GetMeanTrackList(RightKneesInfo);
        var meanTrackKneeLeft  = GetMeanTrackList(LeftKneesInfo);

        var stringMeanTrackKneeRight = meanTrackKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeRight.Value);
        var stringMeanTrackKneeLeft  = meanTrackKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeLeft.Value);
        
        mensagem += 
            MessageSeparators.L2 +
            "MeanTrackKneeRight" + MessageSeparators.SET + stringMeanTrackKneeRight +
            MessageSeparators.L2 +
            "MeanTrackKneeLeft"  + MessageSeparators.SET + stringMeanTrackKneeLeft;
        
        
        var closeKneeRight = CloseKnee(RightKneesInfo, h, Side.Right, false, _lastRigthKneePosition);
        var closeKneeLeft  = CloseKnee(LeftKneesInfo,  h, Side.Left,  false, _lastLeftKneePosition);

        var stringCloseKneeRight = closeKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(closeKneeRight.Value);
        var stringCloseKneeLeft  = closeKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(closeKneeLeft.Value);

        mensagem +=
            MessageSeparators.L2 +
            "CloseKneeRight" + MessageSeparators.SET + stringCloseKneeRight +
            MessageSeparators.L2 +
            "CloseKneeLeft"  + MessageSeparators.SET + stringCloseKneeLeft;


        var closeTrackKneeRight = CloseKnee(RightKneesInfo, h, Side.Right, true, _lastRigthKneePosition);
        var closeTrackKneeLeft  = CloseKnee(LeftKneesInfo,  h, Side.Left,  true, _lastLeftKneePosition);


        var stringCloseTrackKneeRight = closeTrackKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(closeTrackKneeRight.Value);
        var stringCloseTrackKneeLeft  = closeTrackKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(closeTrackKneeLeft.Value);

        mensagem +=
            MessageSeparators.L2 +
            "CloseTrackKneeRight" + MessageSeparators.SET + stringCloseTrackKneeRight +
            MessageSeparators.L2 +
            "CloseTrackKneeLeft"  + MessageSeparators.SET + stringCloseTrackKneeLeft;
        
        //  GetLastPosition(Tracker localTracker, Side thisSide, string idHuman, );
        return mensagem;
    }

    private void SaveTheMirrorKnees(Human h)
    {
        var mainRightKnee = h.Skeleton.GetKnee(Side.Right);
        var mainLeftKnee  = h.Skeleton.GetKnee(Side.Left);

        foreach (var b in h.bodies)
        {
            var bodySensorId = b.sensorID;

            var knee1 = _sensors[bodySensorId].PointSensorToScene(
                CommonUtils.PointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]));

            var knee2 = _sensors[bodySensorId].PointSensorToScene(
                CommonUtils.PointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]));


            var diff1 = (knee1 - mainRightKnee).sqrMagnitude;
            var diff2 = (knee2 - mainRightKnee).sqrMagnitude;


            var diff3 = (knee1 - mainLeftKnee).sqrMagnitude;
            var diff4 = (knee2 - mainLeftKnee).sqrMagnitude;

            var kneeRight = new Vector3();
            var kneeLeft  = new Vector3();
            
            if (diff1 < diff2)
            {
                kneeRight = knee1;
                kneeLeft  = knee2;
            }
            else if (diff1 > diff2)
            {
                kneeRight = knee2;
                kneeLeft  = knee1; 
            }
            else if(Math.Abs(diff1 - diff2) < 0)
            {
                if (diff3 < diff4)
                {
                    kneeRight = knee2;
                    kneeLeft  = knee1;
                }
                else if (diff3 >= diff4)
                {
                    kneeRight = knee1;
                    kneeLeft  = knee2;
                }
            }

            //var kneeRight = new AdaptiveDoubleExponentialFilterVector3
            //{
            //    Value = _sensors[bodySensorId].pointSensorToScene(
            //        CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]))
            //};

            //var kneeLeft = new AdaptiveDoubleExponentialFilterVector3
            //{
            //    Value = _sensors[bodySensorId].pointSensorToScene(
            //        CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]))
            //};
            
            var trackingStateKneeRight = b.skeleton.TrackingStateKneeRight;
            var trackingStateKneeLeft  = b.skeleton.TrackingStateKneeLeft;

            // Debug.Log("h id = " + h.ID + ", b.sensorID = " + bodySensorId + ", kneeRight : x = " + kneeRight.x + ", y = " + kneeRight.y + ", z = " + kneeRight.z);

            var key = h.ID + "_" + bodySensorId;
            if (RightKneesInfo.ContainsKey(key) && LeftKneesInfo.ContainsKey(key)) continue;
            
            RightKneesInfo.Add(key, new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeRight,
                Track = (trackingStateKneeRight == TrackingState.Tracked)
            });

            LeftKneesInfo.Add(key, new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeLeft,
                Track = (trackingStateKneeLeft == TrackingState.Tracked)
            });


            _allKneesInfo.Add(key + "_1", new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeRight,
                Track = (trackingStateKneeRight == TrackingState.Tracked)
            });


            _allKneesInfo.Add(key + "_2", new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeLeft,
                Track = (trackingStateKneeLeft == TrackingState.Tracked)
            });

            //mensagem +=
            //    "TrackingKneeRight" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(kneeRight.Value) + MessageSeparators.L5 + trackingStateKneeRight +
            //    MessageSeparators.L2 +
            //    "TrackingKneeLeft"  + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(kneeLeft.Value)  + MessageSeparators.L5 + trackingStateKneeLeft +
            //    MessageSeparators.L2;

            // if (index + 1 < h.bodies.Count) mensagem += MessageSeparators.L2;

            //kneeRightList.Add(kneeRight.Value);
            //kneeLeftList.Add(kneeLeft.Value);
        }
    }
    
    private static Vector3? GetMeanList(Dictionary<string, KneesInfo> meanList)
    {
        if (meanList.Count == 0)
        {
            Debug.Log("meanList.Count == 0");
            return null;
        }

        var meanResult = meanList.Aggregate(Vector3.zero, (current, pair) => current + pair.Value.Pos);
        meanResult /= meanList.Count;
        return meanResult;
    }

    private static Vector3? GetMeanTrackList(Dictionary<string, KneesInfo> meanList)
    {
        if (meanList.Count == 0)
        {
            Debug.Log("meanList.Count == 0");
            return null;
        }

        var meanResult = Vector3.zero;
        var trackCount = 0;
        foreach (var info in meanList)
        {
            if (!info.Value.Track) continue;
            trackCount++;
            meanResult = meanResult + info.Value.Pos;
        }

        if (trackCount == 0) return GetMeanList(meanList);

        meanResult /= trackCount;
        return meanResult;
    }

    private static Vector3? CloseKnee(Dictionary<string, KneesInfo> kneesList, Human human, Side thisSide, bool track, Vector3? lastPosition)
    {
        var position = GetLastPosition(human, thisSide, lastPosition);

        lastPosition = GetCloserKnee(kneesList, track, position);
        return lastPosition;
    }

    private static Vector3? GetCloserKnee(Dictionary<string, KneesInfo> kneesList, bool track, Vector3 position)
    {
        return track ? GetCloseKneeTrack(kneesList, position) : GetCloseKnee(kneesList, position);
    }

    private static Vector3? GetCloseKnee(Dictionary<string, KneesInfo> kneesList, Vector3 position)
    {

        if (kneesList.Count == 0) return null;

        var res = new Vector3();
        var diff = float.MaxValue;

        foreach (var info in kneesList)
        {
            var d = (info.Value.Pos - position).magnitude;
            if (!(d < diff)) continue;
            diff = d;
            res = info.Value.Pos;
        }

        if (Math.Abs(diff - float.MaxValue) < 0) return null;
        return res;
    }

    private static Vector3? GetCloseKneeTrack(Dictionary<string, KneesInfo> kneesList, Vector3 position)
    {
        if (kneesList.Count == 0) return null;

        var res = new Vector3();
        var diff = float.MaxValue;
        var hasTrack = false;

        foreach (var info in kneesList)
        {
            if (!info.Value.Track) continue;
            hasTrack = true;
            var d = (info.Value.Pos - position).magnitude;
            if (!(d < diff)) continue;
            diff = d;
            res = info.Value.Pos;
        }

        if (!hasTrack) return GetCloseKnee(kneesList, position);

        if (Math.Abs(diff - float.MaxValue) < 0) return null;


        return res;
    }
 *  
     //private void SaveTheKnees(Human h)
    //{
    //    //var kneeRightList = new List<Vector3>();
    //    //var kneeLeftList  = new List<Vector3>();
        
    //    //Debug.Log("1  kneeRightList.Count = " + kneeRightList.Count);
    //    //Debug.Log("1  kneeLeftList.Count = "  + kneeLeftList.Count);
        
    //    foreach (var b in h.bodies)
    //    {
    //        var bodySensorId = b.sensorID;

    //        //var kneeRight = new AdaptiveDoubleExponentialFilterVector3
    //        //{
    //        //    Value = _sensors[bodySensorId].pointSensorToScene(
    //        //        CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]))
    //        //};

    //        //var kneeLeft = new AdaptiveDoubleExponentialFilterVector3
    //        //{
    //        //    Value = _sensors[bodySensorId].pointSensorToScene(
    //        //        CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]))
    //        //};

    //        var kneeRight = _sensors[bodySensorId].PointSensorToScene(
    //            CommonUtils.PointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]));
            
    //        var kneeLeft = _sensors[bodySensorId].PointSensorToScene(
    //            CommonUtils.PointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]));
            
    //        var trackingStateKneeRight = b.skeleton.TrackingStateKneeRight;
    //        var trackingStateKneeLeft  = b.skeleton.TrackingStateKneeLeft;

    //        // Debug.Log("h id = " + h.ID + ", b.sensorID = " + bodySensorId + ", kneeRight : x = " + kneeRight.x + ", y = " + kneeRight.y + ", z = " + kneeRight.z);

    //        var key = h.ID + "_" + bodySensorId;
    //        if (RightKneesInfo.ContainsKey(key) && LeftKneesInfo.ContainsKey(key)) continue;

    //        RightKneesInfo.Add(key, new KneesInfo
    //        {
    //            IdHuman = h.ID,
    //            IdBody = bodySensorId,
    //            Pos = kneeRight,
    //            Track = (trackingStateKneeRight == TrackingState.Tracked)
    //        });

    //        LeftKneesInfo.Add(key, new KneesInfo
    //        {
    //            IdHuman = h.ID,
    //            IdBody = bodySensorId,
    //            Pos = kneeLeft,
    //            Track = (trackingStateKneeLeft == TrackingState.Tracked)
    //        });


    //        _allKneesInfo.Add(key + "_1", new KneesInfo
    //        {
    //            IdHuman = h.ID,
    //            IdBody = bodySensorId,
    //            Pos = kneeRight,
    //            Track = (trackingStateKneeRight == TrackingState.Tracked)
    //        });
            
    //        _allKneesInfo.Add(key + "_2", new KneesInfo
    //        {
    //            IdHuman = h.ID,
    //            IdBody = bodySensorId,
    //            Pos = kneeLeft,
    //            Track = (trackingStateKneeLeft == TrackingState.Tracked)
    //        });
            

    //        //mensagem +=
    //        //    "TrackingKneeRight" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(kneeRight.Value) + MessageSeparators.L5 + trackingStateKneeRight +
    //        //    MessageSeparators.L2 +
    //        //    "TrackingKneeLeft"  + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(kneeLeft.Value)  + MessageSeparators.L5 + trackingStateKneeLeft +
    //        //    MessageSeparators.L2;

    //        // if (index + 1 < h.bodies.Count) mensagem += MessageSeparators.L2;

    //        //kneeRightList.Add(kneeRight.Value);
    //        //kneeLeftList.Add(kneeLeft.Value);
    //    }
    //}  
     
    
     private string GetKnees(Human h) {
        
        // CommonUtils.convertVectorToStringRPC
        // if (!_humans.ContainsKey(h1)) return null;

        if(h == null || !_humans.ContainsValue(h)) return null;

        // Human h = _humans[h1];
        // SensorBody bestBody = h.bodies[0];
        var mensagem = "";
        mensagem += MessageSeparators.L4;

        //SaveTheKnees(h);

        var meanKneeRight = GetMeanList(RightKneesInfo);
        var meanKneeLeft  = GetMeanList(LeftKneesInfo);

        var stringMeanKneeRight = meanKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeRight.Value);
        var stringMeanKneeLeft  = meanKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(meanKneeLeft.Value);

        mensagem +=
            "MeanKneeRight" + MessageSeparators.SET + stringMeanKneeRight +
            MessageSeparators.L2 +
            "MeanKneeLeft"  + MessageSeparators.SET + stringMeanKneeLeft;

        var meanTrackKneeRight = GetMeanTrackList(RightKneesInfo);
        var meanTrackKneeLeft  = GetMeanTrackList(LeftKneesInfo);

        var stringMeanTrackKneeRight = meanTrackKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeRight.Value);
        var stringMeanTrackKneeLeft  = meanTrackKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(meanTrackKneeLeft.Value);
        
        mensagem += 
            MessageSeparators.L2 +
            "MeanTrackKneeRight" + MessageSeparators.SET + stringMeanTrackKneeRight +
            MessageSeparators.L2 +
            "MeanTrackKneeLeft"  + MessageSeparators.SET + stringMeanTrackKneeLeft;
        
        
        var closeKneeRight = CloseKnee(RightKneesInfo, h, Side.Right, false, _lastRigthKneePosition);
        var closeKneeLeft  = CloseKnee(LeftKneesInfo,  h, Side.Left,  false, _lastLeftKneePosition);

        var stringCloseKneeRight = closeKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(closeKneeRight.Value);
        var stringCloseKneeLeft  = closeKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(closeKneeLeft.Value);

        mensagem +=
            MessageSeparators.L2 +
            "CloseKneeRight" + MessageSeparators.SET + stringCloseKneeRight +
            MessageSeparators.L2 +
            "CloseKneeLeft"  + MessageSeparators.SET + stringCloseKneeLeft;


        var closeTrackKneeRight = CloseKnee(RightKneesInfo, h, Side.Right, true, _lastRigthKneePosition);
        var closeTrackKneeLeft  = CloseKnee(LeftKneesInfo,  h, Side.Left,  true, _lastLeftKneePosition);


        var stringCloseTrackKneeRight = closeTrackKneeRight == null ? "null" : CommonUtils.convertVectorToStringRPC(closeTrackKneeRight.Value);
        var stringCloseTrackKneeLeft  = closeTrackKneeLeft  == null ? "null" : CommonUtils.convertVectorToStringRPC(closeTrackKneeLeft.Value);

        mensagem +=
            MessageSeparators.L2 +
            "CloseTrackKneeRight" + MessageSeparators.SET + stringCloseTrackKneeRight +
            MessageSeparators.L2 +
            "CloseTrackKneeLeft"  + MessageSeparators.SET + stringCloseTrackKneeLeft;
        
        //  GetLastPosition(Tracker localTracker, Side thisSide, string idHuman, );
        return mensagem;
    }
     
     
     
     
     
     
     
     */
