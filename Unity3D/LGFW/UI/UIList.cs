using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Reflection;

namespace LGFW
{
    /// <summary>
    /// A list
    /// </summary>
    public class UIList : BaseMono
    {
        /// <summary>
        /// The prefab of the cell in this list
        /// </summary>
        public UIListCell m_cellPrefab;
        /// <summary>
        /// The parent transform for all cells in this list
        /// </summary>
        public RectTransform m_listAnchor;

        /// <summary>
        /// If the list is a loop list
        /// </summary>
        public bool m_loopList;
        private bool m_vertical;
        private bool m_draging;

        private RectTransform m_trans;
        private UIListScrollRect m_scroll;
        private List<UIListCell> m_cells = new List<UIListCell>();
        private int m_dataListLength;
        private float m_minFactor;
        private float m_maxFactor;
        private float m_skipFactor;
        private float m_totalLength;
        private float m_speed;
        private Vector2 m_lastPostion;
        private Canvas m_canvas;

        protected override void doAwake()
        {
            m_trans = this.GetComponent<RectTransform>();
            m_scroll = this.GetComponent<UIListScrollRect>();
            m_canvas = this.GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// Gets a cell by its list index
        /// </summary>
        /// <param name="index">The list index</param>
        /// <typeparam name="T">The type of the cell, must be a subclass of UIListCell</typeparam>
        /// <returns>The cell, or null if index is incorrect</returns>
        public T getCellByIndex<T>(int index) where T : UIListCell
        {
            int len = m_dataListLength;
            if (m_loopList)
            {
                len *= 3;
            }
            if (index < 0 || index >= len)
            {
                return null;
            }
            return (T)m_cells[index];
        }

        /// <summary>
        /// Gets a cell by its data index
        /// </summary>
        /// <param name="index">The data index</param>
        /// <typeparam name="T">The type of the cell, must be a subclass of UIListCell</typeparam>
        /// <returns>The first cell with the data index, or null is the index is incorrect. If the list is a loop list, you should call getAllCellsByDataIndex()</returns>
        public T getCellByDataIndex<T>(int index) where T : UIListCell
        {
            if (index < 0 || index >= m_dataListLength)
            {
                return null;
            }
            return (T)m_cells[index];
        }

        /// <summary>
        /// Gets all cells has the same data index
        /// </summary>
        /// <param name="index">The data index</param>
        /// <typeparam name="T">The type of the cell, must be a subclass of UIListCell</typeparam>
        /// <returns>If the index is incorrect, returns null. If the list is a loop list, returns 3 cells, or 1 cell</returns>
        public T[] getAllCellsByDataIndex<T>(int index) where T : UIListCell
        {

            if (index < 0 || index >= m_dataListLength)
            {
                return null;
            }
            T[] ret;
            if (m_loopList)
            {
                ret = new T[3];
                ret[0] = (T)m_cells[index];
                index += m_dataListLength;
                ret[1] = (T)m_cells[index];
                index += m_dataListLength;
                ret[2] = (T)m_cells[index];
            }
            else
            {
                ret = new T[1];
                ret[0] = (T)m_cells[index];
            }
            return ret;
        }

        private void createList(int number)
        {
            while (m_cells.Count < number)
            {
                GameObject go = GameObject.Instantiate<GameObject>(m_cellPrefab.gameObject, m_listAnchor);
                var c = go.GetComponent<UIListCell>();
                c.CellIndex = m_cells.Count;
                m_cells.Add(c);
            }
        }

        /// <summary>
        /// Sets the data of this list, each element of data is a cell
        /// </summary>
        /// <param name="data">The data list</param>
        /// <typeparam name="T">The type of the data</typeparam>
        public void setDataList<T>(T[] data)
        {
            Awake();
            m_dataListLength = data.Length;
            if (m_scroll == null || m_canvas == null)
            {
                m_loopList = false;
            }
            else
            {
                m_scroll.LoopList = m_loopList;
                m_vertical = m_scroll.vertical;
            }
            if (m_loopList)
            {
                T[] temp = new T[data.Length * 3];
                System.Array.Copy(data, temp, m_dataListLength);
                System.Array.Copy(data, 0, temp, m_dataListLength, m_dataListLength);
                System.Array.Copy(data, 0, temp, m_dataListLength << 1, m_dataListLength);
                data = temp;
            }
            createList(data.Length);
            int di = 0;
            for (int i = 0; i < data.Length; ++i, ++di)
            {
                if (di >= m_dataListLength)
                {
                    di = 0;
                }
                m_cells[i].gameObject.SetActive(true);
                m_cells[i].Awake();
                m_cells[i].DataIndex = di;
                m_cells[i].setData(data[i]);
            }
            for (int i = data.Length; i < m_cells.Count; ++i)
            {
                m_cells[i].gameObject.SetActive(false);
            }
            m_flag |= 0x10;
        }

        private void computeBound()
        {
            if ((m_flag & 0x10) != 0)
            {
                m_flag &= ~0x10;
                if (m_vertical)
                {
                    float size = m_listAnchor.rect.height;
                    m_totalLength = size - m_trans.rect.height;
                    size /= 3;
                    m_skipFactor = size / m_totalLength;
                    m_maxFactor = 1 - m_skipFactor;
                    m_minFactor = 1 - (size * 2 - m_trans.rect.height) / m_totalLength;
                }
                else
                {
                    float size = m_listAnchor.rect.width;
                    m_totalLength = size - m_trans.rect.width;
                    size /= 3;
                    m_skipFactor = size / m_totalLength;
                    m_minFactor = m_skipFactor;
                    m_maxFactor = (size * 2 - m_trans.rect.width) / m_totalLength;
                }
            }
        }

        public void onValueChange(Vector2 v)
        {
            if (!m_loopList)
            {
                return;
            }
            computeBound();
            Vector2 vol = m_scroll.velocity;
            if (m_vertical)
            {
                if (vol.y >= 0)
                {
                    if (v.y < m_minFactor)
                    {
                        float p = m_scroll.verticalNormalizedPosition;
                        p += m_skipFactor;
                        m_scroll.verticalNormalizedPosition = p;
                        m_scroll.velocity = vol;
                    }
                }
                else
                {
                    if (v.y > m_maxFactor)
                    {
                        float p = m_scroll.verticalNormalizedPosition;
                        p -= m_skipFactor;
                        m_scroll.verticalNormalizedPosition = p;
                        m_scroll.velocity = vol;
                    }
                }
            }
            else
            {
                if (vol.x <= 0)
                {
                    if (v.x > m_maxFactor)
                    {
                        float p = m_scroll.horizontalNormalizedPosition;
                        p -= m_skipFactor;
                        m_scroll.horizontalNormalizedPosition = p;
                        m_scroll.velocity = vol;
                    }
                }
                else
                {
                    if (v.x < m_minFactor)
                    {
                        float p = m_scroll.horizontalNormalizedPosition;
                        p += m_skipFactor;
                        m_scroll.horizontalNormalizedPosition = p;
                        m_scroll.velocity = vol;
                    }
                }
            }
        }

        private Vector2 translatePoint(Vector2 p)
        {
            Vector2 local;
            Camera c = (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : m_canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_trans, p, c, out local);
            return local;
        }

        public void onDrag(BaseEventData e)
        {
            if (m_loopList)
            {
                computeBound();
                PointerEventData pe = (PointerEventData)e;
                Vector2 p = translatePoint(pe.position);
                Vector2 delta = p - m_lastPostion;
                m_lastPostion = p;
                if (m_vertical)
                {
                    if (m_draging)
                    {
                        m_speed = delta.y / Time.deltaTime;
                        m_scroll.verticalNormalizedPosition -= delta.y / m_totalLength;
                    }
                }
                else
                {
                    if (m_draging)
                    {
                        m_speed = delta.x / Time.deltaTime;
                        m_scroll.horizontalNormalizedPosition -= delta.x / m_totalLength;
                    }
                }
            }
        }

        public void onPress(BaseEventData e)
        {
            if (m_loopList)
            {
                m_draging = true;
                PointerEventData pd = (PointerEventData)e;
                m_lastPostion = translatePoint(pd.position);
            }
        }

        public void onRelease(BaseEventData e)
        {
            if (m_loopList)
            {
                m_draging = false;
                PointerEventData pe = (PointerEventData)e;
                if (pe.IsPointerMoving())
                {
                    if (m_vertical)
                    {
                        m_scroll.velocity = new Vector2(0, m_speed);
                    }
                    else
                    {
                        m_scroll.velocity = new Vector2(m_speed, 0);
                    }
                }
                m_speed = 0;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIList Vertical", false, (int)'l')]
        public static void addListVToGameObjects()
        {
            addToGameObjects(false, true);
        }

        [UnityEditor.MenuItem("LGFW/UI/UIList Horizontal", false, (int)'l')]
        public static void addListHToGameObjects()
        {
            addToGameObjects(false, false);
        }

        [UnityEditor.MenuItem("LGFW/UI/UIList Loop Vertical", false, (int)'l')]
        public static void addLoopListVToGameObjects()
        {
            addToGameObjects(true, true);
        }

        [UnityEditor.MenuItem("LGFW/UI/UIList Loop Horizontal", false, (int)'l')]
        public static void addLoopListHToGameObjects()
        {
            addToGameObjects(true, false);
        }

        private static void addToGameObjects(bool loop, bool vertical)
        {
            GameObject parent = null;
            if (UnityEditor.Selection.gameObjects.Length >= 1)
            {
                parent = UnityEditor.Selection.gameObjects[0];
            }
            var go = new GameObject("list");
            var listT = go.AddComponent<RectTransform>();
            if (parent != null)
            {
                listT.SetParent(parent.transform, false);
            }
            var anchor = new GameObject("anchor");
            var anchorT = anchor.AddComponent<RectTransform>();
            var fitter = anchor.AddComponent<ContentSizeFitter>();
            anchorT.SetParent(listT, false);
            var list = go.AddComponent<UIList>();
            var sr = go.AddComponent<UIListScrollRect>();
            sr.content = anchorT;
            sr.viewport = listT;
            if (vertical)
            {
                sr.vertical = true;
                sr.horizontal = false;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var lay = anchor.AddComponent<VerticalLayoutGroup>();
                anchorT.pivot = new Vector2(0.5f, 1);
                lay.childForceExpandWidth = false;
                lay.childForceExpandHeight = false;
            }
            else
            {
                sr.vertical = false;
                sr.horizontal = true;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                var lay = anchor.AddComponent<HorizontalLayoutGroup>();
                anchorT.pivot = new Vector2(0, 0.5f);
                lay.childForceExpandWidth = false;
                lay.childForceExpandHeight = false;
            }

            var img = go.AddComponent<Image>();
            Color c = Color.white;
            c.a = 0;
            img.color = c;
            list.m_listAnchor = anchorT;
            go.AddComponent<RectMask2D>();

            if (loop)
            {
                list.m_loopList = true;
                System.Type lt = typeof(UIList);
                var minfo = lt.GetMethod("onValueChange", BindingFlags.Instance | BindingFlags.Public);
                UnityEngine.Events.UnityAction<Vector2> valueChange = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<Vector2>), list, minfo) as UnityEngine.Events.UnityAction<Vector2>;
                UnityEditor.Events.UnityEventTools.AddPersistentListener<Vector2>(sr.onValueChanged, valueChange);
                UnityEngine.EventSystems.EventTrigger trigger = go.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                minfo = lt.GetMethod("onDrag", BindingFlags.Instance | BindingFlags.Public);
                UnityEngine.Events.UnityAction<BaseEventData> drag = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<BaseEventData>), list, minfo) as UnityEngine.Events.UnityAction<BaseEventData>;
                var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener<BaseEventData>(entry.callback, drag);
                trigger.triggers.Add(entry);

                minfo = lt.GetMethod("onPress", BindingFlags.Instance | BindingFlags.Public);
                UnityEngine.Events.UnityAction<BaseEventData> press = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<BaseEventData>), list, minfo) as UnityEngine.Events.UnityAction<BaseEventData>;
                entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;
                entry.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener<BaseEventData>(entry.callback, press);
                trigger.triggers.Add(entry);

                minfo = lt.GetMethod("onRelease", BindingFlags.Instance | BindingFlags.Public);
                UnityEngine.Events.UnityAction<BaseEventData> release = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction<BaseEventData>), list, minfo) as UnityEngine.Events.UnityAction<BaseEventData>;
                entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                entry.eventID = EventTriggerType.EndDrag;
                entry.callback = new UnityEngine.EventSystems.EventTrigger.TriggerEvent();
                UnityEditor.Events.UnityEventTools.AddPersistentListener<BaseEventData>(entry.callback, release);
                trigger.triggers.Add(entry);
            }
        }
#endif
    }
}