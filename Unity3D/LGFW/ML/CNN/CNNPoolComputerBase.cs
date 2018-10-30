using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public abstract class CNNPoolComputerBase
    {
        public CNNPoolLayer m_layer;

        public virtual void initBP()
        {
        }

        public virtual void clearBP()
        {
        }

        public virtual Vector2Int getOutputPoolArea()
        {
            return Vector2Int.one;
        }

        public virtual void setTraningMode(bool isTraning, CNNBPCache cache)
        {
        }

        public abstract void bpInToE(int inIndex, int x, int y, int outIndex);

        public abstract void computePool(int inIndex, int x, int y, CNNDualArray input, int outIndex);
    }
}
