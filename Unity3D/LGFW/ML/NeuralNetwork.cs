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
        /// The total number of the training data
        /// </summary>
        public int m_totalTrainingData;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputCount">The length of the input array</param>
        /// <param name="useDropout">If use dropout</param>
        /// <param name="configs">The configurations for all layers</param>
        public NeuralNetwork(int inputCount, bool useDropout, params NNlayerConfig[] configs)
        {
            m_layers = new NeuralNetworkLayer[configs.Length];
            int max = m_layers.Length - 1;
            configs[max].m_dropoutRate = 1;
            int w = inputCount;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                if (useDropout && configs[i].m_dropoutRate < 1)
                {
                    m_layers[i] = new NNDropoutLayer(configs[i].m_type, configs[i].m_neuronNumber, w, configs[i].m_dropoutRate);
                }
                else
                {
                    m_layers[i] = new NeuralNetworkLayer(configs[i].m_type, configs[i].m_neuronNumber, w);
                }
                w = m_layers[i].m_neuronNum;
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
        public override void setTrainingMode(bool isTraining)
        {
            base.setTrainingMode(isTraining);
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].setTrainingMode(isTraining);
            }
        }

        /// <inheritdoc/>
        public override double[] output()
        {
            double[] ret = m_inputs;
            bool[] inputMask = null;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                ret = m_layers[i].output(ret);
                inputMask = m_layers[i].OutputMask;
            }
            return ret;
        }

        protected override double getWeightSquare()
        {
            double w = 0;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                w += m_layers[i].getWeightSquare();
            }
            return w;
        }

        private void bpDerivative(double delta = 0.000001)
        {
            bool[] inMask = null;
            for (int i = 0; i < m_layers.Length; ++i)
            {
                m_layers[i].InputMask = inMask;
                m_layers[i].clearGD();
                m_layers[i].initDropout();
                inMask = m_layers[i].OutputMask;
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
            if (m_regularizationLambda > 0)
            {
                for (int i = 0; i < m_layers.Length; ++i)
                {
                    m_layers[i].addRegularizationToGD(m_regularizationLambda);
                }
            }
        }

        public void testDropout()
        {
            int count = Random.Range(1, 20);
            bpDerivative();
            for (int i = 0; i < m_layers[0].m_matrixGD.Length; ++i)
            {
                if (m_layers[0].m_matrixGD[i] > 0)
                {
                    --count;
                    if (count <= 0)
                    {
                        double e = error();
                        m_layers[0].changeParam(i, 0.0001);
                        double e1 = error();
                        e = (e1 - e) / 0.0001;
                        e1 = m_layers[0].m_matrixGD[i];
                        Debug.Log("index " + i);
                        Debug.Log(e + "  " + e1 + "  " + (e / e1));
                        return;
                    }
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

        /// <inheritdoc/>
        public override string toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_layers.Length; ++i)
            {
                l.Add(m_layers[i].toJson());
            }
            return MiniJSON.Json.Serialize(l, true);
        }

        /// <inheritdoc/>
        public override void initWithJson(string json)
        {
            initDropoutWithJson(json, true);
        }

        /// <summary>
        /// Initializes the weights from a json string
        /// </summary>
        /// <param name="json">The json string</param>
        /// <param name="useDropout">If true, the weights will be multiply with the dropout rate</param>
        public void initDropoutWithJson(string json, bool useDropout)
        {
            List<object> l = (List<object>)MiniJSON.Json.Deserialize(json, true);
            for (int i = 0; i < l.Count; ++i)
            {
                m_layers[i].fromJson((Dictionary<string, object>)l[i], useDropout);
            }
        }
    }
}