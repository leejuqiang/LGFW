using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomEditor(typeof(UITween), true)]
    [CanEditMultipleObjects]
    public class InspectorUITween : Editor
    {

        private float m_lastTime;

        void OnEnable()
        {
            m_lastTime = Time.realtimeSinceStartup;
        }

        void OnDisable()
        {
            for (int i = 0; i < targets.Length; ++i)
            {
                UITween t = (UITween)targets[i];
                t.stop();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("play"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UITween t = (UITween)targets[i];
                    t.forceAwake();
                    t.reset(t.m_isForward);
                    t.play(t.m_isForward);
                }
            }
            if (GUILayout.Button("reset"))
            {
                for (int i = 0; i < targets.Length; ++i)
                {
                    UITween t = (UITween)targets[i];
                    t.reset(t.m_isForward);
                }
            }
            if (!Application.isPlaying)
            {
                bool isPlaying = false;
                for (int i = 0; i < targets.Length; ++i)
                {
                    UITween t = (UITween)targets[i];
                    t.manualUpdate(Time.realtimeSinceStartup - m_lastTime);
                    isPlaying = isPlaying || t.IsPlaying;
                }
                m_lastTime = Time.realtimeSinceStartup;
                if (isPlaying)
                {
                    HandleUtility.Repaint();
                }
            }
        }
    }
}
