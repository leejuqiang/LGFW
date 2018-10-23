using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIScreen))]
    public class InspectorUIScreen : Editor
    {

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                UIScreen s = (UIScreen)target;
                s.reset();
            }
        }
    }
}
