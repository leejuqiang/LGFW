using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum DCTreeType
    {
        gini,
    }

    /// <summary>
    /// A node of a decision tree
    /// </summary>
    public abstract class DCTreeNodeBase : DecisionTreeNode<DCTreeData>
    {
        public int m_dimension;
        public double m_threshold;

        public double m_maxError;

        public DCAttribute[] m_attributes;

        public List<DCTreeData> m_dataList;

        protected List<DCTreeData>[] m_segments;
        protected DCAttribute m_attribute;

        protected DCTreeType m_type;
        protected bool m_isLeaf;
        protected double m_label;
        protected double m_weight;
        protected Dictionary<int, int>[] m_labelCounts;

        public override bool IsLeaf
        {
            get
            {
                return m_isLeaf;
            }
        }

        /// <summary>
        /// The label of this node
        /// </summary>
        /// <value>The label</value>
        public double Label
        {
            get { return m_label; }
        }

        /// <summary>
        /// The probability of this node's label
        /// </summary>
        /// <value>The probability</value>
        public double Weight
        {
            get { return m_weight; }
        }

        public DCTreeNodeBase(int depth, double maxError, DCTreeType type) : base(depth)
        {
            m_segments = new List<DCTreeData>[2];
            m_type = type;
            m_maxError = maxError;
            m_labelCounts = new Dictionary<int, int>[2];
            m_labelCounts[0] = new Dictionary<int, int>();
            m_labelCounts[1] = new Dictionary<int, int>();
        }

        public static DCTreeNodeBase createNode(DCTreeType t, int depth, double maxError)
        {
            switch (t)
            {
                case DCTreeType.gini:
                default:
                    return new DCNodeGini(depth, maxError);
            }
        }

        protected int maxLabel(List<DCTreeData> l, Dictionary<int, int> countSet, out int maxCount)
        {
            countLabel(l, countSet);
            maxCount = -1;
            int label = 0;
            foreach (int la in countSet.Keys)
            {
                int c = countSet[la];
                if (c > maxCount)
                {
                    label = la;
                    maxCount = c;
                }
            }
            return label;
        }

        protected void countLabel(List<DCTreeData> l, Dictionary<int, int> countSet)
        {
            countSet.Clear();
            for (int i = 0; i < l.Count; ++i)
            {
                int la = l[i].m_label;
                int c = 0;
                if (!countSet.TryGetValue(la, out c))
                {
                    c = 0;
                }
                countSet[la] = c + 1;
            }
        }

        protected void initLabel()
        {
            int max = 0;
            m_label = maxLabel(m_dataList, m_labelCounts[0], out max);
            m_weight = (double)max / m_dataList.Count;
        }

        public void initData(List<DCTreeData> l, DCAttribute[] attributes)
        {
            m_dataList = l;
            m_attributes = attributes;
            m_segments[0] = new List<DCTreeData>(l.Count);
            m_segments[1] = new List<DCTreeData>(l.Count);
            initLabel();
            m_isLeaf = 1 - m_weight <= m_maxError;
        }

        public virtual void decideParam()
        {
            double min = 0;
            m_attribute = null;
            bool hasAtt = false;
            for (int i = 0; i < m_attributes.Length; ++i)
            {

                DCAttribute att = m_attributes[i];
                DCAttribute.ClassifyData cd = att.getClassifyMethod();
                int len = att.getValueCount();
                for (int j = 0; j < len; ++j)
                {
                    double v = att.getValue(j);
                    splitData(att.m_dimension, v, cd);
                    if (m_segments[0].Count <= 0 || m_segments[1].Count <= 0)
                    {
                        continue;
                    }
                    hasAtt = true;
                    double e = getError(att.m_dimension, v);
                    if (m_attribute == null || e < min)
                    {
                        m_attribute = att;
                        m_attribute.m_index = i;
                        m_threshold = v;
                    }
                }
            }
            if (!hasAtt)
            {
                m_isLeaf = true;
            }
            else
            {
                m_dimension = m_attribute.m_dimension;
            }
        }

        public void initChildren()
        {
            int d = m_depth + 1;
            DCTreeNodeBase n1 = createNode(m_type, d, m_maxError);
            m_children.Add(n1);
            DCTreeNodeBase n2 = createNode(m_type, d, m_maxError);
            m_children.Add(n2);
            splitData(m_dimension, m_threshold, m_attribute.getClassifyMethod());
            DCAttribute[] att1 = null;
            DCAttribute[] att2 = null;
            switch (m_attribute.m_type)
            {
                case DCAttributeType.binary:
                    {
                        att1 = new DCAttribute[m_attributes.Length - 1];
                        att2 = new DCAttribute[att1.Length];
                        for (int i = 0, j = 0; i < m_attributes.Length; ++i)
                        {
                            if (m_attributes[i] != m_attribute)
                            {
                                att1[j] = m_attributes[i];
                                att2[j] = m_attributes[i];
                                ++j;
                            }
                        }
                    }
                    break;
                case DCAttributeType.split:
                    {
                        DCAttribute att = m_attribute.copy(m_threshold, true);
                        m_attributes[att.m_index] = att;
                        att1 = new DCAttribute[m_attributes.Length];
                        System.Array.Copy(m_attributes, att1, att1.Length);
                        att2 = new DCAttribute[m_attributes.Length];
                        att = m_attribute.copy(m_threshold, false);
                        m_attributes[att.m_index] = att;
                        System.Array.Copy(m_attributes, att2, att2.Length);
                    }
                    break;
                case DCAttributeType.equal:
                case DCAttributeType.notEqual:
                    {
                        DCAttribute att = m_attribute.copy(m_threshold, true);
                        m_attributes[att.m_index] = att;
                        att1 = new DCAttribute[m_attributes.Length];
                        att2 = new DCAttribute[m_attributes.Length];
                        System.Array.Copy(m_attributes, att1, att1.Length);
                        System.Array.Copy(m_attributes, att2, att1.Length);
                    }
                    break;
                default:
                    break;
            }
            n1.initData(m_segments[0], att1);
            n2.initData(m_segments[1], att2);
        }

        public static void train(List<DCTreeData> data, DCAttribute[] attributes, DCTreeNodeBase node, Queue<DCTreeNodeBase> queue)
        {
            queue.Clear();
            queue.Enqueue(node);
            node.initData(data, attributes);
            while (queue.Count > 0)
            {
                DCTreeNodeBase n = queue.Dequeue();
                if (!n.IsLeaf)
                {
                    n.decideParam();
                }
                if (!n.IsLeaf)
                {
                    n.initChildren();
                    queue.Enqueue((DCTreeNodeBase)n.m_children[0]);
                    queue.Enqueue((DCTreeNodeBase)n.m_children[1]);
                }
                else
                {
                    n.m_data = new HashSet<DCTreeData>(n.m_dataList);
                }
            }
        }

        protected virtual void splitData(int dimension, double threshold, DCAttribute.ClassifyData del)
        {
            m_segments[0].Clear();
            m_segments[1].Clear();
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                if (del(m_dataList[i], m_threshold))
                {
                    m_segments[0].Add(m_dataList[i]);
                }
                else
                {
                    m_segments[1].Add(m_dataList[i]);
                }
            }
        }

        protected abstract double getError(int dimension, double threshold);

        public override DecisionTreeNode<DCTreeData> getLeaf(DCTreeData data)
        {
            DCTreeNodeBase n = this;
            while (!n.IsLeaf)
            {
                bool c = n.m_attribute.getClassifyMethod()(data, m_threshold);
                n = (DCTreeNodeBase)(c ? n.m_children[0] : n.m_children[1]);
            }
            return n;
        }

        public Dictionary<string, object> toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["type"] = (int)m_type;
            dict["maxError"] = m_maxError;
            dict["depth"] = m_depth;
            if (!IsLeaf)
            {
                dict["dimension"] = m_dimension;
                dict["threshold"] = m_threshold;
                List<object> l = new List<object>();
                l.Add(((DCTreeNodeBase)m_children[0]).toJson());
                l.Add(((DCTreeNodeBase)m_children[1]).toJson());
                dict["children"] = l;
            }
            else
            {
                dict["label"] = m_label;
                dict["weight"] = m_weight;
            }
            return dict;
        }

        public static DCTreeNodeBase fromJson(Dictionary<string, object> dict)
        {
            DCTreeType t = (DCTreeType)(int)dict["type"];
            int depth = (int)dict["depth"];
            double maxError = (double)dict["maxError"];
            DCTreeNodeBase n = createNode(t, depth, maxError);
            object o = null;
            if (dict.TryGetValue("children", out o))
            {
                n.m_isLeaf = false;
                n.m_dimension = (int)dict["dimension"];
                n.m_threshold = (double)dict["threshold"];
                List<object> l = (List<object>)o;
                n.m_children.Add(fromJson((Dictionary<string, object>)l[0]));
                n.m_children.Add(fromJson((Dictionary<string, object>)l[1]));
            }
            else
            {
                n.m_isLeaf = true;
                n.m_label = (double)dict["label"];
                n.m_weight = (double)dict["weight"];
            }
            return n;
        }

        protected void mergeLeaf(DCTreeNodeBase n1, DCTreeNodeBase n2)
        {
            if (n1.m_parent != n2.m_parent)
            {
                return;
            }
            if (n1.m_label != n2.m_label)
            {
                return;
            }
            DCTreeNodeBase p = (DCTreeNodeBase)n1.m_parent;
            p.m_isLeaf = true;
            p.m_data = new HashSet<DCTreeData>(p.m_dataList);
            p.initLabel();
        }
    }
}