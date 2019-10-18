using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace LGFW
{
    public class ObjectInspector
    {
        private Dictionary<string, bool> m_foldMap = new Dictionary<string, bool>();
        private Dictionary<string, object> m_tempValues = new Dictionary<string, object>();
        private object getObjectValue<T>(object obj)
        {
            T ret = default;
            if (obj != null)
            {
                ret = (T)obj;
            }
            return ret;
        }

        private object drawPrimitive(System.Type t, object o, string name)
        {
            if (t == typeof(int))
            {
                return EditorGUILayout.IntField(name, (int)(getObjectValue<int>(o)));
            }
            else if (t == typeof(long))
            {
                return EditorGUILayout.LongField(name, (int)(getObjectValue<int>(o)));
            }
            else if (t == typeof(short))
            {
                return (short)EditorGUILayout.IntField(name, (short)(getObjectValue<short>(o)));
            }
            else if (t == typeof(byte))
            {
                return (byte)EditorGUILayout.IntField(name, (byte)(getObjectValue<byte>(o)));
            }
            else if (t == typeof(uint))
            {
                long l = EditorGUILayout.LongField(name, (uint)(getObjectValue<uint>(o)));
                return (uint)l;
            }
            else if (t == typeof(ushort))
            {
                int i = EditorGUILayout.IntField(name, (ushort)(getObjectValue<ushort>(o)));
                return (ushort)i;
            }
            else if (t == typeof(ulong))
            {
                string s = EditorGUILayout.TextField(name, getObjectValue<string>(o).ToString());
                ulong l = 0;
                if (ulong.TryParse(s, out l))
                {
                    return l;
                }
                return 0;
            }
            else if (t == typeof(char))
            {
                string str = EditorGUILayout.TextField(name, getObjectValue<char>(o).ToString());
                if (string.IsNullOrEmpty(str))
                {
                    return '\0';
                }
                return str[0];
            }
            else if (t == typeof(float))
            {
                return EditorGUILayout.FloatField(name, (float)getObjectValue<float>(o));
            }
            else if (t == typeof(double))
            {
                return EditorGUILayout.DoubleField(name, (double)getObjectValue<double>(o));
            }
            else if (t == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool)getObjectValue<bool>(o));
            }
            return EditorGUILayout.TextField(name, o == null ? "" : o.ToString());
        }

        private object drawEnum(System.Type t, object o, string name)
        {
            System.Array arr = System.Enum.GetValues(t);
            System.Enum e = EditorGUILayout.EnumPopup(name, (System.Enum)System.Enum.ToObject(t, getObjectValue<int>(o)));
            return (int)System.Convert.ChangeType(e, System.Enum.GetUnderlyingType(t));
        }

        private List<object> drawList(List<object> l, System.Type elementType, string name, string parentName)
        {
            if (l == null)
            {
                l = new List<object>();
            }
            bool isShow = true;
            string pn = drawFoldOut(parentName, name, ref isShow);
            if (isShow)
            {
                ++EditorGUI.indentLevel;
                int s = EditorGUILayout.IntField("size", l.Count);
                if (s != l.Count)
                {
                    if (s < l.Count)
                    {
                        l.RemoveRange(s, l.Count - s);
                    }
                    else
                    {
                        if (l.Count <= 0)
                        {
                            for (int i = 0; i < s; ++i)
                            {
                                l.Add(null);
                            }
                        }
                        else
                        {
                            while (l.Count < s)
                            {
                                l.Add(l[l.Count - 1]);
                            }
                        }
                    }
                }
                for (int i = 0; i < s; ++i)
                {
                    l[i] = drawObject(l[i], elementType, i.ToString(), pn);
                }
                --EditorGUI.indentLevel;
            }
            return l;
        }

        private object getTempValue(string k)
        {
            object ret = null;
            m_tempValues.TryGetValue(k, out ret);
            return ret;
        }

        private void setTempValue(string k, object v)
        {
            m_tempValues[k] = v;
        }

        private List<object> drawSet(List<object> l, System.Type elementType, string name, string parentName)
        {
            if (l == null)
            {
                l = new List<object>();
            }
            bool isShow = true;
            string pn = drawFoldOut(parentName, name, ref isShow);
            if (isShow)
            {
                ++EditorGUI.indentLevel;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("new");
                bool btn = false;
                if (GUILayout.Button("+"))
                {
                    btn = true;
                }
                GUILayout.EndHorizontal();
                object o = getTempValue(pn + ".new");
                o = drawObject(o, elementType, "", pn + ".new");
                setTempValue(pn + ".new", o);
                if (btn)
                {
                    l.Add(o);
                    setTempValue(pn + ".new", null);
                }
                for (int i = 0; i < l.Count; ++i)
                {
                    btn = false;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i.ToString());
                    if (GUILayout.Button("-"))
                    {
                        btn = true;
                    }
                    GUILayout.EndHorizontal();
                    l[i] = drawObject(l[i], elementType, i.ToString(), pn);
                    if (btn)
                    {
                        l.RemoveAt(i);
                        break;
                    }
                }
                --EditorGUI.indentLevel;
            }
            return l;
        }
        private Dictionary<object, object> drawDict(Dictionary<object, object> dict, System.Type keyType, System.Type valueType, string name, string parentName)
        {
            if (dict == null)
            {
                dict = new Dictionary<object, object>();
            }
            bool isShow = true;
            string pn = drawFoldOut(parentName, name, ref isShow);
            if (isShow)
            {
                ++EditorGUI.indentLevel;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("new");
                bool btn = false;
                if (GUILayout.Button("+"))
                {
                    btn = true;
                }
                GUILayout.EndHorizontal();
                object k = getTempValue(pn + ".new.key");
                k = drawObject(k, keyType, "key", pn + ".new");
                setTempValue(pn + ".new.key", k);
                object v = getTempValue(pn + ".new.value");
                v = drawObject(v, valueType, "value", pn + ".new");
                setTempValue(pn + ".new.value", v);
                if (btn)
                {
                    dict[k] = v;
                    setTempValue(pn + ".new.key", null);
                    setTempValue(pn + ".new.value", null);
                }
                int i = 0;
                foreach (object key in dict.Keys)
                {
                    btn = false;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(i.ToString());
                    if (GUILayout.Button("-"))
                    {
                        btn = true;
                    }
                    GUILayout.EndHorizontal();
                    object value = dict[key];
                    value = drawObject(value, valueType, i + " " + key.ToString(), pn);
                    if (btn)
                    {
                        dict.Remove(key);
                        break;
                    }
                    ++i;
                }
                --EditorGUI.indentLevel;
            }
            return dict;
        }

        private IDictionary dictToIDict(Dictionary<object, object> dict, System.Type dictType)
        {
            IDictionary ret = (IDictionary)System.Activator.CreateInstance(dictType);
            foreach (object k in dict.Keys)
            {
                ret[k] = dict[k];
            }
            return ret;
        }

        public object convertBack(System.Type t, object o)
        {
            if (t.IsEnum)
            {
                return System.Enum.ToObject(t, o);
            }
            if (t.IsArray)
            {
                return JsonSerializer.listToArray((List<object>)o, t);
            }
            if (JsonSerializer.isList(t))
            {
                return JsonSerializer.listToIlist((List<object>)o, t);
            }
            if (JsonSerializer.isDictionary(t))
            {
                return dictToIDict((Dictionary<object, object>)o, t);
            }
            if (JsonSerializer.isSet(t))
            {
                return JsonSerializer.listToISet((List<object>)o, t);
            }
            return o;
        }

        private string drawFoldOut(string parentName, string name, ref bool isShow)
        {
            bool f = false;
            string foldName = parentName + "." + name;
            if (!m_foldMap.TryGetValue(foldName, out f))
            {
                m_foldMap[foldName] = true;
                f = true;
            }
            f = EditorGUILayout.Foldout(f, name, true);
            m_foldMap[foldName] = f;
            isShow = f;
            return foldName;
        }

        public object drawObject(object o, System.Type t, string name, string parentName)
        {
            if (t.IsPrimitive || t == typeof(string))
            {
                return drawPrimitive(t, o, name);
            }
            if (t.IsEnum)
            {
                return drawEnum(t, o, name);
            }
            if (t.IsArray)
            {
                return convertBack(t, drawList(JsonSerializer.arrayToList(o), t.GetElementType(), name, parentName));
            }
            if (o == null)
            {
                o = System.Activator.CreateInstance(t);
            }
            if (JsonSerializer.isList(t))
            {
                return convertBack(t, drawList(JsonSerializer.ilistToList(o), t.GetGenericArguments()[0], name, parentName));
            }
            if (JsonSerializer.isDictionary(t))
            {
                Dictionary<object, object> dict = JsonSerializer.idictionaryToDict(o);
                var ts = t.GetGenericArguments();
                return convertBack(t, drawDict(dict, ts[0], ts[1], name, parentName));
            }
            if (JsonSerializer.isSet(t))
            {
                return convertBack(t, drawSet(JsonSerializer.isetToList(o), t.GetGenericArguments()[0], name, parentName));
            }
            bool isShow = true;
            string pn = drawFoldOut(parentName, name, ref isShow);
            if (isShow)
            {
                ++EditorGUI.indentLevel;
                List<string> names = new List<string>();
                List<FieldInfo> fields = JsonSerializer.getAllField(t, false, false, names);
                for (int i = 0; i < fields.Count; ++i)
                {
                    object value = fields[i].GetValue(o);
                    value = drawObject(value, fields[i].FieldType, names[i], pn);
                    fields[i].SetValue(o, value);
                }
                --EditorGUI.indentLevel;
            }
            return o;
        }
    }
}
