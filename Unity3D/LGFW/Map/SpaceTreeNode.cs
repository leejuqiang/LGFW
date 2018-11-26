using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The node for a 3D space tree
    /// </summary>
    public class SpaceTreeNode : DecisionTreeNode<ISpaceTreeData>
    {
        protected Vector3 m_center;

        protected Vector3 m_minPoint;
        protected Vector3 m_maxPoint;
        protected SpaceTreeNode[] m_neighbors;
        protected List<SpaceTreeNode> m_crossNodes;

        public Vector3Int m_index;

        public SpaceTreeNode Parent
        {
            get { return (SpaceTreeNode)m_parent; }
        }

        public Vector3 Center
        {
            get { return m_center; }
        }

        public SpaceTreeNode getNeighbor(int index)
        {
            return m_neighbors[index];
        }

        public void setNeighbor(int index, SpaceTreeNode n)
        {
            m_neighbors[index] = n;
        }

        public void initNeighbor()
        {
            m_neighbors = new SpaceTreeNode[6];
        }

        public SpaceTreeNode(Vector3 halfSize, Vector3 center, int depth, int maxDepth) : base(depth)
        {
            m_center = center;
            m_minPoint = m_center - halfSize;
            m_maxPoint = m_center + halfSize;
            m_crossNodes = new List<SpaceTreeNode>();
            init(maxDepth, halfSize);
        }

        public SpaceTreeNode getLeafWithoutBound(Vector3 p)
        {
            SpaceTreeNode n = getChildByDataWithoutChecking(p);
            while (!n.isLeaf)
            {
                n = n.getChildByDataWithoutChecking(p);
            }
            return n;
        }

        public override DecisionTreeNode<ISpaceTreeData> getLeaf(ISpaceTreeData data)
        {
            Vector3 p = data.getPosition();
            if (p.x < m_minPoint.x || p.x > m_maxPoint.x)
            {
                return null;
            }
            if (p.y < m_minPoint.y || p.y > m_maxPoint.y)
            {
                return null;
            }
            if (p.z < m_minPoint.z || p.z > m_maxPoint.z)
            {
                return null;
            }
            return getLeafWithoutBound(p);
        }

        protected SpaceTreeNode getChildByDataWithoutChecking(Vector3 data)
        {
            if (data.x <= m_center.x)
            {
                if (data.y <= m_center.y)
                {
                    if (data.z <= m_center.z)
                    {
                        return (SpaceTreeNode)m_children[0];
                    }
                    else
                    {
                        return (SpaceTreeNode)m_children[4];
                    }
                }
                else
                {
                    if (data.z <= m_center.z)
                    {
                        return (SpaceTreeNode)m_children[2];
                    }
                    else
                    {
                        return (SpaceTreeNode)m_children[6];
                    }
                }
            }
            else
            {
                if (data.y <= m_center.y)
                {
                    if (data.z <= m_center.z)
                    {
                        return (SpaceTreeNode)m_children[1];
                    }
                    else
                    {
                        return (SpaceTreeNode)m_children[5];
                    }
                }
                else
                {
                    if (data.z <= m_center.z)
                    {
                        return (SpaceTreeNode)m_children[3];
                    }
                    else
                    {
                        return (SpaceTreeNode)m_children[7];
                    }
                }
            }
        }

        public override DecisionTreeNode<ISpaceTreeData> addData(ISpaceTreeData data)
        {
            return null;
        }

        public void init(int maxDepth, Vector3 halfSize)
        {
            if (m_depth < maxDepth)
            {
                Vector3 hs = halfSize * 0.5f;
                Vector3 c = m_center - hs;
                Vector3 tc = c;
                for (int t = 0; t < 2; ++t)
                {
                    tc.x = c.x;
                    tc.y = c.y;
                    for (int y = 0; y < 2; ++y)
                    {
                        tc.x = c.x;
                        for (int x = 0; x < 2; ++x)
                        {
                            SpaceTreeNode n = new SpaceTreeNode(hs, tc, m_depth + 1, maxDepth);
                            m_children.Add(n);
                            tc.x += halfSize.x;
                        }
                        tc.y += halfSize.y;
                    }
                    tc.z += halfSize.z;
                }
            }
        }
    }
}
