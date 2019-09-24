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
    /// Base class of a neural network layer
    /// </summary>
    public abstract class NNLayerBase
    {
        protected number[] m_parameters;
        protected number[] m_parametersGD;
        protected int m_inputNumber;

        public number[] m_input;
        public number[] m_output;
        public OutputMatrix[] m_outputMatrixes;
        public number[] m_bpInToE;

        protected float m_dropRate;
        protected bool m_isTrainingOutput;

        /// <summary>
        /// The parameters of the layer
        /// </summary>
        /// <value></value>
        public number[] Parameters
        {
            get { return m_parameters; }
        }

        /// <summary>
        /// The length of this layer's output
        /// </summary>
        /// <value></value>
        public virtual int OutputLength
        {
            get { return m_output.Length; }
        }

        /// <summary>
        /// Computes the output
        /// </summary>
        /// <param name="input"> The input</param>
        /// <returns>The output</returns>
        public virtual number[] output(number[] input)
        {
            //for test
            //m_isTrainingOutput = true;
            m_input = input;
            if (m_isTrainingOutput)
            {
                inTrainingOutput(input);
            }
            else
            {
                normalOutput(input);
            }
            return m_output;
        }

        /// <summary>
        /// Sets the dropout rate of the layer
        /// </summary>
        /// <param name="rate">If rate > 0, dropout will be enabled</param>
        public void setDropoutRate(float rate)
        {
            m_dropRate = rate;
        }

        protected void randomDropout()
        {
            for (int i = 0; i < m_outputMatrixes.Length; ++i)
            {
                m_outputMatrixes[i].m_enable = Random.Range(0.0f, 1.0f) > m_dropRate;
            }
        }

        /// <summary>
        /// Initializes the parameters
        /// </summary>
        public virtual void initParameter() { }

        /// <summary>
        /// Serializes the parameters into json
        /// </summary>
        /// <returns></returns>
        public virtual object parameterToJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<object> l = new List<object>();
            for (int i = 0; i < m_parameters.Length; ++i)
            {
                l.Add(m_parameters[i]);
            }
            dict["param"] = l;
            return dict;
        }

        public abstract void setInputNumber(int number);

        /// <summary>
        /// Deserializes the parameters from a json string
        /// </summary>
        /// <param name="obj"></param>
        public virtual void parameterFromJson(object obj)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)obj;
            List<object> l = (List<object>)dict["param"];
            if (l.Count != m_parameters.Length)
            {
                Debug.LogError("The layer expects " + m_parameters.Length + " parameters, but gets " + l.Count);
                return;
            }
            for (int i = 0; i < l.Count; ++i)
            {
                m_parameters[i] = (number)(double)l[i];
            }
        }

        /// <summary>
        /// Enables the layer for training
        /// </summary>
        /// <param name="layerIndex">The index of the layer</param>
        public virtual void enableTraining(int layerIndex)
        {
            m_parametersGD = new number[m_parameters.Length];
            m_outputMatrixes = new OutputMatrix[m_output.Length];
            for (int i = 0; i < m_outputMatrixes.Length; ++i)
            {
                m_outputMatrixes[i] = new OutputMatrix();
            }
            //for test
            //randomDropout();
        }

        public virtual void clearForTraining()
        {
            System.Array.Clear(m_parametersGD, 0, m_parametersGD.Length);
            if (m_dropRate > 0)
            {
                randomDropout();
            }
        }

        public virtual void updateParameters(number rate)
        {
            for (int i = 0; i < m_parameters.Length; ++i)
            {
                m_parameters[i] -= m_parametersGD[i] * rate;
            }
        }

        public void paramOffset(int index, number v)
        {
            m_parameters[index] += v;
        }

        public number getGD(int index)
        {
            return m_parametersGD[index];
        }

        public virtual void startTrain()
        {
            m_isTrainingOutput = true;
        }
        public virtual void endTrain()
        {
            m_isTrainingOutput = false;
            if (m_dropRate > 0)
            {
                number f = 1 - m_dropRate;
                for (int i = 0; i < m_parameters.Length; ++i)
                {
                    m_parameters[i] *= f;
                }
            }
        }

        protected virtual void normalOutput(number[] input)
        {

        }

        protected virtual void inTrainingOutput(number[] input)
        {
            if (m_dropRate > 0)
            {
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    if (m_outputMatrixes[i].m_enable)
                    {
                        m_output[i] = m_outputMatrixes[i].compute(input, m_parameters);
                    }
                    else
                    {
                        m_output[i] = 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    m_output[i] = m_outputMatrixes[i].compute(input, m_parameters);
                }
            }
        }

        protected virtual void paramToE(number[] outToE)
        {
            if (m_dropRate > 0)
            {
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    if (m_outputMatrixes[i].m_enable)
                    {
                        OutputMatrix m = m_outputMatrixes[i];
                        for (int j = 0; j < m.m_paramIndexes.Length; ++j)
                        {
                            m_parametersGD[m.m_paramIndexes[j]] += outToE[i] * m_input[m.m_inputIndexes[j]];
                        }
                        m_parametersGD[m.m_biasIndex] += outToE[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    OutputMatrix m = m_outputMatrixes[i];
                    for (int j = 0; j < m.m_paramIndexes.Length; ++j)
                    {
                        m_parametersGD[m.m_paramIndexes[j]] += outToE[i] * m_input[m.m_inputIndexes[j]];
                    }
                    m_parametersGD[m.m_biasIndex] += outToE[i];
                }
            }
        }

        public virtual void backPropagate(number[] outToE, bool isFirst)
        {
            paramToE(outToE);
            if (!isFirst)
            {
                for (int i = 0; i < m_inputNumber; ++i)
                {
                    m_bpInToE[i] = 0;
                }
                if (m_dropRate > 0)
                {
                    for (int i = 0; i < m_outputMatrixes.Length; ++i)
                    {
                        if (m_outputMatrixes[i].m_enable)
                        {
                            OutputMatrix m = m_outputMatrixes[i];
                            for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                            {
                                m_bpInToE[m.m_inputIndexes[j]] += outToE[i] * m_parameters[m.m_paramIndexes[j]];
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < m_outputMatrixes.Length; ++i)
                    {
                        OutputMatrix m = m_outputMatrixes[i];
                        for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                        {
                            m_bpInToE[m.m_inputIndexes[j]] += outToE[i] * m_parameters[m.m_paramIndexes[j]];
                        }
                    }
                }
            }
        }
    }
}