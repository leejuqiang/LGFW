using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [System.Serializable]
    public class JsonConfig
    {
        public ScriptableObject m_asset;
        public bool m_enumAsNumber;
        public bool m_useFormat;
    }

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

    [System.Serializable]
    public class TextureImporterConfig
    {
        public TextureImporterType m_textureType = TextureImporterType.Default;
        public TextureImporterShape m_textureShape = TextureImporterShape.Texture2D;
        public TextureImporterAlphaSource m_alphaSource = TextureImporterAlphaSource.FromInput;
        public bool m_readable = false;
        public bool m_sRGB = false;
        public bool m_alphaIsTransparency = true;
        public bool m_enableMipMap = false;
        public TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        public FilterMode m_filter = FilterMode.Bilinear;
        public TextureImporterCompression m_compression = TextureImporterCompression.Uncompressed;
        public int m_maxTextureSize = 2048;
        public SpriteImportMode m_spriteMode = SpriteImportMode.None;
        public TextureImporterNPOTScale m_npot = TextureImporterNPOTScale.None;

        public float m_pixelPerUnit = 1;
        public SpriteMeshType m_spriteMeshType = SpriteMeshType.FullRect;
        public bool m_enableSpritePhysicsShape = false;
        public bool m_enable;
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

        public TextureImporterConfig m_defaultTextureImporter;

        public string m_dataFieldPrefix = "m_";
        public ExcelConfig[] m_excelData;
        public JsonConfig[] m_jsonExport;

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

        [MenuItem("LGFW/Editor/Export Json Configuration", false, (int)'c')]
        public static void exportJson()
        {
            string path = LEditorKits.openSaveToFolderPanel("Select a folder");
            if (!string.IsNullOrEmpty(path))
            {
                EditorConfig ec = EditorConfig.Instance;
                foreach (JsonConfig jc in ec.m_jsonExport)
                {
                    if (jc.m_asset != null)
                    {
                        string js = Json.objectToJson(jc.m_asset, jc.m_enumAsNumber, jc.m_useFormat);
                        string savePath = path + "/" + jc.m_asset.name + ".json";
                        LGFWKit.writeTextToFile(savePath, js);
                    }
                }
            }
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
