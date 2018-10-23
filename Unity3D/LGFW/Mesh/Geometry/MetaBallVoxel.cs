using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MetaBallNode
    {
        public Vector3 m_pos;
        public float m_e;
        public int m_id;

        public void addE(Vector3 v)
        {
            v -= m_pos;
            float s = v.sqrMagnitude;
            if (s == 0)
            {
                m_e = 10000000;
            }
            else
            {
                m_e += 1 / s;
            }
        }
    }

    public class MetaBallEdge
    {
        public MetaBallNode m_node1;
        public MetaBallNode m_node2;

        public static MetaBallEdge createEdge(Dictionary<string, MetaBallEdge> dict, MetaBallNode n1, MetaBallNode n2)
        {
            string id = "";
            if (n1.m_id < n2.m_id)
            {
                id = n1.m_id + " " + n2.m_id;
            }
            else
            {
                id = n2.m_id + " " + n1.m_id;
            }
            MetaBallEdge e = null;
            if (!dict.TryGetValue(id, out e))
            {
                e = new MetaBallEdge(n1, n2);
                dict[id] = e;
            }
            return e;
        }

        public int m_index = -1;
        public Vector3 m_vertex;

        public MetaBallEdge(MetaBallNode n1, MetaBallNode n2)
        {
            m_node1 = n1;
            m_node2 = n2;
        }

        public Vector3 getNormal(float e, out bool has)
        {
            has = true;
            if (m_node1.m_e == e && m_node2.m_e == e)
            {
                has = false;
                return Vector3.zero;
            }
            if (m_node1.m_e < e)
            {
                return m_node1.m_pos - m_vertex;
            }
            if (m_node1.m_e > e)
            {
                return m_vertex - m_node1.m_pos;
            }
            if (m_node2.m_e < e)
            {
                return m_node2.m_pos - m_vertex;
            }
            return m_vertex - m_node2.m_pos;
        }

        public void checkE(float e, int index)
        {
            if (m_node1.m_e < e && m_node2.m_e < e)
            {
                m_index = -1;
                return;
            }
            if (m_node1.m_e > e && m_node2.m_e > e)
            {
                m_index = -1;
                return;
            }
            float f = LMath.lerpValue(m_node1.m_e, m_node2.m_e, e);
            m_vertex = Vector3.Lerp(m_node1.m_pos, m_node2.m_pos, f);
            m_index = index;
        }
    }

    public class MetaBallVoxel
    {

        public MetaBallNode[] m_nodes = new MetaBallNode[8];
        public MetaBallEdge[] m_edges = new MetaBallEdge[12];

        public void initEdge(Dictionary<string, MetaBallEdge> dict)
        {
            m_edges[0] = MetaBallEdge.createEdge(dict, m_nodes[0], m_nodes[1]);
            m_edges[1] = MetaBallEdge.createEdge(dict, m_nodes[1], m_nodes[2]);
            m_edges[2] = MetaBallEdge.createEdge(dict, m_nodes[2], m_nodes[3]);
            m_edges[3] = MetaBallEdge.createEdge(dict, m_nodes[3], m_nodes[0]);

            m_edges[4] = MetaBallEdge.createEdge(dict, m_nodes[4], m_nodes[5]);
            m_edges[5] = MetaBallEdge.createEdge(dict, m_nodes[5], m_nodes[6]);
            m_edges[6] = MetaBallEdge.createEdge(dict, m_nodes[6], m_nodes[7]);
            m_edges[7] = MetaBallEdge.createEdge(dict, m_nodes[7], m_nodes[4]);

            m_edges[8] = MetaBallEdge.createEdge(dict, m_nodes[0], m_nodes[4]);
            m_edges[9] = MetaBallEdge.createEdge(dict, m_nodes[1], m_nodes[5]);
            m_edges[10] = MetaBallEdge.createEdge(dict, m_nodes[2], m_nodes[6]);
            m_edges[11] = MetaBallEdge.createEdge(dict, m_nodes[3], m_nodes[7]);
        }

        public void collectIndex(List<int> l, float e)
        {
            for (int i = 0; i < m_edges.Length; ++i)
            {
                for (int j = i + 1; j < m_edges.Length; ++j)
                {
                    for (int k = j + 1; k < m_edges.Length; ++k)
                    {
                        if (m_edges[i].m_index < 0 || m_edges[j].m_index < 0 || m_edges[k].m_index < 0)
                        {
                            continue;
                        }
                        Vector3 v1 = m_edges[j].m_vertex - m_edges[i].m_vertex;
                        Vector3 v2 = m_edges[k].m_vertex - m_edges[i].m_vertex;
                        v1 = Vector3.Cross(v1, v2);
                        bool has = true;
                        v2 = m_edges[i].getNormal(e, out has);
                        if (!has)
                        {
                            v2 = m_edges[j].getNormal(e, out has);
                            if (!has)
                            {
                                v2 = m_edges[k].getNormal(e, out has);
                            }
                        }
                        float f = Vector3.Dot(v1, v2);
                        if (f >= 0)
                        {
                            l.Add(m_edges[i].m_index);
                            l.Add(m_edges[j].m_index);
                            l.Add(m_edges[k].m_index);
                        }
                        else
                        {
                            l.Add(m_edges[i].m_index);
                            l.Add(m_edges[k].m_index);
                            l.Add(m_edges[j].m_index);
                        }
                    }
                }
            }
        }
    }
}
