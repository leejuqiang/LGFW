using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MazeNode
    {
        public int m_x;
        public int m_y;
        public int m_index;

        private bool m_hasVisit;

        private List<MazeNode> m_nodes = new List<MazeNode>();
        private int m_noVisitNodeCount;

        public bool HasVisit
        {
            get { return m_hasVisit; }
            set { m_hasVisit = value; }
        }

        public MazeNode(int x, int y, int index)
        {
            m_index = index;
            m_x = x;
            m_y = y;
            m_hasVisit = false;
        }

        public void addNode(MazeNode n)
        {
            m_nodes.Add(n);
            ++m_noVisitNodeCount;
        }

        public MazeNode findNextNode(RandomKit random)
        {
            int count = 0;
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                if (!m_nodes[i].m_hasVisit)
                {
                    ++count;
                }
            }
            if (count <= 0)
            {
                return null;
            }
            int r = random.range(0, count);
            for (int i = 0; i < m_nodes.Count; ++i)
            {
                if (!m_nodes[i].m_hasVisit)
                {
                    if (r == 0)
                    {
                        m_nodes[i].m_hasVisit = true;
                        return m_nodes[i];
                    }
                    --r;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// This represents a obstacle during a maze's generation
    /// </summary>
    [System.Serializable]
    public class MazeObstacle
    {
        /// <summary>
        /// The left coordinate
        /// </summary>
        public int m_left;
        /// <summary>
        /// The right coordinate
        /// </summary>
        public int m_right;
        /// <summary>
        /// The top coordinate
        /// </summary>
        public int m_top;
        /// <summary>
        /// The bottom coordinate
        /// </summary>
        public int m_bottom;
        /// <summary>
        /// The id of this obstacle, the tiles inside this obstacle will use this value as its value
        /// </summary>
        public byte m_id;
    }

    /// <summary>
    /// A random maze of a tile map
    /// </summary>
    public class Maze : MonoBehaviour
    {

        /// <summary>
        /// Tile type none
        /// </summary>
        public const byte NONE = 19;
        /// <summary>
        /// Tile type empty tile
        /// </summary>
        public const byte EMPTY = 0;
        /// <summary>
        /// Tile type wall
        /// </summary>
        public const byte FILLED = 1;

        /// <summary>
        /// The tile type for obstacle should be larger then this value
        /// </summary>
        public const byte OBSTACLE_START = 20;

        /// <summary>
        /// The width of the maze
        /// </summary>
        public int m_width;
        /// <summary>
        /// The height of the maze
        /// </summary>
        public int m_height;
        /// <summary>
        /// The rate of how many walls will be removed
        /// </summary>
        public float m_breakingWallRate;
        /// <summary>
        /// This value means if a wall is next to a number no less then this value of empty cells, then the wall can be removed
        /// </summary>
        public int m_breakingLimit = 1;
        /// <summary>
        /// The rate of how many empty tiles will be filled as wall
        /// </summary>
        public float m_fillRate;
        /// <summary>
        /// The start tile of the maze
        /// </summary>
        public Vector2Int m_start;
        /// <summary>
        /// The end tile of the maze
        /// </summary>
        public Vector2Int m_end;
        /// <summary>
        /// If true, the maze will see the start and end tile as entrance and exit
        /// </summary>
        public bool m_hasStartEnd;

        /// <summary>
        /// If true, the items generated may be next to each other
        /// </summary>
        public bool m_allowItemsNextToEachOther = false;
        /// <summary>
        /// If ture, there will be no item generated at the start and end tiles, only works when m_hasStartEnd is set true
        /// </summary>
        public bool m_noItemAtStartEnd = true;
        /// <summary>
        /// The items number
        /// </summary>
        public int m_itemNumber;
        /// <summary>
        /// The itme's possibility reduction of generated next to another item
        /// </summary>
        public float m_itemChanceDescent = 0.2f;
        /// <summary>
        /// The max distance a itme will influence the possibility of other items generated near it
        /// </summary>
        public int m_itemDescentRange = 3;
        /// <summary>
        /// The item's possibility reduce of generated at edges of the maze
        /// </summary>
        public float m_itemReduceChanceAtEdges = 0;

#if UNITY_EDITOR
        [System.NonSerialized]
        public bool m_editMap;
        [System.NonSerialized]
        public bool m_editItem;
        [System.NonSerialized]
        public byte m_editItemId = 1;
#endif

        /// <summary>
        /// The obstacles in this maze
        /// </summary>
        public MazeObstacle[] m_obstacles;

        [SerializeField]
        [HideInInspector]
        private byte[] m_matrix;
        [SerializeField]
        [HideInInspector]
        private byte[] m_items;

        private int[] m_template;

        private RandomKit m_random = new RandomKit();

        /// <summary>
        /// Gets all the maze cells 
        /// </summary>
        /// <value>The maze cells.</value>
        public byte[] MazeCells
        {
            get { return m_matrix; }
        }

        /// <summary>
        /// Sets the seed of random
        /// </summary>
        /// <param name="seed">The seed</param>
        public void setSeed(int seed)
        {
            m_random.setSeed(seed);
        }

        private int tileIndex(int x, int y)
        {
            if (x < 0 || x >= m_width)
            {
                return -1;
            }
            if (y < 0 || y >= m_height)
            {
                return -1;
            }
            return m_width * y + x;
        }

        /// <summary>
        /// Gets a tile's type
        /// </summary>
        /// <returns>The tile's type</returns>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public byte getTile(int x, int y)
        {
            int index = tileIndex(x, y);
            if (index < 0)
            {
                return NONE;
            }
            return m_matrix[index];
        }

        /// <summary>
        /// Sets a tile's type
        /// </summary>
        /// <returns>True if the tile exists</returns>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="type">The tile's type</param>
        public bool setTile(int x, int y, byte type)
        {
            int index = tileIndex(x, y);
            if (index >= 0)
            {
                m_matrix[index] = type;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Initialize this instance
        /// </summary>
        public void init()
        {
            m_matrix = new byte[m_width * m_height];
            m_items = null;
        }

        /// <summary>
        /// Sets an obstacle to the maze
        /// </summary>
        /// <param name="o">The obstacle</param>
        public void setObstacle(MazeObstacle o)
        {
            setObstacle(o.m_left, o.m_bottom, o.m_right, o.m_top, o.m_id);
        }

        /// <summary>
        /// Sets an obstacle to the maze
        /// </summary>
        /// <param name="left">The left of the obstacle</param>
        /// <param name="bottom">The bottom of the obstacle</param>
        /// <param name="right">The right of the obstacle</param>
        /// <param name="top">The top of the obstacle</param>
        /// <param name="obstacleId">The id of obstacle, if smaller then OBSTACLE_START, will use OBSTACLE_START</param>
        public void setObstacle(int left, int bottom, int right, int top, byte obstacleId)
        {
            if (obstacleId < OBSTACLE_START)
            {
                obstacleId = OBSTACLE_START;
            }
            for (int x = left; x <= right; ++x)
            {
                for (int y = bottom; y <= top; ++y)
                {
                    setTile(x, y, obstacleId);
                }
            }
        }

        /// <summary>
        /// Sets a template for this maze
        /// </summary>
        /// <param name="template">The template</param>
        public void setTemplate(int[] template)
        {
            if (template.Length != m_width * m_height)
            {
                Debug.LogError("Template length should be " + (m_width * m_height));
                return;
            }
            m_template = template;
        }

        /// <summary>
        /// Clears all the itmes
        /// </summary>
        public void clearItmes()
        {
            if (m_items == null || m_items.Length != m_matrix.Length)
            {
                m_items = new byte[m_matrix.Length];
            }
            else
            {
                for (int i = 0; i < m_matrix.Length; ++i)
                {
                    m_items[i] = 0;
                }
            }
        }

        /// <summary>
        /// Creates all the items
        /// </summary>
        /// <param name="id">The id of the items</param>
        public void createItems(byte id)
        {
            InfluenceMap imap = new InfluenceMap(m_width, m_height);
            float[] factors = new float[m_itemDescentRange];
            float v = 1;
            if (m_allowItemsNextToEachOther)
            {
                v -= m_itemChanceDescent;
            }
            for (int i = 0; i < m_itemDescentRange; ++i)
            {
                factors[i] = v;
                v -= m_itemChanceDescent;
            }
            imap.init(1, 0, factors, true);
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                if (m_matrix[i] != EMPTY || m_items[i] > 0)
                {
                    imap.setValueAt(i, 0);
                }
            }
            if (m_hasStartEnd && m_noItemAtStartEnd)
            {
                imap.changeInfluenceOfTile(m_start.x, m_start.y, -1);
                imap.changeInfluenceOfTile(m_end.x, m_end.y, -1);
            }
            if (m_itemReduceChanceAtEdges > 0)
            {
                imap.changeInfluenceOfRow(0, -m_itemReduceChanceAtEdges);
                imap.changeInfluenceOfRow(m_height - 1, -m_itemReduceChanceAtEdges);
                imap.changeInfluenceOfColumn(0, -m_itemReduceChanceAtEdges);
                imap.changeInfluenceOfColumn(m_width - 1, -m_itemReduceChanceAtEdges);
            }
            for (int i = 0; i < m_itemNumber; ++i)
            {
                int index = imap.random(m_random);
                if (index < 0)
                {
                    break;
                }
                m_items[index] = id;
                Vector2Int xy = imap.getXY(index);
                imap.changeInfluenceOfTile(xy.x, xy.y, -1);
            }
        }

        private void breakCell(int x, int y)
        {
            int index = tileIndex(x, y);
            if (m_matrix[index] < OBSTACLE_START)
            {
                m_matrix[index] = EMPTY;
            }
        }

        private void breakToNext(MazeNode current, MazeNode next)
        {
            if (current.m_x == next.m_x)
            {
                int ins = 1;
                if (current.m_y > next.m_y)
                {
                    ins = -1;
                }
                for (int i = current.m_y + ins; i != next.m_y; i += ins)
                {
                    breakCell(current.m_x, i);
                }
            }
            else if (current.m_y == next.m_y)
            {
                int ins = 1;
                if (current.m_x > next.m_x)
                {
                    ins = -1;
                }
                for (int i = current.m_x + ins; i != next.m_x; i += ins)
                {
                    breakCell(i, current.m_y);
                }
            }
            else
            {
                int x = current.m_x;
                int y = current.m_y;
                int xIns = 1;
                if (x > next.m_x)
                {
                    xIns = -1;
                }
                int yIns = 1;
                if (y > next.m_y)
                {
                    yIns = -1;
                }
                while (x != next.m_x && y != next.m_y)
                {
                    int r = m_random.range(0, 2);
                    if (r == 0)
                    {
                        x += xIns;
                    }
                    else
                    {
                        y += yIns;
                    }
                    breakCell(x, y);
                }
                if (x == next.m_x)
                {
                    while (y != next.m_y)
                    {
                        breakCell(x, y);
                        y += yIns;
                    }
                }
                else
                {
                    while (x != next.m_x)
                    {
                        breakCell(x, y);
                        x += xIns;
                    }
                }
            }
        }

        /// <summary>
        /// Clears the template
        /// </summary>
        public void clearTemplate()
        {
            for (int i = 0; i < m_template.Length; ++i)
            {
                m_template[i] = -1;
            }
        }

        private void defaultTemplate()
        {
            clearTemplate();
            for (int i = 0; i < m_height; i += 2)
            {
                for (int j = 0; j < m_width; j += 2)
                {
                    int index = tileIndex(j, i);
                    m_template[index] = 0;
                }
            }
        }

        private void createNodes(List<MazeNode> l)
        {
            int x = 0;
            int y = 0;
            for (int i = 0; i < m_template.Length; ++i)
            {
                if (m_template[i] >= 0 && m_matrix[i] < OBSTACLE_START)
                {
                    getXY(i, ref x, ref y);
                    MazeNode n = new MazeNode(x, y, i);
                    m_template[i] = l.Count;
                    l.Add(n);
                }
            }
            if (m_hasStartEnd)
            {
                int index = tileIndex(m_start.x, m_start.y);
                if (m_template[index] < 0)
                {
                    MazeNode n = new MazeNode(m_start.x, m_start.y, index);
                    m_template[index] = l.Count;
                    l.Add(n);
                }
                index = tileIndex(m_end.x, m_end.y);
                if (m_template[index] < 0)
                {
                    MazeNode n = new MazeNode(m_end.x, m_end.y, index);
                    m_template[index] = l.Count;
                    l.Add(n);
                }
            }

            for (int i = 0; i < l.Count; ++i)
            {
                MazeNode n = l[i];
                m_matrix[n.m_index] = EMPTY;
                int max = n.m_x;
                if (n.m_y > max)
                {
                    max = n.m_y;
                }
                int t = m_width - n.m_x - 1;
                if (max < t)
                {
                    max = t;
                }
                t = m_height - n.m_y - 1;
                if (max < t)
                {
                    max = t;
                }
                for (int d = 1; d <= max; ++d)
                {
                    bool find = false;
                    int row = 0;
                    for (y = 0; y <= d; ++y, row += m_width)
                    {
                        if (n.m_y + y >= m_height)
                        {
                            break;
                        }
                        x = d - y;
                        if (n.m_x + x < m_width)
                        {
                            int index = n.m_index + row + x;
                            if (m_template[index] >= 0)
                            {
                                n.addNode(l[m_template[index]]);
                                find = true;
                            }
                        }
                        if (x != 0 && n.m_x >= x)
                        {
                            int index = n.m_index + row - x;
                            if (m_template[index] >= 0)
                            {
                                n.addNode(l[m_template[index]]);
                                find = true;
                            }
                        }
                    }
                    row = m_width;
                    for (y = 1; y <= d; ++y, row += m_width)
                    {
                        if (n.m_y < y)
                        {
                            break;
                        }
                        x = d - y;
                        if (n.m_x + x < m_width)
                        {
                            int index = n.m_index - row + x;
                            if (m_template[index] >= 0)
                            {
                                n.addNode(l[m_template[index]]);
                                find = true;
                            }
                        }
                        if (x != 0 && n.m_x >= x)
                        {
                            int index = n.m_index - row - x;
                            if (m_template[index] >= 0)
                            {
                                n.addNode(l[m_template[index]]);
                                find = true;
                            }
                        }
                    }
                    if (find)
                    {
                        break;
                    }
                }
            }
        }

        public void onEditorChange(float x, float y, bool editMap, byte id)
        {
            if (m_matrix != null && m_matrix.Length == m_width * m_height)
            {
                x += 0.5f;
                y += 0.5f;
                int index = tileIndex((int)x, (int)y);
                if (index >= 0 && index < m_matrix.Length)
                {
                    if (editMap)
                    {
                        if (m_matrix[index] == EMPTY)
                        {
                            m_matrix[index] = FILLED;
                        }
                        else if (m_matrix[index] == FILLED)
                        {
                            m_matrix[index] = EMPTY;
                        }
                    }
                    else
                    {
                        if (m_items == null || m_items.Length != m_matrix.Length)
                        {
                            m_items = new byte[m_matrix.Length];
                        }
                        if (m_items[index] == 0)
                        {
                            if (m_matrix[index] == EMPTY)
                            {
                                m_items[index] = id;
                            }
                        }
                        else
                        {
                            m_items[index] = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates the maze
        /// </summary>
        public void createMaze()
        {
            m_random.enableContinous();
            init();
            m_template = new int[m_matrix.Length];
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                m_matrix[i] = FILLED;
            }
            if (m_obstacles != null)
            {
                for (int i = 0; i < m_obstacles.Length; ++i)
                {
                    setObstacle(m_obstacles[i]);
                }
            }
            defaultTemplate();
            List<MazeNode> nodeList = new List<MazeNode>();
            List<MazeNode> visitedNodes = new List<MazeNode>();
            createNodes(nodeList);

            MazeNode c = nodeList[0];
            c.HasVisit = true;
            visitedNodes.Add(c);
            int count = nodeList.Count - 1;
            while (count > 0)
            {
                MazeNode next = c.findNextNode(m_random);
                if (next != null)
                {
                    --count;
                    breakToNext(c, next);
                    c = next;
                    visitedNodes.Add(c);
                }
                else
                {
                    if (visitedNodes.Count < 2)
                    {
                        break;
                    }
                    c = visitedNodes[visitedNodes.Count - 2];
                    visitedNodes.RemoveAt(visitedNodes.Count - 1);
                }
            }

            fillMaze();
            breakWalls();
            m_random.disableContinous();
        }

        private bool checkCanBeBroke(int index)
        {
            int x = 0;
            int y = 0;
            getXY(index, ref x, ref y);
            int count = 0;
            if (getTile(x, y + 1) == EMPTY)
            {
                ++count;
            }
            if (count >= m_breakingLimit)
            {
                return true;
            }
            if (getTile(x + 1, y) == EMPTY)
            {
                ++count;
            }
            if (count >= m_breakingLimit)
            {
                return true;
            }
            if (getTile(x - 1, y) == EMPTY)
            {
                ++count;
            }
            if (count >= m_breakingLimit)
            {
                return true;
            }
            if (getTile(x, y - 1) == EMPTY)
            {
                ++count;
            }
            return count >= m_breakingLimit;
        }

        private void breakWalls()
        {
            if (m_breakingWallRate <= 0)
            {
                return;
            }
            List<int> filledList = new List<int>();
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                if (m_matrix[i] == FILLED)
                {
                    filledList.Add(i);
                }
            }
            LMath.shuffleList<int>(filledList, m_random);
            int breakN = (int)(filledList.Count * m_breakingWallRate);
            while (breakN > 0 && filledList.Count > 0)
            {
                bool find = false;
                for (int i = 0; i < filledList.Count; ++i)
                {
                    if (checkCanBeBroke(filledList[i]))
                    {
                        m_matrix[filledList[i]] = EMPTY;
                        --breakN;
                        int last = filledList.Count - 1;
                        filledList[i] = filledList[last];
                        filledList.RemoveAt(last);
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the x and y coordinate from the index
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void getXY(int index, ref int x, ref int y)
        {
            y = index / m_width;
            x = index - m_width * y;
        }

        private int checkFillable(int index)
        {
            int y = 0;
            int x = 0;
            getXY(index, ref x, ref y);

            if (m_hasStartEnd)
            {
                if (x == m_start.x && y == m_start.y)
                {
                    return -1;
                }
                if (x == m_end.x && y == m_end.y)
                {
                    return -1;
                }
            }
            byte b = getTile(x, y + 1);
            int count = 0;
            int ret = -1;
            if (b != FILLED && b != NONE)
            {
                ++count;
                ret = 0;
            }
            b = getTile(x + 1, y);
            if (b != FILLED && b != NONE)
            {
                ++count;
                ret = 1;
            }
            if (count > 1)
            {
                return -1;
            }
            b = getTile(x, y - 1);
            if (b != FILLED && b != NONE)
            {
                ++count;
                ret = 2;
            }
            if (count > 1)
            {
                return -1;
            }
            b = getTile(x - 1, y);
            if (b != FILLED && b != NONE)
            {
                ++count;
                ret = 3;
            }
            if (count > 1)
            {
                return -1;
            }
            return ret;
        }

        private int getNearIndex(int index, int dir)
        {
            int x = 0;
            int y = 0;
            getXY(index, ref x, ref y);
            switch (dir)
            {
                case 0:
                    return tileIndex(x, y + 1);
                case 1:
                    return tileIndex(x + 1, y);
                case 2:
                    return tileIndex(x, y - 1);
                case 3:
                    return tileIndex(x - 1, y);
                default:
                    return -1;
            }
        }

        private void fillMaze()
        {
            if (m_fillRate <= 0)
            {
                return;
            }
            List<int> fillableList = new List<int>();
            List<int> fillDirList = new List<int>();
            int count = 0;
            for (int i = 0; i < m_matrix.Length; ++i)
            {
                if (m_matrix[i] == EMPTY)
                {
                    ++count;
                    int d = checkFillable(i);
                    if (d >= 0)
                    {
                        fillDirList.Add(d);
                        fillableList.Add(i);
                    }
                }
            }
            int fillN = (int)(count * m_fillRate);
            while (fillN > 0 && fillableList.Count > 0)
            {
                int r = m_random.range(0, fillableList.Count);
                m_matrix[fillableList[r]] = FILLED;
                int last = fillableList.Count - 1;
                int near = getNearIndex(fillableList[r], fillDirList[r]);
                fillableList[r] = fillableList[last];
                fillDirList[r] = fillDirList[last];
                fillableList.RemoveAt(last);
                fillDirList.RemoveAt(last);
                int dir = checkFillable(near);
                if (dir >= 0)
                {
                    fillDirList.Add(dir);
                    fillableList.Add(near);
                }
                --fillN;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Map/Maze", false, (int)'m')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<Maze>(true);
        }

        private void OnDrawGizmos()
        {
            if (m_matrix != null && m_matrix.Length == m_width * m_height)
            {
                Color co = Gizmos.color;
                Gizmos.color = Color.green;
                Vector3 s = new Vector3(m_width, m_height, 0);
                Vector3 c = s / 2;
                c.x -= 0.5f;
                c.y -= 0.5f;
                Gizmos.DrawWireCube(c, s);
                s.Set(1, 1, 0);

                if (m_hasStartEnd)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(new Vector3(m_start.x, m_start.y, 0), 0.5f);
                    Gizmos.DrawSphere(new Vector3(m_end.x, m_end.y, 0), 0.5f);
                }

                Vector3 p = Vector3.zero;
                for (int i = 0; i < m_height; ++i)
                {
                    for (int j = 0; j < m_width; ++j)
                    {
                        byte b = getTile(j, i);
                        if (b == FILLED)
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(p, s);
                        }
                        else if (b > OBSTACLE_START)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawCube(p, s);
                        }
                        p.x += 1;
                    }
                    p.y += 1;
                    p.x = 0;
                }
                if (m_items != null)
                {
                    Gizmos.color = Color.red;
                    for (int i = 0; i < m_items.Length; ++i)
                    {
                        if (m_items[i] > 0)
                        {
                            int x = 0;
                            int y = 0;
                            getXY(i, ref x, ref y);
                            Gizmos.DrawSphere(new Vector3(x, y, 0), 0.4f);
                        }
                    }
                }
                Gizmos.color = co;
            }
        }
#endif
    }
}
