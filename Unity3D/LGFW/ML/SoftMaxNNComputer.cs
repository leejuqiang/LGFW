using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class SoftMaxNNComputer : NNComputer
    {

        private double m_sumZ;
        private double[] m_expOut;
        private double m_sumZ2;

        public override void beforeComputeOutput(bool[] outMask)
        {
            base.beforeComputeOutput(outMask);
            if (m_expOut == null)
            {
                m_expOut = new double[m_layer.m_neuronNum];
            }
            m_sumZ = 0;
            for (int i = 0; i < m_layer.m_midOutput.Length; ++i)
            {
                if (outMask == null || outMask[i])
                {
                    m_expOut[i] = System.Math.Exp(m_layer.m_midOutput[i]);
                    m_sumZ += m_expOut[i];
                }
            }
        }

        public override double computeMidToOut(double result, int midIndex)
        {
            return m_expOut[midIndex] / m_sumZ;
        }

        public override void initBp(double delta, bool[] inputMask)
        {
            m_sumZ2 = m_sumZ * m_sumZ;
        }

        private double midToOut(int midIndex, int outIndex)
        {
            if (midIndex == outIndex)
            {
                double a = m_sumZ - m_expOut[midIndex];
                return a * m_expOut[midIndex] / m_sumZ2;
            }
            return -m_expOut[midIndex] * m_expOut[outIndex] / m_sumZ2;
        }

        public override void computeBpInToE(int inIndex, bool[] outMask)
        {
            int w = inIndex;
            double ds = 0;
            for (int i = 0; i < m_layer.m_neuronNum; ++i, w += m_layer.m_weightsNumber)
            {
                if (outMask == null || outMask[i])
                {
                    ds += m_layer.m_matrix[w] * m_expOut[i];
                }
            }
            double ret = 0;
            w = inIndex;
            for (int i = 0; i < m_layer.m_neuronNum; ++i, w += m_layer.m_weightsNumber)
            {
                if (outMask == null || outMask[i])
                {
                    double d = m_layer.m_matrix[w] * m_expOut[i] * m_sumZ - ds * m_expOut[i];
                    ret += d / m_sumZ2 * m_bpCache.m_derivativeMidToE[i];
                }
            }
            m_bpCache.m_derivativeInToE[inIndex] = ret;
        }

        public override double bpWeightToE(int inIndex, int outIndex)
        {
            double ret = 0;
            for (int i = 0; i < m_layer.m_neuronNum; ++i)
            {
                ret += m_bpCache.m_inputs[inIndex] * midToOut(outIndex, i) * m_bpCache.m_derivativeMidToE[i];
            }
            return ret;
        }

        public override double bpBiasToE(int biasIndex)
        {
            int outIndex = biasIndex - m_layer.m_totalWeightNumber;
            double ret = 0;
            for (int i = 0; i < m_layer.m_neuronNum; ++i)
            {
                ret += midToOut(outIndex, i) * m_bpCache.m_derivativeMidToE[i];
            }
            return ret;
        }
    }
}
