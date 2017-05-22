using UnityEngine;
using System;
using Windows.Kinect;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class HumanSkeleton : MonoBehaviour
{
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

    private string _handStateLeft  = HandState.Unknown.ToString();
    private string _handStateRight = HandState.Unknown.ToString();
 
    private AdaptiveDoubleExponentialFilterVector3 _headFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _neckFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _spineShoulderFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _spineMidFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _spineBaseFiltered;

	private AdaptiveDoubleExponentialFilterVector3 _leftShoulderFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftElbowFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftWristFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftHandFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftThumbFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftHandTipFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftHipFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftKneeFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftAnkleFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _leftFootFiltered;
    private AdaptiveDoubleExponentialFilterVector3 _leftHandScreenSpaceFiltered;
    
    private AdaptiveDoubleExponentialFilterVector3 _rightShoulderFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightElbowFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightWristFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightHandFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightThumbFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightHandTipFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightHipFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightKneeFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightAnkleFiltered;
	private AdaptiveDoubleExponentialFilterVector3 _rightFootFiltered;
    private AdaptiveDoubleExponentialFilterVector3 _rightHandScreenSpaceFiltered;

    public Tracker tracker;
	public int ID;

	private bool canSend = false;

	private bool mirror = false;
	private Vector3 lastForward;

	//private GameObject forwardGO;
	private GameObject floorForwardGameObject;

    //// Versão usada no Master
    //private string handStateLeft  = HandState.Unknown.ToString();
    //private string handStateRight = HandState.Unknown.ToString();

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////        Dissertação - Mestrado em Engenharia Informática e de Computadores                                   //////////
    //////        Francisco Henriques Venda, nº 73839                                                                  //////////
    //////        Alterações apartir daqui                                                                             //////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////// Versão Alterada
    //private string _handLeftState;
    //private string _handRightState;


    void Awake ()
	{
		CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider> ();
		collider.radius = 0.25f;
		collider.height = 1.75f;

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

		_headFiltered          = new AdaptiveDoubleExponentialFilterVector3 ();
		_neckFiltered          = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineShoulderFiltered = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineMidFiltered      = new AdaptiveDoubleExponentialFilterVector3 ();
		_spineBaseFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();

		_leftShoulderFiltered = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftElbowFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftWristFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHandFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftThumbFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHandTipFiltered  = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftHipFiltered      = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftKneeFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftAnkleFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_leftFootFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();

        _leftHandScreenSpaceFiltered = new AdaptiveDoubleExponentialFilterVector3 ();

		_rightShoulderFiltered = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightElbowFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightWristFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHandFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightThumbFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHandTipFiltered  = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightHipFiltered      = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightKneeFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightAnkleFiltered    = new AdaptiveDoubleExponentialFilterVector3 ();
		_rightFootFiltered     = new AdaptiveDoubleExponentialFilterVector3 ();

        _rightHandScreenSpaceFiltered = new AdaptiveDoubleExponentialFilterVector3 ();

		canSend = true;
		lastForward = Vector3.zero;

		// forwardGO = new GameObject();
		// forwardGO.name = "ForwardOld";
		// forwardGO.transform.parent = transform;
		// GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		// cylinder.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
		// cylinder.transform.position += new Vector3(0, 0, 0.25f);
		// cylinder.transform.up = Vector3.forward;
		// cylinder.transform.parent = forwardGO.transform;

		floorForwardGameObject = (GameObject)Instantiate (Resources.Load ("Prefabs/FloorForwardPlane"));
		floorForwardGameObject.name = "Forward";
		floorForwardGameObject.tag = "nocolor";
		floorForwardGameObject.transform.parent = transform;
	}

	private GameObject CreateSphere(string sphereName, float scale = 0.1f)
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
		var spineRight = (mirror ? tracker.GetJointPosition (ID, JointType.ShoulderLeft, Vector3.zero) : tracker.GetJointPosition (ID, JointType.ShoulderRight, Vector3.zero)) - tracker.GetJointPosition (ID, JointType.SpineShoulder, Vector3.zero);
		var spineUp = tracker.GetJointPosition (ID, JointType.SpineShoulder, Vector3.zero) - tracker.GetJointPosition (ID, JointType.SpineMid, Vector3.zero);
        
		return Vector3.Cross (spineRight, spineUp);
	}

	private Vector3 CalcForward ()
	{
//<<<<<<< HEAD
//		var spineRight = _rightShoulderKalman.Value - _spineShoulderKalman.Value;
//		var spineUp = _spineShoulderKalman.Value - _spineMidKalman.Value;
//=======
		Vector3 spineRight = _rightShoulderFiltered.Value - _spineShoulderFiltered.Value;
		Vector3 spineUp = _spineShoulderFiltered.Value - _spineMidFiltered.Value;
		return Vector3.Cross (spineRight, spineUp);
	}

	public void UpdateSkeleton ()
	{
//<<<<<<< HEAD
		if (tracker.HumanHasBodies (ID))
        {
            //_handRightState = tracker.GetHandState(ID, Side.Right);
            //_handLeftState =  tracker.GetHandState(ID, Side.Left);
            
            // Update Forward (mirror or not to mirror?)

            var forward = CalcUnfilteredForward ();

			if (lastForward != Vector3.zero)
            {
				var projectedForward     = new Vector3 (     forward.x, 0,      forward.z);
				var projectedLastForward = new Vector3 (lastForward.x, 0, lastForward.z);
                //if (Vector3.Angle(projectedLastForward, -projectedForward) < Vector3.Angle(projectedLastForward, projectedForward)) // the same as above

                if (Vector3.Angle (projectedLastForward, projectedForward) > 90)
                {
                    mirror = !mirror;
					forward = CalcUnfilteredForward ();
//=======
		//if (tracker.humanHasBodies(ID))
  //      {
		//	// Update Forward (mirror or not to mirror?)
		//	Vector3 forward = calcUnfilteredForward ();

		//	if (lastForward != Vector3.zero)
  //          {
		//		Vector3 projectedForward = new Vector3 (forward.x, 0, forward.z);
		//		Vector3 projectedLastForward = new Vector3 (lastForward.x, 0, lastForward.z);

		//		if (Vector3.Angle (projectedLastForward, projectedForward) > 90) //if (Vector3.Angle(projectedLastForward, -projectedForward) < Vector3.Angle(projectedLastForward, projectedForward)) // the same as above
  //              {              
		//			mirror = !mirror;
		//			forward = calcUnfilteredForward ();
//>>>>>>> refs/remotes/mauriciosousa/master
					projectedForward = new Vector3 (forward.x, 0, forward.z);
				}

				// Front for sure?
//<<<<<<< HEAD
                var elbowHand1 = tracker.GetJointPosition (ID, JointType.HandRight, _rightHandFiltered.Value) - tracker.GetJointPosition (ID, JointType.ElbowRight, _rightElbowFiltered.Value);
				var elbowHand2 = tracker.GetJointPosition (ID, JointType.HandLeft, _leftHandFiltered.Value) - tracker.GetJointPosition (ID, JointType.ElbowLeft, _leftElbowFiltered.Value);
                
                if (Vector3.Angle (elbowHand1, -projectedForward) < 30 || Vector3.Angle (elbowHand2, -projectedForward) < 30) {
					mirror = !mirror;
					forward = CalcUnfilteredForward ();
				}
			}

            lastForward = forward;

            ////// Versão usada no Master
            _handStateLeft = tracker.GetHandState(ID, BodyPropertiesTypes.HandLeftState);
            _handStateRight = tracker.GetHandState(ID, BodyPropertiesTypes.HandRightState);
            //=======

            //Vector3 elbowHand1 = tracker.getJointPosition (ID, JointType.HandRight, rightHandFiltered.Value) - tracker.getJointPosition (ID, JointType.ElbowRight, rightElbowFiltered.Value);
            //Vector3 elbowHand2 = tracker.getJointPosition (ID, JointType.HandLeft, leftHandFiltered.Value) - tracker.getJointPosition (ID, JointType.ElbowLeft, leftElbowFiltered.Value);

//            if (Vector3.Angle (elbowHand1, -projectedForward) < 30 || Vector3.Angle (elbowHand2, -projectedForward) < 30)
//                {
//					mirror = !mirror;
//					forward = calcUnfilteredForward ();
//				}
//			}

//			lastForward = forward;

//            handStateLeft = tracker.getHandState(ID, BodyPropertiesTypes.HandLeftState);
//            handStateRight = tracker.getHandState(ID, BodyPropertiesTypes.HandRightState);
//>>>>>>> refs/remotes/mauriciosousa/master

            // Update Joints
            try
            {

				_headFiltered.Value = tracker.GetJointPosition (ID, JointType.Head, _headFiltered.Value);
				_neckFiltered.Value = tracker.GetJointPosition (ID, JointType.Neck, _neckFiltered.Value);
				_spineShoulderFiltered.Value = tracker.GetJointPosition(ID, JointType.SpineShoulder, _spineShoulderFiltered.Value);
				_spineMidFiltered.Value = tracker.GetJointPosition(ID, JointType.SpineMid, _spineMidFiltered.Value);
				_spineBaseFiltered.Value = tracker.GetJointPosition(ID, JointType.SpineBase, _spineBaseFiltered.Value);

                _leftHandScreenSpaceFiltered.Value = tracker.GetHandScreenSpace(ID, HandScreenSpace.HandLeftPosition);
                _rightHandScreenSpaceFiltered.Value = tracker.GetHandScreenSpace(ID, HandScreenSpace.HandRightPosition);

                if (mirror)
                {
					_rightShoulderFiltered.Value = tracker.GetJointPosition(ID, JointType.ShoulderLeft, _rightShoulderFiltered.Value);
					_rightElbowFiltered.Value = tracker.GetJointPosition(ID, JointType.ElbowLeft, _rightElbowFiltered.Value);
					_rightWristFiltered.Value = tracker.GetJointPosition(ID, JointType.WristLeft, _rightWristFiltered.Value);
					_rightHandFiltered.Value = tracker.GetJointPosition(ID, JointType.HandLeft, _rightHandFiltered.Value);
					_rightThumbFiltered.Value = tracker.GetJointPosition(ID, JointType.ThumbLeft, _rightThumbFiltered.Value);
					_rightHandTipFiltered.Value = tracker.GetJointPosition(ID, JointType.HandTipLeft, _rightHandTipFiltered.Value);
					_rightHipFiltered.Value = tracker.GetJointPosition(ID, JointType.HipLeft, _rightHipFiltered.Value);
					_rightKneeFiltered.Value = tracker.GetJointPosition(ID, JointType.KneeLeft, _rightKneeFiltered.Value);
					_rightAnkleFiltered.Value = tracker.GetJointPosition(ID, JointType.AnkleLeft, _rightAnkleFiltered.Value);
					_rightFootFiltered.Value = tracker.GetJointPosition(ID, JointType.FootLeft, _rightFootFiltered.Value);

					_leftShoulderFiltered.Value = tracker.GetJointPosition(ID, JointType.ShoulderRight, _leftShoulderFiltered.Value);
					_leftElbowFiltered.Value = tracker.GetJointPosition(ID, JointType.ElbowRight, _leftElbowFiltered.Value);
					_leftWristFiltered.Value = tracker.GetJointPosition(ID, JointType.WristRight, _leftWristFiltered.Value);
					_leftHandFiltered.Value = tracker.GetJointPosition(ID, JointType.HandRight, _leftHandFiltered.Value);
					_leftThumbFiltered.Value = tracker.GetJointPosition(ID, JointType.ThumbRight, _leftThumbFiltered.Value);
					_leftHandTipFiltered.Value = tracker.GetJointPosition(ID, JointType.HandTipRight, _leftHandTipFiltered.Value);
					_leftHipFiltered.Value = tracker.GetJointPosition(ID, JointType.HipRight, _leftHipFiltered.Value);
					_leftKneeFiltered.Value = tracker.GetJointPosition(ID, JointType.KneeRight, _leftKneeFiltered.Value);
					_leftAnkleFiltered.Value = tracker.GetJointPosition(ID, JointType.AnkleRight, _leftAnkleFiltered.Value);
					_leftFootFiltered.Value = tracker.GetJointPosition(ID, JointType.FootRight, _leftFootFiltered.Value);
				}
                else
                {
					_leftShoulderFiltered.Value = tracker.GetJointPosition(ID, JointType.ShoulderLeft, _leftShoulderFiltered.Value);
					_leftElbowFiltered.Value = tracker.GetJointPosition(ID, JointType.ElbowLeft, _leftElbowFiltered.Value);
					_leftWristFiltered.Value = tracker.GetJointPosition(ID, JointType.WristLeft, _leftWristFiltered.Value);
					_leftHandFiltered.Value = tracker.GetJointPosition(ID, JointType.HandLeft, _leftHandFiltered.Value);
					_leftThumbFiltered.Value = tracker.GetJointPosition(ID, JointType.ThumbLeft, _leftThumbFiltered.Value);
					_leftHandTipFiltered.Value = tracker.GetJointPosition(ID, JointType.HandTipLeft, _leftHandTipFiltered.Value);
					_leftHipFiltered.Value = tracker.GetJointPosition(ID, JointType.HipLeft, _leftHipFiltered.Value);
					_leftKneeFiltered.Value = tracker.GetJointPosition(ID, JointType.KneeLeft, _leftKneeFiltered.Value);
					_leftAnkleFiltered.Value = tracker.GetJointPosition(ID, JointType.AnkleLeft, _leftAnkleFiltered.Value);
					_leftFootFiltered.Value = tracker.GetJointPosition(ID, JointType.FootLeft, _leftFootFiltered.Value);

					_rightShoulderFiltered.Value = tracker.GetJointPosition(ID, JointType.ShoulderRight, _rightShoulderFiltered.Value);
					_rightElbowFiltered.Value = tracker.GetJointPosition(ID, JointType.ElbowRight, _rightElbowFiltered.Value);
					_rightWristFiltered.Value = tracker.GetJointPosition(ID, JointType.WristRight, _rightWristFiltered.Value);
					_rightHandFiltered.Value = tracker.GetJointPosition(ID, JointType.HandRight, _rightHandFiltered.Value);
					_rightThumbFiltered.Value = tracker.GetJointPosition(ID, JointType.ThumbRight, _rightThumbFiltered.Value);
					_rightHandTipFiltered.Value = tracker.GetJointPosition(ID, JointType.HandTipRight, _rightHandTipFiltered.Value);
                    _rightHipFiltered.Value = tracker.GetJointPosition(ID, JointType.HipRight, _rightHipFiltered.Value);
					_rightKneeFiltered.Value = tracker.GetJointPosition(ID, JointType.KneeRight, _rightKneeFiltered.Value);
					_rightAnkleFiltered.Value = tracker.GetJointPosition(ID, JointType.AnkleRight, _rightAnkleFiltered.Value);
					_rightFootFiltered.Value = tracker.GetJointPosition(ID, JointType.FootRight, _rightFootFiltered.Value);
				}

				_head.transform.position = _headFiltered.Value;
				_leftShoulder.transform.position = _leftShoulderFiltered.Value;
				_rightShoulder.transform.position = _rightShoulderFiltered.Value;
				_leftElbow.transform.position = _leftElbowFiltered.Value;
				_rightElbow.transform.position = _rightElbowFiltered.Value;
				_leftHand.transform.position = _leftHandFiltered.Value;
				_rightHand.transform.position = _rightHandFiltered.Value;
				_spineMid.transform.position = _spineMidFiltered.Value;
				_leftHip.transform.position = _leftHipFiltered.Value;
				_rightHip.transform.position = _rightHipFiltered.Value;
				_leftKnee.transform.position = _leftKneeFiltered.Value;
				_rightKnee.transform.position = _rightKneeFiltered.Value;
				_leftFoot.transform.position = _leftFootFiltered.Value;
				_rightFoot.transform.position = _rightFootFiltered.Value;

				// update forward
				Vector3 fw = CalcForward ();
				Vector3 pos = _spineMid.transform.position;

				//forwardGO.transform.forward = fw;
				//forwardGO.transform.position = pos;
				floorForwardGameObject.transform.forward = new Vector3 (fw.x, 0, fw.z);
				floorForwardGameObject.transform.position = new Vector3 (pos.x, 0.001f, pos.z);
				floorForwardGameObject.transform.parent = transform;
//>>>>>>> refs/remotes/mauriciosousa/master
			}
            catch (Exception e)
            {
				Debug.Log (e.Message + "\n" + e.StackTrace);
			}
		}
	}

            /*
             <<<<<<< HEAD
                    if (_canSend)
                        {
                        string pdu = BodyPropertiesTypes.UID.ToString () + MessageSeparators.SET + ID + MessageSeparators.L2;

                        ////// Versão usada no Master
                        // pdu += BodyPropertiesTypes.HandLeftState.ToString ()       + MessageSeparators.SET + handStateLeft  + MessageSeparators.L2;
                        // pdu += BodyPropertiesTypes.HandLeftConfidence.ToString ()  + MessageSeparators.SET + "Null"         + MessageSeparators.L2;
                        // pdu += BodyPropertiesTypes.HandRightState.ToString ()      + MessageSeparators.SET + handStateRight + MessageSeparators.L2;
                        // pdu += BodyPropertiesTypes.HandRightConfidence.ToString () + MessageSeparators.SET + "Null"         + MessageSeparators.L2;

                        ////// Versão Alterada
                        pdu += BodyPropertiesTypes.HandLeftState.ToString ()      + MessageSeparators.SET + _handLeftState  + MessageSeparators.L2;
                        pdu += BodyPropertiesTypes.HandLeftConfidence.ToString()  + MessageSeparators.SET + "Null"          + MessageSeparators.L2;
                        pdu += BodyPropertiesTypes.HandRightState.ToString ()     + MessageSeparators.SET + _handRightState + MessageSeparators.L2;
                        pdu += BodyPropertiesTypes.HandRightConfidence.ToString() + MessageSeparators.SET + "Null"          + MessageSeparators.L2;

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
                    else
                        throw new Exception ("Human not initalized.");
            =======

                         */

