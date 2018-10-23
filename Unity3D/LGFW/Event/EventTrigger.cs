using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Any instance implement this interface can be an event receiver
    /// </summary>
    public interface IEventTrigger
    {
        /// <summary>
        /// When receive an event, this function will be called
        /// </summary>
        /// <param name="e">The event</param>
        void onEvent(LEvent e);
    }

    /// <summary>
    /// Attachs this to a GameObject so it will receive event
    /// </summary>
    public class EventTrigger : MonoBehaviour
    {

        /// <summary>
        /// The events will be responsed
        /// </summary>
        public string[] m_responseToEvents;
        /// <summary>
        /// A Monobehaviour implements the IEventTrigger interface, this will be the script responses to events
        /// </summary>
        [HideInInspector]
        public MonoBehaviour m_receiver;

        /// <summary>
        /// The priority of this trigger, if more then one triggers is attached to the GameObject, the hihger priority trigger will be notified first
        /// </summary>
        public int m_priority;
        /// <summary>
        /// If true, only active GameObject will receive events
        /// </summary>
        public bool m_mustActive = true;
        /// <summary>
        /// If true, the trigger will use GameObject.SendMessage when receive an event, the message's name will be the event's id. Otherwise, will call onEvent
        /// </summary>
        public bool m_useSendMessage = true;

        private IEventTrigger m_trigger;
        [System.NonSerialized]
        public List<string> m_registeredEvents = new List<string>();

        public bool ResponseEvent
        {
            get
            {
                return !m_mustActive || this.gameObject.activeInHierarchy;
            }
        }

        void Awake()
        {
            if (!m_useSendMessage)
            {
                m_trigger = (IEventTrigger)m_receiver;
            }
        }

        public bool onEvent(LEvent e)
        {
            if (m_receiver == null)
            {
                return false;
            }
            if (m_useSendMessage)
            {
                m_receiver.gameObject.SendMessage(e.m_id, e, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                if (m_trigger == null)
                {
                    return false;
                }
                m_trigger.onEvent(e);
            }
            return true;
        }

        void Start()
        {
            EventManager.Instance.registerEventTrigger(this);
        }

        void OnDestroy()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.removeEventTrigger(this);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Event/EventTrigger", false, (int)'e')]
        public static void addToSelect()
        {
            LEditorKits.addComponentToSelectedOjbects<EventTrigger>(false);
        }
#endif
    }
}
