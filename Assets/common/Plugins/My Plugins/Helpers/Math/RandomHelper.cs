/*************************************************************************************************
    Dissertação - Mestrado em Engenharia Informática e de Computadores
    Francisco Henriques Venda, nº 73839
    Modificado
*************************************************************************************************/
using UnityEngine;

public static class RandomHelper
    {
        public static float RandomBinomial(float max)
        {
            return Random.Range(0, max) - Random.Range(0, max);
        }

        public static float RandomBinomial()
        {
            return Random.Range(0, 1.0f) - Random.Range(0, 1.0f);
        }
    }

