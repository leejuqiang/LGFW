using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class TextureImporterConfig : ScriptableObject
    {
        public TextureImporterType m_textureType = TextureImporterType.Default;
        public TextureImporterShape m_textureShape = TextureImporterShape.Texture2D;
        public TextureImporterAlphaSource m_alphaSource = TextureImporterAlphaSource.FromInput;
        public bool m_readable = false;
        public bool m_sRGB = false;
        public bool m_alphaIsTransparency = true;
        public bool m_enableMipMap = false;
        public TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        public FilterMode m_filter = FilterMode.Bilinear;
        public TextureImporterCompression m_compression = TextureImporterCompression.Uncompressed;
        public int m_maxTextureSize = 2048;
        public SpriteImportMode m_spriteMode = SpriteImportMode.None;
        public TextureImporterNPOTScale m_npot = TextureImporterNPOTScale.None;

        public float m_pixelPerUnit = 1;
        public SpriteMeshType m_spriteMeshType = SpriteMeshType.FullRect;
        public bool m_enableSpritePhysicsShape = false;
        public bool m_enable = true;
    }
}