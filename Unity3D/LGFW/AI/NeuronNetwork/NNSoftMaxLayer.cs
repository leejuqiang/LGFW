using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
#if NN_USE_FLOAT
    using number = System.Float; 
#else
    using number = System.Double;
#endif

    /// <summary>
    /// A soft max activation function layer
    /// </summary>
    public class NNSoftMaxLayer : NNLayerBase
    {
        private number[] m_expOutput;
        private number m_sum;

        public NNSoftMaxLayer()
        {
        }

        public override void setInputNumber(int number)
        {
            m_output = new number[number];
            m_expOutput = new number[number];
        }

        public override number[] output(number[] input)
        {
            m_input = input;
            m_sum = 0;
            for (int i = 0; i < m_output.Length; ++i)
            {
                m_expOutput[i] = System.Math.Exp(m_input[i]);
                m_sum += m_expOutput[i];
            }
            m_sum = 1 / m_sum;
            for (int i = 0; i < input.Length; ++i)
            {
                m_output[i] = m_expOutput[i] * m_sum;
            }
            return m_output;
        }

        public override void enableTraining(int layerIndex)
        {
            if (layerIndex > 0)
            {
                m_bpInToE = new number[m_output.Length];
            }
        }

        public override void backPropagate(number[] outToE, bool isFirst)
        {
            if (!isFirst)
            {
                number sum2 = m_sum * m_sum;
                for (int i = 0; i < m_bpInToE.Length; ++i)
                {
                    m_bpInToE[i] = 0;
                    for (int j = 0; j < m_bpInToE.Length; ++j)
                    {
                        if (i == j)
                        {
                            m_bpInToE[i] += (m_sum - m_expOutput[i] * sum2) * m_expOutput[i] * outToE[j];
                        }
                        else
                        {
                            m_bpInToE[i] -= m_expOutput[j] * sum2 * m_expOutput[i] * outToE[j];
                        }
                    }
                }
            }
        }

        public override void clearForTraining()
        {
        }

        public override void updateParameters(number rate)
        {

        }
    }
}
