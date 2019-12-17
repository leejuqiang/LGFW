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
    public class RectIterator
    {
        private int m_zeroIndex;
        public Vector2Int m_pos;
        private Vector2Int m_min;
        private int m_totalWidth;

        private int m_maxX;
        private int m_width;

        public RectIterator(int width, int totalWidth)
        {
            m_totalWidth = totalWidth;
            m_width = width;
        }

        public void reset(int zeroIndex, int x, int y)
        {
            m_zeroIndex = zeroIndex;
            m_min.Set(x, y);
            m_pos.Set(m_min.x - 1, m_min.y);
            m_maxX = m_min.x + m_width;
        }


        public int next()
        {
            ++m_pos.x;
            if (m_pos.x >= m_maxX)
            {
                m_pos.x = m_min.x;
                ++m_pos.y;
                m_zeroIndex += m_totalWidth;
            }
            return m_zeroIndex + m_pos.x;
        }
    }

    /// <summary>
    /// A convolutional neural network layer
    /// </summary>
    public class NNConvolutionLayer : NNLayerBase
    {
        private int m_channel;
        private int m_filter;
        private Vector3Int m_imageSize;
        private Vector2Int m_stride;
        private Vector3Int m_step;
        private Vector3Int m_filterSize;

        private Vector2Int m_padding;
        private int m_weightLength;
        private RectIterator m_imageIterator;
        private bool m_usePadding;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel"> The channel of the input image</param>
        /// <param name="filter">The filter number</param>
        /// <param name="imageSize">The size of the input image</param>
        /// <param name="filterSize">The size of the filter</param>
        /// <param name="stride">The stride of the filter's moving</param>
        /// <param name="padding">If true, the filter will align its center to the corner of the input image, this can avoid image shrinking after filtering</param>
        public NNConvolutionLayer(int channel, int filter, Vector2Int imageSize, Vector2Int filterSize, Vector2Int stride, bool padding)
        {
            m_channel = channel;
            m_filter = filter;
            m_imageSize.Set(imageSize.x, imageSize.y, imageSize.x * imageSize.y);
            m_stride = stride;
            m_filterSize.Set(filterSize.x, filterSize.y, filterSize.x * filterSize.y);
            m_weightLength = m_filterSize.z * m_filter;
            m_parameters = new number[m_weightLength + m_filter];
            m_inputNumber = imageSize.x * imageSize.y * channel;
            m_usePadding = padding;
            computePadding();
            m_output = new number[m_step.z * m_filter * m_channel];
            m_imageIterator = new RectIterator(m_filterSize.x, m_imageSize.x);
            m_outputMatrixes = new OutputMatrix[m_step.z];
            for (int i = 0; i < m_outputMatrixes.Length; ++i)
            {
                m_outputMatrixes[i] = new OutputMatrix();
                m_outputMatrixes[i].m_inputIndexes = new int[m_filterSize.z];
                m_outputMatrixes[i].m_paramIndexes = new int[m_filterSize.z];
            }
            initOutMatrix();
        }

        /// <summary>
        /// Gets the output image size for one chanel
        /// </summary>
        /// <returns>The size</returns>
        public Vector2Int getOutputImageSize()
        {
            return new Vector2Int(m_step.x, m_step.y);
        }

        private void computePadding()
        {
            if (m_usePadding)
            {
                m_padding.x = m_filterSize.x >> 1;
                m_padding.y = m_filterSize.y >> 1;
            }
            else
            {
                m_padding.Set(0, 0);
            }
            m_step.x = (m_imageSize.x + m_padding.x) / m_stride.x;
            m_step.y = (m_imageSize.y + m_padding.y) / m_stride.y;
            m_step.z = m_step.x * m_step.y;
        }

        public override void setInputNumber(int number)
        {
        }

        public override void initParameter()
        {
            for (int i = 0; i < m_parameters.Length; ++i)
            {
                m_parameters[i] = Random.Range(0.0f, 1.0f);
            }
        }

        private void expandOutMatrix()
        {
            OutputMatrix[] temp = new OutputMatrix[m_output.Length];
            System.Array.Copy(m_outputMatrixes, temp, m_step.z);
            int outI = 0;
            int pOffset = 0;
            int imageOffset = 0;
            for (int c = 0; c < m_channel; ++c)
            {
                pOffset = 0;
                for (int f = 0; f < m_filter; ++f)
                {
                    if (outI < m_step.z)
                    {
                        outI += m_step.z;
                    }
                    else
                    {
                        for (int i = 0; i < m_step.z; ++i)
                        {
                            OutputMatrix m = new OutputMatrix();
                            m.m_inputIndexes = new int[m_filterSize.z];
                            m.m_paramIndexes = new int[m_filterSize.z];
                            for (int j = 0; j < m_filterSize.z; ++j)
                            {
                                if (m_outputMatrixes[i].m_inputIndexes[j] >= 0)
                                {
                                    m.m_inputIndexes[j] = m_outputMatrixes[i].m_inputIndexes[j] + imageOffset;
                                }
                                else
                                {
                                    m.m_inputIndexes[j] = -1;
                                }
                                m.m_paramIndexes[j] = m_outputMatrixes[i].m_paramIndexes[j] + pOffset;
                            }
                            m.m_biasIndex = m_outputMatrixes[i].m_biasIndex + f;
                            ++outI;
                        }
                    }
                    pOffset += m_filterSize.z;
                }
                imageOffset += m_imageSize.z;
            }
            m_outputMatrixes = temp;
        }

        private void initOutMatrix()
        {
            int outIndex = 0;
            int imageY = -m_padding.y;
            for (int sy = 0; sy < m_step.y; ++sy)
            {
                int imageX = -m_padding.x;
                int imagePos = imageY * m_imageSize.x;
                for (int sx = 0; sx < m_step.x; ++sx)
                {
                    OutputMatrix m = m_outputMatrixes[outIndex];
                    ++outIndex;
                    m_imageIterator.reset(imagePos, imageX, imageY);
                    for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                    {
                        m.m_paramIndexes[j] = j;
                        int n = m_imageIterator.next();
                        if (m_imageIterator.m_pos.x < 0 || m_imageIterator.m_pos.x >= m_imageSize.x || m_imageIterator.m_pos.y < 0 || m_imageIterator.m_pos.y >= m_imageSize.y)
                        {
                            m.m_inputIndexes[j] = -1;
                        }
                        else
                        {
                            m.m_inputIndexes[j] = n;
                        }
                    }
                    m.m_biasIndex = m_weightLength;
                    imageX += m_stride.x;
                }
                imageY += m_stride.y;
            }
        }

        public override void enableTraining(int layerIndex)
        {
            m_parametersGD = new number[m_parameters.Length];
            if (layerIndex > 0)
            {
                m_bpInToE = new number[m_inputNumber];
            }
            expandOutMatrix();
        }

        protected override void normalOutput(number[] input)
        {
            int outIndex = 0;
            int imageOffset = 0;
            int pOffset = 0;
            for (int c = 0; c < m_channel; ++c)
            {
                int imageY = -m_padding.y;
                for (int f = 0; f < m_filter; ++f)
                {
                    for (int i = 0; i < m_step.z; ++i)
                    {
                        OutputMatrix m = m_outputMatrixes[i];
                        m_output[outIndex] = 0;
                        for (int j = 0; j < m_filterSize.z; ++j)
                        {
                            if (m.m_inputIndexes[j] >= 0)
                            {
                                m_output[outIndex] += input[m.m_inputIndexes[j] + imageOffset] * m_parameters[m.m_paramIndexes[j] + pOffset];
                            }
                        }
                        m_output[outIndex] += m_parameters[m.m_biasIndex + f];
                        ++outIndex;
                    }
                    pOffset += m_filterSize.z;
                }
                imageOffset += m_imageSize.z;
            }
        }

        protected override void inTrainingOutput(number[] input)
        {
            if (m_dropRate > 0)
            {
                for (int i = 0; i < m_outputMatrixes.Length; ++i)
                {
                    if (m_outputMatrixes[i].m_enable)
                    {
                        OutputMatrix m = m_outputMatrixes[i];
                        m_output[i] = 0;
                        for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                        {
                            if (m.m_inputIndexes[j] >= 0)
                            {
                                m_output[i] += input[m.m_inputIndexes[j]] * m_parameters[m.m_paramIndexes[j]];
                            }
                        }
                        m_output[i] += m_parameters[m.m_biasIndex];
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
                    OutputMatrix m = m_outputMatrixes[i];
                    m_output[i] = 0;
                    for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                    {
                        if (m.m_inputIndexes[j] >= 0)
                        {
                            m_output[i] += input[m.m_inputIndexes[j]] * m_parameters[m.m_paramIndexes[j]];
                        }
                    }
                    m_output[i] += m_parameters[m.m_biasIndex];
                }
            }
        }

        protected override void paramToE(number[] outToE)
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
                            if (m.m_inputIndexes[j] >= 0)
                            {
                                m_parametersGD[m.m_paramIndexes[j]] += outToE[i] * m_input[m.m_inputIndexes[j]];
                            }
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
                        if (m.m_inputIndexes[j] >= 0)
                        {
                            m_parametersGD[m.m_paramIndexes[j]] += outToE[i] * m_input[m.m_inputIndexes[j]];
                        }
                    }
                    m_parametersGD[m.m_biasIndex] += outToE[i];
                }
            }
        }

        public override void backPropagate(number[] outToE, bool isFirst)
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
                                if (m.m_inputIndexes[j] >= 0)
                                {
                                    m_bpInToE[m.m_inputIndexes[j]] += outToE[i] * m_parameters[m.m_paramIndexes[j]];
                                }
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
                            if (m.m_inputIndexes[j] >= 0)
                            {
                                m_bpInToE[m.m_inputIndexes[j]] += outToE[i] * m_parameters[m.m_paramIndexes[j]];
                            }
                        }
                    }
                }
            }
        }
    }
}
