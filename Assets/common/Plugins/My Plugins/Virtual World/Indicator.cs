/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
***************************************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
public class Indicator
{
    private readonly RepresentationMode _representation;

    public IndicatorRelativePosition PositionMode;

    public RelativePosition RelativePosition;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // public FadeObjectInOut FadeObjectInOutComponent;
    
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // public IndicatorInfo Info;

    public readonly IndicatorType Type;

    public IndicatorMode Mode;

    public DangerMode IndicatorDangerMode;

    public OutlineMode IndicatorOutlineMode;

    public IndicatorVisualModeInUse IndicatorVisualMode;

    public Direction? DirectionLimit;

    public GameObject ObjectIndicator;
    public GameObject ObjectIndicatorOutline;

    private readonly Transform _parent;

    private DateTime _inicio;
    private DateTime _relogio;

    public readonly string Name;

    public readonly int Id;

    private int _iColorFactor;
    private int _iEmission;

    public readonly float Raio;
    public float DistanceToUser;

    private const float MarkTimeLimit = 30;

    private float _colorFactor;
    private float _emission;
    private float _tempoMuda;

    private readonly Vector3 _scale;

    public Vector3 PreviousPosition { get; private set; }
    public Vector3 PositionCenter   { get; private set; }
    public Vector3 Position         { get; private set; }

    public Vector3 UserPosition;

    public bool IsMark { get;  private set; }
    
    public bool IsPreWarning;
    public bool IsDanger;
    public bool IsEmbate;


    private readonly Quaternion _rotation;

    public Indicator(Vector3 position, Vector3 scale, Quaternion rotation, IndicatorType type, Vector3 positionCenter, Transform parent, int id, RepresentationMode representation, float raio, Direction? direction = null)
    {
        Name = "Indicator " + type + " " + id;
        Mode = IndicatorMode.Normal;
        Type = type;

        Id   = id;

        DirectionLimit = direction;

        IsPreWarning = IsMark = false;

        RelativePosition = RelativePosition.Front;
        PositionMode     = IndicatorRelativePosition.Front;

        PreviousPosition = Position = position;
        PositionCenter = positionCenter;

        _representation = representation;
        
        _rotation = rotation;
        _parent = parent;
        _scale = scale;
 
        Raio = raio;
        
        CreateIndicator(type);
        
       // FadeObjectInOutComponent = ObjectIndicator.AddComponent<FadeObjectInOut>();
        
        //#if UNITY_EDITOR
        //    Info = ObjectIndicator.AddComponent<IndicatorInfo>();
        //    Info.ThisIndicator = this;
        //#endif

        _colorFactor = 0.0f;
        _emission    = 0.5f;

        _iColorFactor = 1;
        _iEmission    = 1;

        _relogio = DateTime.Now;
        _tempoMuda = 0.1f;
        IsDanger = false;
        //  ClosestDistanceToObstacle = Single.PositiveInfinity;
        // IsAlerted = false;

        IsEmbate = false;
    }
    
