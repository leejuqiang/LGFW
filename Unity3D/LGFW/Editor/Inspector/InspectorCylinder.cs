using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Cylinder))]
    [CanEditMultipleObjects]
    public class InspectorCylinder : Editor
    {

        private void onSelectSprite(object item, MessageData data)
        {
            UIAtlasSprite s = (UIAtlasSprite)item;
            int f = (int)data.m_data;
            for (int i = 0; i < targets.Length; ++i)
            {
                Cylinder c = (Cylinder)targets[i];
                if (f == 0)
                {
                    c.BottomSprite = s.m_name;
                }
                else if (f == 1)
                {
                    c.TopSprite = s.m_name;
                }
                else
                {
                    c.SideSprite = s.m_name;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();

            Cylinder cy = (Cylinder)target;
            if (cy.Atlas != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Bottom Sprite", cy.BottomSprite);
                if (GUILayout.Button("select"))
                {
                    WindowSelectSprite.showWindow(cy.Atlas, onSelectSprite, 0);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Top Sprite", cy.TopSprite);
                if (GUILayout.Button("select"))
                {
                    WindowSelectSprite.showWindow(cy.Atlas, onSelectSprite, 1);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Side Sprite", cy.SideSprite);
                if (GUILayout.Button("select"))
                {
                    WindowSelectSprite.showWindow(cy.Atlas, onSelectSprite, 2);
                }
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    Cylinder c = (Cylinder)targets[i];
                    c.reset();
                }
            }
        }
    }
}
