using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UISprite))]
    [CanEditMultipleObjects]
    public class InspectorUISprite : InspectorUIImage
    {

        protected string m_selectSpriteName;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_selectSpriteName = null;
        }

        void OnDisable()
        {
            m_selectSpriteName = null;
            WindowSelectSprite.close();
        }

        private void onSelectSprite(object obj, MessageData data)
        {
            UIAtlasSprite uas = (UIAtlasSprite)obj;
            m_selectSpriteName = uas.m_name;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets.Length < 2)
            {
                UISprite us = (UISprite)target;
                if (us.ImageType == LImageType.slice)
                {
                    if (GUILayout.Button("snap slice"))
                    {
                        us.snapSliceMargin();
                        us.reset();
                    }
                }
            }
        }

        protected override void customUpdate()
        {
            base.customUpdate();
            if (targets.Length < 2)
            {
                UISprite us = (UISprite)target;
                if (us.AtlasSprite != null)
                {
                    EditorGUILayout.LabelField("Sprite " + us.AtlasSprite.m_name + "      " + us.AtlasSprite.m_originalSize.x + "x" + us.AtlasSprite.m_originalSize.y);
                }
                if (us.Atlas != null && GUILayout.Button("sprite"))
                {
                    WindowSelectSprite.showWindow(us.Atlas, onSelectSprite);
                }
                if (!string.IsNullOrEmpty(m_selectSpriteName))
                {
                    us.Sprite = m_selectSpriteName;
                    m_selectSpriteName = null;
                }
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            base.OnPreviewGUI(r, background);
            UISprite s = (UISprite)target;
            if (s.AtlasSprite != null && s.Atlas != null && s.Atlas.m_material != null && s.Atlas.m_material.mainTexture != null)
            {
                InspectorUIAtlas.guiDrawSprite(r, s.Atlas, s.AtlasSprite);
            }
        }
    }
}
