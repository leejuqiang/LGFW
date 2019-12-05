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
    /// The pooling layer for convolutional neural network
    /// </summary>
    public abstract class PoolingLayer : NNLayerBase
    {
        protected Vector2Int m_poolingSize;
        protected Vector3Int m_imageSize;
        protected Vector3Int m_steps;
        protected number[] m_poolArea;
        protected int m_channel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imageSize">The input image size of the layer, you can get getOutputImageSize() of a convolutional layer to get the output image size</param>
        /// <param name="poolingSize">The pooling area size</param>
        /// <returns></returns>
        public PoolingLayer(Vector2Int imageSize, Vector2Int poolingSize) : base()
        {
            m_poolingSize = poolingSize;
            m_imageSize.x = imageSize.x;
            m_imageSize.y = imageSize.y;
            m_imageSize.z = imageSize.x * m_imageSize.y;
            m_steps.x = (m_imageSize.x + m_poolingSize.x - 1) / m_poolingSize.x;
            m_steps.y = (m_imageSize.y + m_poolingSize.y - 1) / m_poolingSize.y;
            m_steps.z = m_steps.x * m_steps.y;
            m_poolArea = new number[m_poolingSize.x * m_poolingSize.y];
        }

        public override void enableTraining(int layerIndex)
        {
            m_bpInToE = new number[m_inputNumber];
            OutputMatrix[] temp = new OutputMatrix[m_output.Length];
            System.Array.Copy(m_outputMatrixes, temp, m_outputMatrixes.Length);
            for (int i = m_outputMatrixes.Length; i < temp.Length; ++i)
            {
                temp[i] = new OutputMatrix();
                temp[i].m_inputIndexes = new int[m_poolArea.Length];
            }
            int index = m_outputMatrixes.Length;
            m_outputMatrixes = temp;
            for (int c = 1; c < m_channel; ++c)
            {
                for (int j = 0; j < m_steps.z; ++j, ++index)
                {
                    var last = m_outputMatrixes[index - m_steps.z];
                    for (int i = 0; i < last.m_inputIndexes.Length; ++i)
                    {
                        if (last.m_inputIndexes[i] < 0)
                        {
                            m_outputMatrixes[index].m_inputIndexes[i] = -1;
                        }
                        else
                        {
                            m_outputMatrixes[index].m_inputIndexes[i] = last.m_inputIndexes[i] + m_imageSize.z;
                        }
                    }
                }
            }
        }

        public override void clearForTraining()
        {

        }

        public override void updateParameters(number rate)
        {

        }

        public override void setInputNumber(int number)
        {
            m_inputNumber = number;
            m_channel = number / m_imageSize.z;
            m_output = new number[m_channel * m_steps.z];
            m_outputMatrixes = new OutputMatrix[m_steps.z];
            int i = 0;
            for (; i < m_outputMatrixes.Length; ++i)
            {
                m_outputMatrixes[i] = new OutputMatrix();
                m_outputMatrixes[i].m_inputIndexes = new int[m_poolArea.Length];
            }

            i = 0;
            int imy = 0;
            for (int y = 0; y < m_steps.y; ++y)
            {
                int imx = 0;
                for (int x = 0; x < m_steps.x; ++x)
                {
                    getPoolArea(imx, imy, m_outputMatrixes[i]);
                    ++i;
                    imx += m_poolingSize.x;
                }
                imy += m_poolingSize.y;
            }
        }

        private void getPoolArea(int imx, int imy, OutputMatrix m)
        {
            int index = imx + imy * m_imageSize.x;
            int i = 0;
            for (int y = 0; y < m_poolingSize.y; ++y)
            {
                if (imy + y >= m_imageSize.y)
                {
                    for (int x = 0; x < m_poolingSize.x; ++x)
                    {
                        m.m_inputIndexes[i] = -1;
                        ++i;
                    }
                }
                else
                {
                    for (int x = 0; x < m_poolingSize.x; ++x)
                    {
                        if (imx + x >= m_imageSize.x)
                        {
                            m.m_inputIndexes[i] = -1;
                        }
                        else
                        {
                            m.m_inputIndexes[i] = index + x;
                        }
                        ++i;
                    }
                }
                index += m_imageSize.x;
            }
        }

        /// <summary>
        /// Subclasses override this to compute the output of one pooling area
        /// </summary>
        /// <param name="outIndex">The index of this output</param>
        /// <param name="poolArea">The value of the pooling area, row major</param>
        /// <returns>The number of the output</returns>
        protected abstract number computePool(int outIndex, number[] poolArea);

        protected override void paramToE(number[] outToE)
        {
        }

        public override void endTrain()
        {
            m_isTrainingOutput = false;
        }

        /// <summary>
        /// Subclasses override this to compute the derivative of d_output / d_input
        /// </summary>
        /// <param name="inputIndex">The index of the input</param>
        /// <param name="outIndex">The index of the output</param>
        /// <returns>The derivative</returns>
        protected abstract number bpInToOut(int inputIndex, int outIndex);


        public override void backPropagate(number[] outToE, bool isFirst)
        {
            if (!isFirst)
            {
                for (int i = 0; i < m_inputNumber; ++i)
                {
                    m_bpInToE[i] = 0;
                }
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    OutputMatrix m = m_outputMatrixes[i];
                    for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                    {
                        if (m.m_inputIndexes[j] >= 0)
                        {
                            m_bpInToE[m.m_inputIndexes[j]] += outToE[i] * bpInToOut(m.m_inputIndexes[j], i);
                        }
                    }
                }
            }
        }

        protected override void normalOutput(number[] input)
        {
            int outI = 0;
            int offset = 0;
            for (int c = 0; c < m_channel; ++c)
            {
                for (int i = 0; i < m_steps.z; ++i)
                {
                    for (int j = 0; j < m_outputMatrixes[i].m_inputIndexes.Length; ++j)
                    {
                        if (m_outputMatrixes[i].m_inputIndexes[j] < 0)
                        {
                            m_poolArea[j] = 0;
                        }
                        else
                        {
                            m_poolArea[j] = input[m_outputMatrixes[i].m_inputIndexes[j] + offset];
                        }
                    }
                    m_output[outI] = computePool(outI, m_poolArea);
                    ++outI;
                }
                offset += m_imageSize.z;
            }
        }

        protected override void inTrainingOutput(number[] input)
        {
            for (int i = 0; i < m_output.Length; ++i)
            {
                for (int j = 0; j < m_outputMatrixes[i].m_inputIndexes.Length; ++j)
                {
                    if (m_outputMatrixes[i].m_inputIndexes[j] < 0)
                    {
                        m_poolArea[j] = 0;
                    }
                    else
                    {
                        m_poolArea[j] = input[m_outputMatrixes[i].m_inputIndexes[j]];
                    }
                }
                m_output[i] = computePool(i, m_poolArea);
            }
        }
    }
}
