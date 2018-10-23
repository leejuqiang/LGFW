using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum UIPanelType
    {
        normal,
        popup,
    }

    /// <summary>
    /// A group of tweens for the panel
    /// </summary>
    [System.Serializable]
    public class UIPanelTweens
    {
        /// <summary>
        /// The id of the group
        /// </summary>
        public string m_id;
        /// <summary>
        /// The tweens
        /// </summary>
        public UITween[] m_tweens;

        /// <summary>
        /// Resets all tweens
        /// </summary>
        /// <param name="visible">If true, tweens will be reset to backward</param>
        public void resetTweens(bool visible)
        {
            bool isForward = visible ? false : true;
            for (int i = 0; i < m_tweens.Length; ++i)
            {
                m_tweens[i].reset(isForward);
            }
        }

        /// <summary>
        /// Plays all tweens
        /// </summary>
        /// <param name="isForward">If forward</param>
        public void play(bool isForward)
        {
            for (int i = 0; i < m_tweens.Length; ++i)
            {
                m_tweens[i].resetAndPlay(isForward);
            }
        }

        /// <summary>
        /// If all tweens have stopped
        /// </summary>
        /// <returns></returns>
        public bool isFinish()
        {
            for (int i = 0; i < m_tweens.Length; ++i)
            {
                if (m_tweens[i].IsPlaying)
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// A panel
    /// </summary>
    public class UIPanel : MonoBehaviour
    {

        /// <summary>
        /// The id of the panel
        /// </summary>
        public string m_id;
        /// <summary>
        /// If true this panel will be closed when back key released
        /// </summary>
        public bool m_responseBackKey = true;
        /// <summary>
        /// If the panel use panel tweens
        /// </summary>
        public bool m_enablePanelTweens = true;
        /// <summary>
        /// All the panel tweens
        /// </summary>
        public UIPanelTweens[] m_panelTweens;
        /// <summary>
        /// Type of the panel
        /// </summary>
        public UIPanelType m_panelType = UIPanelType.normal;

        private UIPanelTweens m_currentPanelTween;
        private Transform m_trans;
        private bool m_isVisible;
        private bool m_isPanelTweenPlaying;

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
        /// If this panel is playing any panel tween
        /// </summary>
        /// <value></value>
        public bool PanelTweenPlaying
        {
            get { return m_isPanelTweenPlaying; }
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

        void Awake()
        {
            if (m_currentPanelTween == null && m_panelTweens.Length > 0)
            {
                m_currentPanelTween = m_panelTweens[0];
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
        /// Sets the z value of this panel
        /// </summary>
        /// <param name="z">The z value</param>
        public void setZ(float z)
        {
            Vector3 v = Trans.position;
            v.z = z;
            Trans.position = v;
        }

        /// <summary>
        /// Sets the data of this panel
        /// </summary>
        /// <param name="datas">The data</param>
        public void refreshData(params object[] datas)
        {
            MessageData md = new MessageData(datas);
            this.gameObject.SendMessage(UIPanelManager.E_DATA, md, SendMessageOptions.DontRequireReceiver);
        }

        void Update()
        {
            if (m_isPanelTweenPlaying)
            {
                if (m_currentPanelTween == null || m_currentPanelTween.isFinish())
                {
                    m_isPanelTweenPlaying = false;
                    if (Visible)
                    {
                        doOpen();
                    }
                    else
                    {
                        doClose();
                    }
                }
            }
        }

        /// <summary>
        /// Changes this panel tween by id
        /// </summary>
        /// <param name="id">The id</param>
        public void changePanelTweens(string id)
        {
            if (m_currentPanelTween != null && m_currentPanelTween.m_id == id)
            {
                return;
            }
            for (int i = 0; i < m_panelTweens.Length; ++i)
            {
                if (id == m_panelTweens[i].m_id)
                {
                    m_currentPanelTween = m_panelTweens[i];
                    return;
                }
            }
        }

        /// <summary>
        /// Opens this panel
        /// </summary>
        public void open()
        {
            m_isPanelTweenPlaying = false;
            m_isVisible = true;
            if (m_currentPanelTween != null && m_enablePanelTweens)
            {
                m_currentPanelTween.play(true);
                m_isPanelTweenPlaying = !m_currentPanelTween.isFinish();
            }
            this.gameObject.SendMessage(UIPanelManager.E_OPEN, SendMessageOptions.DontRequireReceiver);
            if (!m_isPanelTweenPlaying)
            {
                doOpen();
            }
        }

        private void doOpen()
        {
            this.gameObject.SendMessage(UIPanelManager.E_OPEN_FINISH, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Closes this panel
        /// </summary>
        public void close()
        {
            m_isPanelTweenPlaying = false;
            m_isVisible = false;
            if (m_currentPanelTween != null && m_enablePanelTweens)
            {
                m_currentPanelTween.play(false);
                m_isPanelTweenPlaying = !m_currentPanelTween.isFinish();
            }
            this.gameObject.SendMessage(UIPanelManager.E_CLOSE, SendMessageOptions.DontRequireReceiver);
            if (!m_isPanelTweenPlaying)
            {
                doClose();
            }
        }

        private void doClose()
        {
            this.gameObject.SendMessage(UIPanelManager.E_CLOSE_FINISH, SendMessageOptions.DontRequireReceiver);
            if (m_panelType == UIPanelType.normal)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        private void createDefaultTweens()
        {
            m_panelTweens = new UIPanelTweens[1];
            UIPanelTweens pt = new UIPanelTweens();
            pt.m_id = "default";
            m_panelTweens[0] = pt;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIPanel", false, (int)'p')]
        public static void addToGameObjects()
        {
            UIPanel[] ps = LEditorKits.addComponentToSelectedOjbects<UIPanel>(true);
            for (int i = 0; i < ps.Length; ++i)
            {
                ps[i].createDefaultTweens();
            }
        }
#endif
    }
}
