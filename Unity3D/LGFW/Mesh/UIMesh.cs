using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The base class for a mesh for UI
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public abstract class UIMesh : UIRect
    {

        public const int FLAG_VERTEX = 1;
        public const int FLAG_UV = 1 << 1;
        public const int FLAG_COLOR = 1 << 2;
        public const int FLAG_INDEX = 1 << 4;
        public const int FLAG_UV1 = 1 << 5;
        public const int FLAG_TEXT = 1 << 6;
        public const int FLAG_CLIP = 1 << 7;
        public const int FLAG_NORMAL = 1 << 8;

        public const int FLAG_TIP = 1 << 9;

        /// <summary>
        /// If true, this component won't be clipped by UIClip
        /// </summary>
        public bool m_dontClip;

        protected MeshFilter m_meshFilter;
        protected MeshRenderer m_meshRenderer;
        protected Mesh m_mesh;


        [SerializeField]
        protected Color m_color = Color.white;
        [SerializeField]
        protected float m_alpha = 1;

        protected UIClip m_uiClip;
        protected Gradient m_gradient;

        /// <summary>
        /// The UIClip for this component
        /// </summary>
        /// <value>The UIClip</value>
        public UIClip Clip
        {
            get { return m_uiClip; }
            set
            {
                if (m_uiClip != value)
                {
                    m_uiClip = value;
                    onClipChanged();
                }
            }
        }

        /// <summary>
        /// Gets the MeshFilter
        /// </summary>
        /// <value></value>
        public MeshFilter Filter
        {
            get { return m_meshFilter; }
        }

        /// <summary>
        /// Gets the MeshRenderer
        /// </summary>
        /// <value></value>
        public MeshRenderer Render
        {
            get { return m_meshRenderer; }
        }

        protected override void onSizeChanged()
        {
            base.onSizeChanged();
            m_updateFlag |= FLAG_VERTEX;
        }

        protected override void onAnchorChanged()
        {
            base.onAnchorChanged();
            m_updateFlag |= FLAG_VERTEX;
        }

        /// <summary>
        /// The color
        /// </summary>
        /// <value></value>
        public Color CurrentColor
        {
            get { return m_color; }
            set
            {
                if (m_color != value)
                {
                    m_color = value;
                    onColorChanged();
                }
            }
        }

        protected virtual void onColorChanged()
        {
            m_updateFlag |= FLAG_COLOR;
        }

        /// <summary>
        /// The alpha
        /// </summary>
        /// <value></value>
        public virtual float Alpha
        {
            get { return m_alpha; }
            set
            {
                if (m_alpha != value)
                {
                    m_alpha = value;
                    onColorChanged();
                }
            }
        }

        protected int m_updateFlag;

        /// <summary>
        /// Resets the color gradient
        /// </summary>
        public void resetGradient()
        {
            m_gradient = this.GetComponent<Gradient>();
            if (m_gradient != null)
            {
                m_gradient.init();
            }
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_meshFilter = this.GetComponent<MeshFilter>();
            m_meshRenderer = this.GetComponent<MeshRenderer>();
            resetGradient();
            createMesh();
            changeMaterialOnRender();
            reset();
        }

        protected virtual void onClipChanged()
        {
            m_updateFlag |= FLAG_CLIP;
        }

        protected abstract void changeMaterialOnRender();

        protected virtual void createMesh()
        {
            m_mesh = new Mesh();
            m_mesh.MarkDynamic();
            m_meshFilter.sharedMesh = m_mesh;
        }

        protected virtual bool has2ndUV()
        {
            return false;
        }

        /// <summary>
        /// Repaints this component
        /// </summary>
        public void repaint()
        {
            if (m_mesh != null)
            {
                m_mesh.Clear();
            }
            m_updateFlag = 0xffffff;
        }

        /// <summary>
        /// Resets this component
        /// </summary>
        public virtual void reset()
        {
            Awake();
            updateLocalPosition();
            repaint();
        }

        protected virtual void updateFlag()
        {
            if ((m_updateFlag & FLAG_CLIP) > 0)
            {
                changeMaterialOnRender();
            }
            if ((m_updateFlag & FLAG_VERTEX) > 0)
            {
                updateVertex();
            }
            if ((m_updateFlag & FLAG_UV) > 0)
            {
                updateUV();
            }
            if (has2ndUV() && (m_updateFlag & FLAG_UV1) > 0)
            {
                updateUV1();
            }
            if ((m_updateFlag & FLAG_COLOR) > 0)
            {
                if (m_gradient == null || !m_gradient.useGradient())
                {
                    Color c = m_color;
                    c.a *= m_alpha;
                    updateColor(c);
                }
                else
                {
                    updateColorByGradient(m_gradient);
                }
            }
            if ((m_updateFlag & FLAG_INDEX) > 0)
            {
                updateIndex();
            }
        }

        public virtual void LateUpdate()
        {
            if (m_meshRenderer.sharedMaterial == null)
            {
                repaint();
                return;
            }
            if (!Application.isPlaying)
            {
                resetGradient();
            }
            if (m_gradient != null && m_gradient.IsChanged)
            {
                m_updateFlag |= FLAG_COLOR;
            }
            if (m_updateFlag != 0)
            {
                updateFlag();
                m_updateFlag = 0;
            }
            if (m_gradient != null)
            {
                m_gradient.IsChanged = false;
            }
        }

        protected abstract void updateVertex();
        protected abstract void updateUV();

        protected virtual void updateUV1()
        {
            //todo
        }

        protected abstract void updateColor(Color c);

        protected abstract void updateColorByGradient(Gradient g);

        protected abstract void updateIndex();

        protected virtual void OnUIScaleUpdate(UIScale scale)
        {
            Size = scale.getSize(m_size.y);
        }
    }
}
