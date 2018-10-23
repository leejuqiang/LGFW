using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(MeshExporter))]
    [CanEditMultipleObjects]
    public class InspectorMeshExporter : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("export"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    MeshExporter me = (MeshExporter)targets[i];
                    me.export();
                }
            }
        }
    }
}
