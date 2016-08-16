/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using UnityEngine;
using System;
using System.Collections;
//using NatNetML;
using OptitrackManagement;

// ReSharper disable once CheckNamespace
public class OptitrackManager : MonoBehaviour
{

    enum Pos
    {
        pos1,
        pos2,
    }

    private Pos _nextPos;

    private GameObject _forwardGo;
    private GameObject _cylinder;
    private GameObject _optiTrackMarker;
    //public GameObject markerObject;

    private readonly string _myName = "OptiTrack";
    //public string ClientIpAddress ="172.20.41.25";
    //public string ServerIpAddress ="172.20.41.24";

    public bool IsEmulate { get;         set; }
    public bool IsOn      { get; private set; }

    private bool _deinitValue = false;

    private Vector3? _pos1;
    private Vector3? _pos2;

    private Vector3 _positionVector;
    private Vector3 _forward;

    public Matrix4x4 TransformMatrix = Matrix4x4.identity;

    private Quaternion _rotationQuaternion;
    
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
        /////////////////////////////////////

        _forwardGo = new GameObject { name = "Forward" };
        // _forwardGo.transform.parent = transform;

        _cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        _cylinder.GetComponent<CapsuleCollider>().enabled = false;
        //_cylinder.GetComponent<MeshRenderer>().enabled = false;
        _cylinder.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);
        _cylinder.transform.position += new Vector3(0, 0, 0.25f);
        _cylinder.transform.up = Vector3.forward;
        _cylinder.transform.parent = _forwardGo.transform;

        _nextPos = Pos.pos1;
        _pos1 = _pos2 = null;
        _forward = Vector3.forward;

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

                //_positionVector = networkData.RigidBody[0].Pos;//* 2.0f;



                var posVec = TransformMatrix * networkData.RigidBody[0].Pos;
                _positionVector = new Vector3(posVec.x, posVec.y, posVec.z);


                _rotationQuaternion = networkData.RigidBody[0].Ori;


                //CheckInput();
                //var rotation = RotationQuaternion;
                //var forward = rotation.eulerAngles;
                //UpdateForwardObject(_forward, GetUnityPositionVector(), RotationQuaternion);
                
                SetUpOptiTrackMarker();
            }
            else
            {
                if (_optiTrackMarker != null)
                {
                  //  Destroy(_optiTrackMarker);
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

    private void CheckInput()
    {
        if (Input.GetKeyUp("o")) // Input.GetMouseButtonDown(0)
        {
            var temp = GetUnityPositionVector();
            Debug.Log("O");
            switch (_nextPos)
            {
                case Pos.pos1:
                    _nextPos = Pos.pos2;

                    _pos1 = new Vector3(temp.x, 0.0f, temp.z);
                    Debug.Log("Pos 1 =  X = " + _pos1.Value.x + ", Z = " + _pos1.Value.z);
                    break;
                case Pos.pos2:
                    _nextPos = Pos.pos1;
                    _pos2 = new Vector3(temp.x, 0.0f, temp.z);
                    Debug.Log("Pos 2 : X = " + _pos2.Value.x + ", Z = " + _pos2.Value.z);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Input.GetKeyUp("t"))
        {
            Debug.Log("T");
            // ResetForward();
            if (_pos1 != null && _pos2 != null)
            {
                _forward = _pos2.Value - _pos1.Value;
                _forward = _forward.normalized;
                Debug.Log("_forward : X = " + _forward.x + ", Z = " + _forward.z);
            }
        }

    }

    private void UpdateForwardObject(Vector3 forward, Vector3 position, Quaternion rotation)
    {
        _forwardGo.transform.forward = forward;
        _forwardGo.transform.position = position;
        _forwardGo.transform.rotation = rotation;
    }

    public void SetEmulateValues(Vector3 positionVector, Quaternion rotationQuaternion)
    {
        _positionVector = positionVector;
        _rotationQuaternion = rotationQuaternion;
    }

    private static Vector3 ConvertUnityVector3(Vector3 vec3)
    {
        return new Vector3(-1*vec3.x, vec3.y, vec3.z);
    }

    public Vector3 GetPositionVector()
    {
        return _positionVector;
    }

    public Vector3 GetUnityPositionVector()
    {
        return ConvertUnityVector3(_positionVector);
    }

    public Quaternion GetRotationQuaternion()
    {
        return _rotationQuaternion;
    }

    private void SetUpOptiTrackMarker()
    { 
        var unityPositionVector = GetUnityPositionVector();

        if (_optiTrackMarker == null)
        {
            
            _optiTrackMarker = MyCreateSphere("Opti Tracker Marker", unityPositionVector, 0.3f);
            _optiTrackMarker.transform.position = unityPositionVector;
        }
        else
        {
            _optiTrackMarker.transform.position = unityPositionVector;
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
