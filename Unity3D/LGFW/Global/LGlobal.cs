using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Global Manager
    /// </summary>
    public class LGlobal : MonoBehaviour
    {

        /// <summary>
        /// The height of the screen, this is not the pixel in screen, this is a relative height in unity editor
        /// </summary>
        public float m_screenHeight = 1000;

        private static LGlobal m_instance;

        /// <summary>
        /// Gets the singleton of LGlobal
        /// </summary>
        /// <value>The singleton</value>
        public static LGlobal Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// This is used for check text warp, default is for English, if wants to support other language, add other char in the function
        /// </summary>
        /// <returns>Return false if the char should consider as a warp char, otherwise true</returns>
        /// <param name="ch">The char</param>
        public static bool isCharacterWrapByWord(int ch)
        {
            return (ch >= (int)'A' && ch <= (int)'Z') || (ch >= (int)'a' && ch <= (int)'z');
        }

        private float m_serverTime = -1;
        private System.DateTime m_serverDate;
        private System.DateTime m_standardDate;

        /// <summary>
        /// Gets the date from server, if not set server time, return the local time
        /// </summary>
        /// <value>The date</value>
        public System.DateTime ServerDate
        {
            get
            {
                if (m_serverTime < 0)
                {
                    return System.DateTime.UtcNow;
                }
                System.DateTime dt = m_serverDate.AddSeconds(Time.realtimeSinceStartup - m_serverTime);
                return dt;
            }
        }

        /// <summary>
        /// Sets the server date
        /// </summary>
        /// <param name="timeStamp">The time stamp</param>
        public void setServerDate(long timeStamp)
        {
            System.DateTime dt = m_standardDate.AddMilliseconds(timeStamp);
            setServerDate(dt);
        }

        /// <summary>
        /// Sets the server date
        /// </summary>
        /// <param name="date">The date</param>
        public void setServerDate(System.DateTime date)
        {
            m_serverDate = date;
            m_serverTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Gets the time stamp of Server time
        /// </summary>
        /// <value>The time stamp</value>
        public long TimeStamp
        {
            get
            {
                return (long)(ServerDate - m_standardDate).TotalMilliseconds;
            }
        }

        void Awake()
        {
            m_instance = this;
            m_standardDate = new System.DateTime(1970, 1, 1);
        }

        void OnDestroy()
        {
            m_instance = null;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/create global manager", false, (int)'z')]
        public static void createGlobal()
        {
            GameObject go = new GameObject("Global");
            go.AddComponent<LGlobal>();
            go.AddComponent<TimeScales>();
            go.AddComponent<Localization>();
            go.AddComponent<UIPanelManager>();
            go.AddComponent<EventManager>();
            go.AddComponent<ResourceManager>();
            go.AddComponent<NotDestory>();
        }
#endif
    }
}
