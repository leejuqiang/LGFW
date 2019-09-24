using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LGFW
{
    /// <summary>
    /// Marks this column in excel will combine all the cells of it with a specific string
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
    /// Marks this field won't be serialized into json
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class NoJson : Attribute
    {

    }

    /// <summary>
    /// Marks this field will be serialized into json using this name as the key instead of the variable name
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class JsonName : Attribute
    {
        private string m_name;

        /// <summary>
        /// The key name
        /// </summary>
        /// <value>The key name</value>
        public string Name
        {
            get { return m_name; }
        }

        public JsonName(string name)
        {
            m_name = name;
        }
    }

    /// <summary>
    /// Marks that this value will be split by a specific char into an array
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DataSplit : Attribute
    {
        private string m_split;

        /// <summary>
        /// Gets the split char array
        /// </summary>
        /// <value>The split char array</value>
        public string SplitString
        {
            get { return m_split; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataSplit"/> class. Using the default split char ","
        /// </summary>
        public DataSplit()
        {
            m_split = ",";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.DataSplit"/> class
        /// </summary>
        /// <param name="splitString">The split char array</param>
        public DataSplit(string splitString)
        {
            m_split = splitString;
        }
    }
}
