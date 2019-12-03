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
        /// The name of the sprite
        /// </summary>
        public string m_name;
        /// <summary>
        /// The size in pixel of the sprite
        /// </summary>
        public Vector2 m_pixelSize;

        /// <summary>
        /// The atlas this sprite belong to
        /// </summary>
        public UIAtlas m_atlas;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.UIAtlasSprite"/> class.
        /// </summary>
        /// <param name="name">The name of the sprite</param>
        public UIAtlasSprite(string name)
        {
            m_name = name;
        }
    }
}
