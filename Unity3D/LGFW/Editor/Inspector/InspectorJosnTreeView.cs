using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(JsonTreeView))]
    public class InspectorJosnTreeView : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("show"))
            {
                ((JsonTreeView)target).showText();
            }
        }
    }
}
