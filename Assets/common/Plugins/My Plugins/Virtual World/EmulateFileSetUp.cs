/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Original
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming")]
// ReSharper disable once CheckNamespace
public enum FilesToEmulate
{
   // Teste,
    File01DirectoAltV0_02,
    File02DirectoAltV0_02_01,
    File02DirectoAltV0_02_02,
    File02DirectoAltV0_02_03,

    File02DirectoTotalV0_02,
    File03DirectWipTotalTamGraV2_12,
    File04DirectWipTotalV2_21,
    File05WipTotalTamMedV2_46,
    File06DirectTotalV2_47,
    File07DirectWipTotalTamMedV2_48,
    File08DirectWipTotalTamMedProbV2_76,
    File09DirectWipTotalTamMedProbV2_74,
    File10WipTotalTamPeqPoucoProbV2_19,
    File11WipTotalTamPeqProbV2_69,
    File12WipTotalTamPeqPoucoProbV2_20,
    File13DirectProb2UsersV2_102,
    File14CWIPPostFixedTotalV2_155,
    File15CWIPPostFixedTotalV2_156,
    File16CWIPPostFixedTotalV2_154,

    FileTotalV0_01,

    FileTotalV1_01,
    FileTotalV1_02,
    FileTotalV1_03,
    FileTotalV1_04,

    FileTotalV2_001,
    FileTotalV2_002,
    FileTotalV2_003,
    FileTotalV2_004,
    FileTotalV2_005,
    FileTotalV2_006,
    FileTotalV2_007,
    FileTotalV2_008,
    FileTotalV2_009,

    FileTotalV2_010,
    FileTotalV2_011,
    FileTotalV2_012,
    FileTotalV2_013,
    FileTotalV2_014,
    FileTotalV2_015,
    FileTotalV2_016,
    FileTotalV2_017,
    FileTotalV2_018,
    FileTotalV2_019,

    FileTotalV2_020,
    FileTotalV2_021,
    FileTotalV2_022,
    FileTotalV2_023,
    FileTotalV2_024,
    FileTotalV2_025,
    FileTotalV2_026,
    FileTotalV2_027,
    FileTotalV2_028,
    FileTotalV2_029,

    FileTotalV2_030,
    FileTotalV2_031,
    FileTotalV2_032,
    FileTotalV2_033,
    FileTotalV2_034,
    FileTotalV2_035,
    FileTotalV2_036,
    FileTotalV2_037,
    FileTotalV2_038,
    FileTotalV2_039,

    FileTotalV2_040,
    FileTotalV2_041,
    FileTotalV2_042,
    FileTotalV2_043,
    FileTotalV2_044,
    FileTotalV2_045,
    FileTotalV2_046,
    FileTotalV2_047,
    FileTotalV2_048,
    FileTotalV2_049,

    FileTotalV2_050,
    FileTotalV2_051,
    FileTotalV2_052,
    FileTotalV2_053,
    FileTotalV2_054,
    FileTotalV2_055,
    FileTotalV2_056,
    FileTotalV2_057,
    FileTotalV2_058,
    FileTotalV2_059,

    FileTotalV2_060,
    FileTotalV2_061,
    FileTotalV2_062,
    FileTotalV2_063,
    FileTotalV2_064,
    FileTotalV2_065,
    FileTotalV2_066,
    FileTotalV2_067,
    FileTotalV2_068,
    FileTotalV2_069,

    FileTotalV2_070,
    FileTotalV2_071,
    FileTotalV2_072,
    FileTotalV2_073,
    FileTotalV2_074,
    FileTotalV2_075,
    FileTotalV2_076,
    FileTotalV2_077,
    FileTotalV2_078,
    FileTotalV2_079,

    FileTotalV2_080,
    FileTotalV2_081,
    FileTotalV2_082,
    FileTotalV2_083,
    FileTotalV2_084,
    FileTotalV2_085,
    FileTotalV2_086,
    FileTotalV2_087,
    FileTotalV2_088,
    FileTotalV2_089,

    FileTotalV2_090,
    FileTotalV2_091,
    FileTotalV2_092,
    FileTotalV2_093,
    FileTotalV2_094,
    FileTotalV2_095,
    FileTotalV2_096,
    FileTotalV2_097,
    FileTotalV2_098,
    FileTotalV2_099,

