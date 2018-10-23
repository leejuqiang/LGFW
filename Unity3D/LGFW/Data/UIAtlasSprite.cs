using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The sprite in an atlas
    /// </summary>
    [System.Serializable]
    public class UIAtlasSprite
    {

        /// <summary>
        /// The uv of the sprite
        /// </summary>
        public Rect m_uv;
        /// <summary>
        /// The margin of the uv for slice sprite
        /// </summary>
        public Vector4 m_sliceMargin;
        /// <summary>
        /// The name of the sprite
        /// </summary>
        public string m_name;
        /// <summary>
        /// The margin of the uv for triming
        /// </summary>
        public Vector4 m_trimMargin;
        /// <summary>
        /// The size in pixel of the sprite
        /// </summary>
        public Vector2 m_originalSize;

        /// <summary>
        /// The atlas this sprite belong to
        /// </summary>
        public UIAtlas m_atlas;

        /// <summary>
        /// Is this sprite trimed
        /// </summary>
        /// <value>If this sprite is trimed</value>
        public bool IsTrim
        {
            get { return m_trimMargin.x > 0 || m_trimMargin.y > 0 || m_trimMargin.z > 0 || m_trimMargin.w > 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.UIAtlasSprite"/> class.
        /// </summary>
        /// <param name="name">The name of the sprite</param>
        public UIAtlasSprite(string name)
        {
            m_name = name;
            m_sliceMargin = Vector4.zero;
            m_trimMargin = Vector4.zero;
        }

        /// <summary>
        /// Sets the slice margin
        /// </summary>
        /// <param name="v">The margin in the form of a Vector4, x is left, y is right, z is top, w is bottom</param>
        public void setSliceMargin(Vector4 v)
        {
            float w = m_uv.width;
            float h = m_uv.height;
            v.x = Mathf.Clamp(v.x, 0, w);
            v.y = Mathf.Clamp(v.y, 0, w);
            v.z = Mathf.Clamp(v.z, 0, h);
            v.w = Mathf.Clamp(v.w, 0, h);
            if (v.x + v.y > w)
            {
                v.y = w - v.x;
            }
            if (v.z + v.w > h)
            {
                v.w = h - v.z;
            }
            m_sliceMargin = v;
        }
    }
}
