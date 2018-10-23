using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace LGFW
{
    /// <summary>
    /// This represent an atlas
    /// </summary>
    public class UIAtlas : ScriptableObject
    {

        [SerializeField]
        [HideInInspector]
        private List<UIAtlasSprite> m_allSprites;
        /// <summary>
        /// The Material of this atlas
        /// </summary>
        public Material m_material;
        /// <summary>
        /// The padding used when building this atlas
        /// </summary>
        public int m_padding = 1;

        /// <summary>
        /// The folders contains the images which will be included by the atlas
        /// </summary>
        public string[] m_sourceFolders;
        /// <summary>
        /// Should the subfolders of source folders also be included
        /// </summary>
        public bool m_includeSubfolders;
        /// <summary>
        /// Trim the empty space from the edges of an image, this may make the atlas smaller
        /// </summary>
        public bool m_enableTrim;
        /// <summary>
        /// The min width of the empty space of an edge. Only trim the edge when the space is lager then this value
        /// </summary>
        public int m_trimWhenEmptyLagerThen;
        /// <summary>
        /// If true, the color in this atlas will multiply the alpha, and the atlas will have no alpha
        /// </summary>
        public bool m_premultiplyAlpha;

        private Dictionary<string, UIAtlasSprite> m_spritesDict;

        /// <summary>
        /// Gets the number of sprites in this atlas
        /// </summary>
        /// <value>The number of sprites</value>
        public int SpriteCount
        {
            get { return m_allSprites.Count; }
        }

        /// <summary>
        /// Gets a sprite by its index
        /// </summary>
        /// <returns>The sprite of the index</returns>
        /// <param name="index">The index</param>
        public UIAtlasSprite getSpriteByIndex(int index)
        {
            if (index < 0 || index >= m_allSprites.Count)
            {
                return null;
            }
            return m_allSprites[index];
        }

        /// <summary>
        /// Gets a sprite by its name
        /// </summary>
        /// <returns>The sprite of the name</returns>
        /// <param name="name">The name</param>
        public UIAtlasSprite getSprite(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (m_spritesDict == null)
            {
                m_spritesDict = new Dictionary<string, UIAtlasSprite>();
                for (int i = 0; i < m_allSprites.Count; ++i)
                {
                    m_spritesDict.Add(m_allSprites[i].m_name, m_allSprites[i]);
                    m_allSprites[i].m_atlas = this;
                }
            }
            UIAtlasSprite s = null;
            m_spritesDict.TryGetValue(name, out s);
            return s;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Asset/UIAtlas", false, (int)'a')]
        public static void createAnAtlasAsset()
        {
            UIAtlas ua = ScriptableObject.CreateInstance<UIAtlas>();
            LEditorKits.createAssetAtSelectedPath(ua, "Select a folder for the UIAtlas", "atlas.asset");
        }

        public void buildAtlas()
        {
            List<Texture2D> l = collectTextures();
            m_allSprites = new List<UIAtlasSprite>();
            for (int i = 0; i < l.Count; ++i)
            {
                UIAtlasSprite uas = new UIAtlasSprite(l[i].name);
                m_allSprites.Add(uas);
                uas.m_atlas = this;
                uas.m_originalSize = new Vector2(l[i].width, l[i].height);
                l[i] = processTexture(l[i]);
                if (m_enableTrim)
                {
                    l[i] = trimTexture(l[i], out uas.m_trimMargin);
                }
            }

            Texture2D atex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            Rect[] rcs = atex.PackTextures(l.ToArray(), m_padding);
            for (int i = 0; i < l.Count; ++i)
            {
                m_allSprites[i].m_uv = rcs[i];
            }
            if (m_premultiplyAlpha)
            {
                multiplyAlpha(atex);
            }

            Shader uis = Shader.Find("LGFW/ui");
            Shader uisPMA = Shader.Find("LGFW/uiPMA");
            string dirPath = LEditorKits.getAssetDirectory(this);
            if (m_material == null)
            {
                m_material = new Material(m_premultiplyAlpha ? uisPMA : uis);
                UnityEditor.AssetDatabase.CreateAsset(m_material, dirPath + "/" + this.name + ".mat");
            }
            if (m_material.shader == uis && m_premultiplyAlpha)
            {
                m_material.shader = uisPMA;
            }
            else if (m_material.shader == uisPMA && !m_premultiplyAlpha)
            {
                m_material.shader = uis;
            }
            string imagePath = "";
            if (m_material.mainTexture == null)
            {
                imagePath = dirPath + "/" + this.name + ".png";
            }
            else
            {
                imagePath = LEditorKits.getPathStartWithAssets(UnityEditor.AssetDatabase.GetAssetPath(m_material.mainTexture));
            }
            byte[] b = atex.EncodeToPNG();
            System.IO.File.WriteAllBytes(imagePath, b);
            UnityEditor.AssetDatabase.Refresh();
            atex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            m_material.mainTexture = atex;
            UnityEditor.EditorUtility.SetDirty(m_material);
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void multiplyAlpha(Texture2D t)
        {
            Color[] cs = t.GetPixels();
            for (int i = 0; i < cs.Length; ++i)
            {
                cs[i].r *= cs[i].a;
                cs[i].g *= cs[i].a;
                cs[i].b = cs[i].a;
            }
            t.SetPixels(cs);
        }

        private bool checkTrimRow(Color[] cols, int row, int width)
        {
            int s = row * width;
            int e = s + width;
            for (; s < e; ++s)
            {
                if (cols[s].a > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkTrimColumn(Color[] cols, int column, int width, int height)
        {
            int e = (height - 1) * width + column;
            for (int i = column; i < e; i += width)
            {
                if (cols[i].a > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private Texture2D trimTexture(Texture2D t, out Vector4 trimMargin)
        {
            trimMargin = Vector4.zero;
            Color[] cols = t.GetPixels();
            int left = 0;
            int right = t.width - 1;
            int top = t.height - 1;
            int bot = 0;
            while (checkTrimColumn(cols, left, t.width, t.height))
            {
                ++left;
            }
            while (checkTrimColumn(cols, right, t.width, t.height))
            {
                --right;
            }
            while (checkTrimRow(cols, bot, t.width))
            {
                ++bot;
            }
            while (checkTrimRow(cols, top, t.height))
            {
                --top;
            }
            if (left > m_trimWhenEmptyLagerThen || right > m_trimWhenEmptyLagerThen || bot > m_trimWhenEmptyLagerThen || top > m_trimWhenEmptyLagerThen)
            {
                --left;
                ++right;
                --bot;
                ++top;
                trimMargin.x = Mathf.Max(0, (float)left / t.width);
                trimMargin.y = Mathf.Max(0, 1 - (float)right / t.width);
                trimMargin.z = Mathf.Max(0, 1 - (float)top / t.height);
                trimMargin.w = Mathf.Max(0, (float)bot / t.height);
                int newW = right - left - 1;
                int newH = top - bot - 1;
                int len = newW * newH;
                Color[] newCols = new Color[len];
                int index = 0;
                for (int i = bot + 1; i < top; ++i)
                {
                    for (int j = left + 1; j < right; ++j)
                    {
                        newCols[index] = cols[j + i * t.width];
                        ++index;
                    }
                }
                t = new Texture2D(newW, newH, TextureFormat.ARGB32, false);
                t.SetPixels(newCols);
            }
            return t;
        }

        private Texture2D processTexture(Texture2D t)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(t);
            Texture2D ret = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            byte[] b = System.IO.File.ReadAllBytes(path);
            ret.LoadImage(b);
            return ret;
        }

        private List<Texture2D> collectTextures()
        {
            List<Texture2D> l = new List<Texture2D>();
            for (int i = 0; i < m_sourceFolders.Length; ++i)
            {
                DirectoryInfo dinfo = new DirectoryInfo(m_sourceFolders[i]);
                collectTexturesInDir(dinfo, l);
            }
            return l;
        }

        private void collectTexturesInDir(DirectoryInfo dinfo, List<Texture2D> l)
        {
            foreach (FileInfo finfo in dinfo.GetFiles())
            {
                string p = LEditorKits.getPathStartWithAssets(finfo.FullName);
                Texture2D tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (tex != null)
                {
                    l.Add(tex);
                }
            }
            if (m_includeSubfolders)
            {
                foreach (DirectoryInfo info in dinfo.GetDirectories())
                {
                    collectTexturesInDir(info, l);
                }
            }
        }
#endif
    }
}
