/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

// ReSharper disable once CheckNamespace
 public static class ObterStringValues
{
     public static string RemoverExtraWorld(string text)
     {
         char[] del = { '£' };
         return text.Split(del)[2];
     }

    public static string ObterLinha(string text, int i)
    {
        char[] del = { '«' };
        return text.Split(del)[i];
    }

    public static string ObterLinha(string text)
    {
        char[] del = { '«' };
        return text.Split(del)[1];
    }


    public static int ObterNumLinha(string text)
    {
        char[] del = { '«' };
        return text.Split(del).Length;
    }
    public static string GetType(string text)
    {
        char[] del = { '[' };
        return text.Split(del)[0];
    }

    
    public static Vector2 GetWorldVectorPosition(string text)
    {
        char[] del  = { '[' };
        char[] del1 = { ']' };
        char[] del2 = { ',' };
        
        var vectorString = text.Split(del)[1].Split(del1)[0];

        var x = (float) Convert.ToDouble(vectorString.Split(del2)[0]);
        var y = (float) Convert.ToDouble(vectorString.Split(del2)[1]);
      

        return new Vector2(x, y);
    }

    public static string GetMode(string text)
    {
        char[] del = { '|' };
        return text.Split(del)[1];
    }

}

//var t1 = text.Split(del)[1];
//  x = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[0], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
//  y = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }),