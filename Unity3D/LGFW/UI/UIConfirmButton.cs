using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Reflection;

namespace LGFW
{
    /// <summary>
    /// A button which check if the button can be pressed and then call different callbacks
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIConfirmButton : BaseMono
    {
        protected const int REFRESH_FLAG = 1 << 8;
        /// <summary>
        /// The integer id of the button
        /// </summary>
        public int m_id;
        /// <summary>
        /// The string id of the button
        /// </summary>
        public string m_sid;
        /// <summary>
        /// If the button can be pressed, the success events are invoked
        /// </summary>
        public Button.ButtonClickedEvent m_successEvent;
        /// <summary>
        /// If the button can't be pressed, the fail events are invoked, pass a string as the reason of the failure
        /// </summary>
        public ConfirmButtonFailEvent m_failEvent;

        protected Button m_btn;

        protected override void doAwake()
        {
            m_btn = this.gameObject.GetComponent<Button>();
        }

        /// <summary>
        /// Bind this function to a Button
        /// </summary>
        public void onClick()
        {
            Awake();
            var error = check();
            if (string.IsNullOrEmpty(error))
            {
                m_successEvent.Invoke();
            }
            else
            {
                m_failEvent.Invoke(error);
            }
        }

        /// <summary>
        /// Overwrite this function to check if the button can be pressed
        /// </summary>
        /// <param name="id">The integer id</param>
        /// <param name="sid">The string id</param>
        /// <returns>The error message. If there is no error, return an empty or null string</returns>
        protected virtual string check()
        {
            return "";
        }

        /// <summary>
        /// Refreshes this button, if the button can't be pressed, it is set interactable = false
        /// </summary>
        public void refreshButton()
        {
            m_flag |= REFRESH_FLAG;
        }

        void Update()
        {
            if ((m_flag & REFRESH_FLAG) != 0)
            {
                m_flag &= ~REFRESH_FLAG;
                m_btn.interactable = string.IsNullOrEmpty(check());
            }
        }

        [System.Serializable]
        public class ConfirmButtonFailEvent : UnityEvent<string>
        {
            public ConfirmButtonFailEvent() : base()
            {

            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIConfirmButton", false, (int)'c')]
        public static void addToGameObjects()
        {
            UIConfirmButton[] bs = LEditorKits.addComponentToSelectedObjects<UIConfirmButton>(true);
            for (int i = 0; i < bs.Length; ++i)
            {
                Button b = bs[i].gameObject.GetComponent<Button>();
                System.Type t = typeof(UIConfirmButton);
                var minfo = t.GetMethod("onClick", BindingFlags.Instance | BindingFlags.Public);
                UnityEngine.Events.UnityAction click = System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), bs[i], minfo) as UnityEngine.Events.UnityAction;
                UnityEditor.Events.UnityEventTools.AddPersistentListener(b.onClick, click);
            }
        }
#endif
    }
}