using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;

// ReSharper disable once ClassNeverInstantiated.Global
// ReSharper disable once CheckNamespace
public class UdpListener : MonoBehaviour {

    private UdpClient _udpClient = null;
    private IPEndPoint _anyIp;
    private List<byte[]> _stringsToParse; // TMA: Store the bytes from the socket instead of converting to strings. Saves time.
#pragma warning disable 169
    private byte[] _receivedBytes;

    private int number = 0;
    //so we don't have to create again
    CloudMessage message;

    void Start()
    {
        message = new CloudMessage();
    }

    public void UdpRestart()
    {
        if (_udpClient != null) _udpClient.Close();

        _stringsToParse = new List<byte[]>();
        _anyIp = new IPEndPoint(IPAddress.Any, TrackerProperties.Instance.ListenPort);
        _udpClient = new UdpClient(_anyIp);
        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
        Debug.Log("[UDPListener] Receiving in port: " + TrackerProperties.Instance.ListenPort);
    }

    public void ReceiveCallback(IAsyncResult ar)
    {

        Byte[] receiveBytes = _udpClient.EndReceive(ar, ref _anyIp);
        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
        _stringsToParse.Add(receiveBytes);
    }

    private void PressToRestart()
    {
        if (!Input.GetKeyUp("r")) return;
        UdpRestart();
        Debug.Log("R");
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void FixedUpdate() // Update
    {
        PressToRestart();

        while (_stringsToParse.Count > 0)
        {
            try
            {
                var toProcess = _stringsToParse.First();
                if (toProcess != null)
                {
                    // TMA: THe first char distinguishes between a BodyMessage and a CloudMessage
                    if (Convert.ToChar(toProcess[0]) == 'B')
                    {
                        try
                        {
                            string stringToParse = Encoding.ASCII.GetString(toProcess);
                            string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
                            BodiesMessage b = new BodiesMessage(splitmsg[1]);
                            gameObject.GetComponent<Tracker>().SetNewFrame(b);
                        }
                        catch (BodiesMessageException e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
                    else if (Convert.ToChar(toProcess[0]) == 'C')
                    {
                        string stringToParse = Encoding.ASCII.GetString(toProcess);
                        string[] splitmsg = stringToParse.Split(MessageSeparators.L0);

                        message.set(splitmsg[1], toProcess, splitmsg[0].Length);
                        gameObject.GetComponent<Tracker>().SetNewCloud(message);
                    }
                    else if (Convert.ToChar(toProcess[0]) == 'A')
                    {
                        string stringToParse = Encoding.ASCII.GetString(toProcess);
                        string[] splitmsg = stringToParse.Split(MessageSeparators.L0);
                        AvatarMessage av = new AvatarMessage(splitmsg[1], toProcess);
                        gameObject.GetComponent<Tracker>().processAvatarMessage(av);
                    }
                }
                _stringsToParse.RemoveAt(0);
            }
            catch (Exception exc)
            {
                _stringsToParse.RemoveAt(0);
            }
        }
    }

    void OnApplicationQuit()
    {
        if (_udpClient != null) _udpClient.Close();
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnQuit()
    {
        OnApplicationQuit();
    }
}
