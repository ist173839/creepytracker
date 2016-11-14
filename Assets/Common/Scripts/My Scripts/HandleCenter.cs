using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;


// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class HandleCenter : MonoBehaviour
{
    public enum Side
    {
        Right,
        Left,
        Front,
        Behind,
    }


    public List<GameObject> IndicatorsList;
    public List<GameObject> ObstacleList;

    private OptitrackManager _localOptitrackManager;

    private UdpBroadcast _udpBroadcast;

    private TrackerUI _localTrackerUi;

  //  private Transform _parent;

    private GameObject _indicadores;

    private GameObject _helpers;

    private GameObject _centroGameObject;
    private GameObject _forwardGameObject;

    public bool ShowIndicator;

    private bool _setUpCentro;
    private bool _setUpForward;
    private bool _reset;

#pragma warning disable 169
    private int _indicadorCounter;
#pragma warning restore 169
    private int _port;

    private int _countId;

    private Vector3 _forwardPoint;
    private Vector3 _forward;
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


        IndicatorsList = new List<GameObject>();
        ObstacleList   = new List<GameObject>();


        _helpers = new GameObject { name = "Helpers" };
        _helpers.transform.position = transform.position;
        _helpers.transform.parent   = transform;

        _indicadores = new GameObject { name = "Indicators" };
        _indicadores.transform.position = transform.position;
        _indicadores.transform.parent   = _helpers.transform;


        _centroGameObject  = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, "Centro", Vector3.zero, _helpers.transform, false);
        _centroGameObject.GetComponent<MeshRenderer>().material.color = Color.black;
        _centroGameObject.transform.localScale = new Vector3(0.50f, 0.1f, 0.50f);

        _forwardGameObject = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, "Forward", Vector3.zero, _helpers.transform, false);
        _forwardGameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        _forwardGameObject.transform.localScale = new Vector3(0.50f, 0.1f, 0.50f);

        _reset = false;

        // ShowIndicator = false;
        _countId = 0;
    }
	
	// Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
	void Update ()
	{
	    SetCenterOptiTrack();
	    if (_setUpCentro && _setUpForward && _reset)
	    {
	        _forward = _forwardPoint - _centro;
            _reset = false;
            Debug.DrawLine(_centro, _centro + _forward * 2.0f, Color.white);
        }

	    SetRender(ShowIndicator);

	}

    private void SetCenterOptiTrack()
    {
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    if (_localOptitrackManager != null)
        //    {
        //        _reset = true;
        //        _setUpCentro = true;
        //        _centroGameObject.transform.position =
        //            _centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
        //    }
        //}
        //_centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
        //if (_setUpCentro)
        //{
        //    var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
        //                   MessageSeparators.L2;

        //    _udpBroadcast.Send(mensagem);
        //}
    }

    public void SetCenterOptiTrackButton()
    {
        if (_localOptitrackManager != null)
        {
            _reset = true;
            _setUpCentro = true;
            _centroGameObject.transform.position =
                _centro = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
            SetIndicators(_centro);

        }
       
        _centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
        if (_setUpCentro)
        {
            var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
                           MessageSeparators.L2;

            _udpBroadcast.Send(mensagem);
        }
    }
    
    public void SetForwardPointOptiTrackButton()
    {
        if (_localOptitrackManager != null)
        {
            _reset = true;
            _setUpForward = true;
            _forwardGameObject.transform.position =
                _forwardPoint = MathHelper.DeslocamentoHorizontal(_localOptitrackManager.GetUnityPositionVector());
        }

        _forwardGameObject.GetComponent<MeshRenderer>().enabled = _setUpForward; // && _localTrackerUi.SetUpCenter;
        //if (_setUpForward)
        //{
        //    var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
        //                   MessageSeparators.L2;

        //    _udpBroadcast.Send(mensagem);
        //}
    }

    private void SetIndicators(Vector3 center)
    {
        DestroyAll();
    
        IndicatorsList.Add(CreateLimit(_indicadores.transform, center, Side.Right,  _countId++));
        IndicatorsList.Add(CreateLimit(_indicadores.transform, center, Side.Left,   _countId++));
        IndicatorsList.Add(CreateLimit(_indicadores.transform, center, Side.Front,  _countId++));
        IndicatorsList.Add(CreateLimit(_indicadores.transform, center, Side.Behind, _countId++));

        var obstacle1 = CreateObstacles(_indicadores.transform, center, new Vector3(-0.80f, -0.015f,  0.90f), _countId++);
        var obstacle2 = CreateObstacles(_indicadores.transform, center, new Vector3(-0.90f, -0.015f, -0.90f), _countId++);

        IndicatorsList.Add(obstacle1);
        IndicatorsList.Add(obstacle2);

        ObstacleList.Add(obstacle1);
        ObstacleList.Add(obstacle2);
        
    }

    private void DestroyAll()
    {
        if (IndicatorsList.Count == 0) return;

        foreach (var indicator in IndicatorsList) Destroy(indicator);

        IndicatorsList = null;
        ObstacleList   = null;

        IndicatorsList = new List<GameObject>();
    }

    private void SetRender(bool render)
    {
        if (IndicatorsList.Count == 0) return;

        foreach (var indicator in IndicatorsList)
            indicator.GetComponent<MeshRenderer>().enabled = render;

        foreach (var obstacle in ObstacleList)
            for (var i = 0; i < obstacle.transform.childCount; i++)
                obstacle.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = render;

    }


    private static GameObject CreateLimit(Transform parent, Vector3 center, Side side, int id)
    {
        Quaternion rotation;
        Vector3 position;
        
        switch (side)
        {
            case Side.Right:
                rotation = Quaternion.Euler(0.00f,   0.00f,  90.00f);
                position = new Vector3( 1.5f, 0.0f, 0.0f);
                break;
            case Side.Left:
                rotation = Quaternion.Euler(0.00f,   0.00f, -90.00f);
                position = new Vector3(-1.5f, 0.0f, 0.0f);
                break;
            case Side.Front:
                rotation = Quaternion.Euler(0.00f,  90.00f, -90.00f);
                position = new Vector3(0.0f, 0.0f, 1.2f);
                break;
            case Side.Behind:
                rotation = Quaternion.Euler(0.00f, -90.00f, -90.00f);
                position = new Vector3(0.0f, 0.0f, -1.8f);
                break;
            default:
                throw new ArgumentOutOfRangeException("side", side, null);
        }

        var transparentMaterial = (Material)Object.Instantiate(Resources.Load("Materials/Mt_Transparent"));

        var indicatorName = "Indicator " + side + " " + id;

        var scale = new Vector3(0.5f, 1.0f, 0.5f);

        var newPos = center + position;

        newPos.y = 10 * 0.5f * 0.5f;

        var color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        return GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Plane, indicatorName, parent, newPos, scale, rotation, transparentMaterial, color, false);
    }


    private static GameObject CreateObstacles(Transform parent, Vector3 center, Vector3 position, int id)
    {
        var newPos = center + position;

        var indicatorName = "Indicator " + "Chair " + " " + id;

        var rotation = Quaternion.Euler(-90.0f, 00.0f, 00.0f);

        var scale = Vector3.one;

        var transparentMaterial = (Material) Object.Instantiate(Resources.Load("Materials/Mt_Transparent"));

        var outlineMaterial = (Material) Object.Instantiate(Resources.Load("Materials/Mt_Outline"));
  
        var chairGameObject = Object.Instantiate(Resources.Load("Prefabs/chair", typeof(GameObject))) as GameObject;

        chairGameObject = GameObjectHelper.ResizeMesh(chairGameObject, 0.015f, true);

        var color = new Color(1.0f, 1.0f, 1.0f, 0.5f);

        var objectIndicator = GameObjectHelper.MyCreateObject(chairGameObject, indicatorName, parent, newPos, scale, rotation, transparentMaterial, color, false);
        //var outline         = GameObjectHelper.MyCreateObject(Object.Instantiate(objectIndicator), indicatorName + " Outline", objectIndicator.transform, newPos, scale, rotation, outlineMaterial);

        //outline.GetComponent<MeshRenderer>().enabled = false;
        
        return objectIndicator;

    }



}

