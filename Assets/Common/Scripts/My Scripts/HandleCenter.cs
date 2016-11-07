using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class HandleCenter : MonoBehaviour
{
    public List<Indicator> IndicatorsList;

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


        IndicatorsList = new List<Indicator>();


        _indicadores = new GameObject { name = "Indicators" };
        _indicadores.transform.position = transform.position;
        _indicadores.transform.parent = transform;



    }
	
	// Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
	void Update ()
	{
	    SetCenterOptiTrack();
	}

    private void SetCenterOptiTrack()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_localOptitrackManager != null)
            {
                _setUpCentro = true;
                _centroGameObject.transform.position =
                    _centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
            }
        }
        _centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
        if (_setUpCentro)
        {
            var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
                           MessageSeparators.L2;

            _udpBroadcast.Send(mensagem);
        }
    }
    public void SetCenterOptiTrackButton()
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

/*
 * 
 * 
 * 
 * private void CreateWolrd(VirtualWorld virtualWorld)
    {
        //if (worldIndicatorList == null) worldIndicatorList = IndicatorSetUp.GetVirtualWorldIndicators();
        var worldIndicatorList = IndicatorSetUp.GetVirtualWorldIndicators(virtualWorld);

        IndicatorsList = new List<Indicator>();
    

        if (_indicadores.transform.childCount != 0) KillAllChilds(_indicadores);
      
        foreach (var worldIndicator in worldIndicatorList)
        {
            var indicator = new Indicator(worldIndicator.Position, worldIndicator.Scale, worldIndicator.Rotation, worldIndicator.Type,  _centro, _indicadores.transform, _indicadorCounter++, worldIndicator.Representation, worldIndicator.Raio, worldIndicator.DirectionLimit);
            IndicatorsList.Add(indicator); 
        }
    }

    private static void KillAllChilds(GameObject gameObject)
    {
        for (var i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
    }
 * 
 * 
 * 
 * 
 * 
 * 
    "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(Centro) + MessageSeparators.L2 +
     "Use"      + MessageSeparators.SET + _setUpCentro.ToString();

private void CreateWolrd(VirtualWorld virtualWorld)
{
    //if (worldIndicatorList == null) worldIndicatorList = IndicatorSetUp.GetVirtualWorldIndicators();
    var worldIndicatorList = IndicatorSetUp.GetVirtualWorldIndicators(virtualWorld);

    // if (IndicatorsList == null)
    IndicatorsList = new List<Indicator>();
    // if (IndicatorsInfo == null)
    _indicatorsInfo = new Dictionary<string, IndicatorInfoToSave>();

    if (_indicadores.transform.childCount != 0) KillAllChilds(_indicadores);
    _indicadorCounter = 0;
    ObstacleCounter = 0;
    LimitCounter = 0;

    foreach (var worldIndicator in worldIndicatorList)
    {
        var indicator = new Indicator(worldIndicator.Position, worldIndicator.Scale, worldIndicator.Rotation, worldIndicator.Type, _parent.position, _indicadores.transform, _indicadorCounter++, worldIndicator.Representation, worldIndicator.Raio, IndicatorsDangerMode, IndicatorsOutlineMode, worldIndicator.DirectionLimit);
        IndicatorsList.Add(indicator); //   
        if (worldIndicator.Type == IndicatorType.Obstacle)
        {
            ObstacleCounter++;
            ObstaclesNames.Add(indicator.Name);
        }
        if (worldIndicator.Type == IndicatorType.Limit) LimitCounter++;

        IndicatorsNames.Add(indicator.Name);
        _indicatorsInfo.Add(indicator.Name, new IndicatorInfoToSave
        {
            Name = indicator.Name,
            Dist = null,
            State = null,
            IsEmbate = false,
            numEmbate = 0,
        });
    }
}

 */
