/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class WriteMatrix
{
    private StreamWriter Doc;

    private DateTime _inicio;

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
    private string _fileName;
    private string _target;
    private string _format;

    private readonly string _sigla;
    private readonly string _versao;

    private int _specialTypeDocName;
    private int _numMatrix;

    private static readonly int TamanhoMaximo = 2 * (int)Math.Pow(2, 20); //2 * (2^20)

    //  public bool DirectoryChange;

    private bool _useDefaultFolder;
    private bool _useDefaultDocName;
    private bool _activo;


    public WriteMatrix()
    {
        _useDefaultDocName = true;
        _useDefaultFolder = true;
        _activo = false;

        Directory = System.IO.Directory.GetCurrentDirectory();

        _sigla = "MOT";

        _versao = "V1";
        
        CurrentFolderDestino = _defaultFolderDestino = "Matrix Opti Track";
        _format = ".txt";
        // _defaultDocName = "UTD_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt"; // UserTrakerData
        InfoCompl = "NADA";
        InfoExtra = null;
        _caminhoCompleto = null;
        _fileName = null;
        _specialTypeDocName = 0;
        ///////////////////////////////////////////////////////////////
        _numMatrix  = 0;
    }

    ~WriteMatrix()
    {
        Fim();
    }

    public void Fim()
    {
        ResetMessage();
        EndMessage();
    }
    
    public void SaveMatrix(Matrix4x4 matrix)
    {
        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        //Debug.Log("Pasta : " + _target);
        CheckFileSize();
        if (!_activo) NovoRegisto();
        

        string registo = "«" + Matrix4ToString(matrix); // + tipo + "[" + _localOptitrackManager.GetPositionVector().x + "," + _localOptitrackManager.GetPositionVector().z + "]";
      

        WriteToDoc(registo);
    }

    private static string Matrix4ToString(Matrix4x4 matrix4)
    {

    return    matrix4.m00.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m10.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m20.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m30.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) +
    MyMessaSepa.SepaCol +
              matrix4.m01.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m11.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m21.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m31.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + 
    MyMessaSepa.SepaCol +
              matrix4.m02.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m12.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m22.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m32.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) +
    MyMessaSepa.SepaCol +
              matrix4.m03.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m13.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m23.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
              matrix4.m33.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," })
              ;
    }

    private void NovoRegisto()
    {
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
            _fileName = _sigla + "_" + _versao + "_" + DateTime.Now.ToString("yyyyMMddTHHmmss");

            _currentDocName = _fileName; // + _format; // UserTrakerData (Versão 3)
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

        var info = "£Matrix Opti Track;" + _versao + ";Tese de Mestrado de Francisco Henriques Venda, 73839;Criado : " + _inicio.ToString("dd/MM/yyyy, HH:mm:ss") + ";Pasta Original : " + _target + ";Nome Ficheiro Original : " + _currentDocName + ";Info complementar : " + InfoCompl + ";£";

        WriteToDoc(info);
    }

    private void WriteToDoc(string info)
    {
        // File.OpenWrite(_target +  _CurrentDocName);
        Doc = new StreamWriter(_target + _currentDocName, true);
        Doc.WriteLine(info);
        Doc.Close();
        // Debug.Log("Criar Novo Ficheiro : " + _currentDocName);
        // Debug.Log("info : " + info);
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
    }

    public void SpecialDocName(string newName)
    {
        DocName = newName;
        _useDefaultDocName = false;
        _currentDocName = DocName;
        _activo = false;
    }

    public void SpecialTypeDocName(int t)
    {
        _specialTypeDocName = t;
    }

    public void UseDefaultDocName()
    {
        if (_useDefaultDocName) return;
        _useDefaultDocName = true;
        _activo = false;
    }

    public void UseDefaultFolderName()
    {
        if (_useDefaultFolder) return;
        _useDefaultFolder = true;
        _activo = false;
        CurrentFolderDestino = _defaultFolderDestino;
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

    private void StopRecording()
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        Doc = new StreamWriter(_target + _currentDocName, true);

        InfoExtra += "|Matrix = " + _numMatrix + "|"; 

        if (InfoExtra != null) Doc.WriteLine(InfoExtra);
        Doc.WriteLine(infoFinal);
        Doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
    }

    private void StopRecording(string mensagemFinal)
    {
        if (!_activo) return;
        ResetMessage();
        var infoFinal = "£Mensagem_Final_" + mensagemFinal + "|Matrix = " + _numMatrix + "|" + "£" + DateTime.Now.ToString("yyyyMMddTHHmmssff") + "£Fim£";
        Doc = new StreamWriter(_target + _currentDocName, true);
        Doc.WriteLine(infoFinal);
        Doc.Close();
    }

    private void EndMessage()
    {
        var infoFinal = "£" + DateTime.Now.ToString("yyyyMMddTHHmmss") + "£Fim£\n";
        Doc = new StreamWriter(_target + _currentDocName, true);
        InfoExtra += "|Matrix = " + _numMatrix + "|";
        if (InfoExtra != null) Doc.WriteLine(InfoExtra);
        Doc.WriteLine(infoFinal);
        Doc.Close();
        File.SetAttributes(_target + _currentDocName, FileAttributes.ReadOnly);
    }

    private void ResetMessage()
    {
        _activo = false;
        InfoCompl = "NADA";
        InfoExtra = null;
    }

    public string GetDocActivo()
    {
        if (_activo)
            return _target + _currentDocName;
        return null;
    }

    private static string Vector3ToString(Vector3 vector3)
    {
        return vector3.x.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","}) + MyMessaSepa.SepaVec + vector3.y.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","}) + MyMessaSepa.SepaVec + vector3.z.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","});
    }
}


