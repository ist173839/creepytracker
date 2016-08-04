﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackerProperties : MonoBehaviour {

    private static TrackerProperties _singleton;

    public int listenPort = 56555;
    public int broadcastPort = 56839;
    public int sendInterval = 50;

    [Range(0, 1)]
    public float mergeDistance = 0.3f;

    [Range(0, 17)]
    public int confidenceTreshold = 7;

    public Windows.Kinect.JointType centerJoint = Windows.Kinect.JointType.SpineShoulder;
    public Windows.Kinect.JointType upJointA = Windows.Kinect.JointType.SpineBase;
    public Windows.Kinect.JointType upJointB = Windows.Kinect.JointType.SpineShoulder;

    public string configFilename = "configSettings.txt";

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

    void Start()
    {
        //_singleton = this;
    }
}
