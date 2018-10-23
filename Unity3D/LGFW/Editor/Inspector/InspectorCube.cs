using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Cube))]
    [CanEditMultipleObjects]
    public class InspectorCube : Editor
    {

        private void onSelectSprite(object item, MessageData data)
        {
            UIAtlasSprite s = (UIAtlasSprite)item;
            for (int i = 0; i < targets.Length; ++i)
            {
                Cube c = (Cube)targets[i];
                c.setSpriteOfFace(s.m_name, (CubeFace)data.m_data);
            }
        }

        protected void drawFace(int f, Cube c)
        {
            CubeFace cf = (CubeFace)f;
            EditorGUILayout.LabelField(cf.ToString());
            ++EditorGUI.indentLevel;
            c.setColorOfFace(EditorGUILayout.ColorField("color", c.getColorOfFace(cf)), cf);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("sprite " + c.getSpriteOfFace(cf));
            if (c.Atlas != null)
            {
                if (GUILayout.Button("select"))
                {
                    WindowSelectSprite.showWindow(c.Atlas, onSelectSprite, (CubeFace)f);
                }
            }
            EditorGUILayout.EndHorizontal();
            c.setRotateOfFace((CubeFaceRotate)EditorGUILayout.EnumPopup("rotate", c.getRotateOfFace(cf)), cf);
            --EditorGUI.indentLevel;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Cube cu = (Cube)target;
            base.OnInspectorGUI();
            for (int i = 0; i < 6; ++i)
            {
                drawFace(i, cu);
            }
            if (GUILayout.Button("set all face as front"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    Cube c = (Cube)targets[i];
                    c.setColorForAllFace(c.getColorOfFace(CubeFace.front));
                    c.setSpriteForAllFace(c.getSpriteOfFace(CubeFace.front));
                    c.setRotateForAllFace(c.getRotateOfFace(CubeFace.front));
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    Cube c = (Cube)targets[i];
                    c.reset();
                }
            }
        }
    }
}
