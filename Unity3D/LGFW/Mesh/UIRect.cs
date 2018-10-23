using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum UIAlignmentX
    {
        left,
        center,
        right,
    }

    public enum UIAlignmentY
    {
        top,
        center,
        bottom,
    }

    /// <summary>
    /// The base calss for a rectangle shape component
    /// </summary>
    public class UIRect : MonoBehaviour
    {

        protected Transform m_trans;
        [SerializeField]
        protected Vector2 m_anchor = new Vector2(0.5f, 0.5f);
        [SerializeField]
        protected Vector2 m_size;
        protected bool m_hasAwake;

        protected Rect m_localPosition;
        protected int m_transChangeCount;

        /// <summary>
        /// The change id for the transform
        /// </summary>
        /// <value>The id</value>
        public int TransChangeCount
        {
            get { return m_transChangeCount; }
        }

        /// <summary>
        /// Gets local position of the rectangle
        /// </summary>
        /// <value>The local position</value>
        public Rect LocalPosition
        {
            get { return m_localPosition; }
        }

        /// <summary>
        /// Gets the Transform
        /// </summary>
        /// <value>The Transform</value>
        public Transform Trans
        {
            get
            {
                if (m_trans == null)
                {
                    m_trans = this.transform;
                }
                return m_trans;
            }
        }

        /// <summary>
        /// The size of the rectangle
        /// </summary>
        /// <value>The size</value>
        public virtual Vector2 Size
        {
            get { return m_size; }
            set
            {
                m_size = value;
                onSizeChanged();
            }
        }

        protected virtual void onSizeChanged()
        {
            updateLocalPosition();
            increaseChangeCount();
        }

        /// <summary>
        /// The anchor of the rectangle
        /// </summary>
        /// <value>The anchor</value>
        public virtual Vector2 Anchor
        {
            get { return m_anchor; }
            set
            {
                m_anchor = value;
                onAnchorChanged();
            }
        }

        protected virtual void onAnchorChanged()
        {
            updateLocalPosition();
            increaseChangeCount();
        }

        protected virtual void updateLocalPosition()
        {
            m_localPosition.xMin = -m_anchor.x * m_size.x;
            m_localPosition.yMin = -m_anchor.y * m_size.y;
            m_localPosition.xMax = m_localPosition.xMin + m_size.x;
            m_localPosition.yMax = m_localPosition.yMin + m_size.y;
        }

        /// <summary>
        /// Execute the Awake function of this component
        /// </summary>
        public void forceAwake()
        {
            m_hasAwake = false;
            Awake();
        }

        public virtual void Awake()
        {
            if (!m_hasAwake)
            {
                m_hasAwake = true;
                doAwake();
            }
        }

        protected void increaseChangeCount()
        {
            ++m_transChangeCount;
            if (m_transChangeCount <= 0)
            {
                m_transChangeCount = 1;
            }
        }

        public virtual void Update()
        {
            if (Trans.hasChanged)
            {
                increaseChangeCount();
                Trans.hasChanged = false;
            }
        }

        protected virtual void doAwake()
        {
            //todo
        }

        /// <summary>
        /// Gets the left bottom of the rectangle
        /// </summary>
        /// <value>The left bottom</value>
        public virtual Vector3 LeftBottom
        {
            get
            {
                return new Vector3(m_localPosition.xMin, m_localPosition.yMin, 0);
            }
        }

        /// <summary>
        /// Gets the right top of the rectangle
        /// </summary>
        /// <value>The right top</value>
        public virtual Vector3 RightTop
        {
            get
            {
                return new Vector3(m_localPosition.xMax, m_localPosition.yMax, 0);
            }
        }

        /// <summary>
        /// Gets the position in a local space
        /// </summary>
        /// <param name="t">The transform of the local space</param>
        /// <returns>The position</returns>
        public virtual Rect getPositionForLayout(Transform t)
        {
            if (Trans == t.parent)
            {
                return m_localPosition;
            }
            Vector3 v1 = LeftBottomInWorldSpace;
            Vector3 v2 = RightTopInWorldSpace;
            Rect rc = new Rect();
            if (t.parent != null)
            {
                v1 = t.parent.InverseTransformPoint(v1);
                v2 = t.parent.InverseTransformPoint(v2);
            }
            rc.xMin = v1.x;
            rc.yMin = v1.y;
            rc.xMax = v2.x;
            rc.yMax = v2.y;
            return rc;
        }

        /// <summary>
        /// Gets the left bottom of the rectangle in world space
        /// </summary>
        /// <value>The left bottom</value>
        public virtual Vector3 LeftBottomInWorldSpace
        {
            get
            {
                return Trans.TransformPoint(LeftBottom);
            }
        }

        /// <summary>
        /// Sets the left bottom of the rectangle in world space
        /// </summary>
        /// <param name="p">The position</param>
        /// <param name="changeZ">If true, the z value will be set too</param>
        public void setLeftBottomInWorldSpace(Vector3 p, bool changeZ)
        {
            Vector3 v = Trans.position;
            Vector3 lb = LeftBottomInWorldSpace;
            v.x += p.x - lb.x;
            v.y += p.y - lb.y;
            if (changeZ)
            {
                v.z = p.z;
            }
            Trans.position = v;
        }

        /// <summary>
        /// Gets the right top of the rectangle in world space
        /// </summary>
        /// <value>The right top</value>
        public virtual Vector3 RightTopInWorldSpace
        {
            get
            {
                return Trans.TransformPoint(RightTop);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject != this.gameObject)
            {
                return;
            }
            Color c = Gizmos.color;
            doDrawGizmos();
            Gizmos.color = c;
        }

        protected virtual void doDrawGizmos()
        {
            Vector3 v1 = new Vector3(m_localPosition.xMin, m_localPosition.yMin, 0);
            Vector3 v2 = new Vector3(m_localPosition.xMin, m_localPosition.yMax, 0);
            Vector3 v3 = new Vector3(m_localPosition.xMax, m_localPosition.yMax, 0);
            Vector3 v4 = new Vector3(m_localPosition.xMax, m_localPosition.yMin, 0);
            v1 = Trans.TransformPoint(v1);
            v2 = Trans.TransformPoint(v2);
            v3 = Trans.TransformPoint(v3);
            v4 = Trans.TransformPoint(v4);
            Gizmos.color = new Color(0.22f, 0.8f, 1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v1, v4);
            Gizmos.DrawLine(v3, v2);
            Gizmos.DrawLine(v3, v4);
        }
#endif
    }
}