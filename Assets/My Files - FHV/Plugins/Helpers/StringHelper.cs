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
using JetBrains.Annotations;
using UnityEngine;

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
    public const string Sepa = ";";
}


// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
public static class StringHelper
{
    public static bool? ConvertStringToBool(string s)
    {
        switch (s)
        {
            case "True":
                return true;
            case "False":
                return false;
            default:
                return null;
        }
    }
    
    // ReSharper disable once UnusedMember.Global
    public static bool? ConvertHandStateToBool(string s)
    {
        switch (s)
        {
            case "Closed":
                return true;
            case "Open":
                return false;
            default:
                return null;
        }
    }
    
    // ReSharper disable once UnusedMember.Global
    public static string RemoverExtraCompleto(string text)
    {
        char[] del = { '£' };
        return text.Split(del)[2];
    }

    // ReSharper disable once UnusedMember.Global
    public static string ObterInfoTempoLinha(string text)
    {
        char[] del1 = { '$' };
        return text.Split(del1)[0];
    }

    // ReSharper disable once UnusedMember.Global
    public static string ObterMensagemLinha(string text)
    {
        char[] del1 = { '$' };
        return text.Split(del1)[1];
    }

    // ReSharper disable once UnusedMember.Global
    public static string ObterOptitrackLinha(string text)
    {
        return text.Split(MyMessaSepa.InicioOptitrack[0])[1];
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 ObterOptiPos(string text)
    {
        return StringToVector3(text.Split(MyMessaSepa.SepaOptitrack[0])[0]);
    }

    // ReSharper disable once UnusedMember.Global
    public static Quaternion ObterOptiRot(string text)
    {
        return StringToQuaternion(text.Split(MyMessaSepa.SepaOptitrack[0])[1]);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static Vector3 StringToVector3(string text)
    {
        return new Vector3
        {
            x = (float) Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[0], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            y = (float) Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            z = (float) Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[2], new NumberFormatInfo() { NumberDecimalSeparator = "," })
        };
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static Quaternion StringToQuaternion(string text)
    {
        return new Quaternion()
        {
            x = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[0], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            y = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            z = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[2], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            w = (float)Convert.ToDouble(text.Split(MyMessaSepa.SepaVec[0])[3], new NumberFormatInfo() { NumberDecimalSeparator = "," })
        };
    }

    // ReSharper disable once UnusedMember.Local
    private static Vector3 StringToVector3([NotNull] string text, char separador)
    {
        if (text == null) throw new ArgumentNullException("text");
        return new Vector3
        {
            x = (float) Convert.ToDouble(text.Split(separador)[0], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            y = (float) Convert.ToDouble(text.Split(separador)[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            z = (float) Convert.ToDouble(text.Split(separador)[2], new NumberFormatInfo() { NumberDecimalSeparator = "," })
        };
    }

    // ReSharper disable once UnusedMember.Local
    private static Vector3 MyStringToVector3([NotNull] string text, char separador, [NotNull] string newNumberDecimalSeparator)
    {
        if (text == null) throw new ArgumentNullException("text");
        if (newNumberDecimalSeparator == null) throw new ArgumentNullException("newNumberDecimalSeparator");
        return new Vector3
        {
            x = (float)Convert.ToDouble(text.Split(separador)[0], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator }),
            y = (float)Convert.ToDouble(text.Split(separador)[1], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator }),
            z = (float)Convert.ToDouble(text.Split(separador)[2], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator })
        };
    }

    // ReSharper disable once UnusedMember.Local
    private static Quaternion MyStringToQuaternion([NotNull] string text, char separador)
    {
        if (text == null) throw new ArgumentNullException("text");
        return new Quaternion()
        {
            x = (float)Convert.ToDouble(text.Split(separador)[0], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            y = (float)Convert.ToDouble(text.Split(separador)[1], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            z = (float)Convert.ToDouble(text.Split(separador)[2], new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            w = (float)Convert.ToDouble(text.Split(separador)[3], new NumberFormatInfo() { NumberDecimalSeparator = "," })
        };
    }

    // ReSharper disable once UnusedMember.Local
    private static Quaternion MyStringToQuaternion([NotNull] string text, char separador,
        [NotNull] string newNumberDecimalSeparator)
    {
        if (text == null) throw new ArgumentNullException("text");
        if (newNumberDecimalSeparator == null) throw new ArgumentNullException("newNumberDecimalSeparator");
        return new Quaternion()
        {
            x = (float)Convert.ToDouble(text.Split(separador)[0], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator }),
            y = (float)Convert.ToDouble(text.Split(separador)[1], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator }),
            z = (float)Convert.ToDouble(text.Split(separador)[2], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator }),
            w = (float)Convert.ToDouble(text.Split(separador)[3], new NumberFormatInfo() { NumberDecimalSeparator = newNumberDecimalSeparator })
        };
    }

    public static string Vector3ToStringV1(Vector3 vector3, string separador = MyMessaSepa.SepaVec, string decimalSeparator = ",")
    {
        return vector3.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = decimalSeparator }) + separador +
               vector3.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = decimalSeparator }) + separador +
               vector3.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = decimalSeparator });
    }
    
    // ReSharper disable once UnusedMember.Global
    public static string Vector3ToStringV2(Vector3 vector3, string separador = MyMessaSepa.SepaVec)
    {
        var stringX = vector3.x < 0
            ? " " + vector3.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + separador + " "
            :       vector3.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + separador + " ";

        var stringY = vector3.y < 0
            ? " " + vector3.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + separador + " "
            :       vector3.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + separador + " ";
        
        var stringZ = vector3.z < 0
            ? " " + vector3.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + separador + " "
            :       vector3.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," });
        
        return stringX + stringY + stringZ;
    }
    public static string QuaternionToString(Quaternion quaternion)
    {
        return quaternion.x.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.y.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.z.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
               quaternion.w.ToString("0.000", new NumberFormatInfo() { NumberDecimalSeparator = "," });
    }

    // ReSharper disable once UnusedMember.Global
    public static Vector3 _convertBodyJointStringToVector3(string strJoint)
    {
        var p = strJoint.Split(MessageSeparators.L3);
        return new Vector3(Single.Parse(p[0].Replace(',', '.')), Single.Parse(p[1].Replace(',', '.')), Single.Parse(p[2].Replace(',', '.')));
    }
    
    // ReSharper disable once UnusedMember.Local
    private static string Matrix4ToString(Matrix4x4 matrix4)
    {

        return matrix4.m00.ToString(new NumberFormatInfo() { NumberDecimalSeparator = "," }) + MyMessaSepa.SepaVec +
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

    // ReSharper disable once UnusedMember.Global
    public static string Vector3ToStringRecord(Vector3 vec)
    {
        return "( " + vec.x.ToString("0.000f") + ", " + vec.y.ToString("0.000f") + ", " + vec.z.ToString("0.000f") + " )";
    }

    // ReSharper disable once UnusedMember.Global
    public static string Vector3ToVector2ToStringRecord(Vector3 vec)
    {
        return "( " + vec.x.ToString("0.000f") + ", " + vec.z.ToString("0.000f") + " )";
    }

    // ReSharper disable once UnusedMember.Global
    public static string QuaternionToStringRecord(Quaternion qua)
    {
        return "( " + qua.x.ToString("0.000f") + ", " + qua.y.ToString("0.000f") + ", " + qua.z.ToString("0.000f") + ", " + qua.w.ToString("0.000f") + " )"; 
    }

    // ReSharper disable once UnusedMember.Global
    public static string GetFrameRateString(float delta)
    {
        var msec = delta * 1000.0f;
        var fps = 1.0f / delta;
        var text = String.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        return text;
    }
}
///////////////////////////////////////////////////////////////////////////////////////////////////////////
    /*
     
     */ 
