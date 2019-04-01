using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A random forest
    /// </summary>
    public class RandomForest
    {
        /// <summary>
        /// The training data
        /// </summary>
        public List<DCTreeData> m_data;

        /// <summary>
        /// The number of decision trees
        /// </summary>
        public int m_treeNumber;
        /// <summary>
        /// The size of data for one tree
        /// </summary>
        public int m_subsetNumber;
        /// <summary>
        /// The nubmer of attributes for one tree
        /// </summary>
        public int m_attributeNumber;
        /// <summary>
        /// The max depth for a tree
        /// </summary>
        public int m_treeDepth;
        private DCTreeNode[] m_trees;
        private DCTreeNode[] m_output;

        public RandomForest(int treeNumber, int dataSetNumber, int attributeNumber, int treeDepth)
        {
            m_treeNumber = treeNumber;
            m_subsetNumber = dataSetNumber;
            m_treeDepth = treeDepth;
            m_attributeNumber = attributeNumber;
            m_trees = new DCTreeNode[m_treeNumber];
            m_output = new DCTreeNode[m_treeNumber];
        }

        private List<DCTreeData> getSubset()
        {
            List<DCTreeData> l = new List<DCTreeData>();
            for (int i = 0; i < m_subsetNumber; ++i)
            {
                int index = Random.Range(0, m_data.Count);
                l.Add(m_data[index]);
            }
            return l;
        }

        private List<DCAttribute> getSubAtt(List<DCAttribute> l)
        {
            List<DCAttribute> ret = new List<DCAttribute>();
            LMath.shuffleList<DCAttribute>(l);
            for (int i = 0; i < m_attributeNumber; ++i)
            {
                if (l[i].m_type == DCAttributeType.split)
                {
                    ret.Add(l[i].copy());
                }
                else
                {
                    ret.Add(l[i]);
                }
            }
            return ret;
        }

        /// <summary>
        /// Trains the forest using the attributes
        /// </summary>
        /// <param name="attributes"></param>
        public void train(List<DCAttribute> attributes, DCTreeNode.GetSplitScore getScore)
        {
            for (int i = 0; i < m_trees.Length; ++i)
            {
                List<DCTreeData> set = getSubset();
                m_trees[i] = DCTreeNode.train(m_treeDepth, set, getSubAtt(attributes), getScore);
            }
        }

        /// <summary>
        /// Outputs the result of this forest
        /// </summary>
        /// <param name="data">The input data</param>
        /// <returns>The array of result for each tree</returns>
        public DCTreeNode[] output(double[] data)
        {
            for (int i = 0; i < m_trees.Length; ++i)
            {
                m_output[i] = m_trees[i].output(data);
            }
            return m_output;
        }

        /// <summary>
        /// Gets a label from the output
        /// </summary>
        /// <returns>The label</returns>
        public double getLabelFromOutput()
        {
            Dictionary<double, int> count = new Dictionary<double, int>();
            for (int i = 0; i < m_output.Length; ++i)
            {
                if (m_output[i] != null)
                {
                    int n = 0;
                    if (!count.TryGetValue(m_output[i].m_label, out n))
                    {
                        n = 1;
                    }
                    else
                    {
                        ++n;
                    }
                    count[m_output[i].m_label] = n;
                }
            }
            int maxCount = 0;
            double maxK = 0;
            foreach (double k in count.Keys)
            {
                int v = count[k];
                if (v > maxCount)
                {
                    maxCount = v;
                    maxK = k;
                }
            }
            return maxK;
        }

        /// <summary>
        /// Converts the forest to json string
        /// </summary>
        /// <returns>The json string</returns>
        public string toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["treeNum"] = m_treeNumber;
            dict["setNum"] = m_subsetNumber;
            dict["attNum"] = m_attributeNumber;
            dict["depth"] = m_treeDepth;
            List<object> l = new List<object>();
            dict["tree"] = l;
            for (int i = 0; i < m_trees.Length; ++i)
            {
                l.Add(m_trees[i].toJson());
            }
            return Json.encode(dict, true);
        }

        /// <summary>
        /// Initializes the forest with a json string
        /// </summary>
        /// <param name="json">The json string</param>
        public void fromJson(string json)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)Json.decode(json);
            m_treeNumber = (int)dict["treeNum"];
            m_subsetNumber = (int)dict["setNum"];
            m_attributeNumber = (int)dict["attNum"];
            m_treeDepth = (int)dict["depth"];
            List<object> l = (List<object>)dict["tree"];
            m_trees = new DCTreeNode[l.Count];
            m_output = new DCTreeNode[l.Count];
            for (int i = 0; i < m_trees.Length; ++i)
            {
                m_trees[i] = DCTreeNode.fromJson((Dictionary<string, object>)l[i]);
            }
        }
    }
}
