using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UITimer))]
    public class InspectorUITimer : Editor
    {

        private static string[] m_labels = new string[] { "Year", "Month", "Day", "Hour", "Minuter", "Second" };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            UITimer t = (UITimer)target;
            if (t.m_timeFormatTextIds == null || t.m_timeFormatTextIds.Length != 6)
            {
                string[] temp = new string[6];
                for (int i = 0; i < temp.Length; ++i)
                {
                    if (i < t.m_timeFormatTextIds.Length)
                    {
                        temp[i] = t.m_timeFormatTextIds[i];
                    }
                    else
                    {
                        temp[i] = "";
                    }
                }
                t.m_timeFormatTextIds = temp;
            }
            t.m_timeFormat = EditorGUILayout.MaskField("Time Format", t.m_timeFormat, m_labels);
            EditorGUILayout.LabelField("Time Format Text ID");
            ++EditorGUI.indentLevel;
            int index = 0;
            if ((t.m_timeFormat & UITimer.F_YEAR) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Year", t.m_timeFormatTextIds[index]);
            }
            ++index;
            if ((t.m_timeFormat & UITimer.F_MONTH) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Month", t.m_timeFormatTextIds[index]);
            }
            ++index;
            if ((t.m_timeFormat & UITimer.F_DAY) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Day", t.m_timeFormatTextIds[index]);
            }
            ++index;
            if ((t.m_timeFormat & UITimer.F_HOUR) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Hour", t.m_timeFormatTextIds[index]);
            }
            ++index;
            if ((t.m_timeFormat & UITimer.F_MINUTER) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Minuter", t.m_timeFormatTextIds[index]);
            }
            ++index;
            if ((t.m_timeFormat & UITimer.F_SECOND) > 0)
            {
                t.m_timeFormatTextIds[index] = EditorGUILayout.TextField("Second", t.m_timeFormatTextIds[index]);
            }
            --EditorGUI.indentLevel;
        }
    }
}
