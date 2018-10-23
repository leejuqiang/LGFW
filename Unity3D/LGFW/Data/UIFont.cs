using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// This represent a font
    /// </summary>
    public class UIFont : ScriptableObject
    {

        /// <summary>
        /// The font file of this UIFont
        /// </summary>
        public Font m_font;
        /// <summary>
        /// The material this font used
        /// </summary>
        public Material m_material;

        public void resetMaterial()
        {
            if (m_material != null)
            {
                if (m_font == null)
                {
                    m_material.mainTexture = null;
                }
                else
                {
                    m_material.mainTexture = m_font.material.mainTexture;
                }
            }
        }

        public void createMaterial()
        {
            if (m_material == null)
            {
                string path = LEditorKits.getAssetDirectory(this);
                Material m = new Material(Shader.Find("LGFW/uiText"));
                if (m_font != null)
                {
                    m.mainTexture = m_font.material.mainTexture;
                }
                UnityEditor.AssetDatabase.CreateAsset(m, path + "/" + this.name + ".mat");
                m_material = m;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Asset/UIFont", false, (int)'f')]
        public static void createAnUIFont()
        {
            UIFont f = ScriptableObject.CreateInstance<UIFont>();
            LEditorKits.createAssetAtSelectedPath(f, "Select a folder for the UIFont", "font.asset");
            f.createMaterial();
        }
#endif
    }
}
