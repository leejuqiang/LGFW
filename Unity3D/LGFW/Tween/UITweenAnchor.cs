using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for anchor
    /// </summary>
    public class UITweenAnchor : UITween
    {

        /// <summary>
        /// From anchor
        /// </summary>
        public Vector2 m_from;
        /// <summary>
        /// To anchor
        /// </summary>
        public Vector2 m_to;

        private UIAnchor m_anchor;

        protected override void doAwake()
        {
            base.doAwake();
            m_anchor = this.GetComponent<UIAnchor>();
        }

        protected override void resetFromCurrentValue()
        {
            base.resetFromCurrentValue();
            if (m_anchor != null)
            {
                m_from = m_anchor.Anchor;
            }
        }

        protected override void resetToCurrentValue()
        {
            base.resetToCurrentValue();
            if (m_anchor != null)
            {
                m_to = m_anchor.Anchor;
            }
        }

        protected override void applyFactor(float f)
        {
            if (m_anchor != null)
            {
                Vector2 v = Vector2.Lerp(m_from, m_to, f);
                m_anchor.Anchor = v;
                if (!Application.isPlaying)
                {
                    m_anchor.LateUpdate();
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenAnchor", false, (int)'a')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenAnchor>(false);
        }
#endif
    }
}
