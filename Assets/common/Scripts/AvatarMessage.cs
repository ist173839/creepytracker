using UnityEngine;
using System.Collections.Generic;
using System.Net;

// ReSharper disable once CheckNamespace
public class AvatarMessage {

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public IPAddress ReplyIpAddress;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int Port;
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int Mode;

    public AvatarMessage(string message, byte[] receivedBytes)
    {
        string[] msg = message.Split(MessageSeparators.L1);
        ReplyIpAddress = IPAddress.Parse(msg[0]);
        Mode = int.Parse(msg[1]);
        Port = int.Parse(msg[2]);
    }

    public string createCalibrationMessage(Dictionary<string,Sensor> sensors)
    {
        string res = "AvatarMessage"+ MessageSeparators.L0;
        bool first = true;
        foreach (Sensor s in sensors.Values)
        {
            if (!first) res += MessageSeparators.L1;
            Vector3 p = s.SensorGameObject.transform.position;
            Quaternion r = s.SensorGameObject.transform.rotation;
            res += s.SensorID + ";" + p.x + ";" + p.y + ";" + p.z + ";" + r.x + ";" + r.y + ";" + r.z + ";" + r.w + MessageSeparators.L1;
        }
        return res;
    }
}