internal string GetPdu()
	{
        if (canSend)
        {
            string pdu = BodyPropertiesTypes.UID.ToString() + MessageSeparators.SET + ID + MessageSeparators.L2;

            pdu += BodyPropertiesTypes.HandLeftState.ToString()       + MessageSeparators.SET + _handStateLeft  + MessageSeparators.L2;
            pdu += BodyPropertiesTypes.HandLeftConfidence.ToString()  + MessageSeparators.SET + "Null"         + MessageSeparators.L2;
            pdu += BodyPropertiesTypes.HandRightState.ToString()      + MessageSeparators.SET + _handStateRight + MessageSeparators.L2;
            pdu += BodyPropertiesTypes.HandRightConfidence.ToString() + MessageSeparators.SET + "Null"         + MessageSeparators.L2;

            pdu += HandScreenSpace.HandLeftPosition.ToString() + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftHandScreenSpaceFiltered.Value) + MessageSeparators.L2;
            pdu += HandScreenSpace.HandRightPosition.ToString() + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightHandScreenSpaceFiltered.Value) + MessageSeparators.L2;

            pdu += "head" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_headFiltered.Value) + MessageSeparators.L2;
            pdu += "neck" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_neckFiltered.Value) + MessageSeparators.L2;
            pdu += "spineShoulder" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_spineShoulderFiltered.Value) + MessageSeparators.L2;
            pdu += "spineMid" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_spineMidFiltered.Value) + MessageSeparators.L2;
            pdu += "spineBase" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_spineBaseFiltered.Value) + MessageSeparators.L2;

            pdu += "leftShoulder" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftShoulderFiltered.Value) + MessageSeparators.L2;
            pdu += "leftElbow" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftElbowFiltered.Value) + MessageSeparators.L2;
            pdu += "leftWrist" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftWristFiltered.Value) + MessageSeparators.L2;
            pdu += "leftHand" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftHandFiltered.Value) + MessageSeparators.L2;
            pdu += "leftThumb" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftThumbFiltered.Value) + MessageSeparators.L2;
            pdu += "leftHandTip" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftHandTipFiltered.Value) + MessageSeparators.L2;
            pdu += "leftHip" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftHipFiltered.Value) + MessageSeparators.L2;
            pdu += "leftKnee" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftKneeFiltered.Value) + MessageSeparators.L2;
            pdu += "leftAnkle" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftAnkleFiltered.Value) + MessageSeparators.L2;
            pdu += "leftFoot" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_leftFootFiltered.Value) + MessageSeparators.L2;

            pdu += "rightShoulder" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightShoulderFiltered.Value) + MessageSeparators.L2;
            pdu += "rightElbow" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightElbowFiltered.Value) + MessageSeparators.L2;
            pdu += "rightWrist" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightWristFiltered.Value) + MessageSeparators.L2;
            pdu += "rightHand" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightHandFiltered.Value) + MessageSeparators.L2;
            pdu += "rightThumb" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightThumbFiltered.Value) + MessageSeparators.L2;
            pdu += "rightHandTip" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightHandTipFiltered.Value) + MessageSeparators.L2;
            pdu += "rightHip" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightHipFiltered.Value) + MessageSeparators.L2;
            pdu += "rightKnee" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightKneeFiltered.Value) + MessageSeparators.L2;
            pdu += "rightAnkle" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightAnkleFiltered.Value) + MessageSeparators.L2;
            pdu += "rightFoot" + MessageSeparators.SET + CommonUtils.ConvertVectorToStringRPC(_rightFootFiltered.Value);

            return pdu;
        }
        else
        {
            throw new Exception("Human not initalized.");
        }
