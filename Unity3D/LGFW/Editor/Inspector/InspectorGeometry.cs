using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeometryMesh), true)]
    public class InspectorGeometry : Editor
    {

        private List<int> m_ids;
        private List<string> m_labels;
        private List<UIAtlasSprite> m_values;

        void OnEnable()
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                var g = (GeometryMesh)targets[i];
                g.EditorChanged = false;
            }
            m_ids = new List<int>();
            m_labels = new List<string>();
            m_values = new List<UIAtlasSprite>();
        }

        void OnDisable()
        {
            m_ids = null;
            m_labels = null;
            m_values = null;
            WindowSelectSprite.close();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            var t = (GeometryMesh)target;
            m_labels.Clear();
            m_values.Clear();
            m_ids.Clear();
            t.getSelectSprite(m_labels, m_values, m_ids);
            drawSprites();
            bool changed = EditorGUI.EndChangeCheck();
            if (!changed)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    var g = (GeometryMesh)targets[i];
                    if (g.EditorChanged)
                    {
                        changed = true;
                        break;
                    }
                }
            }
            if (changed)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    var g = (GeometryMesh)targets[i];
                    g.onEditorChanged();
                    g.reset();
                }
            }
        }

        private void onSelectSprit(object item, MessageData data)
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                var g = (GeometryMesh)targets[i];
                g.onSelectedSprite((UIAtlasSprite)item, (int)data.m_data);
            }
        }

        private void drawSprites()
        {
            for (int i = 0; i < m_labels.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                string name = m_values[i] == null ? "" : m_values[i].m_name;
                var s = UnityEditor.EditorGUILayout.TextField(m_labels[i], name);
                if (s != name)
                {
                    for (int j = 0; j < targets.Length; ++j)
                    {
                        var g = (GeometryMesh)targets[j];
                        var at = g.editorGetAtlas();
                        if (at != null)
                        {
                            g.onSelectedSprite(at.getSprite(s), m_ids[i]);
                        }
                        else
                        {
                            g.onSelectedSprite(null, m_ids[i]);
                        }
                    }
                }
                if (GUILayout.Button("select"))
                {
                    var g = (GeometryMesh)target;
                    WindowSelectSprite.showWindow(g.editorGetAtlas(), onSelectSprit, m_ids[i]);
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}