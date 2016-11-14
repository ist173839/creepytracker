﻿/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class SaveAvr
{
    private StreamWriter _doc;
#pragma warning disable 169
    private DateTime _inicio;
#pragma warning restore 169

    //private ControloMode _activeControloMode;

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Separador { get; private set; }

    private readonly string _defaultFolderDestino;
#pragma warning disable 414
    private readonly string _startMessage;
#pragma warning restore 414
    private readonly string _endMessage;
    private readonly string _directory;
    private readonly string _format;
    private readonly string _versao;
    private readonly string _sigla;

    private string _currentFolderDestino;
    private string _caminhoCompleto;
    private string _defaultDocName;
    private string _currentDocName;
    private string _recordingName;
    private string _folderDestino;
    private string _fimCiclo;
    private string _docName;
    private string _target;
    private string _saveHeader;
    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

    private int _cont;

    private int _specialTypeDocName;

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isInitiate;
    private bool _isRecording;
    private bool _oversize;

    public SaveAvr() 
    {
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _isInitiate        = false;
        _oversize          = false;

        _directory = System.IO.Directory.GetCurrentDirectory();
        _currentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "AVR Data";
        Separador = ";";

        _format = ".csv";
        _sigla  = "AVRD";
        _versao = "V1";

        _startMessage = "INICIO";
        _endMessage   = "FIM";

        _recordingName   = null;
        _caminhoCompleto = null;
        _saveHeader      = null;

        _specialTypeDocName = 0;
        
        _target = _directory + "\\" + _currentFolderDestino + "\\";
        _cont = 0;
        NumColunas = 0;
        
        //_activeControloMode  = ControloMode.CWIP;
        //_headerCwip          = GetCwipHeader();
        //_headerWip           = GetWipHeader();
        //_header = GetHeader();
    }

    ~SaveAvr()
    {
        if (!_isInitiate) return;
        ResetRecord();
        if (File.Exists(_target + _currentDocName)) File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
        
    }

    private void ResetRecord()
    {
        _recordingName = null;
        _isInitiate = false;

        _cont = 0;
        NumColunas = 0;
    }

    private void CheckFileSize()
    {
        if (!_isInitiate) return;
        if (!File.Exists(_target + _currentDocName)) return;
        var info = new FileInfo(_target + _currentDocName);
        if (info.Length < TamanhoMaximo) return;
        _oversize = true;
        Debug.Log("New File, Current Size = " + info.Length + " ( MAX = " + TamanhoMaximo + " )");
        StopRecording();
    }

    public void RecordMessage(string message)
    {
        if (message.Contains("Registo"))
        {
            _isInitiate = false;
            _saveHeader = message;
        }
        
        if (!_isInitiate) SetUpFileAndDirectory();
        // if (!_isInitiate) SetUpFileAndDirectory(message);
        // if (message != _startMessage  && !_isInitiate)
        // {
        // } else
        CheckFileSize();
        if (message == _endMessage)
        {
            StopRecording();
            Console.WriteLine(_endMessage);
        }
        else
            WriteStringInDoc(message, true);
        
    }
    
    private void SetUpFileAndDirectory()
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        _cont = 0;
        SetUpDirectory();
        SetFileName();
        if (_oversize)
        {
            SetUpHeader();
            _oversize = false;
        }
        _isInitiate = true;
    }

    // ReSharper disable once UnusedMember.Local
    private void SetUpFileAndDirectory(string first)
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        SetUpDirectory();
        SetFileName();
        //SetUpHeader(first);
        _isInitiate = true;
    }
    private void SetUpHeader()
    {
        // if (first.Contains("Registo")) return;
        WriteStringInDoc(_saveHeader, true);
    }


    // ReSharper disable once UnusedMember.Local
    private void SetUpHeader(string first)
    {
        if (first.Contains("Registo")) return;
        WriteStringInDoc(first, true);
    }

    private void WriteStringInDoc(string registo, bool isAppend)
    {
        _doc = new StreamWriter(_target + _currentDocName, isAppend);
        _doc.WriteLine(registo);
        _doc.Close();
    }

    private void StopRecording()
    {
        if (!_isInitiate) return;
        ResetRecord();
        if (!File.Exists(_target + _currentDocName)) return;
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
    }
  
    private void SetUpDirectory()
    {
        if (!System.IO.Directory.Exists(_target))
            System.IO.Directory.CreateDirectory(_target);
    }

    private void SetFileName()
    {
        if (_useDefaultDocName)
        {
            _currentDocName = _sigla + "_" + _versao + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss") ;
            _currentDocName = SolveDuplicateFileNames() + _format;
        }
        else
        {
            string temp;
            switch (_specialTypeDocName)
            {
                case 0:
                    temp = SolveDuplicateFileNames();
                    break;
                case 1:
                    temp = _currentDocName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
                    break;
                default:
                    temp = _currentDocName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
                    break;
            }
            _currentDocName = temp + _format;

            Debug.Log("New Walking Data File : " + _currentDocName);
        }
    }

    private string SolveDuplicateFileNames()
    {
        var temp = _currentDocName;
        var count = 1;
        while (File.Exists(_target + temp + _format))
        {
            temp = string.Format("{0}_{1}", _currentDocName, count++);
            Debug.Log(temp);
        }
        return temp;
    }

    private void WriteStringInDoc(string registo)
    {
        _doc = new StreamWriter(_target + _currentDocName);
        _doc.WriteLine(registo);
        _doc.Close();
    }

    // ReSharper disable once UnusedMember.Global
    public string GetDocActivo()
    {
        if (_isInitiate) return _target + _currentDocName;
        return null;
    }

    // ReSharper disable once UnusedMember.Global
    public void SetRecordingName(string recordName)
    {
        if (recordName.Equals(_recordingName)) return;
        _recordingName = recordName;
        _isInitiate = false;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialFolderName(string newName)
    {
        _currentFolderDestino = _folderDestino = newName;
        _useDefaultFolder = false;
        _isInitiate = false;
        _target = _directory + "\\" + _currentFolderDestino + "\\";
    }

    public void SpecialDocName(string newName)
    {
        //#if !UNITY_ANDROID
        _docName = newName;
        _useDefaultDocName = false;
        _currentDocName = _docName;
        _isInitiate = false;
        //#endif
    }

    public void SpecialTypeDocName(int t)
    {
        _specialTypeDocName = t;
    }

    // ReSharper disable once UnusedMember.Global
    public void UseDefaultDocName()
    {
        if (_useDefaultDocName) return;
        _useDefaultDocName = true;
        _isInitiate = false;
    }

    // ReSharper disable once UnusedMember.Global
    public void UseDefaultFolderName()
    {
        if (_useDefaultFolder) return;
        _useDefaultFolder = true;
        _isInitiate = false;
        _currentFolderDestino = _defaultFolderDestino;
    }

}


//if (!IsRecording)
//{
//    StopRecording();
//    return;
//}
//CheckHeaders(message);


//private string GetHeader()
//{
//    return
//       null;
//}

//private void SetUpHeader()
//{
//    var info = GetHeader();
//    WriteStringInDoc(info, true);
//}

//private void SetUpHeader(string first)
//{
//    var info = GetHeader(); 
//    if (first == info) return;
//    WriteStringInDoc(info, true);
//}

//private string _header;