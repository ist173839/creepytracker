/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using UnityEngine;
using System.Collections;
using OptitrackManagement;

//WARNING: THIS SCRIPT SHOULD BE ASSOCIATED TO THE CAMERA!
// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
public class OptiDrawScript : MonoBehaviour
{

    public GameObject MarkerObject;
    public GameObject TrailObject;
    public GameObject RigidBodyObject;
    public GameObject PenObject;
    public GameObject MenuObject;
    public GameObject DrawLimits;

    private GameObject _currTrail;
    private GameObject _lastTrail;

    public Transform WorldObjects;

    private Vector3 _mLastMarkerPos;

    public Vector3 WorldScaleOffset;

    public Vector3 CameraOffset;

    public float OptiTrackPosMultiplyer;

    private string message_to_show;

    //private OVRCameraController RiftCamController;
   // private OVRCameraRig RiftCamController;

    private bool _drawing;

    ~OptiDrawScript()
    {
        Debug.Log("OptitrackManager: Destruct");
        OptitrackManagement.DirectMulticastSocketClient.Close();
    }

	// Use this for initialization
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
	void Start ()
    {
        WorldScaleOffset = Vector3.zero;
        message_to_show = "";
        OptitrackManagement.DirectMulticastSocketClient.Start();

        if (PenObject == null)
        {
            PenObject = GameObject.Find("Pen Prefab");
        }
        if (RigidBodyObject == null)
        {
            RigidBodyObject = GameObject.Find("RigidBody Prefab");
        }
        if (MenuObject == null)
        {
            MenuObject = GameObject.Find("Menu Prefab");
        }
        //RiftCamController = this.GetComponent<OVRCameraRig>();
	}
	
	// Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
	void Update ()
    {
        OptitrackManagement.DirectMulticastSocketClient.Update();
        ProcessOptiTrackInput();

        DrawLimits.transform.position = WorldScaleOffset;
	}


    void ProcessOptiTrackInput()
    {
        OtherMarker[] markers = OptitrackManagement.DirectMulticastSocketClient.GetStreemData().OtherMarkers;

        OptitrackManagement.RigidBody[] rigidBodies = OptitrackManagement.DirectMulticastSocketClient.GetStreemData().RigidBody;

        //Debug.Log("Numero de rigid: " + OptitrackManagement.DirectMulticastSocketClient.GetStreemData()._nRigidBodies);
        //Debug.Log("Numero de markers: " + OptitrackManagement.DirectMulticastSocketClient.GetStreemData()._nOtherMarkers);

        for (int i = 0; i < OptitrackManagement.DirectMulticastSocketClient.GetStreemData().NRigidBodies; i++)
        {
            //Associates the first RigidBody to the PEN
            if (i == 0)
            {
                if (rigidBodies[i].RigidBodyGameObject == null && PenObject != null)
                {
                    //instanciate the pen prefab
                    rigidBodies[i].RigidBodyGameObject = (GameObject)Instantiate(PenObject, Vector3.Scale(rigidBodies[i].Pos, new Vector3(-1, 1, 1)), rigidBodies[i].Ori);
                    // set rigidbody to the first one
                   // rigidBodies[i].RigidBodyGameObject.GetComponent<PenDrawerScript>().setOptiTrackRigidbody(rigidBodies[i]);
                }
                else
                {
                    ApplyOptiTrackTransformToObject(rigidBodies[i], WorldScaleOffset);
                }
            }
            //Associates the second RigidBody to the Camera
            else if (i == 1)
            {
                if (rigidBodies[i].RigidBodyGameObject == null && RigidBodyObject != null)
                {
                   rigidBodies[i].RigidBodyGameObject = (GameObject)Instantiate(RigidBodyObject, Vector3.Scale(rigidBodies[i].Pos, new Vector3(-1, 1, 1)), rigidBodies[i].Ori);
              
                    this.transform.position = rigidBodies[i].RigidBodyGameObject.transform.position;
                    //this.transform.rotation = Quaternion.Inverse(rigidBodies[i].RigidBodyGameObject.transform.rotation);
                    this.transform.right = rigidBodies[i].RigidBodyGameObject.transform.right;
                    this.transform.forward = -rigidBodies[i].RigidBodyGameObject.transform.forward;
                    this.transform.parent = rigidBodies[i].RigidBodyGameObject.transform;
                }
                else
                {
                    ApplyOptiTrackTransformToObject(rigidBodies[i], CameraOffset + WorldScaleOffset);
                    //RiftCamController.SetYRotation(rigidBodies[i].RigidBodyGameObject.transform.eulerAngles.y);
                    //RiftCamController.SetOrientationOffset(rigidBodies[i].RigidBodyGameObject.transform.rotation);
                }
            }
            //Associates the third RigidBody to the Menu
            else if (i == 2)
            {
                if (rigidBodies[i].RigidBodyGameObject == null && MenuObject != null)
                {
                    rigidBodies[i].RigidBodyGameObject = (GameObject)Instantiate(MenuObject, Vector3.Scale(rigidBodies[i].Pos, new Vector3(-1, 1, 1)), rigidBodies[i].Ori);

                    if (MenuObject != null)
                    {
                        rigidBodies[i].RigidBodyGameObject.transform.position = rigidBodies[i].RigidBodyGameObject.transform.position + new Vector3(0, 3, 0);
                        rigidBodies[i].RigidBodyGameObject.transform.rotation = Quaternion.identity;
                    }
                }
                else
                {
                    ApplyOptiTrackTransformToObject(rigidBodies[i], new Vector3(0.0f, 2.0f, 0.0f) + WorldScaleOffset);
                }
            }
            //Associates the rest RigidBodies
			else
			{
                if (rigidBodies[i].RigidBodyGameObject == null && RigidBodyObject != null)
                {
					rigidBodies[i].RigidBodyGameObject = (GameObject)Instantiate(RigidBodyObject, Vector3.Scale(rigidBodies[i].Pos, new Vector3(-1, 1, 1)), rigidBodies[i].Ori);
              
				}
				else
                {
                    ApplyOptiTrackTransformToObject(rigidBodies[i], new Vector3(0.0f, 1.0f, 0.0f) + WorldScaleOffset);
                    Debug.DrawRay(rigidBodies[i].RigidBodyGameObject.transform.position, rigidBodies[i].RigidBodyGameObject.transform.right, Color.blue);
                    Debug.DrawRay(rigidBodies[i].RigidBodyGameObject.transform.position, rigidBodies[i].RigidBodyGameObject.transform.up, Color.green);
                    Debug.DrawRay(rigidBodies[i].RigidBodyGameObject.transform.position, rigidBodies[i].RigidBodyGameObject.transform.forward, Color.red);
				}
			}
        }
    }

    void ApplyOptiTrackTransformToObject(OptitrackManagement.RigidBody rigidBody, Vector3 offsetPos) 
    {
        rigidBody.RigidBodyGameObject.transform.position = Vector3.Scale(rigidBody.Pos, new Vector3(-1, 1, 1)) * OptiTrackPosMultiplyer + offsetPos; ;
		//rigidBody.RigidBodyGameObject.transform.localPosition += offsetPos;
        //Debug.Log("Cube Pos: " + rigidBodies[i].RigidBodyGameObject.transform.position + " devia estar: " + rigidBodies[i].pos * optiTrackPosMultiplyer);
        rigidBody.RigidBodyGameObject.transform.rotation = Quaternion.Inverse(rigidBody.Ori);
    }

    //void OnGUI()
    //{
    //    GUI.Label(new Rect(10, 25, 500, 20), "OptiDraw: " + message_to_show);
    //}
}
