using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    [System.Serializable]
    public class UIClipShaderPair
    {
        public Shader m_normalShader;
        public Shader m_clipShader;
    }

    /// <summary>
    /// Clips all the ui components under it
    /// </summary>
    public class UIClip : UIWidget
    {

        /// <summary>
        /// The parent clip
        /// </summary>
        public UIClip m_parent;
        /// <summary>
        /// The clip will replace the shader of the clipped component based on the pairs
        /// </summary>
        public UIClipShaderPair[] m_shaderPairs;
        [SerializeField]
        protected Vector2 m_center;
        [SerializeField]
        protected Vector2 m_size;

        private Vector4 m_clipArea;
        private bool m_clipAreaChanged = true;
        private Transform m_trans;

        private Dictionary<Material, Material> m_matDict = new Dictionary<Material, Material>();

        /// <summary>
        /// The center of the clip
        /// </summary>
        /// <value></value>
        public Vector2 Center
        {
            get { return m_center; }
            set
            {
                if (m_center != value)
                {
                    m_center = value;
                    m_clipAreaChanged = true;
                }
            }
        }

        /// <summary>
        /// The size of the clip
        /// </summary>
        /// <value></value>
        public Vector2 Size
        {
            get { return m_size; }
            set
            {
                if (m_size != value)
                {
                    m_size = value;
                    m_clipAreaChanged = true;
                }
            }
        }

        /// <summary>
        /// Finds the pair of shaders by a shader
        /// </summary>
        /// <param name="s">The shader</param>
        /// <returns>The pair</returns>
        public UIClipShaderPair findShaderPair(Shader s)
        {
            for (int i = 0; i < m_shaderPairs.Length; ++i)
            {
                if (m_shaderPairs[i].m_normalShader == s)
                {
                    return m_shaderPairs[i];
                }
            }
            return m_shaderPairs[0];
        }

        public Material getClipMaterial(Material m)
        {
            if (m == null)
            {
                return null;
            }
            Material ret = null;
            if (!m_matDict.TryGetValue(m, out ret))
            {
                UIClipShaderPair p = findShaderPair(m.shader);
                ret = new Material(p.m_clipShader);
                ret.mainTexture = m.mainTexture;
                m_matDict[m] = ret;
            }
            return ret;
        }

        private bool isParentOfThisClip(UIClip c)
        {
            UIClip temp = this;
            while (temp != null)
            {
                if (temp == c)
                {
                    return true;
                }
                temp = temp.m_parent;
            }
            return false;
        }

        /// <summary>
        /// If the click position in the clipped area
        /// </summary>
        /// <param name="worldPos">The click position in world</param>
        /// <returns>If the position is clipped</returns>
        public bool isPointInsideClipArea(Vector3 worldPos)
        {
            Vector3 v = m_trans.InverseTransformPoint(worldPos);
            return v.x >= m_clipArea.x && v.x <= m_clipArea.z && v.y >= m_clipArea.y && v.y <= m_clipArea.w;
        }

        /// <summary>
        /// Adds a GameObject and all its children to the clip
        /// </summary>
        /// <param name="go">The GameObject</param>
        /// <param name="isIncludeSelf">If true, the GameObject will be clipped</param>
        public void addGameObjectToClip(GameObject go, bool isIncludeSelf)
        {
            UIMesh[] ms = go.GetComponentsInChildren<UIMesh>(true);
            for (int i = 0; i < ms.Length; ++i)
            {
                if (!ms[i].m_dontClip && (isIncludeSelf || ms[i].gameObject != go))
                {
                    ms[i].Awake();
                    UIClip c = ms[i].Clip;
                    if (c == null || isParentOfThisClip(c))
                    {
                        ms[i].Clip = this;
                    }
                }
            }
            UITouchWidget[] tms = go.GetComponentsInChildren<UITouchWidget>(true);
            for (int i = 0; i < tms.Length; ++i)
            {
                if (!tms[i].m_dontClip && (isIncludeSelf || tms[i].gameObject != go))
                {
                    UIClip c = tms[i].Clip;
                    if (c == null || isParentOfThisClip(c))
                    {
                        tms[i].Clip = this;
                    }
                }
            }
        }

        private void findAllClipComponent()
        {
            addGameObjectToClip(this.gameObject, false);
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_trans = this.transform;
            reset();
        }

        protected void OnUIScaleUpdate(UIScale scale)
        {
            Size = scale.getSize(m_size.y);
        }

        /// <summary>
        /// Resets the clip
        /// </summary>
        public void reset()
        {
            findAllClipComponent();
            m_clipAreaChanged = true;
        }

        private void updateClipArea()
        {
            if (m_parent != null)
            {
                m_parent.updateClipArea();
            }
            if (m_clipAreaChanged)
            {
                Vector3 v = Vector3.zero;
                v.x += m_center.x - m_size.x * 0.5f;
                v.y += m_center.y - m_size.y * 0.5f;
                Vector3 v1 = v;
                v1.x += m_size.x;
                v1.y += m_size.y;
                if (m_parent != null)
                {
                    Vector3 pv = new Vector3(m_parent.m_clipArea.x, m_parent.m_clipArea.y, 0);
                    Vector3 pv1 = new Vector3(m_parent.m_clipArea.z, m_parent.m_clipArea.w, 0);
                    pv = m_parent.m_trans.TransformPoint(pv);
                    pv1 = m_parent.m_trans.TransformPoint(pv1);
                    pv = m_trans.InverseTransformPoint(pv);
                    pv1 = m_trans.InverseTransformPoint(pv1);
                    v.x = Mathf.Max(v.x, pv.x);
                    v.y = Mathf.Max(v.y, pv.y);
                    v1.x = Mathf.Min(v1.x, pv1.x);
                    v1.y = Mathf.Min(v1.y, pv1.y);
                }
                m_clipArea.Set(v.x, v.y, v1.x, v1.y);
                m_clipAreaChanged = false;
                Matrix4x4 mat = m_trans.worldToLocalMatrix;
                foreach (Material m in m_matDict.Values)
                {
                    m.SetVector("_Clip", m_clipArea);
                    m.SetMatrix("_Mat", mat);
                }
            }
        }

        void LateUpdate()
        {
            if (m_trans.hasChanged)
            {
                m_clipAreaChanged = true;
                m_trans.hasChanged = false;
            }
            updateClipArea();
        }

        public void createDefaultShaderPairs()
        {
            m_shaderPairs = new UIClipShaderPair[3];
            UIClipShaderPair sp = new UIClipShaderPair();
            sp.m_normalShader = Shader.Find("LGFW/ui");
            sp.m_clipShader = Shader.Find("LGFW/ui_clip");
            m_shaderPairs[0] = sp;
            sp = new UIClipShaderPair();
            sp.m_normalShader = Shader.Find("LGFW/uiText");
            sp.m_clipShader = Shader.Find("LGFW/uiText_clip");
            m_shaderPairs[1] = sp;
            sp = new UIClipShaderPair();
            sp.m_normalShader = Shader.Find("LGFW/uiPMA");
            sp.m_clipShader = Shader.Find("LGFW/uiPMA_clip");
            m_shaderPairs[2] = sp;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/UI/UIClip", false, (int)'c')]
        public static void addToGameObjects()
        {
            UIClip[] cs = LEditorKits.addComponentToSelectedOjbects<UIClip>(true);
            for (int i = 0; i < cs.Length; ++i)
            {
                cs[i].createDefaultShaderPairs();
            }
        }

        void OnDrawGizmos()
        {
            if (UnityEditor.Selection.activeGameObject != this.gameObject)
            {
                return;
            }
            Color c = Gizmos.color;
            Gizmos.color = Color.yellow;
            Vector3 v1 = new Vector3(m_center.x - m_size.x * 0.5f, m_center.y - m_size.y * 0.5f, 0);
            Vector3 v2 = new Vector3(v1.x, v1.y + m_size.y, 0);
            Vector3 v3 = new Vector3(v1.x + m_size.x, v1.y + m_size.y, 0);
            Vector3 v4 = new Vector3(v1.x + m_size.x, v1.y, 0);
            Transform t = this.transform;
            v1 = t.TransformPoint(v1);
            v2 = t.TransformPoint(v2);
            v3 = t.TransformPoint(v3);
            v4 = t.TransformPoint(v4);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v4);
            Gizmos.DrawLine(v1, v4);
            Gizmos.color = c;
        }
#endif
    }
}
