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
        /// The first data of all datas
        /// </summary>
        public object m_data;
        /// <summary>
        /// All datas
        /// </summary>
        public object[] m_datas;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.MessageData"/> class.
        /// </summary>
        /// <param name="datas">Datas</param>
        public MessageData(object[] datas)
        {
            m_datas = datas;
            if (datas != null && datas.Length > 0)
            {
                m_data = m_datas[0];
            }
        }
    }
}
