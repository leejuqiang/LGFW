using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class GraphEdge<T>
    {
        public GraphNode<T> m_from;
        public GraphNode<T> m_to;
        public float m_cost;
        public bool m_isDirect;
    }

    public class GraphEdgeFromNode<T>
    {
        public GraphNode<T> m_end;
        public float m_cost;
    }

    public class GraphNode<T>
    {
        public int m_id;
        public List<GraphEdgeFromNode<T>> m_edges;
        public T m_value;

        public GraphNode(int id)
        {
            m_id = id;
            m_edges = new List<GraphEdgeFromNode<T>>();
        }

        public void addEdge(GraphNode<T> to, float cost)
        {
            GraphEdgeFromNode<T> e = new GraphEdgeFromNode<T>();
            e.m_end = to;
            e.m_cost = cost;
            m_edges.Add(e);
        }
    }
}
