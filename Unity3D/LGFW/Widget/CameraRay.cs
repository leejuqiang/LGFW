using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The touch on screen
    /// </summary>
    public class UITouch
    {
        /// <summary>
        /// Finger id
        /// </summary>
        public int m_id;
        /// <summary>
        /// Position on screen
        /// </summary>
        public Vector3 m_screenPosition;
        /// <summary>
        /// If the touch is released inside the collider
        /// </summary>
        public bool m_releaseInside;

        /// <summary>
        /// The touched object
        /// </summary>
        public GameObject m_hitObject;
        /// <summary>
        /// The position of touch
        /// </summary>
        public Vector3 m_hitPoint;
        /// <summary>
        /// If this is a 2d collider
        /// </summary>
        public bool m_is2DHit;
        /// <summary>
        /// The CameraRay of this touch
        /// </summary>
        public CameraRay m_cameraRay;

        public UITouch(int id, CameraRay cr)
        {
            m_id = id;
            m_cameraRay = cr;
        }

        /// <summary>
        /// The world position of this touch's screen point
        /// </summary>
        /// <value></value>
        public Vector3 ScreenPositionInWorld
        {
            get
            {
                return m_cameraRay.CameraAttached.ScreenToWorldPoint(m_screenPosition);
            }
        }
    }

    /// <summary>
    /// Attached to a camera to cast touch events, will send event: "OnPress", "OnRelease", "OnDrag"
    /// </summary>
    public class CameraRay : MonoBehaviour
    {

        public const string m_eventPress = "OnPress";
        public const string m_eventRelease = "OnRelease";
        public const string m_eventDrag = "OnDrag";

        private Camera m_camera;
        private List<UITouch> m_touches = new List<UITouch>();

        private RaycastHit m_rayHit = new RaycastHit();
        private RaycastHit2D m_rayHit2D = new RaycastHit2D();
        private static int m_touchCount;

        private static LinkedList<CameraRay> m_updateList = new LinkedList<CameraRay>();
        private bool m_useMouse;

        /// <summary>
        /// Gets the touch count
        /// </summary>
        /// <value></value>
        public static int TouchCount
        {
            get { return m_touchCount; }
        }

        /// <summary>
        /// Gets the camera of it
        /// </summary>
        /// <value></value>
        public Camera CameraAttached
        {
            get
            {
                if (m_camera == null)
                {
                    m_camera = this.GetComponent<Camera>();
                }
                return m_camera;
            }
        }

        private UITouch getTouchById(int id)
        {
            while (id >= m_touches.Count)
            {
                m_touches.Add(new UITouch(id, this));
            }
            return m_touches[id];
        }

        private void countMouseTouch()
        {
            m_touchCount = 0;
            if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
            {
                ++m_touchCount;
            }
            if (Input.GetMouseButton(1) || Input.GetMouseButtonUp(1))
            {
                ++m_touchCount;
            }
            if (Input.GetMouseButton(2) || Input.GetMouseButtonUp(2))
            {
                ++m_touchCount;
            }
        }

        void Awake()
        {
            m_useMouse = Application.isEditor || Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerX86;
            LinkedListNode<CameraRay> n = m_updateList.First;
            while (n != null)
            {
                if (n.Value.CameraAttached.depth > this.CameraAttached.depth)
                {
                    m_updateList.AddBefore(n, this);
                    return;
                }
                n = n.Next;
            }
            m_updateList.AddLast(this);
        }

        void OnDestroy()
        {
            m_updateList.Remove(this);
        }

        private void updateMouse()
        {
            countMouseTouch();
            updateMouseByButton(0);
            updateMouseByButton(1);
            updateMouseByButton(2);
        }

        private void updateMouseByButton(int button)
        {
            LinkedListNode<CameraRay> n = m_updateList.First;
            while (n != null)
            {
                UITouch ut = n.Value.getTouchById(button);
                if (Input.GetMouseButtonDown(button))
                {
                    ut.m_screenPosition = Input.mousePosition;
                    n.Value.updateTouchBegin(ut);
                }
                else if (Input.GetMouseButtonUp(button))
                {
                    ut.m_screenPosition = Input.mousePosition;
                    n.Value.updateTouchEnd(ut);
                }
                else if (Input.GetMouseButton(button))
                {
                    ut.m_screenPosition = Input.mousePosition;
                    n.Value.updateTouchMove(ut);
                }
                if (ut.m_hitObject != null)
                {
                    break;
                }
                n = n.Next;
            }
        }

        void Update()
        {
            if (this == m_updateList.First.Value)
            {
                doUpdate();
            }
        }

        private void doUpdate()
        {
            if (m_useMouse)
            {
                updateMouse();
            }
            else
            {
                updateTouch();
            }
        }

        private void updateTouchByIndex(int index)
        {
            Touch t = Input.GetTouch(index);
            LinkedListNode<CameraRay> n = m_updateList.First;
            while (n != null)
            {
                UITouch ut = n.Value.getTouchById(t.fingerId);
                ut.m_screenPosition = t.position;
                if (t.phase == TouchPhase.Began)
                {
                    n.Value.updateTouchBegin(ut);
                }
                else if (t.phase == TouchPhase.Moved)
                {
                    n.Value.updateTouchMove(ut);
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    n.Value.updateTouchEnd(ut);
                }
                if (ut.m_hitObject != null)
                {
                    break;
                }
                n = n.Next;
            }
        }

        private void updateTouch()
        {
            m_touchCount = Input.touchCount;
            for (int i = 0; i < m_touchCount; ++i)
            {
                updateTouchByIndex(i);
            }
        }

        private void rayCast(UITouch t)
        {
            Camera c = CameraAttached;
            Ray r = c.ScreenPointToRay(t.m_screenPosition);
            if (c.orthographic)
            {
                t.m_is2DHit = true;
                m_rayHit2D = Physics2D.Raycast(r.origin, r.direction, c.farClipPlane, c.cullingMask);
            }
            else
            {
                t.m_is2DHit = false;
                Physics.Raycast(r, out m_rayHit, c.farClipPlane, c.cullingMask);
            }
        }

        private GameObject getHitGameObject(UITouch t)
        {
            if (t.m_is2DHit)
            {
                if (m_rayHit2D.collider != null)
                {
                    return m_rayHit2D.collider.gameObject;
                }
                return null;
            }
            if (m_rayHit.collider != null)
            {
                return m_rayHit.collider.gameObject;
            }
            return null;
        }

        private void setHitPoint(UITouch t)
        {
            if (t.m_is2DHit)
            {
                t.m_hitPoint = m_rayHit2D.point;
            }
            t.m_hitPoint = m_rayHit.point;
        }

        private bool updateTouchBegin(UITouch t)
        {
            rayCast(t);
            t.m_hitObject = getHitGameObject(t);
            if (t.m_hitObject != null)
            {
                setHitPoint(t);
                t.m_hitObject.SendMessage(m_eventPress, t, SendMessageOptions.DontRequireReceiver);
                return true;
            }
            return false;
        }

        private bool updateTouchMove(UITouch t)
        {
            if (t.m_hitObject != null)
            {
                t.m_hitObject.SendMessage(m_eventDrag, t, SendMessageOptions.DontRequireReceiver);
                return true;
            }
            return false;
        }

        private bool updateTouchEnd(UITouch t)
        {
            if (t.m_hitObject != null)
            {
                rayCast(t);
                GameObject go = getHitGameObject(t);
                t.m_releaseInside = go == t.m_hitObject;
                if (go != null)
                {
                    setHitPoint(t);
                }
                t.m_hitObject.SendMessage(m_eventRelease, t, SendMessageOptions.DontRequireReceiver);
            }
            return false;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/CameraRay", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<CameraRay>(true);
        }
#endif
    }
}
