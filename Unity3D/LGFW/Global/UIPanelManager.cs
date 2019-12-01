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
        /// If true, panels will received the escape key as a back
        /// </summary>
        public bool m_useBackKey = true;

        private HashSet<UIPanelGroup> m_groups = new HashSet<UIPanelGroup>();
        private LinkedList<UIPanel> m_openStack = new LinkedList<UIPanel>();

        void Awake()
        {
            m_instance = this;
        }

        void OnDestroy()
        {
            m_instance = null;
        }

        /// <summary>
        /// Opens a panel and push it to the stack, you can use popFromStack() to close this panel later
        /// </summary>
        /// <param name="p">The panel</param>
        /// <param name="data">The data used to open this panel</param>
        public void pushToStack(UIPanel p, UIPanelData data)
        {
            var n = m_openStack.Last;
            if (n != null)
            {
                if (n.Value == p)
                {
                    p.refresh(data);
                    return;
                }
                n.Value.close(false);
            }
            p.open(true, data);
            m_openStack.AddLast(p);
        }

        public void addGroup(UIPanelGroup g)
        {
            m_groups.Add(g);
        }

        public void removeGroup(UIPanelGroup g)
        {
            m_groups.Remove(g);
        }

        /// <summary>
        /// Gets the UIPanel by id
        /// </summary>
        /// <returns>The subclass of UIPanel with type T</returns>
        /// <param name="id">The id</param>
        /// <typeparam name="T">Type of the UIPanel</typeparam>
        public T getUIPanel<T>(string id) where T : UIPanel
        {
            foreach (UIPanelGroup g in m_groups)
            {
                UIPanel p = g.getPanel(id);
                if (p != null)
                {
                    return (T)p;
                }
            }
            return null;
        }

        /// <summary>
        /// Opens a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        public void openPanel(string id)
        {
            openPanel(id, true, null);
        }

        /// <summary>
        /// Opens a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        /// <param name="isForward">Is open forward</param>
        public void openPanel(string id, bool isForward)
        {
            openPanel(id, isForward, null);
        }

        /// <summary>
        /// Opens a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        /// <param name="data">The data for the panel</param>
        /// <param name="isForward">Is open forward</param>
        public void openPanel(string id, bool isForward, UIPanelData data)
        {
            UIPanel p = getUIPanel<UIPanel>(id);
            if (p == null)
            {
                return;
            }
            p.open(isForward, data);
        }

        /// <summary>
        /// Closes a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        public void closePanel(string id)
        {
            closePanel(id, true);
        }

        /// <summary>
        /// Back to the last panel
        /// </summary>
        public void popFromStack()
        {
            var n = m_openStack.Last;
            if (n != null)
            {
                n.Value.close(true);
                m_openStack.RemoveLast();
                n = m_openStack.Last;
                if (n != null)
                {
                    n.Value.open(false);
                }
            }
        }

        /// <summary>
        /// Closes a panel
        /// </summary>
        /// <param name="id">The id of the panel</param>
        /// <param name="isForward">Is close forward</param>
        public void closePanel(string id, bool isForward)
        {
            UIPanel p = getUIPanel<UIPanel>(id);
            if (p == null)
            {
                return;
            }
            p.close(isForward);
        }

        private int sortGroups(UIPanelGroup l, UIPanelGroup r)
        {
            return r.m_order - l.m_order;
        }

        private UIPanel getTopPanel()
        {
            List<UIPanelGroup> l = new List<UIPanelGroup>();
            l.AddRange(m_groups);
            l.Sort(sortGroups);
            for (int i = 0; i < l.Count; ++i)
            {
                var p = l[i].getTopPanel();
                if (p != null)
                {
                    return p;
                }
            }
            return null;
        }

        void Update()
        {
            if (m_useBackKey)
            {
                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    var p = getTopPanel();
                    if (p != null)
                    {
                        p.onPressBackKey();
                    }
                }
            }
        }
    }
}
