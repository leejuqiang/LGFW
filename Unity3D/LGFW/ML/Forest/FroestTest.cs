using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class TitanicData : DCTreeData
    {
        public string m_name;

        public void init(string[] arr)
        {
            m_name = arr[2];
            m_label = System.Convert.ToInt32(arr[0]);
            m_data = new double[6];
            m_data[0] = System.Convert.ToInt32(arr[1]); //class
            m_data[1] = arr[3] == "male" ? 1 : 0;//sex
            m_data[2] = System.Convert.ToDouble(arr[4]); //age
            m_data[3] = System.Convert.ToDouble(arr[5]);//sibling aboard
            m_data[4] = System.Convert.ToDouble(arr[6]);//parent aboard
            m_data[5] = System.Convert.ToDouble(arr[7]); //fare
        }
    }
    public class FroestTest : MonoBehaviour
    {

        public string m_dataPath;
        public int m_testSize;
        public int m_treeNum;
        public int m_subsetNum;
        public int m_attributeForATree;
        public double m_maxError;

        private List<DCTreeData> m_data;
        private List<DCTreeData> m_testData;
        private List<DCAttribute> m_attributes;

        private System.IO.StreamReader m_reader;

        private DCTreeNode m_tree;
        private RandomForest m_forest;

        void Awake()
        {
            m_reader = new System.IO.StreamReader(m_dataPath);
            m_reader.ReadLine();
        }

        private void initAttr()
        {
            m_attributes = new List<DCAttribute>();
            DCAttribute att = DCAttribute.getMatchAttribute(m_data, 0);//class
            m_attributes.Add(att);
            att = DCAttribute.getMatchAttribute(m_data, 1);//sex
            m_attributes.Add(att);
            att = DCAttribute.getSplitAttribute(m_data, 2, 1, 10);//age
            m_attributes.Add(att);
            att = DCAttribute.getMatchAttribute(m_data, 3);//sibling aboard
            m_attributes.Add(att);
            att = DCAttribute.getMatchAttribute(m_data, 4);//parent aboard
            m_attributes.Add(att);
            att = DCAttribute.getSplitAttribute(m_data, 5, 0.1, 10);//fare
            m_attributes.Add(att);
        }

        private bool readData()
        {
            string line = m_reader.ReadLine();
            if (line == null)
            {
                return false;
            }
            TitanicData data = new TitanicData();
            string[] arr = line.Split(',');
            data.init(arr);
            m_data.Add(data);
            return true;
        }

        private void initData()
        {
            m_data = new List<DCTreeData>();
            while (readData())
            {
            }
            m_testData = new List<DCTreeData>();
            LMath.shuffleList<DCTreeData>(m_data);
            for (int i = 0; i < m_testSize; ++i)
            {
                int last = m_data.Count - 1;
                m_testData.Add(m_data[last]);
                m_data.RemoveAt(last);
            }
        }

        public void trainTree()
        {
            initData();
            float t = Time.realtimeSinceStartup;
            initAttr();
            m_tree = DCTreeNode.train(5, m_data, m_attributes, DCTreeNode.entropyScore);
            Dictionary<string, object> dict = m_tree.toJson();
            string js = Json.encode(dict, true);
            LGFWKit.writeTextToFile("Assets/tree.json", js);
            t = Time.realtimeSinceStartup - t;
            Debug.Log("finish train " + t);
            float count = 0;
            for (int i = 0; i < m_testData.Count; ++i)
            {
                DCTreeNode n = m_tree.output(m_testData[i].m_data);
                if (n != null && (int)n.m_label == m_testData[i].m_label)
                {
                    ++count;
                }
            }
            Debug.Log("test " + (count / m_testData.Count * 100));
        }

        public void trainForest()
        {
            initData();
            initAttr();
            float t = Time.realtimeSinceStartup;
            m_forest = new RandomForest(m_treeNum, m_subsetNum, m_attributeForATree, 5);
            m_forest.m_data = m_data;
            m_forest.train(m_attributes, DCTreeNode.entropyScore);
            t = Time.realtimeSinceStartup - t;
            Debug.Log("finish " + t);
            string js = m_forest.toJson();
            LGFWKit.writeTextToFile("Assets/forest.json", js);
            float count = 0;
            for (int i = 0; i < m_testData.Count; ++i)
            {
                m_forest.output(m_testData[i].m_data);
                double l = m_forest.getLabelFromOutput();
                if ((int)l == m_testData[i].m_label)
                {
                    ++count;
                }
            }
            Debug.Log("test " + (count / m_testData.Count * 100));
        }
    }
}