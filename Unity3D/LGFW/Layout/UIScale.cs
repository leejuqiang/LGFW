using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An layout size
    /// </summary>
    [ExecuteInEditMode]
    public class UIScale : UILayout
    {

        [SerializeField]
        protected Vector2 m_scale;
        [SerializeField]
        protected Vector2 m_offset;
        /// <summary>
        /// The colliders will be resized
        /// </summary>
        public BoxCollider2D[] m_colliders;

        private Vector2 m_size;

        /// <summary>
        /// The size of the layout in its dependence
        /// </summary>
        /// <value>The size</value>
        public Vector2 Scale
        {
            get { return m_scale; }
            set
            {
                if (m_scale != value)
                {
                    m_scale = value;
                    m_updateLayout = true;
                }
            }
        }

        /// <summary>
        /// The size offset of the layout in its dependence
        /// </summary>
        /// <value>The size offset</value>
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

        /// <summary>
        /// Gets the size of this layout
        /// </summary>
        /// <returns>The size</returns>
        /// <param name="y">The height of the size, only used when just change x</param>
        public Vector2 getSize(float y)
        {
            Vector2 s = m_size;
            if (m_justChangeX)
            {
                s.y = y;
            }
            return s;
        }

        protected override void doUpdateLayout(Rect position)
        {
            m_size.x = position.width * m_scale.x + m_offset.x;
            if (!m_justChangeX)
            {
                m_size.y = position.height * m_scale.y + m_offset.y;
            }
            this.gameObject.SendMessage("OnUIScaleUpdate", this, SendMessageOptions.DontRequireReceiver);
            if (m_colliders != null)
            {
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    if (m_colliders[i] == null)
                    {
                        continue;
                    }
                    BoxCollider2D bx = m_colliders[i];
                    Vector2 s = bx.size;
                    s.x = m_size.x;
                    if (!m_justChangeX)
                    {
                        s.y = m_size.y;
                    }
                    bx.size = s;
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Layout/UIScale", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UIScale>(true);
        }
#endif
    }
}
