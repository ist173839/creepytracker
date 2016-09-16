/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;

public class MyUdpListener : MonoBehaviour {

    private SaveMessage _saveMessage;
    private IPEndPoint _anyIP;
    private UdpClient _udpClient = null;
  
    private List<string> _stringsToParse;

    private int _port;



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

    void FixedUpdate()
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
    /*
  if (_localTrackerUi == null || _localTrackerUi.UseRecord)
            {
                Debug.Log("Save");
                _saveMessage.IsRecording = true;
              
            }
            else if(!_localTrackerUi.UseRecord)
            {
                _saveMessage.IsRecording = false;
            }
     
     
     
     */
