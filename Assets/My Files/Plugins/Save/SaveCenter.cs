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
public class SaveCenter
{
    private SpecialTypeDoc _specialTypeDocName;

    private StreamWriter _doc;

#pragma warning disable 169
    private DateTime _inicio;
#pragma warning restore 169

    //private ControloMode _activeControloMode;

    // ReSharper disable once MemberCanBePrivate.Global
    public string Separador { get; private set; }

    private readonly string _defaultFolderDestino;
#pragma warning disable 414
    private readonly string _startMessage;
#pragma warning restore 414
    private readonly string _endMessage;
    private readonly string _directory;
    private readonly string _format;

    private string _currentFolderDestino;
#pragma warning disable 414
    private string _caminhoCompleto;
#pragma warning restore 414
#pragma warning disable 169
    private string _defaultDocName;
#pragma warning restore 169
    private string _currentDocName;
    private string _recordingName;
    // ReSharper disable once NotAccessedField.Local
    private string _folderDestino;
#pragma warning disable 414
    private string _saveHeader;
#pragma warning restore 414
#pragma warning disable 169
    private string _fimCiclo;
#pragma warning restore 169
    private string _docName;
    private string _target;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _versao;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _sigla;

    // private string _header;

    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

#pragma warning disable 414
    private int _cont;
#pragma warning restore 414
    
    //  public bool DirectoryChange;
    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
#pragma warning disable 169
    private bool _isRecording;
#pragma warning restore 169
    private bool _isInitiate;
#pragma warning disable 414
    private bool _oversize;
#pragma warning restore 414
    
    public SaveCenter() 
    {
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _isInitiate        = false;
        _oversize          = false;

        _directory = System.IO.Directory.GetCurrentDirectory();
        _currentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "Center Data";
        _format   = ".txt";
        Separador = ";";

        _startMessage = "INICIO";
        _endMessage   = "FIM";

        _sigla = "CD";
        _versao = "V1";

        _recordingName = null;
        _caminhoCompleto = null;
        _specialTypeDocName = SpecialTypeDoc.SolveDuplicate;
        
        _target = _directory + "\\" + _currentFolderDestino + "\\";
        _cont = 0;
        NumColunas = 0;

        _saveHeader = null;

        // _header = GetHeader();
    }

    ~SaveCenter()
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
       
        if (!_isInitiate) SetUpFileAndDirectory();

        //if (!_isInitiate) SetUpFileAndDirectory(message);
        //if (message != _startMessage  && !_isInitiate)
        //{
        //} else
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
        _isInitiate = true;
    }

    private void WriteStringInDoc(string registo, bool isAppend)
    {
        _doc = new StreamWriter(_target + _currentDocName, isAppend);
        _doc.WriteLine(registo);
        _doc.Close();
    }

    public void StopRecording()
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
                case SpecialTypeDoc.SolveDuplicate:
                    temp = SolveDuplicateFileNames();
                    break;
                case SpecialTypeDoc.Normal:
                    temp = _currentDocName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
                    break;
                default:
                    temp = _currentDocName + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
                    break;
            }
            _currentDocName = temp + _format;

            Debug.Log("New Colision Data File : " + _currentDocName);
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

    // ReSharper disable once UnusedMember.Global

    public void SpecialDocName(string newName)
    {
        //#if !UNITY_ANDROID
        _docName = newName;
        _useDefaultDocName = false;
        _currentDocName = _docName;
        _isInitiate = false;
        //#endif
    }

    // ReSharper disable once UnusedMember.Global

    public void SpecialTypeDocName(SpecialTypeDoc t)
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

    // ReSharper disable once UnusedMember.Local

    private void SetUpFileAndDirectory(string first)
    {

        SetUpDirectory();
        SetFileName();
        //SetUpHeader(first);
        _isInitiate = true;
    }
}
/*
 * 
if (_oversize)
        {
            SetUpHeader();
            _oversize = false;
        }

 * 
 * 
private void SetUpHeader()
    {
        //var info = GetHeader();
        WriteStringInDoc(_saveHeader, true);
    }
  private void SetUpHeader(string first)
{
    var info = GetHeader(); 
    if (first == info) return;
    WriteStringInDoc(info, true);
}
    



public void RecordOneMessage(string message)
{

    // if (!_isInitiate)
    SetUpFileAndDirectory();

    //if (!_isInitiate) SetUpFileAndDirectory(message);
    //if (message != _startMessage  && !_isInitiate)
    //{
    //} else
    // CheckFileSize();
    if (message == _endMessage)
    //{
    //    StopRecording();
    //    Console.WriteLine(_endMessage);
    //}
    //else
    WriteStringInDoc(message, true);
    StopRecording();
}



private string GetHeader()
{
    return "Registo" + Separador;       
}




 */



// _target = _directory + "\\" +_CurrentFolderDestino ;

//if (!IsRecording)
//{
//    StopRecording();
//    return;
//}
//CheckHeaders(message);