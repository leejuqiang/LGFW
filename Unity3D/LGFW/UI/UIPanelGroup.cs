using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A group of panel, all the UIPanel under this GameObject are controlled by it
    /// </summary>
    public class UIPanelGroup : BaseMono
    {
        /// <summary>
        /// The start panel will be opened when this group is enabled
        /// </summary>
        public string m_startPanel;
        /// <summary>
        /// The order of the panel group. The bigger order makes this group in front of others.
        /// </summary>
        public int m_order;

        private Dictionary<string, UIPanel> m_panelDict = new Dictionary<string, UIPanel>();
        private HashSet<UIPanel> m_openedPanels = new HashSet<UIPanel>();
        private Transform m_trans;
        private Canvas m_canvas;

        /// <summary>
        /// Gets the transform of the group
        /// </summary>
        /// <value></value>
        public Transform Trans
        {
            get { return m_trans; }
        }

        protected override void doAwake()
        {
            m_trans = this.transform;
            m_canvas = this.GetComponent<Canvas>();
            if (UIPanelManager.Instance != null)
            {
                UIPanelManager.Instance.addGroup(this);
            }
            var panels = this.gameObject.GetComponentsInChildren<UIPanel>();
            for (int i = 0; i < panels.Length; ++i)
            {
                if (!string.IsNullOrEmpty(panels[i].m_id))
                {
                    m_panelDict.Add(panels[i].m_id, panels[i]);
                    panels[i].PanelGroup = this;
                    panels[i].Awake();
                    panels[i].gameObject.SetActive(false);
                }
            }
            m_openedPanels.Clear();
        }

        protected override void doStart()
        {
            if (!string.IsNullOrEmpty(m_startPanel))
            {
                UIPanel p = getPanel(m_startPanel);
                if (p != null)
                {
                    p.open();
                }
            }
        }

        /// <summary>
        /// Gets the most front panel
        /// </summary>
        /// <returns>The panel</returns>
        public UIPanel getTopPanel()
        {
            UIPanel max = null;
            foreach (var p in m_openedPanels)
            {
                if (max == null || p.Trans.GetSiblingIndex() > max.Trans.GetSiblingIndex())
                {
                    max = p;
                }
            }
            return max;
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

        /// <summary>
        /// Sends a panel to top
        /// </summary>
        /// <param name="p">The panel</param>
        public void bringToFront(UIPanel p)
        {
            Awake();
            int max = -1;
            foreach (UIPanel pp in m_openedPanels)
            {
                if (pp != p)
                {
                    int i = pp.Trans.GetSiblingIndex();
                    if (i > max)
                    {
                        max = i;
                    }
                }
            }
            if (max >= 0)
            {
                int pi = p.Trans.GetSiblingIndex();
                if (pi < max)
                {
                    p.Trans.SetSiblingIndex(max);
                }
            }
        }

        /// <summary>
        /// Sends a panel to bottom
        /// </summary>
        /// <param name="p">The panel</param>
        public void sendToBack(UIPanel p)
        {
            Awake();
            int min = m_trans.childCount;
            foreach (UIPanel pp in m_openedPanels)
            {
                if (pp != p)
                {
                    int i = pp.Trans.GetSiblingIndex();
                    if (i < min)
                    {
                        min = i;
                    }
                }
            }
            if (min < m_trans.childCount)
            {
                int pi = p.Trans.GetSiblingIndex();
                if (pi > min)
                {
                    p.Trans.SetSiblingIndex(min);
                }
            }
        }

        public void addToOpenedPanel(UIPanel p)
        {
            m_openedPanels.Add(p);
        }

        public bool removeFromOpenedPanel(UIPanel p)
        {
            return m_openedPanels.Remove(p);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIPanelGroup", false, (int)'p')]
        public static void createGlobal()
        {
            LEditorKits.addComponentToSelectedObjects<UIPanelGroup>(true);
        }
#endif
    }
}
