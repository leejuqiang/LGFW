using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A data for the 3D space tree
    /// </summary>
    public class SpaceTreeData : MonoBehaviour, ISpaceTreeData
    {
        [SerializeField]
        protected Vector3 m_size;
        protected Transform m_trans;
        protected Vector3 m_halfSize;

        [SerializeField]
        protected SpaceTree m_spaceTree;

        /// <summary>
        /// The space tree for this data
        /// </summary>
        /// <value>The space tree</value>
        public SpaceTree Tree
        {
            get { return m_spaceTree; }
            set
            {
                if (m_spaceTree != value)
                {
                    if (m_spaceTree != null)
                    {
                        m_spaceTree.removeFromTree(this);
                    }
                    m_spaceTree = value;
                    if (m_spaceTree != null)
                    {
                        m_spaceTree.addToTree(this);
                    }
                }
            }
        }

        /// <summary>
        /// The size of this data
        /// </summary>
        /// <value>The size</value>
        public Vector3 Size
        {
            get { return m_size; }
            set
            {
                if (m_size != value)
                {
                    m_size = value;
                    m_halfSize = m_size * 0.5f;
                }
            }
        }

        /// <summary>
        /// The transform of the data
        /// </summary>
        /// <value>The transform</value>
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

        void Awake()
        {
            m_halfSize = m_size * 0.5f;
        }

        void Start()
        {
            if (m_spaceTree != null)
            {
                m_spaceTree.addToTree(this);
            }
        }

        /// <inheritdoc/>
        public virtual Vector3 getPosition()
        {
            return Trans.position;
        }
        /// <inheritdoc/>
        public virtual Vector3 getHalfSize()
        {
            return m_halfSize;
        }
        /// <inheritdoc/>
        public virtual Vector3 getMinPoint()
        {
            return getPosition() - m_halfSize;
        }
        /// <inheritdoc/>
        public virtual Vector3 getMaxPoint()
        {
            return getPosition() + m_halfSize;
        }

        /// <summary>
        /// Updates itself in the tree
        /// </summary>
        public void updateTree()
        {
            if (m_spaceTree != null)
            {
                m_spaceTree.updateData(this);
            }
        }

        /// <summary>
        /// Gets all the possible collision for this data
        /// </summary>
        /// <returns>The possible collision</returns>
        public List<ISpaceTreeData> getCollision()
        {
            if (m_spaceTree == null)
            {
                return null;
            }
            return m_spaceTree.getCollision(this);
        }
    }
}