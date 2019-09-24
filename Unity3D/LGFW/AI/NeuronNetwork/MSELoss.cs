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
    /// The MSE loss function
    /// </summary>
    public class MSELoss : NNLossBase
    {
        private number m_factor;

        public override number Loss
        {
            get { return m_loss / (m_dataSetSize << 1); }
        }

        public override void setDataSetSize(int size)
        {
            m_dataSetSize = size;
            m_factor = (number)(1.0 / size);
        }

        public override number[] bpInputToE(number[] predict, number[] expect)
        {
            for (int i = 0; i < predict.Length; ++i)
            {
                m_bpInToE[i] = m_factor * (predict[i] - expect[i]);
            }
            return m_bpInToE;
        }

        public override void addLoss(number[] predict, number[] expect)
        {
            for (int i = 0; i < predict.Length; ++i)
            {
                number o = predict[i] - expect[i];
                m_loss += o * o;
            }
        }
    }
}
