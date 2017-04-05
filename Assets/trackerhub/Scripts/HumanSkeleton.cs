using UnityEngine;
using System.Collections;
using System;
using Windows.Kinect;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class HumanSkeleton : MonoBehaviour
{
    public Tracker tracker;

    private GameObject _floorForwardGameObject;
    // private GameObject forwardGO;

    private GameObject _head;
    private GameObject _leftShoulder;
    private GameObject _rightShoulder;
    private GameObject _leftElbow;
    private GameObject _rightElbow;
    private GameObject _leftHand;
    private GameObject _rightHand;
    private GameObject _spineMid;
    private GameObject _leftHip;
    private GameObject _rightHip;
    private GameObject _leftKnee;
    private GameObject _rightKnee;
    private GameObject _leftFoot;
    private GameObject _rightFoot;

    private AdaptiveDoubleExponentialFilterVector3 _headKalman;
    private AdaptiveDoubleExponentialFilterVector3 _neckKalman;
    private AdaptiveDoubleExponentialFilterVector3 _spineShoulderKalman;
    private AdaptiveDoubleExponentialFilterVector3 _spineMidKalman;
    private AdaptiveDoubleExponentialFilterVector3 _spineBaseKalman;

    private AdaptiveDoubleExponentialFilterVector3 _leftShoulderKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftElbowKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftWristKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftHandKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftThumbKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftHandTipKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftHipKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftKneeKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftAnkleKalman;
    private AdaptiveDoubleExponentialFilterVector3 _leftFootKalman;

    private AdaptiveDoubleExponentialFilterVector3 _rightShoulderKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightElbowKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightWristKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightHandKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightThumbKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightHandTipKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightHipKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightKneeKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightAnkleKalman;
    private AdaptiveDoubleExponentialFilterVector3 _rightFootKalman;

    private Vector3 _lastForward;

    public int ID;

    private bool _canSend = false;
    private bool _mirror  = false;
    
    private string _handLeftState;
    private string _handRightState;

    //private string handStateLeft = HandState.Unknown.ToString();
    //private string handStateRight = HandState.Unknown.ToString();
    
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start ()
	{
		var humanCollider = gameObject.AddComponent<CapsuleCollider> ();
		humanCollider.radius = 0.25f;
		humanCollider.height = 1.75f;

		_head          = CreateSphere ("head", 0.3f);
		_leftShoulder  = CreateSphere ("leftShoulder");
		_rightShoulder = CreateSphere ("rightShoulder");
		_leftElbow     = CreateSphere ("leftElbow");
		_rightElbow    = CreateSphere ("rightElbow");
		_leftHand      = CreateSphere ("leftHand");
		_rightHand     = CreateSphere ("rightHand");
		_spineMid      = CreateSphere ("spineMid", 0.2f);
		_leftHip       = CreateSphere ("leftHip");
		_rightHip      = CreateSphere ("rightHip");
		_leftKnee      = CreateSphere ("leftKnee");
		_rightKnee     = CreateSphere ("rightKnee");
		_leftFoot      = CreateSphere ("leftFoot");
		_rightFoot     = CreateSphere ("rightFoot");

		_headKalman          = new AdaptiveDoubleExponentialFilterVector3 ();
		_neckKalman          = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineMidKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineBaseKalman     = new AdaptiveDoubleExponentialFilterVector3 ();

		_leftShoulderKalman  = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftElbowKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftWristKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHandKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftThumbKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHandTipKalman   = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHipKalman       = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftKneeKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftAnkleKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftFootKalman      = new AdaptiveDoubleExponentialFilterVector3 ();

		_rightShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightElbowKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightWristKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHandKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightThumbKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHandTipKalman  = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHipKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightKneeKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightAnkleKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightFootKalman     = new AdaptiveDoubleExponentialFilterVector3 ();

        _canSend = true;
        _lastForward = Vector3.zero;

		// forwardGO = new GameObject();
		// forwardGO.name = "ForwardOld";
		// forwardGO.transform.parent = transform;
		// GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		// cylinder.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
		// cylinder.transform.position += new Vector3(0, 0, 0.25f);
		// cylinder.transform.up = Vector3.forward;
		// cylinder.transform.parent = forwardGO.transform;

		_floorForwardGameObject = (GameObject)Instantiate (Resources.Load ("Prefabs/FloorForwardPlane"));
		_floorForwardGameObject.name = "Forward";
		_floorForwardGameObject.tag  = "nocolor";
		_floorForwardGameObject.transform.parent = transform;
        
        _handLeftState  = "Null";
        _handRightState = "Null";
    }

	private GameObject CreateSphere (string sphereName, float scale = 0.1f)
	{
		GameObject gameObjectSphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		gameObjectSphere.GetComponent<SphereCollider> ().enabled = false;
		gameObjectSphere.transform.parent = transform;
		gameObjectSphere.transform.localScale = new Vector3 (scale, scale, scale);
		gameObjectSphere.name = sphereName;
		return gameObjectSphere;
	}

	private Vector3 CalcUnfilteredForward ()
	{
		var spineRight = (_mirror ? tracker.GetJointPosition (ID, JointType.ShoulderLeft, Vector3.zero) : tracker.GetJointPosition (ID, JointType.ShoulderRight, Vector3.zero)) - tracker.GetJointPosition (ID, JointType.SpineShoulder, Vector3.zero);
		var spineUp = tracker.GetJointPosition (ID, JointType.SpineShoulder, Vector3.zero) - tracker.GetJointPosition (ID, JointType.SpineMid, Vector3.zero);
        
		return Vector3.Cross (spineRight, spineUp);
	}

	private Vector3 CalcForward ()
	{
		var spineRight = _rightShoulderKalman.Value - _spineShoulderKalman.Value;
		var spineUp = _spineShoulderKalman.Value - _spineMidKalman.Value;

		return Vector3.Cross (spineRight, spineUp);
	}

	public void UpdateSkeleton ()
	{
		if (tracker.HumanHasBodies (ID))
        {
            _handRightState = tracker.GetHandState(ID, Side.Right);
            _handLeftState =  tracker.GetHandState(ID, Side.Left);
            
            // Update Forward (mirror or not to mirror?)

            var forward = CalcUnfilteredForward ();

			if (_lastForward != Vector3.zero)
            {
				var projectedForward = new Vector3 (forward.x, 0, forward.z);
				var projectedLastForward = new Vector3 (_lastForward.x, 0, _lastForward.z);
                //if (Vector3.Angle(projectedLastForward, -projectedForward) < Vector3.Angle(projectedLastForward, projectedForward)) // the same as above

                if (Vector3.Angle (projectedLastForward, projectedForward) > 90)
                {
                    _mirror = !_mirror;
					forward = CalcUnfilteredForward ();
					projectedForward = new Vector3 (forward.x, 0, forward.z);
				}

				// Front for sure?
                var elbowHand1 = tracker.GetJointPosition (ID, JointType.HandRight, _rightHandKalman.Value) - tracker.GetJointPosition (ID, JointType.ElbowRight, _rightElbowKalman.Value);
				var elbowHand2 = tracker.GetJointPosition (ID, JointType.HandLeft, _leftHandKalman.Value) - tracker.GetJointPosition (ID, JointType.ElbowLeft, _leftElbowKalman.Value);
                
                if (Vector3.Angle (elbowHand1, -projectedForward) < 30 || Vector3.Angle (elbowHand2, -projectedForward) < 30) {
					_mirror = !_mirror;
					forward = CalcUnfilteredForward ();
				}
			}

			_lastForward = forward;
            
            //handStateLeft = tracker.GetHandState(ID, BodyPropertiesTypes.HandLeftState);
            //handStateRight = tracker.GetHandState(ID, BodyPropertiesTypes.HandRightState);
            
            // Update Joints
            try
            {
				_headKalman.Value          = tracker.GetJointPosition (ID, JointType.Head,          _headKalman.Value);
				_neckKalman.Value          = tracker.GetJointPosition (ID, JointType.Neck,          _neckKalman.Value);
				_spineShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.SpineShoulder, _spineShoulderKalman.Value);
				_spineMidKalman.Value      = tracker.GetJointPosition (ID, JointType.SpineMid,      _spineMidKalman.Value);
				_spineBaseKalman.Value     = tracker.GetJointPosition (ID, JointType.SpineBase,     _spineBaseKalman.Value);
                
				if (_mirror)
                {
					_rightShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderLeft, _rightShoulderKalman.Value);
					_rightElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowLeft,    _rightElbowKalman.Value);
					_rightWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristLeft,    _rightWristKalman.Value);
					_rightHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandLeft,     _rightHandKalman.Value);
					_rightThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbLeft,    _rightThumbKalman.Value);
					_rightHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipLeft,  _rightHandTipKalman.Value);
					_rightHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipLeft,      _rightHipKalman.Value);
					_rightKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeLeft,     _rightKneeKalman.Value);
					_rightAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleLeft,    _rightAnkleKalman.Value);
					_rightFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootLeft,     _rightFootKalman.Value);

					_leftShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderRight, _leftShoulderKalman.Value);
					_leftElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowRight,    _leftElbowKalman.Value);
					_leftWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristRight,    _leftWristKalman.Value);
					_leftHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandRight,     _leftHandKalman.Value);
					_leftThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbRight,    _leftThumbKalman.Value);
					_leftHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipRight,  _leftHandTipKalman.Value);
					_leftHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipRight,      _leftHipKalman.Value);
					_leftKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeRight,     _leftKneeKalman.Value);
					_leftAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleRight,    _leftAnkleKalman.Value);
					_leftFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootRight,     _leftFootKalman.Value);
				}
                else
                {
					_leftShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderLeft, _leftShoulderKalman.Value);
					_leftElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowLeft,    _leftElbowKalman.Value);
					_leftWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristLeft,    _leftWristKalman.Value);
					_leftHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandLeft,     _leftHandKalman.Value);
					_leftThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbLeft,    _leftThumbKalman.Value);
					_leftHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipLeft,  _leftHandTipKalman.Value);
					_leftHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipLeft,      _leftHipKalman.Value);
					_leftKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeLeft,     _leftKneeKalman.Value);
					_leftAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleLeft,    _leftAnkleKalman.Value);
					_leftFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootLeft,     _leftFootKalman.Value);

					_rightShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderRight, _rightShoulderKalman.Value);
					_rightElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowRight,    _rightElbowKalman.Value);
					_rightWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristRight,    _rightWristKalman.Value);
					_rightHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandRight,     _rightHandKalman.Value);
					_rightThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbRight,    _rightThumbKalman.Value);
					_rightHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipRight,  _rightHandTipKalman.Value);
					_rightHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipRight,      _rightHipKalman.Value);
					_rightKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeRight,     _rightKneeKalman.Value);
					_rightAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleRight,    _rightAnkleKalman.Value);
					_rightFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootRight,     _rightFootKalman.Value);
				}

				_head.transform.position          = _headKalman.Value;
				_leftShoulder.transform.position  = _leftShoulderKalman.Value;
				_rightShoulder.transform.position = _rightShoulderKalman.Value;
				_leftElbow.transform.position     = _leftElbowKalman.Value;
				_rightElbow.transform.position    = _rightElbowKalman.Value;
				_leftHand.transform.position      = _leftHandKalman.Value;
				_rightHand.transform.position     = _rightHandKalman.Value;
				_spineMid.transform.position      = _spineMidKalman.Value;
				_leftHip.transform.position       = _leftHipKalman.Value;
				_rightHip.transform.position      = _rightHipKalman.Value;
				_leftKnee.transform.position      = _leftKneeKalman.Value;
				_rightKnee.transform.position     = _rightKneeKalman.Value;
				_leftFoot.transform.position      = _leftFootKalman.Value;
				_rightFoot.transform.position     = _rightFootKalman.Value;

				// update forward

				var fw = CalcForward ();
				var pos = _spineMid.transform.position;

				// forwardGO.transform.forward  = fw;
				// forwardGO.transform.position = pos;

				_floorForwardGameObject.transform.forward  = new Vector3 ( fw.x,      0,  fw.z);
				_floorForwardGameObject.transform.position = new Vector3 (pos.x, 0.001f, pos.z);
				_floorForwardGameObject.transform.parent = transform;
			}
            catch (Exception e)
            {
				Debug.Log (e.Message + "\n" + e.StackTrace);
			}
		}
	}

	internal string GetPdu()
	{
		if (_canSend)
        {
			var pdu = BodyPropertiesTypes.UID.ToString () + MessageSeparators.SET + ID + MessageSeparators.L2;

			pdu += BodyPropertiesTypes.HandLeftState.ToString ()      + MessageSeparators.SET + _handLeftState  + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandLeftConfidence.ToString()  + MessageSeparators.SET + "Null"         + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightState.ToString ()     + MessageSeparators.SET + _handRightState + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightConfidence.ToString() + MessageSeparators.SET + "Null"         + MessageSeparators.L2;
            
			pdu += "head"          + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_headKalman.Value)          + MessageSeparators.L2;
			pdu += "neck"          + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_neckKalman.Value)          + MessageSeparators.L2;

			pdu += "spineShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_spineShoulderKalman.Value) + MessageSeparators.L2;
			pdu += "spineMid"      + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_spineMidKalman.Value)      + MessageSeparators.L2;
			pdu += "spineBase"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_spineBaseKalman.Value)     + MessageSeparators.L2;

            pdu += "leftShoulder"  + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftShoulderKalman.Value)  + MessageSeparators.L2;
			pdu += "leftElbow"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftElbowKalman.Value)     + MessageSeparators.L2;
			pdu += "leftWrist"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftWristKalman.Value)     + MessageSeparators.L2;
			pdu += "leftHand"      + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftHandKalman.Value)      + MessageSeparators.L2;
			pdu += "leftThumb"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftThumbKalman.Value)     + MessageSeparators.L2;
			pdu += "leftHandTip"   + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftHandTipKalman.Value)   + MessageSeparators.L2;
			pdu += "leftHip"       + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftHipKalman.Value)       + MessageSeparators.L2;
			pdu += "leftKnee"      + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftKneeKalman.Value)      + MessageSeparators.L2;
			pdu += "leftAnkle"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftAnkleKalman.Value)     + MessageSeparators.L2;
			pdu += "leftFoot"      + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_leftFootKalman.Value)      + MessageSeparators.L2;

            pdu += "rightShoulder" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightShoulderKalman.Value) + MessageSeparators.L2;
			pdu += "rightElbow"    + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightElbowKalman.Value)    + MessageSeparators.L2;
			pdu += "rightWrist"    + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightWristKalman.Value)    + MessageSeparators.L2;
			pdu += "rightHand"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightHandKalman.Value)     + MessageSeparators.L2;
			pdu += "rightThumb"    + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightThumbKalman.Value)    + MessageSeparators.L2;
			pdu += "rightHandTip"  + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightHandTipKalman.Value)  + MessageSeparators.L2;
			pdu += "rightHip"      + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightHipKalman.Value)      + MessageSeparators.L2;
			pdu += "rightKnee"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightKneeKalman.Value)     + MessageSeparators.L2;
			pdu += "rightAnkle"    + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightAnkleKalman.Value)    + MessageSeparators.L2;
			pdu += "rightFoot"     + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (_rightFootKalman.Value);

			return pdu;
		}
        else throw new Exception ("Human not initalized.");
	}

	public Vector3 GetMidSpine()
	{
		return _spineMidKalman.Value;
	}

    public Vector3 GetHead ()
	{
		return _headKalman.Value;
	}

    public Vector3 GetKnee(Side side)
    {
        switch (side)
        {
            case Side.Right:
                return GetRightKnee();
                
            case Side.Left:
                return GetLeftKnee();
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }
    }

    private Vector3 GetRightKnee()
    {
        return _rightKneeKalman == null ? tracker.GetJointPosition(ID, JointType.KneeRight) : _rightKneeKalman.Value;
    }

    private Vector3 GetLeftKnee()
    {
        return _leftKneeKalman == null ? tracker.GetJointPosition(ID, JointType.KneeLeft) : _leftKneeKalman.Value;
    }

}
///////////////////////////////////////////////////


