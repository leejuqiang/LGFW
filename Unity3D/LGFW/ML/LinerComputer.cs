using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class LinerComputer : NNComputer
    {
        public override double computeMidToOut(double result, int midIndex)
        {
            return result;
        }

        public override void initBp(double delta, bool[] outputMask)
        {
        }
    }
}