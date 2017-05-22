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

// ReSharper disable once UnusedMember.Local
public class Tracker : MonoBehaviour
{
    private Dictionary<string, Sensor> _sensors;

    public Dictionary<string, Sensor> Sensors { get { return _sensors; } }

    private Dictionary<int, Human> _humans;
    
    private List<Human> _deadHumans;

    private List<Human> _humansToKill;

    private List<Surface> _surfaces;
    
    private UdpBroadcast _udpBroadcast;

    private CalibrationProcess _calibrationStatus;
    public CalibrationProcess CalibrationStatus 
    {
        get { return _calibrationStatus; }
        set { _calibrationStatus = value; }
    }
    
    public Material WhiteMaterial;
    public Material SurfaceMaterial;

    public string[] UnicastClients { get { return _udpBroadcast.UnicastClients; } }

    public int ShowHumanBodies = -1;

    public bool ColorHumans;

    // public bool colorHumans;
    // public List<KneesInfo> RightKneesInfo;
    // public List<KneesInfo> LeftKneesInfo;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////        Dissertação - Mestrado em Engenharia Informática e de Computadores                                   //////////
    //////        Francisco Henriques Venda, nº 73839                                                                  //////////
    //////        Alterações apartir daqui                                                                             //////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // private OptitrackManager _localOptitrackManager;

    public  Dictionary<string, KneesInfo> RightKneesInfo;

    public  Dictionary<string, KneesInfo> LeftKneesInfo;
    private Dictionary<string, KneesInfo> _allKneesInfo;

    public List<int> IdIntList;
    public List<string> IdList;
    
    private TrackerUI _localTrackerUi;

    private SafeWriteFile _safeWriteFile;
    private GameObject _centroGameObject;
    
    private Vector3? _lastRigthKneePosition;
    private Vector3? _lastLeftKneePosition;
    
    public Vector3 Centro;

    public int CountHuman;

    public bool SendKnees;

    private bool _setUpCentro;
    
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        RightKneesInfo = new Dictionary<string, KneesInfo>();
        LeftKneesInfo  = new Dictionary<string, KneesInfo>();

        IdList    = new List<string>();
        IdIntList = new List<int>();

        _safeWriteFile = new SafeWriteFile();

