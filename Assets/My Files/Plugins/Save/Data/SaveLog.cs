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
public class SaveLog
{
    private StreamWriter _doc;

    private DateTime _inicio;

    private SpecialTypeDoc _specialTypeDocName;
    
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
    private string _folderDestino;
    private string _startMessage;
    private string _endMessage;
    private string _folderName;
    private string _saveHeader;
    private string _directory;
    private string _fimTest;
    private string _fimCiclo;
    private string _docName;
    private string _target;
    private string _format;
    private string _versao;
    private string _sigla;
    private string _defaultName;

    private string _lastMessage;

    private int _numColunas; //  { get; private set; }

    public int FinalNum     { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

    private int _cont;
    
    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isRecording;
    private bool _isInitiate;
    private bool _oversize;
    
  
    /////////////////////////////////////////////////////////////////////////////
    public SaveLog(string userFolder)
    {
        SetUp(userFolder);
    }

    public SaveLog()
    {
        SetUp(null);
    }

    ~SaveLog()
    {
        StopRecording();
    }

    private void SetUp(string userFolder)
    {
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _isInitiate        = false;
        _oversize          = false;

        _directory = System.IO.Directory.GetCurrentDirectory();
        
        _folderName = "Log Data";
        _defaultName = "Default User Name - Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");

        SetUpUserFolder(userFolder);
        Separador = ";";

        _format = ".csv";
        _sigla  = "LD";
        _versao = "V4";

        _startMessage = "INICIO";
        _endMessage   = "FIM";

        UserLevelName = null;
        NumTest       = null; // "1";
        // TypeMet       = "A";
        
        _caminhoCompleto = null;
        _saveHeader      = null;
        _fimTest         = null;
        _lastMessage     = null;
        _specialTypeDocName = 0;

        _target = _directory + "\\" + _currentFolderDestino + "\\";
        _cont = 0;
        _numColunas = 0;

        FinalNum = -1;
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

        if (NumTest       != null) tempPath +=  "Teste nº " + NumTest + "\\";

        tempPath += sessao + "\\";
        
        _currentFolderDestino = _defaultFolderDestino = constPath + tempPath + _folderName;
        
        _isInitiate = false;
    } 

    public void ResetFinalNum()
    {
        FinalNum = -1;
    }

    private void ResetRecord()
    {
        //_recordingName = null;
        _isInitiate = false;
        _cont       = 0;
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
        //if (message == _lastMessage) return;
        //_lastMessage = message;

        if (message == _endMessage)
        {
            Debug.Log(_endMessage);
            StopRecording();
            return;
        }

        //Debug.Log("message =  " + message);
        if (message.Contains("Registo")) StopRecording();

        if (!_isInitiate)
        {
            SetUpFileAndDirectory(message);
            if (message.Contains("Registo"))
            {
                _saveHeader = message;
                WriteStringInDoc(_saveHeader, true);
                return;
            }

            var newHeader = _saveHeader != null ? _saveHeader + " (*)" : message;

            SetUpLevelNameAndFinalNum(message);

            WriteStringInDoc(newHeader, true);
            return;
        }

        if (!message.Contains("Registo"))
            SetUpLevelNameAndFinalNum(message);
        
            WriteStringInDoc(message, true);
     
        
        CheckFileSize();
    }

    private void SetUpLevelNameAndFinalNum(string message)
    {
        char[] del = {';'};
        var lineText = message.Split(del);
        FinalNum = int.Parse(lineText[1]);
        var userLevelName = lineText[2];
        if (userLevelName != null && userLevelName != UserLevelName)
        {
            UserLevelName = userLevelName;
            SetUpUserFolder();
        }
        UserLevelName = userLevelName;
        // Debug.Log("FinalNum = " + FinalNum);
        // Debug.Log("UserLevelName = " + UserLevelName);
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

    private void SetUpHeader()
    {
        // if (first.Contains("Registo")) return;
        if (_saveHeader != null)
        {
            Debug.Log("ERRO");
            WriteStringInDoc(_saveHeader, true);
        }
    }
    
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
    
    private void WriteStringInDoc(string registo)
    {
        _doc = new StreamWriter(_target + _currentDocName);
        _doc.WriteLine(registo);
        _doc.Close();
    }

    private void StopRecording()
    {
        if (!_isInitiate) return;
        if (!File.Exists(_target + _currentDocName)) return;
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
        ResetRecord();
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

    // ReSharper disable once UnusedMember.Global
    public string GetDocActivo()
    {
        if (_isInitiate) return _target + _currentDocName;
        return null;
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

}

/////////////////////////////////////////////////////////////////////////////////////////

/*
        
        // else
        //{
        //    if (!_isInitiate)
        //    {
        //        SetUpFileAndDirectory(message); 
        //        if (_saveHeader != null) WriteStringInDoc(_saveHeader + " (*)", true);
        //    } 
        //}
       var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
       _currentFolderDestino =
           _defaultFolderDestino =
               _currentUserFolder == null
                   ? "Files" + "\\" + "Saved Files" + "\\" + _folderName
                   : "Files" + "\\" + "Saved Files" + "\\" + _currentUserFolder + "\\" + sessao + "\\" + _folderName;

       _isInitiate = false;

  /
//if (_currentUserFolder == null)
//{
//    tempPath = null;
//}
//{
//    tempPath = _currentUserFolder + "\\" + sessao ;
//}
//   _folderName + "\\" + _folderName

public void SetUpUserFolder(string userFolder)
    {
        _currentUserFolder = userFolder;

        var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
        _currentFolderDestino =
            _defaultFolderDestino =
                _currentUserFolder == null
                    ? "Files" + "\\" + "Saved Files" + "\\" + _nameFolder
                    : "Files" + "\\" + "Saved Files" + "\\" + _currentUserFolder + "\\" + sessao + "\\" + _nameFolder;

        _isInitiate = false;
    }
    
    _currentUserFolder = userFolder;
        var sessao = "Time " + DateTime.Now.ToString("yyyyMMddTHHmmss");
        _currentFolderDestino =
            _defaultFolderDestino =
                _currentUserFolder == null
                    ? "Files" + "\\" + "Saved Files" + "\\" + _nameFolder
                    : "Files" + "\\" + "Saved Files" + "\\" + _currentUserFolder + "\\" + sessao + "\\" + _nameFolder;

    // if (!_isInitiate) return;
    // ResetRecord();
    // if (File.Exists(_target + _currentDocName)) File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);      

    
    private void SetUpFileAndDirectory(string first)
    {
        
        
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        SetUpDirectory();
        SetFileName();
        //SetUpHeader(first);
        _isInitiate = true;
    }

 */
