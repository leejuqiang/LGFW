using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A data set used as a single parameter
    /// </summary>
    public class MessageData
    {

        /// <summary>
        /// The first data of all data
        /// </summary>
        public object m_data;
        /// <summary>
        /// All data
        /// </summary>
        public object[] m_dataList;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.MessageData"/> class.
        /// </summary>
        /// <param name="dataList">Data</param>
        public MessageData(object[] dataList)
        {
            m_dataList = dataList;
            if (dataList != null && dataList.Length > 0)
            {
                m_data = m_dataList[0];
            }
        }
    }
}
