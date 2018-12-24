using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for a a* star node
    /// </summary>
    public class NavAStarNode : MPItem
    {

        /// <summary>
        /// The id
        /// </summary>
        public int m_id;

        /// <summary>
        /// The g value
        /// </summary>
        public float m_g;
        /// <summary>
        /// The cost to the end node
        /// </summary>
        public float m_cost;
        /// <summary>
        /// The cost won't change
        /// </summary>
        public float m_fixCost;
        /// <summary>
        /// Used for linked list
        /// </summary>
        public NavAStarNode m_previous;
        /// <summary>
        /// Used for linked list
        /// </summary>
        public NavAStarNode m_next;

        /// <summary>
        /// The parent node
        /// </summary>
        public NavAStarNode m_parent;
        /// <summary>
        /// A flag to decide the node is in map
        /// </summary>
        public bool m_map;
        /// <summary>
        /// If the node is closed
        /// </summary>
        public bool m_close;

        /// <summary>
        /// The total cost, h value
        /// </summary>
        public float m_finalCost;

        /// <summary>
        /// Clears the node's flag
        /// </summary>
        /// <value></value>
        public virtual bool Clear
        {
            get { return m_map; }
        }

        /// <summary>
        /// Resets the nodes
        /// </summary>
        public virtual void reset()
        {
            m_close = false;
            m_g = -1;
            m_cost = -1;
            m_parent = null;
        }

        /// <summary>
        /// Computes the final cost
        /// </summary>
        public void computeCost()
        {
            m_finalCost = m_cost + m_g + m_fixCost;
        }

        public virtual string print()
        {
            return "";
        }
    }
}