/* 
 
<<<<<<< HEAD
			// Update Joints
			try
            {
				headKalman.Value          = tracker.GetJointPosition (ID, JointType.Head,          headKalman.Value);
				neckKalman.Value          = tracker.GetJointPosition (ID, JointType.Neck,          neckKalman.Value);
				spineShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.SpineShoulder, spineShoulderKalman.Value);
				spineMidKalman.Value      = tracker.GetJointPosition (ID, JointType.SpineMid,      spineMidKalman.Value);
				spineBaseKalman.Value     = tracker.GetJointPosition (ID, JointType.SpineBase,     spineBaseKalman.Value);

				if (_mirror)
                {
					rightShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderLeft,   rightShoulderKalman.Value);
					rightElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowLeft,      rightElbowKalman.Value);
					rightWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristLeft,      rightWristKalman.Value);
					rightHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandLeft,       rightHandKalman.Value);
					rightThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbLeft,      rightThumbKalman.Value);
					rightHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipLeft,    rightHandTipKalman.Value);
					rightHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipLeft,        rightHipKalman.Value);
					rightKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeLeft,       rightKneeKalman.Value);
					rightAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleLeft,      rightAnkleKalman.Value);
					rightFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootLeft,       rightFootKalman.Value);

					leftShoulderKalman.Value  = tracker.GetJointPosition (ID, JointType.ShoulderRight,  leftShoulderKalman.Value);
					leftElbowKalman.Value     = tracker.GetJointPosition (ID, JointType.ElbowRight,     leftElbowKalman.Value);
					leftWristKalman.Value     = tracker.GetJointPosition (ID, JointType.WristRight,     leftWristKalman.Value);
					leftHandKalman.Value      = tracker.GetJointPosition (ID, JointType.HandRight,      leftHandKalman.Value);
					leftThumbKalman.Value     = tracker.GetJointPosition (ID, JointType.ThumbRight,     leftThumbKalman.Value);
					leftHandTipKalman.Value   = tracker.GetJointPosition (ID, JointType.HandTipRight,   leftHandTipKalman.Value);
					leftHipKalman.Value       = tracker.GetJointPosition (ID, JointType.HipRight,       leftHipKalman.Value);
					leftKneeKalman.Value      = tracker.GetJointPosition (ID, JointType.KneeRight,      leftKneeKalman.Value);
					leftAnkleKalman.Value     = tracker.GetJointPosition (ID, JointType.AnkleRight,     leftAnkleKalman.Value);
					leftFootKalman.Value      = tracker.GetJointPosition (ID, JointType.FootRight,      leftFootKalman.Value);
			    }
                else
                {
					leftShoulderKalman.Value  = tracker.GetJointPosition (ID, JointType.ShoulderLeft,  leftShoulderKalman.Value);
					leftElbowKalman.Value     = tracker.GetJointPosition (ID, JointType.ElbowLeft,     leftElbowKalman.Value);
					leftWristKalman.Value     = tracker.GetJointPosition (ID, JointType.WristLeft,     leftWristKalman.Value);
					leftHandKalman.Value      = tracker.GetJointPosition (ID, JointType.HandLeft,      leftHandKalman.Value);
					leftThumbKalman.Value     = tracker.GetJointPosition (ID, JointType.ThumbLeft,     leftThumbKalman.Value);
					leftHandTipKalman.Value   = tracker.GetJointPosition (ID, JointType.HandTipLeft,   leftHandTipKalman.Value);
					leftHipKalman.Value       = tracker.GetJointPosition (ID, JointType.HipLeft,       leftHipKalman.Value);
					leftKneeKalman.Value      = tracker.GetJointPosition (ID, JointType.KneeLeft,      leftKneeKalman.Value);
					leftAnkleKalman.Value     = tracker.GetJointPosition (ID, JointType.AnkleLeft,     leftAnkleKalman.Value);
					leftFootKalman.Value      = tracker.GetJointPosition (ID, JointType.FootLeft,      leftFootKalman.Value);

					rightShoulderKalman.Value = tracker.GetJointPosition (ID, JointType.ShoulderRight, rightShoulderKalman.Value);
					rightElbowKalman.Value    = tracker.GetJointPosition (ID, JointType.ElbowRight,    rightElbowKalman.Value);
					rightWristKalman.Value    = tracker.GetJointPosition (ID, JointType.WristRight,    rightWristKalman.Value);
					rightHandKalman.Value     = tracker.GetJointPosition (ID, JointType.HandRight,     rightHandKalman.Value);
					rightThumbKalman.Value    = tracker.GetJointPosition (ID, JointType.ThumbRight,    rightThumbKalman.Value);
					rightHandTipKalman.Value  = tracker.GetJointPosition (ID, JointType.HandTipRight,  rightHandTipKalman.Value);
					rightHipKalman.Value      = tracker.GetJointPosition (ID, JointType.HipRight,      rightHipKalman.Value);
					rightKneeKalman.Value     = tracker.GetJointPosition (ID, JointType.KneeRight,     rightKneeKalman.Value);
					rightAnkleKalman.Value    = tracker.GetJointPosition (ID, JointType.AnkleRight,    rightAnkleKalman.Value);
					rightFootKalman.Value     = tracker.GetJointPosition (ID, JointType.FootRight,     rightFootKalman.Value);
=======
     >>>>>>> 38b20e139c2c9db54923b30780c0ff1326ff44e1
     

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    =======
//<<<<<<< HEAD
		if (_canSend) {
			string pdu = BodyPropertiesTypes.UID.ToString () + MessageSeparators.SET + ID + MessageSeparators.L2;

			pdu += BodyPropertiesTypes.HandLeftState.ToString () + MessageSeparators.SET + handStateLeft + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandLeftConfidence.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightState.ToString () + MessageSeparators.SET + handStateRight + MessageSeparators.L2;
			pdu += BodyPropertiesTypes.HandRightConfidence.ToString () + MessageSeparators.SET + "Null" + MessageSeparators.L2;

			pdu += "head" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (headKalman.Value) + MessageSeparators.L2;
			pdu += "neck" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC (neckKalman.Value) + MessageSeparators.L2;
>>>>>>> 38b20e139c2c9db54923b30780c0ff1326ff44e1
     */
