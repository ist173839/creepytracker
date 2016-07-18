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

/*
  "£Registo User Tracker Data;" + prefixoVersao + ";Tese de Mestrado de Francisco Henriques Venda, 73839;Inicio de Registo : " +
            _inicio.ToString("dd/MM/yyyy, HH:mm:ss") + ";Pasta Original : " + _target + ";Nome Ficheiro Original : " +
            _currentDocName + ";Info complementar : " + InfoCompl + ";£";
             
    string registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "&" + pos + "&" +"$" + mensagemKinect;     
     */

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
}
// ReSharper disable once CheckNamespace
public class WriteSafeFile
{
    public enum VersaoDisponivel
    {
        Versao2,
        Versao3,
        NaoActivo,
    }

    private VersaoDisponivel _versaoActiva;
    
    private DateTime _inicio;
    private DateTime _tempoMensagemAnterior;

    private StreamWriter Doc;
    
    public string FolderDestino;
    public string DocName;
    public string InfoCompl;
    public string InfoExtra;
    public string CurrentFolderDestino;
    public string Directory;

    private string _defaultFolderDestino;
    private string _defaultDocName;
    private string _currentDocName;
    private string _fileName;
    private string _target;
    private string _format;
    private string _caminhoCompleto;

    private int _cont;
    private int _specialTypeDocName;

    private static readonly int TamanhoMaximo = 2 * (int) Math.Pow(2,20); //2 * (2^20)

    //  public bool DirectoryChange;

    private bool _useDefaultFolder;
    private bool _useDefaultDocName;
    private bool _activo;
    private bool _first;

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
        
        string infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        Doc = new StreamWriter(_target + _currentDocName, true);
        if (InfoExtra != null) Doc.WriteLine(InfoExtra);
        Doc.WriteLine(infoFinal);
        Doc.Close();
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

    public string GetCurrentFileName()
    {
        return _fileName;
    }

    public void AddExtraInfo(string extra)
    {
        InfoExtra += "£ExtraInfo: " + extra;
    }

    public void SpecialFolderName(string newName)
    {
        CurrentFolderDestino = FolderDestino = newName;
        _useDefaultFolder = false;
        _activo = false;
        _versaoActiva = VersaoDisponivel.NaoActivo;
    }

    public void SpecialDocName(string newName)
    {
        DocName = newName;
        _useDefaultDocName = false;
        _currentDocName = DocName;
        _activo = false;
        _versaoActiva = VersaoDisponivel.NaoActivo;
    }

    public void SpecialTypeDocName(int t)
    {
        _specialTypeDocName = t;      
    }
    
    public void UseDefaultDocName()
    {
        if (!_useDefaultDocName)
        {
            _useDefaultDocName = true;
            _activo = false;
            _versaoActiva = VersaoDisponivel.NaoActivo;
        }
    }

    public void UseDefaultFolderName()
    {
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
#if !UNITY_ANDROID  || UNITY_EDITOR
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //Debug.Log("Pasta : " + _target);
        CheckFileSize();
        if (!_activo || _versaoActiva == VersaoDisponivel.NaoActivo) NovoRegisto(VersaoDisponivel.Versao2);


        //_caminhoCompleto = _target + _CurrentDocName;

        Doc = new StreamWriter(_target + _currentDocName, true);
        DateTime agora = DateTime.Now;
        if (_first)
        {
            _tempoMensagemAnterior = agora;
            _first = false;
        }

        TimeSpan diff = agora - _inicio;// 
        TimeSpan diffIntervalo = agora - _tempoMensagemAnterior;

        string registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$"+ mensagemKinect; 
        Doc.WriteLine(registo);
        Doc.Close(); // Todo Apenas um Doc.Close

        _tempoMensagemAnterior = agora;
#endif
    }
    
