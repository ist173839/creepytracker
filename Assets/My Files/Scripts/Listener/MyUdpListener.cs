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
    private int _finalNum; // { get; set; }
    private string _userLevelName;
    
    private List<string> _stringsToParseColicoes;
    private SaveColicoes _saveColicoes;
    private IPEndPoint   _anyIpColicoes;
    private UdpClient    _udpClientColicoes = null;
    private int          _portColicoes;

    private List<string> _stringsToParseLog;
    private SaveLog      _saveLog;
    private IPEndPoint   _anyIpLog;
    private UdpClient    _udpClientLog = null;
    private int          _portLog;

    //private List<string> _stringsToParseRecord;
    //private SaveRecord   _saveRecord;
    //private IPEndPoint   _anyIpRecord;
    //private UdpClient    _udpClientRecord   = null;
    //private int          _portRecord;

    //private List<string> _stringsToParseStatus;
    //private SaveStatus   _saveStatus;
    //private IPEndPoint   _anyIpStatus;
    //private UdpClient    _udpClientStatus   = null;
    //private int          _portStatus;


    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        _portColicoes = 58839;
        _saveColicoes = new SaveColicoes();

        _portLog      = 60839;
        _saveLog      = new SaveLog();

        _finalNum = -1;
        _userLevelName = null;
        //_portRecord   = 57839;
        //_saveRecord   = new SaveRecord();

        //_portStatus   = 61839;
        //_saveStatus   = new SaveStatus();
    }
    
    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void Start()
    {
        UdpRestart();
    }
    
    public void SetNewUser(string nameUser)
    {
        var newUser = nameUser;
        _saveColicoes.SetUpUserFolder(newUser);
        _saveLog.SetUpUserFolder(newUser);
        
        // _saveRecord.SetUpUserFolder(newUser);
        // _saveStatus.SetUpUserFolder(newUser);
    }

    public void SetNewSection()
    {
        _saveColicoes.SetUpUserFolder();
        _saveLog.SetUpUserFolder();
       
        // _saveRecord.SetUpUserFolder();
        // _saveStatus.SetUpUserFolder();
    }

    public void EndTest()
    {
        _saveLog.EndLog();
        _saveColicoes.EndLog();
    }

    private void UdpRestart()
    {
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

        //if (_udpClientRecord != null) _udpClientRecord.Close();
        //_stringsToParseRecord = new List<string>();
        //_anyIpRecord          = new IPEndPoint(IPAddress.Any, _portRecord);
        //_udpClientRecord      = new UdpClient(_anyIpRecord);
        //_udpClientRecord.BeginReceive(new AsyncCallback(this.ReceiveCallbackRecord), null);
        //Debug.Log("[UDPListener] Receiving Record in port : " + _portRecord);

        //if (_udpClientStatus != null) _udpClientStatus.Close();
        //_stringsToParseStatus = new List<string>();
        //_anyIpStatus          = new IPEndPoint(IPAddress.Any, _portStatus);
        //_udpClientStatus      = new UdpClient(_anyIpStatus);
        //_udpClientStatus.BeginReceive(new AsyncCallback(this.ReceiveCallbackStatus), null);
        //Debug.Log("[UDPListener] Receiving Status in port: " + _portStatus);
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

    //private void ReceiveCallbackRecord(IAsyncResult ar)
    //{
    //    var receiveBytes = _udpClientRecord.EndReceive(ar, ref _anyIpRecord);
    //    _stringsToParseRecord.Add(Encoding.ASCII.GetString(receiveBytes));
    //    _udpClientRecord.BeginReceive(new AsyncCallback(this.ReceiveCallbackRecord), null);
    //}

    //private void ReceiveCallbackStatus(IAsyncResult ar)
    //{
    //    var receiveBytes = _udpClientStatus.EndReceive(ar, ref _anyIpLog);
    //    _stringsToParseStatus.Add(Encoding.ASCII.GetString(receiveBytes));
    //    _udpClientStatus.BeginReceive(new AsyncCallback(this.ReceiveCallbackStatus), null);
    //}

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local

    void FixedUpdate()
    {
        PressToRestart();
        UpdateMessages();
    }

    private void UpdateMessages()
    {
        while (_stringsToParseLog.Count > 0)
        {
            var stringToParse = _stringsToParseLog.First();
            _stringsToParseLog.RemoveAt(0);
            _saveLog.RecordMessage(stringToParse);
        }

        if (_saveLog.UserLevelName != null && _userLevelName != _saveLog.UserLevelName)
        {

            _userLevelName = _saveColicoes.UserLevelName = _saveLog.UserLevelName;
        }


        while (_stringsToParseColicoes.Count > 0)
        {
            var stringToParse = _stringsToParseColicoes.First();
            _stringsToParseColicoes.RemoveAt(0);
            _saveColicoes.RecordMessage(stringToParse);
        }


        //while (_stringsToParseStatus.Count > 0)
        //{
        //    var stringToParse = _stringsToParseStatus.First();
        //    _stringsToParseStatus.RemoveAt(0);
        //    _saveStatus.RecordMessage(stringToParse);
        //}
        
        //while (_stringsToParseRecord.Count > 0)
        //{
        //    var stringToParse = _stringsToParseRecord.First();
        //    _stringsToParseRecord.RemoveAt(0);
        //    _saveRecord.RecordMessage(stringToParse);
        //}
    }

    public int GetFinalNum()
    {
        return _finalNum = _saveLog.FinalNum;
        // return _finalNum = _saveStatus.FinalNum;
    }

    public void ResetFinalNum()
    {
        _saveLog.ResetFinalNum();
        // _saveStatus.ResetFinalNum();
    }

    private void PressToRestart()
    {
        if (!Input.GetKeyUp("r")) return;
        UdpRestart();
        Debug.Log("R Press - Restart Udps");
    }

    private void OnApplicationQuit()
    {
        CloseClients();
        UpdateMessages();
    }

    private void CloseClients()
    {
        if (_udpClientColicoes != null) _udpClientColicoes.Close();
        if (_udpClientLog      != null) _udpClientLog.Close();

        // if (_udpClientRecord   != null) _udpClientRecord.Close();
        // if (_udpClientStatus   != null) _udpClientStatus.Close();
    }

    // ReSharper disable once ArrangeTypeMemberModifiers
    // ReSharper disable once UnusedMember.Local
    void OnQuit()
    {
        OnApplicationQuit();
    }

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    void OnDestroy()
    {
        OnApplicationQuit();
    }
}
///////////////////////////////////////////////////////////////////////////////////
/*
   
 */
