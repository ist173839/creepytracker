using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class HandleVirtualWorld : MonoBehaviour
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

    private GameObject _forwardGameObject;
    private GameObject _centroGameObject;

    private GameObject _indicadores;
    private GameObject _helpers;
    private GameObject _marker;

    public bool IsSaveFilePossible;
    public bool CanShowIndicators;
    public bool CanUseSaveFile;
    public bool ShowIndicator;
    public bool ShowMarker;
    public bool CanForce;
    public bool UseOpti;
    public bool Force;
    public bool Send;
    
    private bool _setUpForward;
    private bool _saveForward;
    private bool _saveMessage;
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
        
        _marker = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Sphere, "Marker", Vector3.zero, _helpers.transform, false);
        _marker.GetComponent<MeshRenderer>().material.color = Color.green;
        _marker.transform.localScale = new Vector3(0.50f, 0.50f, 0.50f);

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
      //  ShowOpti     = true;
        UseOpti      = false;
        ShowMarker   = true;
        CanShowIndicators = false;
        CanForce = false;
        
        var info = new DirectoryInfo(_path);
        var fileInfo = info.GetFiles();
        var length = fileInfo.Length;
        _localTrackerUi.UseSaveFile = CanUseSaveFile = length != 0;

        _saveMessage = true;


    }
	
	// Update is called once per frame
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Update ()
	{
	    var length = -1;
	    if (!CanUseSaveFile)
	    {
            var info = new DirectoryInfo(_path);
            var fileInfo = info.GetFiles();
            length = fileInfo.Length;
            CanUseSaveFile = length != 0;
            //    MyDebug.Log(">>> Length = " + length + ", CanUseSaveFile = " + CanUseSaveFile);
        }
       

        if (_setUpCentro)
	    {
	        CanForce = true;
            if (_setUpForward)
	        {
	            CanShowIndicators = true;
	        }
	    }
        
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
        

        _localOptitrackManager.RenderMarker(UseOpti && ShowMarker);
        _marker.GetComponent<MeshRenderer>().enabled = !UseOpti && ShowMarker;
        
        _centroGameObject.GetComponent<MeshRenderer>().enabled  = _setUpCentro;
        _forwardGameObject.GetComponent<MeshRenderer>().enabled = _setUpForward;

	    if (!UseOpti)
	    {
	        var idToCheck  = _localTrackerUi.IdToCheck;
	        var humanCheck = _localTracker.GetHuman(idToCheck);
	        if (humanCheck != null)
	        {
                var pos = MathHelper.DeslocamentoHorizontal(humanCheck.gameObject.transform.position, 0.5f);
                _marker.transform.position = pos;
            }
	    }

      //  MyDebug.Log("Length = " + length + ", CanUseSaveFile = " + CanUseSaveFile);
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
            if (humans != null)
            {
                foreach (var h in humans)
                {
                    var position = _centro.Value - h.Value.Position;

                    mensaHumans += MessageSeparators.L2;
                    mensaHumans += "Id" + MessageSeparators.SET + h.Value.ID;
                    mensaHumans += MessageSeparators.L4; // "Desvio" + MessageSeparators.SET +
                    mensaHumans += CommonUtils.convertVectorToStringRPC(position);
                }

                mensagem = center + mensaHumans;
                // mens = center;
                // _saveCenter.RecordMessage(center);
            }
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

    public void SetCenterButton()
    {
        if (UseOpti)
        {
            if (_localOptitrackManager != null) SetUpNewCenter(_localOptitrackManager.GetUnityPositionVector());
        }
        else
        {
           // var human = _localTracker.GetHuman(_localTrackerUi.IdToCheck);
            var pos = MathHelper.DeslocamentoHorizontal(_marker.transform.position, 0.0f);
            SetUpNewCenter(pos);
            
        }

        _saveMessage = true;
 
        //  if (_localOptitrackManager != null ) SetUpNewCenter(_localOptitrackManager.GetUnityPositionVector());
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

    public void SetForwardPointButton()
    {
        if (UseOpti)
        {
            if (_localOptitrackManager != null)
            {
                SetUpNewForward(_localOptitrackManager.GetUnityPositionVector());
            }
        }
        else
        {
            // var human = _localTracker.GetHuman(_localTrackerUi.IdToCheck);
            var pos = MathHelper.DeslocamentoHorizontal(_marker.transform.position, 0.0f);
            SetUpNewForward(pos);
        }

        _saveMessage = true;
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
        CanUseSaveFile = length != 0;
        
        if (length == 0) return;

        var file = fileInfo[0];
        //foreach (var file in fileInfo)
        {
            var fs =  file.Open(FileMode.Open);
            using (var bs = new BufferedStream(fs))
            using (var sr = new StreamReader(bs))
            {
                string line;
               // int l = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    var lineText = line.Split(MessageSeparators.SET);
                    switch (lineText[0])
                    {
                        case "CenterPos":
                            //  _centro       = CommonUtils.ConvertRpcStringToVector3(lineText[1]);
                            SetUpNewCenter(CommonUtils.ConvertRpcStringToVector3(lineText[1]));
                            // MyDebug.Log("CenterPos = " + _centro);
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
        _saveMessage = false; 
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

    public void ResetWorld()
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
                position = new Vector3(0.0f, 0.0f, 1.5f);
                break;
            case Side.Behind:
                rotation = Quaternion.Euler(0.00f, -90.00f, -90.00f);
                position = new Vector3(0.0f, 0.0f, -1.5f);
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
