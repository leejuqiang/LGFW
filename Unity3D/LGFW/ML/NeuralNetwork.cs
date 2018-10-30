using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum NNCostType
    {
        quadratic,
        entropy,
    }

    public class NeuralNetwork : NNBase
    {

        /// <summary>
        /// All the layers of the neural network
        /// </summary>
        public NeuralNetworkLayer[] m_layers;
        /// <summary>
        /// The regularization parameter
        /// </summary>
        public double m_regularizationParam = 0;
        /// <summary>
        /// The total number of the training data
        /// </summary>
        public int m_totalTrainingData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputCount">The length of the input array</param>
        /// <param name="configs">The configurations for all layers</param>
        public NeuralNetwork(int inputCount, params NNlayerConfig[] configs)
        {
            m_layers = new NeuralNetworkLayer[configs.Length];
            int w = inputCount;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i] = new NeuralNetworkLayer(configs[i].m_type, configs[i].m_neuronNumber, w);
                w = m_layers[i].m_neuronNum;
            }
            int max = m_layers.Length - 1;
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
        }

        /// <summary>
        /// Randomly sets all the initial parameters for all layers
        /// </summary>
        public void randomAllLayersParam()
        {
            RandomKit k = new RandomKit();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].randomlyInitParams(k);
            }
        }

        /// <inheritdoc/>
        public override void setAsTrainMode()
        {
            base.setAsTrainMode();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].setTrainingMode(true);
            }
        }

        /// <inheritdoc/>
        public override double[] output()
        {
            double[] ret = m_inputs;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                ret = m_layers[i].output(ret);
            }
            return ret;
        }

        /// <inheritdoc/>
        public override double error()
        {
            double count = base.error();
            if (m_regularizationParam > 0)
            {
                double w = 0;
                for (int i = 0; i < m_layers.Length; ++i)
                {
                    w += m_layers[i].getWeightSquare();
                }
                w *= 0.5 * m_regularizationParam / m_totalTrainingData;
                return w + count;
            }
            return count;
        }

        private void bpDerivative(double delta = 0.000001)
        {
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].clearGD();
            }
            double a = 1.0 / m_trainingSet.Count;
            for (int t = 0; t < m_trainingSet.Count; ++t)
            {
                m_inputs = m_trainingSet[t];
                output();
                double[] b = m_trainingResultSet[t];
                if (m_costType == NNCostType.quadratic)
                {
                    m_layers[m_layers.Length - 1].setQuadraticOutToE(a, b);
                }
                else
                {
                    m_layers[m_layers.Length - 1].setEntropyOutToE(a, b);
                }
                for (int i = m_layers.Length - 1; i > 0; --i)
                {
                    m_layers[i].initBpDerivative(delta);
                    int nn = m_layers[i].m_weightsNumber;
                    for (int j = 0; j < nn; ++j)
                    {
                        m_layers[i].bpDerivativeInToE(j);
                    }
                    m_layers[i].setPreviousOutToE();
                }
                m_layers[0].initBpDerivative(delta);
                for (int i = 0; i < m_layers.Length; ++i)
                {
                    m_layers[i].setBpDerivativeToGD();
                }
            }
            if (m_regularizationParam > 0)
            {
                double p = m_regularizationParam / m_totalTrainingData;
                for (int i = 0; i < m_layers.Length; ++i)
                {
                    m_layers[i].addRegularizationToGD(p);
                }
            }
        }

        public void testDerivative(int index)
        {
            int l = 0;

            double e = error();
            m_layers[l].changeParam(index, 0.001);
            double e1 = error();
            double d3 = (e1 - e) / 0.001;
            m_layers[l].changeParam(index, -0.001);
            bpDerivative();
            double d1 = m_layers[l].getGD(index);
            Debug.Log(d1 + "   " + d3 + "  " + (d1 / d3));
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
                m_layers[i].updateParamsByGD(rate);
            }
        }

        /// <summary>
        /// Saves the parameters of this neural network to a json
        /// </summary>
        /// <returns>The json array</returns>
        public string toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                l.Add(m_layers[i].toJson());
            }
            return MiniJSON.Json.Serialize(l, true);
        }

        /// <summary>
        /// Loads the parameters from a json
        /// </summary>
        /// <param name="js">The json array</param>
        public void fromJson(string js)
        {
            List<object> l = (List<object>)MiniJSON.Json.Deserialize(js, true);
            for (int i = 0; i < l.Count; ++i)
            {
                m_layers[i].fromJson((List<object>)l[i]);
            }
        }
    }
}
