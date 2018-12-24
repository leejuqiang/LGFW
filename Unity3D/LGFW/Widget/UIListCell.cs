using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum UIListCellVisible
    {
        invisible,
        fullyVisible,
        halfVisibleHead,
        halfVisibleEnd,
        halfVisibleBoth,
    }

    /// <summary>
    /// A cell in a list
    /// </summary>
    public class UIListCell : UIWidget, IMemoryPoolItem
    {

        [SerializeField]
        protected float m_anchor = 0.5f;
        [SerializeField]
        protected float m_size;

        protected Transform m_trans;
        protected Vector2 m_margin;
        protected UIListCellVisible m_visible;

        protected int m_id;
        protected bool m_isNew = true;

        /// <summary>
        /// If this item is just created or recycled
        /// </summary>
        /// <value></value>
        public bool IsNew
        {
            get { return m_isNew; }
        }

        /// <summary>
        /// The index of the cell
        /// </summary>
        /// <value></value>
        public int ID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// The index of the data of the cell
        /// </summary>
        /// <value></value>
        public int DataIndex
        {
            get;
            set;
        }

        /// <summary>
        /// The size of the cell
        /// </summary>
        /// <value></value>
        public float Size
        {
            get { return m_size; }
        }

        /// <summary>
        /// The position of the top of the cell
        /// </summary>
        /// <value></value>
        public float TopPosition
        {
            get
            {
                return m_trans.localPosition.y + m_margin.y;
            }
        }

        /// <summary>
        /// The position of the left of the cell
        /// </summary>
        /// <value></value>
        public float LeftPosition
        {
            get
            {
                return m_trans.localPosition.x - m_margin.x;
            }
        }

        /// <summary>
        /// The position of the bottom of the cell
        /// </summary>
        /// <value></value>
        public float BottomPosition
        {
            get
            {
                return m_trans.localPosition.y - m_margin.x;
            }
        }

        /// <summary>
        /// The position of the right of the cell
        /// </summary>
        /// <value></value>
        public float RightPosition
        {
            get
            {
                return m_trans.localPosition.x + m_margin.y;
            }
        }

        /// <summary>
        /// If the cell is visible
        /// </summary>
        /// <value></value>
        public UIListCellVisible Visible
        {
            get { return m_visible; }
        }

        public void initTrans(Transform t)
        {
            m_trans.parent = t;
            m_trans.localScale = Vector3.one;
            m_trans.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Sets the display of the cell
        /// </summary>
        /// <param name="data">The data for the cell</param>
        public virtual void setupCell(object data)
        {
            //todo
        }

        protected override void doAwake()
        {
            m_trans = this.transform;
            m_size = Mathf.Max(1, m_size);
            m_margin.x = m_size * m_anchor;
            m_margin.y = m_size - m_margin.x;
        }

        /// <summary>
        /// Sets the position of the cell
        /// </summary>
        /// <param name="v">The position</param>
        /// <param name="isVertical">If this is vertical list</param>
        public void setPosition(float v, bool isVertical)
        {
            Vector3 p = Vector3.zero;
            if (isVertical)
            {
                p.Set(0, v - m_margin.y, 0);
            }
            else
            {
                p.Set(v + m_margin.x, 0, 0);
            }
            m_trans.localPosition = p;
        }

        public void onInit()
        {

        }

        public void onClear()
        {
            m_isNew = false;
        }

        public void onDestroy()
        {
            GameObject.Destroy(this.gameObject);
        }

        public void updateVisible(Vector2 range, bool isVertical)
        {
            Vector3 p = m_trans.localPosition;
            float max, min;
            if (isVertical)
            {
                max = p.y + m_margin.y;
                min = p.y - m_margin.x;
            }
            else
            {
                max = p.x + m_margin.y;
                min = p.x - m_margin.x;
            }
            if (max < range.x || min > range.y)
            {
                m_visible = UIListCellVisible.invisible;
            }
            else if (max <= range.y && min >= range.x)
            {
                m_visible = UIListCellVisible.fullyVisible;
            }
            else
            {
                if (min < range.x && max > range.y)
                {
                    m_visible = UIListCellVisible.halfVisibleBoth;
                }
                else
                {
                    if (isVertical)
                    {
                        if (min < range.x)
                        {
                            m_visible = UIListCellVisible.halfVisibleEnd;
                        }
                        else
                        {
                            m_visible = UIListCellVisible.halfVisibleHead;
                        }
                    }
                    else
                    {
                        if (min < range.x)
                        {
                            m_visible = UIListCellVisible.halfVisibleHead;
                        }
                        else
                        {
                            m_visible = UIListCellVisible.halfVisibleEnd;
                        }
                    }
                }
            }
        }
    }
}
