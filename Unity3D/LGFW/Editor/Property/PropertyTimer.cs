using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomPropertyDrawer(typeof(Timer), true)]
    public class PropertyTimer : PropertyDrawer
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 35;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = 15;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("m_time"), label);
            position.y += 20;
            ++EditorGUI.indentLevel;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("m_timeScale"), new GUIContent("Time Scale"));
            --EditorGUI.indentLevel;
        }
    }
}
