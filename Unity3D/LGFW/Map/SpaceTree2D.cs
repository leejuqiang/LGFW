using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A 2D space tree
    /// </summary>
    public class SpaceTree2D : MonoBehaviour
    {

        /// <summary>
        /// The size of the space
        /// </summary>
        public Vector2 m_size;
        /// <summary>
        /// The center of the space
        /// </summary>
        public Vector2 m_center;
        /// <summary>
        /// The depth of the tree
        /// </summary>
        public int m_maxDepth;

        private SpaceTreeNode2D m_root;
        private Dictionary<ISpaceTreeData2D, HashSet<SpaceTreeNode2D>> m_data;

        private List<SpaceTreeNode2D> m_leaves = new List<SpaceTreeNode2D>();

        private List<ISpaceTreeData2D> m_collision = new List<ISpaceTreeData2D>();

        void Awake()
        {
            m_data = new Dictionary<ISpaceTreeData2D, HashSet<SpaceTreeNode2D>>();
            m_root = new SpaceTreeNode2D(m_size / 2, m_center, 0, m_maxDepth);
            Queue<SpaceTreeNode2D> l = new Queue<SpaceTreeNode2D>();
            l.Enqueue(m_root);
            while (l.Count > 0)
            {
                SpaceTreeNode2D n = l.Dequeue();
                if (n.IsLeaf)
                {
                    m_leaves.Add(n);
                }
                for (int i = 0; i < n.Children.Count; ++i)
                {
                    l.Enqueue((SpaceTreeNode2D)n.Children[i]);
                }
            }
            m_leaves.Sort(sortLeaves);
            int len = (int)Mathf.Pow(2, m_maxDepth);
            for (int y = 0, i = 0; y < len; ++y)
            {
                for (int x = 0; x < len; ++x, ++i)
                {
                    m_leaves[i].initLeaf();
                    m_leaves[i].m_index.Set(x, y);
                }
            }
            int last = len - 1;
            for (int y = 0, i = 0; y < len; ++y)
            {
                for (int x = 0; x < len; ++x, ++i)
                {
                    m_leaves[i].setNeighbor(SpaceTree.XMIN, x > 0 ? m_leaves[i - 1] : null);
                    m_leaves[i].setNeighbor(SpaceTree.XMAX, x < last ? m_leaves[i + 1] : null);
                    m_leaves[i].setNeighbor(SpaceTree.YMIN, y > 0 ? m_leaves[i - len] : null);
                    m_leaves[i].setNeighbor(SpaceTree.YMAX, y < last ? m_leaves[i + len] : null);
                }
            }
        }

        public void addToTree(ISpaceTreeData2D data)
        {
            if (m_data.ContainsKey(data))
            {
                return;
            }
            m_data[data] = new HashSet<SpaceTreeNode2D>();
        }

        /// <summary>
        /// Gets the nodes of a data
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The nodes</returns>
        public HashSet<SpaceTreeNode2D> getNodesForData(ISpaceTreeData2D data)
        {
            HashSet<SpaceTreeNode2D> set = null;
            m_data.TryGetValue(data, out set);
            return set;
        }

        /// <summary>
        /// Gets all the data can has collision with the data
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>All the possible collision data</returns>
        public List<ISpaceTreeData2D> getCollision(ISpaceTreeData2D data)
        {
            m_collision.Clear();
            HashSet<SpaceTreeNode2D> set = null;
            if (m_data.TryGetValue(data, out set))
            {
                foreach (SpaceTreeNode2D n in set)
                {
                    foreach (ISpaceTreeData2D d in n.AllData)
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
        public void removeFromTree(ISpaceTreeData2D data)
        {
            HashSet<SpaceTreeNode2D> set = null;
            if (!m_data.TryGetValue(data, out set))
            {
                return;
            }
            foreach (SpaceTreeNode2D n in set)
            {
                n.removeData(data);
            }
            m_data.Remove(data);
        }

        /// <summary>
        /// Updates a data's position in the space
        /// </summary>
        /// <param name="data">The data</param>
        public void updateData(ISpaceTreeData2D data)
        {
            HashSet<SpaceTreeNode2D> set = null;
            if (!m_data.TryGetValue(data, out set))
            {
                return;
            }
            foreach (SpaceTreeNode2D n in set)
            {
                n.removeData(data);
            }
            set.Clear();
            SpaceTreeNode2D min = m_root.getLeafWithoutBound(data.getMinPoint());
            SpaceTreeNode2D max = m_root.getLeafWithoutBound(data.getMaxPoint());
            SpaceTreeNode2D xn = min;
            SpaceTreeNode2D yn = min;
            for (int y = min.m_index.y; y <= max.m_index.y; ++y)
            {
                xn = yn;
                for (int x = min.m_index.x; x <= max.m_index.x; ++x)
                {
                    set.Add(xn);
                    xn.addData(data);
                    xn = xn.getNeighbor(SpaceTree.XMAX);
                }
                yn = yn.getNeighbor(SpaceTree.YMAX);
            }
        }

        private int sortLeaves(SpaceTreeNode2D l, SpaceTreeNode2D r)
        {
            Vector2 c1 = l.Center;
            Vector2 c2 = r.Center;
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

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Map/Space Tree 2D", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<SpaceTree2D>(true);
        }
#endif
    }
}
