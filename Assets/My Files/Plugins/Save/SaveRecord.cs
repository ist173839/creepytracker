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
public enum ControloMode
{
    // ReSharper disable once InconsistentNaming
    WIP,
    // ReSharper disable once InconsistentNaming
    CWIP,
  
}

//public enum SpecialTypeDoc
//{
//    SolveDuplicate,
//    Normal,
//}

public class SaveRecord
{
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
    private string _saveHeader;
#pragma warning disable 169
    private string _fimCiclo;
#pragma warning restore 169
    private string _docName;
    private string _target;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _versao;
    // ReSharper disable once NotAccessedField.Local
    private string _header;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _sigla;

    //private string _headerCwip;
    //private string _headerWip;
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 20); // (2 ^ 30)

#pragma warning disable 414
    private int _cont;
#pragma warning restore 414

    private SpecialTypeDoc _specialTypeDocName;

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
#pragma warning disable 169
    private bool _isRecording;
#pragma warning restore 169
    private bool _isInitiate;
    private bool _oversize;

    public SaveRecord() 
    {
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _isInitiate        = false;
        _oversize          = false;

        _directory = System.IO.Directory.GetCurrentDirectory();
        _currentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "Walking Data";
        Separador  = ";";

        _startMessage = "INICIO";
        _endMessage   = "FIM";
        _format       = ".csv";
        _versao       = "V11.7";
        _sigla        = "WVD";
        
        _recordingName   = null;
        _caminhoCompleto = null;
        _specialTypeDocName = SpecialTypeDoc.SolveDuplicate;
        
        _target = _directory + "\\" + _currentFolderDestino + "\\";
        _cont = 0;
        NumColunas = 0;
     
        _header = GetHeader();
        _saveHeader = null;
    }

    ~SaveRecord()
    {
        if (!_isInitiate) return;
        ResetRecord();
        if (File.Exists(_target + _currentDocName))
            File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);        
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
        //if (!IsRecording)
        //{
        //    StopRecording();
        //    return;
        //}
        //CheckHeaders(message);

        if (message.Contains("Registo"))
        {
            _saveHeader = message;
            _isInitiate = false;
            //_cont = 0;
        }
        if (!_isInitiate) SetUpFileAndDirectory();

        //if (!_isInitiate) SetUpFileAndDirectory(message);
        //if (message != _startMessage  && !_isInitiate)
        //{} else

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
            SetUpSaveHeader();
            _oversize = false;
        }
        //SetUpHeader();
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

    private string GetHeader()
    {
        return
           "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Estado Actual" + Separador + "Vel. Real (Directa, Normal)" + Separador + "Vel. Real (Directa, Kalman)" + Separador +
            "Vel. Virtual in use (WIP)" + Separador + "Vel. Virtual (WIP, Normal)" + Separador + "Vel. Virtual (WIP, Kalman)" + Separador + "Vel. Virtual (WIP, Event, Normal)" + Separador + "Vel. Virtual (WIP, Event, Kalman)" + Separador +
            "Vel. Virtual * Aumento (WIP)" + Separador + "Vel. Virtual * Aumento (WIP) * Delta" + Separador + "Delta" + Separador + "Joint Vel. Real (Vector 2)" + Separador + "Joint Camera (Vector 3)" + Separador + "Joelho Direito (y)" + Separador + "Joelho Esquerdo (y)" + Separador +
            "Desvio Joelho Direito" + Separador + "Desvio Joelho Esquerdo" + Separador + "Direito FootStates (WIP)" + Separador + "Esquerdo FootStates (WIP)" + Separador + "Direito FootTransitionEvents (WIP)" + Separador +
            "Esquerdo FootTransitionEvents (WIP)" + Separador + "N. Passos Total (WIP)" + Separador + "N. Passos Direito (WIP)" + Separador + "N. Passos Esquerdo (WIP)" + Separador + "Distancia Direct" + Separador + "Distancia Wip" + Separador +
            "Distancia do anterior" + Separador + "Altura" + Separador + "Threshold de Velocidade Directa" + Separador + "Threshold de Velocidade WIP" + Separador + "Threshold do Passo (WIP)" + Separador + "Velocidade Inicial WIP" + Separador +
            "Nome Joint Vel. Real" + Separador + "Nome Joint Camera" + Separador + "Tempo" + Separador + "Aumento (WIP)" + Separador + "Id" + Separador + "Nivel" + Separador + "WIP Mode" + Separador + "Vel. Real (Directa, Kalman, Base)" + Separador +
            "Begin Stop Active" + Separador + "Direito FootStates (WIP, Int)" + Separador + "Esquerdo FootStates (WIP, Int)" + Separador + "Direito FootTransitionEvents (WIP, Int)" + Separador + "Esquerdo FootTransitionEvents (WIP, Int)" + Separador +
            "Joelho Direito (y, Kalman)" + Separador + "Joelho Esquerdo (y, Kalman)"
            ;
    }

    // ReSharper disable once UnusedMember.Local
    private void SetUpHeader()
    {
        // _positionThreshold,  (_numSteps) 
        var info = GetHeader(); // _header;//
        WriteStringInDoc(info, true);
    }

    private void SetUpSaveHeader()
    {
        // _positionThreshold,  (_numSteps) 
        // var info = GetHeader(); // _header;//
        WriteStringInDoc(_saveHeader, true);
    }

    private void SetUpHeader(string first)
    {
        // _positionThreshold,  (_numSteps) 
        var info = GetHeader(); // _header;//
        if (first == info) return;
        WriteStringInDoc(info, true);
    }

    // ReSharper disable once UnusedMember.Local
    private void SetUpFileAndDirectory(string first)
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        SetUpDirectory();
        SetFileName();
        SetUpHeader(first);
        _isInitiate = true;
    }
}
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*


  private string GetHeader()
{
    return
        "Registo" + Separador + "Tempo Absoluto (Segundos)" + Separador + "Metodo de Deslocamento Em Uso" + Separador + "Estado Actual " + Separador +
        "Vel. Real (Directa, Normal)" + Separador + "Vel. Real (Directa, Kalman)" + Separador + "Vel. Virtual in use (WIP)" + Separador +
        "Vel. Virtual (WIP, Normal)" + Separador + "Vel. Virtual (WIP, Kalman)" + Separador +
        "Vel. Virtual (WIP, Event, Normal)" + Separador + "Vel. Virtual (WIP, Event, Kalman)" + Separador +
        "Vel. Virtual * Aumento (WIP)" + Separador + "Vel. Virtual * Aumento (WIP) * Delta" + Separador + "Delta" + Separador +
        "Joint Vel. Real (Vector 2)" + Separador + "Joint Camera (Vector 3)" + Separador + "Joelho Direito (y)" + Separador + "Joelho Esquerdo (y)" + Separador +
        "Desvio Joelho Direito" + Separador + "Desvio Joelho Esquerdo" + Separador + "Direito FootStates (WIP)" + Separador + "Esquerdo FootStates (WIP)" + Separador +
        "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)" + Separador +
        "N. Passos Total (WIP)" + Separador + "N. Passos Direito (WIP)" + Separador + "N. Passos Esquerdo (WIP)" + Separador +
        "Distancia Direct" + Separador + "Distancia Wip" + Separador + "Distancia do anterior" + Separador +
        "Altura" + Separador + "Threshold de Velocidade Directa" + Separador + "Threshold de Velocidade WIP" + Separador + "Threshold do Passo (WIP)" + Separador +
        "Velocidade Inicial WIP" + Separador + "Nome Joint Vel. Real" + Separador + "Nome Joint Camera" + Separador + "Tempo" + Separador + "Aumento (WIP)" + Separador +
        "Id" + Separador + "Nivel"; ;
}






    // _activeControloMode  = ControloMode.CWIP;
    // _headerCwip          = GetCwipHeader();
    // _headerWip           = GetWipHeader();


 */
