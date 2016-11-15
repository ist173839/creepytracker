using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

[SuppressMessage("ReSharper", "InconsistentNaming")]
// ReSharper disable once CheckNamespace
public enum FilesToEmulateSaveData
{
    File01V11,
    File02V11,
    File03V11,
}

public class EmulateSaveDataSetUp
{
    public EmulateSaveDataSetUp() {}

    public static string FileSelector(FilesToEmulateSaveData whichFileToEmulate)
    {
        var fileToEmulate = "";
        switch (whichFileToEmulate)
        {
            case FilesToEmulateSaveData.File01V11:
                fileToEmulate = "WVD_V11_20161006T125024";
                break;
            case FilesToEmulateSaveData.File02V11: 
                fileToEmulate = "WVD_V11_20161027T144741";
                break;
            case FilesToEmulateSaveData.File03V11:
                fileToEmulate = "WVD_V11_20161027T145040_UTD_20160324T143821-Alt-3";
                break;
            default:
                throw new ArgumentOutOfRangeException("whichFileToEmulate", whichFileToEmulate, null);
        }
        return fileToEmulate;
    }

}


