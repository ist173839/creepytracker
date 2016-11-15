using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
public struct AccaoData
{

    public int Count;
    public double Tempo;
    public float Altura;

    public Vector3 JointCamera;
    public Vector3 JointVelReal;

    public float KneeRightY;
    public float KneeLeftY;

}

// ReSharper disable once CheckNamespace
public class EmulateSaveData
{
    private List<AccaoData> _listAccoes;

    // ReSharper disable once CollectionNeverQueried.Local
    private List<long> _indexes;

   private SafeCenter _wsf;
    
    private DateTime _relogio;

    public string Sufixo;
   
    private string _currentFolderDestino;
    private string _fileToEmulateName;
    private string _debugFolderPath;
    private string _currentFileName;
    private string _readFolderPath;
    private string _directory;
    private string _cleanFile;

    public bool IsCompleteParcial { get; private set; }
    public bool IsRepetirSignal;
    public bool IsDebug;

    private bool _isRelogioActivo;
    private bool _isIndexFull;
    private bool _repetir;
    private bool _isSetUp;

    private int _versionEmulator;
    private int _versionFile;
    private int _nextAction;
    private int _ciclos;
    private int _index;

    private long _previousFilePosition;

    public EmulateSaveData(string fileToEmulateName, bool isAsync, bool begin)
    {
        StandardSetUp(fileToEmulateName);
        if (isAsync) { }
        if (begin) ProcessAllDoc();
    }

    private void StandardSetUp(string fileToEmulateName)
    {
        _wsf        = new SafeCenter();
        _listAccoes = new List<AccaoData>();

        IsDebug = IsCompleteParcial = _isSetUp = false;
        _repetir = true;
        _currentFileName = null;

        Sufixo = ".csv";
        ActualizarDirectorio();
     
        _ciclos = 0;
        _fileToEmulateName = fileToEmulateName + Sufixo;
        _versionEmulator = 2;
        IsRepetirSignal = false;
        _isIndexFull = false;

        _index = 0;
        _previousFilePosition = 0;
        
        _indexes = new List<long> { _previousFilePosition };

         _nextAction = 0 ;
    }

    private void ActualizarDirectorio()
    {
        _directory            = _wsf.Directory;
        _currentFolderDestino = _wsf.CurrentFolderDestino;
        _readFolderPath       = _directory + "\\" + _currentFolderDestino + "\\";
    }
    
    private void ProcessAllDoc() // FileStream
    {
        var inicio = DateTime.Now;

        if (_directory != _wsf.Directory || _currentFolderDestino != _wsf.CurrentFolderDestino) ActualizarDirectorio();

        if (_isSetUp) return;
        
        _isRelogioActivo = false;

        _nextAction = 0;
        _ciclos     = 0;

        var path = _readFolderPath + _fileToEmulateName;

        MyDebug.Log(">> " + path);
        using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var bs = new BufferedStream(fs))
        using (var sr = new StreamReader(bs))
        {
            
            string line;
            var iLine = 0;
            while ((line = sr.ReadLine()) != null)
            {
                iLine++;
                // Debug.Log("kr = " + line.Split(';')[16] + ", kl = " + line.Split(';')[17]);

                if (!line.Contains("Registo"))
                {
                    char[] del = { ';' };
                    var lineText = line.Split(del);

                    var t   = lineText[1];
                    var a   = lineText[30];
                    var jc  = lineText[14];
                    var jvr = lineText[15];

                    var kr = lineText[16];
                    var kl = lineText[17];
                    
                    var ct = Convert.ToDouble(t.Replace(",", ".")); 
                      
                    var ca  = float.Parse( a.Replace(",", "."));
                    var ckr = float.Parse(kr.Replace(",", "."));
                    var ckl = float.Parse(kl.Replace(",", "."));
                    
                    var tempJc  = RemoveExtra(jc);
                    var tempJvr = RemoveExtra(jvr);

                    var newVec2 = ConvertStringToVector3(tempJvr);
                    var newVec1 = ConvertStringToVector3(tempJc, newVec2.y);

                    // Debug.Log("Print : " + i_line + ", " + t + ", " + a + ", " + jc + ", " + jvr + ", kr = "  + kr + ", kl = " + kl);
                    // Debug.Log("Tempo  = " + ct);
                    // Debug.Log("newVec1  :  X = " + newVec1.x + ", Y = " + newVec1.y + ", Z = " + newVec1.z);
                    // Debug.Log("newVec2  :  X = " + newVec2.x + ", Y = " + newVec2.y + ", Z = " + newVec2.z);

                    _listAccoes.Add(new AccaoData
                    {
                        Count        = iLine,
                        Tempo        = ct,
                        Altura       = ca,
                        JointVelReal = newVec1,
                        JointCamera  = newVec2, 
                        KneeRightY   = ckr,
                        KneeLeftY    = ckl,
                    });
                }
            }
        }
 
