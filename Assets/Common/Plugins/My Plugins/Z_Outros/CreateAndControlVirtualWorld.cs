/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
***************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.Utility;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum VersaoTeste
{
    NoSaCwip,
    AvrCwip,
    MinimapCwip,
}

public enum RealWorldAssistance
{
    // ReSharper disable once InconsistentNaming
    AVR,
    Minimap,
}

public enum WalkState
{
    Direct,
    // ReSharper disable once InconsistentNaming
    WIP,
    Estacionario
}

public enum WipMode
{
    Normal,
    NormalKalman,
    Event,
    EventKalman,
}

public enum IndicatorMode
{
    Normal,
    Warning,
    PreWarning,
    Danger,
}

public enum IndicatorRelativePosition
{
    Front,
    RightBehind,
    LeftBehind,
    Behind,
}

public enum Direction
{
    Front,
    Right,
    Left,
    Behind,
}

public enum IndicatorType
{
    Limit,
    Obstacle,
}

public enum MaterialType
{
    Transparent
}

public enum RepresentationMode
{
    Sphere,
    Cube,
    ObjectChair,
    ObjectSinal,
}

public enum IndicatorVisualModeInUse
{
    Discreto,
    Relativo
}

public enum RelativePosition
{
    Front,
    Behind,
}

public enum UiControloType
{
    Up,
    Right,
    Left,
}

public enum UiControloImage
{
    Fundo,
    Setas,
}

public enum DangerMode
{
    Blink,
    DontBlink,
}

public enum OutlineMode
{
    NoOutline,
    WhenEnabled,
    Always,
    // NotUse,
}

// ReSharper disable once CheckNamespace
//public enum VirtualWorld
//{
//    World1SoLimites,
//    World2Chair,
//    World3Cube,
//    World4Sphere,
//    World5Signal
//}

public struct IndicatorInfoToSave
{
    public string Name;
    public float? Dist;
    public IndicatorMode? State;
    public bool IsEmbate;
    public int numEmbate;
}

// ReSharper disable once CheckNamespace
public class CreateAndControlVirtualWorld
{
    private Dictionary<string, IndicatorInfoToSave> _indicatorsInfo;

    public List<string> ObstaclesNames;
    public List<string> IndicatorsNames;
    public List<Indicator> IndicatorsList;
   
    public IndicatorVisualModeInUse IndicatorVisualMode;

    public VirtualWorld WhichVirtualWorldToCreate { get; private set; }

    public DangerMode IndicatorsDangerMode;

    public OutlineMode IndicatorsOutlineMode;

    // public AudioClip Alert;

    private Transform _parent;

    private GameObject _indicadores;

    public Vector3 Center;

    public Vector3 Desvio;
    
    public Vector3 nextPosFrente;
    public Vector3 nextPosBehind;
    public Vector3 nextPosRight;
    public Vector3 nextPosLeft;

    public int LimitCounter;
    public int ObstacleCounter;

    private int _indicadorCounter;
    private int _iEmission;
    private int _iColorFactor;

    public float BehindDist; // = float.PositiveInfinity;
    public float RightDist;  // = float.PositiveInfinity;
    public float LeftDist;   // = float.PositiveInfinity;
    public float FrontDist;  // = float.PositiveInfinity;

    public float DistanciaPerigo;
    public float DistanciaAviso;
    public float DistanciaPreAviso;
     
    private float _emission;
    private float _colorFactor;

    private float _foV = 62.00f;//49.0f;
    private float _angleA = 31.00f;//24.5f;

    private const float AngleB = 90.0f + 45.0f;
    private const float AngleC = 180.0f;

    private const float PlaneSizeConstante = 10 * 0.5f * 0.5f;
    private const float DistanciaInicial = 2.0f;// Para Testes 
    
    private readonly float _distanciaEmbate;

    private readonly float _distanciaPerigoFrente;
    private readonly float _distanciaAvisoFrente;
    private readonly float _distanciaPreAvisoFrente;

    private readonly float _distanciaPerigoAtras;
    private readonly float _distanciaAvisoAtras ;
    private readonly float _distanciaPreAvisoAtras;
    private readonly float _raioMarker;

    public bool IsRightBehind;
    public bool IsLeftBehind;
    public bool IsBehind;
    public bool IsFront;

    public bool IsInDanger;

    public bool UseDirection;
    public bool UseFrenteTras;
    public bool UseAlert;
    public bool UseAlertOne;
 
    public bool CheckIndicators;

