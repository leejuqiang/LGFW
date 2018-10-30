using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MaxPoolComputer : CNNPoolComputerBase
    {
        public bool[] m_maxMap;
        private int m_maxI;
        private double m_max;

        public override void setTraningMode(bool isTraning, CNNBPCache cache)
        {
            if (isTraning)
            {
                m_maxMap = new bool[cache.m_inToE.Length];
            }
            else
            {
                m_maxMap = null;
            }
        }

        public override void clearBP()
        {
            for (int i = 0; i < m_maxMap.Length; ++i)
            {
                m_maxMap[i] = false;
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
                            int i = inIndex + x1;
                            if (m_maxMap[i])
                            {
                                m_layer.m_bpCache.m_inToE[i] = m_layer.m_bpCache.m_midToE[outIndex];
                            }
                            else
                            {
                                m_layer.m_bpCache.m_inToE[i] = 0;
                            }
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
            m_maxI = inIndex;
            m_max = input.m_array[inIndex];
            for (int y1 = 0; y1 < m_layer.m_poolStride.y; ++y1)
            {
                if (y + y1 < m_layer.m_inputSize.y)
                {
                    for (int x1 = 0; x1 < m_layer.m_poolStride.x; ++x1)
                    {
                        if (x + x1 < m_layer.m_inputSize.x)
                        {
                            int i = inIndex + x1;
                            if (input.m_array[i] > m_max)
                            {
                                m_max = input.m_array[i];
                                m_maxI = i;
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
                inIndex += m_layer.m_inputSize.x;
            }
            if (m_maxMap != null && m_layer.m_previous != null)
            {
                m_maxMap[m_maxI] = true;
            }
            m_layer.m_output.m_array[outIndex] = m_max;
        }
    }
}
