/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public enum MatrixFiles
{
    File_01,
}




// ReSharper disable once CheckNamespace
public class ReadMatrix
{
    public enum Versao
    {
        Versao1,
       
    }
    
    //private WorldCreate _worldCreate;

    private int _versionFile;

    private string _previousPath;

    private List<Quaternion> _matrixList;

    public ReadMatrix()
     {
        //_worldCreate = new WorldCreate();
        _matrixList = new List<Quaternion>();
        _versionFile = 0;
        _previousPath = null;

     }

    public List<Quaternion> GetWorldTest(MatrixFiles file)
    {
       

        return GetWorldTestList( ProcessAllDoc( MatrixSelector(file)));
    }


    private string MatrixSelector(MatrixFiles file)
    {
        var _folder = "Matrix Opti Track";
        var _format = ".txt";
        var _path = System.IO.Directory.GetCurrentDirectory();
        

        string _file;
        
        switch (file)
        {
            case MatrixFiles.File_01:
                _file =  "";
                break;
            default:
                throw new ArgumentOutOfRangeException("file", file, null);
        }
        _file += _format;

        var fullPath = _path + "\\" + _folder + "\\" + _file;
        Debug.Log(" Full Path = " + fullPath);
        return fullPath;
    }

    private List<Quaternion> GetWorldTestList(string cleanText)
    {
        if (cleanText == null)
            return null;

        var list = new List<Quaternion>();

        for (var i = 1; i < ObterStringValues.ObterNumLinha(cleanText); i++)
        {
            var linha = ObterStringValues.ObterLinha(cleanText, i);
            

            // if (type == null) continue;
           
        }
        return list;
    }


    //public List<WorldIndicator> GetWorldList(string cleanText)
    //{

    //    if (cleanText == null)
    //        return null;

    //    var list = new List<WorldIndicator>();

    //    for (var i = 1; i < ObterStringValues.ObterNumLinha(cleanText); i++)
    //    {
    //        var linha = ObterStringValues.ObterLinha(cleanText, i);
    //        var typeString = ObterStringValues.GetType(linha);
    //        if (typeString == "Centro") continue;
    //        var type = ObterStringValues.ConvertStringToIndicatorType(typeString);

    //        // if (type == null) continue;
    //        switch (type)
    //        {
    //            case IndicatorType.Limit:
    //                {
    //                    var modeString = ObterStringValues.GetMode(linha);
    //                    var mode = ObterStringValues.ConvertStringToLimitMode(modeString);


    //                    if (mode != null)
    //                    {
    //                        list.Add
    //                        (
    //                            new WorldIndicator
    //                            {
    //                                Position = ConvertVector2In3(ObterStringValues.GetWorldVectorPosition(linha)),
    //                                Rotation = GetLimitRotation(mode.Value),
    //                                Scale = new Vector3(0.5f, 1.0f, 0.5f),

    //                                Material = MaterialType.Transparent,
    //                                Type = type.Value,
    //                                Raio = 0.5f,
    //                            }
    //                        );
    //                    }
    //                }
    //                break;
    //            case IndicatorType.Obstacle:
    //                {
    //                    var modeString = ObterStringValues.GetMode(linha);
    //                    var mode = ObterStringValues.ConvertStringToRepresentationMode(modeString);
    //                    if (mode != null)
    //                    {
    //                        list.Add
    //                        (
    //                            new WorldIndicator
    //                            {
    //                                Position = ConvertVector2In3(ObterStringValues.GetWorldVectorPosition(linha), GetObjectY(mode.Value)),
    //                                Rotation = Quaternion.Euler(00.0f, 00.0f, 00.0f),
    //                                Scale = GetObjectScale(mode.Value),

    //                                Material = MaterialType.Transparent,
    //                                Representation = mode.Value,
    //                                Type = type.Value,
    //                                Raio = 0.5f,
    //                            }
    //                        );
    //                    }
    //                }
    //                break;
    //            case null:
    //                continue;
    //            default: throw new ArgumentOutOfRangeException();
    //        }
    //    }
    //    return list;
    //}

    private Vector3 ConvertVector2In3(Vector2 v2)
    {
        return new Vector3(v2.x, 0.0f, v2.y);
    }


    private Vector3 ConvertVector2In3(Vector2 v2, float y)
    {
        return new Vector3(v2.x, y, v2.y);
    }

