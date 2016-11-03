/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
***************************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public enum VirtualWorld
{
    World1SoLimites,
    World2Chair,
    World3Cube,
    World4Sphere,
    World5Signal
}

// ReSharper disable once CheckNamespace
public struct WorldIndicator
{
    public Vector3 Position;
    public Vector3 Scale;
    public float Raio;
    public Quaternion Rotation;
    public IndicatorType Type;
    public MaterialType Material;
    public RepresentationMode Representation;
    public float DistToCenter;
    public Direction? DirectionLimit;
    public IndicatorVisualModeInUse VisualMode;
}

// ReSharper disable once CheckNamespace
public static class IndicatorSetUp
{
    public const float ObjectY   = -1.7f;
    public const float SphereY   = -1.2f;
    public const float CuboY     = -1.2f;
    public const float ObjSinalY = -1.6f;

    public static Vector3 SphereScale   = new Vector3(0.50f, 1.00f, 0.50f);
    public static Vector3 CubeScale     = new Vector3(0.50f, 0.60f, 0.50f);
    public static Vector3 ObjSinalScale = new Vector3(0.25f, 0.30f, 0.25f);

    public static Quaternion SphereRotation      = Quaternion.Euler( 00.00f, 00.00f, 00.00f);
    public static Quaternion CubeRotation        = Quaternion.Euler( 00.00f, 00.00f, 00.00f);
    public static Quaternion ObjectSinalRotation = Quaternion.Euler( 00.00f, 00.00f, 00.00f);
    public static Quaternion ObjectRotation      = Quaternion.Euler(-90.00f, 00.00f, 00.00f);
    
    public static List<WorldIndicator> GetVirtualWorldIndicators(VirtualWorld virtualWorld = VirtualWorld.World1SoLimites)
    {
        switch (virtualWorld)
        {
            case VirtualWorld.World1SoLimites:
                return GetWorld1Indicator();
            case VirtualWorld.World2Chair:
                return GetWorld2Indicator(); 
            case VirtualWorld.World3Cube:
                return GetWorld3Indicator();
            case VirtualWorld.World4Sphere:
                return GetWorld4Indicator();
            case VirtualWorld.World5Signal:
                return GetWorld5Indicator();
            default:
                throw new ArgumentOutOfRangeException("virtualWorld", virtualWorld, null);
        }
    }
 
    private static List<WorldIndicator> GetWorld1Indicator()
    {
        return new List<WorldIndicator>
        {
            new WorldIndicator
            {
                // Right
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                 // Behind
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            }
        };
    }

    private static List<WorldIndicator> GetWorld2Indicator()
    {
        return new List<WorldIndicator>
        {
            new WorldIndicator
            {
                // Right
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                // Behind
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            },
            ///////////////////////////////////////////////////////////
            new WorldIndicator
            {
                Position = new Vector3(-0.8f, -1.7f,  0.9f), Scale = Vector3.one, Rotation = Quaternion.Euler(-90.0f, 00.0f, 00.0f),
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.ObjectChair, Raio = 1f, DirectionLimit = null,
            },
            new WorldIndicator
            {
                Position = new Vector3(-0.9f, -1.7f, -0.9f), Scale = Vector3.one, Rotation = Quaternion.Euler(-90.0f, 00.0f, 00.0f),
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.ObjectChair, Raio = 1f, DirectionLimit = null,
            }
        };
    }

    private static List<WorldIndicator> GetWorld3Indicator()
    {
        return new List<WorldIndicator>
        {
            new WorldIndicator
            {
                // Right
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                // Behind
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            },
            ///////////////////////////////////////////////////////////
            new WorldIndicator
            {
                Position = new Vector3(-0.8f, CuboY,  0.9f), Scale = CubeScale, Rotation = CubeRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.Cube, Raio = 1f, DirectionLimit = null,
            },
            new WorldIndicator
            {
                Position = new Vector3(-0.9f, CuboY, -0.9f), Scale = CubeScale, Rotation = CubeRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.Cube, Raio = 1f, DirectionLimit = null,
            }
        };
    }

    private static List<WorldIndicator> GetWorld4Indicator()
    {
        return new List<WorldIndicator>
        {
            new WorldIndicator
            {
                // Right
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                // Behind
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            },
            ///////////////////////////////////////////////////////////
            new WorldIndicator
            {
                Position = new Vector3(-0.8f, SphereY,  0.9f), Scale = SphereScale, Rotation = SphereRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.Sphere, Raio = 1f, DirectionLimit = null,
            },
            new WorldIndicator
            {
                Position = new Vector3(-0.9f, SphereY, -0.9f), Scale = SphereScale, Rotation = SphereRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.Sphere, Raio = 1f, DirectionLimit = null,
            }
        };
    }

    private static List<WorldIndicator> GetWorld5Indicator()
    {
        return new List<WorldIndicator>
        {
            new WorldIndicator
            {
                // Right
                Position = new Vector3(1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Right, DistToCenter = 1.5f
            },
            new WorldIndicator
            {
                // Left
                Position = new Vector3(-1.5f, 0.0f, 0.0f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f),
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Left, DistToCenter = -1.5f
            },
            new WorldIndicator
            {
                // Front
                Position = new Vector3(0.0f, 0.0f, 1.2f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, 90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Front, DistToCenter = 1.2f
            },
            new WorldIndicator
            {
                // Behind
                Position = new Vector3(0.0f, 0.0f, -1.8f), Scale = new Vector3(0.5f, 1.0f, 0.5f), Rotation = Quaternion.Euler(0.0f, -90.0f, -90.0f), 
                Type = IndicatorType.Limit, Material = MaterialType.Transparent, Raio = 0.5f, DirectionLimit = Direction.Behind, DistToCenter = -1.8f
            },
            ///////////////////////////////////////////////////////////
            new WorldIndicator
            {
                Position = new Vector3(-0.8f, ObjSinalY,  0.9f), Scale = Vector3.one, Rotation = ObjectSinalRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.ObjectSinal, Raio = 1f, DirectionLimit = null,
            },
            new WorldIndicator
            {
                Position = new Vector3(-0.9f, ObjSinalY, -0.9f), Scale = Vector3.one, Rotation = ObjectSinalRotation,
                Type = IndicatorType.Obstacle, Material = MaterialType.Transparent, Representation = RepresentationMode.ObjectSinal, Raio = 1f, DirectionLimit = null,
            }
        };
    }
}