//>>>>>>> refs/remotes/mauriciosousa/master
	}

	public Vector3 GetMidSpine()
	{
	    return _spineMidFiltered.Value; //_spineMidKalman.Value;
	}

    public Vector3 GetHead ()
    {
        return _headFiltered.Value; // _headKalman.Value;
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
        
        return _rightKneeFiltered == null ? tracker.GetJointPosition(ID, JointType.KneeRight, Vector3.zero) : _rightKneeFiltered.Value; // _rightKneeKalman == null ? tracker.GetJointPosition(ID, JointType.KneeRight) : _rightKneeKalman.Value;
    }

    private Vector3 GetLeftKnee()
    {
        return _leftKneeFiltered == null ? tracker.GetJointPosition(ID, JointType.KneeLeft, Vector3.zero) : _leftKneeFiltered.Value; // _leftKneeKalman == null ? tracker.GetJointPosition(ID, JointType.KneeLeft) : _leftKneeKalman.Value;
    }

}
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//=======
//		return headFiltered.Value;
//	}
//}
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 

    <<<<<<< HEAD
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
=======










     
     
     */

/////////////////////////////////////////////////////////////////////////////
//<<<<<<< HEAD
//    public Tracker tracker;

//    private GameObject _floorForwardGameObject;
//    // private GameObject forwardGO;

