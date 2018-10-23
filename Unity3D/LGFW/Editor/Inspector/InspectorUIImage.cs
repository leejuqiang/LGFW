using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class InspectorUIImage : Editor
    {

        protected virtual void OnEnable()
        {
            if (!Application.isPlaying)
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UIImage temp = (UIImage)targets[i];
                    temp.reset();
                    temp.LateUpdate();
                }
            }
        }

        protected void updateMeshVisible(bool visible)
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                UIImage ii = (UIImage)targets[i];
                if (ii.Filter != null)
                {
                    if (visible)
                    {
                        ii.Filter.hideFlags &= ~HideFlags.HideInInspector;
                    }
                    else
                    {
                        ii.Filter.hideFlags |= HideFlags.HideInInspector;
                    }
                }
                if (ii.Render != null)
                {
                    if (visible)
                    {
                        ii.Render.hideFlags &= ~HideFlags.HideInInspector;
                    }
                    else
                    {
                        ii.Render.hideFlags |= HideFlags.HideInInspector;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            UIImage im = (UIImage)target;
            if (targets.Length <= 1)
            {
                if (im.ImageType == LImageType.fillHorizontal || im.ImageType == LImageType.fillVertical || im.ImageType == LImageType.radar)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_fillValue"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_reverseFill"), true);
                }
                else if (im.ImageType == LImageType.web || im.ImageType == LImageType.tile)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_row"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_column"), true);
                }
                else if (im.ImageType == LImageType.slice)
                {
                    EditorGUILayout.LabelField("Slice Margin");
                    ++EditorGUI.indentLevel;
                    Vector4 sm = im.SliceMargin;
                    sm.x = EditorGUILayout.FloatField("Left", sm.x);
                    sm.y = EditorGUILayout.FloatField("Right", sm.y);
                    sm.z = EditorGUILayout.FloatField("Top", sm.z);
                    sm.w = EditorGUILayout.FloatField("Bottom", sm.w);
                    im.setSliceMargin(sm.x, sm.y, sm.z, sm.w);
                    --EditorGUI.indentLevel;
                }
            }
            customUpdate();
            serializedObject.ApplyModifiedProperties();
            if (GUILayout.Button("snap"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UIImage temp = (UIImage)targets[i];
                    temp.snap();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UIImage temp = (UIImage)targets[i];
                    temp.reset();
                }
            }
        }

        protected virtual void customUpdate()
        {
            //todo
        }
    }
}
