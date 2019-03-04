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
        /// The type of the trees
        /// </summary>
        public DCTreeType m_type;
        /// <summary>
        /// The tolerable error for the leaf of a tree, if it's 0.1, means if >=90% of the data in a node 
        /// has the same label, then the node is a leaf 
        /// </summary>
        public double m_maxError;

        private DCTreeNodeBase[] m_trees;
        private DCTreeNodeBase[] m_output;

        public RandomForest(int treeNumber, int dataSetNumber, int attributeNumber, DCTreeType type)
        {
            m_treeNumber = treeNumber;
            m_subsetNumber = dataSetNumber;
            m_attributeNumber = attributeNumber;
            m_trees = new DCTreeNodeBase[m_treeNumber];
            m_output = new DCTreeNodeBase[m_treeNumber];
            m_type = type;
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

        private DCAttribute[] getSubAtt(DCAttribute[] l)
        {
            DCAttribute[] ret = new DCAttribute[m_attributeNumber];
            LMath.shuffleArray<DCAttribute>(l);
            for (int i = 0; i < m_attributeNumber; ++i)
            {
                ret[i] = l[i];
            }
            return ret;
        }

        /// <summary>
        /// Trains the forest using the attributes
        /// </summary>
        /// <param name="attributes"></param>
        public void train(DCAttribute[] attributes)
        {
            Queue<DCTreeNodeBase> q = new Queue<DCTreeNodeBase>();
            for (int i = 0; i < m_trees.Length; ++i)
            {
                DCTreeNodeBase n = DCTreeNodeBase.createNode(m_type, 0, m_maxError);
                List<DCTreeData> set = getSubset();
                DCTreeNodeBase.train(set, getSubAtt(attributes), n, q);
                m_trees[i] = n;
            }
        }

        /// <summary>
        /// Outputs the result of this forest
        /// </summary>
        /// <param name="data">The input data</param>
        /// <returns>The array of result for each tree</returns>
        public DCTreeNodeBase[] output(DCTreeData data)
        {
            for (int i = 0; i < m_trees.Length; ++i)
            {
                DCTreeNodeBase n = (DCTreeNodeBase)m_trees[i].getLeaf(data);
                m_output[i] = n;
            }
            return m_output;
        }

        /// <summary>
        /// Gets the average of all output, call this after calling output
        /// </summary>
        /// <returns>The average</returns>
        public double getAverageOfOutput()
        {
            double c = 0;
            for (int i = 0; i < m_output.Length; ++i)
            {
                c += m_output[i].Label;
            }
            return c / m_output.Length;
        }

        /// <summary>
        /// Gets the most frequent output, call this after calling output
        /// </summary>
        /// <returns>The label</returns>
        public double getLabelOfOutput()
        {
            Dictionary<double, double> count = new Dictionary<double, double>();
            for (int i = 0; i < m_output.Length; ++i)
            {
                double c = 0;
                if (!count.TryGetValue(m_output[i].Label, out c))
                {
                    c = 0;
                }
                count[m_output[i].Label] = c + m_output[i].Weight;
            }
            double max = -1;
            double l = 0;
            foreach (double d in count.Keys)
            {
                double c = count[d];
                if (c > max)
                {
                    max = c;
                    l = d;
                }
            }
            return l;
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
            dict["error"] = m_maxError;
            dict["type"] = (int)m_type;
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
            m_maxError = (double)dict["error"];
            m_subsetNumber = (int)dict["setNum"];
            m_attributeNumber = (int)dict["attNum"];
            m_type = (DCTreeType)(int)dict["type"];
            List<object> l = (List<object>)dict["tree"];
            m_trees = new DCTreeNodeBase[l.Count];
            for (int i = 0; i < m_trees.Length; ++i)
            {
                m_trees[i] = DCTreeNodeBase.fromJson((Dictionary<string, object>)l[i]);
            }
        }
    }
}
