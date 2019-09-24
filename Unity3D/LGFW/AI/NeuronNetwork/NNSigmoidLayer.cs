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
    /// A sigmoid layer
    /// </summary>
    public class NNSigmoidLayer : NNLayerBase
    {
        public NNSigmoidLayer()
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
                number e = System.Math.Exp(-input[i]);
                m_output[i] = 1 / (1 + e);
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
            for (int i = 0; i < m_bpInToE.Length; ++i)
            {
                m_bpInToE[i] = m_output[i] * (1 - m_output[i]) * outToE[i];
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