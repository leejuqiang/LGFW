using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MNISTBase : MonoBehaviour
    {

        public string m_imagePath;
        public string m_labelPath;
        public string m_testImagePath;
        public string m_testLabelPath;
        public int m_totalTrainDataSize = 10000;
        public int m_trainingSetSize = 15;
        public float m_learningRate = 5;
        public int m_trainingTimes = 10;

        protected byte[] m_images;
        protected byte[] m_labels;
        protected byte[] m_testImages;
        protected byte[] m_testLabels;
        protected int m_dataLength;
        protected int m_testDataLength;

        protected List<int> m_trainDataIndexes = new List<int>();
        protected int m_currentTrainDataIndex;

        public void Awake()
        {
            readData();
        }

        protected double[] readImage(int index, byte[] data)
        {
            index = 784 * index + 16;
            double[] ret = new double[784];
            for (int i = 0; i < ret.Length; ++i, ++index)
            {
                ret[i] = (double)data[index] / 255.0;
            }
            return ret;
        }

        protected int readLabelNumber(int index, byte[] data)
        {
            return (int)data[index + 8];
        }

        protected double[] readLabel(int index, byte[] data)
        {
            double[] ret = new double[10];
            ret[readLabelNumber(index, data)] = 1;
            return ret;
        }

        protected void setTrainingData(NNBase nn)
        {
            nn.m_trainingSet.Clear();
            nn.m_trainingResultSet.Clear();
            for (int i = 0; i < m_trainingSetSize; ++i, ++m_currentTrainDataIndex)
            {
                if (m_currentTrainDataIndex >= m_trainDataIndexes.Count)
                {
                    return;
                }
                int j = m_trainDataIndexes[m_currentTrainDataIndex];
                nn.m_trainingSet.Add(readImage(j, m_images));
                nn.m_trainingResultSet.Add(readLabel(j, m_labels));
            }
        }

        protected void readData()
        {
            m_dataLength = 0;
            m_images = System.IO.File.ReadAllBytes(m_imagePath);
            m_labels = System.IO.File.ReadAllBytes(m_labelPath);
            m_dataLength |= (int)m_images[7];
            m_dataLength |= ((int)m_images[6]) << 8;
            m_dataLength |= ((int)m_images[5]) << 16;
            m_dataLength |= ((int)m_images[4]) << 24;
            m_testImages = System.IO.File.ReadAllBytes(m_testImagePath);
            m_testLabels = System.IO.File.ReadAllBytes(m_testLabelPath);
            m_testDataLength = 0;
            m_testDataLength |= (int)m_testImages[7];
            m_testDataLength |= ((int)m_testImages[6]) << 8;
            m_testDataLength |= ((int)m_testImages[5]) << 16;
            m_testDataLength |= ((int)m_testImages[4]) << 24;

            int[] indexes = new int[m_dataLength];
            for (int i = 0; i < m_dataLength; ++i)
            {
                indexes[i] = i;
            }
            LMath.shuffleArray<int>(indexes);
            for (int i = 0; i < m_totalTrainDataSize; ++i)
            {
                m_trainDataIndexes.Add(indexes[i]);
            }
            m_currentTrainDataIndex = 0;
            Debug.Log("finish " + m_images.Length);
        }

        protected int checkResult(double[] result)
        {
            int ret = 0;
            double f = System.Math.Abs(1 - result[0]);
            for (int i = 1; i < result.Length; ++i)
            {
                double t = System.Math.Abs(1 - result[i]);
                if (t < f)
                {
                    ret = i;
                    f = t;
                }
            }
            return ret;
        }
        public void testData(NNBase nn)
        {
            int count = 0;
            for (int i = 0; i < m_testDataLength; ++i)
            {
                double[] input = readImage(i, m_testImages);
                nn.m_inputs = input;
                double[] result = nn.output();
                if (checkResult(result) == readLabelNumber(i, m_testLabels))
                {
                    ++count;
                }
            }
            double r = (double)count / m_testDataLength;
            Debug.Log("correct " + count + " , total " + m_testDataLength + " rate " + r);
        }
    }
}