        MyDebug.Log("<ProcessAllDoc> Ficheiro Lido: " + _fileToEmulateName);
        _isSetUp = true;


        // if (!IsDebug) return;
        var diff = DateTime.Now - inicio;
        MyDebug.Log("<ProcessAllDoc> deltaTime : " + Time.deltaTime + ", diff : " + diff.Seconds);
    }

    public AccaoData? GetAccao()
    {
        if (StandardMensagemSetUp()) return null;
        var agora = DateTime.Now;
        var diff = agora - _relogio;
        
        if (_nextAction >= _listAccoes.Count) return null;

        // Debug.Log("(diff.TotalSeconds = " + diff.TotalSeconds + ", _listAccoes[_nextAction].Tempo = " + _listAccoes[_nextAction].Tempo);

        if (diff.TotalSeconds >= _listAccoes[_nextAction].Tempo)
        {
            return _listAccoes[_nextAction++];
        }
        
        return null;
    }

    private bool StandardMensagemSetUp()
    {
        if (_nextAction == _listAccoes.Count) // && _registoAccao.Count != 0)
        {
            if (_repetir)
            {
                IsRepetirSignal = true;
                _ciclos++;
                _isRelogioActivo = false;
                //_index = 0;
                _nextAction = 0;
            }
            else
            {
                _nextAction++;
                return true;
            }
        }
        if (_nextAction > _listAccoes.Count && _repetir)
        {
            _isRelogioActivo = false;
            _nextAction = 0;
            //_index = 0;
        }
        if (_isRelogioActivo) return false;
        _isRelogioActivo = true;
        _relogio = DateTime.Now;
        return false;
    }
    
    private static Vector3 ConvertStringToVector3(string text, float h)
    {
        char[] del1 = {','};
        var splitTempJc = text.Split(del1);

        var newX = float.Parse(splitTempJc[0]);
        var newZ = float.Parse(splitTempJc[1]);
        
        return new Vector3(newX, h / 2, newZ);
    }

    private static Vector3 ConvertStringToVector3(string text)
    {
        char[] del1 = {','};
        var splitText = text.Split(del1);

        var newX = float.Parse(splitText[0]);
        var newY = float.Parse(splitText[1]);
        var newZ = float.Parse(splitText[2]);
        
        return new Vector3(newX, newY, newZ);
    }

    private static string RemoveExtra(string text)
    {
        var tempText = text;
        if (tempText == null)
            throw new ArgumentNullException("text");
        tempText = tempText.Replace("(", "");
        tempText = tempText.Replace(")", "");
        tempText = tempText.Replace("f", "");
        return tempText;
    }

    public void ChangeFileToEmulate(string fileToEmulateName)
    {
        if (_fileToEmulateName == fileToEmulateName + Sufixo) return;

        _fileToEmulateName = fileToEmulateName + Sufixo;
        IsCompleteParcial = _isSetUp = false;
        _indexes.Clear();

        _previousFilePosition = 0;
        _versionFile = 0;
        _nextAction  = 0;
        _ciclos      = 0;
        _index       = 0;

        _isRelogioActivo = false;
        _isIndexFull     = false;
    }

}

/*
  //  private EsdVersao _fileVersion;


    // var ct = (float) Convert.ToDouble(t);
    // var ct = (float) decimal.Round(Convert.ToDecimal(t.Replace(".", ",")), 3) ;
      
     
    ////////////////////////////////////////////////////////////////

    public float Tempo;
    public float Altura;
    public Vector3 JointCamera;
    public Vector3 JointVelReal;

           
        //for (int index = 0; index < allText.Length; index++)
        //{
        //    var lineText = allText[index];
        //}


        //var cleanFile = RemoverExtraCompleto(allText);
        //GetVersionFile(allText);
        //SafeActions(cleanFile);


 */

