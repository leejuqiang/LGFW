using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LGFW
{
    /// <summary>
    /// Add this script to a Text. This script automatically set the localized text using the text id
    /// </summary>
    public class UILocalizedText : BaseMono
    {
        /// <summary>
        /// The text id
        /// </summary>
        public string m_textId;

        private Text m_text;

        protected override void doAwake()
        {
            m_text = this.gameObject.GetComponent<Text>();
            applyText();
        }

        public void applyText()
        {
            Awake();
            if (m_text != null && !string.IsNullOrEmpty(m_textId))
            {
                m_text.text = Localization.getString(m_textId);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/LocalizedText", false, (int)'l')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UILocalizedText>(true);
        }
#endif
    }
}