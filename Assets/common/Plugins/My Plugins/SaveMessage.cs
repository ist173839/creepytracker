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


public enum ControloMode
{
    KeyboardMouse,
    // ReSharper disable once InconsistentNaming
    WIP,
    // ReSharper disable once InconsistentNaming
    CWIP,
    WalkGear
}


public class SaveMessage
{
    private StreamWriter _doc;
    private DateTime _inicio;

    private ControloMode _activeControloMode;


    public string Separador { get; private set; }

    private readonly string _defaultFolderDestino;
    private readonly string _startMessage;
    private readonly string _endMessage;
    private readonly string _directory;
    private readonly string _format;

    private string _currentFolderDestino;
    private string _caminhoCompleto;
    private string _defaultDocName;
    private string _currentDocName;
    private string _recordingName;
    private string _folderDestino;
    private string _fimCiclo;
    private string _docName;
    private string _target;

    private string _header;

    private string _headerCwip;
    private string _headerWip;
    private string _headerWalkGear;
    private string _headerKeyboardMouse;

    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 30); // (2 ^ 30)

    private int _cont;

    private int _specialTypeDocName;

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;

    private bool _useDefaultFolder;

    private bool _isInitiate;

    private bool _isRecording;

    public bool IsRecording
    {
        get { return _isRecording; }
        set
        {

            if (!value)
            {
                _isInitiate = false;
                StopRecording();
            }

            _isRecording = value;
        }
    }


    public SaveMessage() 
    {
        _useDefaultFolder  = true;
        _useDefaultDocName = true;
        
        _directory = System.IO.Directory.GetCurrentDirectory();
        _currentFolderDestino = _defaultFolderDestino = "Walking Data";
        _format    = ".csv";
        Separador = ";";


        _startMessage = "INICIO";
        _endMessage   = "FIM";
        
        _caminhoCompleto = null;
        _specialTypeDocName = 0;
            
        _target = _directory + "\\" + _currentFolderDestino + "\\";
        
        _isInitiate = false;
        _recordingName = null;
        _cont = 0;
        NumColunas = 0;


        _activeControloMode  = ControloMode.CWIP;
        _headerCwip          = GetCwipHeader();
        _headerWip           = GetWipHeader();
        _headerWalkGear      = GetWalkGearHeader();
        _headerKeyboardMouse = GetKeyboardMouseHeader();
        _header = GetHeader();
    }

    ~SaveMessage()
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
        Debug.Log("New File, Current Size = " + info.Length + " ( MAX = " + TamanhoMaximo + " )");
        StopRecording();
    }

    public void RecordMessage(string message)
    {
        if (!IsRecording)
        {
            StopRecording();
            return;
        }
        CheckHeaders(message);

        if (!_isInitiate) SetUpFileAndDirectory(message);
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

    private void CheckHeaders(string message)
    {
        if (message == _headerCwip && _activeControloMode != ControloMode.CWIP)
        {
            _activeControloMode = ControloMode.CWIP;
            _isInitiate = false;
        }
        else if (message == _headerWip && _activeControloMode != ControloMode.WIP)
        {
            _activeControloMode = ControloMode.WIP;
            _isInitiate = false;
        }
        else if (message == _headerWalkGear && _activeControloMode != ControloMode.WalkGear)
        {
            _activeControloMode = ControloMode.WalkGear;
            _isInitiate = false;

        }
        else if (message == _headerKeyboardMouse && _activeControloMode != ControloMode.KeyboardMouse)
        {
            _activeControloMode = ControloMode.KeyboardMouse;
            _isInitiate = false;
        }


        //if (message == _headerCwip || message == _headerWip || message == _headerWalkGear || message == _headerKeyboardMouse)
        //{
        //    _isInitiate = false;
        //}
    }

    private string GetHeader()
    {
        switch (_activeControloMode)
        {
            case ControloMode.KeyboardMouse:
                return _headerKeyboardMouse;
            case ControloMode.WIP:
                return _headerWip;
            case ControloMode.CWIP:
                return _headerCwip;
            case ControloMode.WalkGear:
                return _headerWalkGear;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private string GetCwipHeader()
    {
        // "Metodo de Deslocamento Em Uso" + Separador + 

        return "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Estado Actual " + Separador +
               "Vel. Real (Directa, Normal)" + Separador + "Vel. Real (Directa, Kalman)" + Separador +
               "Vel. Virtual (WIP, Normal)" + Separador + "Vel. Virtual (WIP, Kalman)" + Separador +
               "Vel. Virtual (WIP, Event, Normal)" + Separador + "Vel. Virtual (WIP, Event, Kalman)" + Separador +
               "Vel. Virtual + Aumento (WIP)" + Separador + "Joint Vel. Real (Vector 2)" + Separador + "Joint Camera (Vector 3)" + Separador +
               "Joelho Direito (y)" + Separador + "Joelho Esquerdo (y)" + Separador +
               "Desvio Joelho Direito" + Separador + "Desvio Joelho Esquerdo" + Separador +
               "Direito FootStates (WIP)" + Separador + "Esquerdo FootStates (WIP)" + Separador +
               "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)" + Separador +
               "N. Passos Total (WIP)" + Separador + "N. Passos Direito (WIP)" + Separador + "N. Passos Esquerdo (WIP)" + Separador +
               "Altura" + Separador + "Threshold de Velocidade Directa" + Separador + "Threshold de Velocidade WIP" + Separador + "Threshold do Passo (WIP)" + Separador +
               "Nome Joint Vel. Real (Kinect)" + Separador + "Nome Joint Camera (Kinect)" + Separador + "Tempo" + Separador + "Aumento (WIP)" + Separador + "Id";
    }


    private string GetWipHeader()
    {
        return
            "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Estado Actual " + Separador +
            "Vel. Virtual (WIP, Normal)" + Separador + "Vel. Virtual (WIP, Kalman)" + Separador + "Vel. Virtual (WIP, Event, Normal)" + Separador + "Vel. Virtual (WIP, Event, Kalman)" + Separador +
            "Vel. Virtual + Aumento (WIP)" + Separador + "Joint Camera (Vector 3)" + Separador + "Joelho Direito (y)" + Separador + "Joelho Esquerdo (y)" + Separador +
            "Desvio Joelho Direito" + Separador + "Desvio Joelho Esquerdo" + Separador + "Direito FootStates (WIP)" + Separador + "Esquerdo FootStates (WIP)" + Separador +
            "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)" + Separador + "N. Passos Total (WIP)" + Separador + "N. Passos Direito (WIP)" + Separador +
            "N. Passos Esquerdo (WIP)" + Separador + "Altura" + Separador + "Threshold de Velocidade WIP" + Separador + "Threshold do Passo (WIP)" + Separador +
            "Nome Joint Camera (Kinect)" + Separador + "Tempo" + Separador + "Aumento (WIP)" + Separador + "Id";
    }


    private string GetWalkGearHeader()
    {
        return
            "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Estado Actual " + Separador +
            "Vel. Real (Directa, Normal)" + Separador + "Vel. Real (Directa, Kalman)" + Separador + "Joint Vel. Real (Vector 2)" + Separador + "Joint Camera (Vector 3)" + Separador +
            "Threshold de Velocidade Directa" + Separador + "Nome Joint Vel. Real (Kinect)" + Separador + "Nome Joint Camera (Kinect)" + Separador + "Tempo" + Separador +
            "Id";
    }

    private string GetKeyboardMouseHeader()
    {
        return "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Tempo";
    }
    
    private void SetUpHeader()
    {
        // _positionThreshold,  (_numSteps) 

        var info = GetHeader(); // _header;//

        WriteStringInDoc(info, true);
    }

    private void SetUpHeader(string first)
    {
        // _positionThreshold,  (_numSteps) 

        var info = GetHeader(); // _header;//

        if (first == info) return;
        WriteStringInDoc(info, true);
    }

    private void SetUpFileAndDirectory()
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        SetUpDirectory();
        SetFileName();
        SetUpHeader();
        _isInitiate = true;
    }

    private void SetUpFileAndDirectory(string first)
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        SetUpDirectory();
        SetFileName();
        SetUpHeader(first);
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
            _currentDocName = "WVD_V9_" + DateTime.Now.ToString("yyyyMMddTHHmmss") ;
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

    public string GetDocActivo()
    {
        if (_isInitiate) return _target + _currentDocName;
        return null;
    }

    public void SetRecordingName(string recordName)
    {
        if (recordName.Equals(_recordingName)) return;
        _recordingName = recordName;
        _isInitiate = false;
    }

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

    public void UseDefaultDocName()
    {
        if (_useDefaultDocName) return;
        _useDefaultDocName = true;
        _isInitiate = false;
    }

    public void UseDefaultFolderName()
    {
        if (_useDefaultFolder) return;
        _useDefaultFolder = true;
        _isInitiate = false;
        _currentFolderDestino = _defaultFolderDestino;
    }

}
/*
 
     
      private string GetHeader()
    {
        
        return "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador +
            "Vel. Real (Directa, Normal)" + Separador + "Vel. Real (Directa, Kalman)" + Separador +
            "Vel. Virtual (WIP, Normal)" + Separador + "Vel. Virtual (WIP, Kalman)" + Separador + "Vel. Virtual (WIP, Event, Normal)" + Separador +
            "Vel. Virtual + Aumento (WIP)" + Separador + "Joint Vel. Real (Vector 2)" + Separador + "Joint Camera (Vector 3)" + Separador +
            "Joelho Direito (y)" + Separador + "Joelho Esquerdo (y)" + Separador +
            "Desvio Joelho Direito" + Separador + "Desvio Joelho Esquerdo" + Separador +
            "Direito FootStates (WIP)" + Separador + "Esquerdo FootStates (WIP)" + Separador +
            "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)" + Separador +
            "N. Passos Total (WIP)" + Separador + "N. Passos Direito (WIP)" + Separador + "N. Passos Esquerdo (WIP)" + Separador +
            "Altura" + Separador + "Threshold de Velocidade Directa" + Separador + "Threshold de Velocidade WIP" + Separador + "Threshold do Passo (WIP)" + Separador +
            "Nome Joint Vel. Real (Kinect)" + Separador + "Nome Joint Camera (Kinect)" + Separador + "Tempo" + Separador + "Aumento (WIP)" + Separador + "Id";
    }
     
     
     
     
     */
