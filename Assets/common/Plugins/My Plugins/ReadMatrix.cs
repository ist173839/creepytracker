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
    
    private Vector3 ConvertVector2In3(Vector2 v2)
    {
        return new Vector3(v2.x, 0.0f, v2.y);
    }


    private Vector3 ConvertVector2In3(Vector2 v2, float y)
    {
        return new Vector3(v2.x, y, v2.y);
    }

    


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
