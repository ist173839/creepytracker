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


//namespace Assets.CWIP.Plugins.Helpers
//{
// ReSharper disable once CheckNamespace
public static class DebugHelper
{
    public static string DebugVector(Vector3 vector3)
    {
        return "( x = " + vector3.x + ", y = " + vector3.y + ", z = " + vector3.z +"  )";

    }

    public static string DebugColor(Color color)
    {
        return "( R = " + color.r + ", G = " + color.g + ", B = " + color.b + ", A = " + color.a + "  )";
    }

    public static void DebugFoV(Vector3 reticleRight, Vector3 reticleLeft, Vector3 reticleUp, Vector3 reticle)
    {
        MyDebug.DrawLine(reticle, reticleRight, Color.green);
        MyDebug.DrawLine(reticle, reticleLeft,  Color.green);
        MyDebug.DrawLine(reticle, reticleUp,    Color.green);


        var retRight = MathHelper.DeslocamentoHorizontal(reticleRight);
        var retLeft  = MathHelper.DeslocamentoHorizontal(reticleLeft);
        var retUp    = MathHelper.DeslocamentoHorizontal(reticleUp);
        var ret      = MathHelper.DeslocamentoHorizontal(reticle);


        MyDebug.DrawLine(ret, retRight, Color.blue);
        MyDebug.DrawLine(ret, retLeft,  Color.blue);
        MyDebug.DrawLine(ret, retUp,    Color.blue);

    }


    public static void DebugCameraForward(Vector3 cameraRigPosition, Vector3 cameraRigForward, Vector3 transformPosition, Vector3 transformForward)
    {
        //Vector3 cameraRigPosition = CameraRig.transform.position;
        //Vector3 cameraRigForward = CameraRig.transform.forward;
        //Vector3 transformPosition = transform.position;
        //Vector3 transformForward = transform.forward;

        MyDebug.DrawLine(cameraRigPosition, cameraRigPosition + cameraRigForward, Color.black);
        MyDebug.DrawLine(transformPosition, transformPosition + transformForward, Color.blue);
    }


    public static void DebugIndicatorPlane(Indicator ind, Plane indPlane, float distanciaPreAviso, float distanciaAviso, float distanciaPerigo, float y = 0.0f)
    {
        var position            = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position, y);
        var positionPlusNormal  = MathHelper.DeslocamentoHorizontal(ind.ObjectIndicator.transform.position + indPlane.normal, y);
    
        MyDebug.DrawLine(position, positionPlusNormal, Color.blue);

        //indPlane.normal = ind.ObjectIndicator.transform.forward;

        var objIndName = GameObject.Find(ind.Name);

        var debugPosStart = MathHelper.DeslocamentoHorizontal(objIndName.transform.position - objIndName.transform.forward * 2.5f, y);
        var debugPosEnd   = MathHelper.DeslocamentoHorizontal(objIndName.transform.position + objIndName.transform.forward * 2.5f, y);
         
        //debugPosStart.y = 0.0f;
        //debugPosEnd.y = 0.0f;

        MyDebug.DrawLine(debugPosStart, debugPosEnd, Color.black);
        
        DebugLine(debugPosStart, debugPosEnd, objIndName, distanciaPreAviso, Color.gray,   y);
        DebugLine(debugPosStart, debugPosEnd, objIndName, distanciaAviso,    Color.yellow, y);
        DebugLine(debugPosStart, debugPosEnd, objIndName, distanciaPerigo,   Color.red,    y);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static void DebugLine(Vector3 debugPosStart, Vector3 debugPosEnd, GameObject objIndName, float distancia, Color cor, float y = 0.0f)
    {
        var posPreAvisoStart = debugPosStart;
        var posPreAvisoEnd   = debugPosEnd;

        posPreAvisoStart += MathHelper.DeslocamentoHorizontal(objIndName.transform.up * distancia, y);
        posPreAvisoEnd   += MathHelper.DeslocamentoHorizontal(objIndName.transform.up * distancia, y);

        //posPreAvisoStart.y = 0.0f;
        //posPreAvisoEnd.y   = 0.0f;

        MyDebug.DrawLine(posPreAvisoStart, posPreAvisoEnd, cor);
    }

    public static void DebugGetDiffObstacle(Vector3 player, Indicator ind)
    {
        var playerV3 = new Vector3(player.x, 0.0f, player.z);
        var indV3 = new Vector3(ind.ObjectIndicator.transform.position.x, 0.0f, ind.ObjectIndicator.transform.position.z);
        MyDebug.DrawLine(playerV3, indV3, Color.black);
        MyDebug.DrawLine(indV3, indV3 + Vector3.forward * ind.Raio * 0.5f, Color.cyan);
        MyDebug.DrawLine(ind.ObjectIndicator.transform.position, ind.ObjectIndicator.transform.position + Vector3.forward * ind.Raio * 0.5f, Color.cyan);
    }

    public static void DebugGetDistObstacle(Vector3 player, Indicator ind)
    {
        var playerV3 = new Vector3(player.x, 0.0f, player.z);
        var indV3 = new Vector3(ind.ObjectIndicator.transform.position.x, 0.0f, ind.ObjectIndicator.transform.position.z);
        MyDebug.DrawLine(playerV3, indV3, Color.black);
        // MyDebug.DrawLine(indV3, indV3 + Vector3.forward * ind.Raio * 0.5f, Color.cyan);
        // MyDebug.DrawLine(ind.ObjectIndicator.transform.position, ind.ObjectIndicator.transform.position + Vector3.forward * ind.Raio * 0.5f, Color.cyan);
    }

    public static void DebugCheckAngles(Vector3 right, Vector3 left, Vector3 position)
    {
        var angle1 = Vector3.Angle(right, left);
        var angle2 = Vector3.Angle(right, -left);
        var angle3 = Vector3.Angle(-right, left);
        var angle4 = Vector3.Angle(-right, -left);

        if (angle1 <= angle2 && angle1 <= angle3 && angle1 <= angle4)
        {
            MyDebug.DrawLine(position, position + (right * 100), Color.green);
            MyDebug.DrawLine(position, position + (left * 100), Color.red);
        }
        else if (angle2 <= angle1 && angle2 <= angle3 && angle2 <= angle4)
        {
            MyDebug.DrawLine(position, position + (right * 100), Color.green);
            MyDebug.DrawLine(position, position + (-left * 100), Color.red);
        }
        else if (angle3 <= angle1 && angle3 <= angle2 && angle3 <= angle4)
        {
            MyDebug.DrawLine(position, position + (-right * 100), Color.green);
            MyDebug.DrawLine(position, position + (left * 100), Color.red);
        }
        else if (angle4 <= angle1 && angle4 <= angle3 && angle4 <= angle2)
        {
            MyDebug.DrawLine(position, position + (-right * 100), Color.green);
            MyDebug.DrawLine(position, position + (-left * 100), Color.red);
        }
    }
}
//}
