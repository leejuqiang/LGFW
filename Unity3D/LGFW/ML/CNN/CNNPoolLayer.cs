using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNPoolLayer : CNNLayer
    {

        public Vector2Int m_poolStride;
        public Vector2Int m_inputSize;
        public Vector2Int m_poolSize;
        public Vector2Int m_outputPoolStride;
        public CNNPoolComputerBase m_poolComputer;

        public CNNPoolLayer(CNNLayerType t, Vector2Int poolStride, Vector2Int inputSize, int inputNum)
        {
            switch (t)
            {
                case CNNLayerType.maxPooling:
                    m_poolComputer = new MaxPoolComputer();
                    break;
                case CNNLayerType.averagePooling:
                default:
                    m_poolComputer = new AveragePoolComputer();
                    break;
            }
            m_poolComputer.m_layer = this;
            m_poolStride = poolStride;
            m_inputSize = inputSize;
            m_outputPoolStride = m_poolComputer.getOutputPoolArea();
            m_poolSize.x = m_inputSize.x / m_poolStride.x;
            if (m_poolSize.x * m_poolStride.x < m_inputSize.x)
            {
                ++m_poolSize.x;
            }
            m_poolSize.y = m_inputSize.y / m_poolStride.y;
            if (m_poolStride.y * m_poolSize.y < m_inputSize.y)
            {
                ++m_poolSize.y;
            }
            m_poolSize.x *= m_outputPoolStride.x;
            m_poolSize.y *= m_outputPoolStride.y;
            int len = m_poolSize.x * m_poolSize.y;
            m_output = new CNNDualArray(inputNum, len);
        }

        public override void clearBP()
        {
            m_poolComputer.clearBP();
        }

        public override void initBP()
        {
            m_poolComputer.initBP();
        }

        public override void setTrainMode(bool isTraining)
        {
            base.setTrainMode(isTraining);
            m_poolComputer.setTraningMode(isTraining, m_bpCache);
        }

        public override CNNDualArray output(CNNDualArray input)
        {
            if (m_bpCache != null)
            {
                m_bpCache.m_input = input;
            }
            int outIndexStart = 0;
            for (int i = 0; i < input.m_number; ++i)
            {
                for (int y = 0; y < m_inputSize.y; y += m_poolStride.y)
                {
                    int inIndex = m_inputSize.x * y + input.m_starts[i];
                    int outIndex = outIndexStart;
                    for (int x = 0; x < m_inputSize.x; x += m_poolStride.x)
                    {
                        m_poolComputer.computePool(inIndex + x, x, y, input, outIndex);
                        outIndex += m_outputPoolStride.x;
                    }
                    outIndexStart += m_poolSize.x * m_outputPoolStride.y;
                }
            }
            return m_output;
        }

        public override void setOutToE(double[] outToE)
        {
            m_bpCache.m_midToE = outToE;
        }

        public override void bpInToE()
        {
            int outIndexStart = 0;
            for (int i = 0; i < m_bpCache.m_input.m_number; ++i)
            {
                for (int y = 0; y < m_inputSize.y; y += m_poolStride.y)
                {
                    int inIndex = y * m_inputSize.x + m_bpCache.m_input.m_starts[i];
                    int outIndex = outIndexStart;
                    for (int x = 0; x < m_inputSize.x; x += m_poolStride.x)
                    {
                        m_poolComputer.bpInToE(inIndex + x, x, y, outIndex);
                        outIndex += m_outputPoolStride.x;
                    }
                    outIndexStart += m_poolSize.x * m_outputPoolStride.y;
                }
            }
        }
    }
}