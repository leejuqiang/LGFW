using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for position
    /// </summary>
    public class UITweenPosition : UITween
    {

        /// <summary>
        /// From position
        /// </summary>
        public Vector3 m_from;
        /// <summary>
        /// To position
        /// </summary>
        public Vector3 m_to;
        /// <summary>
        /// If ignore the z value
        /// </summary>
        public bool m_ignoreZ = true;

        private Transform m_trans;

        protected override void doAwake()
        {
            base.doAwake();
            m_trans = this.transform;
        }

        protected override void resetFromCurrentValue()
        {
            base.resetFromCurrentValue();
            m_from = m_trans.localPosition;
        }

        protected override void resetToCurrentValue()
        {
            base.resetToCurrentValue();
            m_to = m_trans.localPosition;
        }

        protected override void applyFactor(float f)
        {
            Vector3 v = Vector3.Lerp(m_from, m_to, f);
            if (m_ignoreZ)
            {
                v.z = m_trans.localPosition.z;
            }
            m_trans.localPosition = v;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenPosition", false, (int)'p')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UITweenPosition>(false);
        }
#endif
    }
}
