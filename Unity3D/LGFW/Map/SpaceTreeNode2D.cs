using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The node for a 2D space tree
    /// </summary>
    public class SpaceTreeNode2D : DecisionTreeNode<ISpaceTreeData2D>
    {
        protected Vector2 m_center;

        protected Vector2 m_minPoint;
        protected Vector2 m_maxPoint;
        protected SpaceTreeNode2D[] m_neighbors;
        protected List<SpaceTreeNode2D> m_crossNodes;

        public Vector2Int m_index;

        public SpaceTreeNode2D Parent
        {
            get { return (SpaceTreeNode2D)m_parent; }
        }

        public Vector2 Center
        {
            get { return m_center; }
        }

        public SpaceTreeNode2D getNeighbor(int index)
        {
            return m_neighbors[index];
        }

        public void setNeighbor(int index, SpaceTreeNode2D n)
        {
            m_neighbors[index] = n;
        }

        public void initLeaf()
        {
            m_neighbors = new SpaceTreeNode2D[4];
            m_data = new HashSet<ISpaceTreeData2D>();
        }

        public SpaceTreeNode2D(Vector2 halfSize, Vector2 center, int depth, int maxDepth) : base(depth)
        {
            m_center = center;
            m_minPoint = m_center - halfSize;
            m_maxPoint = m_center + halfSize;
            m_crossNodes = new List<SpaceTreeNode2D>();
            init(maxDepth, halfSize);
        }

        public SpaceTreeNode2D getLeafWithoutBound(Vector2 p)
        {
            SpaceTreeNode2D n = getChildByDataWithoutChecking(p);
            while (!n.IsLeaf)
            {
                n = n.getChildByDataWithoutChecking(p);
            }
            return n;
        }

        public override DecisionTreeNode<ISpaceTreeData2D> getLeaf(ISpaceTreeData2D data)
        {
            Vector2 p = data.getPosition();
            if (p.x < m_minPoint.x || p.x > m_maxPoint.x)
            {
                return null;
            }
            if (p.y < m_minPoint.y || p.y > m_maxPoint.y)
            {
                return null;
            }
            return getLeafWithoutBound(p);
        }

        protected SpaceTreeNode2D getChildByDataWithoutChecking(Vector2 data)
        {
            if (data.x <= m_center.x)
            {
                if (data.y <= m_center.y)
                {
                    return (SpaceTreeNode2D)m_children[0];
                }
                else
                {
                    return (SpaceTreeNode2D)m_children[2];
                }
            }
            else
            {
                if (data.y <= m_center.y)
                {
                    return (SpaceTreeNode2D)m_children[1];
                }
                else
                {
                    return (SpaceTreeNode2D)m_children[3];
                }
            }
        }

        public override DecisionTreeNode<ISpaceTreeData2D> addData(ISpaceTreeData2D data)
        {
            return null;
        }

        public void init(int maxDepth, Vector2 halfSize)
        {
            if (m_depth < maxDepth)
            {
                Vector2 hs = halfSize * 0.5f;
                Vector2 c = m_center - hs;
                Vector2 tc = c;
                for (int y = 0; y < 2; ++y)
                {
                    tc.x = c.x;
                    for (int x = 0; x < 2; ++x)
                    {
                        SpaceTreeNode2D n = new SpaceTreeNode2D(hs, tc, m_depth + 1, maxDepth);
                        m_children.Add(n);
                        tc.x += halfSize.x;
                    }
                    tc.y += halfSize.y;
                }
            }
        }
    }
}
