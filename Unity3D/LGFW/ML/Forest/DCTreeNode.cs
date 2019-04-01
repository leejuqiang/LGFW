using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class DCSplitResult
    {
        public Dictionary<double, List<DCTreeData>> m_dict;
        public DCAttribute m_attr;
        public int m_valueIndex;
    }

    /// <summary>
    /// A node of a decision tree
    /// </summary>
    public class DCTreeNode
    {
        public int m_dimension;
        public int m_depth;
        public double m_threshold;
        public DCAttributeType m_attributeType;

        public delegate double GetSplitScore(Dictionary<double, List<DCTreeData>> count, int total);

        public Dictionary<double, DCTreeNode> m_children;
        public double m_label;
        public bool m_isLeaf;

        public DCTreeNode(int depth)
        {
            m_depth = depth;
            m_isLeaf = false;
        }

        public DCTreeNode output(double[] data)
        {
            DCTreeNode n = this;
            while (!n.m_isLeaf)
            {
                double d = data[n.m_dimension];
                if (n.m_attributeType == DCAttributeType.split)
                {
                    if (d <= n.m_threshold)
                    {
                        n = n.m_children[0];
                    }
                    else
                    {
                        n = n.m_children[1];
                    }
                }
                else
                {
                    if (!n.m_children.TryGetValue(d, out n))
                    {
                        return null;
                    }
                }
            }
            return n;
        }

        public Dictionary<string, object> toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["d"] = m_dimension;
            dict["t"] = m_threshold;
            dict["a"] = (int)m_attributeType;
            dict["h"] = m_depth;
            if (m_isLeaf)
            {
                dict["l"] = m_label;
            }
            else
            {
                Dictionary<string, object> c = new Dictionary<string, object>();
                foreach (double k in m_children.Keys)
                {
                    c[k + ""] = m_children[k].toJson();
                }
                dict["ch"] = c;
            }
            return dict;
        }

        public static DCTreeNode fromJson(Dictionary<string, object> dict)
        {
            DCTreeNode n = new DCTreeNode(0);
            n.m_dimension = (int)dict["d"];
            n.m_threshold = (double)dict["t"];
            n.m_attributeType = (DCAttributeType)(int)dict["a"];
            n.m_depth = (int)dict["h"];
            if (dict.ContainsKey("l"))
            {
                n.m_isLeaf = true;
            }
            else
            {
                n.m_isLeaf = false;
                n.m_children = new Dictionary<double, DCTreeNode>();
                Dictionary<string, object> c = (Dictionary<string, object>)dict["ch"];
                foreach (string k in dict.Keys)
                {
                    double d = System.Convert.ToDouble(k);
                    n.m_children[d] = fromJson((Dictionary<string, object>)dict[k]);
                }
            }
            return n;
        }

        public void setLabel(Dictionary<int, int> count)
        {
            m_isLeaf = true;
            int max = 0;
            int maxIndex = -1;
            foreach (int k in count.Keys)
            {
                int v = count[k];
                if (v > max)
                {
                    maxIndex = k;
                    max = v;
                }
            }
            m_label = maxIndex;
        }

        private static Dictionary<int, int> getLabelCount(List<DCTreeData> data)
        {
            Dictionary<int, int> ret = new Dictionary<int, int>();
            for (int i = 0; i < data.Count; ++i)
            {
                int n = 0;
                if (!ret.TryGetValue(data[i].m_label, out n))
                {
                    n = 1;
                }
                else
                {
                    ++n;
                }
                ret[data[i].m_label] = n;
            }
            return ret;
        }

        private static Dictionary<double, List<DCTreeData>> splitData(List<DCTreeData> data, DCAttribute attr, int valueIndex)
        {
            Dictionary<double, List<DCTreeData>> ret = new Dictionary<double, List<DCTreeData>>();
            int d = attr.m_dimension;
            if (attr.m_type == DCAttributeType.match)
            {
                for (int i = 0; i < data.Count; ++i)
                {
                    List<DCTreeData> l = null;
                    if (!ret.TryGetValue(data[i].m_data[d], out l))
                    {
                        l = new List<DCTreeData>();
                        ret[data[i].m_data[d]] = l;
                    }
                    l.Add(data[i]);
                }
            }
            else
            {
                double t = attr.m_values[valueIndex];
                List<DCTreeData> small = new List<DCTreeData>();
                List<DCTreeData> big = new List<DCTreeData>();
                for (int i = 0; i < data.Count; ++i)
                {
                    if (data[i].m_data[d] <= t)
                    {
                        small.Add(data[i]);
                    }
                    else
                    {
                        big.Add(data[i]);
                    }
                }
                ret[0] = small;
                ret[1] = big;
            }
            return ret;
        }

        private static DCSplitResult splitData(List<DCTreeData> data, List<DCAttribute> attrs, GetSplitScore getScore)
        {
            double minScore = 0;
            DCAttribute minAttr = null;
            int valueIndex = 0;
            Dictionary<double, List<DCTreeData>> minDict = null;
            for (int i = 0; i < attrs.Count; ++i)
            {
                if (attrs[i].m_type == DCAttributeType.match)
                {
                    Dictionary<double, List<DCTreeData>> dict = splitData(data, attrs[i], 0);
                    double s = getScore(dict, data.Count);
                    if (minAttr == null || s < minScore)
                    {
                        minScore = s;
                        minAttr = attrs[i];
                        minDict = dict;
                    }
                }
                else
                {
                    for (int j = 0; j < attrs[i].m_values.Length; ++j)
                    {
                        Dictionary<double, List<DCTreeData>> dict = splitData(data, attrs[i], j);
                        double s = getScore(dict, data.Count);
                        if (minAttr == null || s < minScore)
                        {
                            minScore = s;
                            minAttr = attrs[i];
                            valueIndex = j;
                            minDict = dict;
                        }
                    }
                }
            }
            DCSplitResult r = new DCSplitResult();
            r.m_attr = minAttr;
            r.m_dict = minDict;
            r.m_valueIndex = valueIndex;
            return r;
        }

        private static void train(DCTreeNode n, int maxDepth, List<DCTreeData> data, List<DCAttribute> attrs, GetSplitScore getScore)
        {
            Dictionary<int, int> count = getLabelCount(data);
            if (n.m_depth >= maxDepth || count.Count <= 1 || attrs.Count <= 0)
            {
                n.setLabel(count);
                return;
            }
            DCSplitResult r = splitData(data, attrs, getScore);
            if (r.m_dict.Count <= 1)
            {
                n.setLabel(count);
                return;
            }
            n.m_dimension = r.m_attr.m_dimension;
            n.m_attributeType = r.m_attr.m_type;
            if (n.m_attributeType == DCAttributeType.split)
            {
                n.m_threshold = r.m_attr.m_values[r.m_valueIndex];
            }
            n.m_children = new Dictionary<double, DCTreeNode>();
            foreach (double k in r.m_dict.Keys)
            {
                List<DCTreeData> l = r.m_dict[k];
                DCTreeNode nn = new DCTreeNode(n.m_depth + 1);
                n.m_children[k] = nn;
                List<DCAttribute> newA = new List<DCAttribute>();
                for (int i = 0; i < attrs.Count; ++i)
                {
                    if (attrs[i] != r.m_attr)
                    {
                        if (attrs[i].m_type == DCAttributeType.match)
                        {
                            newA.Add(attrs[i]);
                        }
                        else
                        {
                            newA.Add(attrs[i].copy());
                        }
                    }
                    else if (attrs[i].m_type == DCAttributeType.split)
                    {
                        DCAttribute a = null;
                        if (k < 1)
                        {
                            a = attrs[i].copy(0, r.m_valueIndex);

                        }
                        else
                        {
                            a = attrs[i].copy(r.m_valueIndex + 1, attrs[i].m_values.Length);
                        }
                        if (a.m_values.Length > 0)
                        {
                            newA.Add(a);
                        }
                    }
                }
                train(nn, maxDepth, l, newA, getScore);
            }
        }

        public static DCTreeNode train(int maxDepth, List<DCTreeData> data, List<DCAttribute> attrs, GetSplitScore getScore)
        {
            DCTreeNode root = new DCTreeNode(1);
            train(root, maxDepth, data, attrs, getScore);
            return root;
        }

        /// <summary>
        /// The gini implement of the delegate GetSplitScore
        /// </summary>
        /// <param name="count">The label lists</param>
        /// <param name="total">The total number of data</param>
        /// <returns>The score</returns>
        public static double giniScore(Dictionary<double, List<DCTreeData>> count, int total)
        {
            double d = 0;
            foreach (List<DCTreeData> l in count.Values)
            {
                double f = l.Count / total;
                d += f * (1 - f);
            }
            return d;
        }

        /// <summary>
        /// The entropy implement of the delegate GetSplitScore
        /// </summary>
        /// <param name="count">The label lists</param>
        /// <param name="total">The total number of data</param>
        /// <returns>The score</returns>
        public static double entropyScore(Dictionary<double, List<DCTreeData>> count, int total)
        {
            double d = 0;
            foreach (List<DCTreeData> l in count.Values)
            {
                double f = l.Count / total;
                d -= f * System.Math.Log(f, 2);
            }
            return d;
        }
    }
}