using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;

public class MyUdpListener : MonoBehaviour {

    private UdpClient _udpClient = null;
    private IPEndPoint _anyIP;
    private List<string> _stringsToParse;
    private int _port;
    private SaveMessage _saveMessage;

    void Start()
    {
        _port = 57839;
        _saveMessage = new SaveMessage();
        UdpRestart();

    }

    private void UdpRestart()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
        }

        _stringsToParse = new List<string>();
        
		_anyIP = new IPEndPoint(IPAddress.Any, _port);
        
        _udpClient = new UdpClient(_anyIP);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);

		Debug.Log("[UDPListener] Receiving in port: " + _port);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        Byte[] receiveBytes = _udpClient.EndReceive(ar, ref _anyIP);
        _stringsToParse.Add(Encoding.ASCII.GetString(receiveBytes));

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
    }

    void Update()
    {
        if (Input.GetKeyUp("r")) // Input.GetMouseButtonDown(0)
        {
            UdpRestart();
            Debug.Log("R");
        }


        while (_stringsToParse.Count > 0)
        {
            string stringToParse = _stringsToParse.First();
            _stringsToParse.RemoveAt(0);
            _saveMessage.RecordMessage(stringToParse);
        }
    }

    private void OnApplicationQuit()
    {
        if (_udpClient != null) _udpClient.Close();
    }

    void OnQuit()
    {
        OnApplicationQuit();
    }
}
