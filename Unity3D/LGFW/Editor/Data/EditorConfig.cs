using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [System.Serializable]
    public class JsonDataConfig
    {
        public string m_dataPath;
        public string m_assetPath;

        public string[] m_keys;
        public string[] m_alias;

        public string getAlias(string key)
        {
            for (int i = 0; i < m_keys.Length; ++i)
            {
                if (key == m_keys[i])
                {
                    return m_alias[i];
                }
            }
            return key;
        }
    }

    public class EditorConfig : ScriptableObject
    {

        public static EditorConfig Instance
        {
            get
            {
                EditorConfig ec = AssetDatabase.LoadAssetAtPath<EditorConfig>("Assets/Editor/LGFWConfig.asset");
                if (ec == null)
                {
                    ec = ScriptableObject.CreateInstance<EditorConfig>();
                    AssetDatabase.CreateAsset(ec, "Assets/Editor/LGFWConfig.asset");
                }
                return ec;
            }
        }

        public string m_dataFieldPrefix = "m_";
        public JsonDataConfig[] m_jsonData;
        public string[] m_processExcels;

        public JsonDataConfig getDataConfig(string path)
        {
            if (m_jsonData == null)
            {
                return null;
            }
            for (int i = 0; i < m_jsonData.Length; ++i)
            {
                if (m_jsonData[i].m_dataPath == path)
                {
                    return m_jsonData[i];
                }
            }
            return null;
        }

        public bool isExcelData(string path)
        {
            if (m_processExcels == null)
            {
                return false;
            }
            for (int i = 0; i < m_processExcels.Length; ++i)
            {
                if (m_processExcels[i] == path)
                {
                    return true;
                }
            }
            return false;
        }

        [MenuItem("LGFW/Editor/Config", false, (int)'c')]
        public static void selectConfig()
        {
            Selection.activeObject = EditorConfig.Instance;
        }
    }
}
