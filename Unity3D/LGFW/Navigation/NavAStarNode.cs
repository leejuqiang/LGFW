using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for an a* star node
    /// </summary>
    public class NavAStarNode : GraphNode
    {
        /// <summary>
        /// The estimate cost to the end node, h value
        /// </summary>
        public float m_h;
        /// <summary>
        /// The cost of the node
        /// </summary>
        public float m_fixCost;
        /// <summary>
        /// The parent node
        /// </summary>
        public NavAStarNode m_parent;
        /// <summary>
        /// A flag to decide if the node is accessible
        /// </summary>
        public bool m_accessible;

        protected float m_g;

        protected float m_f;
        /// <summary>
        /// The f value, g + h
        /// </summary>
        /// <value></value>
        public float F
        {
            get { return m_f; }
        }

        /// <summary>
        ///  The total cost, g value
        /// </summary>
        /// <value></value>
        public float G
        {
            get { return m_g; }
            set
            {
                m_g = value;
                m_f = m_g + m_h;
            }
        }

        /// <summary>
        /// The distance to the end node
        /// </summary>
        public float m_range;

        public NavAStarNode(int id) : base(id)
        {

        }

        /// <summary>
        /// Computes the h value to the end node
        /// </summary>
        /// <param name="end">The end node</param>
        public virtual void computeH(NavAStarNode end)
        {
        }

        /// <summary>
        /// Computes the distance to the end node
        /// </summary>
        /// <param name="end">The end node</param>
        public virtual void computeRange(NavAStarNode end)
        {

        }

        /// <inheritdoc>
        public override bool Visited
        {
            get { return !m_accessible || m_visited; }
            set
            {
                m_visited = value;
            }
        }

        /// <summary>
        /// Resets the node
        /// </summary>
        public virtual void reset()
        {
            m_h = 0;
            m_parent = null;
            m_visited = false;
            m_g = 0;
            m_range = -1;
        }
    }
}
