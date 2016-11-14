/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


// ReSharper disable once CheckNamespace
// ReSharper disable once ArrangeTypeModifiers
class QuaternionToEuler
{
    private const float Tolerance = 0.001f; 

    public static float[] ToEuler(Vector4 q)
    {
        float[] euler = new float[3];

        double heading = Math.Atan2(
                            2 * q.y * q.w - 2 * q.x * q.z,
                            1 - 2 * Math.Sqrt(q.y) - 2 * Math.Sqrt(q.z));
        double attitude = Math.Asin(
                            2 * q.x * q.y + 2 * q.z * q.w);
        double bank = Math.Atan2(
                            2 * q.x * q.w - 2 * q.y * q.x,
                            1 - 2 * Math.Sqrt(q.x) - 2 * Math.Sqrt(q.z));


        if (Math.Abs(q.x * q.y + q.z * q.w - 0.5f) < Tolerance)
        {
            heading = 2 * Math.Atan2(q.x, q.w);
            bank = 0;
        }
        else if (Math.Abs(q.x * q.y + q.z * q.w - (-0.5f)) < Tolerance)
        {
            heading = -2 * Math.Atan2(q.x, q.w);
            bank = 0;
        }

        euler[0] = (float)heading;
        euler[1] = (float)attitude;
        euler[2] = (float)bank;

        return euler;
    }
    /*
    public static float[] MatrixToEuler(Matrix4 matrix)
    {

        float[] euler = new float[3];

        double heading = Math.Atan2(matrix.M31, matrix.M11);
        double attitude = Math.Asin(matrix.M21);
        double bank = Math.Atan2(-matrix.M23, matrix.M22);

        if (matrix.M21 == 1)
        {
            heading = Math.Atan2(matrix.M13, matrix.M33);
            bank = 0;
        }
        else if (matrix.M21 == -1)
        {
            heading = Math.Atan2(matrix.M13, matrix.M33);
            bank = 0;
        }

        euler[0] = (float)heading;
        euler[1] = (float)attitude;
        euler[2] = (float)bank;

        return euler;
    }
    */
}

