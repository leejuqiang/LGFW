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

        public override void enableTraining(int layerIndex)
        {
            base.enableTraining(layerIndex);
            if (layerIndex > 0)
            {
                m_bpInToE = new number[m_inputNumber];
            }
            int outIndex = 0;
            int imageOffset = 0;
            for (int c = 0; c < m_channel; ++c)
            {
                int imageY = -m_padding.y;
                for (int f = 0; f < m_filter; ++f)
                {
                    int pIndex = f * m_filterSize.z;
                    for (int sy = 0; sy < m_step.y; ++sy)
                    {
                        int imageX = -m_padding.x;
                        int imagePos = imageY * m_imageSize.x + imageOffset;
                        for (int sx = 0; sx < m_step.x; ++sx)
                        {
                            OutputMatrix m = m_outputMatrixes[outIndex];
                            ++outIndex;
                            m.m_inputIndexes = new int[m_filterSize.z];
                            m.m_paramIndexes = new int[m_filterSize.z];
                            m_imageIterator.reset(imagePos, imageX, imageY);
                            for (int j = 0; j < m.m_inputIndexes.Length; ++j)
                            {
                                m.m_paramIndexes[j] = pIndex + j;
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
                            m.m_biasIndex = m_weightLength + f;
                            imageX += m_stride.x;
                        }
                        imageY += m_stride.y;
                    }
                }
                imageOffset += m_imageSize.z;
            }
        }

        protected override void normalOutput(number[] input)
        {
            int outIndex = 0;
            int imageOffset = 0;
            for (int c = 0; c < m_channel; ++c)
            {
                int imageY = -m_padding.y;
                for (int f = 0; f < m_filter; ++f)
                {
                    int pIndex = f * m_filterSize.z;
                    for (int sy = 0; sy < m_step.y; ++sy)
                    {
                        int imageX = -m_padding.x;
                        int imagePos = imageY * m_imageSize.x + imageOffset;
                        for (int sx = 0; sx < m_step.x; ++sx)
                        {
                            m_output[outIndex] = 0;
                            m_imageIterator.reset(imagePos, imageX, imageY);
                            for (int j = 0; j < m_filterSize.z; ++j)
                            {
                                int n = m_imageIterator.next();
                                if (m_imageIterator.m_pos.x < 0 || m_imageIterator.m_pos.x >= m_imageSize.x || m_imageIterator.m_pos.y < 0 || m_imageIterator.m_pos.y >= m_imageSize.y)
                                {
                                }
                                else
                                {
                                    m_output[outIndex] += m_input[n] * m_parameters[pIndex + j];
                                }
                            }
                            m_output[outIndex] += m_parameters[m_weightLength + f];
                            imageX += m_stride.x;
                            ++outIndex;
                        }
                        imageY += m_stride.y;
                    }
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
