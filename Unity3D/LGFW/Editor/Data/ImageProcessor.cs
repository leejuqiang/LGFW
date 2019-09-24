using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class ImageProcessor : AssetPostprocessor
    {

        void OnPreprocessTexture()
        {
            TextureImporter ti = (TextureImporter)assetImporter;
            if (!LGFWKit.fileExists(AssetDatabase.GetTextMetaFilePathFromAssetPath(ti.assetPath)))
            {
                TextureImporterConfig c = EditorConfig.Instance.m_defaultTextureImporter;
                if (c.m_enable)
                {
                    ti.textureType = c.m_textureType;
                    ti.textureShape = c.m_textureShape;
                    ti.sRGBTexture = c.m_sRGB;
                    ti.alphaSource = c.m_alphaSource;
                    ti.alphaIsTransparency = c.m_alphaIsTransparency;
                    ti.mipmapEnabled = c.m_enableMipMap;
                    ti.wrapMode = c.m_wrapMode;
                    ti.filterMode = c.m_filter;
                    ti.textureCompression = c.m_compression;
                    ti.npotScale = c.m_npot;
                    ti.maxTextureSize = c.m_maxTextureSize;
                    ti.spriteImportMode = c.m_spriteMode;
                    ti.isReadable = c.m_readable;
                    ti.spritePixelsPerUnit = c.m_pixelPerUnit;
                    TextureImporterSettings s = new TextureImporterSettings();
                    ti.ReadTextureSettings(s);
                    s.spriteMeshType = c.m_spriteMeshType;
                    s.spriteGenerateFallbackPhysicsShape = c.m_enableSpritePhysicsShape;
                    ti.SetTextureSettings(s);
                }
            }
        }
    }
}
