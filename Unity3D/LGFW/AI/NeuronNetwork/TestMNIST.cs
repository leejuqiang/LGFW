using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class TestMNIST : MonoBehaviour
    {
        public string m_imagePath;
        public string m_labelPath;
        public string m_testImagePath;
        public string m_testLabelPath;

        public int m_batchSize;

        private int m_dataLength;
        private int m_testDataLength;

        private byte[] m_trainImage;
        private byte[] m_trainLabel;
        private byte[] m_testImage;
        private byte[] m_testLabel;

        private NeuralNetwork m_nn;
        void Start()
        {
            readData();

            m_nn = new NeuralNetwork(784, 0.001);
            NNLayerBase l1 = new NNConvolutionLayer(1, 1, new Vector2Int(28, 28), new Vector2Int(2, 2), new Vector2Int(1, 1), true);
            // l1.setDropoutRate(0.3f);
            m_nn.addLayer(l1);
            NNLayerBase l2 = new NNSigmoidLayer();
            m_nn.addLayer(l2);
            l1 = new NNLinearLayer(10);
            // l1.setDropoutRate(0.3f);
            l2 = new NNSoftMaxLayer();
            m_nn.addLayer(l1);
            m_nn.addLayer(l2);
            m_nn.initParameter();
            NNLossBase loss = new MSELoss();
            m_nn.setLoss(loss);
            m_nn.enableTraining();
            test();
        }

        public void test()
        {
            int index = 1;
            // for (; index < m_nn.m_trainingSet[0].Length; ++index)
            // {
            //     if (m_nn.m_trainingSet[0][index] != 0)
            //     {
            //         Debug.Log("index is " + index);
            //         break;
            //     }
            // }
            List<double[]> trainSet = new List<double[]>();
            List<double[]> trainLabel = new List<double[]>();
            for (int i = 0; i < m_batchSize; ++i)
            {
                trainSet.Add(readImage(i, m_trainImage));
                trainLabel.Add(readLabel(i, m_trainLabel));
            }
            double e = m_nn.getLoss(trainSet, trainLabel);
            m_nn.Layers[0].paramOffset(index, 0.00001);
            e -= m_nn.getLoss(trainSet, trainLabel); ;
            e /= -0.00001;
            m_nn.Layers[0].paramOffset(index, -0.00001);
            m_nn.train(trainSet, trainLabel, 1);
            double e1 = m_nn.Layers[0].getGD(index);
            Debug.Log(" " + e + "   " + e1);
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

        protected void readData()
        {
            m_dataLength = 0;
            m_trainImage = System.IO.File.ReadAllBytes(m_imagePath);
            m_trainLabel = System.IO.File.ReadAllBytes(m_labelPath);
            m_dataLength |= (int)m_trainImage[7];
            m_dataLength |= ((int)m_trainImage[6]) << 8;
            m_dataLength |= ((int)m_trainImage[5]) << 16;
            m_dataLength |= ((int)m_trainImage[4]) << 24;
            m_testImage = System.IO.File.ReadAllBytes(m_testImagePath);
            m_testLabel = System.IO.File.ReadAllBytes(m_testLabelPath);
            m_testDataLength = 0;
            m_testDataLength |= (int)m_testImage[7];
            m_testDataLength |= ((int)m_testImage[6]) << 8;
            m_testDataLength |= ((int)m_testImage[5]) << 16;
            m_testDataLength |= ((int)m_testImage[4]) << 24;

            Debug.Log("finish " + m_trainImage.Length);
        }
    }
}