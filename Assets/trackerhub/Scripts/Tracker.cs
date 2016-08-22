using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Windows.Kinect;
using System.Net.Sockets;
using System.Net;
using System.Text;

public enum CalibrationProcess
{
	None,
	FindCenter,
	FindForward,
	GetPlane,
	CalcNormal
}

public struct KneesInfo
{
    public int IdHuman;
    public string IdBody;
    public Vector3 Pos;
    public bool Track;
}


public class Tracker : MonoBehaviour
{

	private Dictionary<string, Sensor> _sensors;

	public Dictionary<string, Sensor> Sensors
	{
		get 
		{
			return _sensors;
		}
	}
	
	private CalibrationProcess _calibrationStatus;
	public CalibrationProcess CalibrationStatus 
    {
		get 
		{
			return _calibrationStatus;
		}

		set 
		{
			_calibrationStatus = value;
		}
	}
    
	private Dictionary<int, Human> _humans;

	private List<Human> _deadHumans;
	private List<Human> _humansToKill;

	private UdpBroadcast _udpBroadcast;
    private WriteSafeFile _writeSafeFile;
    private OptitrackManager _localOptitrackManager;

    // ReSharper disable once UnassignedField.Global
    public Material WhiteMaterial;

	public string[] UnicastClients {
		get {
			return _udpBroadcast.UnicastClients;
		}
	}

	public int ShowHumanBodies = -1;

	public bool ColorHumans;
    

    //public List<KneesInfo> RightKneesInfo;
    //public List<KneesInfo> LeftKneesInfo;

    public Dictionary<string, KneesInfo> RightKneesInfo;
    public Dictionary<string, KneesInfo> LeftKneesInfo;
    public Dictionary<string, KneesInfo> AllKneesInfo;

    private Vector3? _lastRigthKneePosition;
    private Vector3? _lastLeftKneePosition;

    public int CountHuman;

    public List<string> IdList;
    public List<int>    IdIntList;


    public bool SendKnees;


    void Start ()
	{
		_sensors           = new Dictionary<string, Sensor> ();
		_humans            = new Dictionary<int, Human> ();
		_deadHumans        = new List<Human> ();
		_humansToKill      = new List<Human> ();
		_calibrationStatus = CalibrationProcess.FindCenter;

		_udpBroadcast  = new UdpBroadcast (TrackerProperties.Instance.broadcastPort);
        _writeSafeFile = new WriteSafeFile();
        _localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
        
        _loadConfig();
        _loadSavedSensors();
        
        RightKneesInfo = new Dictionary<string, KneesInfo>();
        LeftKneesInfo  = new Dictionary<string, KneesInfo>();
        AllKneesInfo   = new Dictionary<string, KneesInfo>();

        IdList         = new List<string>();
        IdIntList      = new List<int>();


        _lastRigthKneePosition = null;
        _lastLeftKneePosition  = null;

        CountHuman = 0;
        SendKnees = false;

	}

