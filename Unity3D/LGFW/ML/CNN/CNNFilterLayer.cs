using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNFilter
    {
        public double[] m_weights;

        public double[] m_weightsGD;

        public CNNFilter(int weightLen)
        {
            m_weights = new double[weightLen + 1];
        }

        public double getGD(int index)
        {
            return m_weightsGD[index];
        }

        public void changeParams(int index, double value)
        {
            m_weights[index] += value;
        }

        public void randomParams(RandomKit k)
        {
            int last = m_weights.Length - 1;
            float sd = 1 / Mathf.Sqrt(m_weights.Length - 1);
            for (int i = 0; i < last; ++i)
            {
                m_weights[i] = k.randomNormalDistribution(0, sd);
            }
            m_weights[last] = k.randomNormalDistribution(0, 1);
        }

        public void applyGD(double rate)
        {
            for (int i = 0; i < m_weights.Length; ++i)
            {
                m_weights[i] -= rate * m_weightsGD[i];
            }
        }

        public void clearGD()
        {
            for (int i = 0; i < m_weights.Length; ++i)
            {
                m_weightsGD[i] = 0;
            }
        }

        public void setTrainMode(bool training)
        {
            if (training)
            {
                m_weightsGD = new double[m_weights.Length];
            }
            else
            {
                m_weightsGD = null;
            }
        }

        public List<object> toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_weights.Length; ++i)
            {
                l.Add(m_weights[i]);
            }
            return l;
        }

        public void fromJson(List<object> l)
        {
            for (int i = 0; i < m_weights.Length; ++i)
            {
                m_weights[i] = (double)l[i];
            }
        }
    }

    public class CNNFilterLayer : CNNLayer
    {

        public Vector2Int m_inputSize;
        public Vector2Int m_filterStride;
        public Vector2Int m_filterSize;
        public CNNFilter[] m_filters;
        public CNNFilterComputer m_computer;

        public CNNFilterLayer(CNNFilterLayerConfig con, Vector2Int inputSize, int inputNumber)
        {
            switch ((CNNLayerType)con.m_type)
            {
                case CNNLayerType.reluFilter:
                    m_computer = new ReluFilterComputer();
                    break;
                case CNNLayerType.sigmoidFilter:
                default:
                    m_computer = new CNNFilterComputer();
                    break;
            }
            m_filters = new CNNFilter[con.m_filterCount];
            m_inputSize = inputSize;
            m_filterStride = con.m_filterStride;
            computeFilterSize(con);
            int wn = m_filterStride.x * m_filterStride.y;
            int len = m_filterSize.x * m_filterSize.y;
            m_output = new CNNDualArray(con.m_filterCount * inputNumber, len);
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i] = new CNNFilter(wn);
            }
        }

        protected virtual void computeFilterSize(CNNFilterLayerConfig con)
        {
            m_filterSize.x = m_inputSize.x - m_filterStride.x + 1;
            m_filterSize.y = m_inputSize.y - m_filterStride.y + 1;
        }

        public override void setTrainMode(bool isTraining)
        {
            base.setTrainMode(isTraining);
            if (isTraining)
            {
                m_bpCache.initAsFilter(m_output);
            }
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].setTrainMode(isTraining);
            }
        }

        public override void randomParams(RandomKit k)
        {
            base.randomParams(k);
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].randomParams(k);
            }
            // m_filters[0].m_weights = new double[]{-1, 0, 1, 1, -1, -1, -1, 1, 1, 2};
            // m_filters[1].m_weights = new double[]{-2, 0, 1, -1, -1, -1, 1, 1, 1, 0};
        }

        protected double computeFilter(int inIndex, CNNDualArray input, CNNFilter f)
        {
            int wi = 0;
            double sum = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    sum += f.m_weights[wi] * input.m_array[inIndex + x1];
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
            return sum + f.m_weights[wi];
        }

        public override void applyGD(double rate)
        {
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].applyGD(rate);
            }
        }

        public override void clearGD()
        {
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].clearGD();
            }
        }

        public override void initBP()
        {
            for (int i = 0, len = m_bpCache.m_inToE.Length; i < len; ++i)
            {
                m_bpCache.m_inToE[i] = 0;
            }
        }

        public override void setOutToE(double[] outToE)
        {
            m_bpCache.m_midToE = outToE;
            for (int i = 0; i < m_output.m_array.Length; ++i)
            {
                m_bpCache.m_midToE[i] *= m_computer.bpMidToOut(0, i, m_bpCache);
            }
        }

        //first filter n outputs, second filter n outputs ...
        public override CNNDualArray output(CNNDualArray input)
        {
            if (m_bpCache != null)
            {
                m_bpCache.m_input = input;
            }
            int outIndex = 0;
            for (int i = 0; i < m_filters.Length; ++i)
            {
                for (int j = 0; j < input.m_number; ++j)
                {
                    int index = input.m_starts[j];
                    for (int y = 0; y < m_filterSize.y; ++y)
                    {
                        for (int x = 0; x < m_filterSize.x; ++x)
                        {
                            int inIndex = index + x;
                            double d = computeFilter(inIndex, input, m_filters[i]);
                            if (m_bpCache != null)
                            {
                                m_bpCache.m_midOutput[outIndex] = d;
                            }
                            m_output.m_array[outIndex] = m_computer.computeMidToOut(d);
                            ++outIndex;
                        }
                        index += m_inputSize.x;
                    }
                }
            }
            return m_output;
        }

        protected void computeInToE(int inIndex, int outputIndex, CNNFilter f)
        {
            int wi = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    m_bpCache.m_inToE[inIndex + x1] += f.m_weights[wi] * m_bpCache.m_midToE[outputIndex];
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
        }

        public override void bpInToE()
        {
            int outIndex = 0;
            for (int f = 0; f < m_filters.Length; ++f)
            {
                for (int i = 0; i < m_bpCache.m_input.m_number; ++i)
                {
                    int index = m_bpCache.m_input.m_starts[i];
                    for (int y = 0; y < m_filterSize.y; ++y)
                    {
                        for (int x = 0; x < m_filterSize.x; ++x)
                        {
                            computeInToE(index + x, outIndex, m_filters[f]);
                            ++outIndex;
                        }
                    }
                    index += m_inputSize.x;
                }
            }
        }

        protected void paramToE(int inIndex, int outIndex, CNNFilter f)
        {
            int wi = 0;
            for (int y1 = 0; y1 < m_filterStride.y; ++y1)
            {
                for (int x1 = 0; x1 < m_filterStride.x; ++x1)
                {
                    f.m_weightsGD[wi] += m_bpCache.m_input.m_array[inIndex + x1] * m_bpCache.m_midToE[outIndex];
                    ++wi;
                }
                inIndex += m_inputSize.x;
            }
            f.m_weightsGD[wi] += m_bpCache.m_midToE[outIndex];
        }

        public override void setBpParams()
        {
            int outIndex = 0;
            for (int f = 0; f < m_filters.Length; ++f)
            {
                for (int i = 0; i < m_bpCache.m_input.m_number; ++i)
                {
                    int index = m_bpCache.m_input.m_starts[i];
                    for (int y = 0; y < m_filterSize.y; ++y)
                    {
                        for (int x = 0; x < m_filterSize.x; ++x)
                        {
                            paramToE(index + x, outIndex, m_filters[f]);
                            ++outIndex;
                        }
                        index += m_inputSize.x;
                    }
                }
            }
        }

        public override List<object> toJson()
        {
            List<object> l = new List<object>();
            for (int i = 0; i < m_filters.Length; ++i)
            {
                l.Add(m_filters[i].toJson());
            }
            return l;
        }

        public override void fromJson(List<object> l)
        {
            for (int i = 0; i < m_filters.Length; ++i)
            {
                m_filters[i].fromJson((List<object>)l[i]);
            }
        }
    }
}