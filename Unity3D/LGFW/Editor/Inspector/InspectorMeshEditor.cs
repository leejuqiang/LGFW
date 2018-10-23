using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(MeshEditor))]
    public class InspectorMeshEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MeshEditor me = (MeshEditor)target;
            if (GUILayout.Button("create a plane"))
            {
                GameObject go = LEditorKits.newGameObject("plane", me.transform);
                go.AddComponent<MeshEditorPlane>();
            }
            if (GUILayout.Button("create a bone"))
            {
                GameObject go = LEditorKits.newGameObject("bone", me.transform);
                go.AddComponent<MeshEditorBone>();
            }
            if (GUILayout.Button("create mesh"))
            {
                me.createMesh();
            }
        }
    }
}
