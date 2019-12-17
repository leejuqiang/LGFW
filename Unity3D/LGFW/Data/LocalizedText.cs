using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A text with a key
    /// </summary>
    [System.Serializable]
    public class LocalizedTextData : StringIdData
    {
        [DataCombineText]
        /// <summary>
        /// The text
        /// </summary>
        public string m_text;
    }

    /// <summary>
    /// A database for LocalizedTextData, used for localization
    /// </summary>
    public class LocalizedText : ScriptableObject
    {

        /// <summary>
        /// All languages this database used for
        /// </summary>
        public SystemLanguage[] m_applyForLanguages;

        public List<LocalizedTextData> m_dataList;

        /// <summary>
        /// Check if this database can be use for a language
        /// </summary>
        /// <returns>Return true if can</returns>
        /// <param name="l">The language</param>
        public bool applyForLanguage(SystemLanguage l)
        {
            for (int i = 0; i < m_applyForLanguages.Length; ++i)
            {
                if (l == m_applyForLanguages[i])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes the text Id and the text into a map
        /// </summary>
        /// <param name="dict">The map</param>
        public void initData(Dictionary<string, string> dict)
        {
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                dict.Add(m_dataList[i].m_id, m_dataList[i].m_text);
            }
        }
    }
}
