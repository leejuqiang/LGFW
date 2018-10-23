using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class DataPostProcessor : AssetPostprocessor
    {

        public static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFromPath)
        {
            EditorConfig ec = EditorConfig.Instance;
            for (int i = 0; i < imported.Length; ++i)
            {
                if (ec.isExcelData(imported[i]))
                {
                    new ExcelToJson(imported[i], ec);
                }
                else
                {
                    JsonProcessor.tryToProcess(imported[i], ec);
                }
            }
        }
    }
}
