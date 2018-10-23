using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(RadiusGradient))]
    public class InspectorGradientRadius : InspectorGradient
    {

        protected override void OnSceneGUI()
        {
            RadiusGradient rg = (RadiusGradient)target;
            drawPoint(rg.m_center, rg.transform);
            base.OnSceneGUI();
        }
    }
}
