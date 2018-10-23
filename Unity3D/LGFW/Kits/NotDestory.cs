using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Attach this script to GameObject won't be destoryed when change scene
    /// </summary>
    public class NotDestory : MonoBehaviour
    {

        void Awake()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Don't destory on load", false, (int)'z')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<NotDestory>(true);
        }
#endif
    }
}
