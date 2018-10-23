using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace LGFW
{
    public enum UITextEffect
    {
        none,
        shadow,
        outline,
    }

    [System.Serializable]
    public class UITextImage
    {
        public UIImage m_image;
        public UIAlignmentY m_imageAlignment;
    }

    /// <summary>
    /// Represents for a text
    /// </summary>
    [ExecuteInEditMode]
    public class UIText : UIMesh
    {

        [SerializeField]
        protected UITextEffect m_effect = UITextEffect.none;
        [SerializeField]
        protected Color m_effectColor = Color.white;
        [SerializeField]
        protected Vector2 m_effectOffset;
        [SerializeField]
        protected UIFont m_font;
        [SerializeField]
        protected FontStyle m_fontStyle = FontStyle.Normal;
        [SerializeField]
        protected int m_fontSize = 20;
        [SerializeField]
        protected float m_lineSpace = 1;
        [SerializeField]
        protected float m_maxWidth;
        [SerializeField]
        protected UIAlignmentX m_textAlignment = UIAlignmentX.left;
        [SerializeField]
        protected bool m_formatText;
        [SerializeField]
        protected bool m_wrapByWord;
        [SerializeField]
        protected UIImage[] m_textImages;
        public string m_localizedTextId;
        [SerializeField]
        [Multiline]
        protected string m_text;

        protected UITextMeshCreator m_meshCreator = new UITextMeshCreator();

        /// <summary>
        /// The font of the text
        /// </summary>
        /// <value></value>
        public UIFont TextFont
        {
            get { return m_font; }
            set
            {
                if (m_font != value)
                {
                    m_font = value;
                    onFontChange();
                }
            }
        }

        /// <summary>
        /// The display size of the text
        /// </summary>
        /// <value></value>
        public Rect TextDisplaySize
        {
            get { return m_meshCreator.m_textArea; }
        }

        /// <summary>
        /// The horizon alignment
        /// </summary>
        /// <value></value>
        public UIAlignmentX TextAlignment
        {
            get { return m_textAlignment; }
            set
            {
                if (m_textAlignment != value)
                {
                    m_textAlignment = value;
                    m_updateFlag = FLAG_VERTEX;
                }
            }
        }

        protected void onFontChange()
        {
            changeMaterialOnRender();
            m_updateFlag |= FLAG_TEXT;
        }

        /// <summary>
        /// The font size
        /// </summary>
        /// <value></value>
        public int FontSize
        {
            get { return m_fontSize; }
            set
            {
                if (m_fontSize != value)
                {
                    m_fontSize = value;
                    onTextChange();
                }
            }
        }

        /// <summary>
        /// The font style
        /// </summary>
        /// <value></value>
        public FontStyle Style
        {
            get { return m_fontStyle; }
            set
            {
                if (m_fontStyle != value)
                {
                    m_fontStyle = value;
                    onTextChange();
                }
            }
        }

        /// <summary>
        /// The max width of the text. if <= 0, doesn't apply max width
        /// </summary>
        /// <value></value>
        public float MaxWidth
        {
            get { return m_maxWidth; }
            set
            {
                if (m_maxWidth != value)
                {
                    m_maxWidth = value;
                    onTextChange();
                }
            }
        }

        /// <summary>
        /// The real text of this text
        /// </summary>
        /// <value></value>
        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    onTextChange();
                }
            }
        }

        protected void onTextChange()
        {
            repaint();
        }

        /// <summary>
        /// If true, the text won't warp inside a word
        /// </summary>
        /// <value></value>
        public bool WrapByWord
        {
            get { return m_wrapByWord; }
            set
            {
                if (m_wrapByWord != value)
                {
                    m_wrapByWord = value;
                    onTextChange();
                }
            }
        }

        /// <summary>
        /// The display text
        /// </summary>
        /// <value></value>
        public string DisplayText
        {
            get { return m_meshCreator.m_displayText; }
        }

        /// <summary>
        /// If true, the text will use some chars as format
        /// </summary>
        /// <value></value>
        public bool FormatText
        {
            get { return m_formatText; }
            set
            {
                if (m_formatText != value)
                {
                    m_formatText = value;
                    onTextChange();
                }
            }
        }

        /// <summary>
        /// The space between lines
        /// </summary>
        /// <value></value>
        public float LineSpace
        {
            get { return m_lineSpace; }
            set
            {
                if (m_lineSpace != value)
                {
                    m_lineSpace = value;
                    m_updateFlag |= FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The offset of text effect
        /// </summary>
        /// <value></value>
        public Vector2 EffectOffset
        {
            get { return m_effectOffset; }
            set
            {
                if (m_effectOffset != value)
                {
                    m_effectOffset = value;
                    if (m_effect != UITextEffect.none)
                    {
                        m_updateFlag |= FLAG_VERTEX;
                    }
                }
            }
        }

        /// <summary>
        /// The type of text effect
        /// </summary>
        /// <value></value>
        public UITextEffect Effect
        {
            get { return m_effect; }
            set
            {
                if (m_effect != value)
                {
                    m_effect = value;
                    m_updateFlag |= FLAG_VERTEX | FLAG_UV | FLAG_COLOR | FLAG_INDEX;
                }
            }
        }

        /// <summary>
        /// The color of text effect
        /// </summary>
        /// <value></value>
        public Color EffectColor
        {
            get { return m_effectColor; }
            set
            {
                if (m_effectColor != value)
                {
                    m_effectColor = value;
                    if (m_effect != UITextEffect.none)
                    {
                        m_updateFlag |= FLAG_COLOR;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the image at a index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The image</returns>
        public UIImage getTextImageByIndex(int index)
        {
            if (m_textImages == null || index < 0 || index >= m_textImages.Length)
            {
                return null;
            }
            return m_textImages[index];
        }

        protected override void changeMaterialOnRender()
        {
            Material m = null;
            if (m_font != null)
            {
                m = m_font.m_material;
                if (m_uiClip != null)
                {
                    m = m_uiClip.getClipMaterial(m);
                }
            }
            Render.sharedMaterial = m;
        }

        protected override void doAwake()
        {
            base.doAwake();
            Font.textureRebuilt += onFontRebuild;
        }

        void Start()
        {
            if (Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(m_localizedTextId))
                {
                    m_text = Localization.getString(m_localizedTextId);
                }
            }
        }

        void OnDestroy()
        {
            Font.textureRebuilt -= onFontRebuild;
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
            onFontChange();
            onTextChange();
        }

        protected override void updateVertex()
        {
            Vector3 v = Vector3.zero;
            v.x = m_localPosition.xMin;
            v.y = m_localPosition.yMax;
            m_meshCreator.updateVertex(m_localPosition, this);
            m_mesh.vertices = m_meshCreator.m_vertices.ToArray();
        }

        protected void onFontRebuild(Font f)
        {
            if (m_font != null && m_font.m_font == f)
            {
                m_updateFlag |= FLAG_UV;
                m_meshCreator.onFontRebuild(m_formatText, f, m_fontSize, m_fontStyle);
            }
        }

        protected override void updateUV()
        {
            m_meshCreator.updateUV(this);
            m_mesh.uv = m_meshCreator.m_uvs.ToArray();
        }

        protected override void updateColorByGradient(Gradient g)
        {
            Color32[] cs = new Color32[m_meshCreator.m_vertices.Count];
            int effectColor = 0;
            if (m_effect == UITextEffect.shadow)
            {
                effectColor = 1;
            }
            else if (m_effect == UITextEffect.outline)
            {
                effectColor = 4;
            }
            for (int i = 0; i < cs.Length; ++i)
            {
                for (int j = 0; j < effectColor; ++j)
                {
                    cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 1);
                    ++i;
                    cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 1);
                    ++i;
                    cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 1);
                    ++i;
                    cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 1);
                    ++i;
                }
                cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 0);
                ++i;
                cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 0);
                ++i;
                cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 0);
                ++i;
                cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 0);
            }
            m_mesh.colors32 = cs;
        }

        protected override void updateColor(Color c)
        {
            Color ec = m_effectColor;
            ec.a *= m_alpha;
            m_mesh.colors32 = m_meshCreator.updateColors(c, ec, this);
        }

        protected override void updateIndex()
        {
            m_meshCreator.updateIndex();
            m_mesh.triangles = m_meshCreator.m_index.ToArray();
        }

        protected override void updateFlag()
        {
            if ((m_updateFlag | FLAG_TEXT) > 0)
            {
                m_meshCreator.processText(this);
            }
            base.updateFlag();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIText", false, (int)'t')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UIText>(true);
        }

        protected override void doDrawGizmos()
        {
            base.doDrawGizmos();
            if (m_maxWidth > 0)
            {
                Gizmos.color = Color.green;
                Vector3 v1 = new Vector3(m_localPosition.xMin, m_localPosition.center.y, 0);
                Vector3 v2 = v1;
                v2.x = v1.x + m_maxWidth;
                if (m_textAlignment == UIAlignmentX.center)
                {
                    v1.x += (m_size.x - m_maxWidth) * 0.5f;
                    v2.x = v1.x + m_maxWidth;
                }
                else if (m_textAlignment == UIAlignmentX.right)
                {
                    v1.x = m_localPosition.xMax;
                    v2.x = v1.x - m_maxWidth;
                }
                v1 = Trans.TransformPoint(v1);
                v2 = Trans.TransformPoint(v2);
                Gizmos.DrawLine(v1, v2);
            }
        }
#endif
    }
}
