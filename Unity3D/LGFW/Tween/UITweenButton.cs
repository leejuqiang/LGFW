using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for a button
    /// </summary>
    public class UITweenButton : UITween
    {

        /// <summary>
        /// The scale when the button is pressed
        /// </summary>
        public Vector3 m_pressScale = new Vector3(1.1f, 1.1f, 1.1f);
        /// <summary>
        /// The scale when the button is at normal status
        /// </summary>
        public Vector3 m_normalScale = Vector3.one;

        [System.NonSerialized]
        public Color m_fromColor;
        [System.NonSerialized]
        public Color m_toColor;
        [System.NonSerialized]
        public Vector3 m_fromScale;
        [System.NonSerialized]
        public Vector3 m_toScale;

        private Transform m_trans;
        private UIButton m_btn;

        protected override void doAwake()
        {
            base.doAwake();
            m_trans = this.transform;
            m_btn = this.GetComponent<UIButton>();
            init();
        }

        public void init()
        {
            m_trans.localScale = m_normalScale;
        }

        public void setFrom()
        {
            m_fromScale = m_trans.localScale;
            if (m_btn.Sprite != null)
            {
                m_fromColor = m_btn.Sprite.CurrentColor;
            }
        }

        protected override void applyFactor(float f)
        {
            Vector3 s = Vector3.Lerp(m_fromScale, m_toScale, f);
            m_trans.localScale = s;
            if (m_btn.Sprite != null)
            {
                Color c = Color.Lerp(m_fromColor, m_toColor, f);
                m_btn.Sprite.CurrentColor = c;
            }
        }
    }
}
