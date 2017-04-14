using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
public enum Side
{
    Right,
    Left
}

public enum OtherKnee
{
    Mean,
    Close
}

public struct KneesInfo
{
    public int IdHuman;
    public string IdBody;
    public Vector3 Pos;
    public bool Track;
}

public enum VersaoTeste
{
    NoSaCwip,
    AvrCwip,
    MinimapCwip,
}

public enum RealWorldAssistance
{
    AVR,
    Minimap,
}

public enum WalkState
{
    Direct,
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

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public static class MiscellaneousHelper
{
    

}

