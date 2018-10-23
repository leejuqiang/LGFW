using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for items used in memory pool
    /// </summary>
    public abstract class MemoryPoolItem
    {
        /// <summary>
        /// This will be called when the item is put back into the pool
        /// </summary>
        public virtual void onClear()
        {
            //todo
        }

        /// <summary>
        /// This will be called when the item is reused from the pool
        /// </summary>
        public virtual void onInit()
        {
            //todo
        }
    }

    /// <summary>
    /// A memory pool
    /// </summary>
    public class MemoryPool
    {

        protected HashSet<MemoryPoolItem> m_usedItems = new HashSet<MemoryPoolItem>();
        protected LinkedList<MemoryPoolItem> m_freeItems = new LinkedList<MemoryPoolItem>();

        /// <summary>
        /// Gets an item from the pool
        /// </summary>
        /// <returns>The item</returns>
        /// <typeparam name="T">The type of the item</typeparam>
        public T getAnItem<T>() where T : MemoryPoolItem, new()
        {
            MemoryPoolItem ret = null;
            if (m_freeItems.Count <= 0)
            {
                ret = new T();
            }
            else
            {
                ret = m_freeItems.First.Value;
                m_freeItems.RemoveFirst();
            }
            ret.onInit();
            m_usedItems.Add(ret);
            return (T)ret;
        }

        /// <summary>
        /// Reclaims the item
        /// </summary>
        /// <returns>False if the item has already been put back, otherwise true</returns>
        /// <param name="item">The item</param>
        public bool reclaimItem(MemoryPoolItem item)
        {
            if (m_usedItems.Remove(item))
            {
                item.onClear();
                m_freeItems.AddLast(item);
                return true;
            }
            return false;
        }
    }
}
