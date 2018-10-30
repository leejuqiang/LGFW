using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class ReluFilterComputer : CNNFilterComputer
    {

        public override double computeMidToOut(double result)
        {
            if (result < 0)
            {
                return 0;
            }
            return result;
        }

        public override double bpMidToOut(double delta, int outIndex, CNNBPCache cache)
        {
            if (cache.m_output.m_array[outIndex] <= 0)
            {
                return 0;
            }
            return 1;
        }
    }
}
