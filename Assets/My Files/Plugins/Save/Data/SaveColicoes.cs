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

    private SpecialTypeDoc _specialTypeDocName;

    private DateTime _inicio;
    
    public string Separador { get; private set; }
    public string UserLevelName;
    public string NumTest;
    //  public string TypeMet;


    private string _currentFolderDestino;
    private string _defaultFolderDestino;
    private string _currentUserFolder;
    private string _caminhoCompleto;
    private string _defaultDocName;
    private string _currentDocName;
    private string _recordingName;
    private string _folderDestino;
    private string _folderName;
    private string _saveHeader;
    private string _endMessage;
    private string _directory;
    private string _fimCiclo;
    private string _docName;
    private string _header;
    private string _target;
    private string _format;
    private string _versao;
    private string _sigla;
    private string _defaultName;
    private string _fimTest;
    private string _lastMessage;


    private int _numColunas;// { get; set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

    private int _cont;
    
    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isInitiate;
    
    //////////////////////////////////////////////////////////////////////////
    public SaveColicoes()
    {
        SetUp(null);
    }

    private bool _oversize;

    public SaveColicoes(string userFolder)
    {
        SetUp(userFolder);
    }

    public void SetUpUserFolder(string userFolder)
    {
        _currentUserFolder = userFolder;
        SetUpUserFolder();
    }

    public void SetUpUserFolder()
    {
        var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");

        const string constPath = "Files" + "\\" + "Saved Files"; // + "\\";

        var tempPath = "\\";

        tempPath += (_currentUserFolder == null ? _defaultName : _currentUserFolder) + "\\";

        if (UserLevelName != null) tempPath += UserLevelName + "\\";

        if (NumTest       != null) tempPath += "Teste nº " + NumTest + "\\";

        tempPath += sessao + "\\";

        _currentFolderDestino = _defaultFolderDestino = constPath + tempPath + _folderName;

        _isInitiate = false;
    }


    
    ~SaveColicoes()
    {
        StopRecording();

        //if (!_isInitiate) return;
        //ResetRecord();
        //if (File.Exists(_target + _currentDocName)) File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);    
    }

    private void SetUp(string userFolder)
    {
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _isInitiate        = false;
        _oversize          = false;

        _directory = System.IO.Directory.GetCurrentDirectory();

        _folderName = "Collisions Data";
        _defaultName = "Default User Name - Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
        SetUpUserFolder(userFolder);

        Separador = ";";

        _endMessage = "FIM";
        _format     = ".csv";
        _versao     = "V1";
        _sigla      = "COD";

        UserLevelName = null;
        NumTest = null;

        _caminhoCompleto = null;
        _recordingName   = null;
        _saveHeader      = null;
        _fimTest         = null;

        _specialTypeDocName = SpecialTypeDoc.SolveDuplicate;

        _target = _directory + "\\" + _currentFolderDestino + "\\";

        _cont = 0;
        _numColunas = 0;


        _header = GetHeader();

        // _startMessage = "INICIO";
    }

    private void ResetRecord()
    {
        _recordingName = null;
        _isInitiate = false;
        _cont = 0;
        _numColunas = 0;
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
            StopRecording();
            // WriteStringInDoc(_saveHeader, true);
        }
        else
        {
            if (!_isInitiate)
            {
                SetUpFileAndDirectory(message);
                if (_saveHeader != null)
                    WriteStringInDoc(_saveHeader + " (*)",  true);
                else
                    WriteStringInDoc(_header, true);
            }
        }

        if (!_isInitiate) SetUpFileAndDirectory(message);

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
        _target = _directory + "\\" + _currentFolderDestino + "\\";
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

    private void SetUpFileAndDirectory(string mensagem)
    {
        SetUpFileAndDirectory();
        GetNumCol(mensagem);
        SetUpFimCiclo();
        SetUpEnd();
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
    
    public void EndLog()
    {
        Debug.Log("End Log");
        if (!_isInitiate) return;

        if (_fimTest != null)
        {
            WriteStringInDoc(_fimTest, true);
            StopRecording();
        }
        else
        {
            Debug.Log("(_fimTest == null) > Isto não deve acontecer < ");
            if (_lastMessage == _endMessage) return;
            GetNumCol(_lastMessage);
            SetUpFimCiclo();
            SetUpEnd();
            WriteStringInDoc(_fimTest, true);
            StopRecording();
        }
    }
    private void GetNumCol(string s)
    {
        char[] del = Separador.ToCharArray();
        _numColunas = s.Split(del).Length;
    }

    private void SetUpFimCiclo()
    {
        if (_numColunas == 0) return;
        _fimCiclo = "";
        for (var i = 0; i < _numColunas - 1; i++)
        {
            _fimCiclo += "0" + Separador;
        }
        _fimCiclo += "0";
    }

    private void SetUpEnd()
    {
        if (_numColunas == 0) return;
        _fimTest = "";
        for (var i = 0; i < _numColunas - 1; i++)
        {
            _fimTest += "END" + Separador;
        }
        _fimTest += "END";
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

    // ReSharper disable once UnusedMember.Local
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
        _useDefaultDocName = false;
        _currentDocName    = _docName;
        _isInitiate        = false;
        _docName           = newName;
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
    
    private void SetUpHeader()
    {
        var header = _saveHeader == null ? _header : _saveHeader;

        WriteStringInDoc(header, true);
    }

    private string GetHeader()
    {
        return "Registo" + Separador + "Name" + Separador + "Position Object" + Separador + "Position Player" + Separador + "State" + Separador + "Time";
    }
}
//////////////////////////////////////////////////////////////////////////////////////////////////
/*
  
      private void SetUpFileAndDirectory()
    {
        _target = _directory + "\\" + _currentFolderDestino + "\\";
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
    
    //public void SetUpUserFolder(string userFolder)
    //{
    //    _currentUserFolder = userFolder;

    //    var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
    //    _currentFolderDestino =
    //        _defaultFolderDestino =
    //            _currentUserFolder == null
    //                ? "Files" + "\\" + "Saved Files" + "\\" + _folderName
    //                : "Files" + "\\" + "Saved Files" + "\\" + _currentUserFolder + "\\" + sessao + "\\" +  _folderName;

    //    _isInitiate = false;

    //    //_currentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "Collisions Data";
    //}

    //public void SetUpUserFolder()
    //{
    //    var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
    //    _currentFolderDestino =
    //        _defaultFolderDestino =
    //            _currentUserFolder == null
    //                ? "Files" + "\\" + "Saved Files" + "\\" + _folderName
    //                : "Files" + "\\" + "Saved Files" + "\\" + _currentUserFolder + "\\" + sessao + "\\" + _folderName;

    //    _isInitiate = false;
    //}


    // private string _startMessage;
    // _header = GetHeader();         
    //  public bool DirectoryChange;
    // private string _header;
    // private bool _isRecording;
    
    private void SetUpHeader(string first)
    {
        var info = GetHeader(); 
        if (first == info) return;
        WriteStringInDoc(info, true);
    }
    private void SetUpFileAndDirectory(string first)
    {
        SetUpDirectory();
        SetFileName();
        SetUpHeader(first);
        _isInitiate = true;
    }
    
    private string GetHeader()
    {
        return "Registo" + Separador + "Name" + Separador + "Position Object" + Separador + "Position Player" + Separador + "State" + Separador + "Time";       
    }
     
     
        //if (!_isInitiate) SetUpFileAndDirectory(message);
        //if (message != _startMessage  && !_isInitiate)
        //{
        //} else
     
     */

// _target = _directory + "\\" +_CurrentFolderDestino ;

//if (!IsRecording)
//{
//    StopRecording();
//    return;
//}
//CheckHeaders(message);