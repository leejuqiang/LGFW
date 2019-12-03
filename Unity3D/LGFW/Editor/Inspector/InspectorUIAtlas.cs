using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIAtlas))]
    public class InspectorUIAtlas : Editor
    {
        private UIAtlasSprite m_selectedSprite;
        void OnEnable()
        {
            m_selectedSprite = null;
        }

        void OnDisable()
        {
            m_selectedSprite = null;
            WindowSelectSprite.close();
        }

        private void onSelectSprite(object obj, MessageData data)
        {
            m_selectedSprite = (UIAtlasSprite)obj;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UIAtlas ua = (UIAtlas)target;
            if (ua != null && ua.AtlasTexture != null)
            {
                if (m_selectedSprite != null)
                {
                    EditorGUILayout.LabelField("Sprite " + m_selectedSprite.m_name + "      " + m_selectedSprite.m_pixelSize.x + "x" + m_selectedSprite.m_pixelSize.y);
                    if (GUILayout.Button("clear"))
                    {
                        m_selectedSprite = null;
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Texture size: " + ua.AtlasTexture.width + "x" + ua.AtlasTexture.height);
                }
                if (GUILayout.Button("select sprite"))
                {
                    WindowSelectSprite.showWindow(ua, onSelectSprite);
                }
            }
            if (GUILayout.Button("build"))
            {
                ua.buildAtlas();
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        private static Rect getImageRect(Rect r, float k)
        {
            float kr = r.width / r.height;
            Rect imageRc = r;
            if (k >= kr)
            {
                imageRc.height = r.width / k;
            }
            else
            {
                imageRc.width = r.height * k;
            }
            imageRc.center = r.center;
            return imageRc;
        }

        public static void guiDrawSprite(Rect r, UIAtlas ua, UIAtlasSprite uas)
        {
            float k = uas.m_pixelSize.x / uas.m_pixelSize.y;
            Rect rc = getImageRect(r, k);
            GUI.DrawTextureWithTexCoords(rc, ua.AtlasTexture, uas.m_uv);
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            base.OnPreviewGUI(r, background);
            UIAtlas ua = (UIAtlas)target;
            if (ua != null && ua.AtlasTexture != null)
            {
                if (m_selectedSprite != null)
                {
                    guiDrawSprite(r, ua, m_selectedSprite);
                }
                else
                {
                    var rc = getImageRect(r, (float)ua.AtlasTexture.width / ua.AtlasTexture.height);
                    GUI.DrawTexture(rc, ua.AtlasTexture);
                }
            }
        }
    }
}
