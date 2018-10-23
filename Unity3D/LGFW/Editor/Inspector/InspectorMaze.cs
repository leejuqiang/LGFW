using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Maze))]
    public class InspectorMaze : Editor
    {

        void OnEnable()
        {
            if (target == null)
            {
                return;
            }
            Maze m = (Maze)target;
            m.m_editMap = false;
            m.m_editItem = false;
        }

        public override void OnInspectorGUI()
        {
            Maze m = (Maze)target;
            EditorGUILayout.BeginHorizontal();
            m.m_editMap = EditorGUILayout.Toggle("Edit Map", m.m_editMap);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m.m_editItem = EditorGUILayout.Toggle("Edit Item", m.m_editItem);
            m.m_editItemId = (byte)EditorGUILayout.IntField("Item ID", m.m_editItemId);
            EditorGUILayout.EndHorizontal();
            base.OnInspectorGUI();
            if (GUILayout.Button("create maze"))
            {
                m.createMaze();
            }
            if (GUILayout.Button("create itmes"))
            {
                m.clearItmes();
                m.createItems(1);
            }
        }

        void OnSceneGUI()
        {
            if (target == null)
            {
                return;
            }
            if (Event.current.type == EventType.MouseUp)
            {
                Maze m = (Maze)target;
                if (m.m_editMap || m.m_editItem)
                {
                    Ray r = LGFW.LEditorKits.sceneViewClickToRay();
                    if (r.direction.z == 0)
                    {
                        return;
                    }
                    float n = -r.origin.z / r.direction.z;
                    Vector3 v = r.origin + r.direction * n;
                    m.onEditorChange(v.x, v.y, m.m_editMap, m.m_editItemId);
                }
            }
        }
    }
}
