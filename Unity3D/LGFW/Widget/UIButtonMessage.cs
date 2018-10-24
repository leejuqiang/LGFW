using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Attached to a button to cast event
    /// </summary>
    public class UIButtonMessage : UITouchWidget
    {

        /// <summary>
        /// The target should send message to
        /// </summary>
        public GameObject m_target;
        /// <summary>
        /// The message sent when pressed
        /// </summary>
        public string m_pressTrigger;
        /// <summary>
        /// The message sent when released
        /// </summary>
        public string m_releaseTrigger;
        /// <summary>
        /// The int id of this button
        /// </summary>
        public int m_intId;
        /// <summary>
        /// The string id of this button
        /// </summary>
        public string m_stringId;

        /// <summary>
        /// If this is not null, the button won't react when the panel is playing opening or closing tweens
        /// </summary>
        public UIPanel m_belongPanel;

        protected override void doPress(UITouch t)
        {
            base.doPress(t);
            sendMessage(m_pressTrigger);
        }

        protected override void doRelease(UITouch t)
        {
            base.doRelease(t);
            if (t.m_releaseInside)
            {
                sendMessage(m_releaseTrigger);
            }
        }

        private void sendMessage(string s)
        {
            if (m_belongPanel != null && m_belongPanel.PanelTweenPlaying)
            {
                return;
            }
            if (m_target != null && !string.IsNullOrEmpty(s))
            {
                m_target.SendMessage(s, this, SendMessageOptions.DontRequireReceiver);
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIButtonMessage", false, (int)'b')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UIButtonMessage>(true);
        }
#endif
    }
}
