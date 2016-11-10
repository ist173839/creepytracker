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
//public static class MyMessaSepa
//{
//    public const string InicioOptitrack = "&";
//    public const string InicioMensagem  = "$";
//    public const string InicioRegisto   = "«";
//    public const string SepaOptitrack   = "!";
//    public const string SepaCabeRoda    = ";";
//    public const string SepaRegisto     = "_";
//    public const string CabeRoda        = "£";
//    public const string SepaVec         = "|";
//    public const string SepaCol         = "+";
//}


// ReSharper disable once CheckNamespace
public enum SpecialTypeDoc
{
    SolveDuplicate,
    Normal,
}

// ReSharper disable once CheckNamespace
// ReSharper disable once CheckNamespace
public class SafeWriteFile
{
    private DateTime _inicio;
    private DateTime _tempoMensagemAnterior;

    private StreamWriter _doc;

    private SpecialTypeDoc _specialTypeDocName;

    public string CurrentFolderDestino;
    public string FolderDestino;
    public string Directory;
    public string InfoCompl;
    public string InfoExtra;
    public string DocName;

    private string _defaultFolderDestino;
    private string _caminhoCompleto;
    private string _defaultDocName;
    private string _currentDocName;
    private string _saveHeader;
    private string _fileName;
    private string _target;
    private string _format;
    private string _versao;
    private string _sigla;


    private int _cont;

    private static readonly int TamanhoMaximo = 2 * (int) Math.Pow(2, 20); //2 * (2^20)

    //  public bool DirectoryChange;

    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isRecording;
    private bool _oversize;
    private bool _activo;
    private bool _first;

    public bool IsRecording
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedMember.Global
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
    
    public SafeWriteFile()
    {
#if !UNITY_ANDROID || UNITY_EDITOR

        _specialTypeDocName = SpecialTypeDoc.SolveDuplicate;

        _useDefaultDocName = true;
        _useDefaultFolder  = true;
        _activo            = false;
        _first             = true;
        _oversize          = false;

        Directory = System.IO.Directory.GetCurrentDirectory();

        CurrentFolderDestino = _defaultFolderDestino = "Saved Files" + "\\" + "Recordings";

        InfoCompl = "NADA";

        _format = ".txt";
        _versao = "V4";
        _sigla  = "UTD";

        InfoExtra = null;

        _caminhoCompleto = null;
        _fileName        = null;
        _saveHeader      = null;

        _cont = 0;
        
        // _defaultDocName = "UTD_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt"; // UserTrakerData
        // _specialTypeDocName = 0;
        // _versaoActiva = VersaoDisponivel.NaoActivo;
#endif
    }

    ~SafeWriteFile()
    {
        ResetMessage();
#if !UNITY_ANDROID || UNITY_EDITOR

        EndMessage();
#endif
    }

    private void EndMessage()
    {
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        _doc = new StreamWriter(_target + _currentDocName, true);
        if (InfoExtra != null) _doc.WriteLine(InfoExtra);
        _doc.WriteLine(infoFinal);
        _doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
    }

