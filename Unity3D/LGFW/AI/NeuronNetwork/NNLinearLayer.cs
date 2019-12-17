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
    /// A linear neural network layer
    /// </summary>
    public class NNLinearLayer : NNLayerBase
    {
        protected int m_neuronNumber;
        protected int m_weightLength;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="neuronNumber">The number of neuron in this layer</param>
        public NNLinearLayer(int neuronNumber) : base()
        {
            m_neuronNumber = neuronNumber;
            m_output = new number[m_neuronNumber];
        }

        public override void setInputNumber(int number)
        {
            m_inputNumber = number;
            m_weightLength = m_neuronNumber * m_inputNumber;
            m_parameters = new number[m_neuronNumber + m_weightLength];
        }

        protected override void normalOutput(number[] input)
        {
            int index = 0;
            for (int i = 0; i < m_neuronNumber; ++i)
            {
                m_output[i] = 0;
                for (int j = 0; j < m_inputNumber; ++j, ++index)
                {
                    m_output[i] += m_input[j] * m_parameters[index];
                }
                m_output[i] += m_parameters[m_weightLength + i];
            }
        }

        public override void initParameter()
        {
            Randomizer k = new Randomizer();
            float sd = 1 / Mathf.Sqrt(m_inputNumber);
            k.FastMode = true;
            for (int i = 0; i < m_weightLength; ++i)
            {
                m_parameters[i] = k.randomNormalDistribution(0, sd);
            }
            for (int i = m_weightLength; i < m_parameters.Length; ++i)
            {
                m_parameters[i] = k.randomNormalDistribution(0, 1);
            }
            k.FastMode = false;
        }

        public override void enableTraining(int layerIndex)
        {
            base.enableTraining(layerIndex);
            if (layerIndex > 0)
            {
                m_bpInToE = new number[m_inputNumber];
            }
            int pIndex = 0;
            for (int i = 0; i < m_outputMatrixes.Length; ++i)
            {
                m_outputMatrixes[i].m_inputIndexes = new int[m_inputNumber];
                m_outputMatrixes[i].m_paramIndexes = new int[m_inputNumber];
                for (int iIndex = 0; iIndex < m_inputNumber; ++iIndex)
                {
                    m_outputMatrixes[i].m_inputIndexes[iIndex] = iIndex;
                }
                for (int j = 0; j < m_inputNumber; ++j, ++pIndex)
                {
                    m_outputMatrixes[i].m_paramIndexes[j] = pIndex;
                }
                m_outputMatrixes[i].m_biasIndex = m_weightLength + i;
            }
        }
    }
}