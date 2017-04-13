﻿/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
    public enum Coordenadas
    {
        X,
        Y,
        Z
    }

// ReSharper disable once CheckNamespace
public static class MathHelper
{
    public static Vector3 AddValueToVector(Vector3 s, float valueToAdd)
    {
        return new Vector3(s.x + valueToAdd, s.y + valueToAdd, s.z + valueToAdd);
    }

    public static float SignedDistancePlanePoint(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
    {

        return Vector3.Dot(planeNormal, (point - planePoint));
    }
    
    public static float PlaneRayIntersection(Vector3 planePosition, Vector3 planeNormal, Vector3 rayPosition, Vector3 rayForward)
    {
        //A plane can be defined as:
        //a point representing how far the plane is from the world origin
        var p_0 = planePosition;
        //a normal (defining the orientation of the plane), should be negative if we are firing the ray from above
        var n = planeNormal;

        //We are intrerested in calculating a point in this plane called p
        //The vector between p and p0 and the normal is always perpendicular: (p - p_0) . n = 0

        //A ray to point p can be defined as: l_0 + l * t = p, where:
        //the origin of the ray
        var l_0 = rayPosition;
        //l is the direction of the ray
        var l = rayForward;
        //t is the length of the ray, which we can get by combining the above equations:
        //t = ((p_0 - l_0) . n) / (l . n)

        //But there's a chance that the line doesn't intersect with the plane, and we can check this by first
        //calculating the denominator and see if it's not small. 
        //We are also checking that the denominator is positive or we are looking in the opposite direction
        var denominator = Vector3.Dot(l, n);

        if (denominator > 0.00001f)
        {
            //The distance to the plane
            float t = Vector3.Dot(p_0 - l_0, n) / denominator;

            //Where the ray intersects with a plane
            Vector3 p = l_0 + l * t;

            return p.magnitude;
            //Display the ray with a line renderer
        }
        return 0.0f;
    }
    
    public static double BoneLength(Vector3 p1, Vector3 p2)
    {
        return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2) + Math.Pow(p1.z - p2.z, 2));
    }

    // ReSharper disable once UnusedMember.Global
    public static double BoneLength(params Vector3[] joints)
    {
        double length = 0;

        for (var index = 0; index < joints.Length - 1; index++)
        {
            length += BoneLength(joints[index], joints[index + 1]);
        }
        return length;
    }
    
    // ReSharper disable once UnusedMember.Global
    public static Vector3 ConvertUnityVector3(Vector3 vector3)
    {
        return new Vector3(-1 * vector3.x, vector3.y, vector3.z);
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 CheckMidAngles(Vector3 right, Vector3 left)
    {
        var res = Vector3.zero;
         
        var angle1 = Vector3.Angle( right,  left);
        var angle2 = Vector3.Angle( right, -left);
        var angle3 = Vector3.Angle(-right,  left);
        var angle4 = Vector3.Angle(-right, -left);

             if (angle1 <= angle2 && angle1 <= angle3 && angle1 <= angle4) res = MidPoint(right, left);
        else if (angle2 <= angle1 && angle2 <= angle3 && angle2 <= angle4) res = MidPoint(right, -left);
        else if (angle3 <= angle1 && angle3 <= angle2 && angle3 <= angle4) res = MidPoint(-right, left);
        else if (angle4 <= angle1 && angle4 <= angle3 && angle4 <= angle2) res = MidPoint(-right, -left);


        return res;
    }

    public static Vector3 DeslocamentoHorizontal(Vector3 vector3)
    {
        return new Vector3(vector3.x, 0.0f, vector3.z);
    }
    
    public static Vector3 DeslocamentoHorizontal(Vector3 vector3, float y)
    {
        return new Vector3(vector3.x, y, vector3.z);
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 DeslocamentoHorizontalAdd(Vector3 vector3, float y)
    {
        return new Vector3(vector3.x, vector3.y + y, vector3.z);
    }

    public static Vector2 GetVector2FromVector3(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector2 GetVector2FromVector3(Vector3 vector3, Coordenadas semCoord)
    {
        switch (semCoord)
        {
            case Coordenadas.X:
                return new Vector2(vector3.y, vector3.z);
            case Coordenadas.Y:
                return new Vector2(vector3.x, vector3.z);
            case Coordenadas.Z:
                return new Vector2(vector3.x, vector3.y);
            default:
                throw new ArgumentOutOfRangeException("semCoord", semCoord, null);
        }
    }

    // ReSharper disable once UnusedMember.Global
    public static float GetDeltaFromDateTimeInSeconds(DateTime date)
    {
        return (float)(DateTime.Now - date).TotalSeconds;
    }
    
    public static float LinearInterpolation(float x, float x0, float y0, float x1, float y1)
    {
        return Math.Abs(y0 + (y1 - y0) * ((x - x0) / (x1 - x0)));
    }
    
    // ReSharper disable once UnusedMember.Global
    public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        var side1 = b - a;
        var side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    // ReSharper disable once UnusedMember.Global
    public static float SmallestDifferenceBetweenTwoAngles(float sourceAngle, float targetAngle)
    {
        var delta = targetAngle - sourceAngle;
             if (delta >  MathConstants.MATH_PI) delta -= 360;
        else if (delta < -MathConstants.MATH_PI) delta += 360;
        return delta;
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 ConvertOrientationToVector(float orientation)
    {
        return new Vector3((float)Math.Sin(orientation), 0, (float)Math.Cos(orientation));
    }

    // ReSharper disable once UnusedMember.Global
    public static float ConvertVectorToOrientation(Vector3 vector)
    {
        return Mathf.Atan2(vector.x, vector.z);
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 Rotate2D(Vector3 vector, float angle)
    {
        var sin = (float) Math.Sin(angle);
        var cos = (float) Math.Cos(angle);

        var x = vector.x * cos - vector.z * sin;
        var z = vector.x * sin + vector.z * cos;

        return new Vector3(x,vector.y,z);
    }

    public static Vector3 MidPoint(Vector3 v1, Vector3 v2)
    {
        return (v1 + v2) / 2;
    }

    /// <summary>
    /// Returns the closest param (a value between 0 and 1) in the line segment to a given point. 
    /// algorithm based on the algorithm to get the minimum distance between a point and a line segment
    /// http://geomalgorithms.com/a02-_lines.html#Distance-to-Ray-or-Segment
    /// </summary>
    /// <param name="line1P0">Start point of Line Segment</param>
    /// <param name="line1P1">End point of Line segment</param>
    /// <param name="targetPoint">The point to which we want to find the closest param</param>
    /// <returns></returns>
    public static float ClosestParamInLineSegmentToPoint(Vector3 line1P0, Vector3 line1P1, Vector3 targetPoint)
    {
        var v = line1P1 - line1P0;
        var w = targetPoint - line1P0;

        var c1 = Vector3.Dot(w, v);
        if (c1 <= 0)
            return 0;

        var c2 = v.sqrMagnitude;
        if (c2 <= c1)
            return 1;

        return  c1 / c2;
    }
    
    /// <summary>
    /// Returns the point in Line segment2 that is closest to Line Segment 1
    /// algorithm based on the algorithm to get the minimum distance between 2 line segments
    /// http://geomalgorithms.com/a07-_distance.html
    /// </summary>
    /// <param name="line1P0">Start point of Line Segment 1</param>
    /// <param name="line1P1">End point of Line segment 1</param>
    /// <param name="line2P0">Start point of Line Segment 2</param>
    /// <param name="line2P1">End point of Line Segment 2</param>
    /// <param name="parallelTieBreaker">this point is used to select the closest point when the two line segments are pararell. In this situation, the method will return the closest line2P0/line2P1 to the tiebreaker</param>
    /// <returns></returns>
    public static Vector3 ClosestPointInLineSegment2ToLineSegment1(Vector3 line1P0, Vector3 line1P1, Vector3 line2P0, Vector3 line2P1, Vector3 parallelTieBreaker)
    {
        var u = line1P1 - line1P0;
        var v = line2P1 - line2P0;
        var w = line1P0 - line2P0;

        var a = u.sqrMagnitude;
        var b = Vector3.Dot(u, v);
        var c = v.sqrMagnitude;
        var d = Vector3.Dot(u, w);
        var e = Vector3.Dot(v, w);

        float sN;
        float tN;
        var D = a*c - b*b;
        var sD = D;
        var tD = D;

        var cosTeta = b/(u.magnitude*v.magnitude);

        if (cosTeta > (1-0.05f)) //the lines are almost parallel
        {
            //paralel line segments
            //we use a distinct method for parallel line segments
            //We will basically select from P0 or P1, the closest to the tiebreaker;
            if ((parallelTieBreaker - line2P0).magnitude < (parallelTieBreaker - line2P1).magnitude)
            {
                return line2P0;
            }
            else
            {
                return line2P1;
            }
        }
        else
        {
            sN = b*e - c*d;
            tN = a*e - b*d;
            if (sN < 0.0f)
            {
                tN = e;
                tD = c;
            }
            else if (sN > sD)
            {
                tN = e + b;
                tD = c;
            }
        }

        if (tN < 0.0f)
        {
            tN = 0.0f;
        }
        else if (tN > tD)
        {
            tN = tD;
        }

        float tC = tN/tD;

        return line2P0 + tC*v;
    }
}