    private void ResetMessage()
    {
        _activo = false;
        _cont = 0;
        InfoCompl = "NADA";
        InfoExtra = null;
        // _versaoActiva = VersaoDisponivel.NaoActivo;
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
        //_versaoActiva = VersaoDisponivel.NaoActivo;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialDocName(string newName)
    {
        DocName = newName;
        _useDefaultDocName = false;
        _currentDocName = DocName;
        _activo = false;
        //_versaoActiva = VersaoDisponivel.NaoActivo;
    }

    // ReSharper disable once UnusedMember.Global
    public void SpecialTypeDocName(SpecialTypeDoc t)
    {
        _specialTypeDocName = t;
    }

    // ReSharper disable once UnusedMember.Global
    public void UseDefaultDocName()
    {
        if (!_useDefaultDocName)
        {
            _useDefaultDocName = true;
            _activo = false;
            //_versaoActiva = VersaoDisponivel.NaoActivo;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public void UseDefaultFolderName()
    {
        if (!_useDefaultFolder)
        {
            _useDefaultFolder = true;
            _activo = false;
            CurrentFolderDestino = _defaultFolderDestino;
            //_versaoActiva = VersaoDisponivel.NaoActivo;
        }
    }

    private void CheckFileSize()
    {
        if (!_activo) return;
        if (!File.Exists(_target + _currentDocName)) return;
        var info = new FileInfo(_target + _currentDocName);
        if (info.Length < TamanhoMaximo) return;
        _oversize = true;
        MyDebug.Log("New File, Current Size = " + info.Length + " ( MAX = " + TamanhoMaximo + " )");
        StopRecording("MAX_SIZE");
    }

    public void Recording(string mensagem)
    {
#if !UNITY_ANDROID  || UNITY_EDITOR
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //MyDebug.Log("Pasta : " + _target);
        CheckFileSize();
        //if (!_activo || _versaoActiva == VersaoDisponivel.NaoActivo) NovoRegisto(VersaoDisponivel.Versao2);
        if (!_activo) NovoRegisto();
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

        var registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$" + mensagem;
        _doc.WriteLine(registo);
        _doc.Close();

        _tempoMensagemAnterior = agora;
#endif
    }

    public void Recording(string mensagem, Vector3 pos, bool useCentre)
    {
#if !UNITY_ANDROID  || UNITY_EDITOR
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //MyDebug.Log("Pasta : " + _target);
        CheckFileSize();
        //if (!_activo || _versaoActiva != VersaoDisponivel.Versao3) NovoRegisto(VersaoDisponivel.Versao3);
        //_caminhoCompleto = _target + _CurrentDocName;
        if (!_activo) NovoRegisto();

        _doc = new StreamWriter(_target + _currentDocName, true);
        var agora = DateTime.Now;
        if (_first)
        {
            _tempoMensagemAnterior = agora;
            _first = false;
        }

        var diff = agora - _inicio;// 
        var diffIntervalo = agora - _tempoMensagemAnterior;

        // string registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$" + mensagem + "&" + pos;

        var registo = 
            MyMessaSepa.InicioRegisto + _cont++ + //  "_" +
            MyMessaSepa.SepaRegisto + diff.TotalSeconds +
            MyMessaSepa.SepaRegisto + diffIntervalo.TotalSeconds +
            MyMessaSepa.SepaRegisto + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") +
            MyMessaSepa.InicioOptitrack + 
                StringHelper.Vector3ToStringV1(pos)  + MyMessaSepa.SepaOptitrack +
                useCentre.ToString()                 + MyMessaSepa.SepaOptitrack +
            MyMessaSepa.InicioOptitrack +
            MyMessaSepa.InicioMensagem + mensagem;

        _doc.WriteLine(registo);
        _doc.Close();

        _tempoMensagemAnterior = agora;
#endif
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
        var infoFinal = "£Mensagem_Final_" + mensagemFinal + "£" + DateTime.Now.ToString("yyyyMMddTHHmmssff") + "£Fim£";
        _doc = new StreamWriter(_target + _currentDocName, true);
        _doc.WriteLine(infoFinal);
        _doc.Close();
    }

    // ReSharper disable once UnusedMember.Global
    public string GetDocActivo()
    {
        if (_activo)
            return _target + _currentDocName;
        return null;
    }

    // private void NovoRegisto(VersaoDisponivel versao)
    private void NovoRegisto()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        // _target = _directory + "\\" +_CurrentFolderDestino ;

        //  Para prevenir erros, não é uma situação esperada

        // _versaoActiva = versao == VersaoDisponivel.NaoActivo ? VersaoDisponivel.Versao2 : versao;

        if (!System.IO.Directory.Exists(_target))
        {
            System.IO.Directory.CreateDirectory(_target);
            MyDebug.Log("Criar Nova Pasta : " + _target);
        }

        _activo = true;
        _inicio = DateTime.Now;
        // var prefixoVersao = _sigla + "_" + _versao + "_";
        if (_useDefaultDocName)
        {
            // prefixoVersao = _versaoActiva == VersaoDisponivel.Versao3 ? "UTD_V3_" : "UTD_V2_";
            _fileName = _sigla + "_" + _versao + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            _currentDocName = _fileName;// + _format; // UserTrakerData (Versão 3)
            _currentDocName = SolveDuplicateFileNames();
            _currentDocName += _format;
        }
        else
        {
            string temp;
            // MyDebug.Log("C " + temp + "\n  " + _target + temp + _format + "  Bool "+ File.Exists(_target + temp + _format));
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
                    MyDebug.Log("ERRO: Valor invalido em _specialTypeDocName = " + _specialTypeDocName + "Nome default: " + temp);
                    break;
            }

            _currentDocName = temp + _format;
        }

        SetInfoCompl();
        // £ usado para não interferir com a mensagem £

        //prefixoVersao = _versaoActiva == VersaoDisponivel.Versao3 ? "V3" : "V2";

        var info = GetHeader(_sigla + "_" + _versao);

        // File.OpenWrite(_target +  _CurrentDocName);
        _doc = new StreamWriter(_target + _currentDocName, true);
        _doc.WriteLine(info);
        _doc.Close();
        MyDebug.Log("Criar Novo Ficheiro : " + _currentDocName);
        // MyDebug.Log("info : " + info);
#endif
    }

    private string GetHeader(string prefixoVersao)
    {
        return
            "£Registo User Tracker Data;" + prefixoVersao + ";Tese de Mestrado de Francisco Henriques Venda, 73839;Inicio de Registo : " +
            _inicio.ToString("dd/MM/yyyy, HH:mm:ss") + ";Pasta Original : " + _target + ";Nome Ficheiro Original : " +
            _currentDocName + ";Info complementar : " + InfoCompl + ";£";
    }

    private string SolveDuplicateFileNames()
    {
        var temp = _currentDocName;
        var count = 1;
        while (File.Exists(_target + temp + _format))
        {
            temp = string.Format("{0}_{1}", _currentDocName, count++);
            MyDebug.Log(temp);
        }
        return temp;
    }

    private void SetInfoCompl()
    {
        #if UNITY_EDITOR
            if (InfoCompl == "NADA") InfoCompl = "EDITOR";
            else InfoCompl += ", EDITOR";
        #endif
        #if UNITY_ANDROID //&& !UNITY_EDITOR
            if (InfoCompl == "NADA") InfoCompl = "Android (Gear Vr)";
            else InfoCompl += ", Android (Gear Vr)";
        #endif
        #if (UNITY_STANDALONE_WIN) // && !UNITY_EDITOR
            if (InfoCompl == "NADA") InfoCompl = "Windows)";
            else InfoCompl += ", Windows";
        #endif
    }

}


/*
 //string registo1 = "«" + _cont + //  "_" +
        //                  "_" + diff.TotalSeconds +
        //                  "_" + diffIntervalo.TotalSeconds +
        //                  "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") +
        //                  "&" + Vector3ToStringV1(pos) + "!" + QuaternionToString(ori) + "!" +
        //                  "&" + "$" + mensagem;


 
 //public enum VersaoDisponivel
    //{
    //    Versao2,
    //    Versao3,
    //    NaoActivo,
    //}

    //private VersaoDisponivel _versaoActiva;

    //string registo1 = "«" + _cont + //  "_" +
    //                  "_" + diff.TotalSeconds +
    //                  "_" + diffIntervalo.TotalSeconds +
    //                  "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") +
    //                  "&" + Vector3ToString(pos) + "!" + QuaternionToString(ori) + "!" +
    //                  "&" + "$" + mensagemKinect;


    // Debug.Log("C " + temp + "\n  " + _target + temp + _format + "  Bool "+ File.Exists(_target + temp + _format));

 */
