using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace LGFW
{
    public class JsonSerializer
    {
        public static bool isList(System.Type t)
        {
            return t.IsGenericType && typeof(IList).IsAssignableFrom(t);
        }

        public static bool isDictionary(System.Type t)
        {
            return t.IsGenericType && typeof(IDictionary).IsAssignableFrom(t);
        }

        public static bool isSet(System.Type t)
        {
            if (!t.IsGenericType)
            {
                return false;
            }
            System.Type[] ts = t.GetInterfaces();
            for (int i = 0; i < ts.Length; ++i)
            {
                if (ts[i].Name.Contains("ISet"))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<object> arrayToList(object o)
        {
            List<object> l = new List<object>();
            if (o == null)
            {
                return l;
            }
            System.Array arr = (System.Array)o;
            for (int i = 0; i < arr.Length; ++i)
            {
                l.Add(arr.GetValue(i));
            }
            return l;
        }

        public static System.Array listToArray(List<object> l, System.Type arrType)
        {
            System.Array arr = System.Array.CreateInstance(arrType.GetElementType(), l.Count);
            for (int i = 0; i < l.Count; ++i)
            {
                arr.SetValue(l[i], i);
            }
            return arr;
        }

        public static List<object> ilistToList(object o)
        {
            List<object> l = new List<object>();
            IList il = (IList)o;
            for (int i = 0; i < il.Count; ++i)
            {
                l.Add(il[i]);
            }
            return l;
        }

        public static IList listToIlist(List<object> l, System.Type listType)
        {
            IList il = (IList)System.Activator.CreateInstance(listType);
            for (int i = 0; i < l.Count; ++i)
            {
                il.Add(l[i]);
            }
            return il;
        }

        public static List<object> isetToList(object o)
        {
            List<object> l = new List<object>();
            MethodInfo m = o.GetType().GetMethod("GetEnumerator");
            IEnumerator e = (IEnumerator)m.Invoke(o, null);
            e.Reset();
            while (e.MoveNext())
            {
                l.Add(e.Current);
            }
            return l;
        }

        public static object listToISet(List<object> l, System.Type setType)
        {
            object o = System.Activator.CreateInstance(setType);
            MethodInfo m = setType.GetMethod("Add");
            object[] param = new object[1];
            for (int i = 0; i < l.Count; ++i)
            {
                param[0] = l[i];
                m.Invoke(o, param);
            }
            return o;
        }

        public static Dictionary<object, object> idictionaryToDict(object o)
        {
            IDictionary id = (IDictionary)o;
            Dictionary<object, object> dict = new Dictionary<object, object>();
            foreach (object k in id.Keys)
            {
                dict[k] = id[k];
            }
            return dict;
        }

        private static object stringToObject(System.Type t, string s)
        {
            if (t.IsEnum)
            {
                string[] ns = System.Enum.GetNames(t);
                int index = 0;
                for (int i = 0; i < ns.Length; ++i)
                {
                    if (s == ns[i])
                    {
                        index = i;
                        break;
                    }
                }
                return System.Enum.GetValues(t).GetValue(index);
            }
            if (t == typeof(string))
            {
                return s;
            }
            System.Type[] param = new System.Type[1];
            param[0] = typeof(string);
            MethodInfo m = t.GetMethod("Parse", param);
            object[] ps = new object[1];
            ps[0] = s;
            return m.Invoke(null, ps);
        }

        public static object dictToIDictionary(Dictionary<string, object> dict, System.Type dictType, bool enumAsNumber)
        {
            IDictionary ret = (IDictionary)System.Activator.CreateInstance(dictType);
            var ts = dictType.GetGenericArguments();
            if (ts[0].IsPrimitive || ts[0].IsEnum || ts[0] == typeof(string))
            {
                foreach (string k in dict.Keys)
                {
                    object key = stringToObject(ts[0], k);
                    object v = desrializeObject(ts[1], dict[k], enumAsNumber);
                    ret[key] = v;
                }
            }
            return ret;
        }

        public static List<FieldInfo> getAllField(System.Type t, bool filter, bool onlySerialized, List<string> names)
        {
            List<FieldInfo> ret = new List<FieldInfo>();
            names.Clear();
            FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; ++i)
            {
                if (filter)
                {
                    System.Attribute attr = fields[i].GetCustomAttribute(typeof(NoJson), true);
                    if (attr != null)
                    {
                        continue;
                    }
                    if (onlySerialized)
                    {
                        if (fields[i].IsPublic)
                        {
                            attr = fields[i].GetCustomAttribute(typeof(System.NonSerializedAttribute), true);
                            if (attr != null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            attr = fields[i].GetCustomAttribute(typeof(SerializeField), true);
                            if (attr == null)
                            {
                                continue;
                            }
                        }
                    }
                    ret.Add(fields[i]);
                    attr = fields[i].GetCustomAttribute(typeof(JsonName), true);
                    if (attr != null)
                    {
                        JsonName n = (JsonName)attr;
                        names.Add(n.Name);
                    }
                    else
                    {
                        names.Add(fields[i].Name);
                    }
                }
                else
                {
                    ret.Add(fields[i]);
                    names.Add(fields[i].Name);
                }
            }
            return ret;
        }

        /// <summary>
        /// Serializes a object to a json object
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="enumAsNumber">If enum is an int</param>
        /// <param name="onlySerialized">If only serialize the serializable fields</param>
        /// <returns>The json object</returns>
        public static object serializeObject(object o, bool enumAsNumber, bool onlySerialized)
        {
            if (o == null)
            {
                return "";
            }
            System.Type t = o.GetType();
            if (t.IsPrimitive || t == typeof(string))
            {
                return o;
            }
            if (t.IsEnum)
            {
                if (enumAsNumber)
                {
                    System.Type ut = t.GetEnumUnderlyingType();
                    return System.Convert.ChangeType(o, ut);
                }
                else
                {
                    return o.ToString();
                }
            }
            if (isList(t))
            {
                return ilistToList(o);
            }
            if (t.IsArray)
            {
                return arrayToList(o);
            }
            if (isDictionary(t))
            {
                Dictionary<object, object> d = idictionaryToDict(o);
                Dictionary<string, object> ret = new Dictionary<string, object>();
                foreach (object k in d.Keys)
                {
                    ret[k.ToString()] = d[k];
                }
                return ret;
            }
            if (isSet(t))
            {
                return isetToList(o);
            }
            List<string> names = new List<string>();
            List<FieldInfo> fields = getAllField(t, true, onlySerialized, names);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            for (int i = 0; i < fields.Count; ++i)
            {
                object value = fields[i].GetValue(o);
                dict[names[i]] = serializeObject(value, enumAsNumber, onlySerialized);
            }
            return dict;
        }

        private static bool isNull(object value)
        {
            if (value == null)
            {
                return true;
            }
            if (value.GetType() == typeof(string))
            {
                if (string.IsNullOrEmpty(value.ToString()))
                {
                    return true;
                }
            }
            return false;
        }

        public static object intToEnum(int v, System.Type eType)
        {
            var arr = System.Enum.GetValues(eType);
            var ut = eType.GetEnumUnderlyingType();
            for (int i = 0; i < arr.Length; ++i)
            {
                int e = (int)System.Convert.ChangeType(arr.GetValue(i), ut);
                if (e == v)
                {
                    return arr.GetValue(i);
                }
            }
            return arr.GetValue(0);
        }

        /// <summary>
        /// Deserialize a object from a json object
        /// </summary>
        /// <param name="t">The type of the object</param>
        /// <param name="value">The json objcet</param>
        /// <param name="enumAsNumber">If enum is an int</param>
        /// <param name="obj">The object to hold the deserialized value, if null, a new object will be created</param>
        /// <returns>The deserialized object</returns>
        public static object desrializeObject(System.Type t, object value, bool enumAsNumber, object obj = null)
        {
            if (t.IsPrimitive || t == typeof(string))
            {
                return value;
            }
            if (t.IsEnum)
            {
                int e = 0;
                if (enumAsNumber)
                {
                    e = (int)value;
                    return intToEnum(e, t);
                }
                return stringToObject(t, value.ToString());
            }
            if (isNull(value))
            {
                return null;
            }
            if (isList(t))
            {
                return listToIlist((List<object>)value, t);
            }
            if (t.IsArray)
            {
                return listToArray((List<object>)value, t);
            }
            if (isDictionary(t))
            {
                return dictToIDictionary((Dictionary<string, object>)value, t, enumAsNumber);
            }
            if (isSet(t))
            {
                listToISet((List<object>)value, t);
            }
            object ret = obj;
            if (ret == null)
            {
                ret = System.Activator.CreateInstance(t);
            }
            Dictionary<string, object> dict = (Dictionary<string, object>)value;
            List<string> names = new List<string>();
            List<FieldInfo> fields = getAllField(t, true, true, names);
            Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();
            for (int i = 0; i < names.Count; ++i)
            {
                fieldMap[names[i]] = fields[i];
            }
            foreach (string k in dict.Keys)
            {
                FieldInfo f = null;
                if (fieldMap.TryGetValue(k, out f))
                {
                    object v = desrializeObject(f.FieldType, dict[k], enumAsNumber);
                    f.SetValue(ret, v);
                }
            }
            return ret;
        }
    }
}