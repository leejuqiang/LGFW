using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNFilterMultiStepLayer : CNNFilterLayer
    {
        public Vector2Int m_moveStep;
        private Vector2Int m_moveEnd = Vector2Int.zero;

        public bool m_stayInside;

        public CNNFilterMultiStepLayer(CNNFilterLayerConfig con, Vector2Int inputSize, int inputNumber) : base(con, inputSize, inputNumber)
        {
        }

        protected override void computeFilterSize(CNNFilterLayerConfig con)
        {
            m_stayInside = con.m_stayInside;
            m_moveStep = con.m_moveStep;
            if (m_stayInside)
            {
                m_filterSize.x = m_inputSize.x - m_filterStride.x + 1;
                m_filterSize.y = m_inputSize.y - m_filterStride.y + 1;
            }
            else
            {
                m_filterSize.x = m_inputSize.x;
                m_filterSize.y = m_inputSize.y;
            }
            int x = m_filterSize.x / m_moveStep.x;
            int y = m_filterSize.y / m_moveStep.y;
            if (x * m_moveStep.x < m_filterSize.x)
            {
                ++x;
            }
            if (y * m_moveStep.y < m_filterSize.y)
            {
                ++y;
            }
            m_filterSize.Set(x, y);
            m_moveEnd.x = x * m_moveStep.x - 1;
            m_moveEnd.y = y * m_moveStep.y - 1;
        }

        private double computeFilter(int inIndex, int x, int y, CNNDualArray input, CNNFilter f)
        {
            int wi = 0;
            double sum = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                bool isOut = y + y1 >= m_inputSize.y;
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    if (isOut || x + x1 >= m_inputSize.x)
                    {
                        sum += 0;
                    }
                    else
                    {
                        sum += f.m_weights[wi] * input.m_array[inIndex + x1];
                    }
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
            return sum + f.m_weights[wi];
        }

        //first filter n outputs, second filter n outputs ...
        public override CNNDualArray output(CNNDualArray input)
        {
            if (m_bpCache != null)
            {
                m_bpCache.m_input = input;
            }
            int outIndex = 0;
            for (int i = 0; i < m_filters.Length; ++i)
            {
                for (int j = 0; j < input.m_number; ++j)
                {
                    for (int y = 0; y < m_moveEnd.y; y += m_moveStep.y)
                    {
                        int index = input.m_starts[j] + y * m_inputSize.x;
                        for (int x = 0; x < m_moveEnd.x; x += m_moveStep.x)
                        {
                            int inIndex = index + x;
                            double d = computeFilter(inIndex, x, y, input, m_filters[i]);
                            if (m_bpCache != null)
                            {
                                m_bpCache.m_midOutput[outIndex] = d;
                            }
                            m_output.m_array[outIndex] = m_computer.computeMidToOut(d);
                            ++outIndex;
                        }
                    }
                }
            }
            return m_output;
        }

        private void computeInToE(int inIndex, int x, int y, int outputIndex, CNNFilter f)
        {
            int wi = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                if (y + y1 >= m_inputSize.y)
                {
                    break;
                }
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    if (x + x1 < m_inputSize.x)
                    {
                        m_bpCache.m_inToE[inIndex + x1] += f.m_weights[wi] * m_bpCache.m_midToE[outputIndex];
                    }
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
        }

        public override void bpInToE()
        {
            int outIndex = 0;
            for (int f = 0; f < m_filters.Length; ++f)
            {
                for (int i = 0; i < m_bpCache.m_input.m_number; ++i)
                {
                    for (int y = 0; y < m_moveEnd.y; y += m_moveStep.y)
                    {
                        int index = m_bpCache.m_input.m_starts[i] + y * m_inputSize.x;
                        for (int x = 0; x < m_moveEnd.x; x += m_moveStep.x)
                        {
                            computeInToE(index + x, x, y, outIndex, m_filters[f]);
                            ++outIndex;
                        }
                    }
                }
            }
        }

        private void paramToE(int inIndex, int x, int y, int outIndex, CNNFilter f)
        {
            int wi = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                if (y + y1 >= m_inputSize.y)
                {
                    break;
                }
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    if (x + x1 < m_inputSize.x)
                    {
                        f.m_weightsGD[wi] += m_bpCache.m_input.m_array[inIndex + x1] * m_bpCache.m_midToE[outIndex];
                    }
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
            f.m_weightsGD[f.m_weights.Length - 1] += m_bpCache.m_midToE[outIndex];
        }

        public override void setBpParams()
        {
            int outIndex = 0;
            for (int f = 0; f < m_filters.Length; ++f)
            {
                for (int i = 0; i < m_bpCache.m_input.m_number; ++i)
                {
                    for (int y = 0; y < m_moveEnd.y; y += m_moveStep.y)
                    {
                        int index = m_bpCache.m_input.m_starts[i] + y * m_inputSize.x;
                        for (int x = 0; x < m_moveEnd.x; x += m_moveStep.x)
                        {
                            paramToE(index + x, x, y, outIndex, m_filters[f]);
                            ++outIndex;
                        }
                    }
                }
            }
        }
    }
}