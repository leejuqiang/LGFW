using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class WindowCreateAsset : EditorWindow
    {

        private static WindowCreateAsset m_instance;
        private string m_className;
        private string m_assetName;

        [MenuItem("LGFW/Asset/Create Scriptable Asset", false, (int)'s')]
        public static void showWindow()
        {
            if (m_instance == null)
            {
                m_instance = EditorWindow.GetWindow<WindowCreateAsset>(true, "Create Asset");
            }
            m_instance.Show();
        }

        public void OnGUI()
        {
            m_className = EditorGUILayout.TextField("Class Name", m_className);
            m_assetName = EditorGUILayout.TextField("Asset Name", m_assetName);
            if (GUILayout.Button("create"))
            {
                ScriptableObject obj = ScriptableObject.CreateInstance(m_className);
                if (obj == null)
                {
                    Debug.Log("can't create asset with class " + m_className);
                    return;
                }
                string path = LEditorKits.openSaveToFolderPanel("Select a path");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }
                if (string.IsNullOrEmpty(m_assetName))
                {
                    m_assetName = m_className;
                }
                AssetDatabase.CreateAsset(obj, path + "/" + m_assetName + ".asset");
                AssetDatabase.Refresh();
            }
        }
    }
}