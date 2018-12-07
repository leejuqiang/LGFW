using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Display for the value of a json key
    /// </summary>
    public class JsonTreeDisplay : MonoBehaviour
    {
        public string m_value;
    }

    /// <summary>
    /// The json key configuration
    /// </summary>
    [System.Serializable]
    public class JsonTreeKey
    {
        /// <summary>
        /// The name of the key
        /// </summary>
        public string m_key;
        /// <summary>
        /// If hide this key
        /// </summary>
        public bool m_hide;
        /// <summary>
        /// If display the value of this key in the name of the GameObject
        /// </summary>
        public bool m_displayValueInName;
    }
}