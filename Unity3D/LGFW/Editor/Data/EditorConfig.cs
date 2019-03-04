using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [System.Serializable]
    public class ExcelConfig
    {
        public string m_dataPath;
        public string m_assetPath;
        public string m_scriptPath;
        public bool m_isLocalizedText;

        public string m_customDBExtension;

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
        public ExcelConfig[] m_excelData;

        public ExcelConfig getDataConfig(string path)
        {
            if (m_excelData == null)
            {
                return null;
            }
            for (int i = 0; i < m_excelData.Length; ++i)
            {
                if (m_excelData[i].m_dataPath == path)
                {
                    return m_excelData[i];
                }
            }
            return null;
        }

        [MenuItem("LGFW/Editor/Config", false, (int)'c')]
        public static void selectConfig()
        {
            Selection.activeObject = EditorConfig.Instance;
        }

        public static Dictionary<string, object> getTempConfig()
        {
            string path = "temp.json";
            Dictionary<string, object> dict = null;
            if (LGFWKit.fileExists(path))
            {
                string js = LGFWKit.readTextFromFile(path);
                dict = (Dictionary<string, object>)Json.decode(js);
            }
            if (dict == null)
            {
                dict = new Dictionary<string, object>();
            }
            return dict;
        }

        public static void saveTempConfig(Dictionary<string, object> dict)
        {
            string js = Json.encode(dict, true);
            LGFWKit.writeTextToFile("temp.json", js);
        }
    }
}