//    private GameObject _head;
//    private GameObject _leftShoulder;
//    private GameObject _rightShoulder;
//    private GameObject _leftElbow;
//    private GameObject _rightElbow;
//    private GameObject _leftHand;
//    private GameObject _rightHand;
//    private GameObject _spineMid;
//    private GameObject _leftHip;
//    private GameObject _rightHip;
//    private GameObject _leftKnee;
//    private GameObject _rightKnee;
//    private GameObject _leftFoot;
//    private GameObject _rightFoot;

//    private AdaptiveDoubleExponentialFilterVector3 _headKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _neckKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineMidKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineBaseKalman;

//    private AdaptiveDoubleExponentialFilterVector3 _leftShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftElbowKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftWristKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHandKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftThumbKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHandTipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftKneeKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftAnkleKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftFootKalman;

//    private AdaptiveDoubleExponentialFilterVector3 _rightShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightElbowKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightWristKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHandKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightThumbKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHandTipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightKneeKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightAnkleKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightFootKalman;

//    private Vector3 _lastForward;

//    public int ID;

//    private bool _canSend = false;
//    private bool _mirror  = false;


//    ////// Versão usada no Master
//    //private string handStateLeft  = HandState.Unknown.ToString();
//    //private string handStateRight = HandState.Unknown.ToString();

