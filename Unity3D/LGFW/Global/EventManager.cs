using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Event manager
    /// </summary>
    public class EventManager : MonoBehaviour
    {

        private static EventManager m_instance;

        /// <summary>
        /// Gets the singleton of event manger
        /// </summary>
        /// <value>The singleton</value>
        public static EventManager Instance
        {
            get { return m_instance; }
        }

        private Dictionary<string, LinkedList<EventTrigger>> m_allTriggers = new Dictionary<string, LinkedList<EventTrigger>>();

        void Awake()
        {
            m_instance = this;
        }

        void OnDestroy()
        {
            m_instance = null;
        }

        private void addTrigger(EventTrigger t, LinkedList<EventTrigger> l)
        {
            LinkedListNode<EventTrigger> n = l.First;
            while (n != null && n.Value.m_priority > t.m_priority)
            {
                n = n.Next;
            }
            if (n == null)
            {
                l.AddLast(t);
            }
            else
            {
                l.AddBefore(n, t);
            }
        }

        public void registerTriggerForEvent(EventTrigger t, string e)
        {
            registerTriggerForEvent(t, e, true);
        }

        private void registerTriggerForEvent(EventTrigger t, string e, bool addToRegisteredList)
        {
            LinkedList<EventTrigger> l = null;
            if (!m_allTriggers.TryGetValue(e, out l))
            {
                l = new LinkedList<EventTrigger>();
                m_allTriggers.Add(e, l);
            }
            addTrigger(t, l);
            if (addToRegisteredList)
            {
                t.m_registeredEvents.Add(e);
            }
        }

        public void removeTriggerForEvent(EventTrigger t, string e)
        {
            removeTriggerForEvent(t, e, true);
        }

        private void removeTriggerForEvent(EventTrigger t, string e, bool removeFromRegisteredList)
        {
            LinkedList<EventTrigger> l = null;
            if (m_allTriggers.TryGetValue(e, out l))
            {
                l.Remove(t);
                if (removeFromRegisteredList)
                {
                    t.m_registeredEvents.Remove(e);
                }
            }
        }

        public void registerEventTrigger(EventTrigger t)
        {
            for (int i = 0; i < t.m_responseToEvents.Length; ++i)
            {
                registerTriggerForEvent(t, t.m_responseToEvents[i], false);
            }
        }

        public void removeEventTrigger(EventTrigger t)
        {
            for (int i = 0; i < t.m_responseToEvents.Length; ++i)
            {
                removeTriggerForEvent(t, t.m_responseToEvents[i], false);
            }
            t.m_registeredEvents.Clear();
        }

        /// <summary>
        /// Sends an event
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <param name="isOneTime">If the event is a one time event</param>
        /// <param name="datas">Datas of the event</param>
        public void sendEvent(string id, bool isOneTime, params object[] datas)
        {
            LinkedList<EventTrigger> l = null;
            if (m_allTriggers.TryGetValue(id, out l))
            {
                LEvent e = new LEvent(id, isOneTime);
                e.m_datas = datas;
                LinkedListNode<EventTrigger> n = l.First;
                while (n != null)
                {
                    if (n.Value.ResponseEvent)
                    {
                        if (n.Value.onEvent(e) && e.m_oneTimeEvent)
                        {
                            break;
                        }
                    }
                    n = n.Next;
                }
            }
        }
    }
}