    public void Recording(string mensagemKinect, Vector3 pos, Quaternion ori)
    {
#if !UNITY_ANDROID  || UNITY_EDITOR
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //Debug.Log("Pasta : " + _target);
        CheckFileSize();
        if (!_activo || _versaoActiva != VersaoDisponivel.Versao3) NovoRegisto(VersaoDisponivel.Versao3);


        //_caminhoCompleto = _target + _CurrentDocName;

        Doc = new StreamWriter(_target + _currentDocName, true);
        DateTime agora = DateTime.Now;
        if (_first)
        {
            _tempoMensagemAnterior = agora;
            _first = false;
        }

        TimeSpan diff = agora - _inicio;// 
        TimeSpan diffIntervalo = agora - _tempoMensagemAnterior;

        //string registo = "«" + _cont++ + "_" + diff.TotalSeconds + "_" + diffIntervalo.TotalSeconds + "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + "$" + mensagemKinect + "&" + pos;
        string registo = MyMessaSepa.InicioRegisto    + _cont++ + 
                         MyMessaSepa.SepaRegisto      + diff.TotalSeconds +
                         MyMessaSepa.SepaRegisto      + diffIntervalo.TotalSeconds +
                         MyMessaSepa.SepaRegisto      + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") + 
                         MyMessaSepa.InicioOptitrack  + 
                                                      Vector3ToString(pos)    + MyMessaSepa.SepaOptitrack + 
                                                      QuaternionToString(ori) + MyMessaSepa.SepaOptitrack + 
                         MyMessaSepa.InicioOptitrack  +
                         MyMessaSepa.InicioMensagem   + mensagemKinect;


        string registo1 = "«" + _cont++ + //  "_" +
                          "_" + diff.TotalSeconds +
                          "_" + diffIntervalo.TotalSeconds +
                          "_" + agora.ToString("yyyy-MM-dd-HH-mm-ss-fff") +
                          "&" + Vector3ToString(pos) + "!" + QuaternionToString(ori) + "!" +
                          "&" + "$" + mensagemKinect;



        Doc.WriteLine(registo);
        Doc.Close(); 

        _tempoMensagemAnterior = agora;
#endif
    }

    private static string Vector3ToString(Vector3 vector3)
    {
        return vector3.x.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec + 
               vector3.y.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec + 
               vector3.z.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," });
    }

    private static string QuaternionToString(Quaternion quaternion)
    {
        return quaternion.x.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.y.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.z.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.w.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," });
    }

    private void StopRecording()
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        Doc = new StreamWriter(_target + _currentDocName, true);
        if (InfoExtra != null) Doc.WriteLine(InfoExtra);
        Doc.WriteLine(infoFinal);
        Doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
        
    }

    public void StopRecording(string mensagemFinal)
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£Mensagem_Final_" + mensagemFinal + "£"+ DateTime.Now.ToString("yyyyMMddTHHmmssff") + "£Fim£";
        Doc = new StreamWriter(_target + _currentDocName, true);
        Doc.WriteLine(infoFinal);
        Doc.Close();
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
        Doc = new StreamWriter(_target + _currentDocName, true);
        Doc.WriteLine(info);
        Doc.Close();
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
 
      private void NovoRegisto()
    {
#if !UNITY_ANDROID || UNITY_EDITOR
        // _target = _directory + "\\" +_CurrentFolderDestino ;

        if (!System.IO.Directory.Exists(_target))
        {
            System.IO.Directory.CreateDirectory(_target);
            Debug.Log("Criar Nova Pasta : " + _target);
        }

        _activo = true;
        _inicio = DateTime.Now;
        if (_useDefaultDocName)
        {


            _fileName = "UTD_V2_" + DateTime.Now.ToString("yyyyMMddTHHmmss");
            _currentDocName = _fileName + _format; // UserTrakerData (Versão 2)
        }
        else
        {
            string temp = _currentDocName;
            // Debug.Log("C " + temp + "\n  " + _target + temp + _format + "  Bool "+ File.Exists(_target + temp + _format));
            switch (_specialTypeDocName)
            {
                case 0:
                    var count = 1;
                    while (File.Exists(_target + temp + _format))
                    {
                        temp = string.Format("{0}_{1}", _currentDocName, count++);
                        Debug.Log(temp);
                    }
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
        var info =
            "£Registo User Tracker Data;V2;Tese de Mestrado de Francisco Henriques Venda, 73839;Inicio de Registo : " +
            _inicio.ToString("dd/MM/yyyy, HH:mm:ss") + ";Pasta Original : " + _target + ";Nome Ficheiro Original : " +
            _currentDocName + ";Info complementar : " + InfoCompl + ";£";
        
        // File.OpenWrite(_target +  _CurrentDocName);
        Doc = new StreamWriter(_target + _currentDocName, true);
        Doc.WriteLine(info);
        Doc.Close();
        Debug.Log("Criar Novo Ficheiro : " + _currentDocName);
        // Debug.Log("info : " + info);
#endif
    }
///////////////////////////////////////////////////////////////////////////////////////////////
    if (InfoCompl == "NADA") InfoCompl = "Server";
        else InfoCompl += ", Server"; 
     
*/
