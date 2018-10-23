using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(EventTrigger))]
    public class InspectorEventTrigger : Editor
    {

        private string[] m_names;
        private MonoBehaviour[] m_monos;

        private int findScripts(EventTrigger t, GameObject go)
        {
            m_monos = go.GetComponents<MonoBehaviour>();
            m_names = new string[m_monos.Length];
            int ret = 0;
            for (int i = 0; i < m_names.Length; ++i)
            {
                m_names[i] = m_monos[i].GetType().Name;
                if (t.m_receiver == m_monos[i])
                {
                    ret = i;
                }
            }
            return ret;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EventTrigger t = (EventTrigger)target;
            GameObject go = (GameObject)EditorGUILayout.ObjectField("Receiver", t.m_receiver == null ? null : t.m_receiver.gameObject, typeof(GameObject), true);
            if (go != null)
            {
                ++EditorGUI.indentLevel;
                int index = findScripts(t, go);
                index = EditorGUILayout.Popup("Script", index, m_names);
                t.m_receiver = m_monos[index];
                --EditorGUI.indentLevel;
            }
        }
    }
}
