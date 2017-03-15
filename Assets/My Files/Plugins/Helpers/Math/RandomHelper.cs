/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using UnityEngine;

// ReSharper disable once CheckNamespace
// ReSharper disable once UnusedMember.Global
public static class RandomHelper
    {
        // ReSharper disable once UnusedMember.Global
        public static float RandomBinomial(float max)
        {
            return Random.Range(0, max) - Random.Range(0, max);
        }

        // ReSharper disable once UnusedMember.Global
        public static float RandomBinomial()
        {
            return Random.Range(0, 1.0f) - Random.Range(0, 1.0f);
        }
    }

