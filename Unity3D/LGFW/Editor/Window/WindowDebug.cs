using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace LGFW
{
    public class WindowDebugParam
    {
        public string m_name;
        public object m_value;
        public List<object> m_list;

        public bool resizeList(int size)
        {
            if (size == m_list.Count)
            {
                return false;
            }
            if (size < m_list.Count)
            {
                m_list.RemoveRange(size, m_list.Count - size);
            }
            else
            {
                while (m_list.Count < size)
                {
                    if (m_list.Count > 0)
                    {
                        m_list.Add(m_list[m_list.Count - 1]);
                    }
                    else
                    {
                        m_list.Add(null);
                    }
                }
            }
            return true;
        }
    }

    public class WindowDebug : EditorWindow
    {

        private static WindowDebug m_instance;

        private string m_searchText;
        private GameObject m_lastGo;
        private bool m_includeInherit;
        private bool m_includePrivate;

        private MonoBehaviour[] m_monos;
        private string[] m_monoNames;
        private List<MethodInfo> m_functions = new List<MethodInfo>();
        private int m_selectMono;
        private int m_selectFunction;
        private List<string> m_displayFunctions = new List<string>();
        private List<MethodInfo> m_displayMethods = new List<MethodInfo>();
        private List<WindowDebugParam> m_paramValues = new List<WindowDebugParam>();

        [MenuItem("LGFW/Editor/Debug Window", false, (int)'d')]
        public static void showWindow()
        {
            if (m_instance == null)
            {
                m_instance = EditorWindow.GetWindow<WindowDebug>(true, "Debug");
            }
            m_instance.Show();
        }

        void OnEnable()
        {
            m_searchText = "";
            clear();
        }

        private void clear()
        {
            m_monos = null;
            m_functions.Clear();
            m_displayFunctions.Clear();
            m_lastGo = null;
            m_displayMethods.Clear();
            m_paramValues.Clear();
        }

        void OnDisable()
        {
            clear();
        }

        private void initGameObject()
        {
            clear();
            m_lastGo = Selection.activeGameObject;
            if (m_lastGo != null)
            {
                m_monos = m_lastGo.GetComponents<MonoBehaviour>();
                m_monoNames = new string[m_monos.Length];
                for (int i = 0; i < m_monoNames.Length; ++i)
                {
                    m_monoNames[i] = m_monos[i].GetType().ToString();
                }
                m_selectMono = 0;
                findAllFunctions();
            }
        }

        private bool supportParameterType(System.Type t)
        {
            if (t.IsEnum)
            {
                return true;
            }
            if (t == typeof(int))
            {
                return true;
            }
            if (t == typeof(float))
            {
                return true;
            }
            if (t == typeof(double))
            {
                return true;
            }
            if (t == typeof(string))
            {
                return true;
            }
            if (t == typeof(bool))
            {
                return true;
            }
            if (t == typeof(Vector2))
            {
                return true;
            }
            if (t == typeof(Vector3))
            {
                return true;
            }
            if (t == typeof(Vector4))
            {
                return true;
            }
            if (t == typeof(Rect))
            {
                return true;
            }
            return false;
        }

        private bool isList(System.Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);
        }

        private bool checkParam(ParameterInfo p)
        {
            System.Type t = p.ParameterType;
            if (t.IsArray)
            {
                return supportParameterType(t.GetElementType());
            }
            if (isList(t))
            {
                return supportParameterType(t.GetGenericArguments()[0]);
            }
            return supportParameterType(t);
        }

        private object getObjectValue<T>(object obj)
        {
            T ret = default;
            if (obj != null)
            {
                ret = (T)obj;
            }
            return ret;
        }

        private object drawEnum(System.Type t, object obj, string name)
        {
            System.Array arr = System.Enum.GetValues(t);
            System.Enum e = EditorGUILayout.EnumPopup(name, (System.Enum)System.Enum.ToObject(t, getObjectValue<int>(obj)));
            return (int)System.Convert.ChangeType(e, System.Enum.GetUnderlyingType(t));
        }

        private void drawList(System.Type t, WindowDebugParam p, string name)
        {
            if (p.m_list == null)
            {
                p.m_list = new List<object>();
            }
            EditorGUILayout.LabelField(name);
            ++EditorGUI.indentLevel;
            int s = EditorGUILayout.IntField("size", p.m_list.Count);
            p.resizeList(s);
            for (int i = 0; i < s; ++i)
            {
                p.m_list[i] = drawValue(t, p.m_list[i], i.ToString());
            }
            --EditorGUI.indentLevel;
        }

        private object drawValue(System.Type t, object obj, string name)
        {
            if (t.IsEnum)
            {
                return drawEnum(t, obj, name);
            }
            if (t == typeof(int))
            {
                return EditorGUILayout.IntField(name, (int)(getObjectValue<int>(obj)));
            }
            else if (t == typeof(float))
            {
                return EditorGUILayout.FloatField(name, (float)getObjectValue<float>(obj));
            }
            else if (t == typeof(double))
            {
                return EditorGUILayout.DoubleField(name, (double)getObjectValue<double>(obj));
            }
            else if (t == typeof(string))
            {
                return EditorGUILayout.TextField(name, getObjectValue<string>(obj).ToString());
            }
            else if (t == typeof(bool))
            {
                return EditorGUILayout.Toggle(name, (bool)getObjectValue<bool>(obj));
            }
            else if (t == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(name, (Vector2)getObjectValue<Vector2>(obj));
            }
            else if (t == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(name, (Vector3)getObjectValue<Vector3>(obj));
            }
            else if (t == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(name, (Vector4)getObjectValue<Vector4>(obj));
            }
            else if (t == typeof(Rect))
            {
                return EditorGUILayout.RectField(name, (Rect)getObjectValue<Rect>(obj));
            }
            return null;
        }

        private void drawParam(ParameterInfo p, int i)
        {
            System.Type t = p.ParameterType;
            if (t.IsArray)
            {
                drawList(t.GetElementType(), m_paramValues[i], p.Name);
            }
            else if (isList(t))
            {
                drawList(t.GetGenericArguments()[0], m_paramValues[i], p.Name);
            }
            else
            {
                m_paramValues[i].m_value = drawValue(t, m_paramValues[i].m_value, p.Name);
            }
        }

        private bool checkMethod(MethodInfo m)
        {
            ParameterInfo[] ps = m.GetParameters();
            if (ps.Length > 0)
            {
                for (int i = 0; i < ps.Length; ++i)
                {
                    if (!checkParam(ps[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void findAllFunctions()
        {
            if (m_lastGo == null)
            {
                return;
            }
            m_selectFunction = 0;
            m_functions.Clear();
            if (m_selectMono < m_monos.Length)
            {
                MonoBehaviour m = m_monos[m_selectMono];
                System.Type t = m.GetType();
                BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
                if (!m_includeInherit)
                {
                    flag |= BindingFlags.DeclaredOnly;
                }
                if (m_includePrivate)
                {
                    flag |= BindingFlags.NonPublic;
                }
                MethodInfo[] infos = t.GetMethods(flag);
                for (int i = 0; i < infos.Length; ++i)
                {
                    if (checkMethod(infos[i]))
                    {
                        m_functions.Add(infos[i]);
                    }
                }
            }
        }

        private bool filterMethodName(MethodInfo m)
        {
            if (string.IsNullOrEmpty(m_searchText))
            {
                return true;
            }
            return m.Name.Contains(m_searchText);
        }

        public void OnGUI()
        {
            string s = EditorGUILayout.TextField("Search", m_searchText);
            if (s != m_searchText)
            {
                m_searchText = s;
                m_selectFunction = 0;
            }

            if (Selection.activeGameObject != m_lastGo)
            {
                initGameObject();
            }
            if (m_lastGo == null)
            {
                return;
            }
            bool change = false;
            bool inherit = EditorGUILayout.Toggle("Include Inheritance", m_includeInherit);
            if (inherit != m_includeInherit)
            {
                change = true;
                m_includeInherit = inherit;
            }
            bool includePrivate = EditorGUILayout.Toggle("Include private", m_includePrivate);
            if (includePrivate != m_includePrivate)
            {
                change = true;
                m_includePrivate = includePrivate;
            }
            if (change)
            {
                findAllFunctions();
            }
            int index = EditorGUILayout.Popup("Script", m_selectMono, m_monoNames);
            if (index != m_selectMono)
            {
                m_selectMono = index;
                findAllFunctions();
            }
            m_displayMethods.Clear();
            m_displayFunctions.Clear();
            for (int i = 0; i < m_functions.Count; ++i)
            {
                if (filterMethodName(m_functions[i]))
                {
                    m_displayMethods.Add(m_functions[i]);
                    m_displayFunctions.Add(m_functions[i].Name);
                }
            }
            if (m_selectFunction >= m_displayMethods.Count)
            {
                m_selectFunction = 0;
            }
            m_selectFunction = EditorGUILayout.Popup("Method", m_selectFunction, m_displayFunctions.ToArray());
            if (m_selectFunction < m_displayMethods.Count)
            {
                EditorGUILayout.LabelField("Parameters");
                ++EditorGUI.indentLevel;
                ParameterInfo[] ps = m_displayMethods[m_selectFunction].GetParameters();
                while (m_paramValues.Count < ps.Length)
                {
                    m_paramValues.Add(new WindowDebugParam());
                }
                for (int i = 0; i < ps.Length; ++i)
                {
                    drawParam(ps[i], i);
                }
                --EditorGUI.indentLevel;
                if (GUILayout.Button("execute"))
                {
                    object[] os = new object[ps.Length];
                    for (int i = 0; i < os.Length; ++i)
                    {
                        System.Type pt = ps[i].ParameterType;
                        if (pt.IsArray)
                        {
                            var arr = System.Array.CreateInstance(pt.GetElementType(), m_paramValues[i].m_list.Count);
                            System.Array.Copy(arr, m_paramValues[i].m_list.ToArray(), m_paramValues[i].m_list.Count);
                            os[i] = arr;
                        }
                        else if (isList(pt))
                        {
                            IList l = (IList)System.Activator.CreateInstance(pt);
                            for (int j = 0; j < m_paramValues[i].m_list.Count; ++j)
                            {
                                l.Add(m_paramValues[i].m_list[j]);
                            }
                            os[i] = l;
                        }
                        else
                        {
                            os[i] = m_paramValues[i].m_value;
                        }
                    }
                    object o = m_displayMethods[m_selectFunction].Invoke(m_monos[m_selectMono], os);
                    if (o != null)
                    {
                        Debug.Log("Debug Window Return: " + o.ToString());
                    }
                }
            }
        }
    }
}
