using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Reflection;

namespace LGFW
{
    public class ExcelHead
    {
        public int m_start;
        public int m_end;
        public string m_name;
    }

    public class ExcelSheet
    {
        public List<ExcelHead> m_heads;

        public string m_typeName;
        public string m_dsTypeName;
        public List<List<object>>[] m_values;

        public void initValues()
        {
            m_values = new List<List<object>>[m_heads.Count];
            for (int i = 0; i < m_values.Length; ++i)
            {
                m_values[i] = new List<List<object>>();
            }
        }

        public void addRow(List<ExcelHead> l)
        {
            int cellIndex = m_heads[0].m_start;
            int len = m_heads[m_heads.Count - 1].m_end;
            List<object> sl = new List<object>();
            int headIndex = 0;
            int dataIndex = 0;
            m_values[headIndex].Add(sl);
            while (cellIndex <= len)
            {
                if (dataIndex < l.Count)
                {
                    sl.Add(l[dataIndex].m_name);
                }
                else
                {
                    sl.Add("");
                }
                ++cellIndex;
                if (cellIndex > m_heads[headIndex].m_end)
                {
                    ++headIndex;
                    if (headIndex >= m_heads.Count)
                    {
                        return;
                    }
                    sl = new List<object>();
                    m_values[headIndex].Add(sl);
                }
                if (dataIndex < l.Count && cellIndex > l[dataIndex].m_end)
                {
                    ++dataIndex;
                }
            }
        }

        public void getDataTypeName()
        {
            System.Type dsT = LEditorKits.findTypeByName(m_dsTypeName);
            if (dsT == null)
            {
                return;
            }
            FieldInfo fi = dsT.GetField("m_datas", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (fi == null)
            {
                return;
            }
            m_typeName = fi.FieldType.GetGenericArguments()[0].Name;
        }

        public Dictionary<string, object> toJson()
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict[JsonProcessor.K_TYPE] = m_typeName;
            dict[JsonProcessor.K_DS_TYPE] = m_dsTypeName;
            List<object> datas = new List<object>();
            dict[JsonProcessor.K_DATA] = datas;
            int len = m_values[0].Count;
            for (int i = 0; i < len; ++i)
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                for (int h = 0; h < m_heads.Count; ++h)
                {
                    d[m_heads[h].m_name] = m_values[h][i];
                }
                datas.Add(d);
            }
            return dict;
        }
    }

    public class ExcelToJson
    {

        private XmlNamespaceManager m_namespaceManager;
        private string m_dirPath;

        private string getDirPath(string path)
        {
            int index = path.LastIndexOf('/');
            if (index >= 0)
            {
                return path.Substring(0, index + 1);
            }
            return "";
        }

        public ExcelToJson(string path, EditorConfig ec)
        {
            m_dirPath = getDirPath(path);
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            m_namespaceManager = new XmlNamespaceManager(doc.NameTable);
            m_namespaceManager.AddNamespace("ns", "urn:schemas-microsoft-com:office:spreadsheet");

            XmlNodeList sheets = doc.SelectNodes("//ns:Worksheet", m_namespaceManager);
            foreach (XmlNode node in sheets)
            {
                processSheet(node, ec);
            }
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
            if (ret[0].m_start != 1)
            {
                ExcelHead h = new ExcelHead();
                h.m_name = "";
                h.m_start = 1;
                h.m_end = ret[0].m_start - 1;
                ret.Insert(0, h);
            }
            for (int i = 1; i < ret.Count; ++i)
            {
                if (ret[i].m_start != ret[i - 1].m_end + 1)
                {
                    ExcelHead h = new ExcelHead();
                    h.m_name = "";
                    h.m_start = ret[i - 1].m_end + 1;
                    h.m_end = ret[i].m_start - 1;
                    ret.Insert(i, h);
                }
            }
            return ret;
        }

        private void processSheet(XmlNode sheet, EditorConfig ec)
        {
            XmlElement e = (XmlElement)sheet;
            ExcelSheet es = new ExcelSheet();
            es.m_dsTypeName = e.GetAttribute("ss:Name");
            es.getDataTypeName();
            if (string.IsNullOrEmpty(es.m_typeName))
            {
                return;
            }
            XmlNode table = findNodesInChildren(sheet, "Table")[0];
            List<XmlNode> rows = findNodesInChildren(table, "Row");
            if (rows.Count <= 0)
            {
                return;
            }
            XmlNode r = rows[0];
            List<XmlNode> cells = findNodesInChildren(r, "Cell");
            if (cells.Count <= 0)
            {
                return;
            }
            es.m_heads = readRow(cells);
            es.initValues();

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
            Dictionary<string, object> dict = es.toJson();
            string js = MiniJSON.Json.Serialize(dict);
            string outPath = m_dirPath + es.m_dsTypeName + ".json";
            System.IO.File.WriteAllText(outPath, js);
            JsonProcessor.tryToProcess(outPath, ec);
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
