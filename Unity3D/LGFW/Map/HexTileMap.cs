using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A hexagon tile map, in this case, the m_size.y must be an even number.
    /// The y coordinate has an offset of 2. For example, if m_size is (3, 6), the map looks like this:
    /// 
    ///         (1, 5)
    /// (0, 4)          (2, 4)
    ///         (1, 3)
    /// (0, 2)          (2, 2)
    ///         (1, 1)
    /// (0, 0)          (2, 0)
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HexTileMap<T> : TileMap<T> where T : BaseTile, new()
    {
        protected int m_halfSize;

        protected override void doAwake()
        {
            if ((m_size.y & 1) != 0)
            {
                Debug.LogError("m_size.y must be an even number");
                return;
            }
            m_halfSize = m_size.y >> 1;
            m_tiles = new T[m_size.x * m_halfSize];
            int i = 0;
            int yi = 0;
            for (int x = 0; x < m_size.x; ++x)
            {
                for (int y = yi; y < m_size.y; y += 2)
                {
                    m_tiles[i] = new T();
                    m_tiles[i].Coordinate = new Vector2Int(x, y);
                    ++i;
                }
                yi = yi == 0 ? 1 : 0;
            }
            if (m_tileSize.x > 0 && m_tileSize.y > 0)
            {
                computePosition();
            }
        }

        protected override void computePosition()
        {
            if (m_tileSize.x <= 0)
            {
                m_tileSize.x = m_tileSize.y * 1.154700538f;
            }
            else if (m_tileSize.y <= 0)
            {
                m_tileSize.y = m_tileSize.x * 0.866025404f;
            }
            Vector2 v = m_tileSize * 0.5f;
            float w = m_tileSize.x * 0.25f;
            m_mapRect.xMin = 0;
            m_mapRect.yMin = 0;
            m_mapRect.yMax = m_halfSize * m_tileSize.y + v.y;
            m_mapRect.xMax = (m_size.x * 3 + 1) * w;
            int i = 0;
            int yi = 0;
            float ys = v.y;
            Vector2 step = new Vector2(w * 3, v.y);
            for (int x = 0; x < m_size.x; ++x)
            {
                for (int y = yi; y < m_size.y; y += 2)
                {
                    m_tiles[i].Position = v;
                    v.y += m_tileSize.y;
                    ++i;
                }
                yi = yi == 0 ? 1 : 0;
                v.x += step.x;
                v.y = ys;
                if ((x & 1) == 0)
                {
                    v.y += step.y;
                }
            }
        }

        protected int getIndexFromXY(int x, int y)
        {
            if (x < 0 || x >= m_size.x)
            {
                return -1;
            }
            if ((x & 1) == 0)
            {
                if (y < 0 || y >= m_size.y - 1)
                {
                    return -1;
                }
                return x * m_halfSize + (y >> 1);
            }
            if (y < 1 || y >= m_size.y)
            {
                return -1;
            }
            return x * m_halfSize + ((y - 1) >> 1);
        }

        public override T getTile(int x, int y)
        {
            int i = getIndexFromXY(x, y);
            if (i < 0)
            {
                return null;
            }
            return m_tiles[i];
        }

        public override void generateGraph<N>(Graph<N> g)
        {
            if (!g.IsDoubleDirection)
            {
                Debug.LogError("The graph used for generateGraph must be a double direction graph");
                return;
            }
            int yi = 0;
            for (int x = 0; x < m_size.x; ++x)
            {
                for (int y = yi; y < m_size.y; y += 2)
                {
                    N node = new N();
                    node.ID = BaseTile.getIDByCoordinate(x, y);
                    g.addNode(node);
                }
                yi = yi == 0 ? 1 : 0;
            }
            yi = 0;
            int i = 0;
            for (int x = 0; x < m_size.x; ++x)
            {
                for (int y = yi; y < m_size.y; y += 2, ++i)
                {
                    N node = g.m_nodes[i];
                    int index = getIndexFromXY(x - 1, y - 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    index = getIndexFromXY(x - 1, y + 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    index = getIndexFromXY(x, y - 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    index = getIndexFromXY(x, y + 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    index = getIndexFromXY(x + 1, y - 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    index = getIndexFromXY(x + 1, y + 1);
                    if (index >= 0)
                    {
                        node.addOutEdge(g.m_nodes[index], 1);
                    }
                    node.m_inEdge = node.m_outEdge;
                }
                yi = yi == 0 ? 1 : 0;
            }
        }

        public override int distance(int x1, int y1, int x2, int y2)
        {
            int dx = Mathf.Abs(x1 - x2);
            int dy = Mathf.Abs(y1 - y2);
            return dx + ((dy - Mathf.Min(dy, dx)) >> 1);
        }
    }
}