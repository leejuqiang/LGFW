using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An edge in a graph
    /// </summary>
    /// <typeparam name="T">The type of the node in the graph</typeparam>
    public class GraphEdge<T> where T : GraphNode
    {
        /// <summary>
        /// The start node of the edge
        /// </summary>
        public T m_from;
        /// <summary>
        /// The end node of the edge
        /// </summary>
        public T m_to;
        /// <summary>
        /// The cost of the edge
        /// </summary>
        public float m_cost;
    }

    /// <summary>
    /// The edge from a node
    /// </summary>
    public class GraphEdgeFromNode
    {
        /// <summary>
        /// The other end of the edge
        /// </summary>
        public GraphNode m_end;
        /// <summary>
        /// The cost of the edge
        /// </summary>
        public float m_cost;
    }

    /// <summary>
    /// The base class of the node in the graph
    /// </summary>
    public class GraphNode
    {
        protected int m_id;
        /// <summary>
        /// The edges go out from the node
        /// </summary>
        public List<GraphEdgeFromNode> m_outEdge;
        /// <summary>
        /// The edges come into the node
        /// </summary>
        public List<GraphEdgeFromNode> m_inEdge;
        protected bool m_visited;

        /// <summary>
        /// The id of the node
        /// </summary>
        /// <value></value>
        public int ID
        {
            get { return m_id; }
        }

        /// <summary>
        /// If the node has been visited
        /// </summary>
        /// <value></value>
        public virtual bool Visited
        {
            get { return m_visited; }
            set { m_visited = value; }
        }

        public GraphNode(int id)
        {
            m_id = id;
            m_outEdge = new List<GraphEdgeFromNode>();
            m_inEdge = new List<GraphEdgeFromNode>();
        }

        /// <summary>
        /// Adds an out edge
        /// </summary>
        /// <param name="to">The other end</param>
        /// <param name="cost">The cost</param>
        public void addOutEdge(GraphNode to, float cost)
        {
            GraphEdgeFromNode e = new GraphEdgeFromNode();
            e.m_end = to;
            e.m_cost = cost;
            m_outEdge.Add(e);
        }

        /// <summary>
        /// Adds an in edge
        /// </summary>
        /// <param name="from">The other end</param>
        /// <param name="cost">The cost</param>
        public void addInEdge(GraphNode from, float cost)
        {
            GraphEdgeFromNode e = new GraphEdgeFromNode();
            e.m_end = from;
            e.m_cost = cost;
            m_inEdge.Add(e);
        }

        /// <summary>
        /// Adds an out edge and an in edge
        /// </summary>
        /// <param name="node">The other end of the edge</param>
        /// <param name="cost">The cost</param>
        public void addEdge(GraphNode node, float cost)
        {
            GraphEdgeFromNode e = new GraphEdgeFromNode();
            e.m_end = node;
            e.m_cost = cost;
            m_inEdge.Add(e);
            e = new GraphEdgeFromNode();
            e.m_end = node;
            e.m_cost = cost;
            m_outEdge.Add(e);
        }

        /// <summary>
        /// Gets an out edge to n
        /// </summary>
        /// <param name="n">The other end</param>
        /// <returns>The out edge</returns>
        public GraphEdgeFromNode getEdgeTo(GraphNode n)
        {
            for (int i = 0; i < m_outEdge.Count; ++i)
            {
                if (m_outEdge[i].m_end == n)
                {
                    return m_outEdge[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets an in edge from n
        /// </summary>
        /// <param name="n">The other end</param>
        /// <returns>The in edge</returns>
        public GraphEdgeFromNode getEdgeFrom(GraphNode n)
        {
            for (int i = 0; i < m_inEdge.Count; ++i)
            {
                if (m_inEdge[i].m_end == n)
                {
                    return m_inEdge[i];
                }
            }
            return null;
        }
    }
}
