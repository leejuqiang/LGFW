using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A tile influence map
    /// </summary>
    public class InfluenceMap
    {

        protected int m_width;
        protected int m_height;
        protected float m_influenceThreshold;

        protected float[] m_influenceFactors;
        protected float[] m_values;
        protected float[] m_valuesOfRange;
        protected float m_totalWeight;
        protected bool m_useRandomWeight = false;

        protected int m_lastRange;

        /// <summary>
        /// If true, this map can be used for randomly selecting a tile based on its influence value
        /// </summary>
        /// <value>If use for randomly selecting tile</value>
        public bool UseForRandom
        {
            get { return m_useRandomWeight; }
        }

        /// <summary>
        /// The min influence value propagate to other tiles
        /// </summary>
        /// <value>The threshold</value>
        public float Threshold
        {
            get { return m_influenceThreshold; }
            set { m_influenceThreshold = value; }
        }

        /// <summary>
        /// The factor[i] means how much the influence value will be multiplied for a tile with distance i
        /// </summary>
        /// <value>The factors</value>
        public float[] Factor
        {
            get { return m_influenceFactors; }
            set
            {
                m_influenceFactors = value;
                m_valuesOfRange = new float[m_influenceFactors.Length + 1];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.InfluenceMap"/> class.
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public InfluenceMap(int width, int height)
        {
            m_width = width;
            m_height = height;
            m_values = new float[m_width * m_height];
        }

        /// <summary>
        /// Initialize this map
        /// </summary>
        /// <param name="v">The initial value of each tile</param>
        /// <param name="threshold">The threshold</param>
        /// <param name="factors">The factors</param>
        /// <param name="useForRandom">If use for randomly selecting tile</param>
        public void init(float v, float threshold, float[] factors, bool useForRandom)
        {
            m_useRandomWeight = useForRandom;
            m_influenceFactors = factors;
            m_influenceThreshold = threshold;
            m_valuesOfRange = new float[factors.Length + 1];
            for (int i = 0; i < m_values.Length; ++i)
            {
                m_values[i] = v;
            }
            if (m_useRandomWeight)
            {
                m_totalWeight = v * m_values.Length;
            }
        }

        protected void changeValue(int index, float v)
        {
            if (v < 0 && m_values[index] < -v)
            {
                v = -m_values[index];
                m_values[index] = 0;
            }
            else
            {
                m_values[index] += v;
            }
            if (m_useRandomWeight)
            {
                m_totalWeight += v;
            }
        }

        /// <summary>
        /// Compute the influence offset from 0 to the max range
        /// </summary>
        /// <returns>The max range</returns>
        /// <param name="v">The influence offset of distance 0</param>
        public virtual int influenceByRange(float v)
        {
            if (m_valuesOfRange[0] != v)
            {
                m_valuesOfRange[0] = v;
                int r = 1;
                for (; r <= m_influenceFactors.Length; ++r)
                {
                    float t = m_influenceFactors[r - 1] * v;
                    if (Mathf.Abs(t) < m_influenceThreshold)
                    {
                        m_lastRange = r - 1;
                        return m_lastRange;
                    }
                    m_valuesOfRange[r] = t;
                }
                m_lastRange = m_influenceFactors.Length;
                return m_lastRange;
            }
            return m_lastRange;
        }

        /// <summary>
        /// Gets a tile's index by its x and y
        /// </summary>
        /// <returns>The index</returns>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public int tileIndex(int x, int y)
        {
            if (x < 0 || x >= m_width || y < 0 || y >= m_height)
            {
                return -1;
            }
            return x + y * m_width;
        }

        /// <summary>
        /// Gets the value of a tile
        /// </summary>
        /// <returns>The value, if out of range, return 0</returns>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public float getValueAt(int x, int y)
        {
            int index = tileIndex(x, y);
            if (index >= 0)
            {
                return m_values[index];
            }
            return 0;
        }

        /// <summary>
        /// Sets the value of a tile, this function won't influence other tiles
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="v">The value</param>
        public void setValueAt(int x, int y, float v)
        {
            int index = tileIndex(x, y);
            if (index >= 0)
            {
                setValueAt(index, v);
            }
        }

        /// <summary>
        /// Sets the value of a tile, this function won't influence other tiles
        /// </summary>
        /// <param name="index">The index of the tile</param>
        /// <param name="v">The value</param>
        public void setValueAt(int index, float v)
        {
            if (m_useRandomWeight)
            {
                m_totalWeight += v - m_values[index];
            }
            m_values[index] = v;
        }

        /// <summary>
        /// Changes the influence of a row
        /// </summary>
        /// <param name="y">The y coordinate</param>
        /// <param name="v">The value</param>
        public void changeInfluenceOfRow(int y, float v)
        {
            if (y < 0 || y >= m_height)
            {
                return;
            }
            int r = influenceByRange(v);
            int index = y * m_width;
            for (int i = 0; i < m_width; ++i, ++index)
            {
                changeInfluence(index, i, y, v, r);
            }
        }

        /// <summary>
        /// Changes the influence of a column
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="v">The value</param>
        public void changeInfluenceOfColumn(int x, float v)
        {
            if (x < 0 || x >= m_width)
            {
                return;
            }
            int r = influenceByRange(v);
            int index = 0;
            for (int i = 0; i < m_height; ++i, index += m_width)
            {
                changeInfluence(index + x, x, i, v, r);
            }
        }

        /// <summary>
        /// Changes the influence of a tile.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="v">The influence value</param>
        public void changeInfluenceOfTile(int x, int y, float v)
        {
            int index = tileIndex(x, y);
            if (index >= 0)
            {
                int r = influenceByRange(v);
                changeInfluence(index, x, y, v, r);
            }
        }

        protected void changeInfluence(int index, int x, int y, float v, int range)
        {
            changeValue(index, v);
            for (int r = 1; r <= range; ++r)
            {
                int row = 0;
                for (int j = 0; j <= r; ++j)
                {
                    if (y + j >= m_height)
                    {
                        break;
                    }
                    int i = r - j;
                    if (x + i < m_width)
                    {
                        changeValue(index + row + i, m_valuesOfRange[r]);
                    }
                    if (i != 0 && x >= i)
                    {
                        changeValue(index + row - i, m_valuesOfRange[r]);
                    }
                    row += m_width;
                }
                row = m_width;
                for (int j = 1; j <= r; ++j)
                {
                    if (y < j)
                    {
                        break;
                    }
                    int i = r - j;
                    if (x + i < m_width)
                    {
                        changeValue(index - row + i, m_valuesOfRange[r]);
                    }
                    if (i != 0 && x >= i)
                    {
                        changeValue(index - row - i, m_valuesOfRange[r]);
                    }
                    row += m_width;
                }
            }
        }

        /// <summary>
        /// Randomly selects a tile by the tiles influence value, tiles with larger value is more likely being selected 
        /// </summary>
        /// <param name="random">The RandomKit, if don't want to use a RandomKit, pass null</param>
        public int random(Randomizer random = null)
        {
            if (m_totalWeight <= 0)
            {
                return -1;
            }
            float r = 0;
            if (random == null)
            {
                r = Random.Range(0, m_totalWeight);
            }
            else
            {
                r = random.range(0, m_totalWeight);
            }
            for (int i = 0; i < m_values.Length; ++i)
            {
                r -= m_values[i];
                if (r <= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Gets the x and y coordinates by a tile's index
        /// </summary>
        /// <returns>The x y coordinates</returns>
        /// <param name="index">The index</param>
        public Vector2Int getXY(int index)
        {
            int y = index / m_width;
            return new Vector2Int(index - y * m_width, y);
        }
    }
}
