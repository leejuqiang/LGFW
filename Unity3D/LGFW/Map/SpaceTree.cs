using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A 3D space tree
    /// </summary>
    public class SpaceTree : MonoBehaviour
    {

        public const int XMIN = 0;
        public const int XMAX = 1;
        public const int YMIN = 2;
        public const int YMAX = 3;
        public const int ZMIN = 4;
        public const int ZMAX = 5;

        /// <summary>
        /// The size of the space
        /// </summary>
        public Vector3 m_size;
        /// <summary>
        /// The center of the space
        /// </summary>
        public Vector3 m_center;
        /// <summary>
        /// The depth of the tree
        /// </summary>
        public int m_maxDepth;

        private SpaceTreeNode m_root;
        private Dictionary<ISpaceTreeData, HashSet<SpaceTreeNode>> m_data;

        private List<SpaceTreeNode> m_leaves = new List<SpaceTreeNode>();

        private List<ISpaceTreeData> m_collision = new List<ISpaceTreeData>();

        void Awake()
        {
            m_data = new Dictionary<ISpaceTreeData, HashSet<SpaceTreeNode>>();
            m_root = new SpaceTreeNode(m_size / 2, m_center, 0, m_maxDepth);
            Queue<SpaceTreeNode> l = new Queue<SpaceTreeNode>();
            l.Enqueue(m_root);
            while (l.Count > 0)
            {
                SpaceTreeNode n = l.Dequeue();
                if (n.isLeaf)
                {
                    m_leaves.Add(n);
                }
                for (int i = 0; i < n.Children.Count; ++i)
                {
                    l.Enqueue((SpaceTreeNode)n.Children[i]);
                }
            }
            m_leaves.Sort(sortLeaves);
            int len = (int)Mathf.Pow(2, m_maxDepth);
            for (int z = 0, i = 0; z < len; ++z)
            {
                for (int y = 0; y < len; ++y)
                {
                    for (int x = 0; x < len; ++x, ++i)
                    {
                        m_leaves[i].initNeighbor();
                        m_leaves[i].m_index.Set(x, y, z);
                    }
                }
            }
            int last = len - 1;
            int yOffset = len;
            int zOffset = len * len;
            for (int z = 0, i = 0; z < len; ++z)
            {
                for (int y = 0; y < len; ++y)
                {
                    for (int x = 0; x < len; ++x, ++i)
                    {
                        m_leaves[i].setNeighbor(XMIN, x > 0 ? m_leaves[i - 1] : null);
                        m_leaves[i].setNeighbor(XMAX, x < last ? m_leaves[i + 1] : null);
                        m_leaves[i].setNeighbor(YMIN, y > 0 ? m_leaves[i - yOffset] : null);
                        m_leaves[i].setNeighbor(YMAX, y < last ? m_leaves[i + yOffset] : null);
                        m_leaves[i].setNeighbor(ZMIN, z > 0 ? m_leaves[i - zOffset] : null);
                        m_leaves[i].setNeighbor(ZMAX, z < last ? m_leaves[i + zOffset] : null);
                    }
                }
            }
        }

        public void addToTree(ISpaceTreeData data)
        {
            if (m_data.ContainsKey(data))
            {
                return;
            }
            m_data[data] = new HashSet<SpaceTreeNode>();
        }

        /// <summary>
        /// Gets the nodes of a data
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The nodes</returns>
        public HashSet<SpaceTreeNode> getNodesForData(ISpaceTreeData data)
        {
            HashSet<SpaceTreeNode> set = null;
            m_data.TryGetValue(data, out set);
            return set;
        }

        /// <summary>
        /// Gets all the data can has collision with the data
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>All the possible collision data</returns>
        public List<ISpaceTreeData> getCollision(ISpaceTreeData data)
        {
            m_collision.Clear();
            HashSet<SpaceTreeNode> set = null;
            if (m_data.TryGetValue(data, out set))
            {
                foreach (SpaceTreeNode n in set)
                {
                    foreach (ISpaceTreeData d in n.AllData)
                    {
                        m_collision.Add(d);
                    }
                }
            }
            return m_collision;
        }

        /// <summary>
        /// Removes the data from the tree
        /// </summary>
        /// <param name="data">The data</param>
        public void removeFromTree(ISpaceTreeData data)
        {
            HashSet<SpaceTreeNode> set = null;
            if (!m_data.TryGetValue(data, out set))
            {
                return;
            }
            foreach (SpaceTreeNode n in set)
            {
                n.removeData(data);
            }
            m_data.Remove(data);
        }

        /// <summary>
        /// Updates a data's position in the space
        /// </summary>
        /// <param name="data">The data</param>
        public void updateData(ISpaceTreeData data)
        {
            HashSet<SpaceTreeNode> set = null;
            if (!m_data.TryGetValue(data, out set))
            {
                return;
            }
            foreach (SpaceTreeNode n in set)
            {
                n.removeData(data);
            }
            set.Clear();
            SpaceTreeNode min = m_root.getLeafWithoutBound(data.getMinPoint());
            SpaceTreeNode max = m_root.getLeafWithoutBound(data.getMaxPoint());
            SpaceTreeNode xn = min;
            SpaceTreeNode yn = min;
            SpaceTreeNode zn = min;
            for (int z = min.m_index.z; z <= max.m_index.z; ++z)
            {
                yn = zn;
                for (int y = min.m_index.y; y <= max.m_index.y; ++y)
                {
                    xn = yn;
                    for (int x = min.m_index.x; x <= max.m_index.x; ++x)
                    {
                        set.Add(xn);
                        xn.addData(data);
                        xn = xn.getNeighbor(XMAX);
                    }
                    yn = yn.getNeighbor(YMAX);
                }
                zn = zn.getNeighbor(ZMAX);
            }
        }

        private int sortLeaves(SpaceTreeNode l, SpaceTreeNode r)
        {
            Vector3 c1 = l.Center;
            Vector3 c2 = r.Center;
            if (c1.z < c2.z)
            {
                return -1;
            }
            else if (c1.z > c2.z)
            {
                return 1;
            }
            else
            {
                if (c1.y < c2.y)
                {
                    return -1;
                }
                else if (c1.y > c2.y)
                {
                    return 1;
                }
                else
                {
                    if (c1.x < c2.x)
                    {
                        return -1;
                    }
                    else if (c1.x > c2.x)
                    {
                        return 1;
                    }
                    return 0;
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Map/Space Tree 3D", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<SpaceTree>(true);
        }
#endif
    }
}