/*

 * 
 * 
 * 
 * 
 * 
 
    Position = new Vector3(-0.8f, -1.7f,  0.9f),
    Position = new Vector3(-0.9f, -1.7f, -0.9f),

 * 
 * 
 * 
 *  private void CreateObstacles()
    {
        var newPos = PositionCenter + Position;
        var transparentMaterial = (Material) Object.Instantiate(Resources.Load("Materials/Mt_Transparent"));

        var outlineMaterial     = (Material) Object.Instantiate(Resources.Load("Materials/Mt_Outline"));
        //var outlineScale = MathHelper.AddValueToVector(_scale, 0.1f);
        var outlineScale = new Vector3(1.0f, 1.0f, 1.0f);

        switch (_representation)
        {
            case RepresentationMode.Sphere:
                //ObjectIndicator        = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, Name,              _parent,                   newPos, _scale,       _rotation, transparentMaterial);
                ObjectIndicator        = GameObjectHelper.MyCreatePrimitiveObjectMesh(PrimitiveType.Sphere, Name,              _parent,                   newPos, _scale,       _rotation, transparentMaterial);
                ObjectIndicatorOutline = GameObjectHelper.MyCreatePrimitiveObjectMesh(PrimitiveType.Sphere, Name + " Outline", ObjectIndicator.transform, newPos, _scale, _rotation, outlineMaterial);

                // CreateObstaclePrimitiveSphere();
                // CreateObstaclePrimitiveSphereOutline();
                break;
            case RepresentationMode.Cube:

                ObjectIndicator        = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Cube, Name,              _parent,                   newPos, _scale,       _rotation, transparentMaterial);
                ObjectIndicatorOutline = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Cube, Name + " Outline", ObjectIndicator.transform, newPos, _scale, _rotation, outlineMaterial);


                // CreateObstaclePrimitiveCube();
                // CreateObstaclePrimitiveCubeOutline();
                break;
            case RepresentationMode.ObjectChair:

                var chairGameObject = Object.Instantiate(Resources.Load("Prefabs/chair", typeof(GameObject))) as GameObject;
                chairGameObject = GameObjectHelper.ResizeMesh(chairGameObject, 0.015f, true);

                ObjectIndicator        = GameObjectHelper.MyCreateObject(chairGameObject, Name,              _parent,                   newPos, _scale,       _rotation, transparentMaterial);
                ObjectIndicatorOutline = GameObjectHelper.MyCreateObject(Object.Instantiate(ObjectIndicator), Name + " Outline", ObjectIndicator.transform, newPos, _scale, _rotation, outlineMaterial);
                //ObjectIndicatorOutline.transform.localScale = Vector3.one;
                // CreateObstaclePrimitiveObject();
                // CreateObstaclePrimitiveObjectOutline();
                break;
            case RepresentationMode.ObjectSinal:
                var sinalGameObject = Object.Instantiate(Resources.Load("Prefabs/ObjSinal", typeof(GameObject))) as GameObject;
                sinalGameObject = GameObjectHelper.ResizeMesh(sinalGameObject, 0.25f, true);

                ObjectIndicator = GameObjectHelper.MyCreateObject(sinalGameObject, Name, _parent, newPos, _scale, _rotation, transparentMaterial);
                ObjectIndicatorOutline = GameObjectHelper.MyCreateObject(Object.Instantiate(ObjectIndicator), Name + " Outline", ObjectIndicator.transform, newPos, _scale, _rotation, outlineMaterial);
                //ObjectIndicatorOutline.transform.localScale = Vector3.one;

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        EnableMesh(false);
    }
    
 * 
 * 
 * new WorldIndicator
            {
                // Right  Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f)
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left  Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f)
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front  Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f)
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                 // Behind Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f)
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            }
 * 
     
     */

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
