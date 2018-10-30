using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class NNComputer
    {

        public BPCache m_bpCache;
        public NeuralNetworkLayer m_layer;

        public virtual void onChangeTrainMode()
        {
            //nothing
        }

        public virtual void beforeComputeOutput()
        {
            //nothing
        }

        public virtual double computeMidToOut(double result, int midIndex)
        {
            result = System.Math.Exp(-result);
            return 1 / (1 + result);
        }

        public virtual void initBp(double delta)
        {
            for (int i = 0; i < m_layer.m_neuronNum; ++i)
            {
                m_bpCache.m_derivativeMidToE[i] *= bpMidToOut(delta, i);
            }
        }

        protected virtual double bpMidToOut(double delta, int midIndex)
        {
            return m_bpCache.m_output[midIndex] * (1 - m_bpCache.m_output[midIndex]);
        }

        public virtual double bpWeightToE(int inIndex, int outIndex)
        {
            return m_bpCache.m_inputs[inIndex] * m_bpCache.m_derivativeMidToE[outIndex];
        }

        public virtual double bpBiasToE(int biasIndex)
        {
            int outIndex = biasIndex - m_layer.m_totalWeightNumber;
            return m_bpCache.m_derivativeMidToE[outIndex];
        }

        public virtual void computeBpInToE(int inIndex)
        {
            double d = 0;
            int weightIndex = inIndex;
            for (int i = 0; i < m_layer.m_neuronNum; ++i)
            {
                d += m_layer.m_matrix[weightIndex] * m_bpCache.m_derivativeMidToE[i];
                weightIndex += m_layer.m_weightsNumber;
            }
            m_bpCache.m_derivativeInToE[inIndex] = d;
        }
    }
}
