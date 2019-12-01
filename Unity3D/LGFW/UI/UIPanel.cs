using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A panel
    /// </summary>
    public class UIPanel : BaseMono
    {

        /// <summary>
        /// The id of the panel
        /// </summary>
        public string m_id;
        /// <summary>
        /// The forward open state
        /// </summary>
        public UIState m_openForward;
        /// <summary>
        /// The backward open state
        /// </summary>
        public UIState m_openBackward;
        /// <summary>
        /// The forward close state
        /// </summary>
        public UIState m_closeForward;
        /// <summary>
        /// The backward close state
        /// </summary>
        public UIState m_closeBackward;

        /// <summary>
        /// The custom states
        /// </summary>
        public UIState[] m_states;

        private Transform m_trans;
        private UIState m_currentState;
        private UIState m_nextState;
        private Dictionary<string, UIState> m_stateMap = new Dictionary<string, UIState>();
        private UIPanelData m_data;
        private CanvasGroup m_canvasG;

        /// <summary>
        /// If this panel should be brought to the front when opened
        /// </summary>
        public bool m_bringToFront;
        private bool m_isVisible;
        private bool m_isForward = true;

        /// <summary>
        /// The data used for this panel
        /// </summary>
        /// <value>The data</value>
        public UIPanelData Data
        {
            get { return m_data; }
        }

        /// <summary>
        /// The panel group of this panel
        /// </summary>
        /// <value></value>
        public UIPanelGroup PanelGroup
        {
            get;
            set;
        }


        /// <summary>
        /// If this panel is visible
        /// </summary>
        /// <value></value>
        public bool Visible
        {
            get { return m_isVisible; }
        }

        /// <summary>
        /// Gets the transform of this panel
        /// </summary>
        /// <value></value>
        public Transform Trans
        {
            get
            {
                if (m_trans == null)
                {
                    m_trans = this.transform;
                }
                return m_trans;
            }
        }

        protected override void doAwake()
        {
            m_stateMap.Clear();
            m_canvasG = this.GetComponent<CanvasGroup>();
            for (int i = 0; i < m_states.Length; ++i)
            {
                if (!string.IsNullOrEmpty(m_states[i].m_id))
                {
                    m_stateMap[m_states[i].m_id] = m_states[i];
                }
            }
        }

        /// <summary>
        /// Sends this panel to the top
        /// </summary>
        public void bringToFront()
        {
            PanelGroup.bringToFront(this);
        }

        /// <summary>
        /// Sends this panel to the bottom
        /// </summary>
        public void sendToBack()
        {
            PanelGroup.sendToBack(this);
        }

        /// <summary>
        /// Called when a state has finished exiting
        /// </summary>
        /// <param name="state"></param>
        public void onFinishExitingState(UIState state)
        {
            if (m_nextState != null)
            {
                m_currentState = m_nextState;
                m_nextState = null;
                m_currentState.enterState(this);
            }
        }

        /// <summary>
        /// Called when a state has finished entering
        /// </summary>
        /// <param name="state"></param>
        public void onFinishEnteringState(UIState state)
        {
            if (state == m_closeForward || state == m_closeBackward)
            {
                doClose();
            }
        }

        /// <summary>
        /// Changes the state of the panel
        /// </summary>
        /// <param name="id">The id of the new state</param>
        public void changeState(string id)
        {
            UIState s = null;
            if (m_stateMap.TryGetValue(id, out s))
            {
                changeState(s);
            }
        }

        /// <summary>
        /// Changes the state of the panel
        /// </summary>
        /// <param name="s">The new state</param>
        protected void changeState(UIState s)
        {
            if (m_currentState != null)
            {
                m_nextState = s;
                m_currentState.exitState(this);
            }
            else
            {
                m_nextState = null;
                m_currentState = s;
                m_currentState.enterState(this);
            }
        }

        /// <summary>
        /// Override this function for Update()
        /// </summary>
        protected virtual void doUpdate()
        {

        }

        void Update()
        {
            if (m_canvasG != null)
            {
                if (m_currentState != null)
                {
                    m_canvasG.interactable = !m_currentState.IsWaiting;
                }
                else
                {
                    m_canvasG.interactable = true;
                }
            }
            doUpdate();
        }

        /// <summary>
        /// Refreshes the panel
        /// </summary>
        /// <param name="data"></param>
        public void refresh(UIPanelData data)
        {
            m_data = data;
            if (m_currentState != null)
            {
                m_currentState.refresh();
            }
            onRefresh();
        }

        /// <summary>
        /// Called when a panel is refreshed. A panel is refreshed when it's opened
        /// </summary>
        protected virtual void onRefresh()
        {
        }

        /// <summary>
        /// Called when the player pressed the back key
        /// </summary>
        public void onPressBackKey()
        {
            if (m_currentState != null & m_currentState.IsWaiting)
            {
                return;
            }
            onBackKey();
        }

        /// <summary>
        /// Override this function to response the back key, default behavior is close the panel
        /// </summary>
        protected virtual void onBackKey()
        {
            close();
        }

        /// <summary>
        /// Opens this panel forward, if m_openForward is not null, switches the state to m_openForward
        /// </summary>
        public void open()
        {
            open(true, null);
        }

        /// <summary>
        /// Opens this panel, if m_openForward or m_openBackward is not null, switches the state to one of them
        /// </summary>
        /// <param name="isForward"></param>
        public void open(bool isForward)
        {
            open(isForward, null);
        }

        /// <summary>
        /// Opens this panel, if m_openForward or m_openBackward is not null, switches the state to one of them
        /// </summary>
        /// <param name="isForward">Is opening forward</param>
        /// <param name="data">The data for this panel</param>
        public void open(bool isForward, UIPanelData data)
        {
            if (m_bringToFront)
            {
                bringToFront();
            }
            if (!m_isVisible)
            {
                Awake();
                m_isForward = true;
                m_isVisible = true;
                if (PanelGroup != null)
                {
                    PanelGroup.addToOpenedPanel(this);
                }
                this.gameObject.SetActive(true);
                UIState s = m_isForward ? m_openForward : m_openBackward;
                if (s != null)
                {
                    changeState(s);
                }
                refresh(data);
            }
        }

        protected void doClose()
        {
            m_currentState = null;
            m_nextState = null;
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Closes this panel forward, if m_closeForward is not null, switches the state to it
        /// </summary>
        /// <param name="isForward"></param>
        public void close()
        {
            close(true);
        }

        /// <summary>
        /// Closes this panel, if m_closeForward or m_closeBackward is not null, switches the state to one of them
        /// </summary>
        /// <param name="isForward">Is close forward</param>
        public void close(bool isFoward)
        {
            if (m_isVisible)
            {
                m_isVisible = false;
                m_isForward = isFoward;
                if (PanelGroup != null)
                {
                    PanelGroup.removeFromOpenedPanel(this);
                }
                UIState s = m_isForward ? m_closeForward : m_closeBackward;
                if (s != null)
                {
                    changeState(s);
                }
                else
                {
                    doClose();
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIPanel", false, (int)'p')]
        public static void addToGameObjects()
        {
            UIPanel[] ps = LEditorKits.addComponentToSelectedObjects<UIPanel>(true);
            LEditorKits.addComponentToSelectedObjects<CanvasGroup>(true);
            for (int i = 0; i < ps.Length; ++i)
            {
                ps[i].m_id = ps[i].gameObject.name;
            }
            UIState[] f = LEditorKits.addComponentToSelectedObjects<UIState>(false);
            for (int i = 0; i < f.Length; ++i)
            {
                f[i].m_id = "openForward";
                ps[i].m_openForward = f[i];
            }
            f = LEditorKits.addComponentToSelectedObjects<UIState>(false);
            for (int i = 0; i < f.Length; ++i)
            {
                f[i].m_id = "closeForward";
                ps[i].m_closeForward = f[i];
            }
        }
#endif
    }
}
