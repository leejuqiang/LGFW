using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Cone))]
    [CanEditMultipleObjects]
    public class InspectorCone : Editor
    {

        private void onSelectTop(object o, MessageData data)
        {
            UIAtlasSprite s = (UIAtlasSprite)o;
            for (int i = 0; i < targets.Length; ++i)
            {
                Cone c = (Cone)targets[i];
                c.TopSprite = s.m_name;
            }
        }

        private void onSelectBottom(object o, MessageData data)
        {
            UIAtlasSprite s = (UIAtlasSprite)o;
            for (int i = 0; i < targets.Length; ++i)
            {
                Cone c = (Cone)targets[i];
                c.BottomSprite = s.m_name;
            }
        }

        public override void OnInspectorGUI()
        {
            Cone c = (Cone)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            float h = EditorGUILayout.FloatField("Height", c.Height);
            if (h == 0)
            {
                h = c.Height;
            }
            for (int i = 0; i < targets.Length; ++i)
            {
                Cone t = (Cone)targets[i];
                t.Height = h;
            }
            int n = EditorGUILayout.IntField("Split Number", c.SplitNumber);
            for (int i = 0; i < targets.Length; ++i)
            {
                Cone t = (Cone)targets[i];
                t.SplitNumber = n;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Top Sprite");
            if (GUILayout.Button("select"))
            {
                WindowSelectSprite.showWindow(c.Atlas, onSelectTop);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bottom Sprite");
            if (GUILayout.Button("select"))
            {
                WindowSelectSprite.showWindow(c.Atlas, onSelectBottom);
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    Cone t = (Cone)targets[i];
                    t.reset();
                }
            }
        }
    }

}