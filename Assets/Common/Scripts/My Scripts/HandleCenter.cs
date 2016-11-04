using UnityEngine;
using System.Collections;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class HandleCenter : MonoBehaviour
{
    private OptitrackManager _localOptitrackManager;

    private UdpBroadcast _udpBroadcast;

    private TrackerUI _localTrackerUi;

    private GameObject _centroGameObject;

    private bool _setUpCentro;

    private int _port;

    private Vector3 _centro;
    
    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start ()
    {
        _port = 53839;
        _udpBroadcast = new UdpBroadcast(_port);
      
        _localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
        _localTrackerUi        = gameObject.GetComponent<TrackerUI>();


        _centroGameObject = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, "Centro", Vector3.zero, transform, false);
        _centroGameObject.transform.localScale = new Vector3(1.0f, 0.1f, 1.0f);


    }
	
	// Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
	void Update ()
    {

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_localOptitrackManager != null)
            {
                _setUpCentro = true;
                _centroGameObject.transform.position = _centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
                

            }
        }
        _centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
        if (_setUpCentro)
        {
            var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) + MessageSeparators.L2;

            _udpBroadcast.Send(mensagem);
        }
    }
}

    /*
        "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(Centro) + MessageSeparators.L2 +
         "Use"      + MessageSeparators.SET + _setUpCentro.ToString();
     
     
     */
