using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A space to render a 3D metaball effect
    /// </summary>
    [ExecuteInEditMode]
    public class MetaBall : GeometryMesh
    {

        /// <summary>
        /// The size of the space, the space will be a cube
        /// </summary>
        public float m_spaceSize = 100;
        /// <summary>
        /// The split number for each dimension
        /// </summary>
        public int m_spaceDivid = 10;
        /// <summary>
        /// The initial value of each point in the space
        /// </summary>
        public float m_energy = 100;
        /// <summary>
        /// The transforms of all balls
        /// </summary>
        public Transform[] m_balls;

        private MetaBallNode[,,] m_nodes;
        private MetaBallVoxel[,,] m_voxels;
        private Dictionary<string, MetaBallEdge> m_edges = new Dictionary<string, MetaBallEdge>();
        private Vector3[] m_ballPoss;
        private float m_e;

        /// <summary>
        /// Computes the energy of each voxel
        /// </summary>
        public void computeE()
        {
            m_e = 1 / m_energy / m_energy;
            int size = m_spaceDivid + 1;
            for (int i1 = 0; i1 < size; ++i1)
            {
                for (int i2 = 0; i2 < size; ++i2)
                {
                    for (int i3 = 0; i3 < size; ++i3)
                    {
                        m_nodes[i3, i2, i1].m_e = 0;
                    }
                }
            }
            m_ballPoss = new Vector3[m_balls.Length];
            Transform t = this.transform;
            for (int i = 0; i < m_balls.Length; ++i)
            {
                Vector3 v = m_balls[i].position;
                v = t.InverseTransformPoint(v);
                m_ballPoss[i] = v;
                for (int i1 = 0; i1 < size; ++i1)
                {
                    for (int i2 = 0; i2 < size; ++i2)
                    {
                        for (int i3 = 0; i3 < size; ++i3)
                        {
                            m_nodes[i3, i2, i1].addE(v);
                        }
                    }
                }
            }
        }

        protected override void updateVertex()
        {
            foreach (MetaBallEdge me in m_edges.Values)
            {
                me.checkE(m_e, m_vertices.Count);
                if (me.m_index >= 0)
                {
                    m_vertices.Add(me.m_vertex);
                }
            }
        }

        protected override void updateIndex()
        {
            for (int i1 = 0; i1 < m_spaceDivid; ++i1)
            {
                for (int i2 = 0; i2 < m_spaceDivid; ++i2)
                {
                    for (int i3 = 0; i3 < m_spaceDivid; ++i3)
                    {
                        m_voxels[i3, i2, i1].collectIndex(m_indexs, m_e);
                    }
                }
            }
        }

        protected override void updateNormal()
        {
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                Vector3 v = Vector3.zero;
                for (int j = 0; j < m_ballPoss.Length; ++j)
                {
                    Vector3 n = m_vertices[i] - m_ballPoss[j];
                    float w = 1 / n.sqrMagnitude;
                    n.Normalize();
                    v += n * w;
                }
                v /= m_ballPoss.Length;
                v.Normalize();
                m_normals.Add(v);
            }
        }

        protected override void updateUV()
        {
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                m_uvs.Add(Vector2.zero);
            }
        }

        protected override void updateColor()
        {
            Color32 c = Color.white;
            for (int i = 0; i < m_vertices.Count; ++i)
            {
                m_colors.Add(c);
            }
        }

        public override void reset()
        {
            base.reset();
            initSpace();
        }

        public void initSpace()
        {
            int size = m_spaceDivid + 1;
            m_nodes = new MetaBallNode[size, size, size];
            m_voxels = new MetaBallVoxel[m_spaceDivid, m_spaceDivid, m_spaceDivid];

            float step = m_spaceSize / m_spaceDivid;
            Vector3 start = Vector3.zero;
            start.x = -m_spaceSize / 2;
            start.y = start.x;
            start.z = start.x;
            Vector3 p = start;
            int id = 0;
            for (int i1 = 0; i1 < size; ++i1)
            {
                for (int i2 = 0; i2 < size; ++i2)
                {
                    for (int i3 = 0; i3 < size; ++i3)
                    {
                        MetaBallNode n = new MetaBallNode();
                        n.m_pos = p;
                        n.m_id = id;
                        ++id;
                        p.x += step;
                        m_nodes[i3, i2, i1] = n;
                    }
                    p.x = start.x;
                    p.y += step;
                }
                p.y = start.y;
                p.z += step;
            }
            m_edges.Clear();
            for (int i1 = 0; i1 < m_spaceDivid; ++i1)
            {
                for (int i2 = 0; i2 < m_spaceDivid; ++i2)
                {
                    for (int i3 = 0; i3 < m_spaceDivid; ++i3)
                    {
                        int c = m_edges.Count;
                        MetaBallVoxel v = new MetaBallVoxel();
                        v.m_nodes[0] = m_nodes[i3, i2, i1];
                        v.m_nodes[1] = m_nodes[i3 + 1, i2, i1];
                        v.m_nodes[2] = m_nodes[i3 + 1, i2 + 1, i1];
                        v.m_nodes[3] = m_nodes[i3, i2 + 1, i1];
                        v.m_nodes[4] = m_nodes[i3, i2, i1 + 1];
                        v.m_nodes[5] = m_nodes[i3 + 1, i2, i1 + 1];
                        v.m_nodes[6] = m_nodes[i3 + 1, i2 + 1, i1 + 1];
                        v.m_nodes[7] = m_nodes[i3, i2 + 1, i1 + 1];
                        v.initEdge(m_edges);
                        m_voxels[i3, i2, i1] = v;
                    }
                }
            }
            computeE();
        }

        void OnDrawGizmos()
        {
            Color c = Gizmos.color;
            Gizmos.color = Color.blue;
            Transform t = this.transform;
            Gizmos.DrawWireCube(t.position, new Vector3(m_spaceSize, m_spaceSize, m_spaceSize));
            Gizmos.color = c;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Geometry/MetaBall", false, (int)'m')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<MetaBall>(true);
        }
#endif
    }
}
