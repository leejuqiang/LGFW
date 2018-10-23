using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(MeshEditorPlane))]
    public class InspectorMeshEditorPlane : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MeshEditorPlane p = (MeshEditorPlane)target;
            if (GUILayout.Button("create a node"))
            {

                GameObject go = LEditorKits.newGameObject("node", p.transform);
                go.AddComponent<MeshEditorNode>();
            }
        }
    }
}
