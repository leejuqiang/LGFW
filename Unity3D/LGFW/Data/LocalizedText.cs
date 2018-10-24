using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A text with a key
    /// </summary>
    [SerializeField]
    public class LocalizedTextData : StringIdData
    {
        /// <summary>
        /// The text
        /// </summary>
        public string m_text;
    }

    /// <summary>
    /// A database for LocalizedTextData, used for localization
    /// </summary>
    public class LocalizedText : DataSetBase<LocalizedTextData, string>
    {

        /// <summary>
        /// All languages this database used for
        /// </summary>
        public SystemLanguage[] m_applyForLanguages;

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

        /// <inheritdoc/>
        public override void initData()
        {
            if (m_dict == null)
            {
                base.initData();
            }
        }

        /// <summary>
        /// Gets the text by its key
        /// </summary>
        /// <returns>The text</returns>
        /// <param name="id">The key</param>
        public string getText(string id)
        {
            initData();
            if (string.IsNullOrEmpty(id))
            {
                return "";
            }
            LocalizedTextData d = null;
            if (m_dict.TryGetValue(id, out d))
            {
                return d.m_text;
            }
            return id;
        }
    }
}
