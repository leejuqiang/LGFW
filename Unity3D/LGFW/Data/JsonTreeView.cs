using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Displays a json structure as a tree of GameObject
    /// </summary>
    public class JsonTreeView : MonoBehaviour
    {
        /// <summary>
        /// The text file for the json
        /// </summary>
        public TextAsset m_jsonText;
        public JsonTreeKey[] m_keys;
        private object m_obj;

        private static Dictionary<string, JsonTreeKey> m_keyDict;

        /// <summary>
        /// Shows the json in the json text file
        /// </summary>
        public void showText()
        {
            if (m_jsonText == null)
            {
                return;
            }
            showJson(m_jsonText.text);
        }

        /// <summary>
        /// Shows the json
        /// </summary>
        /// <param name="js">The json string</param>
        public void showJson(string js)
        {
            m_obj = Json.decode(js);
            m_keyDict = new Dictionary<string, JsonTreeKey>();
            for (int i = 0; i < m_keys.Length; ++i)
            {
                m_keyDict.Add(m_keys[i].m_key, m_keys[i]);
            }
            Transform t = this.transform;
            for (int i = 0; i < t.childCount; ++i)
            {
                if (Application.isPlaying)
                {
                    Destroy(t.GetChild(i).gameObject);
                }
                else
                {
                    DestroyImmediate(t.GetChild(i).gameObject);
                }
            }
            newGameObject("json", m_obj, t);
            m_keyDict = null;
        }

        private static GameObject createGameObject(string name, Transform parent, bool hasDisplay)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent;
            if (hasDisplay)
            {
                go.AddComponent<JsonTreeDisplay>();
            }
            return go;
        }

        private static void newGameObject(string name, object o, Transform parent)
        {
            JsonTreeKey k = null;
            m_keyDict.TryGetValue(name, out k);
            if (k != null && k.m_hide)
            {
                return;
            }
            if (o is Dictionary<string, object>)
            {
                newGOAsObject((Dictionary<string, object>)o, name, parent);
            }
            else if (o is List<object>)
            {
                newGOAsArray((List<object>)o, name, parent);
            }
            else
            {
                if (o is string)
                {
                    name += " (string)";
                }
                else if (o is int)
                {
                    name += " (int)";
                }
                else if (o is float)
                {
                    name += " (float)";
                }
                else if (o is double)
                {
                    name += " (double)";
                }
                if (k != null && k.m_displayValueInName)
                {
                    name += ":" + o.ToString();
                    createGameObject(name, parent, false);
                }
                else
                {
                    GameObject go = createGameObject(name, parent, true);
                    go.GetComponent<JsonTreeDisplay>().m_value = o.ToString();
                }
            }
        }

        public static void newGOAsObject(Dictionary<string, object> dict, string name, Transform parent)
        {
            name += " (object)";
            GameObject go = createGameObject(name, parent, false);
            Transform t = go.transform;
            foreach (string k in dict.Keys)
            {
                newGameObject(k, dict[k], t);
            }
        }

        public static void newGOAsArray(List<object> l, string name, Transform parent)
        {
            name += " (array)";
            GameObject go = createGameObject(name, parent, false);
            Transform t = go.transform;
            for (int i = 0; i < l.Count; ++i)
            {
                newGameObject(i.ToString(), l[i], t);
            }
        }
    }
}
