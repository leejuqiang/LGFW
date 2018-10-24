using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Resource manager
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {

        private static ResourceManager m_instance;

        /// <summary>
        /// Gets the singleton of ResourceManager
        /// </summary>
        /// <value>The singleton</value>
        public static ResourceManager Instance
        {
            get { return m_instance; }
        }

        void Awake()
        {
            m_instance = this;
        }

        void OnDestroy()
        {
            m_instance = null;
        }

        /// <summary>
        /// Load assets from Resources fold
        /// </summary>
        /// <returns>The asset</returns>
        /// <param name="path">The path of the asset</param>
        /// <typeparam name="T">The type of the asset</typeparam>
        public T loadResource<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// Initialize a GameObject
        /// </summary>
        /// <returns>The GameObject initialized</returns>
        /// <param name="prefab">The Prefab of the GameObject</param>
        public GameObject initPrefab(GameObject prefab)
        {
            return GameObject.Instantiate<GameObject>(prefab);
        }

        /// <summary>
        /// Initialize a GameObject with a parent
        /// </summary>
        /// <returns>The GameObject initialized</returns>
        /// <param name="prefab">The Prefab of the GameObject</param>
        /// <param name="parent">The parent</param>
        public GameObject initPrefab(GameObject prefab, Transform parent)
        {
            return GameObject.Instantiate<GameObject>(prefab, parent);
        }
    }
}
