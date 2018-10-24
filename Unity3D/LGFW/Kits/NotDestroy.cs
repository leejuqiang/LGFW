using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Attach this script to GameObject won't be destroyed when change scene
    /// </summary>
    public class NotDestroy : MonoBehaviour
    {

        void Awake()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Don't destory on load", false, (int)'z')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<NotDestroy>(true);
        }
#endif
    }
}
