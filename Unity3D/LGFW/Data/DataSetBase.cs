﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// An interface for data in scriptableObject, K is the type of the key of the data
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
        public string m_id;


        /// <inheritdoc/>
        public string getID()
        {
            return m_id;
        }
    }

    /// <summary>
    /// A dataset that can map the data with its id
    /// </summary>
    public abstract class DataSetDictionary : ScriptableObject
    {
        /// <summary>
        /// Initialize the dataset
        /// </summary>
        public abstract void initData();
    }

    /// <summary>
    /// A ScriptableObject stores all the data implement IData<K>, T is the type of the data
    /// </summary>
    public class DataSetBase<K, T> : DataSetDictionary where T : IData<K>
    {
        public List<T> m_dataList;
        protected Dictionary<K, T> m_dict;

        /// <summary>
        /// Initialize all data, creates a map to store data
        /// </summary>
        public override void initData()
        {
            m_dict = new Dictionary<K, T>();
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                m_dict.Add(m_dataList[i].getID(), m_dataList[i]);
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
            if (index < 0 || index >= m_dataList.Count)
            {
                return default(T);
            }
            return m_dataList[index];
        }

        /// <summary>
        /// Gets the number of data list
        /// </summary>
        /// <value>The number of data list</value>
        public int DataCount
        {
            get { return m_dataList.Count; }
        }

        /// <summary>
        /// Checks the data after processed
        /// </summary>
        public virtual void checkData()
        {
            HashSet<K> set = new HashSet<K>();
            for (int i = 0; i < m_dataList.Count; ++i)
            {
                K id = m_dataList[i].getID();
                if (set.Contains(id))
                {
                    Debug.LogError("duplicate id " + id + " in " + this.GetType().ToString());
                }
                else
                {
                    set.Add(id);
                }
            }
        }
    }
}
