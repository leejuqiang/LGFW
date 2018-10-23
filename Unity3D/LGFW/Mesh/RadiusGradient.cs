using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A radius gradient
    /// </summary>
    public class RadiusGradient : Gradient
    {

        /// <summary>
        /// The center of the gradient
        /// </summary>
        public GradientPoint m_center;

        /// <inheritdoc/>
        public override bool useGradient()
        {
            return base.useGradient() && m_center.Useful && m_points.Count >= 1;
        }

        public void createPoints()
        {
            createPoints(1);
        }

        /// <inheritdoc/>
        public override void init()
        {
            base.init();
            for (int i = 0; i < m_points.Count; ++i)
            {
                Vector3 p = m_points[i].m_position - m_center.m_position;
                p.z = 0;
                m_points[i].m_cache = p.magnitude;
            }
            m_points.Sort(sortPoints);
        }

        /// <inheritdoc/>
        public override Color getColor(Vector3 p, float alpha, int colorIndex)
        {
            p -= m_center.m_position;
            p.z = 0;
            float f = p.magnitude;
            Color c = Color.white;
            if (f <= 0)
            {
                c = m_center.m_colors[colorIndex];
            }
            else if (f >= m_points[m_points.Count - 1].m_cache)
            {
                c = m_points[m_points.Count - 1].m_colors[colorIndex];
            }
            else
            {
                for (int i = 0; i < m_points.Count; ++i)
                {
                    if (m_points[i].m_cache >= f)
                    {
                        GradientPoint gp = i <= 0 ? m_center : m_points[i - 1];
                        f = LMath.lerpValue(gp.m_cache, m_points[i].m_cache, f);
                        f = Mathf.Clamp01(f);
                        c = Color.Lerp(gp.m_colors[colorIndex], m_points[i].m_colors[colorIndex], f);
                        break;
                    }
                }
            }
            c.a *= alpha;
            return c;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Gradient/Radius", false, (int)'l')]
        public static void addToGameObjects()
        {
            RadiusGradient[] rg = LEditorKits.addComponentToSelectedOjbects<RadiusGradient>(true);
            for (int i = 0; i < rg.Length; ++i)
            {
                rg[i].createPoints();
            }
        }

        protected override void OnDrawGizmos()
        {
            Color c = Gizmos.color;
            if (UnityEditor.Selection.activeGameObject == this.gameObject)
            {
                drawPoint(m_center, this.transform);
            }
            base.OnDrawGizmos();
            Gizmos.color = c;
        }
#endif
    }
}
