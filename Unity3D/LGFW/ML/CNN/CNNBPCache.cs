using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNBPCache
    {
        public CNNDualArray m_input;
        public CNNDualArray m_output;
        public double[] m_midOutput;

        public double[] m_inToE;
        public double[] m_midToE;

        public double[][] convertInToE(double[] inToE, int number)
        {
            int len = inToE.Length / number;
            int index = 0;
            double[][] ret = new double[number][];
            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = new double[len];
                for (int j = 0; j < len; ++j, ++index)
                {
                    ret[i][j] = inToE[index];
                }
            }
            return ret;
        }

        public void initAsFilter(CNNDualArray output)
        {
            m_midOutput = new double[output.m_number * output.m_length];
        }

        public void init(int inNumber, int inLen)
        {
            m_inToE = new double[inNumber * inLen];
        }
    }
}
