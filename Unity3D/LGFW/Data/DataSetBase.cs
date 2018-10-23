using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An interface for data in scriptableObjcet, K is the type of the key of the data
    /// </summary>
    public interface IData<K>
    {
        /// <summary>
        /// Gets the key
        /// </summary>
        /// <returns>The key</returns>
        K getID();
    }

    /// <summary>
    /// A data with a string type key
    /// </summary>
    public class StringIdData : IData<string>
    {
        /// <summary>
        /// The key of the data
        /// </summary>
        [DataCombineText]
        public string m_id;

        /// <inheritdoc/>
        public string getID()
        {
            return m_id;
        }
    }

    /// <summary>
    /// A ScriptableObject stores all the data implement IData<K>, T is the type of the data
    /// </summary>
    public class DataSetBase<T, K> : ScriptableObject where T : IData<K>
    {

        [SerializeField]
        protected List<T> m_datas;
        protected Dictionary<K, T> m_dict;

        /// <summary>
        /// Initialize all datas
        /// </summary>
        public virtual void initDatas()
        {
            m_dict = new Dictionary<K, T>();
            for (int i = 0; i < m_datas.Count; ++i)
            {
                m_dict.Add(m_datas[i].getID(), m_datas[i]);
            }
        }

        /// <summary>
        /// Gets the data by its key
        /// </summary>
        /// <returns>The data</returns>
        /// <param name="id">The key</param>
        public T getDataById(K id)
        {
            T ret = default(T);
            m_dict.TryGetValue(id, out ret);
            return ret;
        }

        /// <summary>
        /// Gets the data by index
        /// </summary>
        /// <returns>The data</returns>
        /// <param name="index">The index</param>
        public T getDataByIndex(int index)
        {
            if (index < 0 || index >= m_datas.Count)
            {
                return default(T);
            }
            return m_datas[index];
        }

        /// <summary>
        /// Gets the number of datas
        /// </summary>
        /// <value>The number of datas</value>
        public int DataCount
        {
            get { return m_datas.Count; }
        }
    }
}
