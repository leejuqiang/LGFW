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
        binary,
        split,
        equal,
        notEqual,
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
        public int m_index;

        /// <summary>
        /// Delegate for classify data method
        /// </summary>
        /// <param name="data">The data</param>
        /// <param name="threshold">The threshold value</param>
        /// <returns>True if the data belongs to the first child</returns>
        public delegate bool ClassifyData(DCTreeData data, double threshold);

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <param name="index">The index of the value</param>
        /// <returns>The value</returns>
        public double getValue(int index)
        {
            if (m_type == DCAttributeType.binary)
            {
                return 0.5;
            }
            return m_values[index];
        }

        /// <summary>
        /// Gets the number of the values
        /// </summary>
        /// <returns>The count</returns>
        public int getValueCount()
        {
            return m_type == DCAttributeType.binary ? 1 : m_values.Length;
        }

        /// <summary>
        /// Gets the classify method of this attribute
        /// </summary>
        /// <returns>The delegate of the method</returns>
        public ClassifyData getClassifyMethod()
        {
            switch (m_type)
            {
                case DCAttributeType.binary:
                    return classifyBinary;
                case DCAttributeType.split:
                    return classifySplit;
                case DCAttributeType.equal:
                    return classifyEqual;
                case DCAttributeType.notEqual:
                    return classifyNotEqual;
                default:
                    return null;
            }
        }

        private bool classifyBinary(DCTreeData data, double threshold)
        {
            return data.m_data[m_dimension] < 0.5;
        }

        private bool classifySplit(DCTreeData data, double threshold)
        {
            return data.m_data[m_dimension] < threshold;
        }

        private bool classifyEqual(DCTreeData data, double threshold)
        {
            return data.m_data[m_dimension] == threshold;
        }

        private bool classifyNotEqual(DCTreeData data, double threshold)
        {
            return data.m_data[m_dimension] != threshold;
        }

        public DCAttribute copy(double value, bool smallerValue)
        {
            DCAttribute att = new DCAttribute();
            att.m_type = m_type;
            att.m_dimension = m_dimension;
            List<double> l = new List<double>();
            if (m_type == DCAttributeType.equal || m_type == DCAttributeType.notEqual)
            {
                for (int i = 0; i < m_values.Length; ++i)
                {
                    if (m_values[i] != value)
                    {
                        l.Add(m_values[i]);
                    }
                }
            }
            else if (m_type != DCAttributeType.binary)
            {
                if (smallerValue)
                {
                    for (int i = 0; m_values[i] < value; ++i)
                    {
                        l.Add(m_values[i]);
                    }
                }
                else
                {
                    for (int i = m_values.Length - 1; m_values[i] > value; --i)
                    {
                        l.Add(m_values[i]);
                    }
                }
            }
            att.m_values = l.ToArray();
            att.m_index = m_index;
            return att;
        }

        /// <summary>
        /// Reduces the values' length
        /// </summary>
        /// <param name="maxLength">The length after reduction</param>
        public void trimValues(int maxLength)
        {
            if (m_values.Length <= maxLength + 1)
            {
                return;
            }
            float step = (float)m_values.Length / (maxLength + 1);
            double[] v = new double[maxLength];
            float index = step;
            for (int i = 0; i < maxLength; ++i, index += step)
            {
                v[i] = m_values[(int)index];
            }
        }

        /// <summary>
        /// Gets a split type attribute based on the data list
        /// </summary>
        /// <param name="dimension">The dimension of this attribute</param>
        /// <param name="data">The data list</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getSplitAttribute(int dimension, List<DCTreeData> data)
        {
            DCAttribute att = new DCAttribute();
            att.m_type = DCAttributeType.split;
            att.m_dimension = dimension;
            HashSet<double> set = new HashSet<double>();
            for (int i = 0; i < data.Count; ++i)
            {
                set.Add(data[i].m_data[dimension]);
            }
            List<double> l = new List<double>(set);
            l.Sort();
            att.m_values = new double[l.Count - 1];
            for (int i = 1; i < l.Count; ++i)
            {
                int last = i - 1;
                att.m_values[last] = (l[last] + l[i]) * 0.5;
            }
            return att;
        }

        /// <summary>
        /// Gets a binary type attribute
        /// </summary>
        /// <param name="dimension">The dimension</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getBinaryAttribute(int dimension)
        {
            DCAttribute att = new DCAttribute();
            att.m_dimension = dimension;
            att.m_type = DCAttributeType.binary;
            return att;
        }

        /// <summary>
        /// Gets a equal or not equal type attribute based on a data list
        /// </summary>
        /// <param name="dimension">The dimension</param>
        /// <param name="equal">If the type is equal or not</param>
        /// <param name="data">The data list</param>
        /// <returns>The attribute</returns>
        public static DCAttribute getEqualOrNotAttribute(int dimension, bool equal, List<DCTreeData> data)
        {
            DCAttribute att = new DCAttribute();
            att.m_dimension = dimension;
            att.m_type = equal ? DCAttributeType.equal : DCAttributeType.notEqual;
            HashSet<double> set = new HashSet<double>();
            for (int i = 0; i < data.Count; ++i)
            {
                set.Add(data[i].m_data[dimension]);
            }
            att.m_values = new double[set.Count];
            int index = 0;
            foreach (double d in set)
            {
                att.m_values[index] = d;
                ++index;
            }
            return att;
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