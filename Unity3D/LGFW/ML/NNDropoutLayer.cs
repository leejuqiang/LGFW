using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class NNDropoutLayer : NeuralNetworkLayer
    {
        public bool[] m_inputMask;
        public bool[] m_outputMask;

        public float m_dropoutRate;
        public int m_dropoutOutLen;

        public override bool[] OutputMask
        {
            get { return m_outputMask; }
        }

        public override bool[] InputMask
        {
            get { return m_inputMask; }
            set { m_inputMask = value; }
        }

        public NNDropoutLayer(NNLayerType type, int neuronNum, int weightNum, float dropoutRate) : base(type, neuronNum, weightNum)
        {
            m_dropoutRate = dropoutRate;
        }

        public override void setTrainingMode(bool training)
        {
            base.setTrainingMode(training);
            if (training)
            {
                m_dropoutOutLen = Mathf.Max((int)(m_dropoutRate * m_neuronNum), 1);
                m_outputMask = new bool[m_neuronNum];
            }
            else
            {
                m_outputMask = null;
                m_inputMask = null;
            }
        }

        public override void initDropout()
        {
            int count = 0;
            for (int i = 0; i < m_outputMask.Length; ++i)
            {
                float r = Random.Range(0.0f, 1.0f);
                if (r < m_dropoutRate)
                {
                    m_outputMask[i] = true;
                    ++count;
                }
                else
                {
                    m_outputMask[i] = false;
                }
            }
            if (count <= 0)
            {
                int i = Random.Range(0, m_neuronNum);
                m_outputMask[i] = true;
            }
        }

        protected override float DropoutRate
        {
            get { return m_dropoutRate; }
        }

        protected override void compute(double[] input)
        {
            for (int j = 0, index = 0, s = m_totalWeightNumber; j < m_neuronNum; ++j, ++s)
            {
                if (m_outputMask == null || m_outputMask[j])
                {
                    m_midOutput[j] = 0;
                    for (int i = 0; i < m_weightsNumber; ++i)
                    {
                        if (m_inputMask == null || m_inputMask[i])
                        {
                            m_midOutput[j] += m_matrix[index] * input[i];
                        }
                        ++index;
                    }
                    m_midOutput[j] += m_matrix[s];
                }
                else
                {
                    index += m_weightsNumber;
                }
            }
        }

        public override void setBpDerivativeToGD()
        {
            for (int n = 0, i = 0; n < m_neuronNum; ++n)
            {
                if (m_outputMask == null || m_outputMask[n])
                {
                    for (int w = 0; w < m_weightsNumber; ++w, ++i)
                    {
                        if (m_inputMask == null || m_inputMask[w])
                        {
                            m_matrixGD[i] += m_computer.bpWeightToE(w, n);
                        }
                    }
                }
                else
                {
                    i += m_weightsNumber;
                }
            }
            for (int i = m_totalWeightNumber, j = 0; i < m_matrix.Length; ++i, ++j)
            {
                if (m_outputMask == null || m_outputMask[j])
                {
                    m_matrixGD[i] += m_computer.bpBiasToE(i);
                }
            }
        }

        public override void bpDerivativeInToE(int inIndex)
        {
            if (m_inputMask == null || m_inputMask[inIndex])
            {
                m_computer.computeBpInToE(inIndex, m_outputMask);
            }
        }

        public override double[] output(double[] inputs)
        {
            compute(inputs);
            if (m_computer.m_bpCache != null)
            {
                m_computer.m_bpCache.m_inputs = inputs;
            }
            m_computer.beforeComputeOutput(m_outputMask);
            for (int i = 0; i < m_output.Length; ++i)
            {
                if (m_outputMask == null || m_outputMask[i])
                {
                    m_output[i] = m_computer.computeMidToOut(m_midOutput[i], i);
                }
            }
            return m_output;
        }
    }
}