    public void EnableMesh(bool enabled)
    {
        ObjectIndicator.GetComponent<MeshRenderer>().enabled = enabled;

       // MyDebug.Log("IndicatorsOutlineMode = " + IndicatorsOutlineMode);

        for (var i = 0; i < ObjectIndicator.transform.childCount; i++)
        {
            var child = ObjectIndicator.transform.GetChild(i);
            if (child.GetComponent<MeshRenderer>() == null) continue;

            switch (IndicatorOutlineMode)
            {
                case OutlineMode.WhenEnabled:
                    child.GetComponent<MeshRenderer>().enabled = enabled;
                    break;
                case OutlineMode.Always:
                    child.GetComponent<MeshRenderer>().enabled = true;
                    break;
                case OutlineMode.NoOutline:
                    child.GetComponent<MeshRenderer>().enabled = false;
                    break;
                //case OutlineMode.NotUse:
                //    child.GetComponent<MeshRenderer>().enabled = false;
                //    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    //public void CheckMark()
    //{
    //    if (Mode == IndicatorMode.Normal && IsMark)
    //    { 
    //        MyDebug.Log("CheckMark in use");
    //        switch (FadeObjectInOutComponent.TheFadeStatus)
    //        {
    //            case FadeStatus.IsOff:
    //                FadeObjectInOutComponent.Fade();
    //                var newColor = Color.white;
    //                newColor.a = 0.5f;
    //                ObjectIndicator.GetComponent<Renderer>().material.color = newColor;
    //                break;
    //            case FadeStatus.IsIn:
    //                var diff = (DateTime.Now - _inicio).Seconds;
    //                if (diff > MarkTimeLimit)
    //                {
    //                    FadeObjectInOutComponent.Fade();
    //                    IsMark = false;
    //                }
    //                break;
    //            default:
    //                throw new ArgumentOutOfRangeException();
    //        }
    //    }
    //    if (Mode != IndicatorMode.Normal) IsMark = false;
    //}

    public void SetIsMark(bool m)
    {
        if (m) _inicio = DateTime.Now;

        IsMark = m;
    }

    public void UpdateMode()
    {
        switch (Mode)
        {
            case IndicatorMode.Normal:
                //ObjectIndicator.GetComponent<MeshRenderer>().enabled = false;
                EnableMesh(false);
                break;
            case IndicatorMode.Warning:
                //ObjectIndicator.GetComponent<MeshRenderer>().enabled = true;
                EnableMesh(true);
                var newColor = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.5f);
                ObjectIndicator.GetComponent<Renderer>().material.color = newColor;
                ObjectIndicator.GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor * 0.5f);
                //  Color.yellow;
                break;
            case IndicatorMode.Danger:
                //ObjectIndicator.GetComponent<MeshRenderer>().enabled = true;
                //ObjectIndicator.GetComponent<Renderer>().material.color = Color.red;
                LetsGetDangerous();

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateMode(Color? c)
    {
        if (c == null) ObjectIndicator.GetComponent<MeshRenderer>().enabled = false;
        else
        {
            ObjectIndicator.GetComponent<MeshRenderer>().enabled = true;
            ObjectIndicator.GetComponent<Renderer>().material.color = c.Value;
        }
    }

    public void LockPreviousPosition()
    {
        ObjectIndicator.transform.position = PreviousPosition;
    }

    public void AddDesvioToIndicator(Vector3 desvio)
    {
        ObjectIndicator.transform.position += desvio;
    }

    public void SetPreviousPosition()
    {
        PreviousPosition = ObjectIndicator.transform.position;
    }

    public void SetPreviousPosition(Vector3 desvio)
    {
        // ObjectIndicator.transform.position = _previousPosition + desvio;
        ObjectIndicator.transform.position += desvio; // + _position;
        PreviousPosition = ObjectIndicator.transform.position;
    }

    public void SetNewCenter(Vector3 center)
    {
        PositionCenter = MathHelper.DeslocamentoHorizontal(center, PositionCenter.y) ;

        var y = ObjectIndicator.transform.position.y;
        ObjectIndicator.transform.position = PreviousPosition = MathHelper.DeslocamentoHorizontal(center + Position, y);
    }

    public void LetsGetDangerous()
    {
        Color newColor;
        //ObjectIndicator.GetComponent<MeshRenderer>().enabled = true;
        EnableMesh(true);
        switch (IndicatorDangerMode)
        {
            case DangerMode.Blink:
              //  MyDebug.Log("Blink");
                GetEmissionAndFactor();

                //newColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
                newColor = new Color(1.0f, _colorFactor, _colorFactor, 0.9f);
                ObjectIndicator.GetComponent<Renderer>().material.color = newColor;
                ObjectIndicator.GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor * _emission);
                break;
            case DangerMode.DontBlink:
               // MyDebug.Log("Don't Blink");
                newColor = new Color(1.0f, 0.0f, 0.0f, 0.9f);
                ObjectIndicator.GetComponent<Renderer>().material.color = newColor;
                ObjectIndicator.GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor * 0.5f);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CreateIndicator(IndicatorType type)
    {
        switch (type)
        {
            case IndicatorType.Limit:
                CreateLimit();
                break;
            case IndicatorType.Obstacle:
                CreateObstacles();
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }

    private void CreateLimit()
    {
        var newPos = PositionCenter + Position;
        newPos.y = 10 * 0.5f * 0.5f;
        var transparentMaterial = (Material) Object.Instantiate(Resources.Load("Materials/Mt_Transparent"));
        ObjectIndicator = GameObjectHelper.MyCreatePrimitiveObject(PrimitiveType.Plane, Name, _parent, newPos, _scale, _rotation, transparentMaterial);
    }
    
    private void CreateObstacles()
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
    
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    private void GetEmissionAndFactor()
    {
        var delta = (float)(DateTime.Now - _relogio).TotalSeconds;
        if (delta < _tempoMuda) return;

        GetEmission();
        GetColorFactor();

        _relogio = DateTime.Now;
    }

    private void GetEmission()
    {
        _emission += 0.01f * _iEmission;
        if (!(_emission >= 1) && !(_emission <= 0)) return;
        if (_emission >= 1) _emission = 1;
        if (_emission <= 0) _emission = 0;
        _iEmission *= -1;
    }

    private void GetColorFactor()
    {
        const float limSup = 0.5f;
        const float limInf = 0.0f;
        
        _colorFactor += 0.01f * _iColorFactor;
        if (!(_colorFactor >= limSup) && !(_colorFactor <= limInf)) return;
        if (_colorFactor >= limSup) _colorFactor = limSup;
        if (_colorFactor <= limInf) _colorFactor = limInf;
        _iColorFactor *= -1;
    }
}

