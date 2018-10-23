using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A group of panel
    /// </summary>
    public class UIPanelGroup : MonoBehaviour
    {

        /// <summary>
        /// The nearest z of all panels in this group
        /// </summary>
        public float m_nearZ;
        /// <summary>
        /// The farest z of all panels in this group
        /// </summary>
        public float m_farZ;
        /// <summary>
        /// All panels in this group
        /// </summary>
        public UIPanel[] m_panels;
        /// <summary>
        /// The start panel will be opened when this group is enabled
        /// </summary>
        public string m_startPanel;

        private Dictionary<string, UIPanel> m_panelDict = new Dictionary<string, UIPanel>();
        private Transform m_trans;

        private LinkedList<UIPanel> m_openedPanels = new LinkedList<UIPanel>();

        /// <summary>
        /// Gets the panel on top
        /// </summary>
        /// <returns>The panel</returns>
        public UIPanel getTopPanel()
        {
            if (m_openedPanels.Count > 0)
            {
                return m_openedPanels.Last.Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the transform of the group
        /// </summary>
        /// <value></value>
        public Transform Trans
        {
            get { return m_trans; }
        }

        void Awake()
        {
            m_trans = this.transform;
            UIPanelManager.Instance.addGroup(this);
            for (int i = 0; i < m_panels.Length; ++i)
            {
                m_panelDict.Add(m_panels[i].m_id, m_panels[i]);
                if (m_panels[i].m_panelType == UIPanelType.normal)
                {
                    m_panels[i].PanelGroup = this;
                    m_panels[i].gameObject.SetActive(false);
                }
            }
        }

        void Start()
        {
            if (!string.IsNullOrEmpty(m_startPanel))
            {
                UIPanelManager.Instance.openPanel(m_startPanel);
            }
        }

        /// <summary>
        /// Gets a panel by id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The panel, null if not found</returns>
        public UIPanel getPanel(string id)
        {
            UIPanel p = null;
            m_panelDict.TryGetValue(id, out p);
            return p;
        }

        void OnDestroy()
        {
            if (UIPanelManager.Instance != null)
            {
                UIPanelManager.Instance.removeGroup(this);
            }
        }

        private void repositionPanels()
        {
            if (m_openedPanels.Count <= 0)
            {
                return;
            }
            if (m_openedPanels.Count <= 1)
            {
                m_openedPanels.First.Value.setZ(m_farZ);
                return;
            }
            float step = m_nearZ - m_farZ;
            step /= m_openedPanels.Count - 1;
            float z = m_farZ;
            LinkedListNode<UIPanel> n = m_openedPanels.First;
            while (n != null)
            {
                n.Value.setZ(z);
                n = n.Next;
                z += step;
            }
        }

        /// <summary>
        /// Sends a panel to top
        /// </summary>
        /// <param name="p">The panel</param>
        public void bringToFront(UIPanel p)
        {
            if (m_openedPanels.Remove(p))
            {
                m_openedPanels.AddLast(p);
                repositionPanels();
            }
        }

        /// <summary>
        /// Sends a panel to bottom
        /// </summary>
        /// <param name="p">The panel</param>
        public void sendToBack(UIPanel p)
        {
            if (m_openedPanels.Remove(p))
            {
                m_openedPanels.AddFirst(p);
                repositionPanels();
            }
        }

        public void addToOpenedPanel(UIPanel p)
        {
            LinkedListNode<UIPanel> n = m_openedPanels.First;
            while (n != null)
            {
                if (n.Value == p)
                {
                    return;
                }
                n = n.Next;
            }
            m_openedPanels.AddLast(p);
            repositionPanels();
        }

        public bool removeFromOpenedPanel(UIPanel p)
        {
            LinkedListNode<UIPanel> n = m_openedPanels.First;
            while (n != null)
            {
                if (n.Value == p)
                {
                    m_openedPanels.Remove(n);
                    return true;
                }
                n = n.Next;
            }
            return false;
        }

        public UIPanel onBackKey()
        {
            LinkedListNode<UIPanel> n = m_openedPanels.Last;
            while (n != null)
            {
                if (n.Value.m_responseBackKey)
                {
                    return n.Value;
                }
                n = n.Previous;
            }
            return null;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/create a panel group", false, (int)'z')]
        public static void createGlobal()
        {
            GameObject go = new GameObject("panels");
            go.AddComponent<UIPanelGroup>();
        }
#endif
    }
}