    //private Vector3 GetObjectScale(RepresentationMode representation)
    //{
    //    Vector3 scale;
    //    switch (representation)
    //    {
    //        case RepresentationMode.Sphere:
    //            scale = new Vector3(0.5f, 1.0f, 0.5f);
    //            break;
    //        case RepresentationMode.Cube:
    //            scale = new Vector3(0.5f, 0.6f, 0.5f);
    //            break;
    //        case RepresentationMode.Object:
    //            scale = new Vector3(0.015f, 0.015f, 0.015f);
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException("representation", representation, null);
    //    }
    //    return scale;
    //}

    //private float GetObjectY(RepresentationMode representation)
    //{
    //    float y;
    //    switch (representation)
    //    {
    //        case RepresentationMode.Sphere:
    //            y = -1.20f;
    //            break;
    //        case RepresentationMode.Cube:
    //            y = -1.20f;
    //            break;
    //        case RepresentationMode.Object:
    //            y = -1.50f;
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException("representation", representation, null);
    //    }
    //    return y;
    //}

    //private Quaternion GetLimitRotation(LimitPositionMode mode)
    //{
    //    Quaternion quaternion;
    //    switch (mode)
    //    {
    //        case LimitPositionMode.Front:
    //            quaternion = Quaternion.Euler(0.0f, 90.0f, -90.0f);
    //            break;
    //        case LimitPositionMode.Right:
    //            quaternion = Quaternion.Euler(0.0f, 0.0f, 90.0f);
    //            break;
    //        case LimitPositionMode.Left:
    //            quaternion = Quaternion.Euler(0.0f, 0.0f, -90.0f);
    //            break;
    //        case LimitPositionMode.Behind:
    //            quaternion = Quaternion.Euler(0.0f, -90.0f, -90.0f);
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException("mode", mode, null);
    //    }

    //    return quaternion;
    //}


    private string ProcessAllDoc(string path) // FileStream
    {
        if (_previousPath == path) return null;


        DateTime inicio = DateTime.Now;

        string allText = "";

        Debug.Log(">> " + path);
        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (BufferedStream bs = new BufferedStream(fs))
        using (StreamReader sr = new StreamReader(bs))
        {
            //var l = 0;
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // Debug.Log("Print : " + l++ + ",  " + line);
                allText += line;
            }
        }

        var cleanFile = ""; // ObterStringValues.RemoverExtraWorld(allText);
        GetVersionFile(allText);

        var diff = DateTime.Now - inicio;
        Debug.Log("<ProcessAllDoc> deltaTime : " + Time.deltaTime + ", diff : " + diff.Seconds);

        _previousPath = path;
        return cleanFile;
    }


    private void GetVersionFile(string text)
    {
        char[] del = {'£'};
        char[] del1 = {';'};
        // Debug.Log("Del : " + text.Split(del)[1]);
        // Debug.Log("Del1 : " + text.Split(del)[1].Split(del1)[1]);

        var stringVersion = text.Split(del)[1].Split(del1)[1];
        if (stringVersion.Equals("V1")) _versionFile = 1;
    }
}

//SafeActions(cleanFile);

//Debug.Log("<ProcessAllDoc> Ficheiro Lido: " + _fileToEmulateName);
//_isSetUp = true;


// if (!IsDebug) return;

//if (_directory != _wsf.Directory || _currentFolderDestino != _wsf.CurrentFolderDestino) ActualizarDirectorio();

//if (_isSetUp) return;
//_registoAccao.Clear();
//_isRelogioActivo = false;
//_nextAction = 0;
//_ciclos = 0;


//list.Add(new VirtualWorldTest
//    {

//    }
//);

//  Debug.Log("ReadDoc: Accao: Num = " + GetNum(tempoLinha) + ", Desvio = " + GetDiff(tempoLinha) + ", Mensagem = " + mensagemLinha);
//_registoAccao.Add(_fileVersion == Versao.Versao3
//    ? new Accao
//    {
//        Num = GetNum(tempoLinha),
//        Diff = GetDiff(tempoLinha),
//        Mensagem = mensagemLinha,
//        OptiPos = ObterOptiPos(optiLinha),
//        OptiRot = ObterOptiRot(optiLinha),
//    }
//    : new Accao { Num = GetNum(tempoLinha), Diff = GetDiff(tempoLinha), Mensagem = mensagemLinha });