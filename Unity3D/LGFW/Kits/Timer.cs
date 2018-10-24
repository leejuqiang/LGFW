using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A timer
    /// </summary>
    [System.Serializable]
    public class Timer
    {

        [SerializeField]
        protected float m_time;
        [SerializeField]
        protected TimeScaleID m_timeScale = TimeScaleID.unity;
        protected float m_t;
        protected bool m_pause = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.Timer"/> class.
        /// </summary>
        public Timer() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.Timer"/> class.
        /// </summary>
        /// <param name="time">The time of this timer</param>
        /// <param name="timeScale">The time scale this timer using</param>
        public Timer(float time, TimeScaleID timeScale)
        {
            m_time = time;
            m_timeScale = timeScale;
        }

        /// <summary>
        /// Gets the factor of this timer, the factor is the the current time / the time
        /// </summary>
        /// <value>The factor</value>
        public virtual float Factor
        {
            get
            {
                if (m_time <= 0)
                {
                    return 0;
                }
                return Mathf.Clamp01(m_t / m_time);
            }
        }

        /// <summary>
        /// If the timer is paused
        /// </summary>
        /// <value>If paused</value>
        public bool Pause
        {
            get { return m_pause; }
            set
            {
                m_pause = value;
            }
        }

        /// <summary>
        /// The current time of this timer
        /// </summary>
        /// <value>The current time</value>
        public float Time
        {
            get { return m_time; }
            set
            {
                m_time = value;
                reset();
            }
        }

        /// <summary>
        /// Reset this timer
        /// </summary>
        public virtual void reset()
        {
            m_t = 0;
            m_pause = false;
        }

        /// <summary>
        /// Call update so the timer will be timing
        /// </summary>
        /// <returns>If the time has stopped, return true</returns>
        public bool update()
        {
            if (!m_pause)
            {
                if (m_t < m_time)
                {
                    m_t += TimeScales.Instance.getDeltaTime(m_timeScale);
                }
            }
            return checkTimeUp();
        }

        protected virtual bool checkTimeUp()
        {
            return m_t >= m_time;
        }
    }

    /// <summary>
    /// A loop timer, when it's time up, it will restart
    /// The update of this timer will return true if the time < 0 or reaches the predetermined time
    /// </summary>
    [System.Serializable]
    public class LoopTimer : Timer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.LoopTimer"/> class.
        /// </summary>
        public LoopTimer() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.LoopTimer"/> class.
        /// </summary>
        /// <param name="time">The time</param>
        /// <param name="timeScale">The time scale</param>
        public LoopTimer(float time, TimeScaleID timeScale) : base(time, timeScale) { }

        protected override bool checkTimeUp()
        {
            if (m_time > 0)
            {
                if (m_t >= m_time)
                {
                    m_t -= m_time;
                    return true;
                }
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// A ping pong timer, the time will count up and down
    /// The update will return true when the time < 0 or reaches the predetermined time
    /// </summary>
    [System.Serializable]
    public class PingPongTimer : Timer
    {
        protected bool m_isReverse;

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.PingPongTimer"/> class.
        /// </summary>
        public PingPongTimer() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.PingPongTimer"/> class.
        /// </summary>
        /// <param name="time">The time</param>
        /// <param name="timeScale">The time scale</param>
        public PingPongTimer(float time, TimeScaleID timeScale) : base(time, timeScale) { }

        /// <inheritdoc/>
        public override float Factor
        {
            get
            {
                if (m_isReverse)
                {
                    return 1 - base.Factor;
                }
                return base.Factor;
            }
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
            m_isReverse = false;
        }

        protected override bool checkTimeUp()
        {
            if (m_time > 0)
            {
                if (m_t >= m_time)
                {
                    m_t -= m_time;
                    m_isReverse = !m_isReverse;
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}
