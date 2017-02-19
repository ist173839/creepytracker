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
// ReSharper disable once ClassNeverInstantiated.Global
public class MyUdpListener : MonoBehaviour
{
    //private List<string> _stringsToParseAvr;
    //private SaveAvr      _saveAvr;
    //private IPEndPoint _anyIpAvr;
    //private UdpClient _udpClientAvr = null;
    //private int _portAvr;

    private List<string> _stringsToParseColicoes;
    private List<string> _stringsToParseRecord;
    private List<string> _stringsToParseLog;
    private List<string> _stringsToParseStatus;


    private SaveColicoes _saveColicoes;
    private SaveRecord   _saveRecord;
    private SaveLog      _saveLog;
    private SaveStatus   _saveStatus;

    private IPEndPoint _anyIpColicoes;
    private IPEndPoint _anyIpRecord;
    private IPEndPoint _anyIpLog;
    private IPEndPoint _anyIpStatus;

    private UdpClient _udpClientColicoes = null;
    private UdpClient _udpClientRecord   = null;
    private UdpClient _udpClientLog      = null;
    private UdpClient _udpClientStatus   = null;

    private int _portColicoes;
    private int _portRecord;
    private int _portLog;
    private int _portStatus;

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        //_portAvr = 59839;
        //_saveAvr = new SaveAvr();

        _portRecord   = 57839;
        _portColicoes = 58839;
        _portLog      = 60839;
        _portStatus   = 61839;

