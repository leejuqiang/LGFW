using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A memory pool can't reclaim items, only clear all
    /// </summary>
    public class FlushMemoryPool
    {

        private List<MemoryPoolItem> m_list = new List<MemoryPoolItem>();
        private int m_count;

        /// <summary>
        /// Gets the count of items
        /// </summary>
        /// <value>The count</value>
        public int Count
        {
            get { return m_count; }
        }

        /// <summary>
        /// Gets an item by its index
        /// </summary>
        /// <returns>The item of the index</returns>
        /// <param name="index">The index</param>
        /// <typeparam name="T">The type of the item</typeparam>
        public T getItemByIndex<T>(int index) where T : MemoryPoolItem
        {
            return (T)m_list[index];
        }

        /// <summary>
        /// Gets an item, if the pool dose not have an unused item, a new item will be created
        /// </summary>
        /// <returns>The item</returns>
        /// <typeparam name="T">The type of the item</typeparam>
        public T getAnItem<T>() where T : MemoryPoolItem, new()
        {
            MemoryPoolItem ret = null;
            if (m_count >= m_list.Count)
            {
                ret = new T();
                m_list.Add(ret);
            }
            else
            {
                ret = m_list[m_count];
            }
            ++m_count;
            ret.onInit();
            return (T)ret;
        }

        /// <summary>
        /// Clear the pool
        /// </summary>
        /// <param name="clear">If true, the items will be destroy, otherwise, only the pool's count will be set to 0</param>
        public void flush(bool clear)
        {
            if (clear)
            {
                for (int i = 0; i < m_count; ++i)
                {
                    m_list[i].onClear();
                }
            }
            m_count = 0;
        }
    }
}
