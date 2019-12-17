using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A rectangle tile map
    /// </summary>
    /// <typeparam name="T">The type of the tile in this map, must be a subclass of BaseTile</typeparam>
    public class TileMap<T> : BaseMono where T : BaseTile, new()
    {
        /// <summary>
        /// The max value of x and y. The tile's coordinates are from (0, 0) to (m_size.x - 1, m_size.y - 1)
        /// </summary>
        [SerializeField]
        protected Vector2Int m_size = Vector2Int.one;
        /// <summary>
        /// The size of a tile. If x or y is greater then 0, the position of each tile is computed.
        /// If only one of x and y is 0, the 0 value is computed as the tile is a regular shape(square, regular hexagon)
        /// </summary>
        [SerializeField]
        protected Vector2 m_tileSize = Vector2.zero;

        protected T[] m_tiles;
        protected Rect m_mapRect = Rect.zero;

        /// <summary>
        /// The Rect represents the AABB of this map, the left bottom is always (0, 0)
        /// </summary>
        /// <value>The Rect</value>
        public Rect MapRect
        {
            get { return m_mapRect; }
        }

        protected override void editorAwake()
        {
            doAwake();
        }

        protected override void doAwake()
        {
            m_tiles = new T[m_size.x * m_size.y];
            int i = 0;
            for (int y = 0; y < m_size.y; ++y)
            {
                for (int x = 0; x < m_size.x; ++x)
                {
                    m_tiles[i] = new T();
                    m_tiles[i].Coordinate = new Vector2Int(x, y);
                    ++i;
                }
            }
            if (m_tileSize.x > 0 || m_tileSize.y > 0)
            {
                computePosition();
            }
        }

        protected virtual void computePosition()
        {
            if (m_tileSize.x <= 0)
            {
                m_tileSize.x = m_tileSize.y;
            }
            else if (m_tileSize.y <= 0)
            {
                m_tileSize.y = m_tileSize.x;
            }
            m_mapRect.xMin = 0;
            m_mapRect.yMin = 0;
            m_mapRect.xMax = m_tileSize.x * m_size.x;
            m_mapRect.yMax = m_tileSize.y * m_size.y;
            Vector2 v = m_tileSize * 0.5f;
            float xs = v.x;
            int i = 0;
            for (int y = 0; y < m_size.y; ++y)
            {
                for (int x = 0; x < m_size.x; ++x)
                {
                    m_tiles[i].Position = v;
                    v.x += m_tileSize.x;
                    ++i;
                }
                v.x = xs;
                v.y += m_tileSize.y;
            }
        }

        public virtual void generateGraph<N>(Graph<N> g) where N : GraphNode, new()
        {
            if (!g.IsDoubleDirection)
            {
                Debug.LogError("The graph used for generateGraph must be a double direction graph");
                return;
            }
            for (int y = 0; y < m_size.y; ++y)
            {
                for (int x = 0; x < m_size.x; ++x)
                {
                    N node = new N();
                    node.ID = BaseTile.getIDByCoordinate(x, y);
                    g.addNode(node);
                }
            }
            int index = 0;
            for (int y = 0; y < m_size.y; ++y)
            {
                int i = index;
                for (int x = 0; x < m_size.x; ++x, ++i)
                {
                    N node = g.m_nodes[i];
                    if (x > 0)
                    {
                        node.addOutEdge(g.m_nodes[i - 1], 1);
                    }
                    if (x < m_size.x - 1)
                    {
                        node.addOutEdge(g.m_nodes[i + 1], 1);
                    }
                    if (y > 0)
                    {
                        node.addOutEdge(g.m_nodes[i - m_size.x], 1);
                    }
                    if (y < m_size.y - 1)
                    {
                        node.addOutEdge(g.m_nodes[i + m_size.x], 1);
                    }
                    node.m_inEdge = node.m_outEdge;
                }
                index += m_size.x;
            }
        }

        /// <summary>
        /// Gets the tile by id
        /// </summary>
        /// <param name="id">The id of the tile</param>
        /// <returns>The tile</returns>
        public T getTile(int id)
        {
            Vector2Int v = BaseTile.getCoordinateByID(id);
            return getTile(v.x, v.y);
        }

        /// <summary>
        /// Gets the tile by coordinate
        /// </summary>
        /// <param name="coordinate">The coordinate</param>
        /// <returns>The tile, or null if the coordinate is incorrect</returns>
        public T getTile(Vector2Int coordinate)
        {
            return getTile(coordinate.x, coordinate.y);
        }

        /// <summary>
        /// Gets the tile by coordinate
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The x coordinate</returns>
        public virtual T getTile(int x, int y)
        {
            if (x < 0 || x >= m_size.x || y < 0 || y >= m_size.y)
            {
                return null;
            }
            return m_tiles[x + y * m_size.x];
        }

        /// <summary>
        /// Gets the distance between tiles (x1, y1) and (x2, y2)
        /// </summary>
        /// <param name="x1">The first tile's x</param>
        /// <param name="y1">The first tile's y</param>
        /// <param name="x2">The second tile's x</param>
        /// <param name="y2">The second tile's y</param>
        /// <returns>The distance</returns>
        public virtual int distance(int x1, int y1, int x2, int y2)
        {
            return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
        }

        /// <summary>
        /// Gets the distance between tiles c1 and c2
        /// </summary>
        /// <param name="c1">The first tile's coordinate</param>
        /// <param name="c2">The second tile's coordinate</param>
        /// <returns>The distance</returns>
        public int distance(Vector2Int c1, Vector2Int c2)
        {
            return distance(c1.x, c1.y, c2.x, c2.y);
        }

#if UNITY_EDITOR
        protected virtual void drawGizmos()
        {
            if (m_tileSize.x > 0 || m_tileSize.y > 0)
            {
                Color co = Gizmos.color;
                resetFlag();
                Awake();
                Transform t = this.transform;
                Vector3 s = m_tileSize * 0.25f;
                s = t.TransformVector(s);
                Graph<GraphNode> g = new Graph<GraphNode>(true);
                generateGraph<GraphNode>(g);
                Gizmos.color = Color.green;
                for (int i = 0; i < m_tiles.Length; ++i)
                {
                    Vector3 v = m_tiles[i].Position;
                    Gizmos.DrawCube(t.TransformPoint(v), s);
                }
                Gizmos.color = Color.red;
                for (int i = 0; i < g.m_nodes.Count; ++i)
                {
                    for (int j = 0; j < g.m_nodes[i].m_outEdge.Count; ++j)
                    {
                        var from = g.m_nodes[i];
                        var to = g.m_nodes[i].m_outEdge[j].m_end;
                        Vector3 vf = getTile(from.ID).Position;
                        Vector3 vt = getTile(to.ID).Position;
                        Gizmos.DrawLine(t.TransformPoint(vf), t.TransformPoint(vt));
                    }
                }
                Gizmos.color = co;
            }
        }
#endif
    }
}
