using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class DataPostProcessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void compliedExcelScripts()
        {
            EditorConfig ec = EditorConfig.Instance;
            Dictionary<string, object> dict = EditorConfig.getTempConfig();
            object o = null;
            if (dict.TryGetValue("excel", out o))
            {
                List<object> li = (List<object>)o;
                for (int i = 0; i < li.Count; ++i)
                {
                    string path = li[i].ToString();
                    try
                    {
                        ExcelConfig c = ec.getDataConfig(path);
                        new ExcelData(path).start(c, true);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("exception when processing " + path);
                        Debug.LogException(e);
                    }
                }
                dict.Remove("excel");
            }
            EditorConfig.saveTempConfig(dict);
        }

        public static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromPath)
        {
            EditorConfig ec = EditorConfig.Instance;
            List<object> l = new List<object>();
            List<object> jsonL = new List<object>();
            for (int i = 0; i < imported.Length; ++i)
            {
                ExcelConfig c = ec.getDataConfig(imported[i]);
                if (c != null)
                {
                    if (c.m_isLocalizedText)
                    {
                        List<ExcelSheet> sheets = new ExcelData(imported[i]).processSheets();
                        for (int j = 0; j < sheets.Count; ++j)
                        {
                            ExcelParser.parseLocalizedText(sheets[j], c);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (new ExcelData(imported[i]).start(c, false))
                            {
                                l.Add(imported[i]);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("exception when processing " + imported[i]);
                            Debug.LogException(e);
                        }
                    }
                }
            }
            if (l.Count > 0 || jsonL.Count > 0)
            {
                AssetDatabase.Refresh();
                Dictionary<string, object> dict = EditorConfig.getTempConfig();
                if (l.Count > 0)
                {
                    dict["excel"] = l;
                }
                EditorConfig.saveTempConfig(dict);
            }
        }
    }
}
