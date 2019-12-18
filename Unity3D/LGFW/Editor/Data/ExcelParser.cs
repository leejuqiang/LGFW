using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEditor;
using System.Text;

namespace LGFW
{
    public class ExcelParser
    {
        private static void callCheck(System.Type t, object o)
        {
            MethodInfo mi = t.GetMethod("checkData");
            if (mi != null)
            {
                mi.Invoke(o, null);
            }
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

        private static object stringToEnum(System.Type et, string s)
        {
            int index = 0;
            if (!int.TryParse(s, out index))
            {
                string[] ns = System.Enum.GetNames(et);
                for (int i = 0; i < ns.Length; ++i)
                {
                    if (s == ns[i])
                    {
                        index = i;
                        break;
                    }
                }
            }
            return System.Enum.GetValues(et).GetValue(index);
        }

        private static object stringToPrimitive(System.Type t, string s)
        {
            System.Type[] param = new System.Type[1];
            param[0] = typeof(string);
            MethodInfo m = t.GetMethod("Parse", param);
            object[] ps = new object[1];
            ps[0] = s;
            return m.Invoke(null, ps);
        }

        private static object stringToValue(string s, System.Type t, Dictionary<string, Dictionary<string, object>> refMap)
        {
            if (t.IsSubclassOf(typeof(ScriptableObject)))
            {
                return AssetDatabase.LoadAssetAtPath(s, t);
            }
            if (t == typeof(string))
            {
                return s;
            }
            if (t == typeof(bool))
            {
                bool ret = false;
                if (bool.TryParse(s, out ret))
                {
                    return ret;
                }
                if (string.IsNullOrEmpty(s))
                {
                    return false;
                }
                if (s == "0")
                {
                    return false;
                }
                return true;
            }
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            if (t.IsEnum)
            {
                return stringToEnum(t, s);
            }
            if (t.IsPrimitive)
            {
                return stringToPrimitive(t, s);
            }
            if (t == typeof(Vector2))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                float[] v = stringToVector(s, 2);
                return new Vector2(v[0], v[1]);
            }
            if (t == typeof(Vector3))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                float[] v = stringToVector(s, 3);
                return new Vector3(v[0], v[1], v[2]);
            }
            if (t == typeof(Vector4))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                float[] v = stringToVector(s, 4);
                return new Vector4(v[0], v[1], v[2], v[3]);
            }
            if (t == typeof(Vector2Int))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                int[] v = stringToIntVector(s, 2);
                return new Vector2Int(v[0], v[1]);
            }
            if (t == typeof(Vector3Int))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                int[] v = stringToIntVector(s, 3);
                return new Vector3Int(v[0], v[1], v[2]);
            }
            if (t == typeof(Quaternion))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                float[] v = stringToVector(s, 4);
                return new Quaternion(v[0], v[1], v[2], v[3]);
            }
            if (t.IsClass)
            {
                Dictionary<string, object> d = null;
                if (!refMap.TryGetValue(t.Name + "." + s, out d))
                {
                    return null;
                }
                return parseOneData(d, t, refMap);
            }
            return null;
        }

        private static object getAttribute(object[] attributes, System.Type attrType)
        {
            for (int i = 0; i < attributes.Length; ++i)
            {
                if (attributes[i].GetType() == attrType)
                {
                    return attributes[i];
                }
            }
            return null;
        }

        private static List<string> generateString(List<object> l, object[] attributes)
        {
            List<string> values = new List<string>();
            object attr = getAttribute(attributes, typeof(DataSplit));
            if (attr != null)
            {
                string s = "";
                for (int i = 0; i < l.Count; ++i)
                {
                    s += l[i];
                }
                string[] arr = s.Split(((DataSplit)attr).SplitString.ToCharArray());
                values.AddRange(arr);
            }
            else
            {
                if (getAttribute(attributes, typeof(DontCombineText)) != null)
                {
                    values.Add(l[0].ToString());
                }
                else
                {
                    attr = getAttribute(attributes, typeof(DataCombineText));
                    if (attr != null)
                    {
                        string s = "";
                        string join = ((DataCombineText)attr).CombineText;
                        for (int i = 0; i < l.Count; ++i)
                        {
                            s += l[i];
                            if (i < l.Count - 1)
                            {
                                s += join;
                            }
                        }
                        values.Add(s);
                    }
                    else
                    {
                        for (int i = 0; i < l.Count; ++i)
                        {
                            values.Add(l[i].ToString());
                        }
                    }
                }
            }
            for (int i = 0; i < values.Count;)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    break;
                }
                values.RemoveAt(i);
            }
            for (int i = values.Count - 1; i >= 0; --i)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    break;
                }
                values.RemoveAt(i);
            }
            if (values.Count <= 0)
            {
                values.Add("");
            }
            return values;
        }

        private static string combineList(List<string> l)
        {
            string ret = "";
            for (int i = 0; i < l.Count; ++i)
            {
                ret += l[i];
            }
            return ret;
        }

        private static object getValueFromText(List<object> l, System.Type fieldType, object[] attributes, Dictionary<string, Dictionary<string, object>> refMap)
        {
            List<string> values = generateString(l, attributes);
            if (fieldType.IsGenericType)
            {
                System.Type gt = fieldType.GetGenericArguments()[0];
                System.Type lt = typeof(List<>).MakeGenericType(gt);
                IList list = (IList)System.Activator.CreateInstance(lt);
                for (int i = 0; i < values.Count; ++i)
                {
                    object o = stringToValue(values[i], gt, refMap);
                    if (o != null)
                    {
                        list.Add(o);
                    }
                }
                return list;
            }
            if (fieldType.IsArray)
            {
                System.Type et = fieldType.GetElementType();
                List<object> temp = new List<object>();
                for (int i = 0; i < values.Count; ++i)
                {
                    object o = stringToValue(values[i], et, refMap);
                    if (o != null)
                    {
                        temp.Add(o);
                    }
                }
                System.Array ret = System.Array.CreateInstance(et, temp.Count);
                for (int i = 0; i < temp.Count; ++i)
                {
                    ret.SetValue(temp[i], i);
                }
                return ret;
            }
            return stringToValue(combineList(values), fieldType, refMap);
        }

        private static object parseOneData(Dictionary<string, object> dict, System.Type t, Dictionary<string, Dictionary<string, object>> refMap)
        {
            object ret = System.Activator.CreateInstance(t);
            foreach (string key in dict.Keys)
            {
                FieldInfo fi = t.GetField("m_" + key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi == null)
                {
                    fi = t.GetField(key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
                if (fi == null)
                {
                    continue;
                }
                object[] attributes = fi.GetCustomAttributes(true);
                if (fi.IsPublic && getAttribute(attributes, typeof(System.NonSerializedAttribute)) != null)
                {
                    continue;
                }
                if (!fi.IsPublic && getAttribute(attributes, typeof(SerializeField)) == null)
                {
                    continue;
                }
                object v = getValueFromText((List<object>)dict[key], fi.FieldType, attributes, refMap);
                fi.SetValue(ret, v);
            }
            return ret;
        }

        private static List<object> parseRows(List<object> data, System.Type dt, Dictionary<string, Dictionary<string, object>> refMap)
        {
            List<object> ret = new List<object>();
            for (int i = 0; i < data.Count; ++i)
            {
                ret.Add(parseOneData((Dictionary<string, object>)data[i], dt, refMap));
            }
            return ret;
        }

        private static string findNameSpace(string className, out string space)
        {
            space = "";
            int last = className.LastIndexOf(".");
            if (last >= 0)
            {
                space = className.Substring(0, last);
                return className.Substring(last + 1);
            }
            return className;
        }

        private static Dictionary<string, Dictionary<string, object>> createReferenceMap(string main, Dictionary<string, object> dict)
        {
            var ret = new Dictionary<string, Dictionary<string, object>>();
            foreach (string k in dict.Keys)
            {
                if (k == main)
                {
                    continue;
                }
                var rows = (List<object>)dict[k];
                for (int i = 0; i < rows.Count; ++i)
                {
                    object o = null;
                    var d = (Dictionary<string, object>)rows[i];
                    if (!d.TryGetValue("id", out o))
                    {
                        continue;
                    }
                    List<object> l = (List<object>)o;
                    string key = k + ".";
                    for (int j = 0; j < l.Count; ++j)
                    {
                        key += l[j];
                    }
                    ret.Add(key, d);
                }
            }
            return ret;
        }
        private static void parseData(Dictionary<string, object> classes, string fileName, string className, string path)
        {
            object o = null;
            if (!classes.TryGetValue(className, out o))
            {
                return;
            }
            var l = (List<object>)o;
            string nameSpace = "";
            className = findNameSpace(className, out nameSpace);
            System.Type dataType = LEditorKits.findTypeByName(className, nameSpace);
            System.Type dbType = LEditorKits.findTypeByName(className + "DB", nameSpace);
            if (dataType == null || dbType == null)
            {
                return;
            }
            FieldInfo dbListInfo = dbType.GetField("m_dataList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (dbListInfo == null)
            {
                return;
            }
            string outPath = System.IO.Path.GetDirectoryName(path) + "/" + fileName + ".asset";
            Object db = AssetDatabase.LoadAssetAtPath(outPath, dbType);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance(dbType);
                if (db == null)
                {
                    return;
                }
                AssetDatabase.CreateAsset(db, outPath);
            }
            var refMap = createReferenceMap(className, classes);
            List<object> rows = parseRows(l, dataType, refMap);
            System.Type lt = typeof(List<>).MakeGenericType(dataType);
            IList dl = (IList)System.Activator.CreateInstance(lt);
            for (int i = 0; i < rows.Count; ++i)
            {
                dl.Add(rows[i]);
            }
            dbListInfo.SetValue(db, dl);
            EditorUtility.SetDirty(db);
        }

        private static LocalizedTextData parseOneLanguageData(Dictionary<string, object> dict, string key, object[] idAttrs, object[] textAttrs)
        {
            object o = null;
            if (!dict.TryGetValue("id", out o))
            {
                return null;
            }
            List<string> values = generateString((List<object>)o, idAttrs);
            LocalizedTextData ret = new LocalizedTextData();
            ret.m_id = combineList(values);
            if (!dict.TryGetValue(key, out o))
            {
                return null;
            }
            values = generateString((List<object>)o, textAttrs);
            ret.m_text = combineList(values);
            return ret;
        }

        private static void parseOneLanguage(List<object> l, string path, string lang)
        {
            string ext = System.IO.Path.GetExtension(path);
            string outPath = path.Substring(0, path.Length - ext.Length) + "_" + lang + ".asset";
            System.Type dataType = typeof(LocalizedTextData);
            System.Type dbType = typeof(LocalizedText);
            FieldInfo dbListInfo = dbType.GetField("m_dataList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            LocalizedText db = AssetDatabase.LoadAssetAtPath<LocalizedText>(outPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<LocalizedText>();
                AssetDatabase.CreateAsset(db, outPath);
            }
            if (db.m_dataList == null)
            {
                db.m_dataList = new List<LocalizedTextData>();
            }
            else
            {
                db.m_dataList.Clear();
            }
            object[] idAttrs = dataType.GetField("m_id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttributes(true);
            object[] textAttrs = dataType.GetField("m_text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetCustomAttributes(true);
            for (int i = 0; i < l.Count; ++i)
            {
                var d = parseOneLanguageData((Dictionary<string, object>)l[i], lang, idAttrs, textAttrs);
                if (d != null)
                {
                    db.m_dataList.Add(d);
                }
            }
            EditorUtility.SetDirty(db);
        }

        private static void parseLocalization(List<object> rows, List<object> header, string path)
        {
            for (int i = 0; i < header.Count; ++i)
            {
                string key = header[i].ToString();
                if (key != "id")
                {
                    parseOneLanguage(rows, path, key);
                }
            }
        }

        public static void parse(Dictionary<string, object> dict, string path)
        {
            object o = null;
            if (!dict.TryGetValue("name", out o))
            {
                return;
            }
            string fileName = o.ToString();
            if (!dict.TryGetValue("main", out o))
            {
                return;
            }
            string mainClass = o.ToString();
            if (!dict.TryGetValue("data", out o))
            {
                return;
            }
            Dictionary<string, object> classes = (Dictionary<string, object>)o;
            if (!classes.TryGetValue(mainClass, out o))
            {
                return;
            }
            List<object> l = (List<object>)o;
            if (!dict.TryGetValue("header", out o))
            {
                return;
            }
            var headers = (Dictionary<string, object>)o;
            if (!headers.TryGetValue(mainClass, out o))
            {
                return;
            }
            var h = (List<object>)o;
            if (mainClass == "Localization")
            {
                parseLocalization(l, h, path);
            }
            else
            {
                parseData(classes, fileName, mainClass, path);
            }
        }
    }
}
