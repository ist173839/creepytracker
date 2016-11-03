using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Windows.Kinect;


// [RequireComponent(typeof(TrackerClient))]
// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
public class ObjectTracker: MonoBehaviour {

    //References to both trackers to get objects from them
    
    private enum Point
    {
        Point1,
        Point2,
        Point3,
    }
    
    private Tracker _localTracker;

    private WriteMatrix _myWriteMatrix;

    private Point _currentPoint = Point.Point1;

    public int MatrixUpdateTimer = 10;
    public int PointSampleTimer  = 05;
    public int OptiObjectIndex   = 00;


    public bool AutoUpdatePoints = false;

    public Vector3 TransformationVectorCPtoOt = Vector3.zero;

    private Vector3 _point1Cp = Vector3.zero;
    private Vector3 _point1Ot = Vector3.zero;

    private Vector3 _point2Cp = Vector3.zero;
    private Vector3 _point2Ot = Vector3.zero;

    private Vector3 _point3Cp = Vector3.zero;
    private Vector3 _point3Ot = Vector3.zero;

    public Matrix4x4 TransformOTtoCt = Matrix4x4.identity;

    ///////////////////////////////////////////////////////
    public Side HandToUse = Side.Right;

    public int MainId;

    // Use this for initialization
    void Start () {

        _localTracker = gameObject.GetComponent<Tracker>();
        _myWriteMatrix = new WriteMatrix();

        // If autosampling is enabled, start Coroutines
        if (AutoUpdatePoints)
        {
            StartCoroutine(TakePointSample(PointSampleTimer));
            //StartCoroutine(updateMatrix(matrixUpdateYimer));
        }
    }
	
	// Update is called once per frame
	void Update ()
	{
	    if (!SetMainId() || _localTracker == null) return;

	    // Manual Calibration with S
	    // After 3 points get collected, the transformation matrix is calculated
	    if (Input.GetKeyDown(KeyCode.S))
	    {
	        Debug.Log("Calibration Point: " + _currentPoint + "\n");
	        Calibration3Points();
	        if (_currentPoint == Point.Point1)
	        {
	            Debug.Log("Matrix Updated");
	            CalcTransformationMatrix();
	        }
	    }

	    // Enable/Disable the point autosample
	    // Every 3 points, the matrix is updated
	    if (Input.GetKeyDown(KeyCode.A))
	    {
	        AutoUpdatePoints = !AutoUpdatePoints;
	        Debug.Log("AutoSampling " + (AutoUpdatePoints ? "On\n" : "Off\n"));
	    }
	}

    private bool SetMainId()
    {
        var idList = _localTracker.IdIntList;

        if (idList.Count == 0) return false;

        if (!idList.Contains(MainId))
        {
            MainId = idList[0];
        }

        return true;
    }

    // AutoSample Routine every *time* seconds
    private IEnumerator TakePointSample(float time)
    {
        while (AutoUpdatePoints) {
            yield return new WaitForSeconds(time);
            Debug.Log("calibration point: " + _currentPoint + "\n");
            Calibration3Points();
            if (_currentPoint == Point.Point1 && _point3Cp != Vector3.zero)
            {
                Debug.Log("matrix updated");
                CalcTransformationMatrix();
            }
        }
    }

    private void Calibration3Points()
    {
        var positionFromCreepyTracker = Vector3.zero;
        var positionFromOptiTracker   = Vector3.zero;

        // Get rigidbodies from Optitrack system
        var rigidBodies = OptitrackManagement.DirectMulticastSocketClient.GetStreemData().RigidBody;
        
        // Use points from either the right hand or the left hand, in order to have a reference point in both coordinate systems
        var rightHand = _localTracker.GetJointPosition(MainId, JointType.HandRight);
        var leftHand  = _localTracker.GetJointPosition(MainId, JointType.HandLeft);
    
        switch (HandToUse)
        {
            case Side.Right:
                positionFromCreepyTracker = rightHand;
                break;
            case Side.Left:
                positionFromCreepyTracker = leftHand;
                break;
            default:
                Debug.LogError("Use 'right' or 'left' instead of " + HandToUse + " in the handToUse field.\n");
                break;
        }

        // In order to get the original point, if the matrix has been updated
        // We need to multiply the point by the inverse of the transformation matrix, to get the original scaled
        // point from the optitrack coordinate system, so we're able to update the matrix, if needed

        //positionFromOptiTracker = rigidBodies[optiObjectIndex].RigidBodyGameObject.GetComponent<applyTransformation>().scaledPosition;
        positionFromOptiTracker = rigidBodies[OptiObjectIndex].RigidBodyGameObject.transform.position;
        if(TransformOTtoCt != Matrix4x4.identity)
        {
            var aux =  TransformOTtoCt.inverse * positionFromOptiTracker;
            positionFromOptiTracker = new Vector3(aux.x, aux.y, aux.z);
        }

        // Update the correct set of points each time the function is called, according to the currentPoint

        if (_currentPoint == Point.Point1)
        {
            _point1Cp = positionFromCreepyTracker;
            _point1Ot = positionFromOptiTracker;

            _currentPoint = Point.Point2;

			Debug.Log("1CP " + _point1Cp); Debug.Log("1OT " + _point1Ot);
        }
        else if (_currentPoint == Point.Point2)
        {
            _point2Cp = positionFromCreepyTracker;
            _point2Ot = positionFromOptiTracker;
            
            _currentPoint = Point.Point3;

			Debug.Log("2CP " + _point2Cp); Debug.Log("2OT " + _point2Ot);

        }
        else if (_currentPoint == Point.Point3)
        {
            _point3Cp = positionFromCreepyTracker;
            _point3Ot = positionFromOptiTracker;

            _currentPoint = Point.Point1;

			Debug.Log("3CP " + _point3Cp); Debug.Log("3OT " + _point3Ot);
        }
    }

