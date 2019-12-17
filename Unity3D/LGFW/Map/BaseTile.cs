using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The base class of a tile in TileMap
    /// </summary>
    public class BaseTile
    {
        protected Vector2Int m_coordinate;
        protected int m_id;

        /// <summary>
        /// The position of the center of the tile
        /// </summary>
        /// <value>The position</value>
        public Vector2 Position
        {
            get; set;
        }

        /// <summary>
        /// Gets the tile ID by its coordinate
        /// </summary>
        /// <param name="x">coordinate x</param>
        /// <param name="y">coordinate y</param>
        /// <returns>The ID</returns>
        public static int getIDByCoordinate(int x, int y)
        {
            return (x << 16) | y;
        }

        /// <summary>
        /// Gets the coordinate of a tile by its ID
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns>The coordinate</returns>
        public static Vector2Int getCoordinateByID(int id)
        {
            Vector2Int v = Vector2Int.zero;
            v.x = (id >> 16) & 0xfffff;
            v.y = id & 0xffff;
            return v;
        }

        /// <summary>
        /// The coordinate of a tile
        /// </summary>
        /// <value>The coordinate</value>
        public Vector2Int Coordinate
        {
            get { return m_coordinate; }
            set
            {
                m_coordinate = value;
                m_id = getIDByCoordinate(m_coordinate.x, m_coordinate.y);
            }
        }

        /// <summary>
        /// The ID of a tile
        /// </summary>
        /// <value>The ID</value>
        public int ID
        {
            get { return m_id; }
        }

        public BaseTile()
        {

        }
    }
}