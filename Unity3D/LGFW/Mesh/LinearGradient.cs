using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A linear gradient
    /// </summary>
    public class LinearGradient : Gradient
    {

        /// <summary>
        /// If it's gradient vertically
        /// </summary>
        public bool m_isVertical;

        /// <inheritdoc/>
        public override bool useGradient()
        {
            return base.useGradient() && m_points.Count >= 2;
        }

        /// <summary>
        /// Creates the gradient points
        /// </summary>
        public void createPoints()
        {
            createPoints(2);
        }

        /// <inheritdoc/>
        public override void init()
        {
            base.init();
            if (m_isVertical)
            {
                for (int i = 0; i < m_points.Count; ++i)
                {
                    m_points[i].m_cache = m_points[i].m_position.y;
                }
            }
            else
            {
                for (int i = 0; i < m_points.Count; ++i)
                {
                    m_points[i].m_cache = m_points[i].m_position.x;
                }
            }
            m_points.Sort(sortPoints);
        }

        /// <inheritdoc/>
        public override Color getColor(Vector3 p, float alpha, int colorIndex)
        {
            Color c = Color.white;
            float f = m_isVertical ? p.y : p.x;
            return findLerpColor(f, colorIndex, alpha);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Gradient/Linear", false, (int)'l')]
        public static void addToGameObjects()
        {
            LinearGradient[] lg = LEditorKits.addComponentToSelectedObjects<LinearGradient>(true);
            for (int i = 0; i < lg.Length; ++i)
            {
                lg[i].createPoints();
            }
        }
#endif
    }
}
