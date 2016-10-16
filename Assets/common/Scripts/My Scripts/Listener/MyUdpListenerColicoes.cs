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

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
public class MyUdpListenerColicoes : MonoBehaviour
{
    private SaveColicoes _saveColicoes;
    private IPEndPoint _anyIp;
    private UdpClient _udpClient = null;

    private int _port;
    private List<string> _stringsToParse;


    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        _port = 58839;
        _saveColicoes = new SaveColicoes();
        UdpRestart();
    }

    private void UdpRestart()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
        }

        _stringsToParse = new List<string>();
        
		_anyIp = new IPEndPoint(IPAddress.Any, _port);
        
        _udpClient = new UdpClient(_anyIp);

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);

		Debug.Log("[MyUdpListenerColicoes] Receiving in port: " + _port);
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        var receiveBytes = _udpClient.EndReceive(ar, ref _anyIp);
        _stringsToParse.Add(Encoding.ASCII.GetString(receiveBytes));

        _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
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
            _saveColicoes.RecordMessage(stringToParse);
        }
    }

    private void OnApplicationQuit()
    {
        if (_udpClient != null) _udpClient.Close();
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnQuit()
    {
        OnApplicationQuit();
    }
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnDestroy()
    {
        _udpClient.Close();
    }
}

