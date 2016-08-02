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


public class SaveMessage
{
    private StreamWriter _doc;
    private DateTime _inicio;

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

    public int NumColunas   { get; private set; }

    private static readonly int TamanhoMaximo = (int) Math.Pow(2, 30); // (2 ^ 30)

    private int _cont;
    private int _specialTypeDocName;

    //  public bool DirectoryChange;
    private bool _useDefaultDocName;
    private bool _useDefaultFolder;
    private bool _isInitiate;

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
        //#if !UNITY_ANDROID
        if (!_isInitiate) SetUpFileAndDirectory();
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
        //#endif
    }

      /// <summary>
      /// Atenção só funciona para V5
      /// </summary>
       private void SetUpHeader()
    {
       // _positionThreshold,  (_numSteps) 
       var info = "Registo"                            + Separador + "Tempo Absoluto (Segundos)" + Separador + "Directo ou WIP"              + Separador +
                  "Vel. Real (Directa)"                + Separador + "Vel. Virtual (WIP)"        + Separador + "Threshold de Velocidade"     + Separador +
                  "Threshold do Passo (WIP)"           + Separador + "N. Passos Total (WIP)"     + Separador + "N. Passos Direito (WIP)"     + Separador + 
                  "N. Passos Esquerdo (WIP)"           + Separador + "Joelho Direito (y)"        + Separador + "Joelho Esquerdo (y)"         + Separador + 
                  "Desvio Joelho Direito"              + Separador + "Desvio Joelho Esquerdo"      + Separador + "Hip (Vector 2)"              + Separador + 
                  "Head (Vector 3)"                    + Separador + "Direito FootStates (WIP)"  + Separador + "Esquerdo FootStates (WIP)"   + Separador + 
                  "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)";

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
            _currentDocName = "WVD_V5_" + DateTime.Now.ToString("yyyyMMddTHHmmss") ;
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
 * 
 *  private void GetNumCol(string s)
    {
        char[] del = Separador.ToCharArray();
        NumColunas = s.Split(del).Length;
    }
  
 
       private void SetUpFimCiclo()
    {
        if (NumColunas == 0) return;
        _fimCiclo = "";
        for (int i = 0; i < NumColunas - 1; i++)
        {
            _fimCiclo += "0" + Separador;
        }
        _fimCiclo += "0";
    }
     
     */
/*
 * 
 *  private void GetNumCol(string s)
    {
        char[] del = Separador.ToCharArray();
        NumColunas = s.Split(del).Length;
        // Debug.Log("NumColunas : " + NumColunas);
    }
 *     public void FimCiclo()
    {
//#if !UNITY_ANDROID
        if (!_isInitiate) return;
        
        WriteStringInDoc(_fimCiclo, true);
//#endif
    }
    
  
 * 
 * 
 * private void NovoRegisto()
    {
        // _target = _directory + "\\" +_CurrentFolderDestino ;

        SetUpDirectory();
        _activo = true;
        _inicio = DateTime.Now;
        SetFileName();

        // _positionThreshold,  (_numSteps) 

        var info = "Registo"                            + Separador + "Tempo Absoluto (Segundos)" + Separador + "Directo ou WIP"              + Separador +
                   "Vel. Real (Directa)"                + Separador + "Vel. Virtual (WIP)"        + Separador + "Threshold de Velocidade"     + Separador +
                   "Threshold do Passo (WIP)"           + Separador + "N. Passos Total (WIP)"     + Separador + "N. Passos Direito (WIP)"     + Separador + 
                   "N. Passos Esquerdo (WIP)"           + Separador + "Joelho Direito (y)"        + Separador + "Joelho Esquerdo (y)"         + Separador + 
                   "Diff Joelho Direito"                + Separador + "Diff Joelho Esquerdo"      + Separador + "Hip (Vector 2)"              + Separador + 
                   "Head (Vector 3)"                    + Separador + "Direito FootStates (WIP)"  + Separador + "Esquerdo FootStates (WIP)"   + Separador + 
                   "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)";

        GetNumCol(info); 
        SetUpFimCiclo();
        WriteStringInDoc(info, true);
        // if (IsDebugActivo)
       
    }


 *  private void SetUpFimCiclo()
    {
        if (NumColunas == 0) return;
        _fimCiclo = "";
        for (int i = 0; i < NumColunas - 1; i++)
        {
            _fimCiclo += "0" + Separador;
        }
        _fimCiclo += "0";
       }
  public SaveMessage(string recordName)
    {
        _useDefaultFolder = true;
        _useDefaultDocName = true;
        _activo = false;
        Directory = System.IO.Directory.GetCurrentDirectory();
        CurrentFolderDestino = _defaultFolderDestino = "Walking Data";
        _format = ".csv";
        Separador = ";";

        _caminhoCompleto = null;
        _specialTypeDocName = 0;
        _cont = 0;

        _target = Directory + "\\" + CurrentFolderDestino + "\\";
        IsDebugActivo = false;

        _recordingName = recordName;
        NumColunas = 0;
    }
     
     */
// _positionThreshold,  (_numSteps) 
/*  
    var info = "Registo"                            + Separador + "Tempo Absoluto (Segundos)" + Separador + "Directo ou WIP"              + Separador +
               "Vel. Real (Directa)"                + Separador + "Vel. Virtual (WIP)"        + Separador + "Threshold de Velocidade"     + Separador +
               "Threshold do Passo (WIP)"           + Separador + "Número Passos Total (WIP)" + Separador + "Número Passos Direito (WIP)" + Separador + 
               "Número Passos Esquerdo (WIP)"       + Separador + "Joelho Direito (y)"        + Separador + "Joelho Esquerdo (y)"         + Separador + 
               "Diff Joelho Direito"                + Separador + "Diff Joelho Esquerdo"      + Separador + "Hip (Vector 2)"              + Separador + 
               "Head (Vector 3)"                    + Separador + "Direito FootStates (WIP)"  + Separador + "Esquerdo FootStates (WIP)"   + Separador + 
               "Direito FootTransitionEvents (WIP)" + Separador + "Esquerdo FootTransitionEvents (WIP)";
*/
/*
 
     
    public void RecordData(float thresholdVelocidade, float thresholdPosition, float velocidadeReal, float velocidadeWip, float rightDiff, float leftDiff, int numSteps, int rightNumSteps, int leftNumSteps, string estadoActual, string rightFootState, string leftFootState, string rightFootTransitionEvents, string leftFootTransitionEvents, Vector3 rightKnee, Vector3 leftKnee, Vector3 hip, Vector3 head)
    {
//#if !UNITY_ANDROID
        if (!_activo) NovoRegisto();

        DateTime agora = DateTime.Now;
        TimeSpan diff = agora - _inicio;

        Vector2 hipVector2 = new Vector2(hip.x, hip.z);
        
        var thresholdVel = thresholdVelocidade.ToString( new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var thresholdPos = thresholdPosition.ToString(   new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var diffSeconds  = diff.TotalSeconds.ToString(   new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var rightDSring  = rightDiff.ToString(           new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var leftDString  = leftDiff.ToString(            new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var rightKneeY   = rightKnee.y.ToString(         new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var leftKneeY    = leftKnee.y.ToString(          new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var velReal      = velocidadeReal.ToString(      new NumberFormatInfo() { NumberDecimalSeparator = "," });
        var velWip       = velocidadeWip.ToString(       new NumberFormatInfo() { NumberDecimalSeparator = "," });
    
        var registo = _cont++      + Separador + diffSeconds    + Separador + estadoActual  + Separador + velReal                   + Separador + velWip       + Separador +
                      thresholdVel + Separador + thresholdPos   + Separador + numSteps      + Separador + rightNumSteps             + Separador + leftNumSteps + Separador + 
                      rightKneeY   + Separador + leftKneeY      + Separador + rightDSring   + Separador + leftDString               + Separador + hipVector2   + Separador + 
                      head         + Separador + rightFootState + Separador + leftFootState + Separador + rightFootTransitionEvents + Separador + leftFootTransitionEvents
                      ;
        WriteStringInDoc(registo, true);
//#endif
    }
     
     
     */
//"Registo" + "Tempo Absoluto (Segundos)" + "Threshold de Velocidade" + "_positionThreshold (WIP)" + "Vel. Real (Directa)" + "Vel. Virtual (WIP)" 
//+ "Directo ou WIP" + "Número de passos Total (WIP)"+ "Número de passos Direito (WIP)" (_numSteps) + "Número de Passos Esquerdo (WIP)" (_numSteps) 
//"Joelho Direito (y)" + "Joelho Esquerdo (y)" + "Diff Joelho Direito" + "Diff Joelho Esquerdo" + "Hip (Vector 2)" + "Head (Vector 3)";
//+ "Direito FootStates (WIP)" + "Esquerdo FootStates (WIP)" 
//+ "Direito FootTransitionEvents (WIP)" + "Esquerdo FootTransitionEvents (WIP)" 

// (Tempo Intervalo / Time.deltaTime) ; Pos Hip(x,z); Pos Head (x,y,z); rightDiff; leftDiff;
// num ; tempo Total (em segundo) ; Threshold de Velocidade ; Joint ; Velocidade Real (Hip) ;  Velocidade Wip (Virtual) ; EstadoPlayer ; RightKnee (Y) ; LeftKnee (Y)

//float velReal = 
//    velocidadeReal

//var diffSeconds     = diff.TotalSeconds.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","});
//var thresholdString = threshold.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","});
//var velReal         = velocidadeReal.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," });
//var velWip = velocidadeWip.ToString(new NumberFormatInfo() {NumberDecimalSeparator = ","});

// Debug.Log("C " + temp + "\n  " + _target + temp + _format + "  Bool "+ File.Exists(_target + temp + _format));
/*
         var registo = _cont++     + Separador + diff.TotalSeconds + Separador + deltaTime     + Separador + threshold    + Separador + 
                    joint       + Separador + velocidadeReal    + Separador + velocidadeWip + Separador + estadoActual + Separador + 
                    rightKnee.y + Separador + leftKnee.y        + Separador + rightDiff     + Separador + leftDiff     + Separador + 
                    hipVector2  + Separador + head
                    ;
  */
