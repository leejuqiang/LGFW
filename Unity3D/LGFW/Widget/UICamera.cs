using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Attached to a camera to resize the display area
    /// </summary>
    public class UICamera : MonoBehaviour
    {

        /// <summary>
        /// The id
        /// </summary>
        public string m_id = "ui";

        private Camera m_camera;
        private static Dictionary<string, UICamera> m_cameras = new Dictionary<string, UICamera>();

        /// <summary>
        /// The camera
        /// </summary>
        /// <value></value>
        public Camera RealCamera
        {
            get { return m_camera; }
        }

        void Awake()
        {
            m_camera = this.GetComponent<Camera>();
            m_camera.orthographic = true;
            m_camera.orthographicSize = LGlobal.Instance.m_screenHeight * 0.5f;

            m_cameras.Add(m_id, this);
        }

        void OnDestroy()
        {
            m_cameras.Remove(m_id);
        }

        /// <summary>
        /// Gets a UICamera by id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The UICamera</returns>
        public static UICamera getUICamera(string id)
        {
            UICamera c = null;
            m_cameras.TryGetValue(id, out c);
            return c;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UICamera", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<UICamera>(true);
            LEditorKits.addComponentToSelectedOjbects<CameraRay>(true);
        }
#endif
    }
}
