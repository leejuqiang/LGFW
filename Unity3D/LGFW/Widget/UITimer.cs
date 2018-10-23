using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A timer
    /// </summary>
    public class UITimer : UIWidget
    {

        public const int F_YEAR = 1;
        public const int F_MONTH = 1 << 1;
        public const int F_DAY = 1 << 2;
        public const int F_HOUR = 1 << 3;
        public const int F_MINUTER = 1 << 4;
        public const int F_SECOND = 1 << 5;

        /// <summary>
        /// The text for this timer
        /// </summary>
        public UIText m_text;
        /// <summary>
        /// Time format for the timer
        /// </summary>
        [HideInInspector]
        public int m_timeFormat;
        [HideInInspector]
        /// <summary>
        /// The text id for all time units
        /// </summary>
        public string[] m_timeFormatTextIds;
        /// <summary>
        /// The max number of time units
        /// </summary>
        public int m_maxDisplayFormatNumber = 6;
        /// <summary>
        /// The time scale it uses
        /// </summary>
        public TimeScaleID m_timeScale = TimeScaleID.real;
        /// <summary>
        /// If it's a count down timer
        /// </summary>
        public bool m_countDown = true;
        /// <summary>
        /// If true, the bigger units with a value of 0 will not be displayed
        /// </summary>
        public bool m_ignoreZeroAtFront = true;

        private int m_time;
        private LoopTimer m_timer;
        private bool m_isCounting;
        private System.Text.StringBuilder m_textBuilder = new System.Text.StringBuilder();

        /// <summary>
        /// Called when the count down to 0
        /// </summary>
        public System.Action<UITimer> m_onCountDownOver;

        /// <summary>
        /// Current time in seconds
        /// </summary>
        /// <value>The time</value>
        public int CurrentSeconds
        {
            get { return m_time; }
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_timer = new LoopTimer(1, m_timeScale);
            m_isCounting = false;
        }

        /// <summary>
        /// Starts the timer
        /// </summary>
        /// <param name="seconds">The initial time in seconds</param>
        public void start(int seconds)
        {
            m_time = seconds;
            m_timer.reset();
            m_isCounting = true;
            displayTime();
        }

        /// <summary>
        /// Stops the timer
        /// </summary>
        public void stop()
        {
            m_isCounting = false;
        }

        void Update()
        {
            if (m_isCounting)
            {
                if (m_timer.update())
                {
                    m_time += m_countDown ? -1 : 1;
                    displayTime();
                    if (m_countDown && m_time <= 0)
                    {
                        if (m_onCountDownOver != null)
                        {
                            m_onCountDownOver(this);
                        }
                        m_isCounting = false;
                    }
                }
            }
        }

        private string getFormatText(int index)
        {
            return Localization.getString(m_timeFormatTextIds[index]);
        }

        private int handleOneFormat(int seconds, int format, int secondsOfFormat, ref int textIndex, ref int formatLen)
        {
            if ((m_timeFormat & format) > 0)
            {
                int t = seconds / secondsOfFormat;
                seconds -= secondsOfFormat * t;
                if (!m_ignoreZeroAtFront || t != 0 || formatLen > 0)
                {
                    m_textBuilder.Append(t);
                    m_textBuilder.Append(getFormatText(textIndex));
                    ++formatLen;
                }
            }
            ++textIndex;
            return seconds;
        }

        private void buildText()
        {
            m_textBuilder.Remove(0, m_textBuilder.Length);
            int t = m_time;
            int len = 0;
            int index = 0;
            t = handleOneFormat(t, F_YEAR, 31104000, ref index, ref len);
            if (len >= m_maxDisplayFormatNumber)
            {
                return;
            }
            t = handleOneFormat(t, F_MONTH, 2592000, ref index, ref len);
            if (len >= m_maxDisplayFormatNumber)
            {
                return;
            }
            t = handleOneFormat(t, F_DAY, 86400, ref index, ref len);
            if (len >= m_maxDisplayFormatNumber)
            {
                return;
            }
            t = handleOneFormat(t, F_HOUR, 3600, ref index, ref len);
            if (len >= m_maxDisplayFormatNumber)
            {
                return;
            }
            t = handleOneFormat(t, F_MINUTER, 60, ref index, ref len);
            if (len >= m_maxDisplayFormatNumber)
            {
                return;
            }
            m_textBuilder.Append(t);
            m_textBuilder.Append(getFormatText(index));
        }

        private void displayTime()
        {
            buildText();
            if (m_text != null)
            {
                m_text.Text = m_textBuilder.ToString();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UITimer", false, (int)'t')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UITimer>(false);
        }
#endif
    }
}
