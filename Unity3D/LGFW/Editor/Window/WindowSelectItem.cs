using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class WindowSelectItem : EditorWindow
    {

        public delegate void OnSelectItem(object item, MessageData customData);

        protected OnSelectItem m_selectCallback;
        protected MessageData m_customMessage;

        protected virtual void open(OnSelectItem callback, object[] datas)
        {
            m_customMessage = new MessageData(datas);
            m_selectCallback = callback;
        }

        void OnDisable()
        {
            m_selectCallback = null;
            onClose();
        }

        protected virtual void onClose()
        {
            //todo
        }
    }
}
