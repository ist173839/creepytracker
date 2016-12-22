﻿/*************************************************************************************************
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
public class MyUdpListener : MonoBehaviour
{
    private SaveColicoes _saveColicoes;
    private SaveRecord   _saveRecord;
    private SaveAvr      _saveAvr;
    private SaveLog      _saveLog;

    private IPEndPoint _anyIpColicoes;
    private IPEndPoint _anyIpRecord;
    private IPEndPoint _anyIpAvr;
    private IPEndPoint _anyIpLog;
    
    private UdpClient _udpClientColicoes = null;
    private UdpClient _udpClientRecord   = null;
    private UdpClient _udpClientAvr      = null;
    private UdpClient _udpClientLog      = null;

    private int _portColicoes;
    private int _portRecord;
    private int _portAvr;
    private int _portLog;
    
    private List<string> _stringsToParseColicoes;
    private List<string> _stringsToParseRecord;
    private List<string> _stringsToParseAvr;
    private List<string> _stringsToParseLog;

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {

        _portRecord   = 57839;
        _portColicoes = 58839;
        _portAvr      = 59839;
        _portLog      = 50839;

        _saveColicoes = new SaveColicoes();
        _saveRecord   = new SaveRecord();
        _saveAvr      = new SaveAvr();
        _saveLog      = new SaveLog();


        UdpRestart();
    }

    private void UdpRestart()
    {
        if (_udpClientRecord != null) _udpClientRecord.Close();
        _stringsToParseRecord = new List<string>();
        _anyIpRecord = new IPEndPoint(IPAddress.Any, _portRecord);
        _udpClientRecord = new UdpClient(_anyIpRecord);
        _udpClientRecord.BeginReceive(new AsyncCallback(this.ReceiveCallbackRecord), null);
        Debug.Log("[UDPListener] Receiving Record in port : " + _portRecord);
        
        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        _stringsToParseColicoes = new List<string>();
        _anyIpColicoes = new IPEndPoint(IPAddress.Any, _portColicoes);
        _udpClientColicoes = new UdpClient(_anyIpColicoes);
        _udpClientColicoes.BeginReceive(new AsyncCallback(this.ReceiveCallbackColicoes), null);
        Debug.Log("[UDPListener] Receiving Colicoes in port: " + _portColicoes);

        if (_udpClientAvr != null) _udpClientAvr.Close();
        _stringsToParseAvr = new List<string>();
        _anyIpAvr = new IPEndPoint(IPAddress.Any, _portAvr);
        _udpClientAvr = new UdpClient(_anyIpAvr);
        _udpClientAvr.BeginReceive(new AsyncCallback(this.ReceiveCallbackAvr), null);
        Debug.Log("[UDPListener] Receiving Avr in port: " + _portAvr);


        if (_udpClientLog != null) _udpClientLog.Close();
        _stringsToParseLog = new List<string>();
        _anyIpLog = new IPEndPoint(IPAddress.Any, _portLog);
        _udpClientLog = new UdpClient(_anyIpLog);
        _udpClientLog.BeginReceive(new AsyncCallback(this.ReceiveCallbackLog), null);
        Debug.Log("[UDPListener] Receiving Log in port: " + _portLog);



    }

    private void ReceiveCallbackRecord(IAsyncResult ar)
    {
        var receiveBytes = _udpClientRecord.EndReceive(ar, ref _anyIpRecord);
        _stringsToParseRecord.Add(Encoding.ASCII.GetString(receiveBytes));

        _udpClientRecord.BeginReceive(new AsyncCallback(this.ReceiveCallbackRecord), null);
    }

    private void ReceiveCallbackColicoes(IAsyncResult ar)
    {
        var receiveBytes = _udpClientColicoes.EndReceive(ar, ref _anyIpColicoes);
        _stringsToParseColicoes.Add(Encoding.ASCII.GetString(receiveBytes));

        _udpClientColicoes.BeginReceive(new AsyncCallback(this.ReceiveCallbackColicoes), null);
    }

    private void ReceiveCallbackAvr(IAsyncResult ar)
    {
        var receiveBytes = _udpClientAvr.EndReceive(ar, ref _anyIpAvr);
        _stringsToParseAvr.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientAvr.BeginReceive(new AsyncCallback(this.ReceiveCallbackAvr), null);
    }

    private void ReceiveCallbackLog(IAsyncResult ar)
    {
        var receiveBytes = _udpClientLog.EndReceive(ar, ref _anyIpLog);
        _stringsToParseLog.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientLog.BeginReceive(new AsyncCallback(this.ReceiveCallbackLog), null);
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void FixedUpdate()
    {
        PressToRestart();
        UpdateMessages();
    }

    private void PressToRestart()
    {
        if (!Input.GetKeyUp("r")) return;
        UdpRestart();
        Debug.Log("R");
    }

    private void UpdateMessages()
    {
        while (_stringsToParseRecord.Count > 0)
        {
            var stringToParse = _stringsToParseRecord.First();
            _stringsToParseRecord.RemoveAt(0);
            _saveRecord.RecordMessage(stringToParse);
        }

        while (_stringsToParseColicoes.Count > 0)
        {
            var stringToParse = _stringsToParseColicoes.First();
            _stringsToParseColicoes.RemoveAt(0);
            _saveColicoes.RecordMessage(stringToParse);
        }


        while (_stringsToParseAvr.Count > 0)
        {
            var stringToParse = _stringsToParseAvr.First();
            _stringsToParseAvr.RemoveAt(0);
            _saveAvr.RecordMessage(stringToParse);
        }
    }

    private void OnApplicationQuit()
    {
        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        if (_udpClientRecord   != null) _udpClientRecord.Close();
        if (_udpClientAvr      != null) _udpClientAvr.Close();
        if (_udpClientLog      != null) _udpClientLog.Close();
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void OnQuit()
    {
        OnApplicationQuit();
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void OnDestroy()
    {
        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        if (_udpClientRecord   != null) _udpClientRecord.Close();
        if (_udpClientAvr      != null) _udpClientAvr.Close();
        if (_udpClientLog      != null) _udpClientLog.Close();
    }
}

// private List<string> _stringsToParse;
// private UdpClient _udpClient = null;
// private IPEndPoint _anyIp;

//private void ReceiveCallback(IAsyncResult ar)
//{
//    var receiveBytes = _udpClient.EndReceive(ar, ref _anyIp);
//    _stringsToParse.Add(Encoding.ASCII.GetString(receiveBytes));
//    _udpClient.BeginReceive(new AsyncCallback(this.ReceiveCallback), null);
//} 
// Input.GetMouseButtonDown(0)