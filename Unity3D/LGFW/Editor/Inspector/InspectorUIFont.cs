using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIFont))]
    public class InspectorUIFont : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            UIFont f = (UIFont)target;
            if (EditorGUI.EndChangeCheck())
            {
                f.resetMaterial();
            }
            if (GUILayout.Button("create material"))
            {
                f.createMaterial();
            }
        }
    }
}
