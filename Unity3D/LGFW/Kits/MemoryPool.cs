using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for items used in memory pool
    /// </summary>
    public interface IMemoryPoolItem
    {
        /// <summary>
        /// This will be called when the item is put back into the pool
        /// </summary>
        void onClear();

        /// <summary>
        /// This will be called when the item is reused from the pool
        /// </summary>
        void onInit();

        /// <summary>
        /// Called when this item should be destory
        /// </summary>
        void onDestroy();
    }

    /// <summary>
    /// Base class for memory pool item
    /// </summary>
    public class MPItem : IMemoryPoolItem
    {
        public virtual void onInit()
        {

        }

        public virtual void onClear()
        {

        }

        public virtual void onDestroy()
        {

        }
    }

    /// <summary>
    /// Base class for a reference memory pool item
    /// </summary>
    public class ReferenceMPItem : MPItem
    {
        protected int m_reference;
        protected IMemoryPool m_pool;

        public ReferenceMPItem(IMemoryPool pool)
        {
            m_pool = pool;
        }

        public override void onInit()
        {
            m_reference = 0;
        }

        public void retain()
        {
            ++m_reference;
        }

        public void release()
        {
            --m_reference;
            if (m_reference <= 0)
            {
                m_pool.reclaimItem(this);
            }
        }
    }

    /// <summary>
    /// The interface for memory pool
    /// </summary>
    public interface IMemoryPool
    {
        bool reclaimItem(IMemoryPoolItem item);
    }

    /// <summary>
    /// A memory pool
    /// </summary>
    /// <typeparam name="T">The type of the items in this memory pool</typeparam>
    public class MemoryPool<T> : IMemoryPool where T : IMemoryPoolItem
    {

        protected HashSet<T> m_usedItems = new HashSet<T>();
        protected LinkedList<T> m_freeItems = new LinkedList<T>();
        protected List<T> m_tempList = new List<T>();

        protected int m_maxFreeCount;
        protected object m_data;

        /// <summary>
        /// Delegate for creating a new item
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The new item</returns>
        public delegate T CreateNewItem(object data);

        /// <summary>
        /// Delegate for checking if the item should be destroyed
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>True if the item should be destroyed</returns>
        public delegate bool CheckToDestroyItem(T item);

        /// <summary>
        /// Delegate for checking if the item should be freed
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>True if the item should be freed</returns>
        public delegate bool CheckToFreeItem(T item);

        protected CreateNewItem m_createFunc;
        protected CheckToDestroyItem m_checkDestroyFunc;
        protected CheckToFreeItem m_checkFreeFunc;

        /// <summary>
        /// The count of used items
        /// </summary>
        /// <value>The count</value>
        public int UsedCount
        {
            get { return m_usedItems.Count; }
        }

        /// <summary>
        /// The data of this memory pool. The data could be an id or something else
        /// </summary>
        /// <value>The data</value>
        public object Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        /// <summary>
        /// The max free items' number of this pool. If the free items' number excesses this number, then the extra items may be destroyed. 0 means unlimited
        /// </summary>
        /// <value>The number</value>
        public int MaxFreeCount
        {
            get { return m_maxFreeCount; }
            set { m_maxFreeCount = value; }
        }

        /// <summary>
        /// Delegate for creating new items
        /// </summary>
        /// <value>The delegate</value>
        public CreateNewItem CreateFunc
        {
            get { return m_createFunc; }
            set { m_createFunc = value; }
        }

        /// <summary>
        /// Delegate for checking if the item should be destroyed
        /// </summary>
        /// <value>The delegate</value>
        public CheckToDestroyItem CheckDestroyFunc
        {
            get { return m_checkDestroyFunc; }
            set { m_checkDestroyFunc = value; }
        }

        /// <summary>
        /// Delegate for checking if the item should be freed
        /// </summary>
        /// <value>The delegate</value>
        public CheckToFreeItem CheckFreeFunc
        {
            get { return m_checkFreeFunc; }
            set { m_checkFreeFunc = value; }
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="func">The delegate</param>
        /// <param name="maxFreeNumber">The max free items' number</param>
        /// <param name="data">The data</param>
        public MemoryPool(CreateNewItem func, object data, int maxFreeNumber)
        {
            m_createFunc = func;
            m_maxFreeCount = maxFreeNumber;
            m_data = data;
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="func">The delegate</param>
        public MemoryPool(CreateNewItem func)
        {
            m_createFunc = func;
            m_data = null;
            m_maxFreeCount = 0;
        }

        /// <summary>
        /// Gets an item from the pool
        /// </summary>
        /// <returns>The item</returns>
        public T getAnItem()
        {
            T ret = default(T);
            if (m_freeItems.Count <= 0)
            {
                if (m_createFunc != null)
                {
                    ret = m_createFunc(m_data);
                }
            }
            else
            {
                ret = m_freeItems.First.Value;
                m_freeItems.RemoveFirst();
            }
            if (ret != null)
            {
                ret.onInit();
                m_usedItems.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// Reclaims the item
        /// </summary>
        /// <returns>False if the item has already been put back, otherwise true</returns>
        /// <param name="item">The item</param>
        public bool reclaimItem(IMemoryPoolItem item)
        {
            T it = (T)item;
            if (m_usedItems.Remove(it))
            {
                item.onClear();
                m_freeItems.AddLast(it);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reclaims all items
        /// </summary>
        public void reclaimAll()
        {
            foreach (T item in m_usedItems)
            {
                item.onClear();
                m_freeItems.AddLast(item);
            }
            m_usedItems.Clear();
        }

        /// <summary>
        /// Destroys all items in the pool
        /// </summary>
        public void clear()
        {
            foreach (T item in m_usedItems)
            {
                item.onClear();
                item.onDestroy();
            }
            m_usedItems.Clear();
            LinkedListNode<T> n = m_freeItems.First;
            while (n != null)
            {
                n.Value.onDestroy();
                n = n.Next;
            }
            m_freeItems.Clear();
        }

        /// <summary>
        /// Updates the items
        /// </summary>
        public void update()
        {
            if (m_checkFreeFunc != null)
            {
                m_tempList.Clear();
                foreach (T item in m_usedItems)
                {
                    if (m_checkFreeFunc(item))
                    {
                        m_tempList.Add(item);
                    }
                }
                for (int i = 0; i < m_tempList.Count; ++i)
                {
                    reclaimItem(m_tempList[i]);
                }
            }
            if (m_maxFreeCount > 0 && m_checkDestroyFunc != null && m_freeItems.Count > m_maxFreeCount)
            {
                LinkedListNode<T> n = m_freeItems.First;
                while (n != null)
                {
                    LinkedListNode<T> temp = n.Next;
                    if (m_checkDestroyFunc(n.Value))
                    {
                        n.Value.onDestroy();
                        m_freeItems.Remove(n);
                        if (m_freeItems.Count <= m_maxFreeCount)
                        {
                            break;
                        }
                    }
                    n = temp;
                }
            }
        }
    }

    /// <summary>
    /// A memory pool. Items in this pool can't be reclaimed separately, they can only be flushed.
    /// </summary>
    /// <typeparam name="T">The type of item</typeparam>
    public class FlushMemoryPool<T> where T : IMemoryPoolItem
    {
        private List<T> m_usedList;
        private List<T> m_freeList;
        private object m_data;

        /// <summary>
        /// Delegate for creating an new item
        /// </summary>
        /// <param name="data">The data</param>
        /// <returns>The item</returns>
        public delegate T CreateNewItem(object data);
        private CreateNewItem m_createFunc;

        /// <summary>
        /// The delegate for creating new items
        /// </summary>
        /// <value>The delegate</value>
        public CreateNewItem CreateFunc
        {
            get { return m_createFunc; }
            set { m_createFunc = value; }
        }

        /// <summary>
        /// The data of this memory pool. The data could be an id or something else
        /// </summary>
        /// <value>The data</value>
        public object Data
        {
            get { return m_data; }
            set { m_data = value; }
        }

        /// <summary>
        /// Gets the count of used list
        /// </summary>
        /// <value>The count</value>
        public int UsedCount
        {
            get { return m_usedList.Count; }
        }

        /// <summary>
        /// Gets an used item by its index
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The item</returns>
        public T getItemByIndex(int index)
        {
            return m_usedList[index];
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="func">The delegate for creating new items</param>
        /// <param name="data">The data</param>
        public FlushMemoryPool(CreateNewItem func, object data)
        {
            m_usedList = new List<T>();
            m_freeList = new List<T>();
            m_createFunc = func;
            m_data = data;
        }

        /// <summary>
        /// Gets an item
        /// </summary>
        /// <returns>The item</returns>
        public T getAnItem()
        {
            T ret = default(T);
            if (m_freeList.Count > 0)
            {
                int last = m_freeList.Count - 1;
                ret = m_freeList[last];
                m_freeList.RemoveAt(last);
            }
            else
            {
                if (m_createFunc != null)
                {
                    ret = m_createFunc(m_data);
                }
            }
            if (ret != null)
            {
                m_usedList.Add(ret);
                ret.onInit();
            }
            return ret;
        }

        /// <summary>
        /// Reclaims all used items
        /// </summary>
        public void flush()
        {
            for (int i = 0; i < m_usedList.Count; ++i)
            {
                m_usedList[i].onClear();
            }
            m_freeList.AddRange(m_usedList);
            m_usedList.Clear();
        }

        /// <summary>
        /// Clear the pool, destroys all items
        /// </summary>
        public void clear()
        {
            for (int i = 0; i < m_usedList.Count; ++i)
            {
                m_usedList[i].onClear();
                m_usedList[i].onDestroy();
            }
            m_usedList.Clear();
            for (int i = 0; i < m_freeList.Count; ++i)
            {
                m_freeList[i].onDestroy();
            }
            m_freeList.Clear();
        }
    }
}
