using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Represents for a sprite
    /// </summary>
    [ExecuteInEditMode]
    public class UISprite : UIImage
    {

        [SerializeField]
        protected UIAtlas m_atlas;
        [SerializeField]
        [HideInInspector]
        protected string m_spriteName;

        protected UIAtlasSprite m_sprite;

        /// <summary>
        /// The sprite in atlas this sprite using
        /// </summary>
        /// <value>The sprite</value>
        public UIAtlasSprite AtalsSprite
        {
            get { return m_sprite; }
        }

        /// <summary>
        /// The atlas of this sprite
        /// </summary>
        /// <value></value>
        public UIAtlas Atlas
        {
            get { return m_atlas; }
            set
            {
                if (m_atlas != value)
                {
                    m_atlas = value;
                    onAtlasChanged();
                }
            }
        }

        protected virtual void onAtlasChanged()
        {
            if (m_atlas != null)
            {
                m_sprite = m_atlas.getSprite(m_spriteName);
                m_updateFlag |= FLAG_UV | FLAG_UV1;
                if (m_imageType == LImageType.slice)
                {
                    repaint();
                }
            }
            else
            {
                m_sprite = null;
            }
            changeMaterialOnRender();
        }

        /// <summary>
        /// The sprite name of this sprite
        /// </summary>
        /// <value></value>
        public string Sprite
        {
            get { return m_spriteName; }
            set
            {
                if (m_spriteName != value)
                {
                    m_spriteName = value;
                    onSpriteNameChanged();
                }
            }
        }

        protected override void onImageTypeChanged()
        {
            if (m_imageType == LImageType.slice)
            {
                snapSliceMargin();
            }
            base.onImageTypeChanged();
        }

        protected virtual void onSpriteNameChanged()
        {
            if (string.IsNullOrEmpty(m_spriteName) || m_atlas == null)
            {
                m_sprite = null;
                m_meshRenderer.sharedMaterial = null;
            }
            else
            {
                m_sprite = m_atlas.getSprite(m_spriteName);
                changeMaterialOnRender();
            }
            m_updateFlag |= FLAG_UV | FLAG_UV1;
            if (m_imageType == LImageType.slice)
            {
                repaint();
            }
        }

        protected override Vector2 getImageSize()
        {
            if (m_sprite == null)
            {
                return Vector2.zero;
            }
            if (m_imageType == LImageType.normal)
            {
                return m_sprite.m_originalSize;
            }
            if (m_sprite.IsTrim)
            {
                Vector2 s = m_sprite.m_originalSize;
                s.x *= 1 - m_sprite.m_trimMargin.x - m_sprite.m_trimMargin.y;
                s.y *= 1 - m_sprite.m_trimMargin.z - m_sprite.m_trimMargin.w;
                return s;
            }
            return m_sprite.m_originalSize;
        }

        protected override Rect updateVertexPosition()
        {
            Rect rc = m_localPosition;
            if (m_imageType == LImageType.normal)
            {
                if (m_sprite.IsTrim)
                {
                    rc.xMin += m_size.x * m_sprite.m_trimMargin.x;
                    rc.xMax -= m_size.x * m_sprite.m_trimMargin.y;
                    rc.yMax -= m_size.y * m_sprite.m_trimMargin.z;
                    rc.yMin += m_size.y * m_sprite.m_trimMargin.w;
                }
            }
            return rc;
        }

        public void snapSliceMargin()
        {
            Vector4 v = Vector4.zero;
            if (m_sprite != null && m_sprite.m_atlas.m_material != null)
            {
                Texture t = m_sprite.m_atlas.m_material.mainTexture;
                if (t != null)
                {
                    v.Set(m_sprite.m_sliceMargin.x * t.width, m_sprite.m_sliceMargin.y * t.width, m_sprite.m_sliceMargin.z * t.height, m_sprite.m_sliceMargin.w * t.height);
                }
            }
            setSliceMargin(v.x, v.y, v.z, v.w);
        }

        protected override void changeMaterialOnRender()
        {
            Material m = null;
            if (m_sprite != null && m_atlas != null)
            {
                m = m_atlas.m_material;
                if (m_uiClip != null)
                {
                    m = m_uiClip.getClipMaterial(m);
                }
            }
            Render.sharedMaterial = m;
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
            onSpriteNameChanged();
        }

        protected override void makeSliceWeb()
        {
            Rect innerPos = getSliceInnerPos();
            Rect uv = m_sprite.m_uv;
            uv.xMin += m_sprite.m_sliceMargin.x;
            uv.xMax -= m_sprite.m_sliceMargin.y;
            uv.yMin += m_sprite.m_sliceMargin.w;
            uv.yMax -= m_sprite.m_sliceMargin.z;
            m_meshCreator.makeSliceWeb(m_localPosition, innerPos, m_sprite.m_uv, uv);
        }

        protected virtual void updateUVBySprite(UIAtlasSprite s)
        {
            switch (m_imageType)
            {
                case LImageType.normal:
                    m_meshCreator.updateUVNormal(s.m_uv);
                    break;
                case LImageType.fillHorizontal:
                case LImageType.fillVertical:
                    {
                        Rect rc = rectAfterFill(s.m_uv);
                        m_meshCreator.updateUVNormal(rc);
                    }
                    break;
                case LImageType.radar:
                    m_meshCreator.updateUVRadar(s.m_uv, !m_reverseFill);
                    break;
                case LImageType.tile:
                    m_meshCreator.updateUVTile(s.m_uv, m_row, m_column);
                    break;
                case LImageType.web:
                    m_meshCreator.updateUVWeb(s.m_uv, m_row, m_column);
                    break;
                case LImageType.slice:
                    m_meshCreator.updateUVSlice();
                    break;
                default:
                    break;
            }
        }

        protected override void updateUV()
        {
            updateUVBySprite(m_sprite);
            m_mesh.uv = m_meshCreator.m_uvs.ToArray();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UISprite", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UISprite>(true);
        }
#endif
    }
}
