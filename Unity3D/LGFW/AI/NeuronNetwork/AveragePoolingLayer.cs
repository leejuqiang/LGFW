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
    /// Average pooling layer
    /// </summary>
    public class AveragePoolingLayer : PoolingLayer
    {
        private number m_inverse;

        public AveragePoolingLayer(Vector2Int imageSize, Vector2Int poolingSize) : base(imageSize, poolingSize)
        {
            m_inverse = 1.0f / m_poolArea.Length;
        }
        protected override number computePool(int outIndex, number[] poolArea)
        {
            number sum = 0;
            for (int i = 0; i < poolArea.Length; ++i)
            {
                sum += poolArea[i];
            }
            return sum * m_inverse;
        }

        protected override number bpInToOut(int inputIndex, int outIndex)
        {
            return m_inverse;
        }
    }
}