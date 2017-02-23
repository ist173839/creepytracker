﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class KinectStream
{
    private TcpClient _client;
    public int BUFFER = 4341760;

    internal byte[] data;
    internal string name;
    internal int size;
    internal uint lastUpdated;
    internal uint lastID;
    internal bool dirty;

    public KinectStream(TcpClient client)
    {

        name = "Unknown Kinect Stream";
        _client = client;
        data = new byte[BUFFER];
        dirty = false;
    }

    public void StopStream()
    {
        _client.Close();
    }
}

// ReSharper disable once UnusedMember.Global
public class TcpKinectListener : MonoBehaviour
{
    private List<KinectStream> _kinectStreams;

    private TcpListener _server;

    public bool ShowNetworkDetails = true;

    private bool _running;

    private int TcpListeningPort;

    void Awake()
    {
        


    }


    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {

        //_threads = new List<Thread>();

        _kinectStreams = new List<KinectStream>();

        TcpListeningPort = TrackerProperties.Instance.ListenPort;
        _server = new TcpListener(IPAddress.Any, TcpListeningPort);

        _running = true;
        _server.Start();
        Debug.Log("Accepting clients at port " + TcpListeningPort);
        Thread acceptLoop = new Thread(new ParameterizedThreadStart(AcceptClients));
        //_threads.Add(acceptLoop);
        acceptLoop.Start();
    }

    private void AcceptClients(object o)
    {

        while (_running)
        {
            TcpClient newclient = _server.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(clientHandler));
            //_threads.Add(clientThread);
            clientThread.Start(newclient);
        }
    }


    private void clientHandler(object o)
    {
        int SIZEHELLO = 200;
        TcpClient client = (TcpClient)o;
        KinectStream kstream = new KinectStream(client);

        _kinectStreams.Add(kstream);

        using (NetworkStream ns = client.GetStream())
        {

            byte[] message = new byte[SIZEHELLO];
            int bytesRead = 0;
            byte[] buffer = new byte[4341760];
            try
            {
                bytesRead = ns.Read(message, 0, SIZEHELLO);
            }
            catch
            {
                Debug.Log("Connection Lost from " + kstream.name);
                client.Close();
                _kinectStreams.Remove(kstream); ;
            }

            if (bytesRead == 0)
            {
                Debug.Log("Connection Lost from " + kstream.name);
                client.Close();
                _kinectStreams.Remove(kstream); ;
            }

            //Login
            string s = System.Text.Encoding.Default.GetString(message);
            string[] l = s.Split('/');

            if (l.Length == 3 && l[0] == "k")
            {
                kstream.name = l[1];
                Debug.Log("New stream from " + l[1]);
            }

            while (_running)
            {
                try
                {
                    bytesRead = ns.Read(message, 0, 8);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                    _running = false;
                    break;
                }
                if (bytesRead == 0)
                {
                    _running = false;
                    break;
                }

                byte[] idb = { message[0], message[1], message[2], message[3] };
                uint id = BitConverter.ToUInt32(idb, 0);
                kstream.lastID = id;
                byte[] sizeb = { message[4], message[5], message[6], message[7] };
                int size = BitConverter.ToInt32(sizeb, 0);
                kstream.size = size;

                while (size > 0)
                {
                    try
                    {
                        bytesRead = ns.Read(buffer, 0, size);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                        _running = false;
                        break;
                    }
                    if (bytesRead == 0)
                    {
                        _running = false;
                        break;
                    }
                    //save because can't update from outside main thread

                    Array.Copy(buffer, 0, kstream.data, kstream.size - size, bytesRead);
                    size -= bytesRead;
                }
                kstream.dirty = true;
            }
        }

    }

    private int Convert2BytesToInt(byte b1, byte b2)
    {
        return (int)b1 + (int)(b2 * 256);
    }

    public void CloseTcpConnections()
    {
        foreach (KinectStream ks in _kinectStreams)
        {
            ks.StopStream();
        }
        _kinectStreams = new List<KinectStream>();
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    void OnApplicationQuit()
    {
        _running = false;
        CloseTcpConnections();
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void Update()
    {
        foreach (KinectStream k in _kinectStreams)
        {
            if (k.dirty)
            {
                gameObject.GetComponent<Tracker>().SetNewCloud(k.name, k.data, k.size, k.lastID);
                k.dirty = false;
            }
        }
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnQuit()
    {
        OnApplicationQuit();
    }
}
