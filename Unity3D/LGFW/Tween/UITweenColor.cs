using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LGFW
{
    /// <summary>
    /// A tween change the color of an UI Graphic
    /// </summary>
    public class UITweenColor : UITween
    {
        /// <summary>
        /// The UI Graphic. If null, uses the one found on the same GameObject
        /// </summary>
        public Graphic m_graphic;
        /// <summary>
        /// The from Color
        /// </summary>
        public Color m_from;
        /// <summary>
        /// The to Color
        /// </summary>
        public Color m_to;
        /// <summary>
        /// If also change the alpha which changing the color
        /// </summary>
        public bool m_includeAlpha = true;

        protected override void doAwake()
        {
            if (m_graphic == null)
            {
                m_graphic = this.gameObject.GetComponent<Graphic>();
            }
        }

        protected override void editorAwake()
        {
            doAwake();
        }

        protected override void updateTween(float f)
        {
            if (m_graphic != null)
            {
                Color c = Color.LerpUnclamped(m_from, m_to, f);
                if (!m_includeAlpha)
                {
                    c.a = m_graphic.color.a;
                }
                m_graphic.color = c;
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