        _saveColicoes = new SaveColicoes();
        _saveRecord   = new SaveRecord();
        _saveLog      = new SaveLog();
        _saveStatus   = new SaveStatus();
    }


    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        UdpRestart();
    }
    
    public void SetNewUser(string nameUser)
    {
        var newUser = "User- " + nameUser + " Time- " + DateTime.Now.ToString("yyyyMMddTHHmmss");
        //_saveAvr.SetUpUserFolder(newUser);

        _saveStatus.SetUpUserFolder(newUser);
        _saveColicoes.SetUpUserFolder(newUser);
        _saveRecord.SetUpUserFolder(newUser);
        _saveLog.SetUpUserFolder(newUser);
    }

    private void UdpRestart()
    {
        //if (_udpClientAvr != null) _udpClientAvr.Close();
        //_stringsToParseAvr = new List<string>();
        //_anyIpAvr          = new IPEndPoint(IPAddress.Any, _portAvr);
        //_udpClientAvr      = new UdpClient(_anyIpAvr);
        //_udpClientAvr.BeginReceive(new AsyncCallback(this.ReceiveCallbackAvr), null);
        //Debug.Log("[UDPListener] Receiving Avr in port: " + _portAvr);

        if (_udpClientRecord != null) _udpClientRecord.Close();
        _stringsToParseRecord = new List<string>();
        _anyIpRecord          = new IPEndPoint(IPAddress.Any, _portRecord);
        _udpClientRecord      = new UdpClient(_anyIpRecord);
        _udpClientRecord.BeginReceive(new AsyncCallback(this.ReceiveCallbackRecord), null);
        Debug.Log("[UDPListener] Receiving Record in port : " + _portRecord);

        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        _stringsToParseColicoes = new List<string>();
        _anyIpColicoes          = new IPEndPoint(IPAddress.Any, _portColicoes);
        _udpClientColicoes      = new UdpClient(_anyIpColicoes);
        _udpClientColicoes.BeginReceive(new AsyncCallback(this.ReceiveCallbackColicoes), null);

        Debug.Log("[UDPListener] Receiving Colicoes in port: " + _portColicoes);
        if (_udpClientLog != null) _udpClientLog.Close();
        _stringsToParseLog = new List<string>();
        _anyIpLog          = new IPEndPoint(IPAddress.Any, _portLog);
        _udpClientLog      = new UdpClient(_anyIpLog);
        _udpClientLog.BeginReceive(new AsyncCallback(this.ReceiveCallbackLog), null);
        Debug.Log("[UDPListener] Receiving Log in port: " + _portLog);

        if (_udpClientStatus != null) _udpClientStatus.Close();
        _stringsToParseStatus = new List<string>();
        _anyIpStatus          = new IPEndPoint(IPAddress.Any, _portStatus);
        _udpClientStatus      = new UdpClient(_anyIpStatus);
        _udpClientStatus.BeginReceive(new AsyncCallback(this.ReceiveCallbackStatus), null);
        Debug.Log("[UDPListener] Receiving Status in port: " + _portStatus);
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

  

    private void ReceiveCallbackLog(IAsyncResult ar)
    {
        var receiveBytes = _udpClientLog.EndReceive(ar, ref _anyIpLog);
        _stringsToParseLog.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientLog.BeginReceive(new AsyncCallback(this.ReceiveCallbackLog), null);
    }

    private void ReceiveCallbackStatus(IAsyncResult ar)
    {
        var receiveBytes = _udpClientStatus.EndReceive(ar, ref _anyIpLog);
        _stringsToParseStatus.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientStatus.BeginReceive(new AsyncCallback(this.ReceiveCallbackStatus), null);
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
        //while (_stringsToParseAvr.Count > 0)
        //{
        //    var stringToParse = _stringsToParseAvr.First();
        //    _stringsToParseAvr.RemoveAt(0);
        //    _saveAvr.RecordMessage(stringToParse);
        //}

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

        while (_stringsToParseLog.Count > 0)
        {
            var stringToParse = _stringsToParseLog.First();
            _stringsToParseLog.RemoveAt(0);
            _saveLog.RecordMessage(stringToParse);
        }

        while (_stringsToParseStatus.Count > 0)
        {
            var stringToParse = _stringsToParseStatus.First();
            _stringsToParseStatus.RemoveAt(0);
            _saveStatus.RecordMessage(stringToParse);
        }
    }

    private void OnApplicationQuit()
    {
        CloseClients();
    }

    private void CloseClients()
    {
        //if (_udpClientAvr != null)      _udpClientAvr.Close();

        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        if (_udpClientRecord != null)   _udpClientRecord.Close();
        if (_udpClientLog != null)      _udpClientLog.Close();
        if (_udpClientStatus != null)   _udpClientStatus.Close();
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
        CloseClients();
    }
}
/*
 
      private void ReceiveCallbackAvr(IAsyncResult ar)
    {
        var receiveBytes = _udpClientAvr.EndReceive(ar, ref _anyIpAvr);
        _stringsToParseAvr.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientAvr.BeginReceive(new AsyncCallback(this.ReceiveCallbackAvr), null);
    }

//////////////////////////////////////////////////////////////////////////////////////////
    private IPEndPoint _anyIpBase;
    private int _portBase;
    private List<string> _stringsToParseBase;
    private UdpClient _udpClientBase     = null;
    _portBase     = 61839;
    
      if (_udpClientBase != null) _udpClientBase.Close();
        _stringsToParseBase = new List<string>();
        _anyIpBase = new IPEndPoint(IPAddress.Any, _portBase);
        _udpClientBase = new UdpClient(_anyIpBase);
        _udpClientBase.BeginReceive(new AsyncCallback(this.ReceiveCallbackBase), null);
        Debug.Log("[UDPListener] Receiving Base in port: " + _portBase);
            
  private void ReceiveCallbackBase(IAsyncResult ar)
    {
        var receiveBytes = _udpClientLog.EndReceive(ar, ref _anyIpBase);
        _stringsToParseBase.Add(Encoding.ASCII.GetString(receiveBytes));
        _udpClientBase.BeginReceive(new AsyncCallback(this.ReceiveCallbackBase), null);
    }

      if (_udpClientBase     != null) _udpClientBase.Close();
//////////////////////////////////////////////////////////////////////////////////////////

      public void SetNewUser( string id, string num)
    {
        var newUser = "Id" + id + "Num" + num;

        _saveColicoes.SetUpUserFolder(newUser);
        _saveRecord.SetUpUserFolder(newUser);
        _saveAvr.SetUpUserFolder(newUser);
        _saveLog.SetUpUserFolder(newUser);
        //

    }
 
//////////////////////////////////////////////////////////////////////////////////////////
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



    */

