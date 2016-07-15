using UnityEngine;
using System;
using System.Collections;
//using NatNetML;
using OptitrackManagement;

// ReSharper disable once CheckNamespace
public class OptitrackManager : MonoBehaviour
{
    //public GameObject markerObject;

    private readonly string _myName = "OptiTrack";
    //public string ClientIpAddress ="172.20.41.25";
    //public string ServerIpAddress ="172.20.41.24";
    public Vector3 PositionVector { get; set; }
    public Quaternion RotationQuaternion { get; set; }
    private bool _deinitValue = false;
    public bool IsOn { get; private set; }
    public bool IsEmulate { get; set; }

    private GameObject _optiTrackMarker;


    ~OptitrackManager()
    {
        Debug.Log("OptitrackManager: Destruct");
        OptitrackManagement.DirectMulticastSocketClient.Close();
    }

    void Start()
    {
        Debug.Log(_myName + ": I am alive");
        OptitrackManagement.DirectMulticastSocketClient.Start();
        _optiTrackMarker = null;
        IsEmulate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsEmulate)
        {
            OptitrackManagement.DirectMulticastSocketClient.Update();
            //_netClient.Step();
            IsOn = OptitrackManagement.DirectMulticastSocketClient.IsInit();
            if (OptitrackManagement.DirectMulticastSocketClient.IsInit())
            {
                StreemData networkData = OptitrackManagement.DirectMulticastSocketClient.GetStreemData();
                PositionVector = networkData.RigidBody[0].Pos;//* 2.0f;
                RotationQuaternion = networkData.RigidBody[0].Ori;

                SetUpOptiTrackMarker();
            }
            else
            {
                if (_optiTrackMarker != null)
                {
                    Destroy(_optiTrackMarker);
                }

                Debug.Log("Restart OptiTrack");
                OptitrackManagement.DirectMulticastSocketClient.Start();
            }


            // transform.position = PositionVector;

            if (_deinitValue)
            {
                _deinitValue = false;
                OptitrackManagement.DirectMulticastSocketClient.Close();
            }

            OtherMarker[] markers = OptitrackManagement.DirectMulticastSocketClient.GetStreemData().OtherMarkers;
        }
        else
        {
            //todo fazer qualquer coisa ou apagar else
        }
    }

    private void SetUpOptiTrackMarker()// Todo Testar
    {
        if (_optiTrackMarker == null)
        {
            _optiTrackMarker = MyCreateSphere("Opti Tracker Marker", PositionVector, 0.3f);
            _optiTrackMarker.transform.position = PositionVector;
        }
        else
        {
            _optiTrackMarker.transform.position = PositionVector;
        }
    }

    private static GameObject MyCreateSphere(string name, Vector3 position, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;
        gameObjectSphere.GetComponent<Renderer>().material.color = Color.red;
        gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.transform.position = position;
        gameObjectSphere.name = name;
        return gameObjectSphere;
    }

    private static GameObject MyCreateSphere(string name, float scale = 0.1f)
    {
        var gameObjectSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gameObjectSphere.GetComponent<SphereCollider>().enabled = false;
        gameObjectSphere.GetComponent<Renderer>().material.color = Color.red;
     gameObjectSphere.transform.localScale = new Vector3(scale, scale, scale);
        gameObjectSphere.name = name;

        return gameObjectSphere;
    }
}

/*
  for (int i = 0; i < OptitrackManagement.DirectMulticastSocketClient.GetStreemData().NOtherMarkers; i++)
        {
            if (markers[i].markerGameObject == null)
            {
                if (markerObject == null)
                    markerObject = GameObject.Find("Marker Prefab");
                markers[i].markerGameObject = (GameObject)Instantiate(markerObject, markers[i].pos, Quaternion.identity);
            }
            else {
                markers[i].markerGameObject.transform.position = markers[i].pos *10;
            }
        }
     
     
     
     */
