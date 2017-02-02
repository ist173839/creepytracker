/*************************************************************************************************
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
public class SaveColicoes
{
    private StreamWriter _doc;
#pragma warning disable 169
    private DateTime _inicio;
#pragma warning restore 169

    //private ControloMode _activeControloMode;

    // ReSharper disable once MemberCanBePrivate.Global
    public string Separador { get; private set; }

    private string _defaultFolderDestino;
    private string _startMessage;
    private string _endMessage;
    private string _directory;
    private string _format;

    private string _currentUserFolder;
    private string _currentFolderDestino;
    private string _caminhoCompleto;
    private string _defaultDocName;
    private string _currentDocName;
    private string _recordingName;
    private string _folderDestino;
    private string _saveHeader;
    private string _nameFolder;
    private string _fimCiclo;
    private string _docName;
    private string _target;
    private string _versao;
    private string _sigla;

    // private string _header;

    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

    private int _cont;

    private SpecialTypeDoc _specialTypeDocName;

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isInitiate;
    private bool _isRecording;
    private bool _oversize;


    public SaveColicoes(string userFolder)
    {
        SetUp(userFolder);
    }

    public SaveColicoes()
    {
        SetUp(null);
    }

    private void SetUp(string userFolder)
    {
        _useDefaultDocName = true;
        _useDefaultFolder = true;
        _isInitiate = false;
        _oversize = false;

        _directory = System.IO.Directory.GetCurrentDirectory();

        _nameFolder = "Collisions Data";

        _currentUserFolder = userFolder;
        
        _currentFolderDestino =
            _defaultFolderDestino =
                _currentUserFolder == null
                    ? "Saved Files" + "\\" + _nameFolder
                    : _currentUserFolder + "\\" + "Saved Files" + "\\" + _nameFolder;


        //_currentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "Collisions Data";



        _format = ".csv";
        Separador = ";";

        _startMessage = "INICIO";
        _endMessage = "FIM";

        _sigla = "COD";
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

    ~SaveColicoes()
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

        //if (!_isInitiate) SetUpFileAndDirectory(message);
        //if (message != _startMessage  && !_isInitiate)
        //{
        //} else
        if (message == _endMessage)
        {
            StopRecording();
            Debug.Log(_endMessage);
        }
        else
            WriteStringInDoc(message, true);
        CheckFileSize();
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
        SetUpHeader(first);
        _isInitiate = true;
    }

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

    private string GetHeader()
    {
        return "Registo" + Separador + "Name" + Separador + "Position Object" + Separador + "Position Player" + Separador + "State" + Separador + "Time";       
    }
}
// _target = _directory + "\\" +_CurrentFolderDestino ;

//if (!IsRecording)
//{
//    StopRecording();
//    return;
//}
//CheckHeaders(message);