using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIAtlas))]
    public class InspectorUIAtlas : Editor
    {

        private UIAtlasSprite m_editSprite;
        private Texture2D m_lineTexture;

        void OnEnable()
        {
            m_editSprite = null;
            m_lineTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            m_lineTexture.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black });
        }

        void OnDisable()
        {
            m_editSprite = null;
            m_lineTexture = null;
            WindowSelectSprite.close();
        }

        private void onSelectSprite(object obj, MessageData data)
        {
            m_editSprite = (UIAtlasSprite)obj;
        }

        private float marginToPixel(float v, float textureSize)
        {
            return v * textureSize;
        }

        private float pixelToMargin(float v, float textureSize)
        {
            return v / textureSize;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UIAtlas ua = (UIAtlas)target;
            if (m_editSprite != null && ua != null && ua.m_material != null && ua.m_material.mainTexture != null)
            {
                EditorGUILayout.LabelField("Sprite " + m_editSprite.m_name + "      " + m_editSprite.m_originalSize.x + "x" + m_editSprite.m_originalSize.y);
                EditorGUILayout.LabelField("Slice Margin");
                ++EditorGUI.indentLevel;
                Texture tex = ua.m_material.mainTexture;
                Vector4 v = m_editSprite.m_sliceMargin;
                v.x = pixelToMargin(EditorGUILayout.FloatField("Left", marginToPixel(v.x, tex.width)), tex.width);
                v.y = pixelToMargin(EditorGUILayout.FloatField("Right", marginToPixel(v.y, tex.width)), tex.width);
                v.z = pixelToMargin(EditorGUILayout.FloatField("Top", marginToPixel(v.z, tex.height)), tex.height);
                v.w = pixelToMargin(EditorGUILayout.FloatField("Bottom", marginToPixel(v.w, tex.height)), tex.height);
                --EditorGUI.indentLevel;
                m_editSprite.setSliceMargin(v);
            }

            if (GUILayout.Button("edit sprite"))
            {
                WindowSelectSprite.showWindow(ua, onSelectSprite);
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

        public static Rect guiDrawSprite(Rect r, UIAtlas ua, UIAtlasSprite uas)
        {
            float w = uas.m_uv.width * ua.m_material.mainTexture.width;
            float h = uas.m_uv.height * ua.m_material.mainTexture.height;
            w /= 1 - uas.m_trimMargin.x - uas.m_trimMargin.y;
            h /= 1 - uas.m_trimMargin.z - uas.m_trimMargin.w;
            float k = w / h;
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
            Rect rc = imageRc;
            rc.xMin += imageRc.width * uas.m_trimMargin.x;
            rc.xMax -= imageRc.width * uas.m_trimMargin.y;
            rc.yMin += imageRc.height * uas.m_trimMargin.z;
            rc.yMax -= imageRc.height * uas.m_trimMargin.w;
            GUI.DrawTextureWithTexCoords(rc, ua.m_material.mainTexture, uas.m_uv);
            return imageRc;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            base.OnPreviewGUI(r, background);
            UIAtlas ua = (UIAtlas)target;
            if (m_editSprite != null && ua != null && ua.m_material != null && ua.m_material.mainTexture != null)
            {
                Rect imageRc = guiDrawSprite(r, ua, m_editSprite);

                Rect lineRc = new Rect(0, r.yMin, 1, r.height);
                if (m_editSprite.m_sliceMargin.x > 0)
                {
                    float x = m_editSprite.m_sliceMargin.x / m_editSprite.m_uv.width;
                    lineRc.x = imageRc.xMin + imageRc.width * x;
                    GUI.DrawTexture(lineRc, m_lineTexture);
                }
                if (m_editSprite.m_sliceMargin.y > 0)
                {
                    float x = m_editSprite.m_sliceMargin.y / m_editSprite.m_uv.width;
                    lineRc.x = imageRc.xMin + imageRc.width * (1 - x);
                    GUI.DrawTexture(lineRc, m_lineTexture);
                }
                lineRc.height = 1;
                lineRc.width = r.width;
                lineRc.x = r.xMin;
                if (m_editSprite.m_sliceMargin.z > 0)
                {
                    float x = m_editSprite.m_sliceMargin.z / m_editSprite.m_uv.height;
                    lineRc.y = imageRc.yMin + imageRc.height * x;
                    GUI.DrawTexture(lineRc, m_lineTexture);
                }
                if (m_editSprite.m_sliceMargin.w > 0)
                {
                    float x = m_editSprite.m_sliceMargin.w / m_editSprite.m_uv.height;
                    lineRc.y = imageRc.yMin + imageRc.height * (1 - x);
                    GUI.DrawTexture(lineRc, m_lineTexture);
                }
            }
        }
    }
}
