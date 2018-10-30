using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum NNLayerType
    {
        sigmoid,
        softMax,
    }

    /// <summary>
    /// Configuration for a neural network layer
    /// </summary>
    public class NNlayerConfig
    {
        /// <summary>
        /// The layer's type
        /// </summary>
        public NNLayerType m_type;
        /// <summary>
        /// The number of the neurons
        /// </summary>
        public int m_neuronNumber;

        public NNlayerConfig(NNLayerType type, int num)
        {
            m_type = type;
            m_neuronNumber = num;
        }
    }

    public class NeuralNetworkLayer
    {
        public double[] m_matrix;
        public int m_neuronNum;
        public int m_weightsNumber;

        public double[] m_matrixGD;
        public double[] m_midOutput;
        public double[] m_output;

        public int m_totalWeightNumber;
        public NeuralNetworkLayer m_previous;
        public NeuralNetworkLayer m_next;
        public NNComputer m_computer;

        public NeuralNetworkLayer(NNLayerType t, int neuronNum, int weightNum)
        {
            switch (t)
            {
                case NNLayerType.softMax:
                    m_computer = new SoftMaxNNComputer();
                    break;
                case NNLayerType.sigmoid:
                default:
                    m_computer = new NNComputer();
                    break;
            }
            m_neuronNum = neuronNum;
            m_weightsNumber = weightNum;
            m_totalWeightNumber = m_weightsNumber * m_neuronNum;
            m_matrix = new double[m_totalWeightNumber + m_neuronNum];
            m_midOutput = new double[neuronNum];
            m_output = new double[neuronNum];
            m_computer.m_layer = this;
        }

        private int getIndex(int neu, int weight)
        {
            return m_weightsNumber * neu + weight;
        }

        public double value(int neu, int weight)
        {
            return m_matrix[getIndex(neu, weight)];
        }

        public void value(int neu, int weight, double v)
        {
            m_matrix[getIndex(neu, weight)] = v;
        }

        public void changeParam(int index, double v)
        {
            m_matrix[index] += v;
        }

        public void initParams(double[] param)
        {
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                m_matrix[i] = param[i];
            }
        }

        public void setTrainingMode(bool training)
        {
            if (training)
            {
                m_matrixGD = new double[m_matrix.Length];
                BPCache bp = new BPCache();
                bp.initDerivativeArray(m_neuronNum, m_weightsNumber);
                bp.m_output = m_output;
                bp.m_midOutput = m_midOutput;
                m_computer.m_bpCache = bp;
            }
            else
            {
                m_matrixGD = null;
                m_computer.m_bpCache = null;
            }
            m_computer.onChangeTrainMode();
        }

        public void setGD(double dc, int index)
        {
            m_matrixGD[index] = dc;
        }

        public void randomlyInitParams(RandomKit k)
        {
            float sd = 1 / Mathf.Sqrt(m_weightsNumber);
            for (int i = 0; i < m_totalWeightNumber; ++i)
            {
                m_matrix[i] = k.randomNormalDistribution(0, sd);
            }
            for (int i = m_totalWeightNumber; i < m_matrix.Length; ++i)
            {
                m_matrix[i] = k.randomNormalDistribution(0, 1);
            }
        }

        public void clearGD()
        {
            for (int i = 0; i < m_matrixGD.Length; ++i)
            {
                m_matrixGD[i] = 0;
            }
        }

        public double getGD(int index)
        {
            return m_matrixGD[index];
        }

        public void setBpDerivativeToGD()
        {
            for (int n = 0, i = 0; n < m_neuronNum; ++n)
            {
                for (int w = 0; w < m_weightsNumber; ++w, ++i)
                {
                    m_matrixGD[i] += m_computer.bpWeightToE(w, n);
                }
            }
            for (int i = m_totalWeightNumber; i < m_matrix.Length; ++i)
            {
                m_matrixGD[i] += m_computer.bpBiasToE(i);
            }
        }

        protected void compute(double[] input)
        {
            for (int j = 0, index = 0, s = m_totalWeightNumber; j < m_neuronNum; ++j, ++s)
            {
                m_midOutput[j] = 0;
                for (int i = 0; i < m_weightsNumber; ++i)
                {
                    m_midOutput[j] += m_matrix[index] * input[i];
                    ++index;
                }
                m_midOutput[j] += m_matrix[s];
            }
        }

        public void addRegularizationToGD(double p)
        {
            for (int i = 0; i < m_totalWeightNumber; ++i)
            {
                m_matrixGD[i] += m_matrix[i] * p;
            }
        }

        public double getWeightSquare()
        {
            double s = 0;
            for (int i = 0; i < m_totalWeightNumber; ++i)
            {
                s += m_matrix[i] * m_matrix[i];
            }
            return s;
        }

        public double[] output(double[] inputs)
        {
            compute(inputs);
            if (m_computer.m_bpCache != null)
            {
                m_computer.m_bpCache.m_inputs = inputs;
            }
            m_computer.beforeComputeOutput();
            for (int i = 0; i < m_output.Length; ++i)
            {
                m_output[i] = m_computer.computeMidToOut(m_midOutput[i], i);
            }
            return m_output;
        }

        public void updateParamsByGD(double rate)
        {
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                m_matrix[i] -= rate * m_matrixGD[i];
            }
        }

        public void setEntropyOutToE(double a, double[] b)
        {
            BPCache bp = m_computer.m_bpCache;
            bp.m_derivativeMidToE = new double[m_neuronNum];
            for (int i = 0; i < bp.m_derivativeMidToE.Length; ++i)
            {
                bp.m_derivativeMidToE[i] = -a * (b[i] / bp.m_output[i] - (1 - b[i]) / (1 - bp.m_output[i]));
            }
        }

        public void setQuadraticOutToE(double a, double[] b)
        {
            BPCache bp = m_computer.m_bpCache;
            bp.m_derivativeMidToE = new double[m_neuronNum];
            for (int i = 0; i < bp.m_derivativeMidToE.Length; ++i)
            {
                bp.m_derivativeMidToE[i] = a * (bp.m_output[i] - b[i]);
            }
        }

        public void bpDerivativeInToE(int inIndex)
        {
            m_computer.computeBpInToE(inIndex);
        }

        public void setPreviousOutToE()
        {
            if (m_previous != null)
            {
                m_previous.m_computer.m_bpCache.m_derivativeMidToE = m_computer.m_bpCache.m_derivativeInToE;
            }
        }

        public void initBpDerivative(double delta)
        {
            m_computer.initBp(delta);
        }

        public List<object> toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                l.Add(m_matrix[i]);
            }
            return l;
        }

        public void fromJson(List<object> l)
        {
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                m_matrix[i] = (double)l[i];
            }
        }
    }
}