using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UIList))]
    public class InspectorUIList : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            for (int i = 0; i < targets.Length; ++i)
            {
                UIList l = (UIList)targets[i];
                l.resizeColliderAndClip();
            }
        }

    }
}
