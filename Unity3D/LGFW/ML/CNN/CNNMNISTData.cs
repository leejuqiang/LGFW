using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CNNMNISTData : MNISTBase
    {
        public double m_cnnLambda;
        public bool m_dropout;
        private ConvolutionalNN m_nn;

        void Start()
        {
            CNNFilterLayerConfig c1 = new CNNFilterLayerConfig(CNNLayerType.reluFilter, 3, new Vector2Int(4, 4));
            // c1.m_moveStep.Set(2, 2);
            // c1.m_stayInside = false;
            CNNPoolLayerConfig c2 = new CNNPoolLayerConfig(CNNLayerType.maxPooling, new Vector2Int(2, 2));
            NNlayerConfig[] lcs = new NNlayerConfig[2];
            lcs[0] = new NNlayerConfig(NNLayerType.sigmoid, 10, 0.5f);
            lcs[1] = new NNlayerConfig(NNLayerType.sigmoid, 10);
            m_nn = new ConvolutionalNN(new Vector2Int(24, 24), m_dropout, lcs, c1, c2);
            m_nn.m_regularizationLambda = m_lambda;
            m_nn.m_cnnLambda = m_cnnLambda;

            // CNNFilterLayerConfig c1 = new CNNFilterLayerConfig(CNNLayerType.sigmoidFilter, 2, new Vector2Int(3, 3));
            // CNNPoolLayerConfig c2 = new CNNPoolLayerConfig(CNNLayerType.maxPooling, new Vector2Int(2, 2));
            // m_nn = new ConvolutionalNN(NNLayerType.sigmoid, 10, new Vector2Int(5, 5), c1, c2);

            m_nn.randomParams();
            m_nn.setTrainingMode(true);
        }

        public void test()
        {
            setTrainingData(m_nn);
            // m_nn.m_trainingSet.Clear();
            // m_nn.m_trainingSet.Add(new double[]{0, 1, 3, 4, 5, 1, -1, 2, 3, 4, 1, 1, 3, 4, 5, -1, -2, -3, 0, 0, 5, 4, 3, 2,1});
            m_nn.testDropout(0, 16);
            m_nn.testDropout(0, 5);
        }

        private void trainOnce()
        {
            setTrainingData(m_nn);
            for (int i = 0; i < m_trainingTimes; ++i)
            {
                m_nn.gradientDescent(m_learningRate);
            }
        }

        public void train()
        {
            float t = Time.realtimeSinceStartup;
            while (m_currentTrainDataIndex < m_trainDataIndexes.Count)
            {
                trainOnce();
            }
            float t1 = Time.realtimeSinceStartup;
            Debug.Log("finish train in " + (t1 - t));
            // string s = m_nn.toJson();
            // System.IO.File.WriteAllText("Assets/np.txt", s);
        }

        public void testResult()
        {
            testData(m_nn);
        }
    }
}
