using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The type of the attribute
    /// </summary>
    public enum DCAttributeType
    {
        match,
        split,
    }

    /// <summary>
    /// The attribute of data for decision trees
    /// </summary>
    public class DCAttribute
    {
        /// <summary>
        /// If this attribute is binary
        /// </summary>
        public DCAttributeType m_type;
        /// <summary>
        /// The dimension in the data of this attribute
        /// </summary>
        public int m_dimension;
        /// <summary>
        /// The possible values for the attribute
        /// </summary>
        public double[] m_values;

        public DCAttribute copy()
        {
            return copy(0, m_values.Length);
        }

        public DCAttribute copy(int from, int to)
        {
            DCAttribute att = new DCAttribute();
            att.m_type = m_type;
            att.m_dimension = m_dimension;
            att.m_values = new double[to - from];
            for (int i = 0; i < att.m_values.Length; ++i, ++from)
            {
                att.m_values[i] = m_values[from];
            }
            return att;
        }

        /// <summary>
        /// Creates a split attribute from data
        /// </summary>
        /// <param name="l">The data</param>
        /// <param name="dimension">The dimension of this attribute</param>
        /// <param name="step">The step of the values</param>
        /// <param name="maxLength">The max length of the values</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getSplitAttribute(List<DCTreeData> l, int dimension, double step, int maxLength)
        {
            DCAttribute ret = new DCAttribute();
            ret.m_dimension = dimension;
            ret.m_type = DCAttributeType.split;
            double min = l[0].m_data[dimension];
            double max = l[0].m_data[dimension];
            for (int i = 1; i < l.Count; ++i)
            {
                double d = l[i].m_data[dimension];
                if (d < min)
                {
                    min = d;
                }
                else if (d > max)
                {
                    max = d;
                }
            }
            int len = (int)((max - min) / step);
            len = Mathf.Min(len, maxLength);
            len = Mathf.Max(1, len);
            step = (max - min) / (len + 1);
            ret.m_values = new double[len];
            double v = step;
            for (int i = 0; i < len; ++i, v += step)
            {
                ret.m_values[i] = v;
            }
            return ret;
        }

        /// <summary>
        /// Creates a binary attribute
        /// </summary>
        /// <param name="dimension">The dimension of the attribute</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getBinaryAttribute(int dimension)
        {
            DCAttribute ret = new DCAttribute();
            ret.m_dimension = dimension;
            ret.m_type = DCAttributeType.split;
            ret.m_values = new double[2];
            ret.m_values[0] = 0;
            ret.m_values[1] = 1;
            return ret;
        }

        /// <summary>
        /// Creates a n-ary attribute for the data
        /// </summary>
        /// <param name="l">The data</param>
        /// <param name="dimension">The dimension of the attribute</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getMatchAttribute(List<DCTreeData> l, int dimension)
        {
            DCAttribute ret = new DCAttribute();
            ret.m_dimension = dimension;
            ret.m_type = DCAttributeType.match;
            HashSet<double> s = new HashSet<double>();
            for (int i = 0; i < l.Count; ++i)
            {
                s.Add(l[i].m_data[dimension]);
            }
            ret.m_values = new double[s.Count];
            int index = 0;
            foreach (double d in s)
            {
                ret.m_values[index] = d;
                ++index;
            }
            return ret;
        }
    }

    /// <summary>
    /// A data used for a decision tree
    /// </summary>
    public class DCTreeData
    {
        /// <summary>
        /// The values of the data
        /// </summary>
        public double[] m_data;
        /// <summary>
        /// The label of the data
        /// </summary>
        public int m_label;

    }
}