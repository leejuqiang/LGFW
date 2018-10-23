using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Time scales Manager
    /// </summary>
    public class TimeScales : MonoBehaviour
    {

        private static TimeScales m_instance;

        /// <summary>
        /// Gets the singleton of TimeScales
        /// </summary>
        /// <value>The singleton</value>
        public static TimeScales Instance
        {
            get { return m_instance; }
        }

        private float[] m_scales;

        /// <summary>
        /// Sets the time scale by id
        /// </summary>
        /// <param name="id">The id of the time scale</param>
        /// <param name="value">The time scale value</param>
        public void setTimeScale(TimeScaleID id, float value)
        {
            if (id == TimeScaleID.unity)
            {
                Time.timeScale = value;
            }
            else if (id == TimeScaleID.real)
            {
                return;
            }
            else
            {
                m_scales[(int)id] = value;
            }
        }

        /// <summary>
        /// Gets a time scale by id
        /// </summary>
        /// <returns>The time scale</returns>
        /// <param name="id">The id</param>
        public float getTimeScale(TimeScaleID id)
        {
            if (id == TimeScaleID.unity)
            {
                return Time.timeScale;
            }
            if (id == TimeScaleID.real)
            {
                return 1;
            }
            return m_scales[(int)id];
        }

        /// <summary>
        /// Gets the delta time of a time scale
        /// </summary>
        /// <returns>The delta time</returns>
        /// <param name="id">The id of the time scale</param>
        public float getDeltaTime(TimeScaleID id)
        {
            if (id == TimeScaleID.unity)
            {
                return Time.deltaTime;
            }
            if (id == TimeScaleID.real)
            {
                return Time.unscaledDeltaTime;
            }
            return m_scales[(int)id] * Time.unscaledDeltaTime;
        }

        void Awake()
        {
            int len = System.Enum.GetValues(typeof(TimeScaleID)).Length;
            m_scales = new float[len - 2];
            m_instance = this;
        }

        void OnDestroy()
        {
            m_instance = null;
        }
    }
}
