using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LGFW
{
    /// <summary>
    /// A GameObject controlled by a UIState
    /// </summary>
    [System.Serializable]
    public class UIStateGameObject
    {
        /// <summary>
        /// The GameObject
        /// </summary>
        public GameObject m_object;
        /// <summary>
        /// If the GameObject should be active
        /// </summary>
        public bool m_active;
    }

    /// <summary>
    /// An UILocalizedText controlled by a UIState
    /// </summary>
    [System.Serializable]
    public class UIStateText
    {
        /// <summary>
        /// The UILocalizedText
        /// </summary>
        public UILocalizedText m_text;
        /// <summary>
        /// The text id 
        /// </summary>
        public string m_textId;
    }

    /// <summary>
    /// An UITween controlled by a UIState
    /// </summary>
    [System.Serializable]
    public class UIStateTween
    {
        /// <summary>
        /// The UITween
        /// </summary>
        public UITween m_tween;
        /// <summary>
        /// If this tween is played forward
        /// </summary>
        public bool m_forward;
        /// <summary>
        /// If the UIState wait for this tween finished then call panel's onFinishEnteringFlow()
        /// </summary>
        public bool m_waitForFinishing;
    }

    /// <summary>
    /// A state of a UIPanel, you can think it as a state of a state machine
    /// </summary>
    public class UIState : BaseMono
    {
        /// <summary>
        /// The id of the state
        /// </summary>
        public string m_id;

        /// <summary>
        /// The GameObjects controlled by this state when entering this state
        /// </summary>
        public UIStateGameObject[] m_enterGameObjects;
        /// <summary>
        /// The GameObjects controlled by this state when exiting this state
        /// </summary>
        public UIStateGameObject[] m_exitGameObjects;
        /// <summary>
        /// The UILocalizedTexts controlled by this state
        /// </summary>
        public UIStateText[] m_texts;
        /// <summary>
        /// The UITweens controlled by this state when entering this state
        /// </summary>
        public UIStateTween[] m_enterTweens;
        /// <summary>
        /// The UITweens controlled by this state when exiting this state
        /// </summary>
        public UIStateTween[] m_exitTweens;

        protected UIPanel m_panel;
        protected UIStateTween[] m_waitingTweens;
        protected bool m_isEntering;

        /// <summary>
        /// If the state is waiting for tween playing
        /// </summary>
        /// <value>True if it's waiting</value>
        public bool IsWaiting
        {
            get { return m_waitingTweens != null; }
        }

        /// <summary>
        /// Called when a panel enters this state
        /// </summary>
        /// <param name="panel">The UIPanel</param>
        public void enterState(UIPanel panel)
        {
            Awake();
            m_panel = panel;
            for (int i = 0; i < m_enterGameObjects.Length; ++i)
            {
                m_enterGameObjects[i].m_object.SetActive(m_enterGameObjects[i].m_active);
            }
            for (int i = 0; i < m_texts.Length; ++i)
            {
                m_texts[i].m_text.m_textId = m_texts[i].m_textId;
                m_texts[i].m_text.applyText();
            }
            m_isEntering = true;
            m_waitingTweens = null;
            for (int i = 0; i < m_enterTweens.Length; ++i)
            {
                m_enterTweens[i].m_tween.m_isForward = m_enterTweens[i].m_forward;
                m_enterTweens[i].m_tween.resetAndPlay();
                if (m_enterTweens[i].m_waitForFinishing)
                {
                    m_waitingTweens = m_enterTweens;
                }
            }
            onBeginEntering();
            if (m_waitingTweens == null)
            {
                onFinishEntering();
                panel.onFinishEnteringState(this);
            }
        }

        /// <summary>
        /// Called when a panel in this state is refreshed
        /// </summary>
        public virtual void refresh()
        {
        }

        /// <summary>
        /// Called when a panel exits this state
        /// </summary>
        public void exitState(UIPanel panel)
        {
            Awake();
            m_panel = panel;

            for (int i = 0; i < m_exitGameObjects.Length; ++i)
            {
                m_exitGameObjects[i].m_object.SetActive(m_exitGameObjects[i].m_active);
            }
            m_isEntering = false;
            m_waitingTweens = null;
            for (int i = 0; i < m_exitTweens.Length; ++i)
            {
                m_exitTweens[i].m_tween.m_isForward = m_exitTweens[i].m_forward;
                m_exitTweens[i].m_tween.resetAndPlay();
                if (m_exitTweens[i].m_waitForFinishing)
                {
                    m_waitingTweens = m_exitTweens;
                }
            }
            onBeginExiting();
            if (m_waitingTweens == null)
            {
                onFinishExiting();
                panel.onFinishExitingState(this);
            }
        }

        protected virtual void onBeginEntering()
        {

        }

        protected virtual void onFinishEntering()
        {

        }

        protected virtual void onBeginExiting()
        {

        }

        protected virtual void onFinishExiting()
        {

        }

        void Update()
        {
            if (m_waitingTweens != null)
            {
                bool end = true;
                for (int i = 0; i < m_waitingTweens.Length; ++i)
                {
                    if (m_waitingTweens[i].m_waitForFinishing && m_waitingTweens[i].m_tween.IsPlaying)
                    {
                        end = false;
                        break;
                    }
                }
                if (end)
                {
                    m_waitingTweens = null;
                    if (m_isEntering)
                    {
                        onFinishEntering();
                        m_panel.onFinishEnteringState(this);
                    }
                    else
                    {
                        onFinishExiting();
                        m_panel.onFinishExitingState(this);
                    }
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIState", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UIState>(false);
        }
#endif
    }
}