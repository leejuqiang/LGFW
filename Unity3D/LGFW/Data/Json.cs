using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Reflection;

namespace LGFW
{
    /// <summary>
    /// A class to decode and encode json.
    /// To encode json, the only supported types are Dictionary<string, object>, List<object>, int, short, long, float, double, string, bool.
    /// </summary>
    public class Json
    {
        private static void logError(string js, int index)
        {
            if (index < 0 || index >= js.Length)
            {
                Debug.Log("json string is not closed");
                return;
            }
            int len = js.Length - index;
            if (len > 10)
            {
                len = 10;
            }
            string sub = js.Substring(index, len);
            ++index;
            Debug.Log("json format error, index " + index + ", " + sub);
        }

        private static bool printValue(object o, StringBuilder sb, bool format)
        {
            if (o is string)
            {
                sb.Append("\"");
                string s = o.ToString();
                for (int i = 0; i < s.Length; ++i)
                {
                    if (s[i] == '"' || s[i] == '\\')
                    {
                        sb.Append('\\');
                    }
                    sb.Append(s[i]);
                }
                sb.Append("\"");
            }
            else if (o is bool)
            {
                sb.Append((bool)o ? "True" : "False");
            }
            else
            {
                if (format)
                {
                    if (o is int || o is short)
                    {
                        sb.Append("i");
                    }
                    else if (o is float)
                    {
                        sb.Append("f");
                    }
                    else if (o is double)
                    {
                        sb.Append("d");
                    }
                    else if (o is long)
                    {
                        sb.Append("l");
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (!(o is int || o is short || o is float || o is double || o is long))
                    {
                        return false;
                    }
                }
                sb.Append(o.ToString());
            }
            return true;
        }

        private static bool printObject(object o, StringBuilder sb, bool format)
        {
            if (o is Dictionary<string, object>)
            {
                sb.Append("{");
                Dictionary<string, object> dict = (Dictionary<string, object>)o;
                int count = dict.Count;
                foreach (string k in dict.Keys)
                {
                    printObject(k, sb, format);
                    sb.Append(":");
                    if (!printObject(dict[k], sb, format))
                    {
                        return false;
                    }
                    --count;
                    if (count > 0)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("}");
                return true;
            }
            else if (o is List<object>)
            {
                List<object> l = (List<object>)o;
                sb.Append("[");
                int last = l.Count - 1;
                for (int i = 0; i < l.Count; ++i)
                {
                    if (!printObject(l[i], sb, format))
                    {
                        return false;
                    }
                    if (i < last)
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append("]");
                return true;
            }
            else
            {
                return printValue(o, sb, format);
            }
        }

        /// <summary>
        /// Encodes an object to a json string, make sure all types are supported
        /// </summary>
        /// <param name="o">The object</param>
        /// <param name="format">If true, the numerical type will be added a prefix, so it will be decode as the same type, otherwise, all numerical type will be decode as double</param>
        /// <returns>The json string, or empty string is there is an error</returns>
        public static string encode(object o, bool format)
        {
            StringBuilder sb = new StringBuilder();
            if (printObject(o, sb, format))
            {
                return sb.ToString();
            }
            Debug.LogError("Not supported type");
            return "";
        }

        /// <summary>
        /// Decodes a json string to an object
        /// </summary>
        /// <param name="js">The json string</param>
        /// <returns>The object. It will be a dictionary or a list. Or null if there is an error</returns>
        public static object decode(string js)
        {
            int index = 0;
            StringBuilder sb = new StringBuilder();
            object o = parseValue(js, ref index, sb);
            if (o == null)
            {
                logError(js, index);
            }
            return o;
        }

        private static int nextChar(string js, int index)
        {
            for (; index < js.Length; ++index)
            {
                if (!char.IsWhiteSpace(js[index]))
                {
                    return index;
                }
            }
            return -1;
        }

        private static void transChar(char ch, StringBuilder sb)
        {
            switch (ch)
            {
                case 'n':
                    sb.Append('\n');
                    break;
                case 't':
                    sb.Append('\t');
                    break;
                case 'r':
                    sb.Append('\r');
                    break;
                default:
                    sb.Append(ch);
                    break;
            }
        }

        private static string parseString(string js, ref int index, StringBuilder sb)
        {
            if (js[index] != '\"')
            {
                return "";
            }
            sb.Length = 0;
            bool isTrans = false;
            for (++index; index < js.Length; ++index)
            {
                char ch = js[index];
                if (isTrans)
                {
                    isTrans = false;
                    transChar(ch, sb);
                }
                else
                {
                    if (ch == '\\')
                    {
                        isTrans = true;
                    }
                    else if (ch == '\"')
                    {
                        ++index;
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }
            return "";
        }

        private static List<object> parseList(string js, ref int index, StringBuilder sb)
        {
            List<object> l = new List<object>();
            index = nextChar(js, index + 1);
            while (true)
            {
                if (index < 0)
                {
                    return null;
                }
                if (js[index] != ']')
                {
                    object o = parseValue(js, ref index, sb);
                    if (o == null)
                    {
                        return null;
                    }
                    l.Add(o);
                    index = nextChar(js, index);
                    if (index < 0)
                    {
                        return null;
                    }
                }
                if (js[index] == ']')
                {
                    break;
                }
                if (js[index] != ',')
                {
                    return null;
                }
                index = nextChar(js, index + 1);
            }
            ++index;
            return l;
        }

        private static object parseBool(string js, ref int index)
        {
            int start = index;
            for (++index; index < js.Length; ++index)
            {
                char ch = js[index];
                if (ch >= 'a' && ch <= 'z')
                {
                }
                else
                {
                    string v = js.Substring(start, index - start);
                    if (v == "True")
                    {
                        return true;
                    }
                    if (v == "False")
                    {
                        return false;
                    }
                    return null;
                }
            }
            return null;
        }

        private static object parseNumber(string js, ref int index, bool hasFlag)
        {
            string ret = null;
            int start = hasFlag ? (index + 1) : index;
            for (++index; index < js.Length; ++index)
            {
                char ch = js[index];
                if ((ch >= '0' && ch <= '9') || ch == '.' || ch == '-')
                {
                }
                else
                {
                    ret = js.Substring(start, index - start);
                    break;
                }
            }
            if (string.IsNullOrEmpty(ret))
            {
                return null;
            }
            if (hasFlag)
            {
                switch (js[start - 1])
                {
                    case 'i':
                        {
                            int d = 0;
                            if (int.TryParse(ret, out d))
                            {
                                return d;
                            }
                            else
                            {
                                Debug.LogError("can't convert " + ret + " to int");
                                return null;
                            }
                        }
                    case 'f':
                        {
                            float d = 0;
                            if (float.TryParse(ret, out d))
                            {
                                return d;
                            }
                            else
                            {
                                Debug.LogError("can't convert " + ret + " to float");
                                return null;
                            }
                        }
                    case 'd':
                        {
                            double d = 0;
                            if (double.TryParse(ret, out d))
                            {
                                return d;
                            }
                            else
                            {
                                Debug.LogError("can't convert " + ret + " to double");
                                return null;
                            }
                        }
                    case 'l':
                        {
                            long d = 0;
                            if (long.TryParse(ret, out d))
                            {
                                return d;
                            }
                            else
                            {
                                Debug.LogError("can't convert " + ret + " to long");
                                return null;
                            }
                        }
                    default:
                        return null;
                }
            }
            else
            {
                double d = 0;
                if (double.TryParse(ret, out d))
                {
                    return d;
                }
                else
                {
                    Debug.LogError("can't convert " + ret + " to double");
                    return null;
                }
            }
        }

        private static object parseValue(string js, ref int index, StringBuilder sb)
        {
            char ch = js[index];
            switch (ch)
            {
                case '{':
                    return parseDict(js, ref index, sb);
                case '[':
                    return parseList(js, ref index, sb);
                case '"':
                    return parseString(js, ref index, sb);
                case 'i':
                case 'l':
                case 'f':
                case 'd':
                    return parseNumber(js, ref index, true);
                case 'T':
                case 'F':
                    return parseBool(js, ref index);
                default:
                    if ((ch <= '9' && ch >= '0') || ch == '-')
                    {
                        return parseNumber(js, ref index, false);
                    }
                    return null;
            }
        }

        private static Dictionary<string, object> parseDict(string js, ref int index, StringBuilder sb)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            index = nextChar(js, index + 1);
            while (true)
            {
                if (index < 0)
                {
                    return null;
                }
                if (js[index] == '"')
                {
                    string key = parseString(js, ref index, sb);
                    if (string.IsNullOrEmpty(key))
                    {
                        return null;
                    }
                    index = nextChar(js, index);
                    if (index < 0)
                    {
                        return null;
                    }
                    if (js[index] != ':')
                    {
                        return null;
                    }
                    index = nextChar(js, index + 1);
                    if (index < 0)
                    {
                        return null;
                    }
                    object o = parseValue(js, ref index, sb);
                    if (o == null)
                    {
                        return null;
                    }
                    dict[key] = o;
                    index = nextChar(js, index);
                    if (index < 0)
                    {
                        return null;
                    }
                }
                if (js[index] == '}')
                {
                    break;
                }
                if (js[index] != ',')
                {
                    return null;
                }
                index = nextChar(js, index + 1);
            }
            ++index;
            return dict;
        }
    }
}
