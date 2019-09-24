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
        private static bool generateScript(ExcelSheet es, ExcelConfig c, string prefix)
        {
            if (es.m_heads == null || es.m_heads.Count <= 0)
            {
                return false;
            }
            string p = System.IO.Path.Combine(c.m_scriptPath, es.m_typeName + ".cs");
            StringBuilder sb = new StringBuilder();
            ExcelHead idHead = es.m_heads[0];
            string idType = idHead.getTypeText();
            sb.Append("using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing LGFW;\n\n");
            sb.Append("[System.Serializable]\n");
            sb.Append("public class " + es.m_typeName + " : IData<" + idType + ">\n{\n");
            for (int i = 0; i < es.m_heads.Count; ++i)
            {
                ExcelHead h = es.m_heads[i];
                if (h.m_type == ExcelHeadType.noType || h.m_type == ExcelHeadType.emptyType)
                {
                    continue;
                }
                if (h.m_structure == ExcelHeadStructure.combine)
                {
                    sb.Append("\t[DataCombineText");
                    if (!string.IsNullOrEmpty(h.m_split))
                    {
                        sb.Append("(\"" + h.m_split + "\")");
                    }
                    sb.Append("]\n");
                }
                else if (h.m_structure == ExcelHeadStructure.list || h.m_structure == ExcelHeadStructure.array)
                {
                    if (!string.IsNullOrEmpty(h.m_split))
                    {
                        sb.Append("\t[DataSplit");
                        sb.Append("(\"" + h.m_split + "\")");
                        sb.Append("]\n");
                    }
                }
                sb.Append("\tpublic ");
                sb.Append(h.getFullTypeText());
                sb.Append(" " + prefix + h.m_name);
                sb.Append(";\n");
            }
            sb.Append("\n\tpublic " + idType + " getID()\n");
            sb.Append("\t{\n");
            sb.Append("\t\treturn " + prefix + idHead.m_name);
            sb.Append(";\n\t}\n}");
            LGFWKit.writeTextToFile(p, sb.ToString());
            string dsName = getDataSetTypeName(es.m_typeName, c);
            p = System.IO.Path.Combine(c.m_scriptPath, dsName + ".cs");
            if (!LGFWKit.fileExists(p))
            {
                sb.Remove(0, sb.Length);
                sb.Append("using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing LGFW;\n\n");
                sb.Append("[System.Serializable]\n");
                sb.Append("public class " + dsName + " : DataSetBase<" + es.m_typeName + ", " + idType + ">\n{\n}");
                LGFWKit.writeTextToFile(p, sb.ToString());
            }
            return true;
        }

        private static void callCheck(System.Type t, object o)
        {
            MethodInfo mi = t.GetMethod("checkData");
            if (mi != null)
            {
                mi.Invoke(o, null);
            }
        }

        private static string getDataSetTypeName(string type, ExcelConfig c)
        {
            return type + (string.IsNullOrEmpty(c.m_customDBExtension) ? "DB" : c.m_customDBExtension);
        }


        public static void parseLocalizedText(ExcelSheet es, ExcelConfig c)
        {
            if (es.m_heads.Count <= 1)
            {
                return;
            }
            System.Type t = typeof(LocalizedTextData);
            System.Type dsT = typeof(LocalizedText);
            FieldInfo idField = t.GetField("m_id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            object[] attributes = getAttribute(idField);
            FieldInfo textField = t.GetField("m_text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            ExcelHead idHead = es.m_heads[0];
            for (int i = 1; i < es.m_heads.Count; ++i)
            {
                ExcelHead h = es.m_heads[i];
                string assPath = System.IO.Path.Combine(c.m_assetPath, h.m_name + ".asset");
                Object dsO = LEditorKits.createOrLoadAsset(assPath, dsT);
                FieldInfo listInfo = dsT.GetField("m_dataList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                IList dataList = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(t));
                for (int j = 0; j < es.m_rows.Count; ++j)
                {
                    ExcelRow r = es.m_rows[j];
                    object o = System.Activator.CreateInstance(t);
                    object v = getValueFromText(r.m_values[idHead], idHead, typeof(string), attributes);
                    idField.SetValue(o, v);
                    v = getValueFromText(r.m_values[h], h, typeof(string), attributes);
                    textField.SetValue(o, v);
                    dataList.Add(o);
                }
                listInfo.SetValue(dsO, dataList);
                callCheck(dsT, dsO);
                EditorUtility.SetDirty(dsO);
            }
        }

        public static bool startProcess(ExcelSheet es, ExcelConfig c, bool skipScript)
        {
            bool ret = false;
            if (!skipScript && !string.IsNullOrEmpty(c.m_scriptPath))
            {
                if (generateScript(es, c, EditorConfig.Instance.m_dataFieldPrefix))
                {
                    ret = true;
                }
            }
            System.Type t = LEditorKits.findTypeByName(es.m_typeName);
            if (t == null)
            {
                return ret;
            }
            string dsName = getDataSetTypeName(es.m_typeName, c);
            System.Type dsT = LEditorKits.findTypeByName(dsName);
            if (dsT == null)
            {
                return ret;
            }
            Object dsO = LEditorKits.createOrLoadAsset(c.m_assetPath, dsT);
            EditorConfig ec = EditorConfig.Instance;
            FieldInfo listInfo = dsT.GetField("m_dataList", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            IList dataList = (IList)System.Activator.CreateInstance(typeof(List<>).MakeGenericType(t));
            for (int i = 0; i < es.m_rows.Count; ++i)
            {
                object d = processOneData(es.m_rows[i], t, ec.m_dataFieldPrefix, c);
                dataList.Add(d);
            }
            listInfo.SetValue(dsO, dataList);
            callCheck(dsT, dsO);
            EditorUtility.SetDirty(dsO);
            return ret;
        }

        public static object[] getAttribute(FieldInfo fInfo)
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
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                return System.Convert.ToInt32(s);
            }
            if (t == typeof(float))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
                return System.Convert.ToSingle(s);
            }
            if (t == typeof(double))
            {
                if (string.IsNullOrEmpty(s))
                {
                    return null;
                }
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
            return null;
        }

        private static string getPlainString(List<string> l, string combine)
        {
            if (l.Count <= 0)
            {
                return "";
            }
            if (combine == null)
            {
                return l[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < l.Count; ++i)
            {
                sb.Append(l[i]);
                if (i != l.Count - 1)
                {
                    sb.Append(combine);
                }
            }
            return sb.ToString();
        }

        private static List<string> getStringList(List<string> l, string split)
        {
            List<string> ret = new List<string>();
            if (l.Count <= 0)
            {
                return ret;
            }
            if (string.IsNullOrEmpty(split))
            {
                return l;
            }

            for (int i = 0; i < l.Count; ++i)
            {
                string[] arr = l[i].Split(split.ToCharArray());
                ret.AddRange(arr);
            }
            return ret;
        }

        private static string getSplitString(ExcelHead head, object[] attributes)
        {
            string split = head.m_split;
            if (head.m_structure != ExcelHeadStructure.array && head.m_structure != ExcelHeadStructure.list)
            {
                object att = checkAttribute(typeof(DataSplit), attributes);
                if (att != null)
                {
                    DataSplit ds = (DataSplit)att;
                    split = ds.SplitString;
                }
                else
                {
                    split = null;
                }
            }
            return split;
        }

        private static string getCombineString(ExcelHead head, object[] attributes)
        {
            string split = head.m_split;
            if (head.m_structure != ExcelHeadStructure.combine)
            {
                object att = checkAttribute(typeof(DataCombineText), attributes);
                if (att != null)
                {
                    DataCombineText dc = (DataCombineText)att;
                    split = dc.CombineText;
                }
                else
                {
                    split = null;
                }
            }
            return split;
        }

        private static object getValueFromText(List<string> l, ExcelHead head, System.Type fieldType, object[] attributes)
        {

            if (fieldType.IsGenericType)
            {
                System.Type gt = fieldType.GetGenericArguments()[0];
                System.Type lt = typeof(List<>).MakeGenericType(gt);
                IList list = (IList)System.Activator.CreateInstance(lt);
                List<string> s = getStringList(l, getSplitString(head, attributes));
                for (int i = 0; i < s.Count; ++i)
                {
                    object o = stringToValue(s[i], gt);
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
                List<string> arr = getStringList(l, getSplitString(head, attributes));
                List<object> temp = new List<object>();
                for (int i = 0; i < arr.Count; ++i)
                {
                    object o = stringToValue(arr[i], et);
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
            string str = getPlainString(l, getCombineString(head, attributes));
            return stringToValue(str, fieldType);
        }

        private static object processOneData(ExcelRow d, System.Type t, string prefix, ExcelConfig c)
        {
            object ret = System.Activator.CreateInstance(t);
            foreach (ExcelHead h in d.m_values.Keys)
            {
                string name = c.getAlias(h.m_name);
                FieldInfo fi = t.GetField(prefix + name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi == null)
                {
                    continue;
                }
                object[] attributes = getAttribute(fi);
                object v = getValueFromText(d.m_values[h], h, fi.FieldType, attributes);
                fi.SetValue(ret, v);
            }
            return ret;
        }
    }
}