        CountHuman = 0;
    }

    void Start ()
    {
        _surfaces = new List<Surface>();
        _sensors = new Dictionary<string, Sensor> ();
        _humans = new Dictionary<int, Human> ();
        _deadHumans = new List<Human> ();
        _humansToKill = new List<Human> ();
        CalibrationStatus = CalibrationProcess.FindCenter;
        _udpBroadcast  = new UdpBroadcast (TrackerProperties.Instance.BroadcastPort);
        _safeWriteFile = new SafeWriteFile();

        LoadConfig();
        LoadSavedSensors();
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //_localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
        _localTrackerUi = gameObject.GetComponent<TrackerUI>();
    }
   
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void FixedUpdate ()
	{
	    //IdIntList      = new List<int>();
	    IdList = new List<string>();
	    var  idIntList = new List<int>();

	    if (Input.GetKeyDown (KeyCode.C)) ColorHumans = !ColorHumans;


		foreach (Sensor s in _sensors.Values)
        {
			s.UpdateBodies ();
		}

		MergeHumans();

        // var deadHumansToRemove = _deadHumans.Where(h => DateTime.Now > h.TimeOfDeath.AddMilliseconds(0)).ToList();

        //foreach (var h in _deadHumans)
        //{
        //   if (DateTime.Now > h.timeOfDeath.AddMilliseconds (1000)) deadHumansToRemove.Add (h);
	    //}
	    //List<Human> deadHumansToRemove = new List<Human>();

        //foreach (var h in deadHumansToRemove)
		List<Human> deadHumansToRemove = new List<Human> ();
		foreach (Human h in _deadHumans)
        {
			if (DateTime.Now > h.TimeOfDeath.AddMilliseconds (1000))
				deadHumansToRemove.Add (h);
		}

		foreach (Human h in deadHumansToRemove)
        {
			Destroy (h.gameObject);
			_deadHumans.Remove (h);
		}

	    foreach (Human h in _humansToKill)
        {
			Destroy (h.gameObject);
		}

		_humansToKill.Clear ();

		// udp broadcast
		var strToSend = "" + _humans.Count;

	    CountHuman = _humans.Count;

		foreach (Human h in _humans.Values)
        {

            IdList.Add(h.ID.ToString()); // "SpecialHuman " +
            idIntList.Add(h.ID);

            // update Human Skeleton
             h.UpdateSkeleton();

			// get PDU
			try
            {
				strToSend += MessageSeparators.L1 + h.GetPdu();
			}
            catch (Exception e)
			{
			    Debug.Log(e.Message + "\n" + e.StackTrace);
			}
		}

		foreach (Human h in _deadHumans)
        {
			try {
				strToSend += MessageSeparators.L1 + h.GetPdu();
			}
			catch (Exception e)
			{
			    Debug.Log(e.Message + "\n" + e.StackTrace);
			}
              
        }
        _udpBroadcast.Send(strToSend);
        
        //// FHV -> Alteração -> Guarda dados do Tracker 
        SaveRecordServer(strToSend);

		// set human material
        foreach (Human h in _humans.Values)
        {
			if (h.SeenBySensor != null && ColorHumans)
				CommonUtils.ChangeGameObjectMaterial(h.gameObject, Sensors [h.SeenBySensor].Material);
			else if (!ColorHumans)
				CommonUtils.ChangeGameObjectMaterial(h.gameObject, WhiteMaterial);
		}

		// show / hide human bodies

		if (ShowHumanBodies != -1 && !_humans.ContainsKey (ShowHumanBodies))
			ShowHumanBodies = -1;

		foreach (Human h in _humans.Values)
        {
			CapsuleCollider collider = h.gameObject.GetComponent<CapsuleCollider> ();
			if (collider != null)
				collider.enabled = (ShowHumanBodies == -1);

			foreach (Transform child in h.gameObject.transform)
            {
				if (child.gameObject.GetComponent<Renderer> () != null)
					child.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == -1);
			}
            
			foreach (SensorBody b in h.bodies)
            {
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


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void SetHumansVisibility(bool invisible)
    {
        foreach (var h in _humans.Values)
        {
            h.gameObject.SetActive(!invisible);
        }
    }

    private void MergeHumans()
    {
        List<SensorBody> alone_bodies = new List<SensorBody>();

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
                        if (!alone) break;
                    }
                    if (alone) alone_bodies.Add(sensorBody.Value);
                }
            }
        }

        // refresh existing humans
        foreach (KeyValuePair<int, Human> h in _humans)
        {

            Vector3 position = new Vector3();
            int numberOfBodies = 0;
            List<SensorBody> deadBodies = new List<SensorBody>();
            foreach (SensorBody b in h.Value.bodies)
            {
                if (b.updated && Sensors[b.sensorID].Active)
                    position = (position * (float) numberOfBodies++ + b.WorldPosition) / (float) numberOfBodies;
                else
                    deadBodies.Add(b);
            }
            
            foreach (SensorBody b in deadBodies)
            {
                h.Value.bodies.Remove(b);
            }
            
            if (h.Value.bodies.Count == 0)
            {
                h.Value.TimeOfDeath = DateTime.Now;
                _deadHumans.Add(h.Value);
            }
            else
            {
                h.Value.Position = position;
            }
        }

        foreach (Human h in _deadHumans)
        {
            _humans.Remove(h.ID);
        }

        // new bodies
        foreach (SensorBody b in alone_bodies)
        {
            bool hasHuman = false;
            // try to fit in existing humans
            foreach (KeyValuePair<int, Human> h in _humans)
            {
                if (CalcHorizontalDistance(b.WorldPosition, h.Value.Position) < TrackerProperties.Instance.MergeDistance)
                {
                    h.Value.Position = (h.Value.Position * (float)h.Value.bodies.Count + b.WorldPosition) / (float)(h.Value.bodies.Count + 1);
                    h.Value.bodies.Add(b);
                    hasHuman = true;
                    break;
                }
            }

            if (!hasHuman)
            {
                // try to fit in dead humans
                foreach (Human h in _deadHumans)
                {
                    if (CalcHorizontalDistance(b.WorldPosition, h.Position) < TrackerProperties.Instance.MergeDistance)
                    {
                        h.Position = (h.Position * (float)h.bodies.Count + b.WorldPosition) / (float)(h.bodies.Count + 1);
                        h.bodies.Add(b);
                        hasHuman = true;
                        break;
                    }
                }
                if (!hasHuman)
                {
                    // create new human
                    Human h = new Human((GameObject)Instantiate(Resources.Load("Prefabs/Human")), this);
                    h.bodies.Add(b);
                    h.Position = b.WorldPosition;
                    _humans[h.ID] = h;
                }
            }
        }
        
        // bring back to life selected dead humans
        List<Human> undeadHumans = new List<Human>();
        foreach (Human h in _deadHumans)
        {
            if (h.bodies.Count > 0)
            {
                _humans[h.ID] = h;
                undeadHumans.Add(h);
            }
        }

        foreach (Human h in undeadHumans)
        {
            _deadHumans.Remove(h);
        }
        
        // merge humans

        List<Human> mergedHumans = new List<Human>();
        foreach (KeyValuePair<int, Human> h1 in _humans)
        {
            foreach (KeyValuePair<int, Human> h2 in _humans)
            {
                if (h1.Value.ID != h2.Value.ID && !mergedHumans.Contains(h2.Value))
                {
                    if (CalcHorizontalDistance(h1.Value.Position, h2.Value.Position) < TrackerProperties.Instance.MergeDistance)
                    {
                        Vector3 position = (h1.Value.Position * (float)h1.Value.bodies.Count + h2.Value.Position * (float)h2.Value.bodies.Count) / (float)(h1.Value.bodies.Count + h2.Value.bodies.Count);
                        
                        if (h1.Value.ID < h2.Value.ID)
                        {
                            h1.Value.Position = position;
                            foreach (SensorBody b in h2.Value.bodies)
                            {
                                h1.Value.bodies.Add(b);
                            }
                            mergedHumans.Add(h2.Value);
                        }
                        else
                        {
                            h2.Value.Position = position;
                            foreach (SensorBody b in h1.Value.bodies)
                            {
                                h2.Value.bodies.Add(b);
                            }

                            mergedHumans.Add(h1.Value);
                        }
                        break;
                    }
                }
            }
        }

        foreach (Human h in mergedHumans)
        {
            _humansToKill.Add(h);
            _humans.Remove(h.ID);
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

    ////FOR TCP

    internal void SetNewCloud(string kinectId, byte[] data, int size, uint id)
    {
        // tirar o id da mensagem que é um int

        if (Sensors.ContainsKey(kinectId))
        {
            Sensors[kinectId].lastCloud.SetPoints(data, 0, id, size);
            Sensors[kinectId].lastCloud.setToView();
        }
    }

    internal void SetNewCloud (CloudMessage cloud)
	{
	    //if (!Sensors.ContainsKey (cloud.KinectId)) {
	    //	Vector3 position = new Vector3 (Mathf.Ceil (Sensors.Count / 2.0f) * (Sensors.Count % 2 == 0 ? -1.0f : 1.0f), 1, 0);
	    //          Sensors [cloud.KinectId] = new Sensor (cloud.KinectId, (GameObject)Instantiate (Resources.Load ("Prefabs/KinectSensorPrefab"), position, Quaternion.identity));
	    //}
	    //      Sensors[cloud.KinectId].lastCloudupdateCloud (cloud);
        int step = cloud.HeaderSize + 3; // TMA: Size in bytes of heading: "CloudMessage" + L0 + 2 * L1. Check the UDPListener.cs from the Client.
        string[] pdu = cloud.Message.Split(MessageSeparators.L1);

        string kinectId = pdu[0];
        //>>>>>>> refs/remotes/mauriciosousa/master
        uint  id = uint.Parse(pdu[1]);
        step += pdu[0].Length + pdu[1].Length;

        if (Sensors.ContainsKey(kinectId))
        {
            if (pdu[2] == "") {
                Sensors[kinectId].lastCloud.setToView();
            }
            else
                Sensors[kinectId].lastCloud.SetPoints(cloud.ReceivedBytes,step,id,cloud.ReceivedBytes.Length);
        }
	}

    internal void SetCloudToView(string kinectId)
    {
        Sensors[kinectId].lastCloud.setToView();
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
			n.NotifySend (NotificationLevel.IMPORTANT, "Calibration error", "Incorrect user placement!", 5000);
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
		n.NotifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
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
		n.NotifySend (NotificationLevel.INFO, "Calibration complete", "Config file updated", 5000);
	}

    internal Vector3 GetHandScreenSpace(int id, HandScreenSpace type)
    {
        Human h = _humans[id];
        SensorBody s = h.bodies[0];

        return s.skeleton.HandScreenPositions[type];
    }

    ///// Versão usada no ramo Master
    internal string GetHandState(int id, BodyPropertiesTypes type)
    {
        Human h = _humans[id];
        SensorBody s = h.bodies[0];
        return s.skeleton.BodyProperties[type];
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
    
    internal bool HumanHasBodies (int id)
	{
		return _humans.ContainsKey (id) && _humans [id].bodies.Count > 0;
	}
    
    private void _saveConfig ()
	{
		var filePath = Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename;
		ConfigProperties.Clear (filePath);

		ConfigProperties.WriteComment (filePath, "Config File created in " + DateTime.Now.ToString ("yyyy-MM-dd HH:mm:ss"));

		// save properties

		ConfigProperties.Save (filePath, "udp.listenport",              "" + TrackerProperties.Instance.ListenPort);
		ConfigProperties.Save (filePath, "udp.broadcastport",           "" + TrackerProperties.Instance.BroadcastPort);
        ConfigProperties.Save (filePath, "udp.sensor.listener",         "" + TrackerProperties.Instance.SensorListenPort);
	    ConfigProperties.Save (filePath, "udp.sendinterval",            "" + TrackerProperties.Instance.SendInterval);
	    ConfigProperties.Save (filePath, "tracker.mergedistance",       "" + TrackerProperties.Instance.MergeDistance);
	    ConfigProperties.Save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.ConfidenceTreshold);
        
		// save sensors
		foreach (Sensor s in Sensors.Values)
//=======
//		ConfigProperties.save (filePath, "udp.listenport", "" + TrackerProperties.Instance.listenPort);
//		ConfigProperties.save (filePath, "udp.broadcastport", "" + TrackerProperties.Instance.broadcastPort);
//        ConfigProperties.save (filePath, "udp.sensor.listener", "" + TrackerProperties.Instance.sensorListenPort);
//        ConfigProperties.save (filePath, "udp.sendinterval", "" + TrackerProperties.Instance.sendInterval);
//		ConfigProperties.save (filePath, "tracker.mergedistance", "" + TrackerProperties.Instance.mergeDistance);
//		ConfigProperties.save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.confidenceTreshold);

//		// save sensors
//		foreach (Sensor s in _sensors.Values)
//>>>>>>> refs/remotes/mauriciosousa/master
        {
			if (s.Active)
            {
				Vector3 p = s.SensorGameObject.transform.position;
				Quaternion r = s.SensorGameObject.transform.rotation;
				ConfigProperties.Save (filePath, "kinect." + s.SensorID, "" + s.SensorID + ";" + p.x + ";" + p.y + ";" + p.z + ";" + r.x + ";" + r.y + ";" + r.z + ";" + r.w);
			}
		}
	}
    
    private void LoadConfig ()
	{
		var filePath = Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename;

		var port = ConfigProperties.Load (filePath, "udp.listenport");
		if (port != "") {
			TrackerProperties.Instance.ListenPort = int.Parse (port);

		//string port = ConfigProperties.load (filePath, "udp.listenport");
		//if (port != "")
  //      {
		//	TrackerProperties.Instance.listenPort = int.Parse (port);
		}
		ResetListening ();

		port = ConfigProperties.Load (filePath, "udp.broadcastport");
		if (port != "")
        {
			TrackerProperties.Instance.BroadcastPort = int.Parse (port);
        //=======
		//port = ConfigProperties.load (filePath, "udp.broadcastport");
		//if (port != "")
        // {
		//	TrackerProperties.Instance.broadcastPort = int.Parse (port);
		}
		ResetBroadcast ();
////<<<<<<< HEAD
//		string aux = ConfigProperties.Load (filePath, "tracker.mergedistance");
//
//        port = ConfigProperties.Load(filePath, "udp.sensor.listen");
//=======
        
        // port = ConfigProperties.Load(filePath, "udp.sensor.listen");
        port = ConfigProperties.Load(filePath, "udp.sensor.listener");

        if (port != "")
        {
            TrackerProperties.Instance.SensorListenPort = int.Parse(port);
        }

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
//=======
//        string aux = ConfigProperties.load (filePath, "tracker.mergedistance");
//		if (aux != "")
//        {
//			TrackerProperties.Instance.mergeDistance = float.Parse (aux);
//		}

//		aux = ConfigProperties.load (filePath, "tracker.confidencethreshold");
//		if (aux != "")
//        {
//			TrackerProperties.Instance.confidenceTreshold = int.Parse (aux);
//		}

//		aux = ConfigProperties.load (filePath, "udp.sendinterval");
//		if (aux != "")
//        {
//			TrackerProperties.Instance.sendInterval = int.Parse (aux);
//		}
//>>>>>>> refs/remotes/mauriciosousa/master
	}
    
    private void LoadSavedSensors ()
	{
		foreach (var line in ConfigProperties.LoadKinects(Application.dataPath + "/" + TrackerProperties.Instance.ConfigFilename)) {
			var values = line.Split (';');

		//foreach (String line in ConfigProperties.loadKinects(Application.dataPath + "/" + TrackerProperties.Instance.configFilename))
  //      {
		//	string[] values = line.Split (';');

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
		//foreach (var s in Sensors.Values)

		foreach (Sensor s in _sensors.Values)
        {
			s.lastCloud.hideFromView ();
		}

		UdpClient udp = new UdpClient ();
		string message = CloudMessage.CreateRequestMessage (2,Network.player.ipAddress, TrackerProperties.Instance.ListenPort); 
		byte[] data = Encoding.UTF8.GetBytes(message);

        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.SensorListenPort);

        //<<<<<<< HEAD
        //		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //=======
		IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.SensorListenPort);
		udp.Send(data, data.Length, remoteEndPoint);
	}
    
    // ReSharper disable once MemberCanBeMadeStatic.Global
    public void BroadCastCloudRequests (bool continuous)
	{
		UdpClient udp = new UdpClient ();
		string message = CloudMessage.CreateRequestMessage (continuous ? 1 : 0, Network.player.ipAddress, TrackerProperties.Instance.ListenPort); 
		byte[] data = Encoding.UTF8.GetBytes (message);
        //IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //<<<<<<< HEAD
        //  IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //=======
		IPEndPoint remoteEndPoint = new IPEndPoint (IPAddress.Broadcast, TrackerProperties.Instance.SensorListenPort);
		udp.Send (data, data.Length, remoteEndPoint);
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
                var p = Camera.main.WorldToScreenPoint (h.Skeleton.GetHead() + new Vector3 (0, 0.2f, 0));
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

        // Calibration
        // string message = av.createCalibrationMessage(_sensors);
        // Calibration
        // string message = av.createCalibrationMessage(_sensors);
        string message = av.createCalibrationMessage(_sensors);

        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(av.ReplyIpAddress, av.Port);
        Debug.Log("Sent reply with calibration data " + message);
        udp.Send(data, data.Length, remoteEndPoint);


        // Broadcast
        // string message2 = CloudMessage.createRequestMessage(av.mode, av.replyIPAddress.ToString(), av.port);
        // Broadcast
        string message2 = CloudMessage.CreateRequestMessage(av.Mode, av.ReplyIpAddress.ToString(), av.Port);
      
        byte[] data2 = Encoding.UTF8.GetBytes(message2);
        //IPEndPoint remoteEndPoint2 = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //<<<<<<< HEAD
        //        IPEndPoint remoteEndPoint2 = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.ListenPort + 1);
        //=======
        IPEndPoint remoteEndPoint2 = new IPEndPoint(IPAddress.Broadcast, TrackerProperties.Instance.SensorListenPort);

        udp.Send(data2, data2.Length, remoteEndPoint2);
        Debug.Log("Forwarded request to clients " + message2);
    }

    public void ProcessSurfaceMessage(SurfaceMessage sm)
    {
        UdpClient udp = new UdpClient();
        string message = sm.createSurfaceMessage(_surfaces);
        byte[] data = Encoding.UTF8.GetBytes(message);
        IPEndPoint remoteEndPoint = new IPEndPoint(sm.replyIPAddress, sm.port);
        Debug.Log("Sent reply with surface's data " + message);
        udp.Send(data, data.Length, remoteEndPoint);
    }
    
    internal void LoadSurfaces()
    {
        _surfaces = new List<Surface>(Surface.loadSurfaces("Surfaces"));
        foreach (Surface s in _surfaces)
        {
            if (Sensors.ContainsKey(s.sensorid))
            {
                s.surfaceGO = __createSurfaceGos(s.name, Vector3.zero, Sensors[s.sensorid].SensorGameObject.transform);

                GameObject bl = __createSurfaceGos("bl", s.BottomLeft,  s.surfaceGO.transform);
                GameObject br = __createSurfaceGos("br", s.BottomRight, s.surfaceGO.transform);
                GameObject tl = __createSurfaceGos("tl", s.TopLeft,     s.surfaceGO.transform);
                GameObject tr = __createSurfaceGos("tr", s.TopRight,    s.surfaceGO.transform);
                
                gameObject.GetComponent<DoNotify>().NotifySend(NotificationLevel.INFO, "New Surface", "Surface " + s.name + " added", 5000);

                s.SaveSurface(bl, br, tl, tr);

                MeshFilter meshFilter = (MeshFilter)s.surfaceGO.AddComponent(typeof(MeshFilter));
                Mesh m = new Mesh();
                m.name = s.name + "MESH";
                m.vertices = new Vector3[]
                {
                    bl.transform.localPosition, br.transform.localPosition, tr.transform.localPosition, tl.transform.localPosition
                };

                m.triangles = new int[]
                {
                    0, 1, 3,
                    1, 2, 3,
                    3, 1, 0,
                    3, 2, 1
                };
                m.RecalculateNormals();
                m.RecalculateBounds();

                meshFilter.mesh = m;
                MeshRenderer renderer = s.surfaceGO.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
                renderer.material = SurfaceMaterial;
            }
        }
    }
    
    internal GameObject __createSurfaceGos(string nameSurface, Vector3 position, Transform parent)
    {
        GameObject g = new GameObject();
        g.transform.parent = parent;
        g.transform.localRotation = Quaternion.identity;

        g.name = name + "lol";
        g.transform.localPosition = CommonUtils.PointKinectToUnity(position); 

        return g;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////        Dissertação - Mestrado em Engenharia Informática e de Computadores                                   //////////
    //////        Francisco Henriques Venda, nº 73839                                                                  //////////
    //////        Alterações apartir daqui (Em uso)                                                                    //////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
        // Debug
        foreach (var sensor in Sensors)
        {
            var start = sensor.Value.SensorGameObject.transform.position;
            var forward = sensor.Value.SensorGameObject.transform.forward;

            Debug.DrawLine(start, start + forward, Color.black);
        }
    }

    ///// Versão alterada, não é necessario 
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
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////                              Alterações que foram abandonadas                                               //////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private static Vector3 GetLastPosition(Human human, Side thisSide, Vector3? lastPosition)
    {
        // ReSharper disable once MergeConditionalExpression
        var position = lastPosition != null ? lastPosition.Value : human.Skeleton.GetKnee(thisSide);
        return position;
    }

    private string GetKnees(Human h)
    {
        // CommonUtils.ConvertVectorToStringRPC
        // if (!_humans.ContainsKey(h1)) return null;

        if (h == null || !_humans.ContainsValue(h)) return null;

        // Human h = _humans[h1];
        // SensorBody bestBody = h.bodies[0];
        var mensagem = "";
        mensagem += MessageSeparators.L4;

        //SaveTheKnees(h);

        var meanKneeRight = GetMeanList(RightKneesInfo);
        var meanKneeLeft  = GetMeanList(LeftKneesInfo);

        var stringMeanKneeRight = meanKneeRight == null ? "null" : CommonUtils.ConvertVectorToStringRPC(meanKneeRight.Value);
        var stringMeanKneeLeft  = meanKneeLeft  == null ? "null" : CommonUtils.ConvertVectorToStringRPC(meanKneeLeft.Value);

        mensagem +=
            "MeanKneeRight" + MessageSeparators.SET + stringMeanKneeRight + MessageSeparators.L2 +
            "MeanKneeLeft"  + MessageSeparators.SET + stringMeanKneeLeft;

        var meanTrackKneeRight = GetMeanTrackList(RightKneesInfo);
        var meanTrackKneeLeft = GetMeanTrackList(LeftKneesInfo);

        var stringMeanTrackKneeRight = meanTrackKneeRight == null ? "null" : CommonUtils.ConvertVectorToStringRPC(meanTrackKneeRight.Value);
        var stringMeanTrackKneeLeft = meanTrackKneeLeft == null ? "null" : CommonUtils.ConvertVectorToStringRPC(meanTrackKneeLeft.Value);

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
}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 * 
//=======
    //		if (showHumanBodies == -1)
    //        {
    //			foreach (Human h in _humans.Values)
    //            {
    //				//GUI.Label(new Rect(10, Screen.height - (n++ * 50), 1000, 50), "Human " + h.ID + " as seen by " + h.seenBySensor);
    //>>>>>>> refs/remotes/mauriciosousa/master
     
    //internal Vector3 GetJointPosition(int id, JointType joint)
    //{
    //    var h = _humans[id];

    //    var bestBody = h.bodies[0];
    //    var confidence = bestBody.Confidence;
    //    var lastSensorConfidence = 0;
    //    SensorBody lastSensorBody = null;

    //    foreach (var b in h.bodies)
    //    {
    //        var bConfidence = b.Confidence;
    //        if (bConfidence > confidence)
    //        {
    //            confidence = bConfidence;
    //            bestBody = b;
    //        }

    //        if (b.sensorID == h.SeenBySensor)
    //        {
    //            lastSensorConfidence = bConfidence;
    //            lastSensorBody = b;
    //        }
    //    }
    //}



    internal Vector3 getJointPosition (int id, JointType joint, Vector3 garbage)
	{
		Human h = _humans [id];
		SensorBody bestBody = h.bodies [0];
		int confidence = bestBody.Confidence;
		int lastSensorConfidence = 0;
		SensorBody lastSensorBody = null;

		foreach (SensorBody b in h.bodies)
        {
			int bConfidence = b.Confidence;
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
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
     internal string GetHandState(int id, BodyPropertiesTypes type)
    {
        Human      h = _humans[id];
        SensorBody s = h.bodies[0];

        return s.skeleton.BodyProperties[type];
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    
	    //ConfigProperties.save (filePath, "udp.sendinterval", "" + TrackerProperties.Instance.sendInterval);
		//ConfigProperties.save (filePath, "tracker.mergedistance", "" + TrackerProperties.Instance.mergeDistance);
		//ConfigProperties.save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.confidenceTreshold);
        //ConfigProperties.save (filePath, "tracker.filtergain", "" + AdaptiveDoubleExponentialFilterFloat.Gain);
    

	    //ConfigProperties.Save (filePath, "udp.sensor.listener",         "" + TrackerProperties.Instance.SensorListenPort);
        //ConfigProperties.Save (filePath, "udp.sendinterval",            "" + TrackerProperties.Instance.SendInterval);
		//ConfigProperties.Save (filePath, "tracker.mergedistance",       "" + TrackerProperties.Instance.MergeDistance);
		//ConfigProperties.Save (filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.ConfidenceTreshold);
        //=======
        //ConfigProperties.save(filePath, "udp.listenport", "" + TrackerProperties.Instance.listenPort);
        //ConfigProperties.save(filePath, "udp.broadcastport", "" + TrackerProperties.Instance.broadcastPort);
        //ConfigProperties.save(filePath, "udp.sensor.listener", "" + TrackerProperties.Instance.sensorListenPort);
        //ConfigProperties.save(filePath, "udp.sendinterval", "" + TrackerProperties.Instance.sendInterval);
        //ConfigProperties.save(filePath, "tracker.mergedistance", "" + TrackerProperties.Instance.mergeDistance);
        //ConfigProperties.save(filePath, "tracker.confidencethreshold", "" + TrackerProperties.Instance.confidenceTreshold);
        //>>>>>>> refs/remotes/origin/master
        //		ConfigProperties.save (filePath, "tracker.filtergain", "" + AdaptiveDoubleExponentialFilterFloat.Gain);

        // save sensors
        //foreach (Sensor s in Sensors.Values)


////////////////////////////////////////////////////////////////////////////////
 * 

//<<<<<<< HEAD

    // ReSharper disable once UnusedMember.Local

    // ReSharper disable once ArrangeTypeMemberModifiers

//    void Start ()

//	{

//		Sensors           = new Dictionary<string, Sensor> ();

//		_humans           = new Dictionary<int,     Human> ();

//		_deadHumans       = new List<Human> ();

//		_humansToKill     = new List<Human> ();

//		CalibrationStatus = CalibrationProcess.FindCenter;

//=======
 	public string[] UnicastClients
    {
		get
        {
			return _udpBroadcast.UnicastClients;
		}
	}

          //    >>>>>>> b8f19cc2489e5b0bb725e99ab5d20315d7961f60

    //        CountHuman = 0;


    //    RightKneesInfo = new Dictionary<string, KneesInfo>();
    //    LeftKneesInfo  = new Dictionary<string, KneesInfo>();


    //}

    //<<<<<<< HEAD
//                gameObject.GetComponent<DoNotify>().NotifySend(NotificationLevel.INFO, "New Surface", "Surface " + s.name + " added", 5000);
//=======
=======

     */

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*


//    // ReSharper disable once UnusedMember.Local
//    // ReSharper disable once ArrangeTypeMemberModifiers
//    void Start ()
//	{
//		Sensors           = new Dictionary<string, Sensor> ();
//		_humans           = new Dictionary<int,     Human> ();
//		_deadHumans       = new List<Human> ();
//		_humansToKill     = new List<Human> ();
//		CalibrationStatus = CalibrationProcess.FindCenter;

<<<<<<< HEAD
gameObject.GetComponent<DoNotify>().NotifySend(NotificationLevel.INFO, "New Surface", "Surface " + s.name + " added", 5000);
=======

    <<<<<<< HEAD
        g.name = nameSurface;
        g.transform.localPosition = CommonUtils.PointKinectToUnity(position); 
=======

*/

/*
   
    
        //_loadConfig ();
        //_loadSavedSensors ();

public Material SurfaceMaterial;
public Material WhiteMaterial;

    
    
        return _udpBroadcast.UnicastClients;
    }
}

    private bool _setUpCentro;


    public int CountHuman;
    public Vector3 Centro;

    public bool SendKnees;
    private TrackerUI _localTrackerUi;
    private SafeWriteFile _safeWriteFile;
    private GameObject _centroGameObject;



    //public List<KneesInfo> RightKneesInfo;
    //public List<KneesInfo> LeftKneesInfo;

    public Dictionary<string, KneesInfo> RightKneesInfo;
    public Dictionary<string, KneesInfo> LeftKneesInfo;
    //private OptitrackManager _localOptitrackManager;



    
        RightKneesInfo = new Dictionary<string, KneesInfo>();
        LeftKneesInfo  = new Dictionary<string, KneesInfo>();
        CountHuman = 0;

    
        IdList    = new List<string>();
        IdIntList = new List<int>();


    //=======
//	    Sensors = new Dictionary<string, Sensor> ();
//		_humans = new Dictionary<int, Human> ();
//		_deadHumans = new List<Human> ();
//		_humansToKill = new List<Human> ();
//	    CalibrationStatus = CalibrationProcess.FindCenter;

//		_udpBroadcast  = new UdpBroadcast (TrackerProperties.Instance.BroadcastPort);
       
//        _loadConfig();
//	    _loadSavedSensors();
//        ////////////////////////////////////////////////////////////////////////////
//	    ////////////////////////////////////////////////////////////////////////////
//        // _localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
//        _localTrackerUi = gameObject.GetComponent<TrackerUI>();
//	}

    //<<<<<<< HEAD
////        g.name = nameSurface;
////        g.transform.localPosition = CommonUtils.PointKinectToUnity(position); 

//        g.name = name + "lol";
//        g.transform.localPosition = CommonUtils.PointKinectToUnity(position);
//=======



//<<<<<<< HEAD
//		string aux = ConfigProperties.Load (filePath, "tracker.mergedistance");
//=======



 */


// =======
//}


/*
 * 
 * 
 * 
 * 
//<<<<<<< HEAD
//		foreach (var h in _humans.Values)
//        {
//			var humanCollider = h.gameObject.GetComponent<CapsuleCollider> ();
//			if (humanCollider != null)
//				humanCollider.enabled = (ShowHumanBodies == -1);
//=======
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 <<<<<<< HEAD
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
=======
     ////////////////////////////////////////
     
        // private float calcHorizontalDistance (Vector3 a, Vector3 b)

              */





/*
 //<<<<<<< HEAD
//			foreach (var b in h.bodies)
//            {
//				b.gameObject.GetComponent<Renderer> ().enabled = (ShowHumanBodies == h.ID);
//=======
     //<<<<<<< HEAD
//		foreach (var h in _humans.Values)
//        {
//			if (h.SeenBySensor != null && colorHumans)
//				CommonUtils.ChangeGameObjectMaterial (h.gameObject, Sensors [h.SeenBySensor].Material);
//			else if (!ColorHumans)
//				CommonUtils.ChangeGameObjectMaterial (h.gameObject, WhiteMaterial);
//=======
     
     
//////////////////////////////////////////////////////////////
    // public Dictionary<string, Sensor> Sensors { get; private set; }
    // public CalibrationProcess CalibrationStatus { get; set; }
     
       // private List<Surface> _surfaces;

    //public string[] UnicastClients
    //{
    //    get
    //    {
    //        return _udpBroadcast.UnicastClients;
    //    }
    //}
     
     //<<<<<<< HEAD
//		foreach (var s in Sensors.Values)
//        {
//			s.UpdateBodies ();
//=======
     	// foreach (var h in _humansToKill)
     




    // <<<<<<< HEAD
	 
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
// =======






     */
///////////////////////////////////////////////////////////////////




/*
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
//////////////////////////////////////////////////////////////////////////////////////////////////////
//    private void MergeHumans()
//    {
//        var aloneBodies = new List<SensorBody>();

//        // refresh existing bodies
//        foreach (Sensor s in Sensors.Values)
//        {
//            if (s.Active)
//            {
//                foreach (KeyValuePair<string, SensorBody> sensorBody in s.bodies)
//                {
//                    bool alone = true;
//                    foreach (KeyValuePair<int, Human> h in _humans)
//                    {
//                        foreach (SensorBody humanBody in h.Value.bodies)
//                        {
//                            if (sensorBody.Value.sensorID == humanBody.sensorID && sensorBody.Value.ID == humanBody.ID)
//                            {
//                                humanBody.LocalPosition = sensorBody.Value.LocalPosition;
//                                humanBody.lastUpdated = sensorBody.Value.lastUpdated;
//                                humanBody.updated = sensorBody.Value.updated;

//                                alone = false;
//                                break;
//                            }
//                        }


//                        //if (bodyToRemove != null)
//                        //{
//                        //    h.Value.bodies.Remove(bodyToRemove);
//                        //    break;
//                        //}

//                        if (!alone)
//                            break;
//                    }

//                    if (alone)
//                        aloneBodies.Add(sensorBody.Value);
//                }
//            }
//        }

//        // refresh existing humans
//        foreach (KeyValuePair<int, Human> h in _humans)
//        {
//            Vector3 position = new Vector3();
//            int numberOfBodies = 0;
//            List<SensorBody> deadBodies = new List<SensorBody>();

//            foreach (SensorBody b in h.Value.bodies)
//            {
//                if (b.updated && Sensors[b.sensorID].Active)
//                    position = (position * (float) numberOfBodies++ + b.WorldPosition) / (float) numberOfBodies;
//                else
//                    deadBodies.Add(b);
//            }

//            foreach (SensorBody b in deadBodies)
//            {
//                h.Value.bodies.Remove(b);
//            }

//            if (h.Value.bodies.Count == 0)
//            {
//                h.Value.TimeOfDeath = DateTime.Now;
//                _deadHumans.Add(h.Value);
////=======
////			if (h.Value.bodies.Count == 0)
////            {
////				h.Value.timeOfDeath = DateTime.Now;
//            }
//            else
//            {
//                h.Value.Position = position;
//            }
//        }
//        foreach (Human h in _deadHumans)
//        {
//            _humans.Remove(h.ID);
//        }

//        // new bodies
//        foreach (var b in aloneBodies)
//        {
//            var hasHuman = false;
//            //=======
//            //		foreach (SensorBody b in alone_bodies)
//            //        {
//            //			bool hasHuman = false;
//            // try to fit in existing humans
//            foreach (KeyValuePair<int, Human> h in _humans)
//            {
//                //<<<<<<< HEAD
//                //************* NEW DM STUFF *******************
//                // if the human has a body from the same sensor it cannot have another
//                var canHaveThisBody = true;
//                foreach (SensorBody humanBody in h.Value.bodies)
//                {
//                    if (humanBody.sensorID == b.sensorID)
//                    {
//                        canHaveThisBody = false;
//                        break;
//                    }
//                }
//                if (!canHaveThisBody)
//                    continue;
//                //************* END DM STUFF *******************

//                if (CalcHorizontalDistance(b.WorldPosition, h.Value.Position) <
//                    TrackerProperties.Instance.MergeDistance)
//                    //if (calcHorizontalDistance (b.WorldPosition, h.Value.Position) < TrackerProperties.Instance.mergeDistance)

//                {
//                    h.Value.Position = (h.Value.Position * (float) h.Value.bodies.Count + b.WorldPosition) /
//                                       (float) (h.Value.bodies.Count + 1);
//                    h.Value.bodies.Add(b);
//                    hasHuman = true;
//                    break;
//                }
//            }

//            if (!hasHuman)
//            {
//                // try to fit in dead humans
//                foreach (Human h in _deadHumans)
//                {
//                    if (CalcHorizontalDistance(b.WorldPosition, h.Position) < TrackerProperties.Instance.MergeDistance)
//                    {

//                        //foreach (Human h in _deadHumans)
//                        //            {
//                        //	if (calcHorizontalDistance (b.WorldPosition, h.Position) < TrackerProperties.Instance.mergeDistance)
//                        //                {
//                        h.Position = (h.Position * (float) h.bodies.Count + b.WorldPosition) /
//                                     (float) (h.bodies.Count + 1);
//                        h.bodies.Add(b);
//                        hasHuman = true;
//                        break;
//                    }
//                }

//                if (!hasHuman)
//                {
//                    // create new human
//                    Human h = new Human((GameObject) Instantiate(Resources.Load("Prefabs/Human")), this);

//                    h.bodies.Add(b);
//                    h.Position = b.WorldPosition;

//                    _humans[h.ID] = h;
//                }
//            }
//        }

//        // bring back to life selected dead humans
//        List<Human> undeadHumans = new List<Human>();
//        foreach (Human h in _deadHumans)
//        {
//            if (h.bodies.Count > 0)
//            {
//                _humans[h.ID] = h;
//                undeadHumans.Add(h);
//            }
//        }
//        foreach (Human h in undeadHumans)
//        {
//            _deadHumans.Remove(h);
//        }

//        // merge humans
//        List<Human> mergedHumans = new List<Human>();
//        foreach (KeyValuePair<int, Human> h1 in _humans)
//        {
//            foreach (KeyValuePair<int, Human> h2 in _humans)
//            {
//                if (h1.Value.ID != h2.Value.ID && !mergedHumans.Contains(h2.Value))
//                {
//                    if (CalcHorizontalDistance(h1.Value.Position, h2.Value.Position) <
//                        TrackerProperties.Instance.MergeDistance)
//                    {
//                        //************* NEW DM STUFF *******************
//                        // if a human has body from a sensor, the other cannot have a body from the same sensor
//                        bool commonSensor = false;
//                        foreach (SensorBody h1body in h1.Value.bodies)
//                        {
//                            foreach (SensorBody h2body in h2.Value.bodies)
//                            {
//                                if (h1body.sensorID == h2body.sensorID)
//                                {
//                                    commonSensor = true;
//                                    break;
//                                }
//                            }
//                            if (commonSensor)
//                                break;
//                        }
//                        if (commonSensor)
//                        {
//                            continue;
//                        }
//                        //************* END DM STUFF *******************

//                        var position =
//                            (h1.Value.Position * (float) h1.Value.bodies.Count +
//                             h2.Value.Position * (float) h2.Value.bodies.Count) /
//                            (float) (h1.Value.bodies.Count + h2.Value.bodies.Count);

//                        if (CalcHorizontalDistance(h1.Value.Position, h2.Value.Position) <
//                            TrackerProperties.Instance.MergeDistance)
//                        {
//                            Vector3 position =
//                                (h1.Value.Position * (float) h1.Value.bodies.Count +
//                                 h2.Value.Position * (float) h2.Value.bodies.Count) /
//                                (float) (h1.Value.bodies.Count + h2.Value.bodies.Count);


//                            if (h1.Value.ID < h2.Value.ID)
//                            {
//                                h1.Value.Position = position;

//                                foreach (var b in h2.Value.bodies)
//                                    //foreach (SensorBody b in h2.Value.bodies)
//                                {
//                                    h1.Value.bodies.Add(b);
//                                }
//                                mergedHumans.Add(h2.Value);

//                            }
//                            else
//                            {
//                                h2.Value.Position = position;
//                                foreach (var b in h1.Value.bodies)


//                                    //} else
//                                    //                  {
//                                    //	h2.Value.Position = position;
//                                    //	foreach (SensorBody b in h1.Value.bodies)
//                                {
//                                    h2.Value.bodies.Add(b);
//                                }
//                                mergedHumans.Add(h1.Value);
//                            }
//                            break;
//                        }
//                    }
//                }
//            }
//            foreach (var h in mergedHumans)
//                //foreach (Human h in mergedHumans)
//            {
//                _humansToKill.Add(h);
//                _humans.Remove(h.ID);
//            }
//        }
//    }
*/
