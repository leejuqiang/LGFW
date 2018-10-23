using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A mesh represents a cylinder
    /// </summary>
    [ExecuteInEditMode]
    public class Cylinder : GeometryMesh
    {

        [SerializeField]
        protected float m_radius = 1;
        [SerializeField]
        protected float m_height = 1;
        [SerializeField]
        protected int m_divid = 10;
        [SerializeField]
        protected float m_anchorY = 0.5f;

        [SerializeField]
        [HideInInspector]
        protected string m_bottomSprite;
        [SerializeField]
        [HideInInspector]
        protected string m_topSprite;
        [SerializeField]
        [HideInInspector]
        protected string m_wallSprite;
        [SerializeField]
        protected UIAtlas m_atlas;

        [SerializeField]
        protected Color m_bottomColor = Color.white;
        [SerializeField]
        protected Color m_topColor = Color.white;
        [SerializeField]
        protected Color m_sideColor = Color.white;

        protected UIAtlasSprite m_bottomAS;
        protected UIAtlasSprite m_topAS;
        protected UIAtlasSprite m_wallAS;

        /// <summary>
        /// The y anchor of this cylinder
        /// </summary>
        /// <value>The y anchor</value>
        public float AnchorY
        {
            get { return m_anchorY; }
            set
            {
                if (m_anchorY != value)
                {
                    m_anchorY = value;
                    m_updateFlag |= UIMesh.FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The radius of this cylinder
        /// </summary>
        /// <value>The radius</value>
        public float Radius
        {
            get { return m_radius; }
            set
            {
                if (m_radius != value)
                {
                    m_radius = value;
                    m_updateFlag |= UIMesh.FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The height of this cylinder
        /// </summary>
        /// <value>The height</value>
        public float Height
        {
            get { return m_height; }
            set
            {
                if (m_height != value)
                {
                    m_height = value;
                    m_updateFlag |= UIMesh.FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The split number of this cylinder, with more split number, the cylinder will be more circular
        /// </summary>
        /// <value>The split number</value>
        public int Divid
        {
            get { return m_divid; }
            set
            {
                if (m_divid != value)
                {
                    m_divid = value;
                    repaint();
                }
            }
        }

        /// <summary>
        /// The color of the bottom
        /// </summary>
        /// <value>The color of the bottom</value>
        public Color BottomColor
        {
            get { return m_bottomColor; }
            set
            {
                if (m_bottomColor != value)
                {
                    m_bottomColor = value;
                    m_updateFlag |= UIMesh.FLAG_COLOR;
                }
            }
        }

        /// <summary>
        /// The color of the top
        /// </summary>
        /// <value>The color of the top</value>
        public Color TopColor
        {
            get { return m_topColor; }
            set
            {
                if (m_topColor != value)
                {
                    m_topColor = value;
                    m_updateFlag |= UIMesh.FLAG_COLOR;
                }
            }
        }

        /// <summary>
        /// The color of the side
        /// </summary>
        /// <value>The color of the side</value>
        public Color SideColor
        {
            get { return m_sideColor; }
            set
            {
                if (m_sideColor != value)
                {
                    m_sideColor = value;
                    m_updateFlag |= UIMesh.FLAG_COLOR;
                }
            }
        }

        /// <summary>
        /// The sprite of the bottom
        /// </summary>
        /// <value>The bottom sprite</value>
        public string BottomSprite
        {
            get { return m_bottomSprite; }
            set
            {
                if (m_bottomSprite != value)
                {
                    m_bottomSprite = value;
                    m_bottomAS = m_atlas == null ? null : m_atlas.getSprite(m_bottomSprite);
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The sprite of the top
        /// </summary>
        /// <value>The top sprite</value>
        public string TopSprite
        {
            get { return m_topSprite; }
            set
            {
                if (m_topSprite != value)
                {
                    m_topSprite = value;
                    m_topAS = m_atlas == null ? null : m_atlas.getSprite(m_topSprite);
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The sprite of the side
        /// </summary>
        /// <value>The side sprite</value>
        public string SideSprite
        {
            get { return m_wallSprite; }
            set
            {
                if (m_wallSprite != value)
                {
                    m_wallSprite = value;
                    m_wallAS = m_atlas == null ? null : m_atlas.getSprite(m_wallSprite);
                    m_updateFlag |= UIMesh.FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The UIAtlas this cylinder uses
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
                    onAtlasChanged();
                }
            }
        }

        protected void onAtlasChanged()
        {
            if (m_atlas != null)
            {
                m_render.sharedMaterial = m_atlas.m_material;
                m_updateFlag |= UIMesh.FLAG_UV;
            }
            else
            {
                m_render.sharedMaterial = null;
                repaint();
            }
            refindSprite();
        }

        protected void refindSprite()
        {
            if (m_atlas != null)
            {
                m_wallAS = m_atlas.getSprite(m_wallSprite);
                m_topAS = m_atlas.getSprite(m_topSprite);
                m_bottomAS = m_atlas.getSprite(m_bottomSprite);
            }
            else
            {
                m_wallAS = null;
                m_bottomAS = null;
                m_topAS = null;
            }
        }

        protected override void doAwake()
        {
            base.doAwake();
            onAtlasChanged();
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
            onAtlasChanged();
        }

        protected override void updateVertex()
        {
            float yMin = -m_anchorY * m_height;
            float yMax = yMin + m_height;
            float a = 360.0f / m_divid;
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(m_radius, 0, 0);
            for (int i = 0; i < m_divid; ++i)
            {
                m_vertices.Add(new Vector3(dir.x, yMin, dir.z));
                m_vertices.Add(new Vector3(dir.x, yMax, dir.z));
                dir = q * dir;
            }
            for (int i = 0, len = m_vertices.Count; i < len; ++i)
            {
                m_vertices.Add(m_vertices[i]);
            }
            m_vertices.Add(new Vector3(m_radius, yMin, 0));
            m_vertices.Add(new Vector3(m_radius, yMax, 0));
        }

        protected override void updateColor()
        {
            Color32 bc = m_bottomColor;
            Color32 tc = m_topColor;
            Color32 wc = m_sideColor;

            for (int i = 0; i < m_divid; ++i)
            {
                m_colors.Add(bc);
                m_colors.Add(tc);
            }
            while (m_colors.Count < m_vertices.Count)
            {
                m_colors.Add(wc);
            }
        }

        protected override void updateIndex()
        {
            int len = m_divid - 1;
            for (int i = 1; i < len; ++i)
            {
                int ii = i << 1;
                m_indexs.Add(0);
                m_indexs.Add(ii + 2);
                m_indexs.Add(ii);
            }
            for (int i = 1; i < len; ++i)
            {
                int ii = (i << 1) + 1;
                m_indexs.Add(1);
                m_indexs.Add(ii);
                m_indexs.Add(ii + 2);
            }
            int s = m_divid * 2;
            for (int i = 0; i < m_divid; ++i)
            {
                m_indexs.Add(s);
                m_indexs.Add(s + 3);
                m_indexs.Add(s + 1);
                m_indexs.Add(s);
                m_indexs.Add(s + 2);
                m_indexs.Add(s + 3);
                s += 2;
            }
        }

        protected override void updateUV()
        {
            Rect br = m_bottomAS == null ? (new Rect(0, 0, 1, 1)) : m_bottomAS.m_uv;
            Rect tr = m_topAS == null ? (new Rect(0, 0, 1, 1)) : m_topAS.m_uv;
            Rect wr = m_wallAS == null ? (new Rect(0, 0, 1, 1)) : m_wallAS.m_uv;
            Vector2 bc = br.center;
            Vector2 tc = tr.center;
            float a = 360.0f / m_divid;
            float bra = Mathf.Min(br.width, br.height);
            float tra = Mathf.Min(tr.width, tr.height);
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(0.5f, 0, 0);
            for (int i = 0; i < m_divid; ++i)
            {
                m_uvs.Add(new Vector2(dir.x * bra + bc.x, dir.z * bra + bc.y));
                m_uvs.Add(new Vector2(dir.x * tra + tc.x, dir.z * tra + tc.y));
                dir = q * dir;
            }
            float step = wr.width / m_divid;
            float x = wr.xMin;
            for (int i = 0; i <= m_divid; ++i)
            {
                m_uvs.Add(new Vector2(x, wr.yMin));
                m_uvs.Add(new Vector2(x, wr.yMax));
                x += step;
            }
        }

        protected override void updateNormal()
        {
            for (int i = 0; i < m_divid; ++i)
            {
                m_normals.Add(new Vector3(0, -1, 0));
                m_normals.Add(new Vector3(0, 1, 0));
            }
            float a = 360.0f / m_divid;
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(1, 0, 0);
            for (int i = 0; i < m_divid; ++i)
            {
                m_normals.Add(dir);
                m_normals.Add(dir);
                dir = q * dir;
            }
            m_normals.Add(new Vector3(1, 0, 0));
            m_normals.Add(new Vector3(1, 0, 0));
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Geometry/Cylinder", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<Cylinder>(true);
        }
#endif
    }
}
