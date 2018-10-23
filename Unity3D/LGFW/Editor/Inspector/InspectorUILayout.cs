using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UILayout), true)]
    [CanEditMultipleObjects]
    public class InspectorUILayout : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UILayout l = (UILayout)targets[i];
                    l.forceUpdateLayout();
                }
            }
        }
    }
}
