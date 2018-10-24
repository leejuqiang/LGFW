using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for color
    /// </summary>
    public class UITweenColor : UITween
    {

        /// <summary>
        /// From color
        /// </summary>
        public Color m_from;
        /// <summary>
        /// To color
        /// </summary>
        public Color m_to;

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
                m_from = m_mesh.CurrentColor;
            }
        }

        protected override void resetToCurrentValue()
        {
            base.resetToCurrentValue();
            if (m_mesh != null)
            {
                m_to = m_mesh.CurrentColor;
            }
        }

        protected override void applyFactor(float f)
        {
            if (m_mesh != null)
            {
                Color c = Color.Lerp(m_from, m_to, f);
                m_mesh.CurrentColor = c;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenColor", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenColor>(false);
        }
#endif
    }
}
