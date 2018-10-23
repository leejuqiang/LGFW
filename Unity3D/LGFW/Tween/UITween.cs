using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum UITweenMode
    {
        once,
        pingpong,
        loop,
    }

    public enum UITweenReset
    {
        none,
        toCurrent,
        fromCurrent,
    }

    public enum UITweenFinish
    {
        none,
        deactiveWhenForward,
        deactiveWhneBackward,
    }

    /// <summary>
    /// The base class for tween
    /// </summary>
    public abstract class UITween : MonoBehaviour
    {

        /// <summary>
        /// The duration of the tween
        /// </summary>
        public float m_duration;
        /// <summary>
        /// The delay for forward playing
        /// </summary>
        public float m_forwardDelay;
        /// <summary>
        /// The delay for backward playing
        /// </summary>
        public float m_backwardDelay;
        /// <summary>
        /// The playing mode
        /// </summary>
        public UITweenMode m_mode = UITweenMode.once;
        /// <summary>
        /// The time scale type
        /// </summary>
        public TimeScaleID m_timeScale = TimeScaleID.unity;
        /// <summary>
        /// The animation curve
        /// </summary>
        public AnimationCurve m_curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        /// <summary>
        /// The deactive mode when finishing
        /// </summary>
        public UITweenFinish m_deactiveWhenFinish;
        /// <summary>
        /// Reset mode of the tween
        /// </summary>
        public UITweenReset m_resetMode;
        /// <summary>
        /// If played forward
        /// </summary>
        public bool m_isForward = true;
        /// <summary>
        /// If starts playing when enabled
        /// </summary>
        public bool m_playWhenEnable;

        /// <summary>
        /// Delegate called when tween finished
        /// </summary>
        public System.Action<UITween> m_onTweenOver;

        protected float m_factor;
        protected float m_t;
        protected bool m_isInDelay;
        protected bool m_isPlaying;
        protected float m_lastFactor;
        protected bool m_hasAwake;

        /// <summary>
        /// If the tween is playing
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get { return m_isPlaying; }
        }

        /// <summary>
        /// The factor for interpolation
        /// </summary>
        /// <value></value>
        public float Factor
        {
            get { return m_factor; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_factor != value)
                {
                    m_factor = value;
                    m_isInDelay = false;
                    m_lastFactor = m_factor;
                    m_t = m_duration * m_factor;
                    applyFactor(m_curve.Evaluate(m_factor));
                }
            }
        }

        public virtual void OnEnable()
        {
            if (m_playWhenEnable)
            {
                reset(m_isForward);
                play(m_isForward);
            }
        }

        /// <summary>
        /// Plays the tween
        /// </summary>
        public void play()
        {
            play(m_isForward);
        }

        /// <summary>
        /// Plays the tween
        /// </summary>
        /// <param name="isForward">If playing forward</param>
        public virtual void play(bool isForward)
        {
            m_isForward = isForward;
            if (m_duration > 0)
            {
                m_isPlaying = true;
            }
        }

        /// <summary>
        /// Stops the tween
        /// </summary>
        public void stop()
        {
            m_isPlaying = false;
        }

        public void forceAwake()
        {
            m_hasAwake = false;
            Awake();
        }

        public void Awake()
        {
            if (!m_hasAwake)
            {
                doAwake();
                m_hasAwake = true;
            }
        }

        protected virtual void doAwake()
        {
            //todo
        }

        protected virtual void OnDestroy()
        {
            m_onTweenOver = null;
        }

        /// <summary>
        /// Resets and plays the tween
        /// </summary>
        /// <param name="isForward">If forward</param>
        public void resetAndPlay(bool isForward)
        {
            reset(isForward);
            play(isForward);
        }

        /// <summary>
        /// Resets and plays the tween
        /// </summary>
        public void resetAndPlay()
        {
            resetAndPlay(m_isForward);
        }

        /// <summary>
        /// Resets the tween
        /// </summary>
        public void reset()
        {
            reset(m_isForward);
        }

        /// <summary>
        /// Resets the tween
        /// </summary>
        /// <param name="isForward">If forward</param>
        public virtual void reset(bool isForward)
        {
            m_isForward = isForward;
            m_isPlaying = false;
            if (m_isForward)
            {
                m_factor = 0;
                m_lastFactor = 0;
                m_isInDelay = m_forwardDelay > 0;
                m_t = 0;
            }
            else
            {
                m_factor = 1;
                m_lastFactor = 1;
                m_isInDelay = m_backwardDelay > 0;
                m_t = m_isInDelay ? m_backwardDelay : m_duration;
            }
            if (m_resetMode == UITweenReset.fromCurrent)
            {
                resetFromCurrentValue();
            }
            else if (m_resetMode == UITweenReset.toCurrent)
            {
                resetToCurrentValue();
            }
            else
            {
                resetValue();
            }
            applyFactor(m_curve.Evaluate(m_factor));
        }

        protected virtual void resetValue()
        {
            //todo
        }

        protected virtual void resetFromCurrentValue()
        {
            //todo
        }

        protected virtual void resetToCurrentValue()
        {
            //todo
        }

        public virtual void LateUpdate()
        {
            manualUpdate(TimeScales.Instance.getDeltaTime(m_timeScale));
        }

        /// <summary>
        /// Updates manually
        /// </summary>
        /// <param name="dt"></param>
        public void manualUpdate(float dt)
        {
            if (m_isPlaying)
            {
                updateDelta(dt);
            }
        }

        protected abstract void applyFactor(float f);

        protected virtual void onFinish()
        {
            if (m_deactiveWhenFinish == UITweenFinish.deactiveWhenForward && m_isForward)
            {
                this.gameObject.SetActive(false);
            }
            else if (m_deactiveWhenFinish == UITweenFinish.deactiveWhneBackward && !m_isForward)
            {
                this.gameObject.SetActive(false);
            }
            if (m_onTweenOver != null)
            {
                m_onTweenOver(this);
            }
        }

        protected void updateDelta(float dt)
        {
            bool end = false;
            if (m_isForward)
            {
                m_t += dt;
                if (m_isInDelay)
                {
                    if (m_t >= m_forwardDelay)
                    {
                        m_t -= m_forwardDelay;
                        m_isInDelay = false;
                        m_factor = m_t / m_duration;
                    }
                }
                else
                {
                    if (m_t >= m_duration)
                    {
                        end = true;
                    }
                }
            }
            else
            {
                m_t -= dt;
                if (m_isInDelay)
                {
                    if (m_t <= 0)
                    {
                        m_t += m_duration;
                        m_isInDelay = false;
                        m_factor = m_t / m_duration;
                    }
                }
                else
                {
                    if (m_t <= 0)
                    {
                        end = true;
                    }
                }
            }
            if (end)
            {
                if (m_mode == UITweenMode.pingpong)
                {
                    m_isForward = !m_isForward;
                    if (m_isForward)
                    {
                        m_t = -m_t;
                    }
                    else
                    {
                        m_t = m_duration + m_duration - m_t;
                    }
                }
                else if (m_mode == UITweenMode.loop)
                {
                    if (m_isForward)
                    {
                        m_t -= m_duration;
                    }
                    else
                    {
                        m_t += m_duration;
                    }
                }
                else
                {
                    m_isPlaying = false;
                }
            }
            if (!m_isInDelay)
            {
                m_factor = Mathf.Clamp01(m_t / m_duration);
            }
            if (m_factor != m_lastFactor)
            {
                applyFactor(m_curve.Evaluate(m_factor));
                m_lastFactor = m_factor;
            }
            if (end && !m_isPlaying)
            {
                onFinish();
            }
        }
    }
}
