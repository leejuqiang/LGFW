using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class BPCache
    {

        public double[] m_inputs;
        public double[] m_midOutput;
        public double[] m_output;

        public double[] m_derivativeMidToE;
        public double[] m_derivativeInToE;

        public void initDerivativeArray(int outputCount, int inputCount)
        {
            m_derivativeInToE = new double[inputCount];
        }
    }
}
