using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNFilterComputer
    {

        public virtual double computeMidToOut(double result)
        {
            result = System.Math.Exp(-result);
            return 1 / (1 + result);
            // return result;
        }

        public virtual double bpMidToOut(double delta, int outIndex, CNNBPCache cache)
        {
            return cache.m_output.m_array[outIndex] * (1 - cache.m_output.m_array[outIndex]);
        }
    }
}
