using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for rotation with an angle and an axis
    /// </summary>
    public class UITweenAngle : UITween
    {
        /// <summary>
        /// From angle, in angle degree
        /// </summary>
        public float m_from;
        /// <summary>
        /// To angle, in angle degree
        /// </summary>
        public float m_to;
        /// <summary>
        /// The axis which the angle around
        /// </summary>
        public Vector3 m_axis = Vector3.forward;

        private Transform m_trans;

        protected override void doAwake()
        {
            if (m_trans == null)
            {
                m_trans = this.transform;
            }
        }

        protected override void editorAwake()
        {
            doAwake();
        }

        protected override void updateTween(float f)
        {
            float a = Mathf.LerpUnclamped(m_from, m_to, f);
            m_trans.localRotation = Quaternion.AngleAxis(a, m_axis);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenAngle", false, (int)'r')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenAngle>(false);
        }
#endif
    }
}