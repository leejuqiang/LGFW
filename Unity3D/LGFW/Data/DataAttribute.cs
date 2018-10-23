using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LGFW
{
    /// <summary>
    /// Mark this column in excel will combine all the cells of it with a specific string
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DataCombineText : Attribute
    {
        private string m_combineText;

        /// <summary>
        /// Gets the string used to combine cells
        /// </summary>
        /// <value>The string</value>
        public string CombineText
        {
            get { return m_combineText; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataCombineText"/> class. The combine string will be ""
        /// </summary>
        public DataCombineText()
        {
            m_combineText = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataCombineText"/> class.
        /// </summary>
        /// <param name="combinText">The combine string</param>
        public DataCombineText(string combinText)
        {
            m_combineText = CombineText;
        }
    }

    /// <summary>
    /// Mark that this value will be split by a specific char into an array
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DataSplit : Attribute
    {
        private char m_split;

        /// <summary>
        /// Gets the split char
        /// </summary>
        /// <value>The split char</value>
        public char SplitChar
        {
            get { return m_split; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataSplit"/> class. Using the default split char ","
        /// </summary>
        public DataSplit()
        {
            m_split = ',';
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataSplit"/> class
        /// </summary>
        /// <param name="splitChar">The split char</param>
        public DataSplit(char splitChar)
        {
            m_split = splitChar;
        }
    }
}
