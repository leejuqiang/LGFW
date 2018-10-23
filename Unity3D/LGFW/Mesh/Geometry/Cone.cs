using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A mesh represents for a cone
    /// </summary>
    [ExecuteInEditMode]
    public class Cone : GeometryMesh
    {

        [SerializeField]
        protected bool m_hasBottom = true;
        [SerializeField]
        protected bool m_isPyramid = false;
        [SerializeField]
        protected Vector2 m_scale = Vector2.one;
        [SerializeField]
        protected float m_offsetAngle = 0;
        [SerializeField]
        [HideInInspector]
        protected float m_height = 1;
        [SerializeField]
        [HideInInspector]
        protected int m_splitNumber = 3;
        [SerializeField]
        protected Vector2 m_tipAnchor = new Vector2(0.5f, 0.5f);
        [SerializeField]
        protected Color m_color = Color.white;
        [SerializeField]
        [HideInInspector]
        protected string m_topSprite;
        [SerializeField]
        [HideInInspector]
        protected string m_bottomSprite;
        [SerializeField]
        protected UIAtlas m_atlas;

        protected Rect m_rect;
        protected Vector3 m_tip;

        /// <summary>
        /// If the cone has a bottom
        /// </summary>
        /// <value>If the cone has a bottom</value>
        public bool HasBottom
        {
            get { return m_hasBottom; }
            set
            {
                if (m_hasBottom != value)
                {
                    m_hasBottom = value;
                    reset();
                }
            }
        }

        /// <summary>
        /// If true, it's a pyrmaid, this will make the normals difference
        /// </summary>
        /// <value>If it's a pyrmaid</value>
        public bool IsPyrmaid
        {
            get { return m_isPyramid; }
            set
            {
                if (m_isPyramid != value)
                {
                    m_isPyramid = value;
                    reset();
                }
            }
        }

        /// <summary>
        /// The size of x and y, it's the size of the external rectangle of the cone's bottom
        /// </summary>
        /// <value>The scale</value>
        public Vector2 Scale
        {
            get { return m_scale; }
            set
            {
                if (m_scale != value)
                {
                    m_scale = value;
                    m_updateFlag |= UIMesh.FLAG_VERTEX | UIMesh.FLAG_NORMAL;
                }
            }
        }

        /// <summary>
        /// The start angle of the cone
        /// </summary>
        /// <value>The start angle</value>
        public float OffsetAngle
        {
            get { return m_offsetAngle; }
            set
            {
                if (m_offsetAngle != value)
                {
                    m_offsetAngle = value;
                    m_updateFlag |= UIMesh.FLAG_VERTEX | UIMesh.FLAG_NORMAL;
                }
            }
        }

        /// <summary>
        /// The height of the cone
        /// </summary>
        /// <value>The height.</value>
        public float Height
        {
            get { return m_height; }
            set
            {
                if (value == 0)
                {
                    return;
                }
                if (m_height != value)
                {
                    if (m_height * value < 0)
                    {
                        m_updateFlag |= UIMesh.FLAG_VERTEX | UIMesh.FLAG_NORMAL;
                    }
                    else
                    {
                        m_updateFlag |= UIMesh.FLAG_TIP | UIMesh.FLAG_NORMAL;
                    }
                    m_height = value;
                }
            }
        }

        /// <summary>
        /// The split number of the bottom, with larger split number, the cone will be more circular
        /// </summary>
        /// <value>The split number</value>
        public int SplitNumber
        {
            get { return m_splitNumber; }
            set
            {
                if (value < 3)
                {
                    return;
                }
                if (m_splitNumber != value)
                {
                    m_splitNumber = value;
                    reset();
                }
            }
        }

        /// <summary>
        /// The anchor for the tip of the cone, the anchor is based on the external rectangle of the bottom
        /// </summary>
        /// <value>The tip anchor</value>
        public Vector2 TipAnchor
        {
            get { return m_tipAnchor; }
            set
            {
                if (m_tipAnchor != value)
                {
                    m_tipAnchor = value;
                    m_updateFlag |= UIMesh.FLAG_TIP | UIMesh.FLAG_NORMAL;
                }
            }
        }

        /// <summary>
        /// The color of the mesh
        /// </summary>
        /// <value>The color</value>
        public Color CurrentColor
        {
            get { return m_color; }
            set
            {
                if (m_color != value)
                {
                    m_color = value;
                    m_updateFlag |= UIMesh.FLAG_COLOR;
                }
            }
        }

        /// <summary>
        /// The sprite for the texture of the top. The texture will use overhead view
        /// </summary>
        /// <value>The top sprite.</value>
        public string TopSprite
        {
            get { return m_topSprite; }
            set
            {
                if (m_topSprite != value)
                {
                    m_topSprite = value;
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The sprite for the texture of the bottom
        /// </summary>
        /// <value>The bottom sprite.</value>
        public string BottomSprite
        {
            get { return m_bottomSprite; }
            set
            {
                if (m_bottomSprite != value)
                {
                    m_bottomSprite = value;
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The UIAtlas of the sprites
        /// </summary>
        /// <value>The atlas</value>
        public UIAtlas Atlas
        {
            get { return m_atlas; }
            set
            {
                if (m_atlas != value)
                {
                    m_atlas = value;
                    if (m_atlas == null)
                    {
                        m_render.sharedMaterial = null;
                    }
                    else
                    {
                        m_render.sharedMaterial = m_atlas.m_material;
                    }
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        protected override void preLateUpdate()
        {
            base.preLateUpdate();
            if ((m_updateFlag & UIMesh.FLAG_VERTEX) == 0 && (m_updateFlag & UIMesh.FLAG_TIP) > 0)
            {
                updateTip();
                m_mesh.vertices = m_vertices.ToArray();
            }
        }

        protected override void updateVertex()
        {
            float step = 360.0f / m_splitNumber;
            Vector3 v = new Vector3(0, 0, 1);
            if (m_offsetAngle != 0)
            {
                Quaternion q = Quaternion.Euler(0, m_offsetAngle, 0);
                v = q * v;
            }
            m_rect.Set(0, 0, 0, 0);
            Quaternion sq = Quaternion.Euler(0, step, 0);
            for (int i = 0; i < m_splitNumber; ++i)
            {
                m_vertices.Add(v);
                if (m_rect.xMin > v.x)
                {
                    m_rect.xMin = v.x;
                }
                if (m_rect.xMax < v.x)
                {
                    m_rect.xMax = v.x;
                }
                if (m_rect.yMin > v.z)
                {
                    m_rect.yMin = v.z;
                }
                if (m_rect.yMax < v.z)
                {
                    m_rect.yMax = v.z;
                }
                v = sq * v;
            }
            m_rect.xMin *= m_scale.x;
            m_rect.xMax *= m_scale.x;
            m_rect.yMin *= m_scale.y;
            m_rect.yMax *= m_scale.y;
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                v = m_vertices[i];
                v.x *= m_scale.x;
                v.z *= m_scale.y;
                m_vertices[i] = v;
            }
            if (m_isPyramid)
            {
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    m_vertices.Add(m_vertices[i]);
                }
            }
            if (m_hasBottom)
            {
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    m_vertices.Add(m_vertices[i]);
                }
            }
            updateTip();
        }

        protected void updateTip()
        {
            m_tip.x = m_rect.xMin + m_rect.width * m_tipAnchor.x;
            m_tip.z = m_rect.yMin + m_rect.height * m_tipAnchor.y;
            m_tip.y = m_height;
            int size = m_splitNumber;
            if (m_isPyramid)
            {
                size += m_splitNumber;
            }
            if (m_hasBottom)
            {
                size += m_splitNumber;
            }
            if (m_vertices.Count > size)
            {
                for (int i = 0, j = size; i < m_splitNumber; ++i)
                {
                    m_vertices[j++] = m_tip;
                }
            }
            else
            {
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    m_vertices.Add(m_tip);
                }
            }
        }

        protected override void updateUV()
        {
            Rect topUV = new Rect(0, 0, 1, 1);
            Vector2[] tempUVs = new Vector2[m_splitNumber];
            if (m_atlas != null)
            {
                UIAtlasSprite s = m_atlas.getSprite(m_topSprite);
                if (s != null)
                {
                    topUV = s.m_uv;
                }
            }
            Vector3 v = Vector3.forward;
            float step = 360.0f / m_splitNumber;
            Quaternion q = Quaternion.Euler(0, step, 0);

            for (int i = 0; i < m_splitNumber; ++i)
            {
                Vector2 uv = new Vector2(v.x, v.z);
                uv /= 2;
                uv.x += 0.5f;
                uv.y += 0.5f;
                tempUVs[i] = uv;
                v = q * v;
            }

            float w = topUV.width;
            float h = topUV.height;
            for (int i = 0; i < m_splitNumber; ++i)
            {
                Vector2 temp = tempUVs[i];
                Vector2 uv = new Vector2(topUV.xMin + w * temp.x, topUV.yMin + h * temp.y);
                m_uvs.Add(uv);
            }
            if (m_isPyramid)
            {
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    m_uvs.Add(m_uvs[i]);
                }
            }
            if (m_hasBottom)
            {
                Rect botUV = new Rect(0, 0, 1, 1);
                if (m_atlas != null)
                {
                    UIAtlasSprite s = m_atlas.getSprite(m_bottomSprite);
                    if (s != null)
                    {
                        botUV = s.m_uv;
                    }
                }
                w = botUV.width;
                h = botUV.height;
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    Vector2 temp = tempUVs[i];
                    Vector2 uv = new Vector2(botUV.xMin + w * temp.x, botUV.yMin + h * temp.y);
                    m_uvs.Add(uv);
                }
            }
            for (int i = 0; i < m_splitNumber; ++i)
            {
                m_uvs.Add(new Vector2(0.5f, 0.5f));
            }
        }

        protected override void updateNormal()
        {
            int tipStart = m_splitNumber;
            if (m_hasBottom)
            {
                tipStart += m_splitNumber;
            }
            if (m_isPyramid)
            {
                tipStart += m_splitNumber;
            }
            if (m_isPyramid)
            {
                for (int i = 0, l = m_splitNumber + m_splitNumber; i < l; ++i)
                {
                    m_normals.Add(Vector3.zero);
                }
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    Vector3 v1 = m_vertices[i + 1] - m_vertices[i];
                    Vector3 v2 = m_vertices[tipStart + i] - m_vertices[i];
                    v1 = Vector3.Cross(v1, v2);
                    if (Vector3.Dot(v1, m_tip) < 0)
                    {
                        v1 = -v1;
                    }
                    v1.Normalize();
                    m_normals[i] = v1;
                    if (i == m_splitNumber - 1)
                    {
                        m_normals[m_splitNumber] = v1;
                    }
                    else
                    {
                        m_normals[i + m_splitNumber + 1] = v1;
                    }
                }
            }
            else
            {
                Vector3 h = Vector3.up;
                if (m_height < 0)
                {
                    h.y = -1;
                }
                Vector3 mapTip = m_tip;
                mapTip.y = 0;
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    if (mapTip == m_vertices[i])
                    {
                        int i1 = i - 1;
                        if (i1 < 0)
                        {
                            i1 = m_splitNumber - 1;
                        }
                        int i2 = i + 1;
                        if (i2 >= m_splitNumber)
                        {
                            i2 = 0;
                        }
                        Vector3 n = m_vertices[i] - m_vertices[i1] + m_vertices[i] - m_vertices[i2];
                        n.Normalize();
                        m_normals.Add(n);
                    }
                    else
                    {
                        Vector3 v1 = m_vertices[tipStart + i] - m_vertices[i];
                        Vector3 v2 = Vector3.Cross(v1, h);
                        v1 = Vector3.Cross(v1, v2);
                        if (Vector3.Dot(v1, m_tip) < 0)
                        {
                            v1 = -v1;
                        }
                        v1.Normalize();
                        m_normals.Add(v1);
                    }
                }
            }
            if (m_hasBottom)
            {
                Vector3 n = Vector3.down;
                if (m_height < 0)
                {
                    n.y = 1;
                }
                for (int i = 0; i < m_splitNumber; ++i)
                {
                    m_normals.Add(n);
                }
            }
            for (int i = 0; i < m_splitNumber; ++i)
            {
                m_normals.Add(m_normals[i]);
            }
        }

        protected override void updateColor()
        {
            Color32 c = m_color;
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                m_colors.Add(c);
            }
        }

        protected override void updateIndex()
        {
            Vector3 v1 = m_vertices[2] - m_vertices[0];
            Vector3 v2 = m_vertices[1] - m_vertices[0];
            v1 = Vector3.Cross(v1, v2);
            v2 = m_tip - m_vertices[0];
            int tipStart = m_splitNumber;
            if (m_hasBottom)
            {
                tipStart += m_splitNumber;
            }
            if (m_isPyramid)
            {
                tipStart += m_splitNumber;
            }
            int l = m_splitNumber - 1;
            if (Vector3.Dot(v1, v2) < 0)
            {
                if (m_isPyramid)
                {
                    for (int i = 0; i < l; ++i)
                    {
                        m_indexs.Add(i);
                        m_indexs.Add(i + m_splitNumber + 1);
                        m_indexs.Add(tipStart + i);
                    }
                    m_indexs.Add(l);
                    m_indexs.Add(m_splitNumber);
                    m_indexs.Add(m_vertices.Count - 1);
                }
                else
                {
                    for (int i = 0; i < l; ++i)
                    {
                        m_indexs.Add(i);
                        m_indexs.Add(i + 1);
                        m_indexs.Add(tipStart + i);
                    }
                    m_indexs.Add(l);
                    m_indexs.Add(0);
                    m_indexs.Add(m_vertices.Count - 1);
                }
                if (m_hasBottom)
                {
                    int botStart = m_splitNumber;
                    if (m_isPyramid)
                    {
                        botStart += m_splitNumber;
                    }
                    for (int i = 1; i < l; ++i)
                    {
                        m_indexs.Add(botStart);
                        m_indexs.Add(botStart + i + 1);
                        m_indexs.Add(botStart + i);
                    }
                }
            }
            else
            {
                if (m_isPyramid)
                {
                    for (int i = 0; i < l; ++i)
                    {
                        m_indexs.Add(i + m_splitNumber + 1);
                        m_indexs.Add(i);
                        m_indexs.Add(tipStart + i);
                    }
                    m_indexs.Add(m_splitNumber);
                    m_indexs.Add(l);
                    m_indexs.Add(m_vertices.Count - 1);
                }
                else
                {
                    for (int i = 0; i < l; ++i)
                    {
                        m_indexs.Add(i + 1);
                        m_indexs.Add(i);
                        m_indexs.Add(tipStart + i);
                    }
                    m_indexs.Add(0);
                    m_indexs.Add(l);
                    m_indexs.Add(m_vertices.Count - 1);
                }
                if (m_hasBottom)
                {
                    int botStart = m_splitNumber;
                    if (m_isPyramid)
                    {
                        botStart += m_splitNumber;
                    }
                    for (int i = 1; i < l; ++i)
                    {
                        m_indexs.Add(botStart);
                        m_indexs.Add(botStart + i);
                        m_indexs.Add(botStart + i + 1);
                    }
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Geometry/Cone", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<Cone>(true);
        }
#endif
    }
}