//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    //////        Dissertação - Mestrado em Engenharia Informática e de Computadores                                   //////////
//    //////        Francisco Henriques Venda, nº 73839                                                                  //////////
//    //////        Alterações apartir daqui                                                                             //////////
//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    ////// Versão Alterada
//    private string _handLeftState;
//    private string _handRightState;
//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//    // ReSharper disable once UnusedMember.Local
//    // ReSharper disable once ArrangeTypeMemberModifiers
//    void Start ()
//	{
//		var humanCollider = gameObject.AddComponent<CapsuleCollider> ();
//		humanCollider.radius = 0.25f;
//		humanCollider.height = 1.75f;

//		_head          = CreateSphere ("head", 0.3f);
//		_leftShoulder  = CreateSphere ("leftShoulder");
//		_rightShoulder = CreateSphere ("rightShoulder");
//		_leftElbow     = CreateSphere ("leftElbow");
//		_rightElbow    = CreateSphere ("rightElbow");
//		_leftHand      = CreateSphere ("leftHand");
//		_rightHand     = CreateSphere ("rightHand");
//		_spineMid      = CreateSphere ("spineMid", 0.2f);
//		_leftHip       = CreateSphere ("leftHip");
//		_rightHip      = CreateSphere ("rightHip");
//		_leftKnee      = CreateSphere ("leftKnee");
//		_rightKnee     = CreateSphere ("rightKnee");
//		_leftFoot      = CreateSphere ("leftFoot");
//		_rightFoot     = CreateSphere ("rightFoot");

