using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An a* node for tile map
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

        public TilesNavAstarNode(int id) : base(id)
        {

        }

        public override void computeH(NavAStarNode end)
        {
            TilesNavAstarNode n = (TilesNavAstarNode)end;
            m_h = Mathf.Abs(n.m_x - m_x) + Mathf.Abs(n.m_y - m_y);
        }

        public override void computeRange(NavAStarNode end)
        {
            TilesNavAstarNode n = (TilesNavAstarNode)end;
            m_range = Mathf.Abs(n.m_x - m_x) + Mathf.Abs(n.m_y - m_y);
        }
    }

    /// <summary>
    /// An a* navigation for a tile map
    /// </summary>
    /// <typeparam name="TilesNavAstarNode">The node for tile map</typeparam>
    public class TilesNavAstar : MonoBehaviour
    {

        private static int[] m_xOffset = new int[] { 0, 0, -1, 1, -1, 1, -1, 1 };
        private static int[] m_yOffset = new int[] { -1, 1, 0, 0, -1, -1, 1, 1 };
        private static int[] m_indexOffset;
        //The width of the map
        public int m_width;
        //The hight of the map
        public int m_height;

        /// <summary>
        /// If true, the navigation is for 4 directions
        /// </summary>
        public bool m_4Directions = true;

        private NavAStar<TilesNavAstarNode> m_nav = new NavAStar<TilesNavAstarNode>();

        // public Maze m_maze;
        // void Start()
        // {
        //     initByMaze(m_maze);
        //     var l = findPathList(m_maze.m_start.x, m_maze.m_start.y, m_maze.m_end.x, m_maze.m_end.y, 2);
        //     Debug.Log(l.Count);
        //     m_maze.m_path = l;
        // }

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
                m_nav.m_nodes[i].m_accessible = b[i] == Maze.EMPTY;
            }
        }

        /// <summary>
        /// Sets the map
        /// </summary>
        /// <param name="map">True in the array presents a tile can be access in the map</param>
        /// <returns>True If the map is set</returns>
        public bool setMap(bool[] map)
        {
            if (map.Length != m_nav.m_nodes.Count)
            {
                Debug.LogError("map size wrong");
                return false;
            }
            for (int i = 0; i < m_nav.m_nodes.Count; ++i)
            {
                m_nav.m_nodes[i].m_accessible = map[i];
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
            m_width = width;
            m_height = height;
            m_indexOffset = new int[m_xOffset.Length];
            for (int i = 0; i < m_indexOffset.Length; ++i)
            {
                m_indexOffset[i] = m_xOffset[i] + m_yOffset[i] * m_width;
            }
            int index = 0;
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x, ++index)
                {
                    TilesNavAstarNode n = new TilesNavAstarNode(index);
                    n.m_x = x;
                    n.m_y = y;
                    m_nav.addNode(n);
                }
            }
            int d = m_4Directions ? 4 : 8;
            index = 0;
            for (int y = 0; y < m_height; ++y)
            {
                for (int x = 0; x < m_width; ++x, ++index)
                {
                    TilesNavAstarNode n = m_nav.m_nodes[index];
                    for (int i = 0; i < d; ++i)
                    {
                        int x1 = x + m_xOffset[i];
                        int y1 = y + m_yOffset[i];
                        if (x1 >= 0 && x1 < m_width && y1 >= 0 && y1 < m_height)
                        {
                            var n1 = m_nav.m_nodes[index + m_indexOffset[i]];
                            n.addOutEdge(n1, 1);
                        }
                    }
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
                return m_nav.m_nodes[i];
            }
            return null;
        }

        /// <summary>
        /// Finds a path of nodes
        /// </summary>
        /// <param name="startX">X of the start node</param>
        /// <param name="startY">y of the start node</param>
        /// <param name="endX">X of the end node</param>
        /// <param name="endY">Y of the end node</param>
        /// <param name="range">The range</param>
        /// <returns>The list of nodes of the path</returns>
        public List<TilesNavAstarNode> findPathList(int startX, int startY, int endX, int endY, float range = 0)
        {
            TilesNavAstarNode s = getNode(startX, startY);
            TilesNavAstarNode e = getNode(endX, endY);
            return findPathList(s, e, range);
        }

        /// <summary>
        /// Finds a path of nodes
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="range">The range</param>
        /// <returns>The list of nodes of the path</returns>
        public List<TilesNavAstarNode> findPathList(TilesNavAstarNode start, TilesNavAstarNode end, float range = 0)
        {
            return m_nav.findPathList(start, end, range);
        }

        /// <summary>
        /// Finds the last node of a path from a start node to a end node
        /// </summary>
        /// <param name="startX">X of the start node</param>
        /// <param name="startY">y of the start node</param>
        /// <param name="endX">x of the end node</param>
        /// <param name="endY">y of the end node</param>
        /// <param name="range">The range</param>
        /// <returns>The last node of the path</returns>
        public TilesNavAstarNode findPath(int startX, int startY, int endX, int endY, float range = 0)
        {
            TilesNavAstarNode s = getNode(startX, startY);
            TilesNavAstarNode e = getNode(endX, endY);
            return findPath(s, e, range);
        }

        /// <summary>
        /// Finds the last node of a path from a start node to a end node
        /// </summary>
        /// <param name="start">The start node</param>
        /// <param name="end">The end node</param>
        /// <param name="range">The range</param>
        /// <returns>The last node of the path</returns>
        public TilesNavAstarNode findPath(TilesNavAstarNode start, TilesNavAstarNode end, float range = 0)
        {
            return m_nav.findPath(start, end, range);
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
