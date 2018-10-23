using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIButton))]
    public class InspectorUIButton : Editor
    {

        private void onSelectNormalSprite(object obj, MessageData data)
        {
            UIButton ub = (UIButton)target;
            UIAtlasSprite s = (UIAtlasSprite)obj;
            ub.setSpriteForState(UIButtonState.normal, s.m_name);
        }

        private void onSelectPressSprite(object obj, MessageData data)
        {
            UIButton ub = (UIButton)target;
            UIAtlasSprite s = (UIAtlasSprite)obj;
            ub.setSpriteForState(UIButtonState.pressed, s.m_name);
        }

        private void onSelectDisableSprite(object obj, MessageData data)
        {
            UIButton ub = (UIButton)target;
            UIAtlasSprite s = (UIAtlasSprite)obj;
            ub.setSpriteForState(UIButtonState.disable, s.m_name);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets.Length < 2)
            {
                UIButton ub = (UIButton)target;
                ub.Sprite = (UISprite)EditorGUILayout.ObjectField("Sprite", ub.Sprite, typeof(UISprite), true);
                if (ub.Sprite != null)
                {
                    if (ub.Sprite.Atlas != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Normal Sprite " + ub.getSpriteForState(UIButtonState.normal));
                        if (GUILayout.Button("Select"))
                        {
                            WindowSelectSprite.showWindow(ub.Sprite.Atlas, onSelectNormalSprite);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Pressed Sprite " + ub.getSpriteForState(UIButtonState.pressed));
                        if (GUILayout.Button("Select"))
                        {
                            WindowSelectSprite.showWindow(ub.Sprite.Atlas, onSelectPressSprite);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Disable Sprite " + ub.getSpriteForState(UIButtonState.disable));
                        if (GUILayout.Button("Select"))
                        {
                            WindowSelectSprite.showWindow(ub.Sprite.Atlas, onSelectDisableSprite);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    Color c = EditorGUILayout.ColorField("Normal Color", ub.getColorForState(UIButtonState.normal));
                    ub.setColorForState(UIButtonState.normal, c);
                    c = EditorGUILayout.ColorField("Pressed Color", ub.getColorForState(UIButtonState.pressed));
                    ub.setColorForState(UIButtonState.pressed, c);
                    c = EditorGUILayout.ColorField("Disable Color", ub.getColorForState(UIButtonState.disable));
                    ub.setColorForState(UIButtonState.disable, c);
                }
            }
        }
    }
}
