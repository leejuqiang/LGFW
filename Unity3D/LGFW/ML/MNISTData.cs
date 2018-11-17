using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MNISTData : MNISTBase
    {

        public NNlayerConfig[] m_configs;
        public bool m_dropout;
        public UITexture m_texture;
        private NeuralNetwork m_nn;

        // Use this for initialization
        void Start()
        {
            m_nn = new NeuralNetwork(784, m_dropout, m_configs);
            m_nn.randomAllLayersParam();
            //m_layers [2].randomlyInitParams (-1, 1);
            m_nn.setTrainingMode(true);
            m_nn.m_costType = NNCostType.likelihood;
            m_nn.m_regularizationLambda = m_lambda;
            m_nn.m_totalTrainingData = m_totalTrainDataSize;
        }

        private void trainOnce()
        {
            setTrainingData(m_nn);
            for (int i = 0; i < m_trainingTimes; ++i)
            {
                m_nn.gradientDescent(m_learningRate);
            }
        }

        public void test()
        {
            setTrainingData(m_nn);
            m_nn.testDropout();
            // m_nn.testDerivative(index);
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
            string s = m_nn.toJson();
            System.IO.File.WriteAllText("Assets/np.txt", s);
        }

        public void loadParam()
        {
            Debug.Log(m_nn.m_layers[0].value(0, 0));

            string s = System.IO.File.ReadAllText("Assets/np.txt");
            m_nn.initWithJson(s);
            Debug.Log(m_nn.m_layers[0].value(0, 0));
        }


        public void testImage(int index)
        {
            double[] im = readImage(index, m_images);
            Texture2D tex = new Texture2D(28, 28, TextureFormat.ARGB32, false);
            //Color[] cols = new Color[im.Length];

            int k = 0;
            for (int y = 27; y >= 0; --y)
            {
                for (int x = 0; x < 28; ++x)
                {
                    Color c = Color.white;
                    c.a = 1;
                    c.r = (float)im[k];
                    c.b = c.r;
                    c.g = c.r;
                    tex.SetPixel(x, y, c);
                    ++k;
                }
            }
            //			for (int i = 0; i < im.Length; ++i) {
            //				int j = im.Length - i - 1;
            //				cols [j].r = (float)im [i];
            //				cols [j].g = cols [j].r;
            //				cols [j].b = cols [j].r;
            //				cols [j].a = 1;
            //			}
            //tex.SetPixels (cols);
            tex.Apply();
            m_texture.MainTexture = tex;
            Debug.Log(readLabelNumber(index, m_labels));
        }

        public void testResult()
        {
            testData(m_nn);
        }
    }
}
