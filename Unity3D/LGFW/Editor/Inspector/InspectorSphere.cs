using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Sphere))]
    [CanEditMultipleObjects]
    public class InspectorSphere : Editor
    {

        private void onSelectSprite(object item, MessageData md)
        {
            string name = ((UIAtlasSprite)item).m_name;
            for (int i = 0; i < targets.Length; ++i)
            {
                Sphere s = (Sphere)targets[i];
                s.Sprite = name;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            Sphere sp = (Sphere)target;
            EditorGUILayout.BeginHorizontal();
            if (sp.Atlas != null)
            {
                EditorGUILayout.LabelField("sprite " + sp.Sprite);
                if (GUILayout.Button("select"))
                {
                    WindowSelectSprite.showWindow(sp.Atlas, onSelectSprite);
                }
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    Sphere s = (Sphere)targets[i];
                    s.reset();
                }
            }
        }
    }
}
