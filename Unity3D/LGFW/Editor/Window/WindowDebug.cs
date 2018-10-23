using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace LGFW
{
    public class WindowDebugParam
    {
        public int m_int;
        public float m_float;
        public double m_double;
        public string m_string;
        public bool m_bool;
        public Vector2 m_vector2;
        public Vector3 m_vector3;
        public Vector4 m_vector4;
        public Rect m_rect;

        public object getValue(ParameterInfo p)
        {
            System.Type t = p.ParameterType;
            if (t == typeof(int))
            {
                return m_int;
            }
            if (t == typeof(float))
            {
                return m_float;
            }
            if (t == typeof(double))
            {
                return m_double;
            }
            if (t == typeof(string))
            {
                return m_string;
            }
            if (t == typeof(bool))
            {
                return m_bool;
            }
            if (t == typeof(Vector2))
            {
                return m_vector2;
            }
            if (t == typeof(Vector3))
            {
                return m_vector3;
            }
            if (t == typeof(Vector4))
            {
                return m_vector4;
            }
            if (t == typeof(Rect))
            {
                return m_rect;
            }
            return null;
        }
    }

    public class WindowDebug : EditorWindow
    {

        private static WindowDebug m_instance;

        private string m_searchText;
        private GameObject m_lastGo;
        private bool m_includeInherit;

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

        private bool checkParam(ParameterInfo p)
        {
            System.Type t = p.ParameterType;
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

        private void drawParam(ParameterInfo p, int i)
        {
            System.Type t = p.ParameterType;
            if (t == typeof(int))
            {
                m_paramValues[i].m_int = EditorGUILayout.IntField(p.Name, m_paramValues[i].m_int);
            }
            else if (t == typeof(float))
            {
                m_paramValues[i].m_float = EditorGUILayout.FloatField(p.Name, m_paramValues[i].m_float);
            }
            else if (t == typeof(double))
            {
                m_paramValues[i].m_double = EditorGUILayout.DoubleField(p.Name, m_paramValues[i].m_double);
            }
            else if (t == typeof(string))
            {
                m_paramValues[i].m_string = EditorGUILayout.TextField(p.Name, m_paramValues[i].m_string);
            }
            else if (t == typeof(bool))
            {
                m_paramValues[i].m_bool = EditorGUILayout.Toggle(p.Name, m_paramValues[i].m_bool);
            }
            else if (t == typeof(Vector2))
            {
                m_paramValues[i].m_vector2 = EditorGUILayout.Vector2Field(p.Name, m_paramValues[i].m_vector2);
            }
            else if (t == typeof(Vector3))
            {
                m_paramValues[i].m_vector3 = EditorGUILayout.Vector3Field(p.Name, m_paramValues[i].m_vector3);
            }
            else if (t == typeof(Vector4))
            {
                m_paramValues[i].m_vector4 = EditorGUILayout.Vector4Field(p.Name, m_paramValues[i].m_vector4);
            }
            else if (t == typeof(Rect))
            {
                m_paramValues[i].m_rect = EditorGUILayout.RectField(p.Name, m_paramValues[i].m_rect);
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
                BindingFlags flag = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                if (!m_includeInherit)
                {
                    flag |= BindingFlags.DeclaredOnly;
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
            bool inherit = EditorGUILayout.Toggle("Include Inheritance", m_includeInherit);
            if (inherit != m_includeInherit)
            {
                m_includeInherit = inherit;
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
                        os[i] = m_paramValues[i].getValue(ps[i]);
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
