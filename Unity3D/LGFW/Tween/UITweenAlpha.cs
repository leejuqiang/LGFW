﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LGFW
{
    /// <summary>
    /// Tween for alpha
    /// </summary>
    public class UITweenAlpha : UITween
    {
        /// <summary>
        /// The UI Graphic. If null, uses the one found on the same GameObject
        /// </summary>
        public Graphic m_graphic;
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
                float a = Mathf.LerpUnclamped(m_from, m_to, f);
                Color c = m_graphic.color;
                c.a = a;
                m_graphic.color = c;
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