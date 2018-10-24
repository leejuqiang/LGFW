using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum UIButtonState
    {
        normal,
        pressed,
        disable,
    }

    /// <summary>
    /// A button
    /// </summary>
    [RequireComponent(typeof(UITweenButton))]
    public class UIButton : UITouchWidget
    {

        [HideInInspector]
        [SerializeField]
        protected UISprite m_sprite;
        protected UIButtonState m_state = UIButtonState.normal;
        [SerializeField]
        [HideInInspector]
        protected string[] m_spritesForStates = new string[3];
        [SerializeField]
        [HideInInspector]
        protected Color[] m_colorsForStates = new Color[] { Color.white, Color.white, Color.gray };
        protected UITweenButton m_btnTween;
        /// <summary>
        /// If the button is a toggle button
        /// </summary>
        public bool m_toggle;

        /// <summary>
        /// The sprite of this button
        /// </summary>
        /// <value></value>
        public UISprite Sprite
        {
            get { return m_sprite; }
            set
            {
                if (m_sprite != value)
                {
                    m_sprite = value;
                    onSpriteChanged();
                }
            }
        }

        /// <summary>
        /// The state of this button
        /// </summary>
        /// <value></value>
        public UIButtonState State
        {
            get { return m_state; }
            set
            {
                if (m_state != value)
                {
                    UIButtonState old = m_state;
                    m_state = value;
                    onStateChange(old);
                }
            }
        }

        protected void onSpriteChanged()
        {
            string s = "";
            if (m_sprite != null)
            {
                s = m_sprite.Sprite;
            }
            for (int i = 0; i < m_spritesForStates.Length; ++i)
            {
                m_spritesForStates[i] = s;
            }
        }

        /// <summary>
        /// Gets the sprite used for a state
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>The sprite name</returns>
        public string getSpriteForState(UIButtonState state)
        {
            return m_spritesForStates[(int)state];
        }

        /// <summary>
        /// Sets the sprite for a state
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="s">The sprite name</param>
        public void setSpriteForState(UIButtonState state, string s)
        {
            int i = (int)state;
            if (m_spritesForStates[i] != s)
            {
                m_spritesForStates[i] = s;
                if (state == m_state)
                {
                    if (m_sprite != null)
                    {
                        m_sprite.Sprite = s;
                        if (!Application.isPlaying)
                        {
                            m_sprite.LateUpdate();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the color of a state
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>The color</returns>
        public Color getColorForState(UIButtonState state)
        {
            return m_colorsForStates[(int)state];
        }

        /// <summary>
        /// Sets the color of a state
        /// </summary>
        /// <param name="state">The state</param>
        /// <param name="c">The color</param>
        public void setColorForState(UIButtonState state, Color c)
        {
            int i = (int)state;
            if (m_colorsForStates[i] != c)
            {
                m_colorsForStates[i] = c;
                if (state == m_state)
                {
                    if (m_btnTween != null && m_btnTween.IsPlaying)
                    {
                        m_btnTween.m_toColor = c;
                    }
                    else if (m_sprite != null)
                    {
                        m_sprite.CurrentColor = c;
                        if (!Application.isPlaying)
                        {
                            m_sprite.LateUpdate();
                        }
                    }
                }
            }
        }

        protected void onStateChange(UIButtonState oldState)
        {
            if (m_state == UIButtonState.pressed)
            {
                m_btnTween.m_toScale = m_btnTween.m_pressScale;
            }
            else
            {
                m_btnTween.m_toScale = m_btnTween.m_normalScale;
            }
            m_btnTween.setFrom();
            m_btnTween.m_toColor = m_colorsForStates[(int)m_state];
            if (m_sprite != null)
            {
                m_sprite.Sprite = m_spritesForStates[(int)m_state];
            }
            m_btnTween.resetAndPlay(true);
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_btnTween = this.GetComponent<UITweenButton>();
            m_btnTween.Awake();
            m_state = UIButtonState.normal;
            if (m_sprite != null)
            {
                m_sprite.CurrentColor = m_colorsForStates[(int)m_state];
                m_sprite.Sprite = m_spritesForStates[(int)m_state];
            }
        }

        protected override void doPress(UITouch t)
        {
            base.doPress(t);
            if (m_state == UIButtonState.disable)
            {
                return;
            }
            if (m_toggle)
            {
                State = m_state == UIButtonState.pressed ? UIButtonState.normal : UIButtonState.pressed;
            }
            else
            {
                State = UIButtonState.pressed;
            }
        }

        protected override void doRelease(UITouch t)
        {
            base.doRelease(t);
            if (m_state == UIButtonState.disable)
            {
                return;
            }
            if (!m_toggle)
            {
                State = UIButtonState.normal;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIButton", false, (int)'b')]
        public static void addToGameObjects()
        {
            UIButton[] bs = LEditorKits.addComponentToSelectedObjects<UIButton>(true);
            LEditorKits.addComponentToSelectedObjects<BoxCollider2D>(true);
            LEditorKits.addComponentToSelectedObjects<UIButtonMessage>(true);
            for (int i = 0; i < bs.Length; ++i)
            {
                UITweenButton tw = bs[i].GetComponent<UITweenButton>();
                tw.m_duration = 0.2f;
            }
        }
#endif
    }
}
