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

    public enum UITweenFinish
    {
        none,
        deactiveWhenForward,
        deactiveWhenBackward,
        deactive,
    }

    public enum UITweenState
    {
        forwardDelay,
        backwardDelay,
        playForward,
        playBackward,
    }

    /// <summary>
    /// The base class for tween.
    /// Sub calss should implement updateTween(float f), f is the sample factor
    /// </summary>
    public abstract class UITween : BaseMono
    {

        /// <summary>
        /// The duration of the tween
        /// </summary>
        public float m_duration = 1;
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
        /// The deactive mode when finishing
        /// </summary>
        public UITweenFinish m_deactiveWhenFinish;
        /// <summary>
        /// The animation curve
        /// </summary>
        public AnimationCurve m_curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        /// <summary>
        /// If use the curve to compute the sample factor. If false, uses the linear interpolating
        /// </summary>
        public bool m_useCurve = true;
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
        protected UITweenState m_state;
        protected bool m_isPlaying;

        /// <summary>
        /// If the tween is playing
        /// </summary>
        /// <value></value>
        public bool IsPlaying
        {
            get { return m_isPlaying; }
        }

        /// <summary>
        /// The sample factor for interpolation
        /// </summary>
        /// <value></value>
        public float SampleFactor
        {
            get { return m_factor; }
            set
            {
                value = Mathf.Clamp01(value);
                if (m_factor != value)
                {
                    m_factor = value;
                    m_isPlaying = false;
                    m_state = m_isForward ? UITweenState.playForward : UITweenState.playBackward;
                    m_t = m_duration * m_factor;
                    Awake();
                    updateTween(computeFactor());
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
                m_state = m_isForward ? UITweenState.forwardDelay : UITweenState.backwardDelay;
            }
        }

        /// <summary>
        /// Stops the tween
        /// </summary>
        public void stop()
        {
            m_isPlaying = false;
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

        protected float computeFactor()
        {
            if (m_useCurve)
            {
                return m_curve.Evaluate(m_factor);
            }
            return m_factor;
        }

        /// <summary>
        /// Resets the tween
        /// </summary>
        /// <param name="isForward">If forward</param>
        public virtual void reset(bool isForward)
        {
            Awake();
            m_isForward = isForward;
            m_isPlaying = false;
            m_t = 0;
            if (m_isForward)
            {
                m_factor = 0;
                m_state = UITweenState.forwardDelay;
            }
            else
            {
                m_factor = 1;
                m_state = UITweenState.backwardDelay;
            }
            updateTween(computeFactor());
        }

        public virtual void LateUpdate()
        {
            if (TimeScales.Instance == null)
            {
                manualUpdate(Time.deltaTime);
            }
            else
            {
                manualUpdate(TimeScales.Instance.getDeltaTime(m_timeScale));
            }
        }

        /// <summary>
        /// Updates manually
        /// </summary>
        /// <param name="dt"></param>
        public void manualUpdate(float dt)
        {
            if (m_isPlaying)
            {
                Awake();
                updateDelta(dt);
            }
        }

        protected abstract void updateTween(float f);

        protected virtual void onFinish()
        {
            if (m_deactiveWhenFinish == UITweenFinish.deactive)
            {
                this.gameObject.SetActive(false);
            }
            else if (m_deactiveWhenFinish == UITweenFinish.deactiveWhenForward && m_isForward)
            {
                this.gameObject.SetActive(false);
            }
            else if (m_deactiveWhenFinish == UITweenFinish.deactiveWhenBackward && !m_isForward)
            {
                this.gameObject.SetActive(false);
            }
            if (m_onTweenOver != null)
            {
                m_onTweenOver(this);
            }
        }

        protected bool handleEndCase()
        {
            if (m_mode == UITweenMode.once)
            {
                m_factor = m_state == UITweenState.playForward ? 1 : 0;
                return true;
            }
            if (m_mode == UITweenMode.loop)
            {
                if (m_state == UITweenState.playForward)
                {
                    m_t -= m_duration;
                }
                else
                {
                    m_t += m_duration;
                }
                m_factor = m_t / m_duration;
                return false;
            }
            if (m_state == UITweenState.playForward)
            {
                m_t -= m_duration;
                m_t = m_duration - m_t;
                m_state = UITweenState.playBackward;
            }
            else
            {
                m_t = -m_t;
                m_state = UITweenState.playForward;
            }
            m_factor = m_t / m_duration;
            return false;
        }

        protected void updateDelta(float dt)
        {
            if (m_state != UITweenState.playBackward)
            {
                m_t += dt;
            }
            else
            {
                m_t -= dt;
            }
            if (m_state == UITweenState.forwardDelay)
            {
                if (m_t > m_forwardDelay)
                {
                    m_t -= m_forwardDelay;
                    m_state = UITweenState.playForward;
                }
            }
            else if (m_state == UITweenState.backwardDelay)
            {
                if (m_t > m_backwardDelay)
                {
                    m_t -= m_backwardDelay;
                    m_t = m_duration - m_t;
                    m_state = UITweenState.playBackward;
                }
            }
            bool end = false;
            if (m_state == UITweenState.playForward)
            {
                if (m_t >= m_duration)
                {
                    end = handleEndCase();
                }
                else
                {
                    m_factor = m_t / m_duration;
                }
            }
            else if (m_state == UITweenState.playBackward)
            {
                if (m_t <= 0)
                {
                    end = handleEndCase();
                }
                else
                {
                    m_factor = m_t / m_duration;
                }
            }
            updateTween(computeFactor());
            if (end)
            {
                m_isPlaying = false;
                onFinish();
            }
        }
    }
}