    // ReSharper disable once InconsistentNaming
    public CreateAndControlVirtualWorld(VirtualWorld virtualWorld, Transform transformAVR, Vector3 player, float distanciaPerigoFrente, float distanciaAvisoFrente, float distanciaPreAvisoFrente, float distanciaPerigoAtras, float distanciaAvisoAtras, float distanciaPreAvisoAtras, float distanciaEmbate, float raioMarker)
    {
        _indicatorsInfo = new Dictionary<string, IndicatorInfoToSave>();
       // _indicatorsInfo = new List< IndicatorInfoToSave>();

        IndicatorsList  = new  List<Indicator>();
        ObstaclesNames  = new List<string>();
        IndicatorsNames = new List<string>();
        //Infos = new List<AvrInfo>();



        WhichVirtualWorldToCreate = virtualWorld;
        _indicadorCounter = 0;

        ObstacleCounter   = 0;
        LimitCounter      = 0;
        
        _parent = transformAVR;

        _distanciaEmbate = distanciaEmbate;

        _distanciaPerigoFrente   = distanciaPerigoFrente;
        _distanciaAvisoFrente    = distanciaAvisoFrente;
        _distanciaPreAvisoFrente = distanciaPreAvisoFrente;
        
        _distanciaPerigoAtras   = distanciaPerigoAtras;
        _distanciaAvisoAtras    = distanciaAvisoAtras;
        _distanciaPreAvisoAtras = distanciaPreAvisoAtras;
        
        BehindDist = float.PositiveInfinity;
        RightDist  = float.PositiveInfinity;
        LeftDist   = float.PositiveInfinity;
        FrontDist  = float.PositiveInfinity;

        _raioMarker = raioMarker;
       // IndicatorsDangerMode = IndicatorsDangerMode.Blink;

        _indicadores = new GameObject { name = "Indicators" };
        _indicadores.transform.position = transformAVR.position;
        _indicadores.transform.parent   = transformAVR;
        Center = _parent.position; // Vector3.zero;// 
        Desvio = Vector3.zero;

        //IndicatorVisualMode = IndicatorVisualModeInUse.Relativo;

        IsFront = IsBehind = IsLeftBehind = IsRightBehind = false;

      //  CreateWolrd(IndicatorSetUp.GetVirtualWorldIndicators(virtualWorld));
        CreateWolrd(virtualWorld);

        _colorFactor = 0.0f;
        _emission    = 0.5f;

        _iColorFactor = 1;
        _iEmission    = 1;

       // _isAlertPlay = false;
        UseAlert = false;
        UseAlertOne = false;
    }

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
        ObstacleCounter   = 0;
        LimitCounter      = 0;

