using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for a neural network
    /// </summary>
    public abstract class NNBase
    {
        /// <summary>
        /// The regularization lambda, regularization is 0.5 * lambda * sum(weight * weight)
        /// </summary>
        public double m_regularizationLambda = 0;
        /// <summary>
        /// The training data list
        /// </summary>
        public List<double[]> m_trainingSet;
        /// <summary>
        /// The training data's output
        /// </summary>
        public List<double[]> m_trainingResultSet;
        /// <summary>
        /// The cost function
        /// </summary>
        public NNCostType m_costType = NNCostType.quadratic;

        /// <summary>
        /// The input of the neural network
        /// </summary>
        public double[] m_inputs;

        protected double quadraticCost(double[] output, double[] result)
        {
            double ret = 0;
            for (int i = 0; i < output.Length; ++i)
            {
                double o = output[i] - result[i];
                ret += o * o;
            }
            return ret;
        }

        protected double entropyCost(double[] output, double[] result)
        {
            double ret = 0;
            for (int i = 0; i < output.Length; ++i)
            {
                ret += result[i] * System.Math.Log(output[i]) + (1 - result[i]) * System.Math.Log(1 - output[i]);
            }
            return -ret;
        }

        /// <summary>
        /// Sets the neural network's training mode
        /// </summary>
        /// <param name="isTraining">Is training mod</param>
        public virtual void setTrainingMode(bool isTraining)
        {
            if (isTraining)
            {
                m_trainingSet = new List<double[]>();
                m_trainingResultSet = new List<double[]>();
            }
            else
            {
                m_trainingResultSet = null;
                m_trainingSet = null;
            }
        }

        /// <summary>
        /// Gets the output of the neural network
        /// </summary>
        /// <returns>The output</returns>
        public abstract double[] output();

        protected abstract double getWeightSquare();

        protected virtual double basicError()
        {
            double count = 0;
            for (int i = 0; i < m_trainingSet.Count; ++i)
            {
                m_inputs = m_trainingSet[i];
                double[] r = output();
                if (m_costType == NNCostType.quadratic)
                {
                    count += quadraticCost(r, m_trainingResultSet[i]);
                }
                else if (m_costType == NNCostType.entropy)
                {
                    count += entropyCost(r, m_trainingResultSet[i]);
                }
            }
            if (m_costType == NNCostType.quadratic)
            {
                count /= m_trainingSet.Count * 2;
            }
            else if (m_costType == NNCostType.entropy)
            {
                count /= m_trainingSet.Count;
            }
            return count;
        }

        /// <summary>
        /// The error for the given traning data
        /// </summary>
        /// <returns>The error</returns>
        public virtual double error()
        {
            double count = basicError();
            if (m_regularizationLambda > 0)
            {
                count += 0.5 * m_regularizationLambda * getWeightSquare();
            }
            return count;
        }

        /// <summary>
        /// Saves the parameters to a json string
        /// </summary>
        /// <returns>The json string</returns>
        public virtual string toJson()
        {
            return "";
        }

        /// <summary>
        /// Initializes the parameters with a json string
        /// </summary>
        /// <param name="json">The json string</param>
        public virtual void initWithJson(string json)
        {

        }
    }
}
