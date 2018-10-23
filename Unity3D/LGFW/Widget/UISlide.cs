using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A slider
    /// </summary>
    public class UISlide : UITouchWidget
    {

        /// <summary>
        /// The image for the block of the slider
        /// </summary>
        public UIImage m_sliderBlock;
        /// <summary>
        /// The image for the background of the slider
        /// </summary>
        public UIImage m_backGround;
        /// <summary>
        /// If it's a vertical slider
        /// </summary>
        public bool m_isVertical;
        /// <summary>
        /// When save the value of this slider to PlayerPrefs, will use this as the key
        /// </summary>
        public string m_playerPrefKey;

        private Vector3 m_lastTouch;
        [SerializeField]
        [Range(0, 1)]
        private float m_value;
        private Vector2 m_range;

        /// <summary>
        /// The value of the slider
        /// </summary>
        /// <value></value>
        public float Value
        {
            get { return m_value; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_value != value)
                {
                    m_value = value;
                    setBlockPositionByValue();
                }
            }
        }

        public void computeRange()
        {
            if (m_isVertical)
            {
                float len = m_sliderBlock.Size.y;
                m_range.y = m_backGround.RightTop.y - len;
                m_range.x = m_backGround.LeftBottom.y;
            }
            else
            {
                float len = m_sliderBlock.Size.x;
                m_range.x = m_backGround.LeftBottom.x;
                m_range.y = m_backGround.RightTop.x - len;
            }
            setBlockPositionByValue();
        }

        /// <summary>
        /// Saves the value to PlayerPrefs
        /// </summary>
        public void saveToPlayerPref()
        {
            if (!string.IsNullOrEmpty(m_playerPrefKey))
            {
                PlayerPrefs.SetFloat(m_playerPrefKey, m_value);
                PlayerPrefs.Save();
            }
        }

        protected override void doAwake()
        {
            base.doAwake();
            if (m_sliderBlock == null || m_backGround == null)
            {
                return;
            }
            m_sliderBlock.Awake();
            m_backGround.Awake();
            m_sliderBlock.Trans.parent = m_backGround.Trans;
            m_sliderBlock.Trans.localRotation = Quaternion.identity;
            m_sliderBlock.Trans.localScale = Vector3.one;
            computeRange();
        }

        protected override void doPress(UITouch t)
        {
            base.doPress(t);
            Vector3 v = t.ScreenPositionInWorld;
            changeValueByTouch(v);
        }

        private void changeValueByTouch(Vector3 v)
        {
            if (m_backGround == null || m_sliderBlock == null)
            {
                return;
            }
            v = m_backGround.Trans.InverseTransformPoint(v);
            Vector3 p = m_sliderBlock.LeftBottom;
            float f = 0;
            if (m_isVertical)
            {
                v.y = Mathf.Clamp(v.y, m_range.x, m_range.y);
                f = LMath.lerpValue(m_range.x, m_range.y, v.y);
                p.y = v.y;
            }
            else
            {
                v.x = Mathf.Clamp(v.x, m_range.x, m_range.y);
                f = LMath.lerpValue(m_range.x, m_range.y, v.x);
                p.x = v.x;
            }
            if (f != Value)
            {
                m_value = f;
                p = m_backGround.Trans.TransformPoint(p);
                m_sliderBlock.setLeftBottomInWorldSpace(p, false);
            }
        }

        protected override void doDrag(UITouch t)
        {
            base.doDrag(t);
            Vector3 v = t.ScreenPositionInWorld;
            changeValueByTouch(v);
        }

        private void setBlockPositionByValue()
        {
            if (m_backGround == null || m_sliderBlock == null)
            {
                return;
            }
            Vector3 p = m_sliderBlock.LeftBottom;
            if (m_isVertical)
            {
                p.y = Mathf.Lerp(m_range.x, m_range.y, m_value);
            }
            else
            {
                p.x = Mathf.Lerp(m_range.x, m_range.y, m_value);
            }
            p = m_backGround.Trans.TransformPoint(p);
            m_sliderBlock.setLeftBottomInWorldSpace(p, false);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UISlide", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UISlide>(true);
            LEditorKits.addComponentToSelectedOjbects<BoxCollider2D>(true);
        }
#endif
    }
}
