using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class AveragePoolComputer : CNNPoolComputerBase
    {
        private double[] m_divideNumbers;
        private double m_sum;
        private int m_count;

        public override void setTraningMode(bool isTraning, CNNBPCache cache)
        {
            base.setTraningMode(isTraning, cache);
            if (isTraning)
            {
                m_divideNumbers = new double[cache.m_output.m_number * cache.m_output.m_length];
            }
            else
            {
                m_divideNumbers = null;
            }
        }

        public override void bpInToE(int inIndex, int x, int y, int outIndex)
        {
            for (int y1 = 0; y1 < m_layer.m_poolStride.y; ++y1)
            {
                if (y + y1 < m_layer.m_inputSize.y)
                {
                    for (int x1 = 0; x1 < m_layer.m_poolStride.x; ++x1)
                    {
                        if (x + x1 < m_layer.m_inputSize.x)
                        {
                            m_layer.m_bpCache.m_inToE[inIndex + x1] = m_layer.m_bpCache.m_midToE[outIndex] * m_divideNumbers[outIndex];
                        }
                    }
                }
                else
                {
                    break;
                }
                inIndex += m_layer.m_inputSize.x;
            }
        }

        public override void computePool(int inIndex, int x, int y, CNNDualArray input, int outIndex)
        {
            m_sum = 0;
            m_count = 0;
            for (int y1 = 0; y1 < m_layer.m_poolStride.y; ++y1)
            {
                if (y + y1 < m_layer.m_inputSize.y)
                {
                    for (int x1 = 0; x1 < m_layer.m_poolStride.x; ++x1)
                    {
                        if (x + x1 < m_layer.m_inputSize.x)
                        {
                            m_sum += input.m_array[inIndex + x1];
                            ++m_count;
                        }
                    }
                }
                else
                {
                    break;
                }
                inIndex += m_layer.m_inputSize.x;
            }
            if (m_divideNumbers != null)
            {
                m_divideNumbers[outIndex] = 1.0 / m_count;
            }
            m_layer.m_output.m_array[outIndex] = m_sum / m_count;
        }
    }
}
