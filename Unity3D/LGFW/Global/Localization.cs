using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Language using mode
    /// </summary>
    public enum LanguageMode
    {
        /// <summary>
        /// The default language
        /// </summary>
        useDefault,
        /// <summary>
        /// Use system language
        /// </summary>
        useSystem,
        /// <summary>
        /// Use the language selected by player
        /// </summary>
        usePlayerDefine,
    }

    /// <summary>
    /// Localization Manager
    /// </summary>
    public class Localization : MonoBehaviour
    {

        private static Localization m_instance;

        /// <summary>
        /// Gets the singleton of Localization
        /// </summary>
        /// <value>The singleton</value>
        public static Localization Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// Default language
        /// </summary>
        public SystemLanguage m_defaultLanguage = SystemLanguage.English;
        /// <summary>
        /// Language using mode.
        /// </summary>
        public LanguageMode m_languageMode = LanguageMode.useSystem;
        /// <summary>
        /// Player selected language will be saved in PlayerPrefs of this key
        /// </summary>
        public string m_playerDefineSaveKey = "language";
        public LocalizedText[] m_localizeTexts;

        private SystemLanguage m_currentLanguage;
        private List<LocalizedText> m_currentTexts = new List<LocalizedText>();

        private Dictionary<string, string> m_texts = new Dictionary<string, string>();

        /// <summary>
        /// Gets the current language
        /// </summary>
        /// <value>The current language</value>
        public SystemLanguage CurrentLanguage
        {
            get
            {
                return m_currentLanguage;
            }
        }

        /// <summary>
        /// Sets the player selected language
        /// </summary>
        /// <param name="language">The language</param>
        public void setPlayerDefineLanguage(SystemLanguage language)
        {
            if (string.IsNullOrEmpty(m_playerDefineSaveKey))
            {
                return;
            }
            PlayerPrefs.SetInt(m_playerDefineSaveKey, (int)language);
            PlayerPrefs.Save();
        }

        public void initLanguage()
        {
            if (m_languageMode == LanguageMode.useDefault)
            {
                m_currentLanguage = m_defaultLanguage;
            }
            else if (m_languageMode == LanguageMode.usePlayerDefine)
            {
                if (PlayerPrefs.HasKey(m_playerDefineSaveKey))
                {
                    m_currentLanguage = (SystemLanguage)PlayerPrefs.GetInt(m_playerDefineSaveKey);
                }
                else
                {
                    m_currentLanguage = Application.systemLanguage;
                }
            }
            else
            {
                m_currentLanguage = Application.systemLanguage;
            }
            getTextDataByLang();
            if (m_currentTexts.Count <= 0)
            {
                m_currentLanguage = m_defaultLanguage;
                getTextDataByLang();
            }
            if (m_currentTexts.Count <= 0 && m_localizeTexts.Length > 0)
            {
                m_currentTexts.Add(m_localizeTexts[0]);
            }
            m_texts.Clear();
            for (int i = 0; i < m_currentTexts.Count; ++i)
            {
                m_currentTexts[i].initData(m_texts);
            }
        }

        private void getTextDataByLang()
        {
            m_currentTexts.Clear();
            for (int i = 0; i < m_localizeTexts.Length; ++i)
            {
                if (m_localizeTexts[i].applyForLanguage(m_currentLanguage))
                {
                    m_currentTexts.Add(m_localizeTexts[i]);
                    break;
                }
            }
        }

        void Awake()
        {
            m_instance = this;
            initLanguage();
        }

        void OnDestroy()
        {
            m_instance = null;
        }

        /// <summary>
        /// Gets the localized text by its key
        /// </summary>
        /// <returns>The text, if not found, return the id itself</returns>
        /// <param name="id">The key</param>
        public static string getString(string id)
        {
            if (Localization.Instance != null && Localization.Instance.m_currentTexts.Count > 0)
            {
                string t = null;
                if (Instance.m_texts.TryGetValue(id, out t))
                {
                    return t;
                }
                return id;
            }
            return id;
        }

        /// <summary>
        /// Gets a string formated using the localized string
        /// </summary>
        /// <param name="id">The id of the string</param>
        /// <param name="args">The arguments used to format the string</param>
        /// <returns>The formated string</returns>
        public static string getFormatedString(string id, params object[] args)
        {
            string s = getString(id);
            return string.Format(s, args);
        }
    }
}
