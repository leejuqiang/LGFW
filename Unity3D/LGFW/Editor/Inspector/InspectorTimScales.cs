using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(TimeScales))]
    public class InspectorTimScales : Editor
    {

        private List<string> m_labels = new List<string>();
        private string m_newName;

        void OnEnable()
        {
            m_labels.Clear();
            int len = System.Enum.GetValues(typeof(TimeScaleID)).Length - 2;
            for (int i = -2; i < len; ++i)
            {
                m_labels.Add(((TimeScaleID)i).ToString());
            }
            m_newName = "";
        }

        void OnDisable()
        {
            m_labels.Clear();
            m_newName = "";
        }

        private void createNewId()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }
            if (string.IsNullOrEmpty(m_newName))
            {
                return;
            }
            for (int i = 0; i < m_labels.Count; ++i)
            {
                if (m_labels[i] == m_newName)
                {
                    return;
                }
            }
            if (m_newName == "count")
            {
                return;
            }
            m_labels.Add(m_newName);
            saveToText();
            m_newName = "";
        }

        private void onDeleteID()
        {
            saveToText();
        }

        private void saveToText()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            int s = -2;
            sb.Append("namespace LGFW\n{\n\tpublic enum TimeScaleID {\n");
            for (int i = 0; i < m_labels.Count; ++i, ++s)
            {
                sb.Append("\t\t");
                sb.Append(m_labels[i]);
                sb.Append(" = ");
                sb.Append(s + ",\n");
            }
            sb.Append("\t}\n}");
            System.IO.File.WriteAllText("Assets/LGFW/Global/TimeScaleID.cs", sb.ToString());
            AssetDatabase.Refresh();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Time Scale ID");
            ++EditorGUI.indentLevel;
            for (int i = 0; i < m_labels.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_labels[i]);
                if (i > 1)
                {
                    if (GUILayout.Button("-"))
                    {
                        if (!EditorApplication.isCompiling)
                        {
                            m_labels.RemoveAt(i);
                            onDeleteID();
                            return;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
            EditorGUILayout.BeginHorizontal();
            m_newName = EditorGUILayout.TextField("new id", m_newName);
            if (GUILayout.Button("add new id"))
            {
                createNewId();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
