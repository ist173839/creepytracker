using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
public static class MyDebug
{
    private static readonly bool DebugActive = true;
    public static void Log(object message, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.Log(message);
    #endif
    }

    public static void Log(object message, GameObject context, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.Log(message, context);
    #endif
    }

    public static void LogWarning(object message, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.LogWarning(message);
    #endif
    }
    public static void LogWarning(object message, GameObject context, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.LogWarning(message, context);
    #endif
    }

    public static void LogError(object message, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.LogError(message);
    #endif
    }
    public static void LogError(object message, GameObject context, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.LogWarning(message, context);
    #endif
    }

    public static void DrawLine(Vector3 start, Vector3 end, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawLine(start, end);
    #endif
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawLine(start, end, color);
    #endif
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawLine(start, end, color, duration);
    #endif
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest, bool show)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawLine(start, end, color, duration, depthTest);
    #endif
    }
    
    public static void DrawRay(Vector3 start, Vector3 dir, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawRay(start, dir);
    #endif
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawRay(start, dir, color);
    #endif
    }

    public static void DrawRay(Vector3 start, Vector3 dir, Color color, float duration, bool show = false)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawRay(start, dir, color, duration);
    #endif
    }

    public static void DrawRay(Vector3 start, Vector3 end, Color color, float duration, bool depthTest, bool show)
    {
    #if UNITY_EDITOR
        if (DebugActive || show) Debug.DrawRay(start, end, color, duration, depthTest);
    #endif
    }
}
