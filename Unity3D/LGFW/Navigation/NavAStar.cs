using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An a* navigation base class
    /// </summary>
    /// <typeparam name="T">The type of the node</typeparam>
    public class NavAStar<T> : Graph<T> where T : NavAStarNode
    {
        protected T m_start;
        protected T m_end;
        protected float m_stopRange;

        /// <inheritdoc>
        public override void clear()
        {
            base.clear();
            m_start = null;
            m_end = null;
        }

        /// <summary>
        /// Clears the navigation information
        /// </summary>
        public virtual void clearSearch()
        {
            m_openSet.Clear();
            m_openList.Clear();
            m_start = null;
            m_end = null;
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                m_nodes[i].reset();
            }
        }

        /// <summary>
        /// Gets a path to a node based on the navigation information last searching
        /// </summary>
        /// <param name="end">The node</param>
        /// <returns>The list of nodes</returns>
        public List<T> getPath(T end)
        {
            if (end == null)
            {
                return null;
            }
            List<T> p = new List<T>();
            while (end != null)
            {
                p.Add(end);
                end = (T)end.m_parent;
            }
            return p;
        }

        protected bool addToOpenList(T n, T parent, float edgeCost)
        {
            if (n.Visited)
            {
                return false;
            }
            float g = parent.G + n.m_fixCost + edgeCost;
            if (m_openSet.Contains(n))
            {
                if (g < n.G)
                {
                    n.G = g;
                    n.m_parent = parent;
                }
                return false;
            }
            n.m_parent = parent;
            n.G = g;
            m_openSet.Add(n);
            m_openList.Add(n);
            return true;
        }

        protected T getFromOpenList()
        {
            float min = m_openList[0].F;
            int index = 0;
            for (int i = 1; i < m_openList.Count; ++i)
            {
                if (m_openList[i].F < min)
                {
                    min = m_openList[i].F;
                    index = i;
                }
            }
            var ret = m_openList[index];
            int last = m_openList.Count - 1;
            m_openList[index] = m_openList[last];
            m_openList.RemoveAt(last);
            m_openSet.Remove(ret);
            ret.Visited = true;
            return ret;
        }

        protected bool isEnd(T n)
        {
            if (!n.m_accessible)
            {
                return false;
            }
            if (n == m_end)
            {
                return true;
            }
            if (m_stopRange > 0)
            {
                if (n.m_range >= 0 && n.m_range <= m_stopRange)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds a path from start node to end node
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="range">The max range the end of the path from the end node</param>
        /// <returns>The list of nodes</returns>
        public List<T> findPathList(T start, T end, float range = 0)
        {
            T n = findPath(start, end, range);
            return getPath(n);
        }

        /// <summary>
        /// Finds the last node of the path from start node to end node
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="range">The max range the end of the path from the end node</param>
        /// <returns>The last node of the path</returns>
        public virtual T findPath(T start, T end, float range = 0)
        {
            m_stopRange = range;
            clearSearch();
            if (start == null || end == null)
            {
                return null;
            }
            if (!start.m_accessible || !end.m_accessible)
            {
                return null;
            }
            m_start = start;
            m_end = end;
            if (isEnd(start))
            {
                end.m_parent = null;
                return end;
            }
            if (m_stopRange > 0)
            {
                for (int i = 0; i < m_nodes.Count; ++i)
                {
                    m_nodes[i].computeH(m_end);
                    m_nodes[i].computeRange(m_end);
                }
            }
            else
            {
                for (int i = 0; i < m_nodes.Count; ++i)
                {
                    m_nodes[i].computeH(m_end);
                }
            }
            m_start.G = 0;
            m_openSet.Add(m_start);
            m_openList.Add(m_start);
            T n = null;
            while (m_openList.Count > 0)
            {
                n = getFromOpenList();
                for (int i = 0; i < n.m_outEdge.Count; ++i)
                {
                    var nn = (T)n.m_outEdge[i].m_end;
                    if (isEnd(nn))
                    {
                        nn.m_parent = n;
                        n = nn;
                        return nn;
                    }
                    else
                    {
                        addToOpenList(nn, n, n.m_outEdge[i].m_cost);
                    }
                }
            }
            return null;
        }
    }
}