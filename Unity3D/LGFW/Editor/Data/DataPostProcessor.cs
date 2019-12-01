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
            // EditorConfig ec = EditorConfig.Instance;
            // Dictionary<string, object> dict = EditorConfig.getTempConfig();
            // object o = null;
            // if (dict.TryGetValue("excel", out o))
            // {
            //     List<object> li = (List<object>)o;
            //     for (int i = 0; i < li.Count; ++i)
            //     {
            //         string path = li[i].ToString();
            //         try
            //         {
            //             ExcelConfig c = ec.getDataConfig(path);
            //             new ExcelData(path).start(c, true);
            //         }
            //         catch (System.Exception e)
            //         {
            //             Debug.LogError("exception when processing " + path);
            //             Debug.LogException(e);
            //         }
            //     }
            //     dict.Remove("excel");
            // }
            // EditorConfig.saveTempConfig(dict);
        }

        public static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromPath)
        {
            bool refresh = false;
            for (int i = 0; i < imported.Length; ++i)
            {
                if (imported[i].StartsWith("Assets/Data/"))
                {
                    string ext = System.IO.Path.GetExtension(imported[i]);
                    if (ext == ".xls" || ext == ".xlsx")
                    {
#if UNITY_EDITOR_WIN
                        var info = new System.Diagnostics.ProcessStartInfo("python", "Assets/LGFW/Editor/parseExcel.py \"" + imported[i] + "\"");
#endif
#if UNITY_EDITOR_OSX
                        var info = new System.Diagnostics.ProcessStartInfo("Assets/LGFW/Editor/parseExcel.sh", "\"" + imported[i] + "\"");
                        info.WorkingDirectory = System.IO.Path.GetFullPath("./");
#endif
                        info.CreateNoWindow = true;
                        info.UseShellExecute = false;
                        info.RedirectStandardError = true;
                        info.RedirectStandardOutput = true;
                        var p = System.Diagnostics.Process.Start(info);
                        p.WaitForExit();
                        var error = p.StandardError.ReadToEnd();
                        var output = p.StandardOutput.ReadToEnd();
                        p.Close();
                        if (!string.IsNullOrEmpty(output))
                        {
                            Debug.Log("parsing " + imported[i] + " output: " + output);
                        }
                        if (!string.IsNullOrEmpty(error))
                        {
                            Debug.LogError("parsing " + imported[i] + " error: " + error);
                        }
                        refresh = true;
                    }
                    else if (ext == ".json")
                    {
                        string js = System.IO.File.ReadAllText(imported[i], System.Text.UTF8Encoding.UTF8);
                        Dictionary<string, object> dict = (Dictionary<string, object>)Json.decode(js);
                        ExcelParser.parse(dict, imported[i]);
                        refresh = true;
                    }
                }
            }
            if (refresh)
            {
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}
