using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// represents a event sent to the EventTrigger
    /// </summary>
    public class LEvent
    {

        /// <summary>
        /// The id of the event
        /// </summary>
        public string m_id;
        /// <summary>
        /// The data of this event
        /// </summary>
        public object[] m_dataList;
        /// <summary>
        /// If true, the event will only sent to the first EventTrigger that responses to it
        /// </summary>
        public bool m_oneTimeEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.LEvent"/> class
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <param name="oneTime">If the event is a one time event</param>
        public LEvent(string id, bool oneTime)
        {
            m_id = id;
            m_oneTimeEvent = oneTime;
        }
    }
}
