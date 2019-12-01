using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LGFW
{
    /// <summary>
    /// Tween for alpha of a panel, it changes the alpha of a CanvasGroup.
    /// </summary>
    public class UITweenPanelAlpha : UITween
    {
        /// <summary>
        /// The CanvasGroup. If null, uses the one found on the same GameObject
        /// </summary>
        public CanvasGroup m_group;
        /// <summary>
        /// The from alpha
        /// </summary>
        public float m_from;
        /// <summary>
        /// The to alpha
        /// </summary>
        public float m_to;

        protected override void doAwake()
        {
            if (m_group == null)
            {
                m_group = this.gameObject.GetComponent<CanvasGroup>();
            }
        }

        protected override void editorAwake()
        {
            doAwake();
        }

        protected override void updateTween(float f)
        {
            if (m_group != null)
            {
                float a = Mathf.LerpUnclamped(m_from, m_to, f);
                m_group.alpha = a;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenPanelAlpha", false, (int)'p')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenPanelAlpha>(false);
        }
#endif
    }
}