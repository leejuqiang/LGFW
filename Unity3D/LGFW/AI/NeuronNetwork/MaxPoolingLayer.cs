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
    /// Max pooling layer
    /// </summary>
    public class MaxPoolingLayer : PoolingLayer
    {
        protected int[] m_maxIndexes;

        public MaxPoolingLayer(Vector2Int imageSize, Vector2Int poolingSize) : base(imageSize, poolingSize)
        {

        }

        public override void setInputNumber(int number)
        {
            base.setInputNumber(number);
            m_maxIndexes = new int[m_output.Length];
        }

        protected override number bpInToOut(int inputIndex, int outIndex)
        {
            if (inputIndex == m_maxIndexes[outIndex])
            {
                return 1;
            }
            return 0;
        }

        protected override number computePool(int outIndex, number[] poolArea)
        {
            number max = poolArea[0];
            m_maxIndexes[outIndex] = m_outputMatrixes[outIndex].m_inputIndexes[0];
            for (int i = 1; i < poolArea.Length; ++i)
            {
                if (poolArea[i] > max)
                {
                    m_maxIndexes[outIndex] = m_outputMatrixes[outIndex].m_inputIndexes[i];
                    max = poolArea[i];
                }
            }
            return max;
        }
    }
}