    FileTotalV2_100,
    FileTotalV2_101,
    FileTotalV2_102,
    FileTotalV2_103,
    FileTotalV2_104,
    FileTotalV2_105,
    FileTotalV2_106,
    FileTotalV2_107,
    FileTotalV2_108,
    FileTotalV2_109,

    FileTotalV2_110,
    FileTotalV2_111,
    FileTotalV2_112,
    FileTotalV2_113,
    FileTotalV2_114,
    FileTotalV2_115,
    FileTotalV2_116,
    FileTotalV2_117,
    FileTotalV2_118,
    FileTotalV2_119,

    FileTotalV2_120,
    FileTotalV2_121,
    FileTotalV2_122,
    FileTotalV2_123,
    FileTotalV2_124,
    FileTotalV2_125,
    FileTotalV2_126,
    FileTotalV2_127,
    FileTotalV2_128,
    FileTotalV2_129,

    FileTotalV2_130,
    FileTotalV2_131,
    FileTotalV2_132,
    FileTotalV2_133,
    FileTotalV2_134,
    FileTotalV2_135,
    FileTotalV2_136,
    FileTotalV2_137,
    FileTotalV2_138,
    FileTotalV2_139,

    FileTotalV2_140,
    FileTotalV2_141,
    FileTotalV2_142,
    FileTotalV2_143,
    FileTotalV2_144,
    FileTotalV2_145,
    FileTotalV2_146,
    FileTotalV2_147,
    FileTotalV2_148,
    FileTotalV2_149,

    FileTotalV2_150,
    FileTotalV2_151,
    FileTotalV2_152,
    FileTotalV2_153,
    FileTotalV2_154,
    FileTotalV2_155,
    FileTotalV2_156,
    FileTotalV2_157,
    FileTotalV2_158,
    FileTotalV2_159,

    FileTotalV2_160,
    FileTotalV2_161,
    FileTotalV2_162,
    FileTotalV2_163,
    FileTotalV2_164,
    FileTotalV2_165,
    FileTotalV2_166,
    FileTotalV2_167_Teste,
    FileTotalV2_168_Teste,
    FileTotalV2_169_Teste,

    FileTotalV2_170_Teste_01,
    FileTotalV2_171_Teste_01,
  

}

// ReSharper disable once CheckNamespace
public class EmulateFileSetUp
{
    public EmulateFileSetUp() {}

    public static string GetFileName(FilesToEmulate whichFileToEmulate, string fileToEmulate, bool isEmulatorActivo)
    {
        switch (whichFileToEmulate)
        {
            case FilesToEmulate.File01DirectoAltV0_02:
                return "UTD_20160324T143821-Alt";
            case FilesToEmulate.File02DirectoAltV0_02_01:
                return "UTD_20160324T143821-Alt-1";
            case FilesToEmulate.File02DirectoAltV0_02_02:
                return "UTD_20160324T143821-Alt-2";
            case FilesToEmulate.File02DirectoAltV0_02_03:
                return "UTD_20160324T143821-Alt-3";
            //case FilesToEmulate.Null:
            //    return null;
            default:
                return isEmulatorActivo ? fileToEmulate : null; //_wsf.GetCurrentFileName();
        }
    }

