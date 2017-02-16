using System;
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
    internal string name;
    internal byte[] data;
    internal int size;

    public KinectStream(TcpClient client)
    {
        
        name = "Unknown Kinect Stream";
        _client = client;
    }

    public void StopStream()
    {
        _client.Close();
    }
}

// ReSharper disable once UnusedMember.Global
public class TcpKinectListener : MonoBehaviour
{

    public int Buffer = 4341760;

    public bool ShowNetworkDetails = true;

    private int _tcpListeningPort;

    private TcpListener _server;

    private bool _running;

    private List<KinectStream> _kinectStreams;
    
    void Start ()
    {

        //_threads = new List<Thread>();

        _kinectStreams = new List<KinectStream>();

        _tcpListeningPort = TrackerProperties.Instance.ListenPort;
        _server = new TcpListener(IPAddress.Any, _tcpListeningPort);

        _running = true;
        _server.Start();
        Debug.Log("Accepting clients at port " + _tcpListeningPort);
        Thread acceptLoop = new Thread(new ParameterizedThreadStart(AcceptClients));
        //_threads.Add(acceptLoop);
        acceptLoop.Start();
    }

    void AcceptClients(object o)
    {

        while (_running)
        {
            TcpClient newclient = _server.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(clientHandler));
            //_threads.Add(clientThread);
            clientThread.Start(newclient);
        }
    }

    void clientHandler(object o)
    {
        TcpClient client = (TcpClient)o;
        KinectStream kstream = new KinectStream(client);

        _kinectStreams.Add(kstream);

        using (NetworkStream ns = client.GetStream())
        {
            bool login = false;

            byte[] message = new byte[Buffer];
            int bytesRead;

            while (_running)
            {
                try
                {
                    bytesRead = ns.Read(message, 0, Buffer);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }
               
            //new message
            if (!login)
                {
                    string s = System.Text.Encoding.Default.GetString(message);
                    string[] l = s.Split('/');
                    if (l.Length == 3 && l[0] == "k")
                    {
                        kstream.name = l[1];
                        login = true;
                        Debug.Log("New stream from " + l[1]);
                    }
                }
                else
                {
                    //save because can't update from outside main thread
                    kstream.data = message;
                    kstream.size = bytesRead;
                }
            } 
        }
        Debug.Log("Connection Lost from " + kstream.name);
        client.Close();
        _kinectStreams.Remove(kstream);
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

    void OnApplicationQuit()
    {
        _running = false;
        CloseTcpConnections();
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Update()
    {
        foreach(KinectStream k in _kinectStreams)
        {    
            gameObject.GetComponent<Tracker>().SetNewCloud(k.name, k.data,k.size);
        }
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnQuit()
    {
        OnApplicationQuit();
    }
}
