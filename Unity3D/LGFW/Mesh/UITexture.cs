using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Represents for a texture
    /// </summary>
    [ExecuteInEditMode]
    public class UITexture : UIImage
    {

        [SerializeField]
        protected Material m_material;
        [SerializeField]
        protected Texture m_texture;
        [SerializeField]
        protected Shader m_shader;
        [SerializeField]
        [HideInInspector]
        protected Vector4 m_textureMargin;
        [SerializeField]
        [HideInInspector]
        protected Vector4 m_trimMargin;

        protected Material m_temporaryMaterial;

        protected Rect m_uvWithMargin;

        /// <summary>
        /// The trim margin
        /// </summary>
        /// <value></value>
        public Vector4 TrimMargin
        {
            get { return m_trimMargin; }
        }

        /// <summary>
        /// The margin for display part of the image
        /// </summary>
        /// <value></value>
        public Vector4 TextureMargin
        {
            get { return m_textureMargin; }
        }

        /// <summary>
        /// The material of this texture
        /// </summary>
        /// <value></value>
        public Material CustomMaterial
        {
            get { return m_material; }
            set
            {
                if (m_material != value)
                {
                    m_material = value;
                    onMaterialChanged();
                }
            }
        }

        /// <summary>
        /// The shader of the texture, only works when the material is not set
        /// </summary>
        /// <value></value>
        public Shader CustomShader
        {
            get { return m_shader; }
            set
            {
                if (m_shader != value)
                {
                    m_shader = value;
                    onShaderChanged();
                }
            }
        }

        /// <summary>
        /// The texture, only works when the material is not set
        /// </summary>
        /// <value></value>
        public Texture MainTexture
        {
            get { return m_texture; }
            set
            {
                if (m_texture != value)
                {
                    m_texture = value;
                    onTextureChanged();
                }
            }
        }

        /// <summary>
        /// Sets the margin of the image
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public void setTextureMargin(float left, float right, float top, float bottom)
        {
            if (m_textureMargin.x != left || m_textureMargin.y != right || m_textureMargin.z != top || m_textureMargin.w != bottom)
            {
                m_textureMargin.Set(left, right, top, bottom);
                onMarginUVChanged();
            }
        }

        protected void onShaderChanged()
        {
            if (m_material == null)
            {
                changeMaterialOnRender();
            }
        }

        protected void onTextureChanged()
        {
            if (m_material == null)
            {
                changeMaterialOnRender();
                onMarginUVChanged();
            }
        }

        protected override Vector2 getImageSize()
        {
            if (Render.sharedMaterial == null || Render.sharedMaterial.mainTexture == null)
            {
                return Vector2.zero;
            }
            Vector2 s = Vector2.zero;
            Texture t = Render.sharedMaterial.mainTexture;
            if (m_imageType == LImageType.normal)
            {
                float w = 1 - m_trimMargin.x - m_trimMargin.y;
                float h = 1 - m_trimMargin.z - m_trimMargin.w;
                if (w <= 0 || h <= 0)
                {
                    return Vector2.zero;
                }
                s.x = t.width / w;
                s.y = t.height / h;
            }
            else
            {
                s.x = t.width;
                s.y = t.height;
            }
            return s;
        }

        protected void onMarginUVChanged()
        {
            Rect rc = getUVWithMargin();
            if (rc != m_uvWithMargin)
            {
                m_uvWithMargin = rc;
                if (m_imageType == LImageType.slice)
                {
                    repaint();
                }
                else
                {
                    m_updateFlag |= FLAG_UV | FLAG_UV1;
                }
            }
        }

        protected void onMaterialChanged()
        {
            changeMaterialOnRender();
            onMarginUVChanged();
        }

        protected override void changeMaterialOnRender()
        {
            Material m = null;
            if (m_material != null)
            {
                m = m_material;
                m_temporaryMaterial = null;
            }
            else if (m_shader != null)
            {
                if (m_temporaryMaterial == null)
                {
                    m_temporaryMaterial = new Material(m_shader);
                }
                else
                {
                    m_temporaryMaterial.shader = m_shader;
                }
                m_temporaryMaterial.mainTexture = m_texture;
                m = m_temporaryMaterial;
            }
            else
            {
                m_temporaryMaterial = null;
            }
            if (m != null && m_uiClip != null)
            {
                m = m_uiClip.getClipMaterial(m);
                m.shader = m_uiClip.findShaderPair(m.shader).m_clipShader;
            }
            Render.sharedMaterial = m;
        }

        protected override Rect updateVertexPosition()
        {
            Rect rc = m_localPosition;
            if (m_imageType == LImageType.normal)
            {
                rc.xMin += m_size.x * m_trimMargin.x;
                rc.xMax -= m_size.x * m_trimMargin.y;
                rc.yMax -= m_size.y * m_trimMargin.z;
                rc.yMin += m_size.y * m_trimMargin.w;
            }
            return rc;
        }

        protected Rect getUVWithMargin()
        {
            Rect rc = new Rect(0, 0, 1, 1);
            if (m_meshRenderer.sharedMaterial == null || m_meshRenderer.sharedMaterial.mainTexture == null)
            {
                return rc;
            }
            Texture t = m_meshRenderer.sharedMaterial.mainTexture;
            rc.xMin += Mathf.Max(0, m_textureMargin.x / t.width);
            rc.xMax -= Mathf.Max(0, m_textureMargin.y / t.width);
            rc.yMin += Mathf.Max(0, m_textureMargin.w / t.height);
            rc.yMax -= Mathf.Max(0, m_textureMargin.z / t.height);
            return rc;
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
            if (m_material == null)
            {
                onTextureChanged();
            }
            else
            {
                onMaterialChanged();
            }
        }

        /// <summary>
        /// Sets the margin for trim
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        public void setTrimMargin(float left, float right, float top, float bottom)
        {
            left = Mathf.Clamp01(left);
            right = Mathf.Clamp01(right);
            top = Mathf.Clamp01(top);
            bottom = Mathf.Clamp01(bottom);
            if (left + right > 1)
            {
                right = 1 - left;
            }
            if (top + bottom > 1)
            {
                bottom = 1 - top;
            }
            if (left != m_trimMargin.x || right != m_trimMargin.y || top != m_trimMargin.z || bottom != m_trimMargin.w)
            {
                m_trimMargin.Set(left, right, top, bottom);
                if (m_imageType == LImageType.normal)
                {
                    m_updateFlag |= FLAG_VERTEX;
                }
            }
        }

        protected override void makeSliceWeb()
        {
            Rect innerPos = getSliceInnerPos();
            m_meshCreator.makeSliceWeb(m_localPosition, innerPos, new Rect(0, 0, 1, 1), m_uvWithMargin);
        }

        protected override void updateUV()
        {
            switch (m_imageType)
            {
                case LImageType.normal:
                    m_meshCreator.updateUVNormal(m_uvWithMargin);
                    break;
                case LImageType.web:
                    m_meshCreator.updateUVWeb(m_uvWithMargin, m_row, m_column);
                    break;
                case LImageType.tile:
                    m_meshCreator.updateUVTile(m_uvWithMargin, m_row, m_column);
                    break;
                case LImageType.fillVertical:
                case LImageType.fillHorizontal:
                    {
                        Rect uv = m_uvWithMargin;
                        uv = rectAfterFill(uv);
                        m_meshCreator.updateUVNormal(uv);
                    }
                    break;
                case LImageType.radar:
                    m_meshCreator.updateUVRadar(m_uvWithMargin, !m_reverseFill);
                    break;
                case LImageType.slice:
                    m_meshCreator.updateUVSlice();
                    break;
                default:
                    break;
            }
            m_mesh.uv = m_meshCreator.m_uvs.ToArray();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UITexture", false, (int)'t')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UITexture>(true);
        }
#endif
    }
}
