using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class HandleCenter : MonoBehaviour
{
    // ReSharper disable once MemberCanBePrivate.Global
    public enum Side
    {
        Right,
        Left,
        Front,
        Behind,
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public List<GameObject> IndicatorsList;
    // ReSharper disable once MemberCanBePrivate.Global
    public List<GameObject> ObstacleList;

    private OptitrackManager _localOptitrackManager;

    private UdpBroadcast _udpBroadcast;
    
    private TrackerUI _localTrackerUi;

    private Tracker _localTracker;

    private SaveCenter _saveCenter;

  //  private Transform _parent;

    private GameObject _forwardGameObject;
    private GameObject _centroGameObject;

    private GameObject _indicadores;
    private GameObject _helpers;

    public bool IsSaveFilePossible;
    public bool ShowIndicator;
    public bool Force;
    public bool UseOpti;
    public bool Send;

    private bool _setUpForward;
    private bool _saveForward;

    private bool _setUpCentro;
    private bool _saveCentro;
    private bool _reset;
    
    public int Index;
    
    private int _indicadorCounter;
    private int _countId;
    private int _port;

    private Vector3 _forward;

    private Vector3? _forwardPoint;
    private Vector3? _centro;

    private string _mensagem;
    private string _mens;
    private string _path;
    
    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Start ()
    {
        _port = 53839;
        _udpBroadcast = new UdpBroadcast(_port);
      
        _localOptitrackManager = gameObject.GetComponent<OptitrackManager>();
        _localTrackerUi        = gameObject.GetComponent<TrackerUI>();

        _localTracker = gameObject.GetComponent<Tracker>();

        _saveCenter = new SaveCenter();

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


        // ShowIndicator = false;
        _countId = 0;
      
        _path = System.IO.Directory.GetCurrentDirectory() + "\\" + "Files To Use" + "\\" + "Center Data";

        if (!System.IO.Directory.Exists(_path)) System.IO.Directory.CreateDirectory(_path);

        _centro = new Vector3?();
        Index = 0;

        _mens = "";
        _mensagem = "";

        _saveForward = false;
        _saveCentro  = false;
        _reset       = false;
        Force        = false;
        Send         = true;
        UseOpti      = true;
    }
	
	// Update is called once per frame
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Update ()
	{
       
	    if (_setUpCentro && _centro.HasValue && _setUpForward  && _forwardPoint.HasValue && _reset)
        {
	        _forward = _forwardPoint.Value - _centro.Value;
            _reset = false;
            Debug.DrawLine(_centro.Value, _centro.Value + _forward * 2.0f, Color.white);
        }

        SetRender(ShowIndicator);
        
	    if (_setUpForward && _forwardPoint.HasValue)
	    {
           _indicadores.transform.rotation = Quaternion.LookRotation(_forward);
        }

	    SaveMensagem();
        SendMensagem();

        _localOptitrackManager.RenderMarker(UseOpti);
        _centroGameObject.GetComponent<MeshRenderer>().enabled  = _setUpCentro;
        _forwardGameObject.GetComponent<MeshRenderer>().enabled = _setUpForward;
    }

    private void SaveMensagem()
    {
        if (_setUpCentro && _centro.HasValue && !_saveCentro)
        {
            var center = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro.Value);
            _saveCenter.RecordMessage(center);
            _saveCentro = true;
        }

        if (_setUpForward && _forwardPoint.HasValue && !_saveForward)
        {
            var forward = "ForwardPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_forwardPoint.Value);
            _saveCenter.RecordMessage(forward);
            _saveForward = true;
        }

        if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
        {
            _saveCenter.StopRecording();
        }

    }

    private void SendMensagem()
    {
        var mensagem = "";
       // var mens = "";

        if (_setUpCentro && _centro.HasValue)
        {
            var center = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro.Value);
            var humans = _localTracker.GetHumans();
            var mensaHumans = "";
            foreach (var h in humans)
            {
                var position = _centro.Value - h.Value.Position;

                mensaHumans += MessageSeparators.L2;
                mensaHumans += "Id"     + MessageSeparators.SET + h.Value.ID;
                mensaHumans += MessageSeparators.L4; // "Desvio" + MessageSeparators.SET +
                mensaHumans += CommonUtils.convertVectorToStringRPC(position);
            }

            mensagem = center + mensaHumans;
            // mens = center;
            // _saveCenter.RecordMessage(center);
        }


        if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
        {
            mensagem += MessageSeparators.L2;
            // mens     += MessageSeparators.L2;
            // _saveCenter.RecordMessage("" + MessageSeparators.L2);
        }

        if (_setUpForward && _forwardPoint.HasValue)
        {
            var forward = "ForwardPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_forwardPoint.Value);
            mensagem += forward;
            // mens += forward;
            // _saveCenter.RecordMessage(forward);
        }
        
        mensagem += MessageSeparators.L2 + "Force" + MessageSeparators.SET + Force;
        
        //_mensagem = mensagem;

        if (Send) _udpBroadcast.Send(mensagem);
    }

    public void SetCenterOptiTrackButton()
    {
        if (_localOptitrackManager != null && UseOpti) SetUpNewCenter(_localOptitrackManager.GetUnityPositionVector());
        // _centroGameObject.GetComponent<MeshRenderer>().enabled = _setUpCentro; // && _localTrackerUi.SetUpCenter;
    }

    private void SetUpNewCenter(Vector3 newCenter)
    {
        _reset = true;
        _setUpCentro = true;
        _centro = MathHelper.DeslocamentoHorizontal(newCenter);
        _centroGameObject.transform.position = _centro.Value;
        SetIndicators(_centro.Value);
    }

    public void SetForwardPointOptiTrackButton()
    {
        if (_localOptitrackManager != null && UseOpti) SetUpNewForward(_localOptitrackManager.GetUnityPositionVector());
    }

    private void SetUpNewForward(Vector3 newForward)
    {
        _reset = true;
        _setUpForward = true;
        _forwardPoint = MathHelper.DeslocamentoHorizontal(newForward);
        _forwardGameObject.transform.position = _forwardPoint.Value;
    }

    public void SetSaveFilesButton()
    {
        var info = new DirectoryInfo(_path);
        var fileInfo = info.GetFiles();

        var length = fileInfo.Length;
        if (length == 0) return;

        var file = fileInfo[0];
        //foreach (var file in fileInfo)
        {
            var fs =  file.Open(FileMode.Open);
            using (var bs = new BufferedStream(fs))
            using (var sr = new StreamReader(bs))
            {
                string line;
                int l = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    var lineText = line.Split(MessageSeparators.SET);
                    switch (lineText[0])
                    {
                        case "CenterPos":
                            //  _centro       = CommonUtils.ConvertRpcStringToVector3(lineText[1]);
                            SetUpNewCenter(CommonUtils.ConvertRpcStringToVector3(lineText[1]));
                            MyDebug.Log("CenterPos = " + _centro);
                        break;
                        case "ForwardPos":
                            //_forwardPoint = CommonUtils.ConvertRpcStringToVector3(lineText[1]);
                            SetUpNewForward(CommonUtils.ConvertRpcStringToVector3(lineText[1]));
                            break;
                        default:
                        break;
                    } //  MyDebug.Log("Print : " + l++ + ",  " + line);
                }
            }
        }
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

    public void Reset()
    {
        DestroyAll();
        
        _centroGameObject.GetComponent<MeshRenderer>().enabled  = false;
        _forwardGameObject.GetComponent<MeshRenderer>().enabled = false;
        
        _reset = true;
        _setUpForward =  _setUpCentro = false;
        _forwardPoint = null;
        _centro       = null;
    }
    
    private void DestroyAll()
    {
        if (IndicatorsList.Count == 0) return;

        foreach (var indicator in IndicatorsList) Destroy(indicator);

        IndicatorsList = null;
        ObstacleList   = null;

        IndicatorsList = new List<GameObject>();
        ObstacleList   = new List<GameObject>();
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
      
        return objectIndicator;

    }

}

