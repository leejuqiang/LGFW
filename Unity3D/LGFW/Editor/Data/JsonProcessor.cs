using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System.Text;

namespace LGFW
{
    public class JsonProcessor
    {

        public const string K_TYPE = "type";
        public const string K_DS_TYPE = "dsType";
        public const string K_DATA = "data";

        public static void tryToProcess(string path, EditorConfig ec)
        {
            JsonDataConfig c = ec.getDataConfig(path);
            if (c != null)
            {
                string js = System.IO.File.ReadAllText(path);
                startProcess(js, c);
            }
        }

        private static void startProcess(string json, JsonDataConfig c)
        {
            Dictionary<string, object> dict = (Dictionary<string, object>)MiniJSON.Json.Deserialize(json);
            processDataList(dict, c);
        }

        private static object[] getAttribute(FieldInfo fInfo)
        {
            return fInfo.GetCustomAttributes(true);
        }

        private static object checkAttribute(System.Type attributeType, object[] attributes)
        {
            for (int i = 0; i < attributes.Length; ++i)
            {
                if (attributes[i].GetType() == attributeType)
                {
                    return attributes[i];
                }
            }
            return null;
        }

        private static float[] stringToVector(string s, int len)
        {
            float[] ret = new float[len];
            string[] arr = s.Split(',');
            for (int i = 0; i < ret.Length; ++i)
            {
                if (i >= arr.Length)
                {
                    ret[i] = 0;
                }
                else
                {
                    ret[i] = System.Convert.ToSingle(arr[i]);
                }
            }
            return ret;
        }

        private static int[] stringToIntVector(string s, int len)
        {
            int[] ret = new int[len];
            string[] arr = s.Split(',');
            for (int i = 0; i < ret.Length; ++i)
            {
                if (i >= arr.Length)
                {
                    ret[i] = 0;
                }
                else
                {
                    ret[i] = System.Convert.ToInt32(arr[i]);
                }
            }
            return ret;
        }

        private static object stringToValue(string s, System.Type t)
        {
            if (t == typeof(int))
            {
                return System.Convert.ToInt32(s);
            }
            if (t == typeof(float))
            {
                return System.Convert.ToSingle(s);
            }
            if (t == typeof(double))
            {
                return System.Convert.ToDouble(s);
            }
            if (t == typeof(bool))
            {
                return !string.IsNullOrEmpty(s);
            }
            if (t == typeof(string))
            {
                return s;
            }
            if (t == typeof(Vector2))
            {
                float[] v = stringToVector(s, 2);
                return new Vector2(v[0], v[1]);
            }
            if (t == typeof(Vector3))
            {
                float[] v = stringToVector(s, 3);
                return new Vector3(v[0], v[1], v[2]);
            }
            if (t == typeof(Vector4))
            {
                float[] v = stringToVector(s, 4);
                return new Vector4(v[0], v[1], v[2], v[3]);
            }
            if (t == typeof(Vector2Int))
            {
                int[] v = stringToIntVector(s, 2);
                return new Vector2Int(v[0], v[1]);
            }
            if (t == typeof(Vector3Int))
            {
                int[] v = stringToIntVector(s, 3);
                return new Vector3Int(v[0], v[1], v[2]);
            }
            if (t == typeof(Quaternion))
            {
                float[] v = stringToVector(s, 4);
                return new Quaternion(v[0], v[1], v[2], v[3]);
            }
            return null;
        }

        private static string getPlainString(List<object> l, DataCombineText ct)
        {
            if (l.Count <= 0)
            {
                return "";
            }
            if (ct == null)
            {
                return l[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < l.Count; ++i)
            {
                sb.Append(l[i].ToString());
                if (i != l.Count - 1)
                {
                    sb.Append(ct.CombineText);
                }
            }
            return sb.ToString();
        }

        private static string[] getStringArray(List<object> l, DataSplit s)
        {
            if (l.Count <= 0)
            {
                return new string[0];
            }
            if (s == null)
            {
                string[] ret = new string[l.Count];
                for (int i = 0; i < l.Count; ++i)
                {
                    ret[i] = l[i].ToString();
                }
                return ret;
            }
            string str = l[0].ToString();
            return str.Split(s.SplitChar);
        }

        private static object getValueFromText(List<object> l, System.Type fieldType, object[] attributes)
        {
            if (fieldType.IsGenericType)
            {
                System.Type gt = fieldType.GetGenericArguments()[0];
                System.Type lt = typeof(List<>).MakeGenericType(gt);
                IList list = (IList)System.Activator.CreateInstance(lt);
                object o = checkAttribute(typeof(DataSplit), attributes);
                DataSplit s = o == null ? null : (DataSplit)o;
                string[] arr = getStringArray(l, s);
                for (int i = 0; i < arr.Length; ++i)
                {
                    list.Add(stringToValue(arr[i], gt));
                }
                return list;
            }
            if (fieldType.IsArray)
            {
                System.Type et = fieldType.GetElementType();
                object o = checkAttribute(typeof(DataSplit), attributes);
                DataSplit s = o == null ? null : (DataSplit)o;
                string[] arr = getStringArray(l, s);
                System.Array ret = System.Array.CreateInstance(et, arr.Length);
                for (int i = 0; i < arr.Length; ++i)
                {
                    ret.SetValue(stringToValue(arr[i], et), i);
                }
                return ret;
            }
            object ao = checkAttribute(typeof(DataCombineText), attributes);
            DataCombineText ct = ao == null ? null : (DataCombineText)ao;
            string str = getPlainString(l, ct);
            return stringToValue(str, fieldType);
        }

        private static object processOneData(Dictionary<string, object> d, System.Type t, string prefix, JsonDataConfig c)
        {
            object ret = System.Activator.CreateInstance(t);
            foreach (string k in d.Keys)
            {
                string name = c.getAlias(k);
                FieldInfo fi = t.GetField(prefix + name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi == null)
                {
                    continue;
                }
                List<object> l = (List<object>)d[k];
                object[] attributes = getAttribute(fi);
                object v = getValueFromText(l, fi.FieldType, attributes);
                fi.SetValue(ret, v);
            }
            return ret;
        }

        private static void processDataList(Dictionary<string, object> dict, JsonDataConfig c)
        {
            object o = null;
            if (!dict.TryGetValue(K_TYPE, out o))
            {
                return;
            }
            string tName = o.ToString();
            System.Type t = LEditorKits.findTypeByName(tName);
            if (t == null)
            {
                return;
            }
            if (!dict.TryGetValue(K_DS_TYPE, out o))
            {
                return;
            }
            string dsName = o.ToString();
            System.Type dsT = LEditorKits.findTypeByName(dsName);
            if (dsT == null)
            {
                return;
            }
            if (!dict.TryGetValue(K_DATA, out o))
            {
                return;
            }
            Object dsO = AssetDatabase.LoadAssetAtPath(c.m_assetPath, dsT);
            if (dsO == null)
            {
                dsO = ScriptableObject.CreateInstance(dsT);
                AssetDatabase.CreateAsset(dsO, c.m_assetPath);
                AssetDatabase.Refresh();
            }
            EditorConfig ec = EditorConfig.Instance;
            List<object> data = (List<object>)o;
            FieldInfo listInfo = dsT.GetField("m_dataList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IList dataList = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(t));
            for (int i = 0; i < data.Count; ++i)
            {
                object d = processOneData((Dictionary<string, object>)data[i], t, ec.m_dataFieldPrefix, c);
                dataList.Add(d);
            }
            listInfo.SetValue(dsO, dataList);
            EditorUtility.SetDirty(dsO);
        }
    }
}
