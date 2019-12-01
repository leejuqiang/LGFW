using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for scale
    /// </summary>
    public class UITweenScale : UITween
    {
        /// <summary>
        /// From scale
        /// </summary>
        public Vector3 m_from;
        /// <summary>
        /// To scale
        /// </summary>
        public Vector3 m_to;

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
            Vector3 v = Vector3.LerpUnclamped(m_from, m_to, f);
            m_trans.localScale = v;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenScale", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenScale>(false);
        }
#endif
    }
}
