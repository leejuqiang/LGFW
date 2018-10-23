using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class SelectComponentDrawer<T> : PropertyDrawer where T : MonoBehaviour
    {

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = base.GetPropertyHeight(property, label);
            T t = property.objectReferenceValue as T;
            if (t == null || t.gameObject.GetComponents<T>().Length <= 1)
            {
                return h;
            }
            return h * 2 + 1;
        }

        protected virtual string getComponentName(T t)
        {
            return "";
        }

        public string[] getPopup(T t, ref int index, ref T[] ts)
        {
            index = -1;
            ts = null;
            if (t == null)
            {
                return null;
            }
            GameObject go = t.gameObject;
            ts = go.GetComponents<T>();
            if (ts.Length <= 1)
            {
                return null;
            }
            string[] ret = new string[ts.Length];
            for (int i = 0; i < ts.Length; ++i)
            {
                if (t == ts[i])
                {
                    index = i;
                }
                ret[i] = i + " " + getComponentName(ts[i]);
            }
            return ret;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect rc = position;
            float h = base.GetPropertyHeight(property, label);
            rc.height = h;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(rc, property);

            int index = 0;
            T[] ts = null;
            string[] titles = getPopup(property.objectReferenceValue as T, ref index, ref ts);
            if (titles != null)
            {
                rc.x += 40;
                rc.y += h + 1;
                index = EditorGUI.Popup(rc, index, titles);
                property.objectReferenceValue = ts[index];
            }
            EditorGUI.EndProperty();
        }
    }
}
