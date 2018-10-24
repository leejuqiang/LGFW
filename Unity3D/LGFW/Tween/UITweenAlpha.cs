using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for component's alpha
    /// </summary>
    public class UITweenAlpha : UITween
    {

        /// <summary>
        /// From alpha
        /// </summary>
        public float m_from;

        /// <summary>
        /// To alpha
        /// </summary>
        public float m_to;

        private UIMesh m_mesh;

        protected override void doAwake()
        {
            base.doAwake();
            m_mesh = this.GetComponent<UIMesh>();
        }

        protected override void resetFromCurrentValue()
        {
            base.resetFromCurrentValue();
            if (m_mesh != null)
            {
                m_from = m_mesh.Alpha;
            }
        }

        protected override void resetToCurrentValue()
        {
            base.resetToCurrentValue();
            if (m_mesh != null)
            {
                m_to = m_mesh.Alpha;
            }
        }

        protected override void applyFactor(float f)
        {
            if (m_mesh != null)
            {
                float v = Mathf.Lerp(m_from, m_to, f);
                m_mesh.Alpha = v;
                if (!Application.isPlaying)
                {
                    m_mesh.LateUpdate();
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenAlpha", false, (int)'a')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenAlpha>(false);
        }
#endif
    }
}
