using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Panel Manager
    /// </summary>
    public class UIPanelManager : MonoBehaviour
    {

        /// <summary>
        /// Called when a panel begins to open
        /// </summary>
        public const string E_OPEN = "OnOpen";
        /// <summary>
        /// Called when a panel finishes opening
        /// </summary>
        public const string E_OPEN_FINISH = "OnFinishOpen";
        /// <summary>
        /// Called when a panel begins to close
        /// </summary>
        public const string E_CLOSE = "OnClose";
        /// <summary>
        /// Called when a panel finishes closing
        /// </summary>
        public const string E_CLOSE_FINISH = "OnFinishClose";
        /// <summary>
        /// Called when a panel is assigned new data, or refreshed
        /// </summary>
        public const string E_DATA = "OnData";
        /// <summary>
        /// Called when player presses back key of a panel
        /// </summary>
        public const string E_BACK_KEY = "OnBackKey";

        private static UIPanelManager m_instance;

        /// <summary>
        /// Gets the singleton of UIPanelManager
        /// </summary>
        /// <value>The singleton</value>
        public static UIPanelManager Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// If true, panels will received back key event
        /// </summary>
        public bool m_useBackKey = true;

        private LinkedList<UIPanelGroup> m_groups = new LinkedList<UIPanelGroup>();

        void Awake()
        {
            m_instance = this;
        }

        void OnDestroy()
        {
            m_instance = null;
        }

        public void addGroup(UIPanelGroup g)
        {
            LinkedListNode<UIPanelGroup> n = m_groups.First;
            while (n != null)
            {
                if (n.Value == g)
                {
                    return;
                }
                if (n.Value.m_nearZ < g.m_nearZ)
                {
                    m_groups.AddBefore(n, g);
                    return;
                }
                n = n.Next;
            }
            m_groups.AddLast(g);
        }

        public void removeGroup(UIPanelGroup g)
        {
            m_groups.Remove(g);
        }

        /// <summary>
        /// Gets the custom script on a panel
        /// </summary>
        /// <returns>The custom script</returns>
        /// <param name="id">The id of the panel</param>
        /// <typeparam name="T">Type of the script</typeparam>
        public T getCustomPanel<T>(string id) where T : MonoBehaviour
        {
            UIPanel p = getUIPanel(id);
            if (p != null)
            {
                return p.GetComponent<T>();
            }
            return null;
        }

        /// <summary>
        /// Gets the UIPanel by id
        /// </summary>
        /// <returns>The UIPanel</returns>
        /// <param name="id">The id</param>
        public UIPanel getUIPanel(string id)
        {
            UIPanelGroup g = null;
            return getUIPanel(id, out g);
        }

        private UIPanel getUIPanel(string id, out UIPanelGroup g)
        {
            g = null;
            LinkedListNode<UIPanelGroup> n = m_groups.First;
            while (n != null)
            {
                UIPanel p = n.Value.getPanel(id);
                if (p != null)
                {
                    g = n.Value;
                    return p;
                }
                n = n.Next;
            }
            return null;
        }

        private UIPanel createPanel(UIPanel p, UIPanelGroup g)
        {
            GameObject go = ResourceManager.Instance.initPrefab(p.gameObject);
            Transform t = go.transform;
            t.parent = g.Trans;
            t.localPosition = Vector3.zero;
            p = go.GetComponent<UIPanel>();
            p.PanelGroup = g;
            return p;
        }

        /// <summary>
        /// Opens a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        /// <param name="panelTweenId">Play the tween list of this id when open</param>
        public void openPanel(string id, string panelTweenId = "")
        {
            UIPanelGroup g = null;
            UIPanel p = getUIPanel(id, out g);
            if (p == null)
            {
                return;
            }
            if (p.m_panelType == UIPanelType.popup)
            {
                p = createPanel(p, g);
            }
            if (p.Visible)
            {
                return;
            }
            if (!string.IsNullOrEmpty(panelTweenId))
            {
                p.changePanelTweens(panelTweenId);
            }
            g.addToOpenedPanel(p);
            p.gameObject.SetActive(true);
            p.open();
        }

        /// <summary>
        /// Closes a panel
        /// </summary>
        /// <param name="p">The UIPanel</param>
        /// <param name="panelTweenId">Play the tween list of this id when close</param>
        public void closePanel(UIPanel p, string panelTweenId = "")
        {
            if (!p.Visible)
            {
                return;
            }
            LinkedListNode<UIPanelGroup> n = m_groups.Last;
            while (n != null)
            {
                if (n.Value.removeFromOpenedPanel(p))
                {
                    if (!string.IsNullOrEmpty(panelTweenId))
                    {
                        p.changePanelTweens(panelTweenId);
                    }
                    p.close();
                    return;
                }
                n = n.Previous;
            }
        }

        void Update()
        {
            if (m_useBackKey)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    LinkedListNode<UIPanelGroup> n = m_groups.Last;
                    while (n != null)
                    {
                        UIPanel p = n.Value.onBackKey();
                        if (p != null)
                        {
                            if (p.Visible)
                            {
                                p.gameObject.SendMessage(E_BACK_KEY, SendMessageOptions.DontRequireReceiver);
                                closePanel(p);
                            }
                            break;
                        }
                        n = n.Previous;
                    }
                }
            }
        }
    }
}
