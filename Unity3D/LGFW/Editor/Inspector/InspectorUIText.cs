using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIText), true)]
    [CanEditMultipleObjects]
    public class InspectorUIText : Editor
    {

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UIText temp = (UIText)targets[i];
                    temp.reset();
                    temp.LateUpdate();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            UIText t = (UIText)target;
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    t = (UIText)targets[i];
                    t.reset();
                }
            }
        }
    }
}
