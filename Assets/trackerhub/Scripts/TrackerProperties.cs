using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class TrackerProperties : MonoBehaviour
{

    private static TrackerProperties _singleton;

    public int ListenPort = 56555;
    public int BroadcastPort = 56839;
    public int SendInterval = 50;

    [Range(0, 1)]  public float MergeDistance = 0.3f;

    [Range(0, 17)] public int ConfidenceTreshold = 7;

    public Windows.Kinect.JointType CenterJoint = Windows.Kinect.JointType.SpineShoulder;
    public Windows.Kinect.JointType UpJointA    = Windows.Kinect.JointType.SpineBase;
    public Windows.Kinect.JointType UpJointB    = Windows.Kinect.JointType.SpineShoulder;

    public string ConfigFilename = "configSettings.txt";

    private TrackerProperties()
    {
        _singleton = this;
    }

    public static TrackerProperties Instance
    {
        get
        {
            return _singleton;
        }
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start()
    {
        //_singleton = this;
    }
}