/*  
 *  
 *  
   //CheckIfFileToUse();
        
    
        //{
        //    Index = 0;
        //    //_centro = null;
        //    //_forwardPoint = null;
        //}
        //else if (length >= Index)
        //{
        //    Index = length - 1;
        //}
        



 *  
 *  
     private static Vector3? GetCenterFromFile(string line)
    {
        var l = "";
        if (line.Contains("/"))
        {
            var lines = line.Split(MessageSeparators.L2);
            foreach (var ls in lines) if (ls.Contains("CenterPos")) l = ls;
        }
        else l = line;

        var lineText = l.Split(MessageSeparators.SET);
        if (lineText[0] == "CenterPos") return CommonUtils.ConvertRpcStringToVector3(lineText[1]);

        return null;
    }
                    // _centro = GetCenterFromFile(line);
   // allText += line; 
    private bool CheckIfFileToUse()
    {
        var info = new DirectoryInfo(_path);
        var fileInfo = info.GetFiles();

        var length = fileInfo.Length;
        return IsSaveFilePossible = length != 0;
    }



 * 
 *  
 *  
        //if (_mens != mens)
        //{
        //    _saveCenter.RecordMessage(mens);
        //    _mens = mens;
        //    //if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
        //    //{
        //    //    _saveCenter.StopRecording();
        //    //}
        //}


    // Files To Use
    // _path = System.IO.Directory.GetCurrentDirectory() + "\\" +  "Saved Files" + "\\" + "Center Data";

    
    //  _indicadores.transform.rotation.


    private void SendMensagem()
    {
        var mensagem = "";
        var mens = "";

        if (_setUpCentro && _centro.HasValue)
        {
            var center = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro.Value);
            var humans = _localTracker.GetHumans();
            var mensaHumans = "";
            foreach (var h in humans)
            {
                var position = _centro.Value - h.Value.Position;

                mensaHumans += MessageSeparators.L2;
                mensaHumans += "Id"     + MessageSeparators.SET + h.Value.ID;
                mensaHumans += MessageSeparators.L4; // "Desvio" + MessageSeparators.SET +
                mensaHumans += CommonUtils.convertVectorToStringRPC(position);
            }

            mensagem = center + mensaHumans;
            mens = center;
            _saveCenter.RecordMessage(center);
        }


        if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
        {
            mensagem += MessageSeparators.L2;
            mens     += MessageSeparators.L2;
            // _saveCenter.RecordMessage("" + MessageSeparators.L2);
        }

        if (_setUpForward && _forwardPoint.HasValue)
        {
            var forward = "ForwardPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_forwardPoint.Value);
            mensagem += forward;
            mens += forward;
           // _saveCenter.RecordMessage(forward);
        }

        if (_mens != mens)
        {
            _saveCenter.RecordMessage(mens);
            _mens = mens;
            if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
            {
                _saveCenter.StopRecording();
            }
        }

        _udpBroadcast.Send(mensagem);

        
    }

        //if (_setUpCentro && _centro.HasValue && _setUpForward && _forwardPoint.HasValue)
        //{
        //    //mensagem += MessageSeparators.L2;
        //    //mens += MessageSeparators.L2;
        //    // _saveCenter.RecordMessage("" + MessageSeparators.L2);
        //}

 * 
   //var outline         = GameObjectHelper.MyCreateObject(Object.Instantiate(objectIndicator), indicatorName + " Outline", objectIndicator.transform, newPos, scale, rotation, outlineMaterial);

        //outline.GetComponent<MeshRenderer>().enabled = false;


 * 
 * 
 * 
// && _localTrackerUi.SetUpCenter;
        //if (_setUpForward)
        //{
        //    var mensagem = "CenterPos" + MessageSeparators.SET + CommonUtils.convertVectorToStringRPC(_centro) +
        //                   MessageSeparators.L2;

        //    _udpBroadcast.Send(mensagem);
        //}
 *
     var l2 = (string) MessageSeparators.L2;
 
        // Vector3 StringToVector3([NotNull] string text, char separador)
        // return  // float.Parse( a.Replace(",", "."));
        
         line = sr.ReadLine()) != null)
            {
                iLine++;
                // Debug.Log("kr = " + line.Split(';')[16] + ", kl = " + line.Split(';')[17]);
                 char[] del = { ';' };
                if (!line.Contains("Registo"))
                {
                  
       

    SetCenterOptiTrack();


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
