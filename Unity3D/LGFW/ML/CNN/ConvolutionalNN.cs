using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A convolutional neural network
    /// </summary>
    public class ConvolutionalNN : NNBase
    {

        private CNNLayer[] m_layers;
        private NeuralNetworkLayer[] m_lastLayers;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fullLayers">The configurations for the last full connection layers</param>
        /// <param name="inputSize">The area size</param>
        /// <param name="configs">The configurations for each convolutional layers</param>
        public ConvolutionalNN(Vector2Int inputSize, NNlayerConfig[] fullLayers, params CNNLayerConfig[] configs)
        {
            m_layers = new CNNLayer[configs.Length];
            m_lastLayers = new NeuralNetworkLayer[fullLayers.Length];
            int inputNum = 1;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                int t = configs[i].m_type;
                if (t < 0)
                {
                    CNNFilterLayerConfig con = (CNNFilterLayerConfig)configs[i];
                    CNNFilterLayer l = con.isMultiStep() ? new CNNFilterMultiStepLayer(con, inputSize, inputNum) : new CNNFilterLayer(con, inputSize, inputNum);
                    inputSize = l.m_filterSize;
                    m_layers[i] = l;
                    inputNum *= con.m_filterCount;
                }
                else
                {
                    CNNPoolLayerConfig con = (CNNPoolLayerConfig)configs[i];
                    CNNPoolLayer l = new CNNPoolLayer((CNNLayerType)t, con.m_poolStride, inputSize, inputNum);
                    m_layers[i] = l;
                    inputSize = l.m_poolSize;
                }
            }
            int max = m_layers.Length - 1;
            int w = m_layers[max].totalOutputSize();
            for (int i = 0; i < fullLayers.Length; ++i)
            {
                m_lastLayers[i] = new NeuralNetworkLayer(fullLayers[i].m_type, fullLayers[i].m_neuronNumber, w);
                w = m_lastLayers[i].m_neuronNum;
            }
            for (int i = 0; i < m_layers.Length; ++i)
            {
                if (i > 0)
                {
                    m_layers[i].m_previous = m_layers[i - 1];
                }
                if (i < max)
                {
                    m_layers[i].m_next = m_layers[i + 1];
                }
            }
            max = m_lastLayers.Length - 1;
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                if (i > 0)
                {
                    m_lastLayers[i].m_previous = m_lastLayers[i - 1];
                }
                if (i < max)
                {
                    m_lastLayers[i].m_next = m_lastLayers[i + 1];
                }
            }
        }

        public void test(int layer, int index)
        {
            double delta = 0.000001;
            double e = error();
            CNNFilterLayer l = (CNNFilterLayer)m_layers[0];
            l.m_filters[layer].changeParams(index, delta);
            e = error() - e;
            e /= delta;
            l.m_filters[layer].changeParams(index, -delta);
            bpDerivative();
            double e1 = l.m_filters[layer].getGD(index);
            Debug.Log("bp " + e + " " + e1 + " " + (e / e1));
        }

        /// <inheritdoc/>
        public override void setAsTrainMode()
        {
            base.setAsTrainMode();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].setTrainMode(true);
            }
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                m_lastLayers[i].setTrainingMode(true);
            }
        }

        /// <summary>
        /// Randomly sets all parameters for all layers
        /// </summary>
        public void randomParams()
        {
            RandomKit rk = new RandomKit();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].randomParams(rk);
            }
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                m_lastLayers[i].randomlyInitParams(rk);
            }
        }

        /// <inheritdoc/>
        public override double[] output()
        {
            CNNDualArray output = new CNNDualArray();
            output.m_array = m_inputs;
            output.setStarts(1, m_inputs.Length);
            for (int i = 0; i < m_layers.Length; ++i)
            {
                output = m_layers[i].output(output);
            }
            double[] ret = output.m_array;
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                ret = m_lastLayers[i].output(ret);
            }
            return ret;
        }

        private void bpDerivative(double delta = 0.000001)
        {
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].clearGD();
            }
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                m_lastLayers[i].clearGD();
            }
            double a = 1.0 / m_trainingSet.Count;
            for (int t = 0; t < m_trainingSet.Count; ++t)
            {
                for (int i = 0; i < m_layers.Length; ++i)
                {
                    m_layers[i].clearBP();
                }
                m_inputs = m_trainingSet[t];
                output();
                double[] b = m_trainingResultSet[t];
                int last = m_lastLayers.Length - 1;
                if (m_costType == NNCostType.quadratic)
                {
                    m_lastLayers[last].setQuadraticOutToE(a, b);
                }
                else
                {
                    m_lastLayers[last].setEntropyOutToE(a, b);
                }

                for (int i = last; i >= 0; --i)
                {
                    m_lastLayers[i].initBpDerivative(delta);
                    int nn = m_lastLayers[i].m_weightsNumber;
                    for (int j = 0; j < nn; ++j)
                    {
                        m_lastLayers[i].bpDerivativeInToE(j);
                    }
                    m_lastLayers[i].setPreviousOutToE();
                }
                for (int i = 0; i < m_lastLayers.Length; ++i)
                {
                    m_lastLayers[i].setBpDerivativeToGD();
                }

                double[] inToE = m_lastLayers[0].m_computer.m_bpCache.m_derivativeInToE;
                for (int i = m_layers.Length - 1; i > 0; --i)
                {
                    m_layers[i].setOutToE(inToE);
                    m_layers[i].bpInToE();
                    inToE = m_layers[i].m_bpCache.m_inToE;
                    m_layers[i].setBpParams();
                }
                m_layers[0].setOutToE(inToE);
                m_layers[0].setBpParams();
            }
        }

        /// <summary>
        /// Changes the parameter by gradient descent
        /// </summary>
        /// <param name="rate">The learning rate</param>
        /// <param name="delta">The delta value for derivative</param>
        public void gradientDescent(double rate, double delta = 0.000001)
        {
            bpDerivative(delta);
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].applyGD(rate);
            }
            for (int i = 0; i < m_lastLayers.Length; ++i)
            {
                m_lastLayers[i].updateParamsByGD(rate);
            }
        }
    }
}