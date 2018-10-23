using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A safe int type used for anti-memory cheating 
    /// </summary>
    public class SafeInt
    {

        private int m_value;
        private int m_defaultValue;
        private int m_offset;
        private int m_check;

        /// <summary>
        /// Gets or sets the value of the int
        /// </summary>
        /// <value>The value</value>
        public int Value
        {
            get
            {
                if (m_check == getCheckValue())
                {
                    return m_value + m_offset;
                }
                return m_defaultValue;
            }

            set
            {
                m_value = value - m_offset;
                m_check = getCheckValue();
            }
        }

        private int getCheckValue()
        {
            return ~m_value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.SafeInt"/> class.
        /// </summary>
        /// <param name="value">The real value of the int</param>
        /// <param name="defaultValue">If found this int has been hacked, then the value will be this default value</param>
        /// <param name="offset">The offset of real value and the value saved in memory, this is used for anti-batch creation hacking</param>
        public SafeInt(int value, int defaultValue, int offset)
        {
            m_value = value - offset;
            m_defaultValue = defaultValue;
            m_offset = offset;
            m_check = getCheckValue();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.SafeInt"/> class, the list should be from toList function
        /// </summary>
        /// <param name="l">A int list</param>
        public SafeInt(List<int> l)
        {
            fromList(l);
        }

        /// <summary>
        /// Set the value by a int list, the list should be from toList function
        /// </summary>
        /// <param name="l">The list</param>
        public void fromList(List<int> l)
        {
            if (l == null || l.Count < 4)
            {
                m_value = 0;
                m_defaultValue = 0;
                m_offset = 0;
                m_check = getCheckValue();
            }
            else
            {
                m_value = l[0];
                m_check = l[1];
                m_defaultValue = l[2];
                m_offset = l[3];
            }
        }

        /// <summary>
        /// Gets a int list represent this SafeInt
        /// </summary>
        /// <returns>The list</returns>
        public List<int> toList()
        {
            List<int> l = new List<int>();
            l.Add(m_value);
            l.Add(m_check);
            l.Add(m_defaultValue);
            l.Add(m_offset);
            return l;
        }
    }
}
