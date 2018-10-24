using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An layout anchor
    /// </summary>
    [ExecuteInEditMode]
    public class UIAnchor : UILayout
    {

        [SerializeField]
        protected Vector2 m_anchor;
        [SerializeField]
        protected Vector2 m_offset;

        /// <summary>
        /// The anchor of the layout for its dependence
        /// </summary>
        /// <value>The anchor</value>
        public Vector2 Anchor
        {
            get { return m_anchor; }
            set
            {
                if (m_anchor != value)
                {
                    m_anchor = value;
                    m_updateLayout = true;
                }
            }
        }

        /// <summary>
        /// The offset of this layout for its dependence
        /// </summary>
        /// <value>The offset</value>
        public Vector2 Offset
        {
            get { return m_offset; }
            set
            {
                if (m_offset != value)
                {
                    m_offset = value;
                    m_updateLayout = true;
                }
            }
        }

        protected override void doUpdateLayout(Rect position)
        {
            Vector3 v = m_trans.localPosition;
            v.x = position.xMin + position.width * m_anchor.x;
            v.x += m_offset.x;
            if (!m_justChangeX)
            {
                v.y = position.yMin + position.height * m_anchor.y;
                v.y += m_offset.y;
            }
            m_trans.localPosition = v;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Layout/UIAnchor", false, (int)'a')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UIAnchor>(true);
        }
#endif
    }
}
