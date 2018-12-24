using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An a* navigation base class
    /// </summary>
    /// <typeparam name="T">The type of the node</typeparam>
    public abstract class NavAStar<T> : MonoBehaviour where T : NavAStarNode, new()
    {

        public const byte EMPTY = 1;
        public const byte FILLED = 0;
        public const byte RANGE = 2;

        protected T[] m_nodes;

        protected NavAStarNode m_openHead;
        protected NavAStarNode m_openEnd;
        protected T[] m_surroundNodes;
        protected int m_surroundNodesCount;
        protected T m_start;
        protected T m_end;
        protected float m_stopRange;

        protected MemoryPool<T> m_pool = new MemoryPool<T>(createNode);

        /// <summary>
        /// Clears the map
        /// </summary>
        public virtual void clearMap()
        {
            if (m_nodes != null)
            {
                for (int i = 0; i < m_nodes.Length; ++i)
                {
                    m_pool.reclaimItem(m_nodes[i]);
                }
            }
            m_nodes = null;
            m_start = null;
            m_end = null;
            m_openEnd = null;
            m_openHead = null;
        }

        /// <summary>
        /// Clears the navigation information
        /// </summary>
        public virtual void clearSearch()
        {
            m_openHead = null;
            m_openEnd = null;
            for (int i = 0; i < m_nodes.Length; ++i)
            {
                m_nodes[i].reset();
            }
            if (m_stopRange > 0)
            {
                initRange();
            }
        }

        protected static T createNode(object data)
        {
            return new T();
        }

        protected abstract void initRange();

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

        protected void removeOpenHead()
        {
            if (m_openHead.m_next == null)
            {
                m_openHead = null;
                m_openEnd = null;
                return;
            }
            m_openHead = m_openHead.m_next;
            m_openHead.m_previous = null;
        }

        protected void moveForward(NavAStarNode n)
        {
            n.m_previous.m_next = n.m_next;
            if (n.m_next != null)
            {
                n.m_next.m_previous = n.m_previous;
            }
            else
            {
                m_openEnd = n.m_previous;
            }
            NavAStarNode p = n.m_previous.m_previous;
            while (p != null)
            {
                if (p.m_finalCost < n.m_finalCost)
                {
                    n.m_previous = p;
                    n.m_next = p.m_next;
                    p.m_next = n;
                    n.m_next.m_previous = n;
                    return;
                }
            }
            n.m_previous = null;
            n.m_next = m_openHead;
            m_openHead.m_previous = n;
            m_openHead = n;
        }

        protected void addToOpenList(NavAStarNode n)
        {
            if (m_openHead == null)
            {
                n.m_next = null;
                n.m_previous = null;
                m_openHead = n;
                m_openEnd = n;
                return;
            }
            NavAStarNode t = m_openHead;
            while (t != null)
            {
                if (t.m_finalCost >= n.m_finalCost)
                {
                    n.m_next = t;
                    n.m_previous = t.m_previous;
                    t.m_previous = n;
                    if (n.m_previous == null)
                    {
                        m_openHead = n;
                    }
                    else
                    {
                        n.m_previous.m_next = n;
                    }
                    return;
                }
                t = t.m_next;
            }
            n.m_next = null;
            n.m_previous = m_openEnd;
            m_openEnd.m_next = n;
            m_openEnd = n;
        }

        protected abstract void findSurroundNodes(T n);
        protected abstract float computeCost(T n);
        protected abstract float computeG(T n, T parent);

        protected void searchNode(T n)
        {
            findSurroundNodes(n);
            for (int i = 0; i < m_surroundNodesCount; ++i)
            {
                T t = m_surroundNodes[i];
                if (t == null)
                {
                    continue;
                }
                if (t == m_end)
                {
                    t.m_parent = n;
                    m_openHead = t;
                    return;
                }
                else
                {
                    if (t.Clear && !t.m_close)
                    {
                        if (t.m_g < 0)
                        {
                            t.m_parent = n;
                            t.m_g = computeG(t, n);
                            if (t.m_cost < 0)
                            {
                                t.m_cost = computeCost(t);
                            }
                            t.computeCost();
                            addToOpenList(t);
                        }
                        else
                        {
                            float g = computeG(t, n);
                            if (g < t.m_g)
                            {
                                t.m_parent = n;
                                t.m_g = g;
                                t.computeCost();
                                moveNodeInList(t);
                            }
                        }
                    }
                }
            }
        }

        protected void moveNodeInList(NavAStarNode n)
        {
            if (m_openHead.m_next == null)
            {
                return;
            }
            if (n.m_previous != null && n.m_finalCost < n.m_previous.m_finalCost)
            {
                moveForward(n);
            }
        }

        protected abstract float getRangeOfNodes(T node1, T node2);

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
            m_start = start;
            m_end = end;
            m_stopRange = range;
            clearSearch();
            if (start == null || end == null)
            {
                return null;
            }
            if (m_stopRange > 0)
            {
                float r = getRangeOfNodes(start, end);
                if (r <= m_stopRange)
                {
                    start.m_parent = null;
                    return start;
                }
            }
            else
            {
                if (!start.Clear || !end.Clear)
                {
                    return null;
                }
                if (start == end)
                {
                    end.m_parent = null;
                    return end;
                }
            }

            start.m_close = true;
            addToOpenList(start);
            while (m_openHead != null)
            {
                NavAStarNode n = m_openHead;
                if (n == end)
                {
                    break;
                }
                removeOpenHead();
                n.m_close = true;
                searchNode((T)n);
            }
            if (end.m_parent == null)
            {
                return null;
            }
            if (m_stopRange > 0)
            {
                end = (T)end.m_parent;
                while (end != start && getRangeOfNodes(end, m_end) < m_stopRange)
                {
                    end = (T)end.m_parent;
                }
            }
            return end;
        }
    }
}