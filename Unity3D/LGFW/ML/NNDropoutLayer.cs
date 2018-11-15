﻿using System.Collections;
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
        private int[] m_shuffle;

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
                m_shuffle = new int[m_neuronNum];
                for (int i = 0; i < m_shuffle.Length; ++i)
                {
                    m_shuffle[i] = i;
                }
            }
            else
            {
                m_outputMask = null;
                m_inputMask = null;
                m_shuffle = null;
            }
        }

        public override void initDropout()
        {
            LMath.shuffleArray<int>(m_shuffle);
            int i = 0;
            for (; i < m_dropoutOutLen; ++i)
            {
                m_outputMask[m_shuffle[i]] = true;
            }
            for (; i < m_shuffle.Length; ++i)
            {
                m_outputMask[m_shuffle[i]] = false;
            }
        }

        protected override float DropoutRate
        {
            get { return (float)m_dropoutOutLen / m_neuronNum; }
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

        public override void addRegularizationToGD(double p)
        {
            int index = 0;
            for (int i = 0; i < m_neuronNum; ++i)
            {
                if (m_outputMask == null || m_outputMask[i])
                {
                    for (int j = 0; j < m_weightsNumber; ++j)
                    {
                        if (m_inputMask == null || m_inputMask[j])
                        {
                            m_matrixGD[index] += m_matrix[index] * p;
                        }
                        ++index;
                    }
                }
                else
                {
                    index += m_weightsNumber;
                }
            }
        }

        public override double getWeightSquare()
        {
            int index = 0;
            double w = 0;
            for (int i = 0; i < m_neuronNum; ++i)
            {
                if (m_outputMask == null || m_outputMask[i])
                {
                    for (int j = 0; j < m_weightsNumber; ++j)
                    {
                        if (m_inputMask == null || m_inputMask[j])
                        {
                            w += m_matrix[index] * m_matrix[index];
                        }
                        ++index;
                    }
                }
                else
                {
                    index += m_weightsNumber;
                }
            }
            return w;
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