using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNDualArray
    {
        public double[] m_array;
        public int[] m_starts;
        public int m_number;
        public int m_length;

        public CNNDualArray()
        {
        }

        public void setStarts(int num, int len)
        {
            m_number = num;
            m_length = len;
            m_starts = new int[num];
            int s = 0;
            for (int i = 0; i < m_starts.Length; ++i, s += len)
            {
                m_starts[i] = s;
            }
        }

        public void setStarts(int num)
        {
            int len = m_array.Length / num;
            setStarts(num, len);
        }

        public CNNDualArray(int num, int length)
        {
            setStarts(num, length);
            m_array = new double[num * length];
        }
    }
}
