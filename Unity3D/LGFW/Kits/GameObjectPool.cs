using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A memory pool for GameObject
    /// </summary>
    public class GameObjectPool<T> where T : Component
    {

        private GameObject m_prefab;

        private LinkedList<T> m_usedList = new LinkedList<T>();
        private LinkedList<T> m_freeList = new LinkedList<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.GameObjectPool`1"/> class.
        /// </summary>
        /// <param name="prefab">The GameObject(Prefab) used to create new GameObject</param>
        public GameObjectPool(GameObject prefab)
        {
            m_prefab = prefab;
        }

        /// <summary>
        /// Gets a GameObject
        /// </summary>
        /// <returns>The component of the generic type on this GameObject</returns>
        /// <param name="isNew">If the GameObject is created this time, this value will be set true</param>
        public T getAnObject(out bool isNew)
        {
            if (m_freeList.Count > 0)
            {
                isNew = false;
                LinkedListNode<T> n = m_freeList.First;
                m_freeList.RemoveFirst();
                m_usedList.AddLast(n);
                n.Value.gameObject.SetActive(true);
                return n.Value;
            }
            isNew = true;
            GameObject go = GameObject.Instantiate<GameObject>(m_prefab);
            T t = go.GetComponent<T>();
            m_usedList.AddLast(t);
            go.SetActive(true);
            return t;
        }

        /// <summary>
        /// Gets a GameObject
        /// </summary>
        /// <returns>The component of the generic type on this GameObject</returns>
        public T getAnObject()
        {
            bool isNew = false;
            return getAnObject(out isNew);
        }

        /// <summary>
        /// Reclaims the GameObject which the component attaches to
        /// </summary>
        /// <param name="t">The component</param>
        public void freeGameObject(T t)
        {
            t.gameObject.SetActive(false);
            m_usedList.Remove(t);
            m_freeList.AddLast(t);
        }
    }
}