	void FixedUpdate ()
	{
        RightKneesInfo = new Dictionary<string, KneesInfo>();
        LeftKneesInfo  = new Dictionary<string, KneesInfo>();
	    AllKneesInfo   = new Dictionary<string, KneesInfo>();

        IdList         = new List<string>();
        IdIntList      = new List<int>();


        if (Input.GetKeyDown (KeyCode.C))
			ColorHumans = !ColorHumans;

		foreach (Sensor s in _sensors.Values) {
			s.UpdateBodies ();
		}

		MergeHumans ();

		var deadHumansToRemove = new List<Human> ();
		foreach (var h in _deadHumans) {
			if (DateTime.Now > h.timeOfDeath.AddMilliseconds (1000))
				deadHumansToRemove.Add (h);
		}

		foreach (var h in deadHumansToRemove) {
			Destroy (h.gameObject);
			_deadHumans.Remove (h);
		}

		foreach (var h in _humansToKill) {
			Destroy (h.gameObject);
		}
		_humansToKill.Clear ();

		// udp broadcast

		string strToSend = "" + _humans.Count;


	    CountHuman = _humans.Count;

        foreach (var h in _humans.Values)
        {

            IdList.Add(h.ID.ToString());// "SpecialHuman " +
            IdIntList.Add(h.ID);
            
            // udpate Human Skeleton
            h.UpdateSkeleton ();
            
            // get PDU
            try
            {
                strToSend += MessageSeparators.L1 + h.GetPdu(); 
                if(SendKnees) strToSend += GetKnees(h);
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
                strToSend += MessageSeparators.L1 + h.GetPdu(); // + GetKnees(h);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "\n" + e.StackTrace);
            }
		}

        //Debug.Log("Send = " + strToSend);
        _udpBroadcast.Send (strToSend);
        SaveRecordServer(strToSend);

        // set human material
        foreach (var h in _humans.Values) {
			if (h.seenBySensor != null && ColorHumans)
				CommonUtils.changeGameObjectMaterial (h.gameObject, Sensors [h.seenBySensor].Material);
			else if (!ColorHumans)
				CommonUtils.changeGameObjectMaterial (h.gameObject, WhiteMaterial);
		}

		// show / hide human bodies

		if (ShowHumanBodies != -1 && !_humans.ContainsKey (ShowHumanBodies))
			ShowHumanBodies = -1;

		foreach (var h in _humans.Values) {
			CapsuleCollider humanCollider = h.gameObject.GetComponent<CapsuleCollider> ();
			if (humanCollider != null)
				humanCollider.enabled = (ShowHumanBodies == -1);

			foreach (Transform child in h.gameObject.transform) {
				if (child.gameObject.GetComponent<Renderer> () != null)
					child.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == -1);
			}

			foreach (SensorBody b in h.bodies) {
				b.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == h.ID);
			}
		}
	}

    // < Change >
    public Dictionary<int, Human> GetHumans()
    {
        return _humans;
    }

    public Human GetHuman(int id)
    {
        return _humans[id];
    }

    private static Vector3 GetLastPosition(Human human, Side thisSide, Vector3? lastPosition)
    {
        // ReSharper disable once MergeConditionalExpression
        var position = lastPosition != null ? lastPosition.Value : human.Skeleton.GetKnee(thisSide);
        return position;
    }

    private string GetKnees(Human h)
    {
        
        // CommonUtils.convertVectorToStringRPC
        // if (!_humans.ContainsKey(h1)) return null;

        if(h == null || !_humans.ContainsValue(h)) return null;

        // Human h = _humans[h1];
        // SensorBody bestBody = h.bodies[0];
        var mensagem = "";
        mensagem += MessageSeparators.L4;

        SaveTheKnees(h);

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
                CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]));

            var knee2 = _sensors[bodySensorId].PointSensorToScene(
                CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]));


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


            AllKneesInfo.Add(key + "_1", new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeRight,
                Track = (trackingStateKneeRight == TrackingState.Tracked)
            });


            AllKneesInfo.Add(key + "_2", new KneesInfo
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

    private void SaveTheKnees(Human h)
    {
        //var kneeRightList = new List<Vector3>();
        //var kneeLeftList  = new List<Vector3>();
        
        //Debug.Log("1  kneeRightList.Count = " + kneeRightList.Count);
        //Debug.Log("1  kneeLeftList.Count = "  + kneeLeftList.Count);
        
        foreach (var b in h.bodies)
        {
            var bodySensorId = b.sensorID;

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

            var kneeRight = _sensors[bodySensorId].PointSensorToScene(
                CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeRight]));
            
            var kneeLeft = _sensors[bodySensorId].PointSensorToScene(
                CommonUtils.pointKinectToUnity(b.skeleton.JointsPositions[JointType.KneeLeft]));
            
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


            AllKneesInfo.Add(key + "_1", new KneesInfo
            {
                IdHuman = h.ID,
                IdBody = bodySensorId,
                Pos = kneeRight,
                Track = (trackingStateKneeRight == TrackingState.Tracked)
            });
            
            AllKneesInfo.Add(key + "_2", new KneesInfo
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

    // < Change >
    /// <summary>
    /// (Pt) Guarda as mensagens enviadas pelo servidor em ficheiros txt
    /// (En) Store messages sent by the server in txt files
    /// </summary>
    /// <param name="strToSend"> Mensagem para Guardar </param>
    private void SaveRecordServer(string strToSend)
    {
        //, _localOptitrackManager.PositionVector


        const string noneMessage = "0";
        if (strToSend == noneMessage)
        {
            _writeSafeFile.StopRecording("Terminated Because No Messages");
            // _writeSafeFile.StopRecording();
        }
        else
        {
            if (_localOptitrackManager.IsOn) _writeSafeFile.Recording(strToSend, _localOptitrackManager.GetPositionVector(), _localOptitrackManager.GetRotationQuaternion());
            else _writeSafeFile.Recording(strToSend);
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void MergeHumans ()
	{
		List<SensorBody> aloneBodies = new List<SensorBody> ();

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
						foreach (SensorBody humanBody in h.Value.bodies)
                        {
							if (sensorBody.Value.sensorID == humanBody.sensorID && sensorBody.Value.ID == humanBody.ID)
                            {
								humanBody.LocalPosition = sensorBody.Value.LocalPosition;
								humanBody.lastUpdated = sensorBody.Value.lastUpdated;
								humanBody.updated = sensorBody.Value.updated;

								alone = false;
								break;
							}
						}

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
				h.Value.timeOfDeath = DateTime.Now;
				_deadHumans.Add (h.Value);
			} else {
				h.Value.Position = position;
			}
		}
		foreach (Human h in _deadHumans) {
			_humans.Remove (h.ID);
		}

		// new bodies
		foreach (SensorBody b in aloneBodies)
        {
			bool hasHuman = false;

			// try to fit in existing humans
			foreach (KeyValuePair<int, Human> h in _humans)
            {
				if (CalcHorizontalDistance (b.WorldPosition, h.Value.Position) < TrackerProperties.Instance.mergeDistance)
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
					if (CalcHorizontalDistance (b.WorldPosition, h.Position) < TrackerProperties.Instance.mergeDistance) {
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
					if (CalcHorizontalDistance (h1.Value.Position, h2.Value.Position) < TrackerProperties.Instance.mergeDistance)
                    {
						Vector3 position = (h1.Value.Position * (float) h1.Value.bodies.Count + h2.Value.Position * (float)h2.Value.bodies.Count) / (float)(h1.Value.bodies.Count + h2.Value.bodies.Count);

						if (h1.Value.ID < h2.Value.ID) {
							h1.Value.Position = position;
							foreach (SensorBody b in h2.Value.bodies)
                            {
								h1.Value.bodies.Add (b);
							}
							mergedHumans.Add (h2.Value);
						}
                        else
                        {
							h2.Value.Position = position;
							foreach (SensorBody b in h1.Value.bodies)
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
		foreach (Human h in mergedHumans)
        {
			_humansToKill.Add (h);
			_humans.Remove (h.ID);
		}
	}

    private static float CalcHorizontalDistance (Vector3 a, Vector3 b)
	{
		Vector3 c = new Vector3 (a.x, 0, a.z);
		Vector3 d = new Vector3 (b.x, 0, b.z);
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

	internal void SetNewCloud (CloudMessage cloud)
	{
		if (!Sensors.ContainsKey (cloud.KinectId)) {
			Vector3 position = new Vector3 (Mathf.Ceil (Sensors.Count / 2.0f) * (Sensors.Count % 2 == 0 ? -1.0f : 1.0f), 1, 0);
			
			Sensors [cloud.KinectId] = new Sensor (cloud.KinectId, (GameObject)Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, Quaternion.identity));
		}
		
		Sensors [cloud.KinectId].UpdateCloud (cloud);
	}

	internal void SetNewFrame (BodiesMessage bodies)
	{
        //Debug.Log("bodies = " + bodies.Message + ", KinectId = " + bodies.KinectId);


        if (!Sensors.ContainsKey (bodies.KinectId))
        {
			Vector3 position = new Vector3 (Mathf.Ceil (Sensors.Count / 2.0f) * (Sensors.Count % 2 == 0 ? -1.0f : 1.0f), 1, 0);
            

			Sensors [bodies.KinectId] = new Sensor (bodies.KinectId, (GameObject)Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, Quaternion.identity));
		}

		Sensors [bodies.KinectId].lastBodiesMessage = bodies;
	}

	internal bool CalibrationStep1 ()
	{
		bool cannotCalibrate = false;
		foreach (Sensor sensor in _sensors.Values) {
			if (sensor.Active) {
				if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1) {
					sensor.CalibrationStep1 ();
				} else
					cannotCalibrate = true;
			}
		}

		if (cannotCalibrate) {
			DoNotify n = gameObject.GetComponent<DoNotify> ();
			n.notifySend (NotificationLevel.IMPORTANT, "Calibration error", "Incorrect user placement!", 5000);
		}

		return !cannotCalibrate;
	}

	internal void CalibrationStep2 ()
	{

		Vector3 avgCenter = new Vector3 ();
		int sensorCount = 0;

		foreach (Sensor sensor in _sensors.Values) {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.CalibrationStep2 ();

				avgCenter += sensor.PointSensorToScene (sensor.CalibAuxPoint);
				sensorCount += 1;
			}
		}

		avgCenter /= sensorCount;

		foreach (Sensor sensor in _sensors.Values) {
			if (sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.move (avgCenter - sensor.PointSensorToScene (sensor.CalibAuxPoint));   
			}
		}

		_saveConfig ();

		DoNotify n = gameObject.GetComponent<DoNotify> ();
		n.notifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
	}

	internal void CalibrationStep3 ()
	{
		foreach (Sensor sensor in _sensors.Values) {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.CalibrationStep3 ();
			}
		}
	}

	internal void CalibrationStep4 ()
	{
		foreach (Sensor sensor in _sensors.Values) {
			if (sensor.lastBodiesMessage != null && sensor.lastBodiesMessage.Bodies.Count == 1 && sensor.Active) {
				sensor.CalibrationStep4 ();
			}
		}

		_saveConfig ();

		DoNotify n = gameObject.GetComponent<DoNotify> ();
		n.notifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
	}

	internal Vector3 GetJointPosition (int id, JointType joint)
	{
		Human h = _humans [id];

		SensorBody bestBody = h.bodies [0];
		int confidence = bestBody.Confidence;

		foreach (SensorBody b in h.bodies)
        {
			int bConfidence = b.Confidence;
			if (bConfidence > confidence)
            {
				confidence = bConfidence;
				bestBody = b;
			}
		}

		h.seenBySensor = bestBody.sensorID;

        return _sensors [bestBody.sensorID].PointSensorToScene (CommonUtils.pointKinectToUnity (bestBody.skeleton.JointsPositions [joint]));
	}

	internal bool HumanHasBodies (int id)
	{
		return _humans.ContainsKey (id) && _humans [id].bodies.Count > 0;
	}

    private void _saveConfig ()
	{
		string filePath = Application.dataPath + "/" + TrackerProperties.Instance.configFilename;
		ConfigProperties.clear (filePath);

		ConfigProperties.writeComment (filePath, "Config File created in " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss"));

		// save properties
		ConfigProperties.save (filePath, "udp.listenport", "" + TrackerProperties.Instance.listenPort);
		ConfigProperties.save (filePath, "udp.broadcastport", "" + TrackerProperties.Instance.broadcastPort);
		ConfigProperties.save (filePath, "udp.sendinterval", "" + TrackerProperties.Instance.sendInterval);
		ConfigProperties.save (filePath, "tracker.mergedistance", "" + TrackerProperties.Instance.mergeDistance);
		ConfigProperties.save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.confidenceTreshold);
//		ConfigProperties.save (filePath, "tracker.filtergain", "" + AdaptiveDoubleExponentialFilterFloat.Gain);

		// save sensors
		foreach (Sensor s in _sensors.Values) {
			if (s.Active) {
				Vector3 p = s.SensorGameObject.transform.position;
				Quaternion r = s.SensorGameObject.transform.rotation;
				ConfigProperties.save (filePath, "kinect." + s.SensorID, "" + s.SensorID + ";" + p.x + ";" + p.y + ";" + p.z + ";" + r.x + ";" + r.y + ";" + r.z + ";" + r.w);
			}
		}
	}

    private void _loadConfig ()
	{
		string filePath = Application.dataPath + "/" + TrackerProperties.Instance.configFilename;

		string port = ConfigProperties.Load (filePath, "udp.listenport");
		if (port != "") {
			TrackerProperties.Instance.listenPort = int.Parse (port);
		}
		ResetListening ();

		port = ConfigProperties.Load (filePath, "udp.broadcastport");
		if (port != "") {
			TrackerProperties.Instance.broadcastPort = int.Parse (port);
		}
		ResetBroadcast ();

		string aux = ConfigProperties.Load (filePath, "tracker.mergedistance");
		if (aux != "") {
			TrackerProperties.Instance.mergeDistance = float.Parse (aux);
		}

		aux = ConfigProperties.Load (filePath, "tracker.confidencethreshold");
		if (aux != "") {
			TrackerProperties.Instance.confidenceTreshold = int.Parse (aux);
		}

		aux = ConfigProperties.Load (filePath, "udp.sendinterval");
		if (aux != "") {
			TrackerProperties.Instance.sendInterval = int.Parse (aux);
		}

		/*aux = ConfigProperties.load (filePath, "tracker.filtergain");
		if (aux != "") {
			KalmanFilterFloat.Gain = float.Parse (aux);
		}*/
	}

    private void _loadSavedSensors ()
	{
		foreach (String line in ConfigProperties.loadKinects(Application.dataPath + "/" + TrackerProperties.Instance.configFilename)) {
			string[] values = line.Split (';');

			string id = values [0];

			Vector3 position = new Vector3 (
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
		_udpBroadcast.Reset (TrackerProperties.Instance.broadcastPort);
	}

    public void ResetListening ()
	{
		gameObject.GetComponent<UdpListener> ().udpRestart ();
	}

    public void Save ()
	{
		_saveConfig ();
	}

    public void HideAllClouds ()
	{
		foreach (Sensor s in _sensors.Values) {
			s.lastCloud.hideFromView ();
		}
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.createRequestMessage (2); 
		byte[] data = Encoding.UTF8.GetBytes(message);
		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.listenPort + 1);
		udp.Send(data, data.Length, remoteEndPoint);
	}

    public void BroadCastCloudRequests(bool continuous){
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.createRequestMessage (continuous?1:0); 
		byte[] data = Encoding.UTF8.GetBytes (message);
		IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.listenPort + 1);
		udp.Send (data, data.Length, remoteEndPoint);
	}

    void OnGUI ()
    {
        //int n = 1;

        if (ShowHumanBodies == -1) {
            foreach (Human h in _humans.Values) {
                //GUI.Label(new Rect(10, Screen.height - (n++ * 50), 1000, 50), "Human " + h.ID + " as seen by " + h.seenBySensor);

                Vector3 p = Camera.main.WorldToScreenPoint (h.Skeleton.GetHead () + new Vector3 (0, 0.2f, 0));
                if (p.z > 0) {
                    GUI.Label (new Rect (p.x, Screen.height - p.y - 25, 100, 25), "" + h.ID);
                }
            }
        }

        foreach (Sensor s in Sensors.Values) {
            if (s.Active) {
                Vector3 p = Camera.main.WorldToScreenPoint (s.SensorGameObject.transform.position + new Vector3 (0, 0.05f, 0));
                if (p.z > 0) {
                    GUI.Label (new Rect (p.x, Screen.height - p.y - 25, 100, 25), "" + s.SensorID);
                }
            }
        }
    }
}
/*
 * 
 * 
 * 
 * 
    private static Vector3? CloseKneeFromAll(Dictionary<string, KneesInfo> kneesList, Human human, Side thisSide, bool track, Vector3? lastPosition)
    {
        var position = GetLastPosition(human, thisSide, lastPosition);

        lastPosition = GetCloserKneeFromAll(kneesList, track, position);
        return lastPosition;
    }


    private static Vector3? GetCloserKneeFromAll(Dictionary<string, KneesInfo> kneesList, bool track, Vector3 position)
    {
        return track ? GetCloseKneeTrackFromAll(kneesList, position) : GetCloseKneeFromAll(kneesList, position);
    }

    private static Vector3? GetCloseKneeFromAll(Dictionary<string, KneesInfo> kneesList, Vector3 position)
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

    private static Vector3? GetCloseKneeTrackFromAll(Dictionary<string, KneesInfo> kneesList, Vector3 position)
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
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 
 *    //private string GetExtraInfo()
    //{
    //    string extra = null;

    //    foreach (var h in _humans)
    //    {
    //        extra = h.Value.
    //        //h.Value.Skeleton

    //    }



    //    return extra;
    //}
 
     
    // < Change >
    /// <summary>
    /// (Pt) Guarda as mensagens enviadas pelo servidor em ficheiros txt
    /// (En) Store messages sent by the server in txt files
    /// </summary>
    /// <param name="strToSend"> Mensagem para Guardar </param>
    /// <param name="optiTrackPos"> Posicao Obtida pelo OptiTrack </param>
    /// <param name="optiTrackOri"></param>
    private void SaveRecordServer(string strToSend, Vector3 optiTrackPos, Quaternion optiTrackOri)
    {
       

        //_localOptitrackManager.IsOn//
        const string noneMessage = "0";
        if (strToSend == noneMessage)
        {
            _writeSafeFile.StopRecording("Terminated Because No Messages");
            // _writeSafeFile.StopRecording();
        }
        else
        {
            _writeSafeFile.Recording(strToSend, optiTrackPos, optiTrackOri);
        }
    }


     //Debug.Log("2  kneeRightList.Count = " + kneeRightList.Count);
        //Debug.Log("2  kneeLeftList.Count = " + kneeLeftList.Count);



     switch (thisKnee)
            {
                case Side.Right:
                    position = human.Skeleton.GetKnee(thisKnee);
                    break;
                case Side.Left:
                    position = human.Skeleton.GetLeftKnee();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("thisKnee", thisKnee, null);
            }




     */
