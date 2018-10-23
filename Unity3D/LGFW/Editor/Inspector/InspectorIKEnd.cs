using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(IKEnd), true)]
    public class InspectorIKEnd : Editor
    {

        public override void OnInspectorGUI()
        {
            IKEnd e = (IKEnd)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("init"))
            {
                e.initBone();
                e.initBoneConfig();
            }
        }
    }
}
