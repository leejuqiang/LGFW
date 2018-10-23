using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for touchable widgets
    /// </summary>
    public class UITouchWidget : UIWidget
    {

        /// <summary>
        /// If true, the widget can be touched while multi touched
        /// </summary>
        public bool m_reactWhenMultiTouch;
        /// <summary>
        /// If true, the UIClip won't clip the touch of this widget
        /// </summary>
        public bool m_dontClip;

        protected int m_touchFingerId = -1;
        protected UIClip m_uiClip;

        /// <summary>
        /// If the widget is being touched
        /// </summary>
        /// <value></value>
        public bool IsTouching
        {
            get { return m_touchFingerId >= 0; }
        }

        /// <summary>
        /// The UIClip of this widget
        /// </summary>
        /// <value></value>
        public UIClip Clip
        {
            get { return m_uiClip; }
            set { m_uiClip = value; }
        }

        void OnPress(UITouch t)
        {
            if (m_touchFingerId < 0)
            {
                if (m_reactWhenMultiTouch || CameraRay.TouchCount <= 1)
                {
                    if (insideClipArea(t.m_hitPoint))
                    {
                        m_touchFingerId = t.m_id;
                        doPress(t);
                    }
                }
            }
        }

        protected bool insideClipArea(Vector3 worldPosition)
        {
            if (m_uiClip == null || m_dontClip)
            {
                return true;
            }
            return m_uiClip.isPointInsideClipArea(worldPosition);
        }

        protected virtual void doPress(UITouch t)
        {
            //todo
        }

        void OnRelease(UITouch t)
        {
            if (m_touchFingerId == t.m_id)
            {
                if (m_reactWhenMultiTouch || CameraRay.TouchCount <= 1)
                {
                    if (t.m_releaseInside)
                    {
                        t.m_releaseInside = insideClipArea(t.m_hitPoint);
                    }
                    doRelease(t);
                }
                m_touchFingerId = -1;
            }
        }

        protected virtual void doRelease(UITouch t)
        {
            //todo
        }

        void OnDrag(UITouch t)
        {
            if (m_touchFingerId == t.m_id)
            {
                if (m_reactWhenMultiTouch || CameraRay.TouchCount <= 1)
                {
                    doDrag(t);
                }
            }
        }

        protected virtual void doDrag(UITouch t)
        {
            //todo
        }
    }
}
