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

    public class OutputMatrix
    {
        public int[] m_inputIndexes;
        public int[] m_paramIndexes;
        public int m_biasIndex;
        public bool m_enable;

        public number compute(number[] input, number[] param)
        {
            number ret = 0;
            for (int i = 0; i < m_inputIndexes.Length; ++i)
            {
                ret += input[m_inputIndexes[i]] * param[m_paramIndexes[i]];
            }
            ret += param[m_biasIndex];
            return ret;
        }
    }
}
