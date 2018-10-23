using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A gradient point
    /// </summary>
    [System.Serializable]
    public class GradientPoint
    {
        /// <summary>
        /// The position of the point
        /// </summary>
        public Vector3 m_position;
        /// <summary>
        /// The color of the point
        /// </summary>
        public Color[] m_colors;
        [System.NonSerialized]
        public float m_cache;

        public bool Useful
        {
            get { return m_colors != null && m_colors.Length > 0; }
        }
    }

    /// <summary>
    /// Base class for all gradient
    /// </summary>
    public abstract class Gradient : MonoBehaviour
    {

        /// <summary>
        /// If this gradient enabled
        /// </summary>
        public bool m_enable = true;

        /// <summary>
        /// All gradient points
        /// </summary>
        public List<GradientPoint> m_points = new List<GradientPoint>();

        private bool m_isChanged;

        /// <summary>
        /// If the gradient needs to update colors
        /// </summary>
        /// <value>If needs to update</value>
        public bool IsChanged
        {
            get
            {
                return useGradient() && m_isChanged;
            }

            set
            {
                m_isChanged = value;
            }
        }

        /// <summary>
        /// If this gradient is used
        /// </summary>
        /// <returns>If used</returns>
        public virtual bool useGradient()
        {
            if (m_points == null)
            {
                return false;
            }
            for (int i = 0; i < m_points.Count; ++i)
            {
                if (!m_points[i].Useful)
                {
                    return false;
                }
            }
            return m_enable;
        }

        protected Color findLerpColor(float f, int colorIndex, float alpha)
        {
            Color c = Color.white;
            if (f <= m_points[0].m_cache)
            {
                c = m_points[0].m_colors[colorIndex];
            }
            if (f >= m_points[m_points.Count - 1].m_cache)
            {
                c = m_points[m_points.Count - 1].m_colors[colorIndex];
            }
            for (int i = 1; i < m_points.Count; ++i)
            {
                if (f <= m_points[i].m_cache)
                {
                    f = LMath.lerpValue(m_points[i - 1].m_cache, m_points[i].m_cache, f);
                    f = Mathf.Clamp01(f);
                    c = Color.Lerp(m_points[i - 1].m_colors[colorIndex], m_points[i].m_colors[colorIndex], f);
                    break;
                }
            }
            c.a *= alpha;
            return c;
        }

        protected void createPoints(int num)
        {
            m_points = new List<GradientPoint>();
            for (int i = 0; i < num; ++i)
            {
                GradientPoint gp = new GradientPoint();
                gp.m_colors = new Color[1];
                gp.m_colors[0] = Color.white;
                m_points.Add(gp);
            }
        }

        protected int sortPoints(GradientPoint l, GradientPoint r)
        {
            return l.m_cache < r.m_cache ? -1 : 1;
        }

        /// <summary>
        /// Initialize this gradient
        /// </summary>
        public virtual void init()
        {
            IsChanged = true;
        }

        /// <summary>
        /// Gets the color by the position of a vertex
        /// </summary>
        /// <returns>The color</returns>
        /// <param name="p">The position</param>
        /// <param name="alpha">The alpha of the vertex</param>
        /// <param name="colorIndex">Which color will be used</param>
        public abstract Color getColor(Vector3 p, float alpha, int colorIndex);

#if UNITY_EDITOR
        protected void drawPoint(GradientPoint gp, Transform t)
        {
            if (gp.Useful)
            {
                Gizmos.color = gp.m_colors[0];
                Vector3 v = gp.m_position;
                v = t.TransformPoint(v);
                Gizmos.DrawSphere(v, 5);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject != this.gameObject)
            {
                return;
            }
            if (m_points != null)
            {
                Color c = Gizmos.color;
                Transform t = this.transform;
                for (int i = 0; i < m_points.Count; ++i)
                {
                    drawPoint(m_points[i], t);
                }
                Gizmos.color = c;
            }
        }
#endif
    }
}