        foreach (var worldIndicator in worldIndicatorList)
        {
            var indicator = new Indicator(worldIndicator.Position, worldIndicator.Scale, worldIndicator.Rotation, worldIndicator.Type, _parent.position, _indicadores.transform, _indicadorCounter++, worldIndicator.Representation, worldIndicator.Raio, worldIndicator.DirectionLimit);
            IndicatorsList.Add(indicator); //   
            if (worldIndicator.Type == IndicatorType.Obstacle)
            {
                ObstacleCounter++;
                ObstaclesNames.Add(indicator.Name);
            }
            if (worldIndicator.Type == IndicatorType.Limit)    LimitCounter++;
            
            IndicatorsNames.Add(indicator.Name);
            _indicatorsInfo.Add(indicator.Name, new IndicatorInfoToSave
            {
                Name      = indicator.Name,
                Dist      = null,
                State     = null,
                IsEmbate  = false,
                numEmbate =  0,
            });
        }
    }

    public void RecreateWorld(VirtualWorld virtualWorld)
    {
        var virtualWorldIndicators = IndicatorSetUp.GetVirtualWorldIndicators(virtualWorld);
        // if (IndicatorsList == null)
        IndicatorsList = new List<Indicator>();
        if (_indicadores.transform.childCount != 0) KillAllChilds(_indicadores);

        _indicadorCounter = 0;

        ObstacleCounter   = 0;
        LimitCounter      = 0;

        ObstaclesNames  = new List<string>();
        IndicatorsNames = new List<string>();
        _indicatorsInfo = new Dictionary<string, IndicatorInfoToSave>();
        
        foreach (var worldIndicator in virtualWorldIndicators)
        {
            var indicator = new Indicator(worldIndicator.Position, worldIndicator.Scale, worldIndicator.Rotation, worldIndicator.Type, _parent.position, _indicadores.transform, _indicadorCounter++, worldIndicator.Representation, worldIndicator.Raio, worldIndicator.DirectionLimit);
            IndicatorsList.Add(indicator);
            if (worldIndicator.Type == IndicatorType.Obstacle)
            {
                ObstacleCounter++;
                ObstaclesNames.Add(indicator.Name);
            }
            if (worldIndicator.Type == IndicatorType.Limit) LimitCounter++;
 
            IndicatorsNames.Add(indicator.Name);
            _indicatorsInfo.Add(indicator.Name, new IndicatorInfoToSave
            {
                Name      = indicator.Name,
                Dist      = null,
                State     = null,
                IsEmbate  = false, 
                numEmbate = 0,
            });
        }
    }

    private static void KillAllChilds(GameObject gameObject)
    {
        for (var i = 0; i < gameObject.transform.childCount; i++)
        {
            Object.Destroy(gameObject.transform.GetChild(i).gameObject);
        }
    }

    public List<Indicator> GetIndicators()
    {
        if (IndicatorsList == null || IndicatorsList.Count == 0)
            return null;
        return IndicatorsList;
    }
    public Indicator GetIndicatorsByType(IndicatorType type)
    {
        if (IndicatorsList == null || IndicatorsList.Count == 0)
            return null;
        return IndicatorsList.FirstOrDefault(ind => ind.Type == type);
    }
    public Indicator GetIndicatorsLimitByDirection(Direction direction)
    {
        if (IndicatorsList == null || IndicatorsList.Count == 0)
            return null;
        return IndicatorsList.FirstOrDefault(ind => ind.Type == IndicatorType.Limit && ind.DirectionLimit == direction);
    }

    public Vector3 GetIndicatorRelativePosition(Vector3 mainPlayerCameraPosition)
    {
        var frontIndicator = GetIndicatorsLimitByDirection(Direction.Front);
        var rigthIndicator = GetIndicatorsLimitByDirection(Direction.Right);

        var frontPos = frontIndicator.ObjectIndicator.transform.position;
        var rigthPos = rigthIndicator.ObjectIndicator.transform.position;
        
        var posIndZ = (mainPlayerCameraPosition  - frontPos).z;
        var posIndX = (mainPlayerCameraPosition  - rigthPos).x;

        return new Vector3(posIndX, 0.0f, posIndZ);
    }

    public void UpdateFoV(float newFov)
    {
        _foV = newFov;
        _angleA = _foV/2;
    }

    public void CheckVirtualWorld(Vector3 player, Vector3 directionIsLookingUp, Vector3 directionIsLookingRight, Vector3 directionIsLookingLeft, Vector3 directionMarker, Vector3 cameraPosition, WalkState state)
    {
        //Infos = new List<AvrInfo>();

        var countRightBehind = 0; 
        var countLeftBehind  = 0;
        var countBehind      = 0;

        var behindDist = float.PositiveInfinity;
        var rightDist  = float.PositiveInfinity;
        var leftDist   = float.PositiveInfinity;

        UseAlert = false;

        IsInDanger = false;

        //if (!CheckIndicators)
        //{
        //    SetAllToNormal();
        //}

        //var isAlert = false;
        // string teste = "Objectos visiveis :";
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.IndicatorDangerMode  = IndicatorsDangerMode;
            ind.IndicatorOutlineMode = IndicatorsOutlineMode;
            ind.UserPosition         = player;

            // ind.Alert = Alert;
            //if (ind.AudioSourceComponent.isPlaying)
            //{
            //    isAlert = true;
            //}

            //ind.UseOutine = UseOutine;

            /////////// Check if Front or Back

            var indPosition = ind.ObjectIndicator.transform.position;
            var vectorIndicador = MathHelper.DeslocamentoHorizontal(indPosition); // new Vector3(ind.ObjectIndicator.transform.position.x, 0.0f, ind.ObjectIndicator.transform.position.z);
            /*
             
                var tempVect1 = constante + indicator.ObjectIndicator.transform.forward * PlaneSizeConstante;
                    tempVect1.y = 10.0f * 0.5f;

                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(tempVect1, 0.1f);

                    var tempVect2 = constante - indicator.ObjectIndicator.transform.forward * PlaneSizeConstante;
                    tempVect2.y = 10.0f * 0.5f;
             */

            var angleToCenter = Vector3.Angle(directionIsLookingUp, vectorIndicador);

            // MyDebug.Log("Name = " + ind.Name + ", Angle = " + angle + ", Type = 0");
            if (ind.Type == IndicatorType.Obstacle)
            {
                if (angleToCenter >= _angleA)
                {
                    ind.RelativePosition = RelativePosition.Behind;
                }
                else
                {
                    ind.RelativePosition = RelativePosition.Front;
                    ind.PositionMode = IndicatorRelativePosition.Front;
                }
            }
            
            if (ind.Type == IndicatorType.Limit)
            {
                Vector3 planePoint;
                if (ind.DirectionLimit == null) continue;

                float x;
                float z;
                switch (ind.DirectionLimit)
                {
                    case Direction.Front:
                        x = cameraPosition.x;
                        z = indPosition.z;
                        planePoint = new Vector3(x, 0.0f, z);
                        break;
                    case Direction.Right:
                        x = indPosition.x;
                        z = cameraPosition.z;
                        planePoint = new Vector3(x, 0.0f, z);
                        break;
                    case Direction.Left:
                        x = indPosition.x;
                        z = cameraPosition.z;
                        planePoint = new Vector3(x, 0.0f, z);   
                        break;
                    case Direction.Behind:
                        x = cameraPosition.x;
                        z = indPosition.z;
                        planePoint = new Vector3(x, 0.0f, z);
                        break;
                    case null:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                var angleToPlanePoint = Vector3.Angle(directionIsLookingUp, planePoint);
                if (angleToPlanePoint >= _angleA)
                {
                    ind.RelativePosition = RelativePosition.Behind;

                    if (angleToPlanePoint >= AngleB && angleToPlanePoint <= AngleC)
                    {
                        ind.PositionMode = IndicatorRelativePosition.Behind;
                    }
                    else if (angleToPlanePoint >= _angleA && angleToPlanePoint <= AngleB)
                    {
                        var cross = Vector3.Cross(directionIsLookingUp, planePoint);
                        if (cross.y > 0)
                        {
                            ind.PositionMode = IndicatorRelativePosition.RightBehind;
                        }
                        else
                        {
                            ind.PositionMode = IndicatorRelativePosition.LeftBehind;
                        }
                    }
                }
                else
                {
                    ind.RelativePosition = RelativePosition.Front;
                    ind.PositionMode = IndicatorRelativePosition.Front;
                }        
            }

            ////////////////////////////////////////////////////////////////////////
                
            if (UseFrenteTras)
            {
                switch (ind.RelativePosition)
                {
                    case RelativePosition.Front:
                        DistanciaPerigo   = _distanciaPerigoFrente;
                        DistanciaAviso    = _distanciaAvisoFrente;
                        DistanciaPreAviso = _distanciaPreAvisoFrente;
                        break;
                    case RelativePosition.Behind:
                        DistanciaPerigo   = _distanciaPerigoAtras;
                        DistanciaAviso    = _distanciaAvisoAtras;
                        DistanciaPreAviso = _distanciaPreAvisoAtras;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                DistanciaPerigo   = _distanciaPerigoFrente;
                DistanciaAviso    = _distanciaAvisoFrente;
                DistanciaPreAviso = _distanciaPreAvisoFrente;
            }

            /////  Set Distance
            var distance = GetDistPlayerToIndicator(player, ind, DistanciaPerigo, DistanciaAviso, DistanciaPreAviso);
            ind.DistanceToUser = distance;
            ///////////////////

            // var vectorForward = new Vector3(player.x + forward.x, 0.0f, player.z + forward.z);


            ind.Mode = GetMode(IndicatorVisualMode, distance, DistanciaPerigo, DistanciaAviso, DistanciaPreAviso);

            ind.IsEmbate = distance <= _distanciaEmbate;

            ind.IndicatorVisualMode = IndicatorVisualMode;

            //MyDebug.Log("IndicatorVisualMode = " + IndicatorVisualMode);

            switch (IndicatorVisualMode)
            {
                case IndicatorVisualModeInUse.Discreto:
                    ind.UpdateMode();
                    break;
                case IndicatorVisualModeInUse.Relativo:
                    ind.IsPreWarning = distance <= DistanciaPreAviso && distance > DistanciaAviso; 
                    RelativeColor(ind, distance, DistanciaPerigo, DistanciaAviso, DistanciaPreAviso, ind.IsMark);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            ///////////////////////////////////////////////////////////////////////////////////////////  
           
            if ( (ind.Mode != IndicatorMode.Normal || ind.IsMark) && ind.Type == IndicatorType.Obstacle)
            {
                if (angleToCenter >= AngleB && angleToCenter <= AngleC)
                {
                    ind.PositionMode = IndicatorRelativePosition.Behind;
                    countBehind++;
                    if (behindDist > ind.DistanceToUser) behindDist = ind.DistanceToUser;
                }
                else if (angleToCenter >= _angleA && angleToCenter <= AngleB)
                {
                    var cross = Vector3.Cross(directionIsLookingUp, vectorIndicador); 
                    if (cross.y > 0)
                    {
                        ind.PositionMode = IndicatorRelativePosition.RightBehind;
                        countRightBehind++;
                        if (rightDist > ind.DistanceToUser) rightDist = ind.DistanceToUser;
                    }
                    else
                    {
                        ind.PositionMode = IndicatorRelativePosition.LeftBehind;
                        countLeftBehind++;
                        if (leftDist > ind.DistanceToUser) leftDist = ind.DistanceToUser;
                    }
                }
            }

            //if ((ind.Mode != IndicatorMode.Normal || ind.IsMark) && ind.Type == IndicatorType.Limit)
            //{
            //    if (ind.RelativePosition == RelativePosition.Behind)
            //    {
            //        ind.PositionMode = IndicatorRelativePosition.Behind;
            //        countBehind++;
            //    }
            //}

            ////////////////////////////////////////////////
            //CheckAngles(player, forward, ind);   /////
            //DebugGetDistObstacle(player, ind);   /////
            ////////////////////////////////////////////////

            if (UseDirection)
            {
                if (state == WalkState.Direct)
                {
                    var distToMarker = GetDistMarkerToIndicator(directionMarker, ind);
                    ind.SetIsMark(distToMarker <= _raioMarker);

                    //if (ind.Mode == IndicatorMode.Normal && !ind.IsMark && ind.FadeObjectInOutComponent.TheFadeStatus == FadeStatus.IsIn)
                    //{
                    //    ind.FadeObjectInOutComponent.Fade();
                    //}
                }
                //ind.CheckMark();
            }
            
            // ind.ObjectIndicatorOutline.GetComponent<MeshRenderer>().enabled = UseOutine && ind.ObjectIndicator.GetComponent<MeshRenderer>().enabled;


            if (ind.Mode == IndicatorMode.Danger)
            {
                IsInDanger = true;
                UseAlert   = true;
            }

            //if (ind.Mode == IndicatorMode.Danger) {} 

            //if (ind.Mode == IndicatorMode.Danger && !ind.IsAlerted) // && !_isAlertPlay ind.RelativePosition == RelativePosition.Behind
            //{
            //    ind.PlayAlert();
            //    // _isAlertPlay = true;
            //}

            //if (ind.Mode == IndicatorMode.Normal )
            //{
            //    ind.IsAlerted = false;
            //}

            //if (ind.AudioSourceComponent.isPlaying)
            //{
            //}


           // SaveInfo(ind);

            var indName = ind.Name;
            if (_indicatorsInfo.ContainsKey(indName))
            {
                var info = _indicatorsInfo[ind.Name];


                if (!info.IsEmbate && ind.IsEmbate)
                {
                    info.numEmbate++;
                }

                info.Dist     = ind.DistanceToUser;
                info.State    = ind.Mode;
                info.IsEmbate = ind.IsEmbate;

                _indicatorsInfo[indName] = info;
            }

            IndicatorsList[index] = ind;
        }


        IsRightBehind = countRightBehind > 0;
        IsLeftBehind  = countLeftBehind  > 0;
        IsBehind      = countBehind      > 0;

        BehindDist = behindDist;
        RightDist  = rightDist;
        LeftDist   = leftDist;

    }

    public void CheckVirtualWorld(Vector3 player)
    {
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.UserPosition = player;
            var distance = GetDistPlayerToIndicator(player, ind);
            ind.DistanceToUser = distance;

            var embate = distance <= _distanciaEmbate;
            ind.IsEmbate = embate;
            // MyDebug.Log("ind.IsEmbate = " + ind.IsEmbate);

            var indName = ind.Name;
            if (_indicatorsInfo.ContainsKey(indName))
            {
                var info = _indicatorsInfo[ind.Name];
                
                if (!info.IsEmbate && ind.IsEmbate)
                {
                    info.numEmbate++;
                }

                info.Dist     = ind.DistanceToUser;
                info.State    = ind.Mode;
                info.IsEmbate = ind.IsEmbate;

                _indicatorsInfo[indName] = info;
            }
            IndicatorsList[index] = ind;
        }
    }
    
    private void SetUpDistancias(Indicator ind)
    {
        if (UseFrenteTras)
        {
            switch (ind.RelativePosition)
            {
                case RelativePosition.Front:
                    DistanciaPerigo = _distanciaPerigoFrente;
                    DistanciaAviso = _distanciaAvisoFrente;
                    DistanciaPreAviso = _distanciaPreAvisoFrente;
                    break;
                case RelativePosition.Behind:
                    DistanciaPerigo = _distanciaPerigoAtras;
                    DistanciaAviso = _distanciaAvisoAtras;
                    DistanciaPreAviso = _distanciaPreAvisoAtras;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            DistanciaPerigo   = _distanciaPerigoFrente;
            DistanciaAviso    = _distanciaAvisoFrente;
            DistanciaPreAviso = _distanciaPreAvisoFrente;
        }
    }

    public void SetAllToNormal()
    {
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.Mode = IndicatorMode.Normal;
            ind.UpdateMode();
            IndicatorsList[index] = ind;
        }
    }

    private static void CheckAngles(Vector3 player, Vector3 forward, Indicator ind)
    {
        if (ind.Mode != IndicatorMode.Normal && ind.Type == IndicatorType.Obstacle)
        {
            var vectorForward = new Vector3(player.x + forward.x, 0.0f, player.z + forward.z);

            var vectorIndicador = new Vector3(ind.ObjectIndicator.transform.position.x, 0.0f, ind.ObjectIndicator.transform.position.z);
            // var other = ind.ObjectIndicator;
            // Vector3 dir = other.transform.position - player;
            // dir = other.transform.InverseTransformDirection(dir);

            var angle = Math.Abs(Vector3.Angle(forward, vectorIndicador));
           // MyDebug.Log("Name = " + ind.Name + ", Angle = " + angle + ", Type = 0");
        }
    }

    private static IndicatorMode GetMode(IndicatorVisualModeInUse visualMode, float distance, float distanciaPerigo, float distanciaAviso, float distanciaPreAviso)
    {
       
        if (distance <= distanciaPerigo) return IndicatorMode.Danger;

        if (distance <= distanciaAviso) return IndicatorMode.Warning;

        if (visualMode == IndicatorVisualModeInUse.Relativo && distance <= distanciaPreAviso)
            return IndicatorMode.PreWarning;

        return IndicatorMode.Normal;
    }

    private static IndicatorMode GetMode(float distance, float distanciaPerigo, float distanciaAviso)
    {
        return distance <= distanciaPerigo
            ? IndicatorMode.Danger
            : (distance <= distanciaAviso ? IndicatorMode.Warning : IndicatorMode.Normal);
    }

    private static IndicatorMode GetMode(float distance, float distanciaPerigo)
    {
        return distance <= distanciaPerigo ? IndicatorMode.Danger : IndicatorMode.Normal;
    }
    
    public void SetAllIndicators()
    {
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            
            ind.SetPreviousPosition();
            
            IndicatorsList[index] = ind;
        }
    }

    public void LockAllIndicators()
    {
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.LockPreviousPosition();
            IndicatorsList[index] = ind;
        }
    }

    public void SetAllCenter(Vector3 player)
    {
        Center = player;
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.SetNewCenter(player);
            IndicatorsList[index] = ind;
        }
    }

    private static float GetDistPlayerToIndicator(Vector3 player, Indicator ind, float distanciaPerigoToDebug, float distanciaAvisoToDebug, float distanciaPreAvisoToDebug)
    {
        float dist;

        switch (ind.Type)
        {
            case IndicatorType.Limit:
                dist = GetDistPlane(player, ind, distanciaPerigoToDebug, distanciaAvisoToDebug, distanciaPreAvisoToDebug);
                // MyDebug.Log("Limit diff = " + diff + ", Name = " + ind.Name);
                break;
            case IndicatorType.Obstacle:
                dist = GetDistObstacle(player, ind, distanciaPerigoToDebug, distanciaAvisoToDebug); 
                //MyDebug.Log("Obstacle diff = " + diff + ", Name = " + ind.Name);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return dist;
    }
    
    private static float GetDistPlayerToIndicator(Vector3 player, Indicator ind)
    {
        float dist;
        switch (ind.Type)
        {
            case IndicatorType.Limit:
                dist = GetDistPlane(player, ind);
                // MyDebug.Log("Limit diff = " + diff + ", Name = " + ind.Name);
                break;
            case IndicatorType.Obstacle:
                dist = GetDistObstacle(player, ind);
                //MyDebug.Log("Obstacle diff = " + diff + ", Name = " + ind.Name);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return dist;
    }

    private static float GetDistObstacle(Vector3 player, Indicator ind, float distanciaPerigo, float distanciaAviso)
    {
        // new Vector2(player.x, player.z);
        // new Vector2(ind.ObjectIndicator.transform.position.x, ind.ObjectIndicator.transform.position.z);

        var playerV2 = MathHelper.GetVector2FromVector3(player);
        var indV2    = MathHelper.GetVector2FromVector3(ind.ObjectIndicator.transform.position);

        var diff = (playerV2 - indV2).magnitude - ind.Raio*0.5f;

        if (diff < 0) diff = 0;
        // var diff = (player - ind.ObjectIndicator.transform.position).magnitude + ind.Raio * 0.5f;
        // DebugGetDiffObstacle(player, ind);
        return diff;
    }


    private static float GetDistObstacle(Vector3 player, Indicator ind)
    {
        var playerV2 = MathHelper.GetVector2FromVector3(player);
        var indV2 = MathHelper.GetVector2FromVector3(ind.ObjectIndicator.transform.position);
        var diff = (playerV2 - indV2).magnitude - ind.Raio * 0.5f;
        if (diff < 0) diff = 0;
        return diff;
    }

    private static float GetDistPlane(Vector3 player, Indicator ind, float distanciaPerigoToDebug, float distanciaAvisoToDebug, float distanciaPreAvisoToDebug)
    {
        var player2D = new Vector3(player.x, 0.0f, player.z);

        var constante = ind.ObjectIndicator.transform.position;
        var tempVect1 = constante + ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect1.y = 10.0f*0.5f;
        var tempVect2 = constante - ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect2.y = 10.0f*0.5f;
        var tempVect3 = constante + ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect3.y = 00.0f;
        var indPlane = new Plane(tempVect1, tempVect2, tempVect3);

        //indPlane.normal = GetPlaneNormal(player, ind, indPlane);

        //MyDebug.Log("Antes ( " + ind.Id + " ) indPlane.normal =  " + indPlane.normal);
        indPlane.normal = GetPlaneNormal(player2D, ind, indPlane);
        //MyDebug.Log("Depois ( " + ind.Id + " ) indPlane.normal =  " + indPlane.normal);

        DebugHelper.DebugIndicatorPlane(ind, indPlane, distanciaPreAvisoToDebug, distanciaAvisoToDebug, distanciaPerigoToDebug, 5);


        //var diff = CheckDistCheckDiff(player, ind, indPlane);
        var diff = CheckDist(player2D, ind, indPlane);

        return diff;
    }

    private static float GetDistPlane(Vector3 player, Indicator ind)
    {
        var player2D = new Vector3(player.x, 0.0f, player.z);

        var constante = ind.ObjectIndicator.transform.position;
        var tempVect1 = constante + ind.ObjectIndicator.transform.forward * 10 * 0.5f * 0.5f;
        tempVect1.y = 10.0f * 0.5f;
        var tempVect2 = constante - ind.ObjectIndicator.transform.forward * 10 * 0.5f * 0.5f;
        tempVect2.y = 10.0f * 0.5f;
        var tempVect3 = constante + ind.ObjectIndicator.transform.forward * 10 * 0.5f * 0.5f;
        tempVect3.y = 00.0f;
        var indPlane = new Plane(tempVect1, tempVect2, tempVect3);

        indPlane.normal = GetPlaneNormal(player2D, ind, indPlane);

        var diff = CheckDist(player2D, ind, indPlane);

        return diff;
    }
    
    private static float CheckDist(Vector3 player, Indicator ind, Plane indPlane)
    {
        return Math.Abs(MathHelper.SignedDistancePlanePoint(indPlane.normal, ind.ObjectIndicator.transform.position, player));
    }

    private static Vector3 GetPlaneNormal(Vector3 player, Indicator ind, Plane indPlane)
    {
        var normal     = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position + indPlane.normal, 5);
        var normaMinus = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position - indPlane.normal, 5);


        var transformPosition = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position, 5);


        MyDebug.DrawLine(transformPosition, normal,     Color.red);
        MyDebug.DrawLine(transformPosition, normaMinus, Color.black);
        
        
        // MyDebug.Log(" normal = " + indPlane.normal + ", id = " + ind.Id);

        //var diff1 = (player - normal).magnitude;
        //var diff2 = (player - normaMinus).magnitude;

        if (!indPlane.GetSide(player)) indPlane.normal *= -1;

        normal = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position + indPlane.normal, 5);

        MyDebug.DrawLine(transformPosition, normal, Color.white);
        
        // indPlane.normal = diff1 > diff2 ? indPlane.normal : -indPlane.normal;
        
        return indPlane.normal;
    }

    private static Vector3 GetPlaneNormal(Indicator ind, Plane indPlane)
    {
        if (!indPlane.GetSide(ind.PositionCenter)) indPlane.normal *= -1;
        return indPlane.normal;
    }

    private static void RelativeColor(Indicator indicator, float dist, float distanciaPerigo, float distanciaAviso, float distanciaPreAviso, bool isMark) // , int ind Id
    {
        var ind = indicator.ObjectIndicator;
        if (dist > distanciaPreAviso)
        {
            indicator.EnableMesh(false);
            ind.GetComponent<Renderer>().material.color = Color.white;
            ind.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white * 0.5f);
        }

        if (dist <= distanciaPreAviso && dist > distanciaAviso)
        {
            SetCorRelativa(indicator, dist, distanciaPreAviso, distanciaAviso, 0.00f, 0.50f, 0.00f, 1.00f, 0.0f, 0.5f, Color.white, Color.yellow, isMark);
        }
        else if (dist <= distanciaAviso && dist > distanciaPerigo)
        {
            SetCorRelativa(indicator, dist, distanciaAviso, distanciaPerigo, 0.50f, 0.90f, 0.00f, 1.00f,  Color.yellow, Color.red);
        }
        else if (dist <= distanciaPerigo)
        {
            indicator.LetsGetDangerous();
        }
    }
    
    private static void SetCorRelativa(Indicator indicator, float dist, float distanciaInicial, float distanciaFinal, float dtValorInicial, float dtValorFinal, float lrValorInicial, float lrValorFinal, Color corInicial, Color corFinal, bool isMark = false)
    {
        var ind = indicator.ObjectIndicator;
        indicator.EnableMesh(true);
        // ind.GetComponent<MeshRenderer>().enabled = true;

        var dt = MathHelper.LinearInterpolation(dist, distanciaInicial, dtValorInicial, distanciaFinal, dtValorFinal);
        var lr = MathHelper.LinearInterpolation(dist, distanciaInicial, lrValorInicial, distanciaFinal, lrValorFinal);
       
        var newColor = Color.Lerp(corInicial, corFinal, lr);
        // newColor.a = isMark ? ind.GetComponent<Renderer>().material.color.a : dt;
        newColor.a = dt;
        
        //MyDebug.Log("( " + ind.name + " ) "+" Color : r = " + newColor.r + ", g = " + newColor.g + ", b = " + newColor.b + ", a = " + newColor.a);

        ind.GetComponent<Renderer>().material.color = newColor;

        ind.GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor * 0.5f);
    }

    private static void SetCorRelativa(Indicator indicator, float dist, float distanciaInicial, float distanciaFinal, float dtValorInicial, float dtValorFinal, float lrValorInicial, float lrValorFinal, float intInicial, float intFinal, Color corInicial, Color corFinal, bool isMark = false)
    {
        var ind = indicator.ObjectIndicator;

        //ind.GetComponent<MeshRenderer>().enabled = true;
        indicator.EnableMesh(true);

        var dt = MathHelper.LinearInterpolation(dist, distanciaInicial, dtValorInicial, distanciaFinal, dtValorFinal);
        var lr = MathHelper.LinearInterpolation(dist, distanciaInicial, lrValorInicial, distanciaFinal, lrValorFinal);
        var it = MathHelper.LinearInterpolation(dist, distanciaInicial, intInicial,     distanciaFinal, intFinal);
       
        var newColor = Color.Lerp(corInicial, corFinal, lr);
        // newColor.a = isMark ? ind.GetComponent<Renderer>().material.color.a : dt;
        newColor.a =  dt;
        
        ind.GetComponent<Renderer>().material.color = newColor;

        ind.GetComponent<Renderer>().material.SetColor("_EmissionColor", newColor * it);
    }

    public void NotOn()
    {
        for (var index = 0; index < IndicatorsList.Count; index++)
        {
            var ind = IndicatorsList[index];
            ind.Mode = IndicatorMode.Normal;
            ind.UpdateMode();
            IndicatorsList[index] = ind;
        }
    }

    public Dictionary<string, IndicatorInfoToSave> GetIndicatorsInfo()
    {
        return _indicatorsInfo;
    }


    public Vector3 GetIndicatorsPosition()
    {
        return _indicadores.transform.position;
    }

    private static float GetDistMarkerToIndicator(Vector3 marker, Indicator ind)
    {
        float dist;
        switch (ind.Type)
        {
            case IndicatorType.Limit:
                dist = GetDistPlaneMarker(marker, ind);
                // MyDebug.Log("Limit diff = " + diff + ", Name = " + ind.Name);
                break;
            case IndicatorType.Obstacle:
                dist = GetDistObstacleMarker(marker, ind);
                //MyDebug.Log("Obstacle diff = " + diff + ", Name = " + ind.Name);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return dist;
    }

    private static float GetDistObstacleMarker(Vector3 marker, Indicator ind)
    {
        //var markerV2 = new Vector2(marker.x, marker.z);
        //var indV2 = new Vector2(ind.ObjectIndicator.transform.position.x, ind.ObjectIndicator.transform.position.z);

        var markerV2 = MathHelper.GetVector2FromVector3(marker);
        var indV2    = MathHelper.GetVector2FromVector3(ind.ObjectIndicator.transform.position);

        var dist = (markerV2 - indV2).magnitude + ind.Raio*0.5f;
        // var diff = (player - ind.ObjectIndicator.transform.position).magnitude + ind.Raio * 0.5f;

        //DebugGetDiffObstacle(player, ind);
        return dist;
    }

    private static float GetDistPlaneMarker(Vector3 marker, Indicator ind)
    {
        var marker2D = new Vector3(marker.x, 0.0f, marker.z);

        var indPlane = MakeIndPlane(ind);

        //indPlane.normal = GetPlaneNormal(player, ind, indPlane);
        indPlane.normal = GetPlaneNormal(marker2D, ind, indPlane);
        // DebugIndicatorPlane(ind, indPlane, distanciaPreAvisoToDebug, distanciaAvisoToDebug, distanciaPerigoToDebug);

        //var diff = CheckDist(player, ind, indPlane);
        var diff = CheckDist(marker2D, ind, indPlane);

        return diff;
    }

    private static Plane MakeIndPlane(Indicator ind)
    {
        var constante = ind.ObjectIndicator.transform.position;

        var tempVect1 = constante + ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect1.y = 10.0f*0.5f;

        var tempVect2 = constante - ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect2.y = 10.0f*0.5f;

        var tempVect3 = constante + ind.ObjectIndicator.transform.forward*10*0.5f*0.5f;
        tempVect3.y = 00.0f;

        var indPlane = new Plane(tempVect1, tempVect2, tempVect3);
        return indPlane;
    }
}
    /*
 
    // public List<AvrInfo> Infos;
    // private List<GameObject> _indicatorsObjectList;

     
     
     
     
     
     
     */
