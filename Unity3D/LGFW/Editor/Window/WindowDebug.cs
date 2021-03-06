﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace LGFW
{
    public class WindowDebug : EditorWindow
    {

        private static WindowDebug m_instance;

        private string m_searchText;
        private GameObject m_lastGo;
        private bool m_includeInherit;
        private bool m_includePrivate;
        private ObjectInspector m_drawer;

        private MonoBehaviour[] m_monos;
        private string[] m_monoNames;
        private List<MethodInfo> m_functions = new List<MethodInfo>();
        private int m_selectMono;
        private int m_selectFunction;
        private List<string> m_displayFunctions = new List<string>();
        private List<MethodInfo> m_displayMethods = new List<MethodInfo>();
        private List<object> m_paramValues = new List<object>();

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
            m_drawer = new ObjectInspector();
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
            m_drawer = null;
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
                m_functions.AddRange(infos);
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
            int select = EditorGUILayout.Popup("Method", m_selectFunction, m_displayFunctions.ToArray());
            if (select != m_selectFunction)
            {
                m_selectFunction = select;
                for (int i = 0; i < m_paramValues.Count; ++i)
                {
                    m_paramValues[i] = null;
                }
            }
            if (m_selectFunction < m_displayMethods.Count)
            {
                EditorGUILayout.LabelField("Parameters");
                ++EditorGUI.indentLevel;
                ParameterInfo[] ps = m_displayMethods[m_selectFunction].GetParameters();
                while (m_paramValues.Count < ps.Length)
                {
                    m_paramValues.Add(null);
                }
                for (int i = 0; i < ps.Length; ++i)
                {
                    if (ps[i].ParameterType.IsByRef)
                    {
                        m_paramValues[i] = System.Activator.CreateInstance(ps[i].ParameterType.GetElementType());
                    }
                    else
                    {
                        m_paramValues[i] = m_drawer.drawObject(m_paramValues[i], ps[i].ParameterType, ps[i].Name, "");
                    }
                }
                --EditorGUI.indentLevel;
                if (GUILayout.Button("execute"))
                {
                    object[] os = new object[ps.Length];
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        os[i] = m_paramValues[i];
                    }
                    object o = m_displayMethods[m_selectFunction].Invoke(m_monos[m_selectMono], os);
                    if (o != null)
                    {
                        Debug.Log("Debug Window Return: " + o.ToString());
                    }
                    for (int i = 0; i < ps.Length; ++i)
                    {
                        if (ps[i].ParameterType.IsByRef)
                        {
                            Debug.Log("parameter " + ps[i].Name + " value: " + os[i].ToString());
                        }
                    }
                }
            }
        }
    }
}
