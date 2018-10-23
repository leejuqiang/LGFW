using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UITexture), true)]
    [CanEditMultipleObjects]
    public class InspectorUITexture : InspectorUIImage
    {

        private Texture2D m_lineTexture;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_lineTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            m_lineTexture.SetPixels(new Color[] { Color.black, Color.black, Color.black, Color.black });
        }

        void OnDisable()
        {
            m_lineTexture = null;
        }

        protected override void customUpdate()
        {
            base.customUpdate();
            UITexture ut = (UITexture)target;
            if (targets.Length < 2)
            {
                Vector4 m = ut.TrimMargin;
                if (ut.ImageType == LImageType.normal)
                {
                    EditorGUILayout.LabelField("Trim Margin");
                    ++EditorGUI.indentLevel;
                    m.x = EditorGUILayout.Slider("Left", m.x, 0, 1);
                    m.y = EditorGUILayout.Slider("Right", m.y, 0, 1);
                    m.z = EditorGUILayout.Slider("Top", m.z, 0, 1);
                    m.w = EditorGUILayout.Slider("Bottom", m.w, 0, 1);
                    --EditorGUI.indentLevel;
                    ut.setTrimMargin(m.x, m.y, m.z, m.w);
                }

                m = ut.TextureMargin;
                EditorGUILayout.LabelField("Texture Margin");
                ++EditorGUI.indentLevel;
                m.x = EditorGUILayout.FloatField("Left", m.x);
                m.y = EditorGUILayout.FloatField("Right", m.y);
                m.z = EditorGUILayout.FloatField("Top", m.z);
                m.w = EditorGUILayout.FloatField("Bottom", m.w);
                --EditorGUI.indentLevel;
                ut.setTextureMargin(m.x, m.y, m.z, m.w);
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            base.OnPreviewGUI(r, background);
            UITexture ut = (UITexture)target;
            if (ut.Render.sharedMaterial != null)
            {
                Texture t = ut.Render.sharedMaterial.mainTexture;
                if (t != null)
                {
                    float k = (float)t.width / t.height;
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
                    GUI.DrawTexture(imageRc, t);

                    Rect lineRc = new Rect(0, r.yMin, 1, r.height);
                    Vector4 m = ut.TextureMargin;
                    if (m.x > 0)
                    {
                        float x = m.x / t.width;
                        lineRc.x = imageRc.xMin + imageRc.width * x;
                        GUI.DrawTexture(lineRc, m_lineTexture);
                    }
                    if (m.y > 0)
                    {
                        float x = m.y / t.width;
                        lineRc.x = imageRc.xMin + imageRc.width * (1 - x);
                        GUI.DrawTexture(lineRc, m_lineTexture);
                    }
                    lineRc.height = 1;
                    lineRc.width = r.width;
                    lineRc.x = r.xMin;
                    if (m.z > 0)
                    {
                        float x = m.z / t.height;
                        lineRc.y = imageRc.yMin + imageRc.height * x;
                        GUI.DrawTexture(lineRc, m_lineTexture);
                    }
                    if (m.w > 0)
                    {
                        float x = m.w / t.height;
                        lineRc.y = imageRc.yMin + imageRc.height * (1 - x);
                        GUI.DrawTexture(lineRc, m_lineTexture);
                    }
                }
            }
        }
    }
}
