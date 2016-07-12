using UnityEngine;
using System;
using System.Collections;
//using NatNetML;
using OptitrackManagement;

public class OptitrackManager : MonoBehaviour
{
    //public GameObject markerObject;

    private string MyName = "HAL";
    //public string ClientIpAddress ="172.20.41.25";
    //public string ServerIpAddress ="172.20.41.24";
    public Vector3 PositionVector { get; set; }
    public bool DeinitValue = false;
    public bool IsOn { get; private set; }

    public bool IsEmulate { get; set; }

    ~OptitrackManager()
    {
        Debug.Log("OptitrackManager: Destruct");
        OptitrackManagement.DirectMulticastSocketClient.Close();
    }

    void Start()
    {
        Debug.Log(MyName + ": i am alive");
        OptitrackManagement.DirectMulticastSocketClient.Start();
        PositionVector = transform.position;
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

                PositionVector = networkData.RigidBody[0].Pos*2.0f;
            }

            // transform.position = PositionVector;

            if (DeinitValue)
            {
                DeinitValue = false;
                OptitrackManagement.DirectMulticastSocketClient.Close();
            }

            OtherMarker[] markers = OptitrackManagement.DirectMulticastSocketClient.GetStreemData().OtherMarkers;
        }
        else
        {
            //todo fazer qualquer coisa ou apagar else
        }
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
