using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace LGFW
{
    /// <summary>
    /// Base class for all geometry component
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class GeometryMesh : BaseMono
    {
        public const int FLAG_VERTEX = 1 << 4;
        public const int FLAG_UV = 1 << 5;
        public const int FLAG_COLOR = 1 << 6;
        public const int FLAG_INDEX = 1 << 7;
        public const int FLAG_NORMAL = 1 << 8;
        public const int FLAG_TIP = 1 << 9;

        protected MeshFilter m_filter;
        protected MeshRenderer m_render;

        protected List<Vector3> m_vertices = new List<Vector3>();
        protected List<Vector2> m_uvs = new List<Vector2>();
        protected List<Color32> m_colors = new List<Color32>();
        protected List<int> m_indexes = new List<int>();
        protected List<Vector3> m_normals = new List<Vector3>();
        protected Mesh m_mesh;

        [SerializeField]
        protected bool m_hasNormal;

#if UNITY_EDITOR
        public bool EditorChanged
        {
            get; set;
        }
#endif
        /// <summary>
        /// If the mesh has normal
        /// </summary>
        /// <value>If the mesh has normal</value>
        public bool HasNormal
        {
            get { return m_hasNormal; }
            set
            {
                if (m_hasNormal != value)
                {
                    m_hasNormal = value;
                    m_flag |= FLAG_NORMAL;
                }
            }
        }

        public MeshFilter Mesh
        {
            get
            {
                if (m_filter == null)
                {
                    m_filter = this.GetComponent<MeshFilter>();
                }
                return m_filter;
            }
        }

        public MeshRenderer Renderer
        {
            get
            {
                if (m_render == null)
                {
                    m_render = this.GetComponent<MeshRenderer>();
                }
                return m_render;
            }
        }

        /// <summary>
        /// Repaint this mesh
        /// </summary>
        public virtual void repaint()
        {
            m_flag |= 0xfff0;
            m_mesh.Clear();
        }

        /// <summary>
        /// Reset this mesh
        /// </summary>
        public virtual void reset()
        {
            Awake();
            repaint();
        }

        protected virtual void clear()
        {
            m_vertices.Clear();
            m_uvs.Clear();
            m_indexes.Clear();
            m_colors.Clear();
        }

        protected override void editorAwake()
        {
            doAwake();
        }

        protected override void doAwake()
        {
            m_mesh = new Mesh();
            m_mesh.MarkDynamic();
            Mesh.sharedMesh = m_mesh;
            reset();
        }

        protected virtual void preLateUpdate()
        {
            //todo
        }

        public virtual void LateUpdate()
        {
            if ((m_flag & 0xfff0) != 0)
            {
                preLateUpdate();
                if ((m_flag & FLAG_VERTEX) != 0)
                {
                    m_vertices.Clear();
                    updateVertex();
                    m_mesh.vertices = m_vertices.ToArray();
                }
                if ((m_flag & FLAG_UV) != 0)
                {
                    m_uvs.Clear();
                    updateUV();
                    m_mesh.uv = m_uvs.ToArray();
                }
                if ((m_flag & FLAG_NORMAL) != 0)
                {
                    if (m_hasNormal)
                    {
                        m_normals.Clear();
                        updateNormal();
                        m_mesh.normals = m_normals.ToArray();
                    }
                    else
                    {
                        m_mesh.normals = null;
                    }
                }
                if ((m_flag & FLAG_COLOR) != 0)
                {
                    m_colors.Clear();
                    updateColor();
                    m_mesh.colors32 = m_colors.ToArray();
                }
                if ((m_flag & FLAG_INDEX) != 0)
                {
                    m_indexes.Clear();
                    updateIndex();
                    m_mesh.triangles = m_indexes.ToArray();
                }
                m_flag &= 0xf;
            }
        }

        protected abstract void updateVertex();
        protected abstract void updateUV();
        protected abstract void updateNormal();
        protected abstract void updateColor();
        protected abstract void updateIndex();


#if UNITY_EDITOR

        public virtual void onSelectedSprite(UIAtlasSprite s, int id)
        {

        }

        public virtual void onEditorChanged()
        {

        }
        protected void testDrawNormal(float len)
        {
            if (!m_hasNormal || m_vertices.Count != m_normals.Count)
            {
                return;
            }
            Transform t = this.transform;
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                Vector3 p1 = t.TransformPoint(m_vertices[i]);
                Vector3 p2 = m_vertices[i] + m_normals[i] * len;
                p2 = t.TransformPoint(p2);
                Gizmos.DrawLine(p1, p2);
            }
        }

        public virtual UIAtlas editorGetAtlas()
        {
            return null;
        }

        public virtual void getSelectSprite(List<string> labels, List<UIAtlasSprite> values, List<int> ids)
        {

        }
#endif
    }
}
