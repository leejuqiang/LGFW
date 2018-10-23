using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(LayoutLoader))]
    [CanEditMultipleObjects]
    public class InspectorLayoutLoader : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("save layout"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    LayoutLoader ll = (LayoutLoader)targets[i];
                    ll.saveLayout();
                }
            }
            if (GUILayout.Button("load layout"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    LayoutLoader ll = (LayoutLoader)targets[i];
                    ll.loadLayout();
                }
            }
        }
    }
}
