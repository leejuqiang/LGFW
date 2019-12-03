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
                string folder = System.IO.Path.GetDirectoryName(ti.assetPath);
                TextureImporterConfig c = AssetDatabase.LoadAssetAtPath<TextureImporterConfig>(folder + "/teximporter.asset");
                if (c == null)
                {
                    return;
                }
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

        [UnityEditor.MenuItem("LGFW/Asset/create texture importer configuration", false, (int)'t')]
        public static void createImporterConfig()
        {
            string path = LEditorKits.openSaveToFolderPanel("Select a folder");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            var c = ScriptableObject.CreateInstance<TextureImporterConfig>();
            AssetDatabase.CreateAsset(c, path + "/teximporter.asset");
            Debug.Log("Texture importer configuration is created at \"" + path + "/teximporter.asset\"");
        }
    }
}