    public static string FileSelector(FilesToEmulate whichFileToEmulate)
    {
        var fileToEmulate = "";
        switch (whichFileToEmulate)
        {
            case FilesToEmulate.File01DirectoAltV0_02:
                fileToEmulate = "Alt\\UTD_20160324T143821-Alt"; // Visto +-
            break;
            case FilesToEmulate.File02DirectoTotalV0_02:
                fileToEmulate = "UTD_20160324T143821"; // Visto +-
            break;
            case FilesToEmulate.File02DirectoAltV0_02_01:
                fileToEmulate = "Alt\\UTD_20160324T143821-Alt-1";
            break;
            case FilesToEmulate.File02DirectoAltV0_02_02:
                fileToEmulate = "Alt\\UTD_20160324T143821-Alt-2";
            break;
            case FilesToEmulate.File02DirectoAltV0_02_03:
                fileToEmulate = "Alt\\UTD_20160324T143821-Alt-3";
            break;
            case FilesToEmulate.File03DirectWipTotalTamGraV2_12:
                fileToEmulate = "UTD_V2_20160414T173608";
            break;
            case FilesToEmulate.File04DirectWipTotalV2_21:
                fileToEmulate = "UTD_V2_20160415T145104";
           break;
            case FilesToEmulate.File05WipTotalTamMedV2_46:
                fileToEmulate = "UTD_V2_20160422T163406";
            break;
            case FilesToEmulate.File06DirectTotalV2_47:
                fileToEmulate = "UTD_V2_20160422T163426";
            break;
            case FilesToEmulate.File07DirectWipTotalTamMedV2_48:
                fileToEmulate = "UTD_V2_20160422T163451";
            break;
            case FilesToEmulate.File08DirectWipTotalTamMedProbV2_76:
                fileToEmulate = "UTD_V2_20160428T134703";
            break;
            case FilesToEmulate.File09DirectWipTotalTamMedProbV2_74:
                fileToEmulate = "UTD_V2_20160428T134349";
            break;
            case FilesToEmulate.File10WipTotalTamPeqPoucoProbV2_19:
                fileToEmulate = "UTD_V2_20160415T144856";
            break;
            case FilesToEmulate.File11WipTotalTamPeqProbV2_69:
                fileToEmulate = "UTD_V2_20160426T161412";
            break;
            case FilesToEmulate.File12WipTotalTamPeqPoucoProbV2_20:
                fileToEmulate = "UTD_V2_20160415T145010";
            break;
            case FilesToEmulate.File13DirectProb2UsersV2_102:
                fileToEmulate = "UTD_V2_20160602T161125";
            break;
            case FilesToEmulate.File14CWIPPostFixedTotalV2_155:
                fileToEmulate = "UTD_V2_20160912T114609";
            break;
            case FilesToEmulate.File15CWIPPostFixedTotalV2_156:
                fileToEmulate = "UTD_V2_20160912T114648";
            break;
            case FilesToEmulate.File16CWIPPostFixedTotalV2_154:
                fileToEmulate = "UTD_V2_20160912T114529";
            break;
            case FilesToEmulate.FileTotalV0_01:
                fileToEmulate = "UTD_20160324T141257";
            break;
            case FilesToEmulate.FileTotalV1_01:
                fileToEmulate = "UTD_V1_20160412T155932";
            break;
            case FilesToEmulate.FileTotalV1_02:
                fileToEmulate = "UTD_V1_20160412T162428";
                break;
            case FilesToEmulate.FileTotalV1_03:
                fileToEmulate = "UTD_V1_20160412T162804";
            break;
            case FilesToEmulate.FileTotalV1_04:
                fileToEmulate = "UTD_V1_20160412T162837";
            break;

            case FilesToEmulate.FileTotalV2_001:
                fileToEmulate = "UTD_V2_20160413T161131"; // Visto +-
            break;
            case FilesToEmulate.FileTotalV2_002:
                fileToEmulate = "UTD_V2_20160413T165216";
            break;
            case FilesToEmulate.FileTotalV2_003:
                fileToEmulate = "UTD_V2_20160413T165621";
            break;
            case FilesToEmulate.FileTotalV2_004:
                fileToEmulate = "UTD_V2_20160413T165643";
            break;
            case FilesToEmulate.FileTotalV2_005:
                fileToEmulate = "UTD_V2_20160413T165656";
            break;
            case FilesToEmulate.FileTotalV2_006:
                fileToEmulate = "UTD_V2_20160414T173427";
            break;
            case FilesToEmulate.FileTotalV2_007:
                fileToEmulate = "UTD_V2_20160414T173544";
            break;
            case FilesToEmulate.FileTotalV2_008:
                fileToEmulate = "UTD_V2_20160414T173643";
            break;
            case FilesToEmulate.FileTotalV2_009:
                fileToEmulate = "UTD_V2_20160414T173719";
            break;

            case FilesToEmulate.FileTotalV2_010:
                fileToEmulate = "UTD_V2_20160414T173746";
            break;
            case FilesToEmulate.FileTotalV2_011:
                fileToEmulate = "UTD_V2_20160414T173812";
            break;
            case FilesToEmulate.FileTotalV2_012:
                fileToEmulate = "UTD_V2_20160415T145157"; // (Problemas)
            break;
            case FilesToEmulate.FileTotalV2_013:
                fileToEmulate = "UTD_V2_20160415T145236";
            break;
            case FilesToEmulate.FileTotalV2_014:
                fileToEmulate = "UTD_V2_20160415T145321";
            break;
            case FilesToEmulate.FileTotalV2_015:
                fileToEmulate = "UTD_V2_20160415T145421";
            break;
            case FilesToEmulate.FileTotalV2_016:
                fileToEmulate = "UTD_V2_20160415T150023";
            break;
            case FilesToEmulate.FileTotalV2_017:
                fileToEmulate = "UTD_V2_20160415T150204";
            break;
            case FilesToEmulate.FileTotalV2_018:
                fileToEmulate = "UTD_V2_20160418T162341";
            break;
            case FilesToEmulate.FileTotalV2_019:
                fileToEmulate = "UTD_V2_20160418T163154";
            break;

            case FilesToEmulate.FileTotalV2_020:
                fileToEmulate = "UTD_V2_20160418T170124";
            break;
            case FilesToEmulate.FileTotalV2_021:
                fileToEmulate = "UTD_V2_20160421T122841";
            break;
            case FilesToEmulate.FileTotalV2_022:
                fileToEmulate = "UTD_V2_20160421T130029";
            break;
            case FilesToEmulate.FileTotalV2_023:
                fileToEmulate = "UTD_V2_20160421T155613";
            break;
            case FilesToEmulate.FileTotalV2_024:
                fileToEmulate = "UTD_V2_20160422T155159";
            break;
            case FilesToEmulate.FileTotalV2_025:
                fileToEmulate = "UTD_V2_20160422T155555";
            break;
            case FilesToEmulate.FileTotalV2_026:
                fileToEmulate = "UTD_V2_20160422T155724";
            break;
            case FilesToEmulate.FileTotalV2_027:
                fileToEmulate = "UTD_V2_20160422T155904";
            break;
            case FilesToEmulate.FileTotalV2_028:
                fileToEmulate = "UTD_V2_20160422T160230";
            break;
            case FilesToEmulate.FileTotalV2_029:
                fileToEmulate = "UTD_V2_20160422T160323";
            break;

            case FilesToEmulate.FileTotalV2_030:
                fileToEmulate = "UTD_V2_20160422T160546";
            break;
            case FilesToEmulate.FileTotalV2_031:
                fileToEmulate = "UTD_V2_20160422T161541";
            break;
            case FilesToEmulate.FileTotalV2_032:
                fileToEmulate = "UTD_V2_20160422T161636";
            break;
            case FilesToEmulate.FileTotalV2_033:
                fileToEmulate = "UTD_V2_20160422T161803";
            break;
            case FilesToEmulate.FileTotalV2_034:
                fileToEmulate = "UTD_V2_20160422T161838";
            break;
            case FilesToEmulate.FileTotalV2_035:
                 fileToEmulate = "UTD_V2_20160422T162148";
            break;
            case FilesToEmulate.FileTotalV2_036:
                fileToEmulate = "UTD_V2_20160426T121847";
            break;
            case FilesToEmulate.FileTotalV2_037:
                fileToEmulate = "UTD_V2_20160426T131411";
            break;
            case FilesToEmulate.FileTotalV2_038:
                fileToEmulate = "UTD_V2_20160426T131436";
            break;
            case FilesToEmulate.FileTotalV2_039:
                fileToEmulate = "UTD_V2_20160426T133633";
            break;

            case FilesToEmulate.FileTotalV2_040:
                fileToEmulate = "UTD_V2_20160426T134040";
            break;
            case FilesToEmulate.FileTotalV2_041:
                fileToEmulate = "UTD_V2_20160426T135445";
            break;
            case FilesToEmulate.FileTotalV2_042:
                fileToEmulate = "UTD_V2_20160426T135916";
            break;
            case FilesToEmulate.FileTotalV2_043:
                fileToEmulate = "UTD_V2_20160426T141454";
            break;
            case FilesToEmulate.FileTotalV2_044:
                fileToEmulate = "UTD_V2_20160426T142331";
            break;
            case FilesToEmulate.FileTotalV2_045:
                fileToEmulate = "UTD_V2_20160426T142507";
            break;
            case FilesToEmulate.FileTotalV2_046:
                fileToEmulate = "UTD_V2_20160426T150915";
            break;
            case FilesToEmulate.FileTotalV2_047:
                fileToEmulate = "UTD_V2_20160426T151406";
            break;
            case FilesToEmulate.FileTotalV2_048:
                fileToEmulate = "UTD_V2_20160426T154157";
            break;
            case FilesToEmulate.FileTotalV2_049:
                fileToEmulate = "UTD_V2_20160426T154202";
            break;

            case FilesToEmulate.FileTotalV2_050:
                fileToEmulate = "UTD_V2_20160426T154237";
            break;
            case FilesToEmulate.FileTotalV2_051:
                fileToEmulate = "UTD_V2_20160426T154352";
            break;
            case FilesToEmulate.FileTotalV2_052:
                fileToEmulate = "UTD_V2_20160426T154829";
            break;
            case FilesToEmulate.FileTotalV2_053:
                fileToEmulate = "UTD_V2_20160426T154933";
            break;
            case FilesToEmulate.FileTotalV2_054:
                fileToEmulate = "UTD_V2_20160426T160937";
            break;
            case FilesToEmulate.FileTotalV2_055:
                fileToEmulate = "UTD_V2_20160426T161009";
            break;
            case FilesToEmulate.FileTotalV2_056:
                fileToEmulate = "UTD_V2_20160426T161546";
            break;
            case FilesToEmulate.FileTotalV2_057:
                fileToEmulate = "UTD_V2_20160428T134217";
            break;
            case FilesToEmulate.FileTotalV2_058:
                fileToEmulate = "UTD_V2_20160428T134241";
            break;
            case FilesToEmulate.FileTotalV2_059:
                fileToEmulate = "UTD_V2_20160520T172117";
            break;

            case FilesToEmulate.FileTotalV2_060:
                fileToEmulate = "UTD_V2_20160520T172225";
            break;
            case FilesToEmulate.FileTotalV2_061:
                fileToEmulate = "UTD_V2_20160520T174603";
            break;
            case FilesToEmulate.FileTotalV2_062:
                fileToEmulate = "UTD_V2_20160523T132348";
            break;
            case FilesToEmulate.FileTotalV2_063:
                fileToEmulate = "UTD_V2_20160530T163031";
            break;
            case FilesToEmulate.FileTotalV2_064:
                fileToEmulate = "UTD_V2_20160530T164948";
            break;
            case FilesToEmulate.FileTotalV2_065:
                fileToEmulate = "UTD_V2_20160530T171315";
            break;
            case FilesToEmulate.FileTotalV2_066:
                fileToEmulate = "UTD_V2_20160530T171319";
            break;
            case FilesToEmulate.FileTotalV2_067:
                fileToEmulate = "UTD_V2_20160530T171322";
            break;
            case FilesToEmulate.FileTotalV2_068:
                fileToEmulate = "UTD_V2_20160530T171326";
            break;
            case FilesToEmulate.FileTotalV2_069:
                fileToEmulate = "UTD_V2_20160530T171330";
            break;

            case FilesToEmulate.FileTotalV2_070:
                fileToEmulate = "UTD_V2_20160530T171333";
            break;
           case FilesToEmulate.FileTotalV2_071:
                fileToEmulate = "UTD_V2_20160530T171337";
            break;
            case FilesToEmulate.FileTotalV2_072:
                fileToEmulate = "UTD_V2_20160530T180619";
            break;
            case FilesToEmulate.FileTotalV2_073:
                fileToEmulate = "UTD_V2_20160530T180623";
            break;
            case FilesToEmulate.FileTotalV2_074:
                fileToEmulate = "UTD_V2_20160530T180628";
            break;
            case FilesToEmulate.FileTotalV2_075:
                fileToEmulate = "UTD_V2_20160530T180631";
            break;
            case FilesToEmulate.FileTotalV2_076:
                fileToEmulate = "UTD_V2_20160530T180816";
            break;
            case FilesToEmulate.FileTotalV2_077:
                fileToEmulate = "UTD_V2_20160530T180819";
            break;
            case FilesToEmulate.FileTotalV2_078:
                fileToEmulate = "UTD_V2_20160530T180822";
            break;
            case FilesToEmulate.FileTotalV2_079:
                fileToEmulate = "UTD_V2_20160530T180827";
            break;

            case FilesToEmulate.FileTotalV2_080:
                fileToEmulate = "UTD_V2_20160530T180830";
            break;
            case FilesToEmulate.FileTotalV2_081:
                fileToEmulate = "UTD_V2_20160530T180833";
            break;
            case FilesToEmulate.FileTotalV2_082:
                fileToEmulate = "UTD_V2_20160530T180840";
            break;
            case FilesToEmulate.FileTotalV2_083:
                fileToEmulate = "UTD_V2_20160530T180953";
            break;
            case FilesToEmulate.FileTotalV2_084:
                fileToEmulate = "UTD_V2_20160530T180956";
            break;
            case FilesToEmulate.FileTotalV2_085:
                fileToEmulate = "UTD_V2_20160602T141250";
            break;
            case FilesToEmulate.FileTotalV2_086:
                fileToEmulate = "UTD_V2_20160602T141331";
            break;
            case FilesToEmulate.FileTotalV2_087:
                fileToEmulate = "UTD_V2_20160602T152759";
            break;
            case FilesToEmulate.FileTotalV2_088:
                fileToEmulate = "UTD_V2_20160602T153606";
            break;

            case FilesToEmulate.FileTotalV2_089:
                fileToEmulate = "UTD_V2_20160602T153611";
            break;
            case FilesToEmulate.FileTotalV2_090:
                fileToEmulate = "UTD_V2_20160602T161039";
            break;
            case FilesToEmulate.FileTotalV2_091:
                fileToEmulate = "UTD_V2_20160602T161047";
            break;
            case FilesToEmulate.FileTotalV2_092:
                fileToEmulate = "UTD_V2_20160602T161055";
            break;
            case FilesToEmulate.FileTotalV2_093:
                fileToEmulate = "UTD_V2_20160602T161120";
            break;
            case FilesToEmulate.FileTotalV2_094:
                fileToEmulate = "UTD_V2_20160603T134327";
            break;
            case FilesToEmulate.FileTotalV2_095:
                fileToEmulate = "UTD_V2_20160603T134344";
            break;
            case FilesToEmulate.FileTotalV2_096:
                fileToEmulate = "UTD_V2_20160603T134349";
            break;
            case FilesToEmulate.FileTotalV2_097:
                fileToEmulate = "UTD_V2_20160603T134356";
            break;
            case FilesToEmulate.FileTotalV2_098:
                fileToEmulate = "UTD_V2_20160603T140154";
            break;
            case FilesToEmulate.FileTotalV2_099:
                fileToEmulate = "UTD_V2_20160603T140214";
            break;

            case FilesToEmulate.FileTotalV2_100:
                fileToEmulate = "UTD_V2_20160603T140226";
            break;
            case FilesToEmulate.FileTotalV2_101:
                fileToEmulate = "UTD_V2_20160603T141238";
            break;
            case FilesToEmulate.FileTotalV2_102:
                fileToEmulate = "UTD_V2_20160603T141244";
            break;
            case FilesToEmulate.FileTotalV2_103:
                fileToEmulate = "UTD_V2_20160603T141302";
            break;
            case FilesToEmulate.FileTotalV2_104:
                fileToEmulate = "UTD_V2_20160603T141308";
            break;
            case FilesToEmulate.FileTotalV2_105:
                fileToEmulate = "UTD_V2_20160603T141314";
            break;
            case FilesToEmulate.FileTotalV2_106:
                fileToEmulate = "UTD_V2_20160603T141320";
            break;
            case FilesToEmulate.FileTotalV2_107:
                fileToEmulate = "UTD_V2_20160603T141327";
            break;
            case FilesToEmulate.FileTotalV2_108:
                fileToEmulate = "UTD_V2_20160603T141333";
            break;
            case FilesToEmulate.FileTotalV2_109:
                fileToEmulate = "UTD_V2_20160603T141339";
            break;

            case FilesToEmulate.FileTotalV2_110:
                fileToEmulate = "UTD_V2_20160603T141345";
            break;
            case FilesToEmulate.FileTotalV2_111:
                fileToEmulate = "UTD_V2_20160603T141351";
            break;
            case FilesToEmulate.FileTotalV2_112:
                fileToEmulate = "UTD_V2_20160603T141357";
            break;
            case FilesToEmulate.FileTotalV2_113:
                fileToEmulate = "UTD_V2_20160603T141403";
            break;
            case FilesToEmulate.FileTotalV2_114:
                fileToEmulate = "UTD_V2_20160603T141409";
            break;
            case FilesToEmulate.FileTotalV2_115:
                fileToEmulate = "UTD_V2_20160603T141415";
            break;
            case FilesToEmulate.FileTotalV2_116:
                fileToEmulate = "UTD_V2_20160603T141421";
            break;
            case FilesToEmulate.FileTotalV2_117:
                fileToEmulate = "UTD_V2_20160603T141428";
            break;
            case FilesToEmulate.FileTotalV2_118:
                fileToEmulate = "UTD_V2_20160603T141434";
            break;
            case FilesToEmulate.FileTotalV2_119:
                fileToEmulate = "UTD_V2_20160603T141440";
            break;

            case FilesToEmulate.FileTotalV2_120:
                fileToEmulate = "UTD_V2_20160603T141446";
            break;
            case FilesToEmulate.FileTotalV2_121:
                fileToEmulate = "UTD_V2_20160603T141451";
            break;
            case FilesToEmulate.FileTotalV2_122:
                fileToEmulate = "UTD_V2_20160606T144137";
            break;
            case FilesToEmulate.FileTotalV2_123:
                fileToEmulate = "UTD_V2_20160606T145221";
            break;
            case FilesToEmulate.FileTotalV2_124:
                fileToEmulate = "UTD_V2_20160606T152534";
            break;
            case FilesToEmulate.FileTotalV2_125:
                fileToEmulate = "UTD_V2_20160606T153343";
            break;
            case FilesToEmulate.FileTotalV2_126:
                fileToEmulate = "UTD_V2_20160613T134735";
            break;
            case FilesToEmulate.FileTotalV2_127:
                fileToEmulate = "UTD_V2_20160613T134955";
            break;
            case FilesToEmulate.FileTotalV2_128:
                fileToEmulate = "UTD_V2_20160613T135044";
            break;
            case FilesToEmulate.FileTotalV2_129:
                fileToEmulate = "UTD_V2_20160617T132545";
            break;
            
            case FilesToEmulate.FileTotalV2_130:
                fileToEmulate = "UTD_V2_20160617T132750";
            break;
            case FilesToEmulate.FileTotalV2_131:
                fileToEmulate = "UTD_V2_20160617T133414";
            break;
            case FilesToEmulate.FileTotalV2_132:
                fileToEmulate = "UTD_V2_20160617T133733";
            break;
            case FilesToEmulate.FileTotalV2_133:
                fileToEmulate = "UTD_V2_20160617T134211";
            break;
            case FilesToEmulate.FileTotalV2_134:
                fileToEmulate = "UTD_V2_20160617T135816";
            break;
            case FilesToEmulate.FileTotalV2_135:
                fileToEmulate = "UTD_V2_20160617T152733";
            break;
            case FilesToEmulate.FileTotalV2_136:
                fileToEmulate = "UTD_V2_20160617T152814";
            break;
            case FilesToEmulate.FileTotalV2_137:
                fileToEmulate = "UTD_V2_20160620T132203";
            break;
            case FilesToEmulate.FileTotalV2_138:
                fileToEmulate = "UTD_V2_20160620T132857";
            break;
            case FilesToEmulate.FileTotalV2_139:
                fileToEmulate = "UTD_V2_20160620T134614";
            break;

            case FilesToEmulate.FileTotalV2_140:
                fileToEmulate = "UTD_V2_20160624T132114";
            break;
            case FilesToEmulate.FileTotalV2_141:
                fileToEmulate = "UTD_V2_20160624T132150";
            break;
            case FilesToEmulate.FileTotalV2_142:
                fileToEmulate = "UTD_V2_20160707T165953";
            break;
            case FilesToEmulate.FileTotalV2_143:
                fileToEmulate = "UTD_V2_20160707T170418";
            break;
            case FilesToEmulate.FileTotalV2_144:
                fileToEmulate = "UTD_V2_20160707T170650";
            break;
            case FilesToEmulate.FileTotalV2_145:
                fileToEmulate = "UTD_V2_20160707T170916";
            break;
            case FilesToEmulate.FileTotalV2_146:
                fileToEmulate = "UTD_V2_20160715T131008";
            break;
            case FilesToEmulate.FileTotalV2_147:
                fileToEmulate = "UTD_V2_20160907T172304";
            break;
            case FilesToEmulate.FileTotalV2_148:
                fileToEmulate = "UTD_V2_20160907T172325";
            break;
            case FilesToEmulate.FileTotalV2_149:
                fileToEmulate = "UTD_V2_20160907T172344";
            break;

            case FilesToEmulate.FileTotalV2_150:
                fileToEmulate = "UTD_V2_20160907T172402";
            break;
            case FilesToEmulate.FileTotalV2_151:
                fileToEmulate = "UTD_V2_20161031T164934";
            break;
            case FilesToEmulate.FileTotalV2_152:
                fileToEmulate = "UTD_V2_20161031T165014";
            break;
            case FilesToEmulate.FileTotalV2_153:
                fileToEmulate = "UTD_V2_20161031T165053";
            break;
            case FilesToEmulate.FileTotalV2_154:
                fileToEmulate = "UTD_V2_20161031T165133";
            break;
            case FilesToEmulate.FileTotalV2_155:
                fileToEmulate = "UTD_V2_20161031T165214";
            break;
            case FilesToEmulate.FileTotalV2_156:
                fileToEmulate = "UTD_V2_20161031T165254";
            break;
            case FilesToEmulate.FileTotalV2_157:
                fileToEmulate = "UTD_V2_20161031T165333";
            break;
            case FilesToEmulate.FileTotalV2_158:
                fileToEmulate = "UTD_V2_20161031T174258";
            break;
            case FilesToEmulate.FileTotalV2_159:
                fileToEmulate = "UTD_V2_20161031T174338";
            break;

            case FilesToEmulate.FileTotalV2_160:
                fileToEmulate = "UTD_V2_20161031T174418";
            break;
            case FilesToEmulate.FileTotalV2_161:
                fileToEmulate = "UTD_V2_20161031T174459";
            break;
            case FilesToEmulate.FileTotalV2_162:
                fileToEmulate = "UTD_V2_20161031T174539";
            break;
            case FilesToEmulate.FileTotalV2_163:
                fileToEmulate = "UTD_V2_20161031T174618";
            break;
            case FilesToEmulate.FileTotalV2_164:
                fileToEmulate = "UTD_V2_20161031T174657";
            break;
            case FilesToEmulate.FileTotalV2_165:
                fileToEmulate = "UTD_V2_20161031T174737";
            break;
            case FilesToEmulate.FileTotalV2_166:
                fileToEmulate = "UTD_V2_20161031T174817";
            break;
            case FilesToEmulate.FileTotalV2_167_Teste:
                fileToEmulate = "UTD_V2_20161102T120505";
            break;
            case FilesToEmulate.FileTotalV2_168_Teste:
                fileToEmulate = "UTD_V2_20161102T120546";
            break;
            case FilesToEmulate.FileTotalV2_169_Teste:
                fileToEmulate = "UTD_V2_20161102T120626";
            break;

            case FilesToEmulate.FileTotalV2_170_Teste_01:
                fileToEmulate = "UTD_V2_20161102T130144";
            break;
            case FilesToEmulate.FileTotalV2_171_Teste_01:
                fileToEmulate = "UTD_V2_20161102T130223";
            break;
     
            default:
                throw new ArgumentOutOfRangeException();
        }
        ////
        return fileToEmulate;
    }
}
