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
                ti.textureType = TextureImporterType.Default;
                ti.textureShape = TextureImporterShape.Texture2D;
                ti.sRGBTexture = true;
                ti.alphaSource = TextureImporterAlphaSource.FromInput;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Bilinear;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.maxTextureSize = 2048;
                ti.spriteImportMode = SpriteImportMode.None;
                ti.isReadable = false;
            }
        }
    }
}
