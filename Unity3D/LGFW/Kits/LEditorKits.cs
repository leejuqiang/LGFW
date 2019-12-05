using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

#if UNITY_EDITOR
namespace LGFW
{
    public class LEditorKits
    {

        public static System.Type findTypeByName(string name)
        {
            foreach (Assembly a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type t = a.GetType(name);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        public static Transform getChildTransformByName(Transform t, string name)
        {
            for (int i = 0, len = t.childCount; i < len; ++i)
            {
                Transform tt = t.GetChild(i);
                if (tt.name == name)
                {
                    return tt;
                }
            }
            return null;
        }

        public static string openSaveToFolderPanel(string title)
        {
            string path = EditorUtility.OpenFolderPanel(title, "Assets", "");
            path = getPathStartWithAssets(path);
            return path;
        }

        public static string getPathStartWithAssets(string path)
        {
            int index = path.IndexOf("Assets");
            if (index < 0)
            {
                return path;
            }
            return path.Substring(index);
        }

        public static string getAssetDirectory(Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return System.IO.Path.GetDirectoryName(path);
        }

        public static bool isGameObjectSelected(GameObject go)
        {
            for (int i = 0; i < UnityEditor.Selection.gameObjects.Length; ++i)
            {
                if (go == UnityEditor.Selection.gameObjects[i])
                {
                    return true;
                }
            }
            return false;
        }

        public static Object createOrLoadAsset(string path, System.Type t)
        {
            Object o = AssetDatabase.LoadAssetAtPath(path, t);
            if (o == null)
            {
                o = ScriptableObject.CreateInstance(t);
                AssetDatabase.CreateAsset(o, path);
                AssetDatabase.Refresh();
            }
            return o;
        }

        public static string createAssetAtSelectedPath(Object obj, string title, string name)
        {
            string path = openSaveToFolderPanel(title);
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }
            AssetDatabase.CreateAsset(obj, path + "/" + name);
            return path;
        }

        public static GameObject newGameObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        public static T[] getComponentsFromSelectedObjects<T>() where T : Component
        {
            List<T> l = new List<T>();
            foreach (GameObject go in Selection.gameObjects)
            {
                l.AddRange(go.GetComponents<T>());
            }
            return l.ToArray();
        }

        public static T[] addComponentToSelectedObjects<T>(bool notAddIfExist) where T : Component
        {
            List<T> l = new List<T>();
            foreach (GameObject go in Selection.gameObjects)
            {
                if (notAddIfExist && go.GetComponent<T>() != null)
                {
                    continue;
                }
                l.Add(go.AddComponent<T>());
            }
            return l.ToArray();
        }

        public static Ray sceneViewClickToRay()
        {
            Vector3 v = Event.current.mousePosition;
            Camera c = SceneView.currentDrawingSceneView.camera;
            v.y = c.pixelHeight - v.y;
            Ray r = c.ScreenPointToRay(v);
            return r;
        }
    }
}
#endif
