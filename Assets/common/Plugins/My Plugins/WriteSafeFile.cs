/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
public static class MyMessaSepa
{
    public const string InicioOptitrack = "&";
    public const string InicioMensagem = "$";
    public const string InicioRegisto = "«";
    public const string SepaOptitrack = "!";
    public const string SepaCabeRoda = ";";
    public const string SepaRegisto = "_";
    public const string CabeRoda = "£";
    public const string SepaVec = "|";
    public const string SepaCol = "+";

}
// ReSharper disable once CheckNamespace
public class WriteSafeFile
{
    private StreamWriter _doc;

    // ReSharper disable once MemberCanBePrivate.Global
    public enum VersaoDisponivel
    {
        Versao2,
        Versao3,
        NaoActivo,
    }

    private VersaoDisponivel _versaoActiva;

    private DateTime _tempoMensagemAnterior;
    private DateTime _inicio;

    // ReSharper disable once MemberCanBePrivate.Global
    public string CurrentFolderDestino;
    // ReSharper disable once MemberCanBePrivate.Global
    public string FolderDestino;
    // ReSharper disable once MemberCanBePrivate.Global
    public string InfoCompl;
    // ReSharper disable once MemberCanBePrivate.Global
    public string InfoExtra;
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public string Directory;
    // ReSharper disable once MemberCanBePrivate.Global
    public string DocName;

    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _defaultFolderDestino;
#pragma warning disable 414
    private string _caminhoCompleto;
#pragma warning restore 414
#pragma warning disable 169
    private string _defaultDocName;
#pragma warning restore 169
    private string _currentDocName;
    private string _fileName;
    private string _target;
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private string _format;

    private int _specialTypeDocName;
    private int _cont;

    private static readonly int TamanhoMaximo = 2 * (int) Math.Pow(2,20); //2 * (2^20)

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _activo;
    private bool _first;

    private bool _isRecording;

    public bool IsRecording
    {
        // ReSharper disable once MemberCanBePrivate.Global
        get
        {
            return _isRecording;
        }
        set
        {
            if (value == false) StopRecording("Turn Off");
            _isRecording = value;
        }
    }

    public WriteSafeFile()
    {
    #if !UNITY_ANDROID || UNITY_EDITOR
        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _activo            = false;
        _first             = true;
        Directory = System.IO.Directory.GetCurrentDirectory();
        CurrentFolderDestino = _defaultFolderDestino = "Recordings";
        _format = ".txt";
        // _defaultDocName = "UTD_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt"; // UserTrakerData
        InfoCompl = "NADA";
        InfoExtra = null;
        _caminhoCompleto = null;
        _fileName = null;
        _specialTypeDocName = 0;
	    _cont = 0;
        _versaoActiva = VersaoDisponivel.NaoActivo;
    #endif
    }

    ~WriteSafeFile()
    {
        ResetMessage();
#if !UNITY_ANDROID || UNITY_EDITOR
        
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        _doc = new StreamWriter(_target + _currentDocName, true);
        if (InfoExtra != null) _doc.WriteLine(InfoExtra);
        _doc.WriteLine(infoFinal);
        _doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
#endif
    }

    private void ResetMessage()
    {
        _activo = false;
        _versaoActiva = VersaoDisponivel.NaoActivo;
        _cont = 0;
        InfoCompl = "NADA";
        InfoExtra = null;
        //  NumColunas = 0;
    }

    // ReSharper disable once UnusedMember.Global
    public string GetCurrentFileName()
    {
        return _fileName;
    }

