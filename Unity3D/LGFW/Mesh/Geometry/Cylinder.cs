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
        protected int m_segmentNumber = 10;
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
                    m_flag |= FLAG_VERTEX;
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
                    m_flag |= FLAG_VERTEX;
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
                    m_flag |= FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The segment number of this cylinder, with more segment number, the cylinder will be more circular
        /// </summary>
        /// <value>The segment number</value>
        public int SegmentNumber
        {
            get { return m_segmentNumber; }
            set
            {
                if (m_segmentNumber != value)
                {
                    m_segmentNumber = value;
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
                    m_flag |= FLAG_COLOR;
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
                    m_flag |= FLAG_COLOR;
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
                    m_flag |= FLAG_COLOR;
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
                    // m_bottomSprite = value;
                    // m_bottomAS = m_atlas == null ? null : m_atlas.getSprite(m_bottomSprite);
                    // m_updateFlag |= UIMesh.FLAG_UV;
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
                    m_flag |= FLAG_UV;
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
                    m_flag |= FLAG_UV;
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
                    m_flag |= FLAG_UV;
                }
            }
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
        }

        protected override void updateVertex()
        {
            float yMin = -m_anchorY * m_height;
            float yMax = yMin + m_height;
            float a = 360.0f / m_segmentNumber;
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(m_radius, 0, 0);
            for (int i = 0; i < m_segmentNumber; ++i)
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

            for (int i = 0; i < m_segmentNumber; ++i)
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
            int len = m_segmentNumber - 1;
            for (int i = 1; i < len; ++i)
            {
                int ii = i << 1;
                m_indexes.Add(0);
                m_indexes.Add(ii + 2);
                m_indexes.Add(ii);
            }
            for (int i = 1; i < len; ++i)
            {
                int ii = (i << 1) + 1;
                m_indexes.Add(1);
                m_indexes.Add(ii);
                m_indexes.Add(ii + 2);
            }
            int s = m_segmentNumber * 2;
            for (int i = 0; i < m_segmentNumber; ++i)
            {
                m_indexes.Add(s);
                m_indexes.Add(s + 3);
                m_indexes.Add(s + 1);
                m_indexes.Add(s);
                m_indexes.Add(s + 2);
                m_indexes.Add(s + 3);
                s += 2;
            }
        }

        protected override void updateUV()
        {
            UIAtlasSprite bs = null;
            UIAtlasSprite ts = null;
            UIAtlasSprite ss = null;
            if (m_atlas != null)
            {
                bs = m_atlas.getSprite(m_bottomSprite);
                ts = m_atlas.getSprite(m_topSprite);
                ss = m_atlas.getSprite(m_wallSprite);
            }
            Rect br = bs == null ? (new Rect(0, 0, 1, 1)) : bs.m_uv;
            Rect tr = ts == null ? (new Rect(0, 0, 1, 1)) : ts.m_uv;
            Rect wr = ss == null ? (new Rect(0, 0, 1, 1)) : ss.m_uv;
            Vector2 bc = br.center;
            Vector2 tc = tr.center;
            float a = 360.0f / m_segmentNumber;
            float bra = Mathf.Min(br.width, br.height);
            float tra = Mathf.Min(tr.width, tr.height);
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(0.5f, 0, 0);
            for (int i = 0; i < m_segmentNumber; ++i)
            {
                m_uvs.Add(new Vector2(dir.x * bra + bc.x, dir.z * bra + bc.y));
                m_uvs.Add(new Vector2(dir.x * tra + tc.x, dir.z * tra + tc.y));
                dir = q * dir;
            }
            float step = wr.width / m_segmentNumber;
            float x = wr.xMin;
            for (int i = 0; i <= m_segmentNumber; ++i)
            {
                m_uvs.Add(new Vector2(x, wr.yMin));
                m_uvs.Add(new Vector2(x, wr.yMax));
                x += step;
            }
        }

        protected override void updateNormal()
        {
            for (int i = 0; i < m_segmentNumber; ++i)
            {
                m_normals.Add(new Vector3(0, -1, 0));
                m_normals.Add(new Vector3(0, 1, 0));
            }
            float a = 360.0f / m_segmentNumber;
            Quaternion q = Quaternion.Euler(0, a, 0);
            Vector3 dir = new Vector3(1, 0, 0);
            for (int i = 0; i < m_segmentNumber; ++i)
            {
                m_normals.Add(dir);
                m_normals.Add(dir);
                dir = q * dir;
            }
            m_normals.Add(new Vector3(1, 0, 0));
            m_normals.Add(new Vector3(1, 0, 0));
        }

#if UNITY_EDITOR
        public override void onSelectedSprite(UIAtlasSprite s, int id)
        {
            string name = s == null ? "" : s.m_name;
            if (id == 0)
            {
                m_bottomSprite = name;
            }
            else if (id == 1)
            {
                m_topSprite = name;
            }
            else
            {
                m_wallSprite = name;
            }
            EditorChanged = true;
        }

        public override UIAtlas editorGetAtlas()
        {
            return m_atlas;
        }

        public override void getSelectSprite(List<string> labels, List<UIAtlasSprite> values, List<int> ids)
        {
            labels.Add("bottom");
            values.Add(m_atlas == null ? null : m_atlas.getSprite(m_bottomSprite));
            ids.Add(0);
            labels.Add("top");
            values.Add(m_atlas == null ? null : m_atlas.getSprite(m_topSprite));
            ids.Add(1);
            labels.Add("wall");
            values.Add(m_atlas == null ? null : m_atlas.getSprite(m_wallSprite));
            ids.Add(2);
        }

        [UnityEditor.MenuItem("LGFW/Geometry/Cylinder", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<Cylinder>(true);
        }
#endif
    }
}
