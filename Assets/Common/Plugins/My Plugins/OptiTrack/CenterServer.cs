using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class CenterServer
{


    private OptitrackManager _localOptitrackManager;
    private UdpBroadcast _udpBroadcast;

    private TrackerUI _localTrackerUi;

    //  private Transform _parent;

    private GameObject _indicadores;
    private GameObject _centroGameObject;

    private bool _setUpCentro;

    private int _indicadorCounter;
    private int _port;

    private Vector3 _centro;



    public CenterServer()
    {
        _port = 53839;
        _udpBroadcast = new UdpBroadcast(_port);


        //_localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
    }


    public void SetCenterOptiTrack()
    {
            if (_localOptitrackManager != null)
            {
                _setUpCentro = true;
                _centroGameObject.transform.position =
                    _centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
            }
        
        _centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
        if (_setUpCentro)
        {
            var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
                           MessageSeparators.L2;

            _udpBroadcast.Send(mensagem);
        }
    }
}
