using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    [CustomPropertyDrawer(typeof(UIState), true)]
    public class UIStateDrawer : SelectComponentDrawer<UIState>
    {
        protected override string getComponentName(UIState t)
        {
            return t.m_id;
        }
    }
}