using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for all geometry component
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public abstract class GeometryMesh : MonoBehaviour
    {

        protected MeshFilter m_filter;
        protected MeshRenderer m_render;

        protected bool m_hasAwake;
        protected int m_updateFlag;
        protected List<Vector3> m_vertices = new List<Vector3>();
        protected List<Vector2> m_uvs = new List<Vector2>();
        protected List<Color32> m_colors = new List<Color32>();
        protected List<int> m_indexs = new List<int>();
        protected List<Vector3> m_normals = new List<Vector3>();
        protected Mesh m_mesh;

        [SerializeField]
        protected bool m_hasNormal;

        /// <summary>
        /// If the mesh has normal
        /// </summary>
        /// <value>If the mesh has normal</value>
        public bool HasNomral
        {
            get { return m_hasNormal; }
            set
            {
                if (m_hasNormal != value)
                {
                    m_hasNormal = value;
                    m_updateFlag |= UIMesh.FLAG_NORMAL;
                }
            }
        }

        public void Awake()
        {
            if (!m_hasAwake)
            {
                m_hasAwake = true;
                doAwake();
            }
        }

        /// <summary>
        /// Repaint this mesh
        /// </summary>
        public virtual void repaint()
        {
            m_updateFlag = 0xffff;
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
            m_indexs.Clear();
            m_colors.Clear();
        }

        protected virtual void doAwake()
        {
            m_filter = this.GetComponent<MeshFilter>();
            m_render = this.GetComponent<MeshRenderer>();
            m_mesh = new Mesh();
            m_mesh.MarkDynamic();
            m_filter.sharedMesh = m_mesh;
            reset();
        }

        protected virtual void preLateUpdate()
        {
            //todo
        }

        public virtual void LateUpdate()
        {
            if (m_render.sharedMaterial == null)
            {
                return;
            }
            if (m_updateFlag != 0)
            {
                preLateUpdate();
                if ((m_updateFlag & UIMesh.FLAG_VERTEX) > 0)
                {
                    m_vertices.Clear();
                    updateVertex();
                    m_mesh.vertices = m_vertices.ToArray();
                }
                if ((m_updateFlag & UIMesh.FLAG_UV) > 0)
                {
                    m_uvs.Clear();
                    updateUV();
                    m_mesh.uv = m_uvs.ToArray();
                }
                if ((m_updateFlag & UIMesh.FLAG_NORMAL) > 0)
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
                if ((m_updateFlag & UIMesh.FLAG_COLOR) > 0)
                {
                    m_colors.Clear();
                    updateColor();
                    m_mesh.colors32 = m_colors.ToArray();
                }
                if ((m_updateFlag & UIMesh.FLAG_INDEX) > 0)
                {
                    m_indexs.Clear();
                    updateIndex();
                    m_mesh.triangles = m_indexs.ToArray();
                }
                m_updateFlag = 0;
            }
        }

        protected abstract void updateVertex();
        protected abstract void updateUV();
        protected abstract void updateNormal();
        protected abstract void updateColor();
        protected abstract void updateIndex();

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
    }
}
