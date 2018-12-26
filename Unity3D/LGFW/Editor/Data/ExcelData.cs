using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Reflection;

namespace LGFW
{

    public enum ExcelHeadType
    {
        noType,
        intType,
        floatType,
        doubleType,
        stringType,
        vector2Type,
        vector3Type,
        vector4Type,
        vector2IntType,
        vector3IntType,
        vector4IntType,
        quaternionType,
        emptyType,
    }

    public enum ExcelHeadStructure
    {
        single,
        array,
        list,
        combine,
    }

    public class ExcelHead
    {
        public int m_start;
        public int m_end;
        public string m_name;
        public ExcelHeadType m_type = ExcelHeadType.noType;
        public ExcelHeadStructure m_structure;
        public string m_split;

        private bool pairStructure(string str)
        {
            char ch = str[str.Length - 1];
            if (str[0] == '[')
            {
                return ch == ']';
            }
            if (str[0] == '<')
            {
                return ch == '>';
            }
            return ch == ')';
        }

        private void setStructure(string str)
        {
            if (!pairStructure(str))
            {
                m_structure = ExcelHeadStructure.single;
                m_split = null;
            }
            else
            {
                char ch = str[str.Length - 1];
                if (ch == ']')
                {
                    m_structure = ExcelHeadStructure.array;
                }
                else if (ch == ')')
                {
                    m_structure = ExcelHeadStructure.combine;
                }
                else
                {
                    m_structure = ExcelHeadStructure.list;
                }
                m_split = str.Substring(1, str.Length - 2);
            }
        }

        public string getFullTypeText()
        {
            string t = getTypeText();
            if (string.IsNullOrEmpty(t))
            {
                return "";
            }
            if (m_structure == ExcelHeadStructure.array)
            {
                return t + "[]";
            }
            if (m_structure == ExcelHeadStructure.list)
            {
                return "List<" + t + ">";
            }
            return t;
        }

        public string getTypeText()
        {
            switch (m_type)
            {
                case ExcelHeadType.intType:
                    return "int";
                case ExcelHeadType.floatType:
                    return "float";
                case ExcelHeadType.doubleType:
                    return "double";
                case ExcelHeadType.stringType:
                    return "string";
                case ExcelHeadType.vector2Type:
                    return "Vector2";
                case ExcelHeadType.vector3Type:
                    return "Vector3";
                case ExcelHeadType.vector4Type:
                    return "Vector4";
                case ExcelHeadType.vector2IntType:
                    return "Vector2Int";
                case ExcelHeadType.vector3IntType:
                    return "Vector3Int";
                case ExcelHeadType.vector4IntType:
                    return "Vector4Int";
                case ExcelHeadType.quaternionType:
                    return "Quaternion";
                default:
                    return "";
            }
        }

        private void setType(string type)
        {
            switch (type)
            {
                case "i":
                    m_type = ExcelHeadType.intType;
                    break;
                case "f":
                    m_type = ExcelHeadType.floatType;
                    break;
                case "d":
                    m_type = ExcelHeadType.doubleType;
                    break;
                case "s":
                    m_type = ExcelHeadType.stringType;
                    break;
                case "v2":
                    m_type = ExcelHeadType.vector2Type;
                    break;
                case "v3":
                    m_type = ExcelHeadType.vector3Type;
                    break;
                case "v4":
                    m_type = ExcelHeadType.vector4Type;
                    break;
                case "vi2":
                    m_type = ExcelHeadType.vector2IntType;
                    break;
                case "vi3":
                    m_type = ExcelHeadType.vector3IntType;
                    break;
                case "vi4":
                    m_type = ExcelHeadType.vector4IntType;
                    break;
                case "q":
                    m_type = ExcelHeadType.quaternionType;
                    break;
                case "":
                    m_type = ExcelHeadType.emptyType;
                    break;
                default:
                    m_type = ExcelHeadType.noType;
                    break;
            }
        }

