using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for items used for reference memory pool
    /// </summary>
    public abstract class ReferenceMemoryPoolItem : MemoryPoolItem
    {
        protected int m_reference;

        /// <summary>
        /// Gets the reference of this item
        /// </summary>
        /// <value>The reference</value>
        public int Reference
        {
            get { return m_reference; }
        }

        /// <summary>
        /// Add reference
        /// </summary>
        public void retain()
        {
            ++m_reference;
        }

        /// <summary>
        /// Sub reference
        /// </summary>
        public void release()
        {
            --m_reference;
        }

        /// <inheritdoc/>
        public override void onInit()
        {
            base.onInit();
            m_reference = 1;
        }
    }

    /// <summary>
    /// A memory pool using reference count
    /// </summary>
    public class ReferenceMemoryPool
    {

        private LinkedList<ReferenceMemoryPoolItem> m_usedList = new LinkedList<ReferenceMemoryPoolItem>();
        private LinkedList<ReferenceMemoryPoolItem> m_freeList = new LinkedList<ReferenceMemoryPoolItem>();

        /// <summary>
        /// Gets an item, the item's reference is 0, make sure to call retain to keep this item
        /// </summary>
        /// <returns>The item</returns>
        /// <typeparam name="T">The type of the item</typeparam>
        public T getAnItem<T>() where T : ReferenceMemoryPoolItem, new()
        {
            ReferenceMemoryPoolItem ret = null;
            if (m_freeList.Count <= 0)
            {
                ret = new T();
            }
            else
            {
                ret = m_freeList.First.Value;
                m_freeList.RemoveFirst();
            }
            ret.onInit();
            m_usedList.AddLast(ret);
            return (T)ret;
        }

        /// <summary>
        /// Call update so the released items will be reclaimed
        /// </summary>
        public void update()
        {
            LinkedListNode<ReferenceMemoryPoolItem> n = m_usedList.First;
            while (n != null)
            {
                LinkedListNode<ReferenceMemoryPoolItem> temp = n.Next;
                if (n.Value.Reference <= 0)
                {
                    m_usedList.Remove(n);
                    m_freeList.AddLast(n);
                    n.Value.onClear();
                }
                n = temp;
            }
        }
    }
}