//		_headKalman          = new AdaptiveDoubleExponentialFilterVector3 ();
//		_neckKalman          = new AdaptiveDoubleExponentialFilterVector3 ();
//		_spineShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
//		_spineMidKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
//		_spineBaseKalman     = new AdaptiveDoubleExponentialFilterVector3 ();

//		_leftShoulderKalman  = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftElbowKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftWristKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftHandKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftThumbKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftHandTipKalman   = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftHipKalman       = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftKneeKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftAnkleKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_leftFootKalman      = new AdaptiveDoubleExponentialFilterVector3 ();

//		_rightShoulderKalman = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightElbowKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightWristKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightHandKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightThumbKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightHandTipKalman  = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightHipKalman      = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightKneeKalman     = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightAnkleKalman    = new AdaptiveDoubleExponentialFilterVector3 ();
//		_rightFootKalman     = new AdaptiveDoubleExponentialFilterVector3 ();

//        _canSend = true;
//        _lastForward = Vector3.zero;

//		// forwardGO = new GameObject();
//		// forwardGO.name = "ForwardOld";
//		// forwardGO.transform.parent = transform;
//		// GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
//		// cylinder.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
//		// cylinder.transform.position += new Vector3(0, 0, 0.25f);
//		// cylinder.transform.up = Vector3.forward;
//		// cylinder.transform.parent = forwardGO.transform;

