using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A graph
    /// </summary>
    /// <typeparam name="T">The type of the node in this graph</typeparam>
    public class Graph<T> where T : GraphNode
    {
        /// <summary>
        /// All the nodes in the graph
        /// </summary>
        public List<T> m_nodes;
        /// <summary>
        /// All the edges in the graph
        /// </summary>
        public List<GraphEdge<T>> m_edges;
        protected List<T> m_openList;
        protected bool m_isDoubleDirection;

        /// <summary>
        /// If the graph is a double direction graph
        /// </summary>
        /// <value></value>
        public bool IsDoubleDirection
        {
            get { return m_isDoubleDirection; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doubleDirection">If the graph is a double direction graph</param>
        public Graph(bool doubleDirection)
        {
            m_isDoubleDirection = doubleDirection;
            m_nodes = new List<T>();
            m_edges = new List<GraphEdge<T>>();
            m_openList = new List<T>();
        }

        /// <summary>
        /// Clears this graph
        /// </summary>
        public virtual void clear()
        {
            m_nodes.Clear();
            m_edges.Clear();
            m_openList.Clear();
        }

        /// <summary>
        /// Adds an edge to the graph
        /// </summary>
        /// <param name="from">The start node of the edge</param>
        /// <param name="to">The end node of the edge</param>
        /// <param name="cost">The cost of the edge</param>
        public void addEdge(T from, T to, float cost)
        {
            var e = new GraphEdge<T>();
            e.m_from = from;
            e.m_to = to;
            e.m_cost = cost;
        }

        /// <summary>
        /// Adds a node to the graph
        /// </summary>
        /// <param name="node">The node</param>
        public void addNode(T node)
        {
            m_nodes.Add(node);
        }

        /// <summary>
        /// If you only add edges, call this to initialize the graph
        /// </summary>
        public void initNodeFromEdge()
        {
            m_nodes.Clear();
            HashSet<GraphNode> nodes = new HashSet<GraphNode>();
            for (int i = 0; i < m_edges.Count; ++i)
            {
                var e = m_edges[i];
                if (!nodes.Contains(e.m_from))
                {
                    nodes.Add(e.m_from);
                    m_nodes.Add(e.m_from);
                }
                if (!nodes.Contains(e.m_to))
                {
                    nodes.Add(e.m_to);
                    m_nodes.Add(e.m_to);
                }
                if (m_isDoubleDirection)
                {
                    e.m_from.addToConnected(e.m_to);
                    e.m_to.addToConnected(e.m_from);
                }
                else
                {
                    e.m_from.addOutEdge(e.m_to, e.m_cost);
                    e.m_to.addInEdge(e.m_from, e.m_cost);
                }
            }
            if (m_isDoubleDirection)
            {
                for (int i = 0; i < m_nodes.Count; ++i)
                {
                    m_nodes[i].createEdgeFromConnected();
                }
            }
        }

        /// <summary>
        /// If you only add nodes, call this to initialize the graph
        /// </summary>
        public void initEdgeFromNode()
        {
            m_edges.Clear();
            if (m_isDoubleDirection)
            {
                for (int i = 0; i < m_nodes.Count; ++i)
                {
                    for (int j = 0; j < m_nodes[i].m_outEdge.Count; ++j)
                    {
                        m_nodes[i].addToConnected(m_nodes[i].m_outEdge[j].m_end);
                    }
                    for (int j = 0; j < m_nodes[i].m_inEdge.Count; ++j)
                    {
                        m_nodes[i].addToConnected(m_nodes[i].m_inEdge[j].m_end);
                    }
                    m_nodes[i].createEdgeFromConnected();
                }
            }
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                for (int j = 0; j < m_nodes[i].m_outEdge.Count; ++j)
                {
                    var ne = m_nodes[i].m_outEdge[j];
                    var e = new GraphEdge<T>();
                    e.m_from = m_nodes[i];
                    e.m_to = (T)ne.m_end;
                    e.m_cost = ne.m_cost;
                    m_edges.Add(e);
                }
            }
        }

        protected virtual bool tryToAddToOpenList(T n)
        {
            if (!n.Visited)
            {
                m_openList.Add(n);
                n.Visited = true;
                return true;
            }
            return false;
        }

        protected virtual T removeFromOpenList()
        {
            int i = m_openList.Count - 1;
            var ret = m_openList[i];
            m_openList.RemoveAt(i);
            return ret;
        }

        protected bool isFullyConnectedFromNode(T node)
        {
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                m_nodes[i].Visited = false;
            }
            m_openList.Clear();
            tryToAddToOpenList(node);
            while (m_openList.Count > 0)
            {
                var n = removeFromOpenList();
                for (int i = 0; i < n.m_outEdge.Count; ++i)
                {
                    tryToAddToOpenList((T)n.m_outEdge[i].m_end);
                }
            }
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                if (!m_nodes[i].Visited)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// If from a node in the graph, each node can be visited, then this graph is fully connected
        /// </summary>
        /// <returns>True if the graph is fully connected</returns>
        public virtual bool isFullyConnected()
        {
            if (m_nodes.Count <= 0)
            {
                return false;
            }
            if (m_isDoubleDirection)
            {
                return isFullyConnectedFromNode(m_nodes[0]);
            }
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                if (!isFullyConnectedFromNode(m_nodes[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
