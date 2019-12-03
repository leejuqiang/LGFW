using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// This mesh represents a sphere
    /// </summary>
    [ExecuteInEditMode]
    public class Sphere : GeometryMesh
    {

        [SerializeField]
        protected Vector2 m_latitudeRange = new Vector2(0, 1);
        [SerializeField]
        protected Vector2 m_longitudeRange = new Vector2(0, 1);
        [SerializeField]
        protected Vector2Int m_gridNumber = new Vector2Int(10, 10);
        [SerializeField]
        protected float m_radius = 10;
        [SerializeField]
        protected Color m_color = Color.white;
        [SerializeField]
        protected bool m_uvForWholeSphere;
        [SerializeField]
        protected UIAtlas m_atlas;
        [SerializeField]
        [HideInInspector]
        protected string m_sprite;

        protected Vector2 m_latitudeAng;
        protected Vector2 m_longitudeAng;

        /// <summary>
        /// The latitude range of the sphere
        /// </summary>
        /// <value>The latitude range</value>
        public Vector2 LatitudeRange
        {
            get { return m_latitudeRange; }
            set
            {
                clampVector(ref value);
                if (value != m_latitudeRange)
                {
                    onLatitudeChange();
                }
            }
        }

        /// <summary>
        /// The longitude range of the sphere
        /// </summary>
        /// <value>The longitude range</value>
        public Vector2 LongitudeRange
        {
            get { return m_longitudeRange; }
            set
            {
                clampVector(ref value);
                if (value != m_longitudeRange)
                {
                    onLongitudeChange();
                }
            }
        }

        /// <summary>
        /// The radius of the sphere
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
        /// The segment number of longitude and latitude, with larger segment number, the sphere will be more circular
        /// </summary>
        /// <value>The segment number</value>
        public Vector2Int GridNumber
        {
            get { return m_gridNumber; }
            set
            {
                if (m_gridNumber != value)
                {
                    m_gridNumber = value;
                    repaint();
                }
            }
        }

        /// <summary>
        /// The color of the sphere
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
                    m_flag |= FLAG_COLOR;
                }
            }
        }

        /// <summary>
        /// The sprite of the sphere
        /// </summary>
        /// <value>The sprite</value>
        public string Sprite
        {
            get { return m_sprite; }
            set
            {
                if (m_sprite != value)
                {
                    m_sprite = value;
                    m_flag |= FLAG_UV;
                }
            }
        }

        /// <summary>
        /// The UIAtlas this sphere uses 
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
            onLatitudeChange();
            onLongitudeChange();
        }

        protected void clampVector(ref Vector2 v)
        {
            LMath.clampVector(ref v, 0, 1);
            if (v.x > v.y)
            {
                v.x = 0;
            }
        }

        public override void LateUpdate()
        {
            if (m_radius > 0)
            {
                base.LateUpdate();
            }
        }

        protected void onLatitudeChange()
        {
            m_latitudeAng = m_latitudeRange * Mathf.PI;
            repaint();
        }

        protected void onLongitudeChange()
        {
            m_longitudeAng = m_longitudeRange * (Mathf.PI * 2);
            repaint();
        }

        protected override void updateVertex()
        {
            Vector2 step = new Vector2((m_longitudeAng.y - m_longitudeAng.x) / m_gridNumber.x, (m_latitudeAng.y - m_latitudeAng.x) / m_gridNumber.y);
            float angX = m_longitudeAng.x;
            float angY = m_latitudeAng.x;
            for (int i = 0; i <= m_gridNumber.y; ++i)
            {
                for (int j = 0; j <= m_gridNumber.x; ++j)
                {
                    float sinX = Mathf.Sin(angX);
                    float cosX = Mathf.Cos(angX);
                    float sinY = Mathf.Sin(angY);
                    float cosY = Mathf.Cos(angY);
                    m_vertices.Add(new Vector3(-sinY * sinX * m_radius, -cosY * m_radius, m_radius * cosX * sinY));
                    angX += step.x;
                }
                angX = m_longitudeAng.x;
                angY += step.y;
            }
        }

        protected override void updateUV()
        {
            Rect uv = new Rect(0, 0, 1, 1);
            if (m_atlas != null)
            {
                var s = m_atlas.getSprite(m_sprite);
                if (s != null)
                {
                    uv = s.m_uv;
                }
            }
            if (m_uvForWholeSphere)
            {
                float w = uv.width;
                uv.xMin += m_longitudeRange.x * w;
                uv.xMax -= (1 - m_longitudeRange.y) * w;
                float h = uv.height;
                uv.yMin += m_latitudeRange.x * h;
                uv.yMax -= (1 - m_latitudeRange.y) * h;
            }
            Vector2 step = new Vector2(uv.width / m_gridNumber.x, uv.height / m_gridNumber.y);
            float x = uv.xMin;
            float y = uv.yMin;
            for (int i = 0; i <= m_gridNumber.y; ++i)
            {
                for (int j = 0; j <= m_gridNumber.x; ++j)
                {
                    m_uvs.Add(new Vector2(x, y));
                    x += step.x;
                }
                x = uv.xMin;
                y += step.y;
            }
        }

        protected override void updateNormal()
        {
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                Vector3 v = m_vertices[i];
                v /= m_radius;
                m_normals.Add(v);
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
            int len = m_gridNumber.x + 1;
            for (int i = 0; i < m_gridNumber.y; ++i)
            {
                for (int j = 0; j < m_gridNumber.x; ++j)
                {
                    int index = i * len + j;
                    m_indexes.Add(index);
                    m_indexes.Add(index + len);
                    m_indexes.Add(index + len + 1);
                    m_indexes.Add(index);
                    m_indexes.Add(index + len + 1);
                    m_indexes.Add(index + 1);
                }
            }
        }

#if UNITY_EDITOR
        public override void onSelectedSprite(UIAtlasSprite s, int id)
        {
            m_sprite = s == null ? "" : s.m_name;
            EditorChanged = true;
        }

        public override UIAtlas editorGetAtlas()
        {
            return m_atlas;
        }

        public override void getSelectSprite(List<string> labels, List<UIAtlasSprite> values, List<int> ids)
        {
            labels.Add("Sprite");
            values.Add(m_atlas == null ? null : m_atlas.getSprite(m_sprite));
            ids.Add(0);
        }

        [UnityEditor.MenuItem("LGFW/Geometry/Sphere", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<Sphere>(true);
        }
#endif
    }
}