    // ReSharper disable once UnusedMember.Global
    public void AddExtraInfo(string extra)
    {
        InfoExtra += "£ExtraInfo: " + extra;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialFolderName(string newName)
    {
        CurrentFolderDestino = FolderDestino = newName;
        _useDefaultFolder = false;
        _activo = false;
        _versaoActiva = VersaoDisponivel.NaoActivo;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialDocName(string newName)
    {
        DocName = newName;
        _useDefaultDocName = false;
        _currentDocName = DocName;
        _activo = false;
        _versaoActiva = VersaoDisponivel.NaoActivo;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialTypeDocName(int t)
    {
        _specialTypeDocName = t;      
    }
    
    // ReSharper disable once UnusedMember.Global
    public void UseDefaultDocName()
    {
        // ReSharper disable once InvertIf
        if (!_useDefaultDocName)
        {
            _useDefaultDocName = true;
            _activo = false;
            _versaoActiva = VersaoDisponivel.NaoActivo;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public void UseDefaultFolderName()
    {
        // ReSharper disable once InvertIf
        if (!_useDefaultFolder)
        {
            _useDefaultFolder = true;
            _activo = false;
            CurrentFolderDestino = _defaultFolderDestino;
            _versaoActiva = VersaoDisponivel.NaoActivo;
        }
    }

    private void CheckFileSize()
    {
        if (!_activo) return;
        if (!File.Exists(_target + _currentDocName)) return;
        var info = new FileInfo(_target + _currentDocName);
        if (info.Length < TamanhoMaximo) return;
        Debug.Log("New File, Current Size = " + info.Length + " ( MAX = " + TamanhoMaximo + " )");
        StopRecording("MAX_SIZE");
    }

    public void Recording(string mensagemKinect)
    {
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //Debug.Log("Pasta : " + _target);
        CheckFileSize();
        if (!_activo || _versaoActiva == VersaoDisponivel.NaoActivo) NovoRegisto(VersaoDisponivel.Versao2);
        //_caminhoCompleto = _target + _CurrentDocName;
        _doc = new StreamWriter(_target + _currentDocName, true);
        var agora = DateTime.Now;
        if (_first)
        {
            _tempoMensagemAnterior = agora;
            _first = false;
        }
        var diff          = agora - _inicio;
        var diffIntervalo = agora - _tempoMensagemAnterior;
        var registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$"+ mensagemKinect;
        _doc.WriteLine(registo);
        _doc.Close(); // Todo Apenas um Doc.Close

        _tempoMensagemAnterior = agora;
        if (!IsRecording) StopRecording("Turn Off");
    }
    
    public void Recording(string mensagemKinect, Vector3 pos, Quaternion ori)
    {
    #if !UNITY_ANDROID  || UNITY_EDITOR
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //Debug.Log("Pasta : " + _target);
        CheckFileSize();
        if (!_activo || _versaoActiva != VersaoDisponivel.Versao3) NovoRegisto(VersaoDisponivel.Versao3);
        //_caminhoCompleto = _target + _CurrentDocName;
        _doc = new StreamWriter(_target + _currentDocName, true);
        var agora = DateTime.Now;
        if (_first)
        {
            _tempoMensagemAnterior = agora;
            _first = false;
        }
        var diff = agora - _inicio;// 
        var diffIntervalo = agora - _tempoMensagemAnterior;
        //string registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$" + mensagemKinect + "&" + pos;
        var registo = MyMessaSepa.InicioRegisto    + _cont++ + 
                         MyMessaSepa.SepaRegisto      + diff.TotalSeconds +
                         MyMessaSepa.SepaRegisto      + diffIntervalo.TotalSeconds +
                         MyMessaSepa.SepaRegisto      + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + 
                         MyMessaSepa.InicioOptitrack  + 
                                                      Vector3ToString(pos)    + MyMessaSepa.SepaOptitrack + 
                                                      QuaternionToString(ori) + MyMessaSepa.SepaOptitrack + 
                         MyMessaSepa.InicioOptitrack  +
                         MyMessaSepa.InicioMensagem   + mensagemKinect;

        _doc.WriteLine(registo);
        _doc.Close(); 

        _tempoMensagemAnterior = agora;
#endif
    }

    private static string Vector3ToString(Vector3 vector3)
    {
        return vector3.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec + 
               vector3.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec + 
               vector3.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," });
    }

    private static string QuaternionToString(Quaternion quaternion)
    {
        return quaternion.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.w.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," });
    }

    private void StopRecording()
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n"; 
        _doc = new StreamWriter(_target + _currentDocName, true);
        if (InfoExtra != null) _doc.WriteLine(InfoExtra);
        _doc.WriteLine(infoFinal);
        _doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
        
    }

    public void StopRecording(string mensagemFinal)
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£Mensagem_Final_" + mensagemFinal + "£"+ DateTime.Now.ToString("yyyyMMddTHHmmssff") + "£Fim£";
        _doc = new StreamWriter(_target + _currentDocName, true);
        _doc.WriteLine(infoFinal);
        _doc.Close();
    }

    public string GetDocActivo()
    {
        if (_activo)
            return _target + _currentDocName;
        return null;
    }

    private void NovoRegisto(VersaoDisponivel versao)
    {
    #if !UNITY_ANDROID || UNITY_EDITOR
        // _target = _directory + "\\" +_CurrentFolderDestino ;
        //  Para prevenir erros, não é uma situação esperada
        _versaoActiva = versao == VersaoDisponivel.NaoActivo ? VersaoDisponivel.Versao2 : versao;
        if (!System.IO.Directory.Exists(_target))
        {
            System.IO.Directory.CreateDirectory(_target);
            Debug.Log("Criar Nova Pasta : " + _target);
        }
        _activo = true;
        _inicio = DateTime.Now;
        string prefixoVersao;
        if (_useDefaultDocName)
        {
            prefixoVersao = _versaoActiva == VersaoDisponivel.Versao3 ? "UTD_V3_" : "UTD_V2_";
            _fileName = prefixoVersao  + DateTime.Now.ToString("yyyyMMddTHHmmss");
            _currentDocName = _fileName;// + _format; // UserTrakerData (Versão 3)
            _currentDocName = SolveDuplicateFileNames();
            _currentDocName += _format;
        }
        else
        {
            string temp;
            // Debug.Log("C " + temp + "\n  " + _target + temp + _format + "  Bool "+ File.Exists(_target + temp + _format));
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
                    Debug.Log("ERRO: Valor invalido em _specialTypeDocName = " + _specialTypeDocName + "Nome default: " + temp);
                break;
            }
            _currentDocName = temp + _format;
        }
        SetInfoCompl();
        // £ usado para não interferir com a mensagemKinect £
        prefixoVersao = _versaoActiva == VersaoDisponivel.Versao3 ? "V3" : "V2";
        var info =
            "£Registo User Tracker Data;" + prefixoVersao + ";Tese de Mestrado de Francisco Henriques Venda, 73839;Inicio de Registo : " +
            _inicio.ToString("dd/MM/yyyy, HH:mm:ss") + ";Pasta Original : " + _target + ";Nome Ficheiro Original : " +
            _currentDocName + ";Info complementar : " + InfoCompl + ";£";
        // File.OpenWrite(_target +  _CurrentDocName);
        _doc = new StreamWriter(_target + _currentDocName, true);
        _doc.WriteLine(info);
        _doc.Close();
        Debug.Log("Criar Novo Ficheiro : " + _currentDocName);
        // Debug.Log("info : " + info);
    #endif
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
    
    private void SetInfoCompl()
    {
        #if UNITY_EDITOR

            if (InfoCompl == "NADA")
            {
                InfoCompl = "EDITOR";
            }
            else
            {
                InfoCompl += ", EDITOR";
            }
        #endif
        #if UNITY_ANDROID //&& !UNITY_EDITOR
                   
            if (InfoCompl == "NADA")
            {
                InfoCompl = "Android (Gear Vr)";
            }
            else
            {
                InfoCompl += ", Android (Gear Vr)";
            }
        #endif
        #if (UNITY_STANDALONE_WIN) // && !UNITY_EDITOR
             
            if (InfoCompl == "NADA")
            {
                InfoCompl = "Windows)";
            }
            else
            {
                InfoCompl += ", Windows";
            }
        #endif 
             
    } 
}
    /*
 
             
        //string registo1 = "«" + _cont + //  "_" +
        //                  "_" + diff.TotalSeconds +
        //                  "_" + diffIntervalo.TotalSeconds +
        //                  "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") +
        //                  "&" + Vector3ToString(pos) + "!" + QuaternionToString(ori) + "!" +
        //                  "&" + "$" + mensagemKinect;



     
     */
