using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(Gradient), true)]
    public class InspectorGradient : Editor
    {

        public override void OnInspectorGUI()
        {
            Gradient g = (Gradient)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                g.IsChanged = true;
            }
        }

        protected void drawPoint(GradientPoint gp, Transform t)
        {
            Vector3 v = gp.m_position;
            v = t.TransformPoint(v);
            v = Handles.PositionHandle(v, Quaternion.identity);
            v = t.InverseTransformPoint(v);
            gp.m_position = v;
        }

        protected virtual void OnSceneGUI()
        {
            Gradient g = (Gradient)target;
            Transform t = g.transform;
            if (g.m_points != null)
            {
                for (int i = 0; i < g.m_points.Count; ++i)
                {
                    drawPoint(g.m_points[i], t);
                }
            }
        }
    }
}
