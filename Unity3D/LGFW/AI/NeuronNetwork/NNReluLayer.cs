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
    /// A ReLU activation function layer
    /// </summary>
    public class NNReluLayer : NNLayerBase
    {
        public NNReluLayer()
        {
        }

        public override void setInputNumber(int number)
        {
            m_output = new number[number];
        }

        public override number[] output(number[] input)
        {
            m_input = input;
            for (int i = 0; i < input.Length; ++i)
            {
                if (input[i] > 0)
                {
                    m_output[i] = m_input[i];
                }
                else
                {
                    m_output[i] = 0;
                }
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
                for (int i = 0; i < m_bpInToE.Length; ++i)
                {
                    if (m_output[i] > 0)
                    {
                        m_bpInToE[i] = outToE[i];
                    }
                    else
                    {
                        m_bpInToE[i] = 0;
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