using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The update mode of a layout
    /// </summary>
    public enum LayoutUpdateMode
    {
        /// <summary>
        /// Only change the layout at the Start
        /// </summary>
        once,
        /// <summary>
        /// Only change the layout when necessary 
        /// </summary>
        whenChanged,
        /// <summary>
        /// Change layout each update
        /// </summary>
        always,
    }

    /// <summary>
    /// Base class for layout component
    /// </summary>
    public abstract class UILayout : MonoBehaviour
    {

        [SerializeField]
        protected UIRect m_dependence;
        [SerializeField]
        protected LayoutUpdateMode m_updateMode = LayoutUpdateMode.once;

        protected Transform m_trans;
        protected int m_changeCount = 0;
        protected bool m_updateLayout;
        public bool m_justChangeX = true;
        protected bool m_hasAwake;

        /// <summary>
        /// The dependence of this layout, such as a UIScreen, the layout component will change the layout based on this dependence
        /// </summary>
        /// <value>The dependence</value>
        public UIRect Dependence
        {
            get { return m_dependence; }
            set
            {
                if (m_dependence != value)
                {
                    m_dependence = value;
                    m_changeCount = 0;
                }
            }
        }

        /// <summary>
        /// The update mode of this Layout
        /// </summary>
        /// <value>The update mode</value>
        public LayoutUpdateMode UpdateMode
        {
            get { return m_updateMode; }
            set
            {
                if (m_updateMode != value)
                {
                    m_updateMode = value;
                    m_changeCount = 0;
                }
            }
        }

        /// <summary>
        /// Forces the layout to be updated
        /// </summary>
        public void forceUpdateLayout()
        {
            m_updateLayout = true;
        }

        /// <summary>
        /// Forces to call Awake
        /// </summary>
        public void forceAwake()
        {
            m_hasAwake = false;
            Awake();
        }

        public void Awake()
        {
            if (!m_hasAwake)
            {
                m_trans = this.transform;
                if (m_dependence != null)
                {
                    m_dependence.Awake();
                }
                m_hasAwake = true;
            }
        }

        void Start()
        {
            if (m_updateMode == LayoutUpdateMode.once)
            {
                updateLayout();
            }
        }

        /// <summary>
        /// Updates the layout, this will be called in LateUpdate
        /// </summary>
        public void updateLayout()
        {
            m_updateLayout = false;
            if (m_dependence == null)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                Awake();
            }
            Rect rc = m_dependence.getPositionForLayout(m_trans);
            doUpdateLayout(rc);
        }

        protected abstract void doUpdateLayout(Rect position);

        public void LateUpdate()
        {
            if (m_dependence != null)
            {
                if (m_updateMode == LayoutUpdateMode.always || !Application.isPlaying || m_updateLayout)
                {
                    updateLayout();
                }
                else if (m_updateMode == LayoutUpdateMode.whenChanged)
                {
                    if (m_changeCount != m_dependence.TransChangeCount)
                    {
                        updateLayout();
                        m_changeCount = m_dependence.TransChangeCount;
                    }
                }
            }
        }
    }
}
