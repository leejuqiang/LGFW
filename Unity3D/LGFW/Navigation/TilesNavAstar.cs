using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A a* node for tile map
    /// </summary>
    public class TilesNavAstarNode : NavAStarNode
    {
        /// <summary>
        /// The x coordinate
        /// </summary>
        public int m_x;
        /// <summary>
        /// The y coordinate
        /// </summary>
        public int m_y;
        /// <summary>
        /// If the node is in range
        /// </summary>
        public bool m_inRange;

        /// <summary>
        /// If the node can be step in
        /// </summary>
        /// <value></value>
        public override bool Clear
        {
            get { return m_map || m_inRange; }
        }

        /// <inheritdoc>
        public override void reset()
        {
            base.reset();
            m_inRange = false;
        }

    }

    /// <summary>
    /// An a* navigation for a tile map
    /// </summary>
    /// <typeparam name="TilesNavAstarNode">The node for tile map</typeparam>
    public class TilesNavAstar : NavAStar<TilesNavAstarNode>
    {

        private int m_width;
        private int m_height;

        /// <summary>
        /// If true, the navigation is for 4 directions
        /// </summary>
        public bool m_4Directions = true;
        /// <summary>
        /// If true, the navigation will prefer to select straight path
        /// </summary>
        public bool m_preferStraight = true;

        /// <summary>
        /// Initialize the map by a maze
        /// </summary>
        /// <param name="m">The maze</param>
        public void initByMaze(Maze m)
        {
            initMap(m.m_width, m.m_height);
            byte[] b = m.MazeCells;
            for (int i = 0; i < b.Length; ++i)
            {
                m_nodes[i].m_map = b[i] == Maze.EMPTY;
            }
        }

        /// <summary>
        /// Sets the map
        /// </summary>
        /// <param name="map">True in the array presents a tile can be access in the map</param>
        /// <returns>True If the map is set</returns>
        public bool setMap(bool[] map)
        {
            if (map.Length != m_nodes.Length)
            {
                Debug.LogError("map size wrong");
                return false;
            }
            for (int i = 0; i < m_nodes.Length; ++i)
            {
                m_nodes[i].m_map = map[i];
            }
            return true;
        }

        /// <summary>
        /// Initialize the map
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public void initMap(int width, int height)
        {
            clearMap();
            m_width = width;
            m_height = height;
            m_surroundNodes = new TilesNavAstarNode[8];
            m_nodes = new TilesNavAstarNode[width * height];
            int index = 0;
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x, ++index)
                {
                    TilesNavAstarNode n = createANode();
                    n.m_id = index;
                    n.m_x = x;
                    n.m_y = y;
                    m_nodes[index] = n;
                }
            }
        }

        /// <summary>
        /// Gets the index of a tile
        /// </summary>
        /// <param name="x">X of the tile</param>
        /// <param name="y">Y of the tile</param>
        /// <returns>The index</returns>
        public int tileIndex(int x, int y)
        {
            if (x < 0 || x >= m_width || y < 0 || y >= m_height)
            {
                return -1;
            }
            return x + y * m_width;
        }

        /// <summary>
        /// Gets a node
        /// </summary>
        /// <param name="x">X of the node</param>
        /// <param name="y">Y of the node</param>
        /// <returns>The node</returns>
        public TilesNavAstarNode getNode(int x, int y)
        {
            int i = tileIndex(x, y);
            if (i >= 0)
            {
                return m_nodes[i];
            }
            return null;
        }

        protected override void initRange()
        {
            int range = (int)m_stopRange;
            for (int r = 1; r < range; ++r)
            {
                int row = 0;
                for (int y = 0; y <= r; ++y, row += m_width)
                {
                    if (m_end.m_y + y >= m_height)
                    {
                        break;
                    }
                    int x = r - y;
                    if (m_end.m_x + x < m_width)
                    {
                        m_nodes[x + m_end.m_id + row].m_inRange = true;
                    }
                    if (x != 0 && m_end.m_x >= x)
                    {
                        m_nodes[m_end.m_id - x + row].m_inRange = true;
                    }
                }
                row = m_width;
                for (int y = 1; y <= r; ++y, row += m_width)
                {
                    if (m_end.m_y < y)
                    {
                        break;
                    }
                    int x = r - y;
                    if (m_end.m_x + x < m_width)
                    {
                        m_nodes[x + m_end.m_id - row].m_inRange = true;
                    }
                    if (x != 0 && m_end.m_x >= x)
                    {
                        m_nodes[m_end.m_id - x - row].m_inRange = true;
                    }
                }
            }
        }

        protected override float getRangeOfNodes(TilesNavAstarNode node1, TilesNavAstarNode node2)
        {
            return Mathf.Abs(node1.m_x - node2.m_x) + Mathf.Abs(node1.m_y - node2.m_y);
        }

        private TilesNavAstarNode left(TilesNavAstarNode n)
        {
            if (n.m_x - 1 < 0)
            {
                return null;
            }
            return m_nodes[n.m_id - 1];
        }

        private TilesNavAstarNode right(TilesNavAstarNode n)
        {
            if (n.m_x + 1 >= m_width)
            {
                return null;
            }
            return m_nodes[n.m_id + 1];
        }

        private TilesNavAstarNode top(TilesNavAstarNode n)
        {
            if (n.m_y + 1 >= m_height)
            {
                return null;
            }
            return m_nodes[n.m_id + m_width];
        }

        private TilesNavAstarNode bot(TilesNavAstarNode n)
        {
            if (n.m_y - 1 < 0)
            {
                return null;
            }
            return m_nodes[n.m_id - m_width];
        }

        private TilesNavAstarNode leftTop(TilesNavAstarNode n)
        {
            if (n.m_x - 1 < 0 || n.m_y + 1 >= m_height)
            {
                return null;
            }
            return m_nodes[n.m_id - 1 + m_width];
        }

        private TilesNavAstarNode rightTop(TilesNavAstarNode n)
        {
            if (n.m_x + 1 >= m_width || n.m_y + 1 >= m_height)
            {
                return null;
            }
            return m_nodes[n.m_id + 1 + m_width];
        }

        private TilesNavAstarNode leftBot(TilesNavAstarNode n)
        {
            if (n.m_x - 1 < 0 || n.m_y - 1 < 0)
            {
                return null;
            }
            return m_nodes[n.m_id - 1 - m_width];
        }

        private TilesNavAstarNode rightBot(TilesNavAstarNode n)
        {
            if (n.m_x + 1 >= m_width || n.m_y - 1 < 0)
            {
                return null;
            }
            return m_nodes[n.m_id + 1 - m_width];
        }

        protected override void findSurroundNodes(TilesNavAstarNode n)
        {
            m_surroundNodesCount = 4;
            m_surroundNodes[0] = top(n);
            m_surroundNodes[1] = right(n);
            m_surroundNodes[2] = bot(n);
            m_surroundNodes[3] = left(n);
            if (!m_4Directions)
            {
                m_surroundNodesCount = 8;
                m_surroundNodes[4] = rightTop(n);
                m_surroundNodes[5] = rightBot(n);
                m_surroundNodes[6] = leftBot(n);
                m_surroundNodes[7] = leftTop(n);
            }
        }

        protected override float computeCost(TilesNavAstarNode n)
        {
            float h = Mathf.Abs(m_end.m_x - n.m_x) + Mathf.Abs(m_end.m_y - n.m_y);
            if (m_preferStraight)
            {
                TilesNavAstarNode p = (TilesNavAstarNode)n.m_parent;
                if (p == null)
                {
                    return h;
                }
                TilesNavAstarNode p1 = (TilesNavAstarNode)p.m_parent;
                if (p1 == null)
                {
                    return h;
                }
                if (p.m_x - n.m_x != p1.m_x - p.m_x)
                {
                    h += 0.2f;
                }
                else if (p.m_y - n.m_y != p1.m_y - p.m_y)
                {
                    h += 0.2f;
                }
            }
            return h;
        }

        protected override float computeG(TilesNavAstarNode n, TilesNavAstarNode parent)
        {
            if (!m_4Directions && parent.m_x != n.m_x && parent.m_y != n.m_y)
            {
                return parent.m_g + 1.4f;
            }
            return parent.m_g + 1;
        }

        /// <summary>
        /// Finds a path of nodes
        /// </summary>
        /// <param name="startX">X of the start node</param>
        /// <param name="startY">y of the start node</param>
        /// <param name="endX">X of the end node</param>
        /// <param name="endY">Y of the end node</param>
        /// <param name="range">The range</param>
        /// <returns>The list of nodes</returns>
        public List<TilesNavAstarNode> findPathList(int startX, int startY, int endX, int endY, float range = 0)
        {
            TilesNavAstarNode s = getNode(startX, startY);
            TilesNavAstarNode e = getNode(endX, endY);
            return findPathList(s, e, range);
        }

        /// <summary>
        /// Finds the last node of a path from a start node to a end node
        /// </summary>
        /// <param name="startX">X of the start node</param>
        /// <param name="startY">y of the start node</param>
        /// <param name="endX">x of the end node</param>
        /// <param name="endY">y of the end node</param>
        /// <param name="range">The range</param>
        /// <returns>The last node</returns>
        public TilesNavAstarNode findPath(int startX, int startY, int endX, int endY, float range = 0)
        {
            TilesNavAstarNode s = getNode(startX, startY);
            TilesNavAstarNode e = getNode(endX, endY);
            return findPath(s, e, range);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Map/A* For Tiles", false, (int)'a')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<TilesNavAstar>(true);
        }
#endif
    }
}
