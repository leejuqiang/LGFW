using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A list
    /// </summary>
    public class UIList : UITouchWidget
    {

        private const float m_minBounceOffset = 5;
        private const float m_minScrollOffset = 2;
        private const float m_minAlignOffset = 2;
        private const float m_bounceOffsetSpring = 0.3f;
        private const float m_scrollOffsetSpring = 0.1f;
        private const float m_alignOffsetSpring = 0.1f;

        /// <summary>
        /// The prefab for cells
        /// </summary>
        public UIListCell[] m_cellPrefabs;
        /// <summary>
        /// The scroll bar of this list
        /// </summary>
        public UISlide m_scrollBar;

        [SerializeField]
        protected float m_size = 100;
        /// <summary>
        /// The z offset for all cells
        /// </summary>
        public float m_offsetZ;
        /// <summary>
        /// If this list is vertical
        /// </summary>
        public bool m_isVertical = true;
        /// <summary>
        /// If this list is a loop list
        /// </summary>
        public bool m_isLoop;
        /// <summary>
        /// If true, the list will always stop at the top of a cell
        /// </summary>
        public bool m_alignToCell;
        /// <summary>
        /// If true, the list can't be scrolled when the cells fit the size
        /// </summary>
        public bool m_lockWhenFit = true;

        private Transform m_offsetTrans;
        private Transform m_trans;
        private Vector3 m_lastTouch;
        private UIClip m_clip;
        private BoxCollider2D m_collider;
        private List<GameObjectPool<UIListCell>> m_cellPools = new List<GameObjectPool<UIListCell>>();
        private Vector2 m_visibleRange;
        private LinkedList<UIListCell> m_allCells = new LinkedList<UIListCell>();
        private int m_dataLength;
        private float m_scrollSpeed;
        private float m_lastOffset;
        private float m_contentSize;
        private bool m_reachHead;
        private bool m_reachEnd;

        /// <summary>
        /// Gets the prefab index
        /// </summary>
        /// <param name="dataIndex">The index of the data</param>
        /// <returns>The index of the cell prefab</returns>
        public delegate int GetCellIDByData(int dataIndex);
        /// <summary>
        /// Gets the data 
        /// </summary>
        /// <param name="dataIndex">The index of the data</param>
        /// <returns>The data</returns>
        public delegate object GetDataByIndex(int dataIndex);

        /// <summary>
        /// Called when trying to create a new cell
        /// </summary>
        public GetCellIDByData m_getCellIdByData;
        /// <summary>
        /// Called when trying to create a new cell
        /// </summary>
        public GetDataByIndex m_getDataByIndex;

        /// <summary>
        /// Resets the list
        /// </summary>
        public void resetList()
        {
            resetList(m_dataLength);
        }

        /// <summary>
        /// The size of the list
        /// </summary>
        /// <value></value>
        public float Size
        {
            get { return m_size; }
            set
            {
                if (m_size != value)
                {
                    m_size = value;
                    resetList();
                }
            }
        }

        /// <summary>
        /// Resets the list
        /// </summary>
        /// <param name="dataLength">The length of the data list</param>
        public void resetList(int dataLength)
        {
            m_dataLength = dataLength;
            m_offsetTrans.localPosition = new Vector3(0, 0, m_offsetZ);
            while (m_allCells.Count > 0)
            {
                freeACell(m_allCells.First.Value);
                m_allCells.RemoveFirst();
            }
            if (m_cellPrefabs.Length == 1)
            {
                m_contentSize = m_dataLength * m_cellPrefabs[0].Size;
            }
            else
            {
                m_contentSize = 0;
                for (int i = 0; i < m_dataLength; ++i)
                {
                    int id = cellIdByDataIndex(i);
                    m_contentSize += m_cellPrefabs[id].Size;
                }
            }
            m_lastOffset = -1;
            updateOffset();
            computeScrollBarSize();
        }

        private void OnUIScaleUpdate(UIScale scale)
        {
            if (m_isVertical)
            {
                if (!scale.m_justChangeX)
                {
                    Size = scale.getSize(0).y;
                }
            }
            else
            {
                Size = scale.getSize(0).x;
            }
        }

        private void computeScrollBarSize()
        {
            if (m_scrollBar != null)
            {
                m_scrollBar.m_isVertical = m_isVertical;
                if (m_scrollBar.m_backGround != null && m_scrollBar.m_sliderBlock != null)
                {
                    if (m_contentSize <= m_size || m_isLoop)
                    {
                        m_scrollBar.gameObject.SetActive(false);
                    }
                    else
                    {
                        m_scrollBar.gameObject.SetActive(true);
                        float f = m_size / m_contentSize;
                        Vector2 bs = m_scrollBar.m_backGround.Size;
                        Vector2 s = m_scrollBar.m_sliderBlock.Size;
                        if (m_isVertical)
                        {
                            s.y = bs.y * f;
                        }
                        else
                        {
                            s.x = bs.x * f;
                        }
                        m_scrollBar.m_sliderBlock.Size = s;
                        m_scrollBar.Value = m_isVertical ? 1 : 0;
                        m_scrollBar.computeRange();
                    }
                }
                else
                {
                    m_scrollBar.gameObject.SetActive(false);
                }
            }
        }

        private void updateScrollBarPosition()
        {
            if (m_scrollBar != null && !m_isLoop && m_scrollBar.gameObject.activeSelf)
            {
                Vector3 p = m_offsetTrans.localPosition;
                float f = 0;
                if (m_isVertical)
                {
                    f = p.y;
                }
                else
                {
                    f = -p.x;
                }
                f = LMath.lerpValue(0, m_contentSize - m_size, f);
                if (m_isVertical)
                {
                    f = 1 - f;
                }
                m_scrollBar.Value = f;
            }
        }

        public void resizeColliderAndClip()
        {
            m_collider = this.GetComponent<BoxCollider2D>();
            if (m_collider != null)
            {
                Vector2 s = m_collider.size;
                if (m_isVertical)
                {
                    m_collider.offset = new Vector2(0, -m_size * 0.5f);
                    s.y = m_size;
                    m_collider.size = s;
                }
                else
                {
                    m_collider.offset = new Vector2(m_size * 0.5f, 0);
                    s.x = m_size;
                    m_collider.size = s;
                }
            }
            m_clip = this.GetComponent<UIClip>();
            if (m_clip != null)
            {
                Vector2 s = m_clip.Size;
                if (m_isVertical)
                {
                    m_clip.Center = new Vector2(0, -m_size * 0.5f);
                    s.y = m_size;
                    m_clip.Size = s;
                }
                else
                {
                    m_clip.Center = new Vector2(m_size * 0.5f, 0);
                    s.x = m_size;
                    m_clip.Size = s;
                }
            }
        }

        private void initOffsetTrans()
        {
            if (m_offsetTrans == null)
            {
                GameObject go = new GameObject("offset");
                m_offsetTrans = go.transform;
            }
            m_offsetTrans.parent = this.transform;
            Vector3 p = Vector3.zero;
            p.z = m_offsetZ;
            if (m_isVertical)
            {
                p.y = m_size * 0.5f;
                p.x = 0;
            }
            else
            {
                p.x = -m_size * 0.5f;
                p.y = 0;
            }
            m_offsetTrans.localPosition = p;
            m_offsetTrans.localScale = Vector3.one;
            m_offsetTrans.localRotation = Quaternion.identity;
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_trans = this.transform;
            m_clip = this.GetComponent<UIClip>();
            m_collider = this.GetComponent<BoxCollider2D>();
            for (int i = 0; i < m_cellPrefabs.Length; ++i)
            {
                m_cellPools.Add(new GameObjectPool<UIListCell>(m_cellPrefabs[i].gameObject));
            }
            initOffsetTrans();
        }

        private void freeACell(UIListCell c)
        {
            m_cellPools[c.ID].freeGameObject(c);
        }

        private int cellIdByDataIndex(int index)
        {
            if (index < 0 || index >= m_dataLength)
            {
                return -1;
            }
            if (m_getCellIdByData != null)
            {
                return m_getCellIdByData(index);
            }
            return 0;
        }

        private UIListCell getACell(int dataIndex)
        {
            if (dataIndex < 0)
            {
                if (m_isLoop)
                {
                    dataIndex = (-dataIndex) % m_dataLength;
                    dataIndex = dataIndex == 0 ? 0 : m_dataLength - dataIndex;
                }
                else
                {
                    return null;
                }
            }
            else if (dataIndex >= m_dataLength)
            {
                if (m_isLoop)
                {
                    dataIndex %= m_dataLength;
                }
                else
                {
                    return null;
                }
            }
            int id = cellIdByDataIndex(dataIndex);
            bool isNew = false;
            UIListCell c = m_cellPools[id].getAnObject(out isNew);
            c.ID = id;
            c.DataIndex = dataIndex;
            c.Awake();
            c.initTrans(m_offsetTrans);
            object obj = null;
            if (m_getDataByIndex != null)
            {
                obj = m_getDataByIndex(dataIndex);
            }
            c.setupCell(obj);
            if (isNew && m_clip != null)
            {
                m_clip.addGameObjectToClip(c.gameObject, true);
            }
            return c;
        }

        private bool isLock()
        {
            return m_lockWhenFit && m_contentSize <= m_size;
        }

        protected override void doPress(UITouch t)
        {
            base.doPress(t);
            Vector3 v = t.m_cameraRay.CameraAttached.ScreenToWorldPoint(t.m_screenPosition);
            m_lastTouch = m_trans.InverseTransformPoint(v);
        }

        protected override void doDrag(UITouch t)
        {
            base.doDrag(t);
            if (!isLock())
            {
                Vector3 p = t.m_cameraRay.CameraAttached.ScreenToWorldPoint(t.m_screenPosition);
                p = m_trans.InverseTransformPoint(p);
                Vector3 v = m_offsetTrans.localPosition;
                if (m_isVertical)
                {
                    m_scrollSpeed = p.y - m_lastTouch.y;
                    if (m_scrollSpeed != 0)
                    {
                        if (m_scrollSpeed > 0)
                        {
                            if (m_reachEnd)
                            {
                                m_scrollSpeed *= 0.5f;
                            }
                        }
                        else
                        {
                            if (m_reachHead)
                            {
                                m_scrollSpeed *= 0.5f;
                            }
                        }
                        v.y += m_scrollSpeed;
                        m_offsetTrans.localPosition = v;
                        updateOffset();
                    }
                }
                else
                {
                    m_scrollSpeed = p.x - m_lastTouch.x;
                    if (m_scrollSpeed != 0)
                    {
                        if (m_scrollSpeed > 0)
                        {
                            if (m_reachHead)
                            {
                                m_scrollSpeed *= 0.5f;
                            }
                        }
                        else
                        {
                            if (m_reachEnd)
                            {
                                m_scrollSpeed *= 0.5f;
                            }
                        }
                        v.x += m_scrollSpeed;
                        m_offsetTrans.localPosition = v;
                        updateOffset();
                    }
                }
                m_lastTouch = p;
            }
        }

        private void freeInvisibleCells()
        {
            LinkedListNode<UIListCell> n = m_allCells.First;
            while (n != null)
            {
                LinkedListNode<UIListCell> temp = n;
                n = n.Next;
                temp.Value.updateVisible(m_visibleRange, m_isVertical);
                if (temp.Value.Visible == UIListCellVisible.invisible)
                {
                    m_allCells.Remove(temp);
                    freeACell(temp.Value);
                }
            }
        }

        private void addCellsForEmptySpace()
        {
            if (m_allCells.Count <= 0)
            {
                return;
            }
            LinkedListNode<UIListCell> n = m_allCells.First;
            while (n.Value.Visible == UIListCellVisible.fullyVisible || n.Value.Visible == UIListCellVisible.halfVisibleEnd)
            {
                UIListCell c = getACell(m_allCells.First.Value.DataIndex - 1);
                if (c == null)
                {
                    break;
                }
                float v = 0;
                if (m_isVertical)
                {
                    if (n != null)
                    {
                        v = n.Value.TopPosition + c.Size;
                    }
                }
                else
                {
                    if (n != null)
                    {
                        v = n.Value.LeftPosition - c.Size;
                    }
                }
                c.setPosition(v, m_isVertical);
                c.updateVisible(m_visibleRange, m_isVertical);
                m_allCells.AddFirst(c);
                n = m_allCells.First;
            }

            n = m_allCells.Last;
            while (n.Value.Visible == UIListCellVisible.fullyVisible || n.Value.Visible == UIListCellVisible.halfVisibleHead)
            {
                UIListCell c = getACell(m_allCells.Last.Value.DataIndex + 1);
                if (c == null)
                {
                    break;
                }
                float v = 0;
                if (m_isVertical)
                {
                    if (n != null)
                    {
                        v = n.Value.BottomPosition;
                    }
                }
                else
                {
                    if (n != null)
                    {
                        v = n.Value.RightPosition;
                    }
                }
                c.setPosition(v, m_isVertical);
                c.updateVisible(m_visibleRange, m_isVertical);
                m_allCells.AddLast(c);
                n = m_allCells.Last;
            }
        }

        private void recomputeCells(float offset)
        {
            int index = 0;
            float len = 0;
            if (offset >= 0)
            {
                if (m_cellPrefabs.Length == 1)
                {
                    index = (int)(offset / m_cellPrefabs[0].Size);
                    len = index * m_cellPrefabs[0].Size;
                }
                else
                {
                    len = 0;
                    while (true)
                    {
                        int id = cellIdByDataIndex(index);
                        if (id < 0)
                        {
                            return;
                        }
                        float temp = len + m_cellPrefabs[id].Size;
                        if (temp > offset)
                        {
                            break;
                        }
                        len = temp;
                        ++index;
                    }
                }
            }
            if (m_isVertical)
            {
                len = -len;
            }
            UIListCell c = getACell(index);
            while (c != null)
            {
                c.setPosition(len, m_isVertical);
                c.updateVisible(m_visibleRange, m_isVertical);
                m_allCells.AddLast(c);
                if (c.Visible == UIListCellVisible.invisible)
                {
                    break;
                }
                ++index;
                if (m_isVertical)
                {
                    len -= c.Size;
                }
                else
                {
                    len += c.Size;
                }
                c = getACell(index);
            }
        }

        private bool bounce()
        {
            if (m_isLoop)
            {
                return false;
            }
            Vector3 p = m_offsetTrans.localPosition;
            float offset = 0;
            if (m_contentSize <= m_size || m_reachHead)
            {
                if (m_isVertical)
                {
                    offset = -p.y;
                }
                else
                {
                    offset = -p.x;
                }
            }
            else if (m_reachEnd)
            {
                if (m_isVertical)
                {
                    offset = m_contentSize - m_size - p.y;
                }
                else
                {
                    offset = -m_contentSize + m_size - p.x;
                }
            }
            else
            {
                return false;
            }
            if (Mathf.Abs(offset) > m_minBounceOffset)
            {
                offset *= m_bounceOffsetSpring;
            }
            if (m_isVertical)
            {
                p.y += offset;
            }
            else
            {
                p.x += offset;
            }
            m_offsetTrans.localPosition = p;
            updateOffset();
            return true;
        }

        private void alignToCell()
        {
            if (m_allCells.Count <= 0)
            {
                return;
            }
            LinkedListNode<UIListCell> n = m_allCells.First;
            while (n.Value.Visible == UIListCellVisible.invisible)
            {
                n = n.Next;
            }
            if (n.Value.Visible == UIListCellVisible.fullyVisible)
            {
                return;
            }
            float f = 0;
            Vector3 p = m_offsetTrans.localPosition;
            if (m_isVertical)
            {
                f = p.y + n.Value.TopPosition;
                if (n.Next != null)
                {
                    float f1 = n.Next.Value.TopPosition + p.y;
                    if (Mathf.Abs(f1) < Mathf.Abs(f))
                    {
                        n = n.Next;
                        f = f1;
                    }
                }
                if (Mathf.Abs(f) > m_minAlignOffset)
                {
                    f *= m_alignOffsetSpring;
                }
                p.y -= f;
            }
            else
            {
                f = p.x + n.Value.LeftPosition;
                if (n.Next != null)
                {
                    float f1 = n.Next.Value.LeftPosition + p.x;
                    if (Mathf.Abs(f1) < Mathf.Abs(f))
                    {
                        n = n.Next;
                        f = f1;
                    }
                }
                if (Mathf.Abs(f) > m_minAlignOffset)
                {
                    f *= m_alignOffsetSpring;
                }
                p.x -= f;
            }
            m_offsetTrans.localPosition = p;
            updateOffset();
        }

        private void slide()
        {
            float f = m_scrollSpeed;
            if (Mathf.Abs(f) > m_minScrollOffset)
            {
                f *= m_scrollOffsetSpring;
            }
            m_scrollSpeed -= f;
            Vector3 p = m_offsetTrans.localPosition;
            if (m_isVertical)
            {
                p.y += f;
            }
            else
            {
                p.x += f;
            }
            m_offsetTrans.localPosition = p;
            updateOffset();
        }

        void LateUpdate()
        {
            if (!IsTouching)
            {
                if (m_dataLength == 0)
                {
                    return;
                }
                if (bounce())
                {
                    m_scrollSpeed = 0;
                    return;
                }
                if (m_alignToCell)
                {
                    alignToCell();
                }
                else
                {
                    slide();
                }
            }
        }

        private void updateOffset()
        {
            if (m_dataLength <= 0)
            {
                return;
            }
            Vector3 p = m_offsetTrans.localPosition;
            float offset = m_isVertical ? p.y : -p.x;
            if (offset == m_lastOffset)
            {
                return;
            }
            m_lastOffset = offset;
            if (m_isVertical)
            {
                m_visibleRange.x = -p.y - m_size;
                m_visibleRange.y = -p.y;
            }
            else
            {
                m_visibleRange.x = -p.x;
                m_visibleRange.y = m_size - p.x;
            }
            freeInvisibleCells();
            if (m_allCells.Count <= 0)
            {
                recomputeCells(offset);
            }
            else
            {
                addCellsForEmptySpace();
            }
            if (m_isLoop)
            {
                m_reachEnd = false;
                m_reachHead = false;
            }
            else
            {
                if (m_isVertical)
                {
                    m_reachHead = p.y < 0;
                    m_reachEnd = p.y - m_contentSize > -m_size;
                }
                else
                {
                    m_reachHead = p.x > 0;
                    m_reachEnd = p.x + m_contentSize < m_size;
                }
            }
            updateScrollBarPosition();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIList", false, (int)'l')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UIList>(true);
            LEditorKits.addComponentToSelectedObjects<BoxCollider2D>(true);
            UIClip[] cs = LEditorKits.addComponentToSelectedObjects<UIClip>(true);
            for (int i = 0; i < cs.Length; ++i)
            {
                cs[i].createDefaultShaderPairs();
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject != this.gameObject)
            {
                return;
            }
            Color c = Gizmos.color;
            Gizmos.color = Color.red;
            Vector3 v1 = Vector3.zero;
            Vector3 v2 = Vector3.zero;
            if (m_isVertical)
            {
                v2.y = -m_size;
            }
            else
            {
                v2.x = m_size;
            }
            Transform t = this.transform;
            v1 = t.TransformPoint(v1);
            v2 = t.TransformPoint(v2);
            Gizmos.DrawLine(v1, v2);
            Gizmos.color = c;
        }
#endif
    }
}