    // Calculate the transformation matrix, with 3 sets of points from each of the coordinate systems

    private void CalcTransformationMatrix()
    {
        // ReSharper disable once InconsistentNaming
        var CTmatrix = Matrix4x4.identity;
        // ReSharper disable once InconsistentNaming
        var OTmatrix = Matrix4x4.identity;

        // Update the CreepyTracker matrix with the 3 points taken from the calibration method
        // 
        // Assuming X, Y and Z are the 3 points sampled, and Xx, Xy and Xz the 3 components of the X point
        // 
        // [ Xx Yx Zx 0 ]
        // [ Xy Yy Zy 0 ]
        // [ Xz Yz Zz 0 ]
        // [  0  0  0 1 ]
        // 
        // The Optitrack related matrix is calculated using the same method
        // 
        // With the 2 matrices, we can transform a point from the Optitrack's coordinate system to the CreepyTracker's
        // By multiplying it by the Optitrack's matrix inverse, followed by the Creepytracker's matrix using the formula
        // 
        // CT * OT(-1) * V
        // 
        // Being CT the creepytracker's matrix, OT(-1) the inverse of the Optitrack matrix calculated, and V the point to
        // transform to the creepytracker's coordinate system

        CTmatrix.m00 = _point1Cp.x;
        CTmatrix.m10 = _point1Cp.y;
        CTmatrix.m20 = _point1Cp.z;
        CTmatrix.m30 = 0;

        CTmatrix.m01 = _point2Cp.x;
        CTmatrix.m11 = _point2Cp.y;
        CTmatrix.m21 = _point2Cp.z;
        CTmatrix.m31 = 0;

        CTmatrix.m02 = _point3Cp.x;
        CTmatrix.m12 = _point3Cp.y;
        CTmatrix.m22 = _point3Cp.z;
        CTmatrix.m32 = 0;

        CTmatrix.m03 = 0;
        CTmatrix.m13 = 0;
        CTmatrix.m23 = 0;
        CTmatrix.m33 = 1;

        //OTmatrix
        OTmatrix.m00 = _point1Ot.x;
        OTmatrix.m10 = _point1Ot.y;
        OTmatrix.m20 = _point1Ot.z;
        OTmatrix.m30 = 0;

        OTmatrix.m01 = _point2Ot.x;
        OTmatrix.m11 = _point2Ot.y;
        OTmatrix.m21 = _point2Ot.z;
        OTmatrix.m31 = 0;

        OTmatrix.m02 = _point3Ot.x;
        OTmatrix.m12 = _point3Ot.y;
        OTmatrix.m22 = _point3Ot.z;
        OTmatrix.m32 = 0;

        OTmatrix.m03 = 0;
        OTmatrix.m13 = 0;
        OTmatrix.m23 = 0;
        OTmatrix.m33 = 1;

        TransformOTtoCt = CTmatrix * OTmatrix.inverse;
        
        _myWriteMatrix.SaveMatrix(TransformOTtoCt);
        // Update the transform matrix being used in the OptidrawScript
        // Optidraw.GetComponent<OptiDrawScript>().transformMatrix = TransformOTtoCt;
    }

    /*
    IEnumerator updateMatrix(float time)
    {
        while (autoUpdatePoints)
        {
            yield return new WaitForSeconds(time);
            if (_point3CP != Vector3.zero)
            {
                Debug.Log("matrix updated");
                calcTransformationMatrix();
            }
        }         
    }
    */
}

