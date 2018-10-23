using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(MetaBall))]
    public class InspectorMetaball : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MetaBall mb = (MetaBall)target;
            if (GUILayout.Button("reset"))
            {
                mb.reset();
                mb.LateUpdate();
            }
        }
    }
}
