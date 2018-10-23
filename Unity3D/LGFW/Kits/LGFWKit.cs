using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A toolkit set
    /// </summary>
    public class LGFWKit
    {

        /// <summary>
        /// Gets the path of the save files
        /// </summary>
        /// <returns>The path</returns>
        public static string getSavePath()
        {
#if UNITY_EDITOR
            return "Assets/../Save";
#else
			return Application.persistentDataPath;
#endif
        }

        /// <summary>
        /// Check if the file exists
        /// </summary>
        /// <returns>Return true if the file exists</returns>
        /// <param name="path">The path of the file</param>
        public static bool fileExists(string path)
        {
#if UNITY_WSA
			return UnityEngine.Windows.File.Exists (path);
#else
            return System.IO.File.Exists(path);
#endif
        }

        /// <summary>
        /// Writes bytes to a file
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="b">The byte array</param>
        public static void writeBytesToFile(string path, byte[] b)
        {
#if UNITY_WSA
			UnityEngine.Windows.File.WriteAllBytes (path, b);
#else
            System.IO.File.WriteAllBytes(path, b);
#endif
        }

        /// <summary>
        /// Reads bytes from a file
        /// </summary>
        /// <returns>The bytes from the file</returns>
        /// <param name="path">The path of the file</param>
        public static byte[] readBytesFromFile(string path)
        {
#if UNITY_WSA
			return UnityEngine.Windows.File.ReadAllBytes(path);
#else
            return System.IO.File.ReadAllBytes(path);
#endif
        }

        /// <summary>
        /// Writes a text to a file
        /// </summary>
        /// <param name="path">The path of the file</param>
        /// <param name="s">The text</param>
        public static void writeTextToFile(string path, string s)
        {
#if UNITY_WSA
			byte[] b = System.Text.Encoding.UTF8.GetBytes(s);
			UnityEngine.Windows.File.WriteAllBytes (path, b);
#else
            System.IO.File.WriteAllText(path, s);
#endif
        }

        /// <summary>
        /// Reads a text from a file
        /// </summary>
        /// <returns>The text from the file</returns>
        /// <param name="path">The path of the file</param>
        public static string readTextFromFile(string path)
        {
#if UNITY_WSA
			byte[] b = readBytesFromFile(path);
			return System.Text.Encoding.UTF8.GetString(b);
#else
            return System.IO.File.ReadAllText(path);
#endif
        }
    }
}
