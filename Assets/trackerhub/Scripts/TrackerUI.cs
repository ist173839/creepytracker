using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
// ReSharper disable once ArrangeTypeModifiers
enum MenuAction
{
	Settings,
	Sensors,
	Humans,
	Devices,
	Clouds,
	NetworkSettings,
	About,
	None
}

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class TrackerUI : MonoBehaviour
{
	public Texture checkTexture;
	public Texture uncheckTexture;

	public Texture sensorTextureOn;
	public Texture sensorTextureOff;

	public Texture settingsTextureOn;
	public Texture settingsTextureOff;

	public Texture networkTextureOn;
	public Texture networkTextureOff;

	public Texture AboutOn;
	public Texture AboutOff;

	public Texture CloudTextureOn;
	public Texture CloudTextureOff;

	public Texture NextTexture;

    // ReSharper disable once MemberCanBePrivate.Global
    [Range (20, 100)] public int IconSize;

	private MenuAction _menuAction;

	private Tracker _userTracker;
    
	private GUIStyle _titleStyle;

	private int _currentCloudSensor;
    private int _packetsPerSec;

    public float _rotStep = 2f;
    public float _transStep = 0.02f;
    
    private string _newUnicastAddress;
	private string _newUnicastPort;

	private bool _continuous;
    private bool _hideHumans;


    ////////////////////////////////////////////////////////////////

    private HandleCenter _localHandleCenter;

    public bool ShowIndicator { get; set; }
    public bool UseOptiTrack  { get; set; }
    public bool SetUpCenter   { get; set; }
    public bool UseRecord     { get; set; }
    public bool Force         { get; set; }
    public bool Send          { get; set; }

    ////////////////////////////////////////////////////////////////

    // public bool UseSaveFile  { get; set; }
    // public bool UseSaveFile  { get; set; }
    
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start ()
	{
		_userTracker = gameObject.GetComponent<Tracker> ();

        _localHandleCenter = gameObject.GetComponent<HandleCenter> ();

        _menuAction = MenuAction.None;
		_currentCloudSensor = 0;
	    _titleStyle = new GUIStyle
	    {
	        fontStyle = FontStyle.Bold,
	        normal    =
	        {
	            textColor = Color.white
	        }
	    };
	    _continuous = false;
        _hideHumans = false;

		_newUnicastAddress = "";
		_newUnicastPort = "";

		_packetsPerSec = 1000 / TrackerProperties.Instance.SendInterval;

	    IconSize = 60;

        UseRecord     = false;
	    ShowIndicator = false;

	    Force = _localHandleCenter.Force;
	    Send  = _localHandleCenter.Send;
        UseOptiTrack = _localHandleCenter.UseOpti;

	    // UseOptiTrack = false;
	    // SetUpCenter = false;
	}

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
	void Update ()
	{
		if (_userTracker.CalibrationStatus == CalibrationProcess.CalcNormal)
			_userTracker.CalibrationStep3 ();
	}

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once ArrangeTypeMemberModifiers
	void OnGUI ()
	{
		var top = 5;
		var left = 20;

		DisplayMenuButton (MenuAction.Sensors, sensorTextureOn, sensorTextureOff, new Rect (left, top, IconSize, IconSize));
		GUI.Label (new Rect (left + IconSize, top + IconSize - 20, 10, 25), "" + _userTracker.Sensors.Count);
		left += IconSize + IconSize / 2;
        
		//displayMenuButton(MenuAction.Devices, deviceTex_on, deviceTex_off, new Rect(left, top, iconSize, iconSize));
		//left += iconSize + iconSize / 2;
		DisplayMenuButton (MenuAction.Settings, settingsTextureOn, settingsTextureOff, new Rect (left, top, IconSize, IconSize));

		left += IconSize + IconSize / 2;

		DisplayMenuButton (MenuAction.Clouds, CloudTextureOn, CloudTextureOff, new Rect (left, top, IconSize, IconSize));

		left = Screen.width - IconSize - 10;
		DisplayMenuButton (MenuAction.NetworkSettings, networkTextureOn, networkTextureOff, new Rect (left, top, IconSize, IconSize));

		if (_menuAction == MenuAction.Sensors) {
			top = IconSize + IconSize / 2;
			left = 20;

			GUI.Box (new Rect (left - 10, top - 10, 200, _userTracker.Sensors.Count == 0 ? 45 : (35 * _userTracker.Sensors.Count + 35 + 5)), "");


			if (_userTracker.Sensors.Count > 0)
            {
				GUI.Label (new Rect (left, top, 200, 25), "Sensors:", _titleStyle);
				top += 35;

				foreach (var sid in _userTracker.Sensors.Keys)
                {
					if (GUI.Button (new Rect (left, top, 20, 20), _userTracker.Sensors [sid].Active ? checkTexture : uncheckTexture, GUIStyle.none))
                    {
						_userTracker.Sensors [sid].Active = !_userTracker.Sensors [sid].Active;
					}
					GUI.Label (new Rect (left + 40, top, 100, 25), sid);

					top += 35;
				}
			}
            else
            {
				GUI.Label (new Rect (left, top, 1000, 40), "No connected sensors.");
			}
		}

		if (_menuAction == MenuAction.Settings)
        {
			top = IconSize + IconSize / 2;
			left = IconSize + 50;

			GUI.Box (new Rect (left, top - 10, 200, 140), "");
			left += 10;

			GUI.Label (new Rect (left, top, 500, 35), "Calibration: ", _titleStyle);
			top += 40;

			switch (_userTracker.CalibrationStatus)
			{
			    case CalibrationProcess.FindCenter:
			        if (GUI.Button (new Rect (left, top, 150, 35), "(1/2) Find Center"))
			        {
			            if (_userTracker.CalibrationStep1 ())
			            {
			                _userTracker.CalibrationStatus = CalibrationProcess.FindForward;
			            }
			        }
			        break;
			    case CalibrationProcess.FindForward:
			        if (GUI.Button (new Rect (left, top, 150, 35), "(2/2) Find Forward"))
			        {
			            _userTracker.CalibrationStatus = CalibrationProcess.FindCenter;
			            _userTracker.CalibrationStep2 ();
			            _menuAction = MenuAction.None;
			        }
			        break;
			    case CalibrationProcess.GetPlane:
			        if (GUI.Button (new Rect (left, top, 150, 35), "(3/4) Get Points"))
			        {
			            _userTracker.CalibrationStatus = CalibrationProcess.CalcNormal;
			        }
			        break;
			    case CalibrationProcess.CalcNormal:
			        if (GUI.Button (new Rect (left, top, 150, 35), "(4/4) Apply Calibration"))
			        {
			            _userTracker.CalibrationStatus = CalibrationProcess.FindCenter;
			            _userTracker.CalibrationStep4 ();
			            _menuAction = MenuAction.None;
			        }
			        break;
			}

			top += 60;
			if (GUI.Button (new Rect (left, top, 20, 20), AdaptiveDoubleExponentialFilterFloat.filtering ? checkTexture : uncheckTexture, GUIStyle.none)) {
				AdaptiveDoubleExponentialFilterFloat.filtering = !AdaptiveDoubleExponentialFilterFloat.filtering;
			}
			GUI.Label (new Rect (left + 40, top, 100, 25), "Smooth points");
		}

		if (_menuAction == MenuAction.Clouds) {
			top = IconSize + IconSize / 2;
			left = 20 + 3 * IconSize;
			
			GUI.Box (new Rect (left - 10, top - 10, 225, _userTracker.Sensors.Count == 0 ? 45 : (30 * 4 + 80 + 5)), "");
			
			var t = Time.deltaTime;
			if (_userTracker.Sensors.Count > 0) {

				if (GUI.Button (new Rect (left, top, 60, 20), "Request")) {
					_userTracker.BroadCastCloudRequests(_continuous);
				}
				_continuous = GUI.Toggle(new Rect(left,top+20,90,20),_continuous,"Continuous");

                var oldh = _hideHumans;
                _hideHumans = GUI.Toggle(new Rect(left + 90, top + 20, 90, 20), _hideHumans, "Hide humans");
                if(oldh != _hideHumans)
                {
                    _userTracker.SetHumansVisibility(_hideHumans);
                }
                if (GUI.Button (new Rect (left + 70, top, 60, 20), "Hide")) {
					_userTracker.HideAllClouds ();

				}
				if (GUI.Button (new Rect (left + 140, top, 60, 20), "Save")) {
					_userTracker.Save ();
				}
				top += 45;

				List<string> keyList = new List<string> (_userTracker.Sensors.Keys);
				GUI.Label (new Rect (left, top, 1000, 40), keyList [_currentCloudSensor]);
				if (GUI.Button (new Rect (left + 155, top + 2, 25, 25), NextTexture)) {
					_currentCloudSensor = (_currentCloudSensor + 1) % _userTracker.Sensors.Count;
				}
				var sid = keyList [_currentCloudSensor];
				var s = _userTracker.Sensors [sid].SensorGameObject;
				var rotation = s.transform.rotation.eulerAngles;
				var position = s.transform.position;
				var rx = rotation.x.ToString ();
				var ry = rotation.y.ToString ();
				var rz = rotation.z.ToString ();
				var px = position.x.ToString ();
				var py = position.y.ToString ();
				var pz = position.z.ToString ();

				top += 30;
				GUI.Label (new Rect (left + 2, top, 100, 40), "Rotation");
				GUI.Label (new Rect (left + 112, top, 100, 40), "Translation");
				top += 30;
				GUI.Label (new Rect (left + 2, top, 100, 40), "x:");
				GUI.TextField (new Rect (left + 37, top + 2, 40, 20), rx);
				GUI.Label (new Rect (left + 112, top, 100, 40), "x:");
				GUI.TextField (new Rect (left + 147, top + 2, 40, 20), px);
				
				if (GUI.RepeatButton (new Rect (left + 15, top + 2, 20, 20), "<"))
					rotation.x = rotation.x - _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 79, top + 2, 20, 20), ">"))
					rotation.x = rotation.x + _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 125, top + 2, 20, 20), "<"))
					position.x = position.x - _transStep * t;
				if (GUI.RepeatButton (new Rect (left + 189, top + 2, 20, 20), ">"))
					position.x = position.x + _transStep * t;
				top += 30;
				GUI.Label (new Rect (left + 2, top, 100, 40), "y:");
				GUI.TextField (new Rect (left + 37, top + 2, 40, 20), ry);
				GUI.Label (new Rect (left + 112, top, 100, 40), "y:");
				GUI.TextField (new Rect (left + 147, top + 2, 40, 20), py);
				
				if (GUI.RepeatButton (new Rect (left + 15, top + 2, 20, 20), "<"))
					rotation.y = rotation.y - _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 79, top + 2, 20, 20), ">"))
					rotation.y = rotation.y + _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 125, top + 2, 20, 20), "<"))
					position.y = position.y - _transStep * t;
				if (GUI.RepeatButton (new Rect (left + 189, top + 2, 20, 20), ">"))
					position.y = position.y + _transStep * t;


				top += 30;
				GUI.Label (new Rect (left + 2, top, 100, 40), "z:");
				GUI.TextField (new Rect (left + 37, top + 2, 40, 20), rz);
				GUI.Label (new Rect (left + 112, top, 100, 40), "z:");
				GUI.TextField (new Rect (left + 147, top + 2, 40, 20), pz);
				
				if (GUI.RepeatButton (new Rect (left + 15, top + 2, 20, 20), "<"))
					rotation.z = rotation.z - _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 79, top + 2, 20, 20), ">"))
					rotation.z = rotation.z + _rotStep * t;
				if (GUI.RepeatButton (new Rect (left + 125, top + 2, 20, 20), "<"))
					position.z = position.z - _transStep * t;
				if (GUI.RepeatButton (new Rect (left + 189, top + 2, 20, 20), ">"))
					position.z = position.z + _transStep * t;

				float x, y, z;
				float xr, yr, zr;
				if (position == s.transform.position && float.TryParse (px, out x) && float.TryParse (py, out y) && float.TryParse (pz, out z)) {
					s.transform.position = new Vector3 (x, y, z);
				} else {
					s.transform.position = position;
				}
				if (rotation == s.transform.rotation.eulerAngles && float.TryParse (rx, out xr) && float.TryParse (ry, out yr) && float.TryParse (rz, out zr)) {
					s.transform.rotation = Quaternion.Euler (xr, yr, zr);
				} else {
					s.transform.rotation = Quaternion.Euler (rotation);
				}

			} else {
				GUI.Label (new Rect (left, top, 1000, 40), "No connected sensors.");
			}
		}

		if (_menuAction == MenuAction.NetworkSettings)
        {
			top = IconSize + IconSize / 2;
			left = Screen.width - 250;

			GUI.Box (new Rect (left, top - 10, 240, 150), "");
			left += 10;

			GUI.Label (new Rect (left, top, 200, 25), "Broadcast Settings:", _titleStyle);
			left += 10;
			top += 35;

			GUI.Label (new Rect (left, top, 150, 25), "Sensors port:");
			left += 100;

			TrackerProperties.Instance.ListenPort = int.Parse (GUI.TextField (new Rect (left, top, 50, 20), "" + TrackerProperties.Instance.ListenPort));
			left += 55;
			if (GUI.Button (new Rect (left, top, 50, 25), "Reset")) {
				_userTracker.ResetListening ();
				_userTracker.Save ();

				DoNotify n = gameObject.GetComponent<DoNotify> ();
				n.notifySend (NotificationLevel.INFO, "Udp Listening", "Listening to port " + TrackerProperties.Instance.ListenPort, 2000);
			}
			top += 35;

			left = Screen.width - 250 + 20;
			GUI.Label (new Rect (left, top, 150, 25), "Broadcast port:");
			left += 100;

			TrackerProperties.Instance.BroadcastPort = int.Parse (GUI.TextField (new Rect (left, top, 50, 20), "" + TrackerProperties.Instance.BroadcastPort));
			left += 55;
			if (GUI.Button (new Rect (left, top, 50, 25), "Reset")) {
				_userTracker.ResetBroadcast ();
				_userTracker.Save ();

				var n = gameObject.GetComponent<DoNotify> ();
				n.notifySend (NotificationLevel.INFO, "Udp Broadcast", "Sending to port " + TrackerProperties.Instance.BroadcastPort, 2000);
			}

			top += 35;
			left = Screen.width - 250 + 20;
			GUI.Label (new Rect (left, top, 150, 25), "Packets / sec:");
			left += 100;
			_packetsPerSec = int.Parse (GUI.TextField (new Rect (left, top, 50, 20), "" + _packetsPerSec));
			left += 55;
			if (GUI.Button (new Rect (left, top, 50, 25), "Reset")) {
				TrackerProperties.Instance.SendInterval = 1000 / _packetsPerSec;
				_userTracker.Save ();

				DoNotify n = gameObject.GetComponent<DoNotify> ();
				n.notifySend (NotificationLevel.INFO, "Udp Broadcast", "Sending " + _packetsPerSec + " packets / sec", 2000);
			}
            top += 35;
            left = Screen.width - 250 + 20;

            UseRecord    = GUI.Toggle(new Rect(left, top, 100, 25), UseRecord, "Record");

            ShowIndicator = GUI.Toggle(new Rect(left + 120, top, 100, 25), ShowIndicator, "Show Indicators");
            _localHandleCenter.ShowIndicator = ShowIndicator;

            top += 30;

            if (GUI.Button(new Rect(left, top, 100, 25), "Set Center"))
            {
                _localHandleCenter.SetCenterOptiTrackButton();
            }

            if (GUI.Button(new Rect(left + 120, top, 100, 25), "Set Forward"))
            {
                _localHandleCenter.SetForwardPointOptiTrackButton();
            }
           
            top += 35;

            if (GUI.Button(new Rect(left, top, 100, 25), "Active File"))
            {

                _localHandleCenter.SetSaveFilesButton();
            }

            if (GUI.Button(new Rect(left + 120, top, 100, 25), "Reset"))
            {
                _localHandleCenter.Reset();
            }

            top += 35;

            Force = _localHandleCenter.Force;
            Force = GUI.Toggle(new Rect(left, top, 100, 25), Force, "Force Center");
            _localHandleCenter.Force = Force;

            Send = _localHandleCenter.Send;
            Send = GUI.Toggle(new Rect(left + 120, top, 100, 25), Send, "Send");
            _localHandleCenter.Send = Send;

            top += 35;
            UseOptiTrack = _localHandleCenter.UseOpti;
            UseOptiTrack = GUI.Toggle(new Rect(left, top, 100, 25), UseOptiTrack, "Use Opti");
            _localHandleCenter.UseOpti = UseOptiTrack;

            // Unicast Settings
            /*
            top += 60;
            left = Screen.width - 250;

            GUI.Box(new Rect(left, top - 10, 240, 85 + _userTracker.UnicastClients.Length * 30), "");
            left += 10;

            GUI.Label(new Rect(left, top, 200, 25), "Unicast Settings:", _titleStyle);
            left += 10;
            top += 35;

            int addressTextFieldSize = 110;
            newUnicastAddress = GUI.TextField(new Rect(left, top, addressTextFieldSize, 20), newUnicastAddress);
            GUI.Label(new Rect(left + addressTextFieldSize + 3, top, 10, 25), ":");
            newUnicastPort = GUI.TextField(new Rect(left + addressTextFieldSize + 10, top, 50, 20), newUnicastPort);
            left += addressTextFieldSize + 1 + 15 + 50;
            if (GUI.Button(new Rect(left, top, 40, 25), "Add"))
            {
                _userTracker.addUnicast(newUnicastAddress, newUnicastPort);
                newUnicastAddress = "";
                newUnicastPort = "";
            }

            top += 5;
            left = Screen.width - 250 + 20;
            foreach(string ip in _userTracker.UnicastClients)
            {
                top += 30;
                GUI.Label(new Rect(left, top, 140, 25), ip);
                if (GUI.Button(new Rect(left + 140 + 5, top, 60, 20), "Remove"))
                {
                    _userTracker.removeUnicast(ip);
                }
            }
            */

        }

        if (Input.GetMouseButtonDown (0))
        {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit))
            {
				if (hit.collider != null)
                {
					if (hit.collider.gameObject.name.Contains ("Human"))
                    {
						_userTracker.ShowHumanBodies = int.Parse (hit.collider.gameObject.name.Remove (0, "Human ".Length));
					}
                    else _userTracker.ShowHumanBodies = -1;
				}
                else _userTracker.ShowHumanBodies = -1;
			}
            else _userTracker.ShowHumanBodies = -1;
		}
	}

    // ReSharper disable once ArrangeTypeMemberModifiers
	void DisplayMenuButton (MenuAction button, Texture texon, Texture texoff, Rect rect)
	{
		if (GUI.Button (rect, _menuAction == button ? texon : texoff, GUIStyle.none))
		    _menuAction = _menuAction == button ? MenuAction.None : button;
	}
}
    /*   

        // SetUpCenter = GUI.Toggle(new Rect(left, top, 100, 25), SetUpCenter, "Use Center");
        Force = _localHandleCenter.Force;
        Send  = _localHandleCenter.Send;    

        // UseOptiTrack = GUI.Toggle(new Rect(left, top, 100, 25), UseOptiTrack, "Use Opti");
        // Debug.Log("UseRecord = " + UseRecord);   
    */
