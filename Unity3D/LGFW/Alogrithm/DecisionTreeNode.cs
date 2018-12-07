using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for a decision tree node
    /// </summary>
    /// <typeparam name="T">The type of the data in this tree</typeparam>
    public abstract class DecisionTreeNode<T>
    {
        protected HashSet<T> m_data;

        protected List<DecisionTreeNode<T>> m_children;
        protected DecisionTreeNode<T> m_parent;

        protected int m_depth;

        /// <summary>
        /// If the node is a leaf
        /// </summary>
        /// <value>True if it's a leaf</value>
        public virtual bool IsLeaf
        {
            get { return m_children.Count <= 0; }
        }

        /// <summary>
        /// Gets the children of this node
        /// </summary>
        /// <value>The children</value>
        public List<DecisionTreeNode<T>> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// Gets the data set of this node
        /// </summary>
        /// <value>The data set</value>
        public HashSet<T> AllData
        {
            get { return m_data; }
        }

        /// <summary>
        /// The depth of this node
        /// </summary>
        /// <value>The depth</value>
        public int Depth
        {
            get { return m_depth; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="depth">The depth of this node</param>
        public DecisionTreeNode(int depth)
        {
            m_depth = depth;
            m_children = new List<DecisionTreeNode<T>>();
            m_parent = null;
        }

        /// <summary>
        /// Removes a data from the node
        /// </summary>
        /// <param name="data"></param>
        public virtual void removeData(T data)
        {
            m_data.Remove(data);
        }

        /// <summary>
        /// Gets the leaf which this data belongs
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The leaf</returns>
        public abstract DecisionTreeNode<T> getLeaf(T data);

        /// <summary>
        /// Adds a data to the subtree with this node as the root
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The leaf this data added to</returns>
        public virtual DecisionTreeNode<T> addData(T data)
        {
            if (IsLeaf)
            {
                m_data.Add(data);
                return this;
            }
            DecisionTreeNode<T> n = getLeaf(data);
            if (n != null)
            {
                n.m_data.Add(data);
            }
            return n;
        }
    }
}
