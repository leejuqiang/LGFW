using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A GameObject used for storing layout in LayoutLoader
    /// </summary>
    [System.Serializable]
    public class LayoutLoaderObject
    {
        /// <summary>
        /// The Transform of the GameObject
        /// </summary>
        public Transform m_trans;

        [HideInInspector]
        public Vector3 m_position;
        [HideInInspector]
        public Vector3 m_scale;
        [HideInInspector]
        public Quaternion m_roataion;
        [HideInInspector]
        public bool m_active = true;

        public void save()
        {
            m_position = m_trans.localPosition;
            m_scale = m_trans.localScale;
            m_roataion = m_trans.localRotation;
            m_active = m_trans.gameObject.activeSelf;
        }

        public void load()
        {
            m_trans.gameObject.SetActive(m_active);
            m_trans.localPosition = m_position;
            m_trans.localScale = m_scale;
            m_trans.localRotation = m_roataion;
        }
    }

    /// <summary>
    /// This can restore several Transforms as a layout, so users can switch different layouts
    /// </summary>
    public class LayoutLoader : MonoBehaviour
    {

        /// <summary>
        /// The id of the layout
        /// </summary>
        public string m_id;
        /// <summary>
        /// All the Transforms in this layout
        /// </summary>
        public LayoutLoaderObject[] m_objects;

        /// <summary>
        /// Saves the layout using the current state of the Transforms
        /// </summary>
        public void saveLayout()
        {
            for (int i = 0; i < m_objects.Length; ++i)
            {
                m_objects[i].save();
            }
        }

        /// <summary>
        /// Loads the restored layout
        /// </summary>
        public void loadLayout()
        {
            for (int i = 0; i < m_objects.Length; ++i)
            {
                m_objects[i].load();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Layout/LayoutLoader", false, (int)'l')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<LayoutLoader>(false);
        }
#endif
    }
}