        public void initAsHead()
        {
            int index = m_name.LastIndexOf("_");
            if (index > 0)
            {
                string type = m_name.Substring(index + 1);
                int sIndex = type.IndexOfAny(new char[] { '[', '<', '(' });
                if (sIndex > 0)
                {
                    setStructure(type.Substring(sIndex));
                    type = type.Substring(0, sIndex);
                }
                else
                {
                    m_structure = ExcelHeadStructure.single;
                    m_split = null;
                }
                setType(type);
            }
            else
            {
                m_type = ExcelHeadType.noType;
                m_structure = ExcelHeadStructure.single;
                m_split = null;
            }
            if (m_type != ExcelHeadType.noType)
            {
                m_name = m_name.Substring(0, index);
            }
        }
    }

    public class ExcelRow
    {
        public Dictionary<ExcelHead, List<string>> m_values;
        public Dictionary<ExcelHead, List<ExcelHead>> m_tempValues;
        public ExcelRow(List<ExcelHead> heads)
        {
            m_tempValues = new Dictionary<ExcelHead, List<ExcelHead>>();
            for (int i = 0; i < heads.Count; ++i)
            {
                m_tempValues.Add(heads[i], new List<ExcelHead>());
            }
        }

        public void finish()
        {
            m_values = new Dictionary<ExcelHead, List<string>>();
            foreach (ExcelHead h in m_tempValues.Keys)
            {
                List<string> l = new List<string>();
                m_values.Add(h, l);
                List<ExcelHead> dl = m_tempValues[h];
                int last = h.m_start;
                for (int i = 0; i < dl.Count; ++i)
                {
                    while (last < dl[i].m_start)
                    {
                        l.Add("");
                        ++last;
                    }
                    l.Add(dl[i].m_name);
                    last = dl[i].m_end + 1;
                }
            }
            m_tempValues.Clear();
        }
    }

    public class ExcelSheet
    {
        public List<ExcelHead> m_heads;
        public string m_typeName;
        public List<ExcelRow> m_rows = new List<ExcelRow>();

        public void addRow(List<ExcelHead> l)
        {
            ExcelRow row = new ExcelRow(m_heads);
            int headIndex = 0;
            for (int i = 0; i < l.Count && headIndex < m_heads.Count;)
            {
                if (l[i].m_end < m_heads[headIndex].m_start)
                {
                    ++i;
                }
                else if (l[i].m_start > m_heads[headIndex].m_end)
                {
                    ++headIndex;
                }
                else
                {
                    row.m_tempValues[m_heads[headIndex]].Add(l[i]);
                    if (l[i].m_end < m_heads[headIndex].m_end)
                    {
                        ++i;
                    }
                    else if (l[i].m_end > m_heads[headIndex].m_end)
                    {
                        ++headIndex;
                    }
                    else
                    {
                        ++i;
                        ++headIndex;
                    }
                }
            }
            row.finish();
            m_rows.Add(row);
        }

