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

        protected HashSet<T> m_openSet;
        protected List<T> m_openList;

        public Graph()
        {
            m_nodes = new List<T>();
            m_edges = new List<GraphEdge<T>>();
            m_openSet = new HashSet<T>();
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
            m_openSet.Clear();
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
            HashSet<int> nodes = new HashSet<int>();
            for (int i = 0; i < m_edges.Count; ++i)
            {
                var e = m_edges[i];
                if (!nodes.Contains(e.m_from.ID))
                {
                    nodes.Add(e.m_from.ID);
                    m_nodes.Add(e.m_from);
                }
                if (!nodes.Contains(e.m_to.ID))
                {
                    nodes.Add(e.m_to.ID);
                    m_nodes.Add(e.m_to);
                }
                e.m_from.addOutEdge(e.m_to, e.m_cost);
                e.m_to.addInEdge(e.m_from, e.m_cost);
            }
        }

        /// <summary>
        /// If you only add nodes, call this to initialize the graph
        /// </summary>
        public void initEdgeFromNode()
        {
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
            if (!n.Visited && !m_openSet.Contains(n))
            {
                m_openList.Add(n);
                m_openSet.Add(n);
                return true;
            }
            return false;
        }

        protected virtual T removeFromOpenList()
        {
            int i = m_openList.Count - 1;
            var ret = m_openList[i];
            m_openSet.Remove(ret);
            m_openList.RemoveAt(i);
            ret.Visited = true;
            return ret;
        }

        protected bool isFullyConnectedFromNode(T node)
        {
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                m_nodes[i].Visited = false;
            }
            m_openList.Clear();
            m_openSet.Clear();
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
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                if (isFullyConnectedFromNode(m_nodes[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