//		_floorForwardGameObject = (GameObject)Instantiate (Resources.Load ("Prefabs/FloorForwardPlane"));
//		_floorForwardGameObject.name = "Forward";
//		_floorForwardGameObject.tag  = "nocolor";
//		_floorForwardGameObject.transform.parent = transform;

//        ////////////////////////////////////////////////////////////////////////////////////////////////////////
//        ////////////////////////////////////////////////////////////////////////////////////////////////////////
//        _handLeftState  = "Null";
//        _handRightState = "Null";
//	    ////////////////////////////////////////////////////////////////////////////////////////////////////////
//	    ////////////////////////////////////////////////////////////////////////////////////////////////////////
//    }

//    private GameObject CreateSphere (string sphereName, float scale = 0.1f)
//=======

//    public Tracker tracker;

//    private GameObject _floorForwardGameObject;
//    // private GameObject forwardGO;

//    private GameObject _head;
//    private GameObject _leftShoulder;
//    private GameObject _rightShoulder;
//    private GameObject _leftElbow;
//    private GameObject _rightElbow;
//    private GameObject _leftHand;
//    private GameObject _rightHand;
//    private GameObject _spineMid;
//    private GameObject _leftHip;
//    private GameObject _rightHip;
//    private GameObject _leftKnee;
//    private GameObject _rightKnee;
//    private GameObject _leftFoot;
//    private GameObject _rightFoot;

//    private AdaptiveDoubleExponentialFilterVector3 _headKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _neckKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineMidKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _spineBaseKalman;

//    private AdaptiveDoubleExponentialFilterVector3 _leftShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftElbowKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftWristKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHandKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftThumbKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHandTipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftHipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftKneeKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftAnkleKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _leftFootKalman;

//    private AdaptiveDoubleExponentialFilterVector3 _rightShoulderKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightElbowKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightWristKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHandKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightThumbKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHandTipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightHipKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightKneeKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightAnkleKalman;
//    private AdaptiveDoubleExponentialFilterVector3 _rightFootKalman;

//    private Vector3 _lastForward;

//    public int ID;

//    private bool _canSend = false;
//    private bool _mirror  = false;
/////////////////////////////////////////////////////////////