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
    /// The base class of a loss function
    /// </summary>
    public abstract class NNLossBase
    {
        protected int m_dataSetSize;
        protected number[] m_bpInToE;
        protected number m_loss;

        /// <summary>
        /// Gets the loss
        /// </summary>
        /// <value></value>
        public virtual number Loss
        {
            get { return m_loss; }
        }

        public void clearLoss()
        {
            m_loss = 0;
        }

        public virtual void setDataSetSize(int size)
        {
            m_dataSetSize = size;
        }

        public virtual void setInputNumber(int number)
        {
            m_bpInToE = new number[number];
        }

        public abstract void addLoss(number[] predict, number[] expect);

        public abstract number[] bpInputToE(number[] predict, number[] expect);
    }
}
