using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class CommonUtils
{
    internal const int DecimalsRound = 3;

    internal static Vector3 _convertToVector3(Kinect.CameraSpacePoint p)
    {
        return new Vector3(p.X, p.Y, p.Z);
    }

    internal static string ConvertVectorToStringRPC(Vector3 v)
    {
        return "" + Math.Round(v.x, DecimalsRound) + MessageSeparators.L3 + Math.Round(v.y, DecimalsRound) + MessageSeparators.L3 + Math.Round(v.z, DecimalsRound);
    }

    internal static string ConvertCameraDepthPointToStringRpc(Kinect.DepthSpacePoint p)
    {
        return "" + Math.Round(p.X, 3) + MessageSeparators.L3 + Math.Round(p.Y, 3) + MessageSeparators.L3 + 0;
    } 

    internal static string ConvertQuaternionToStringRpc(Quaternion v)
    {
        return "" + v.w + MessageSeparators.L3 + v.x + MessageSeparators.L3 + v.y + MessageSeparators.L3 + v.y;
    }

    internal static Vector3 ConvertRpcStringToVector3(string v)
    {
        string[] p = v.Split(MessageSeparators.L3);
        return new Vector3(float.Parse(p[0].Replace(',','.')), float.Parse(p[1].Replace(',', '.')), float.Parse(p[2].Replace(',', '.')));
    }

    internal static string ConvertVectorToStringRPC(Kinect.CameraSpacePoint position)
    {
        return ConvertVectorToStringRPC(new Vector3(position.X, position.Y, position.Z));
    }

    internal static Quaternion ConvertRpcStringToQuaternion(string v)
    {
        string[] p = v.Split(MessageSeparators.L3);
        return new Quaternion(float.Parse(p[1].Replace(',', '.')), float.Parse(p[2].Replace(',', '.')), float.Parse(p[2].Replace(',', '.')), float.Parse(p[0].Replace(',', '.')));
    }

    internal static Vector3 CenterOfVectors(Vector3[] vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Length == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Length;
    }

    internal static void ChangeGameObjectMaterial(GameObject go, Material mat)
    {
        if (go.GetComponent<Renderer>() != null) go.GetComponent<Renderer>().material = mat;
        foreach (Transform child in go.transform)
        {
            if (child.gameObject.GetComponent<Renderer>() != null && child.gameObject.tag != "nocolor") child.gameObject.GetComponent<Renderer>().material = mat;
        }
    }

    internal static GameObject NewGameObject(Vector3 v)
    {
        GameObject go = new GameObject();
        go.transform.position = v;
        return go;
    }

    private static int _userIDs = 0;

    public static int GetNewId()
    {
        return ++_userIDs;
    }

    internal static Vector3 PointKinectToUnity(Vector3 p)
    {
        return new Vector3(-p.x, p.y, p.z);
    }

    public static List<Color> Colors = new List<Color>()
    {
        //Color.red,
        HexToColor("#e9b96e"),
        HexToColor("#fce94f"),
        HexToColor("#8ae234"),
        HexToColor("#fcaf3e"),
        HexToColor("#729fcf"),
        HexToColor("#ad7fa8"),
        HexToColor("#cc0000"),
        HexToColor("#4e9a06"),
        HexToColor("#ce5c00"),
        HexToColor("#204a87"),
        HexToColor("#5c3566")
    };

    internal static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", ""); // in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");  // in case the string is formatted #FFFFFF
        byte a = 255; // assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        // Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }
}
//////////////////////////////////

//    internal static string ConvertQuaternionToStringRpc(Quaternion v)
//=======
//}
//>>>>>>> refs/remotes/mauriciosousa/master
