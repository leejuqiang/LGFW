using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum LImageType
    {
        normal,
        slice,
        radar,
        web,
        tile,
        fillVertical,
        fillHorizontal,
    }

    /// <summary>
    /// Represents for a image
    /// </summary>
    public abstract class UIImage : UIMesh
    {

        [SerializeField]
        protected LImageType m_imageType;
        [SerializeField]
        [HideInInspector]
        protected int m_row = 1;
        [SerializeField]
        [HideInInspector]
        protected int m_column = 1;
        [SerializeField]
        [Range(0, 1)]
        [HideInInspector]
        protected float m_fillValue = 1;
        [SerializeField]
        [HideInInspector]
        protected Vector4 m_sliceMargin;
        [SerializeField]
        [HideInInspector]
        protected bool m_reverseFill;

        protected UIImageMeshCreator m_meshCreator = new UIImageMeshCreator();

        /// <summary>
        /// Margin for slice type, x, y, z, w are left, right, top, bottom
        /// </summary>
        /// <value>The margin</value>
        public Vector4 SliceMargin
        {
            get { return m_sliceMargin; }
        }

        /// <summary>
        /// The type of this image
        /// </summary>
        /// <value>The type</value>
        public LImageType ImageType
        {
            get { return m_imageType; }
            set
            {
                if (m_imageType != value)
                {
                    m_imageType = value;
                    onImageTypeChanged();
                }
            }
        }

        protected virtual void onImageTypeChanged()
        {
            repaint();
        }

        protected override void onSizeChanged()
        {
            base.onSizeChanged();
            if (m_imageType == LImageType.radar)
            {
                m_updateFlag |= FLAG_UV | FLAG_UV1;
            }
        }

        /// <summary>
        /// Fill value if the type is a kind of filling
        /// </summary>
        /// <value>The fill value</value>
        public float FillValue
        {
            get { return m_fillValue; }
            set
            {
                if (m_fillValue != value)
                {
                    m_fillValue = value;
                    onFillValueChanged();
                }
            }
        }

        /// <summary>
        /// If fills from a reverse direction
        /// </summary>
        /// <value>True if reverse</value>
        public bool ReverseFill
        {
            get { return m_reverseFill; }
            set
            {
                if (m_reverseFill != value)
                {
                    m_reverseFill = value;
                    onFillValueChanged();
                }
            }
        }

        /// <summary>
        /// The row number for type tile and web
        /// </summary>
        /// <value>The row number</value>
        public int Row
        {
            get { return m_row; }
            set
            {
                if (m_row != value)
                {
                    m_row = value;
                    onRowOrColumnChanged();
                }
            }
        }

        /// <summary>
        /// The column nubmer for type tile and web
        /// </summary>
        /// <value>The column number</value>
        public int Column
        {
            get { return m_column; }
            set
            {
                if (m_column != value)
                {
                    m_column = value;
                    onRowOrColumnChanged();
                }
            }
        }

        protected virtual void onRowOrColumnChanged()
        {
            if (m_imageType == LImageType.web || m_imageType == LImageType.tile)
            {
                repaint();
            }
        }

        protected abstract Rect updateVertexPosition();

        protected virtual void onFillValueChanged()
        {
            if (m_imageType == LImageType.radar)
            {
                repaint();
            }
            else if (m_imageType == LImageType.fillVertical || m_imageType == LImageType.fillHorizontal)
            {
                m_updateFlag |= FLAG_VERTEX;
                m_updateFlag |= FLAG_UV | FLAG_UV1;
            }
        }

        /// <summary>
        /// Resizes the image to its actual size
        /// </summary>
        public void snap()
        {
            Size = getImageSize();
        }

        /// <summary>
        /// Resize the image to fill a square, keeps aspect
        /// </summary>
        /// <param name="maxSize">The length of the edge of the square</param>
        public void snap(float maxSize)
        {
            snap(maxSize, maxSize);
        }

        /// <summary>
        /// Resizes the image to fill a rectangle, keeps aspect
        /// </summary>
        /// <param name="maxWidth">The width of the rectangle</param>
        /// <param name="maxHeight">The height of the rectangle</param>
        public void snap(float maxWidth, float maxHeight)
        {
            Vector2 s = getImageSize();
            if (s.x <= 0 || s.y <= 0 || maxWidth <= 0 || maxHeight <= 0)
            {
                Size = Vector2.zero;
                return;
            }
            float k = maxWidth / maxHeight;
            float ki = s.x / s.y;
            if (ki >= k)
            {
                s.y = maxWidth / ki;
                s.x = maxWidth;
            }
            else
            {
                s.y = maxHeight;
                s.x = maxHeight * ki;
            }
            Size = s;
        }

        protected abstract Vector2 getImageSize();

        /// <summary>
        /// Sets the slice margin
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="right">Right</param>
        /// <param name="top">Top</param>
        /// <param name="bottom">Bottom</param>
        public virtual void setSliceMargin(float left, float right, float top, float bottom)
        {
            left = Mathf.Clamp(left, 0, m_size.x);
            right = Mathf.Clamp(right, 0, m_size.x);
            top = Mathf.Clamp(top, 0, m_size.y);
            bottom = Mathf.Clamp(bottom, 0, m_size.y);
            if (left + right > m_size.x)
            {
                right = m_size.x - left;
            }
            if (top + bottom > m_size.y)
            {
                bottom = m_size.y - top;
            }
            if (m_sliceMargin.x != left || m_sliceMargin.y != right || m_sliceMargin.z != top || m_sliceMargin.w != bottom)
            {
                m_sliceMargin.Set(left, right, top, bottom);
                if (m_imageType == LImageType.slice)
                {
                    repaint();
                }
            }
        }

        protected virtual Rect getSliceInnerPos()
        {
            Rect innerPos = m_localPosition;
            innerPos.xMin += m_sliceMargin.x;
            innerPos.xMax -= m_sliceMargin.y;
            innerPos.yMin += m_sliceMargin.w;
            innerPos.yMax -= m_sliceMargin.z;
            return innerPos;
        }

        protected Rect rectAfterFill(Rect rc)
        {
            if (m_imageType == LImageType.fillHorizontal)
            {
                if (m_reverseFill)
                {
                    rc.xMin += (1 - m_fillValue) * rc.width;
                }
                else
                {
                    rc.xMax -= (1 - m_fillValue) * rc.width;
                }
            }
            else if (m_imageType == LImageType.fillVertical)
            {
                if (m_reverseFill)
                {
                    rc.yMin += (1 - m_fillValue) * rc.height;
                }
                else
                {
                    rc.yMax -= (1 - m_fillValue) * rc.height;
                }
            }
            return rc;
        }


        protected override void updateColor(Color c)
        {
            m_mesh.colors32 = m_meshCreator.updateColor(c);
        }

        protected override void updateColorByGradient(Gradient g)
        {
            Color32[] cs = new Color32[m_meshCreator.m_vertices.Count];
            for (int i = 0; i < cs.Length; ++i)
            {
                cs[i] = g.getColor(m_meshCreator.m_vertices[i], m_alpha, 0);
            }
            m_mesh.colors32 = cs;
        }

        protected abstract void makeSliceWeb();

        protected override void updateVertex()
        {
            switch (m_imageType)
            {
                case LImageType.normal:
                    {
                        Rect rc = updateVertexPosition();
                        m_meshCreator.updateVertexNormal(rc);
                    }
                    break;
                case LImageType.radar:
                    m_meshCreator.updateVertexRadar(m_localPosition, m_fillValue, !m_reverseFill);
                    break;
                case LImageType.web:
                    m_meshCreator.updateVertexWeb(m_localPosition, m_row, m_column);
                    break;
                case LImageType.tile:
                    m_meshCreator.updateVertexTile(m_localPosition, m_row, m_column);
                    break;
                case LImageType.slice:
                    makeSliceWeb();
                    m_meshCreator.updateVertexSlice();
                    break;
                case LImageType.fillVertical:
                case LImageType.fillHorizontal:
                    {
                        Rect rc = rectAfterFill(m_localPosition);
                        m_meshCreator.updateVertexNormal(rc);
                    }
                    break;
                default:
                    break;
            }
            m_mesh.vertices = m_meshCreator.m_vertices.ToArray();
        }

        protected override void updateIndex()
        {
            switch (m_imageType)
            {
                case LImageType.normal:
                case LImageType.fillVertical:
                case LImageType.fillHorizontal:
                    m_meshCreator.updateIndexNormal();
                    break;
                case LImageType.radar:
                    {
                        int len = 0;
                        if (m_fillValue >= 1)
                        {
                            len = -1;
                        }
                        else if (m_fillValue > 0)
                        {
                            len = m_mesh.vertices.Length;
                        }
                        m_meshCreator.updateIndexRadar(len, !m_reverseFill);
                    }
                    break;
                case LImageType.web:
                    m_meshCreator.updateIndexWeb(m_row, m_column);
                    break;
                case LImageType.tile:
                    m_meshCreator.updateIndexTile(m_row, m_column);
                    break;
                case LImageType.slice:
                    m_meshCreator.updateIndexSlice();
                    break;
                default:
                    break;
            }
            m_mesh.triangles = m_meshCreator.m_indexes.ToArray();
        }
    }
}
