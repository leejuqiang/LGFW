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
    /// A neural network
    /// </summary>
    public class NeuralNetwork
    {
        private List<NNLayerBase> m_layers;
        private number m_learningRate;
        private int m_inputNumber;
        private NNLossBase m_loss;

        /// <summary>
        /// The layers in the neural network
        /// </summary>
        /// <value></value>
        public List<NNLayerBase> Layers
        {
            get { return m_layers; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputNumber">The input number of the nerual network</param>
        public NeuralNetwork(int inputNumber)
        {
            m_layers = new List<NNLayerBase>();
            m_inputNumber = inputNumber;
        }

        /// <summary>
        /// Gets the output of the network
        /// </summary>
        /// <param name="input">The input of the network</param>
        /// <returns>The output</returns>
        public number[] output(number[] input)
        {
            for (int i = 0; i < m_layers.Count; ++i)
            {
                input = m_layers[i].output(input);
            }
            return input;
        }

        /// <summary>
        /// Sets the loss function
        /// </summary>
        /// <param name="loss">The loss function</param>
        public void setLoss(NNLossBase loss)
        {
            m_loss = loss;
            m_loss.setInputNumber(m_layers[m_layers.Count - 1].OutputLength);
        }

        /// <summary>
        /// Adds a layer to the neural network
        /// </summary>
        /// <param name="layer">The layer</param>
        public void addLayer(NNLayerBase layer)
        {
            m_layers.Add(layer);
            if (m_layers.Count > 1)
            {
                NNLayerBase last = m_layers[m_layers.Count - 2];
                layer.setInputNumber(last.OutputLength);
            }
            else
            {
                layer.setInputNumber(m_inputNumber);
            }
            if (m_loss != null)
            {
                m_loss.setInputNumber(layer.OutputLength);
            }
        }

        /// <summary>
        /// Enables this network for training
        /// </summary>
        public void enableTraining()
        {
            for (int i = 0; i < m_layers.Count; ++i)
            {
                m_layers[i].enableTraining(i);
            }
        }

        /// <summary>
        /// Trains the network using a data set
        /// </summary>
        /// <param name="trainSet">The input list of the data set</param>
        /// <param name="trainLabel">The expected result list of the data set</param>
        /// <param name="times">The training times</param>
        public void train(List<number[]> trainSet, List<number[]> trainLabel, int times)
        {
            m_loss.setDataSetSize(trainSet.Count);
            for (int i = 0; i < m_layers.Count; ++i)
            {
                m_layers[i].startTrain();
            }
            for (int t = 0; t < times; ++t)
            {
                for (int i = 0; i < m_layers.Count; ++i)
                {
                    m_layers[i].clearForTraining();
                }
                for (int i = 0; i < trainSet.Count; ++i)
                {
                    number[] predict = output(trainSet[i]);
                    number[] outToE = m_loss.bpInputToE(predict, trainLabel[i]);
                    for (int j = m_layers.Count - 1; j >= 0; --j)
                    {
                        m_layers[j].backPropagate(outToE, j == 0);
                        outToE = m_layers[j].m_bpInToE;
                    }
                }
                for (int i = 0; i < m_layers.Count; ++i)
                {
                    m_layers[i].updateParameters(m_learningRate);
                }
            }
            for (int i = 0; i < m_layers.Count; ++i)
            {
                m_layers[i].endTrain();
            }
        }

        /// <summary>
        /// Gets the loss of a data set
        /// </summary>
        /// <param name="trainSet">The input list of the data set</param>
        /// <param name="trainLabel">The expected result list of the data set</param>
        /// <returns>The loss value</returns>
        public number getLoss(List<number[]> trainSet, List<number[]> trainLabel)
        {
            m_loss.setDataSetSize(trainSet.Count);
            m_loss.clearLoss();
            for (int i = 0; i < trainSet.Count; ++i)
            {
                number[] predict = output(trainSet[i]);
                m_loss.addLoss(predict, trainLabel[i]);
            }
            return m_loss.Loss;
        }

        /// <summary>
        /// Initializes all layers' parameters
        /// </summary>
        public void initParameter()
        {
            for (int i = 0; i < m_layers.Count; ++i)
            {
                m_layers[i].initParameter();
            }
        }

        /// <summary>
        /// Serializes the network to a json string
        /// </summary>
        /// <returns></returns>
        public string toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_layers.Count; ++i)
            {
                l.Add(m_layers[i].parameterToJson());
            }
            return Json.encode(l, false);
        }

        /// <summary>
        /// Deserializes the network from a json string
        /// </summary>
        /// <param name="js">The json string</param>
        public void fromJson(string js)
        {
            List<object> l = (List<object>)Json.decode(js);
            if (l.Count != m_layers.Count)
            {
                Debug.LogError("Expects " + m_layers.Count + " layers, but gets " + l.Count);
                return;
            }
            for (int i = 0; i < l.Count; ++i)
            {
                m_layers[i].parameterFromJson(l[i]);
            }
        }
    }
}