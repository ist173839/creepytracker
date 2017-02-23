using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// ReSharper disable once CheckNamespace
public class CloudMessage
{
    public byte[] ReceivedBytes;

    public int HeaderSize;

    public string Message;

    public CloudMessage()
    {
        Message = "";
    }

    public void Set(string message, byte[] receivedBytes, int headerSize)
    {
        //moved implementation to
        this.Message = message;
        this.ReceivedBytes = receivedBytes;
        this.HeaderSize = headerSize;
    }

    public static string CreateRequestMessage(int mode, string addr, int port)
    {
        return "CloudMessage" + MessageSeparators.L0 + addr + MessageSeparators.L1 + (mode) + MessageSeparators.L1 + port;
    }
}
