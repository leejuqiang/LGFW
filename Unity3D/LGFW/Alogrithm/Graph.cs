using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class Graph<T>
    {
        public List<GraphNode<T>> m_nodes;
        public List<GraphEdge<T>> m_edges;

        public Graph()
        {
            m_nodes = new List<GraphNode<T>>();
            m_edges = new List<GraphEdge<T>>();
        }

        public void initEdgesInNode()
        {
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                m_nodes[i].m_edges.Clear();
            }
            for (int i = 0; i < m_edges.Count; ++i)
            {
                GraphEdge<T> e = m_edges[i];
                e.m_from.addEdge(e.m_to, e.m_cost);
                if (!e.m_isDirect)
                {
                    e.m_to.addEdge(e.m_from, e.m_cost);
                }
            }
        }
    }
}