        public Dictionary<string, object> toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            // dict[JsonProcessor.K_TYPE] = m_typeName;
            // dict[JsonProcessor.K_DS_TYPE] = m_dsTypeName;
            // List<object> dataList = new List<object>();
            // dict[JsonProcessor.K_DATA] = dataList;
            // int len = m_values[0].Count;
            // for (int i = 0; i < len; ++i)
            // {
            //     Dictionary<string, object> d = new Dictionary<string, object>();
            //     for (int h = 0; h < m_heads.Count; ++h)
            //     {
            //         d[m_heads[h].m_name] = m_values[h][i];
            //     }
            //     dataList.Add(d);
            // }
            return dict;
        }
    }

    public class ExcelData
    {

        private XmlNamespaceManager m_namespaceManager;
        private XmlNodeList m_sheets;

        public ExcelData(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            m_namespaceManager = new XmlNamespaceManager(doc.NameTable);
            m_namespaceManager.AddNamespace("ns", "urn:schemas-microsoft-com:office:spreadsheet");
            m_sheets = doc.SelectNodes("//ns:Worksheet", m_namespaceManager);
        }

        public bool start(ExcelConfig ec, bool skipScript)
        {
            bool ret = false;
            foreach (XmlNode node in m_sheets)
            {
                ExcelSheet es = processSheet(node);
                if (ExcelParser.startProcess(es, ec, skipScript))
                {
                    ret = true;
                }
            }
            return ret;
        }

        public List<ExcelSheet> processSheets()
        {
            List<ExcelSheet> l = new List<ExcelSheet>();
            foreach (XmlNode node in m_sheets)
            {
                ExcelSheet es = processSheet(node);
                l.Add(es);
            }
            return l;
        }

        private XmlElement findElementInChildren(XmlNode node, string name)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == name)
                {
                    return (XmlElement)n;
                }
            }
            return null;
        }

        private List<ExcelHead> readRow(List<XmlNode> cells)
        {
            List<ExcelHead> ret = new List<ExcelHead>();
            int cellIndex = 1;
            foreach (XmlNode n in cells)
            {
                ExcelHead head = new ExcelHead();
                XmlElement cell = (XmlElement)n;
                if (cell.HasAttribute("ss:Index"))
                {
                    cellIndex = System.Convert.ToInt32(cell.GetAttribute("ss:Index"));
                }
                head.m_start = cellIndex;
                head.m_end = head.m_start;
                if (cell.HasAttribute("ss:MergeAcross"))
                {
                    int w = System.Convert.ToInt32(cell.GetAttribute("ss:MergeAcross"));
                    cellIndex += w;
                    head.m_end = head.m_start + w;
                }
                XmlElement data = findElementInChildren(n, "Data");
                if (data == null)
                {
                    data = findElementInChildren(n, "ss:Data");
                }
                if (data != null)
                {
                    string s = data.InnerText;
                    if (string.IsNullOrEmpty(s))
                    {
                        XmlElement font = findElementInChildren(data, "Font");
                        if (font != null)
                        {
                            s = font.InnerText;
                        }
                    }
                    head.m_name = s;
                    ret.Add(head);
                }
                ++cellIndex;
            }
            return ret;
        }

        private ExcelSheet processSheet(XmlNode sheet)
        {
            XmlElement e = (XmlElement)sheet;
            ExcelSheet es = new ExcelSheet();
            es.m_typeName = e.GetAttribute("ss:Name");
            XmlNode table = findNodesInChildren(sheet, "Table")[0];
            List<XmlNode> rows = findNodesInChildren(table, "Row");
            if (rows.Count <= 0)
            {
                return es;
            }
            XmlNode r = rows[0];
            List<XmlNode> cells = findNodesInChildren(r, "Cell");
            if (cells.Count <= 0)
            {
                return es;
            }
            es.m_heads = readRow(cells);
            for (int i = 0; i < es.m_heads.Count; ++i)
            {
                es.m_heads[i].initAsHead();
            }

            for (int i = 1; i < rows.Count; ++i)
            {
                r = rows[i];
                cells = findNodesInChildren(r, "Cell");
                if (isEmptyRow(cells))
                {
                    continue;
                }
                List<ExcelHead> l = readRow(cells);
                es.addRow(l);
            }
            return es;
        }

        private bool isEmptyRow(List<XmlNode> cells)
        {
            if (cells.Count <= 0)
            {
                return true;
            }
            for (int i = 0; i < cells.Count; ++i)
            {
                if (!isCellEmpty(cells[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private bool isCellEmpty(XmlNode cell)
        {
            XmlElement data = findElementInChildren(cell, "Data");
            if (data == null)
            {
                data = findElementInChildren(cell, "ss:Data");
            }
            return data == null;
        }

        private List<XmlNode> findNodesInChildren(XmlNode node, string name)
        {
            List<XmlNode> list = new List<XmlNode>();
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == name)
                {
                    list.Add(n);
                }
            }
            return list;
        }
    }
}
