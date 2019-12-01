using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for values in a shader
    /// </summary>
    public class UITweenMaterial : UITween
    {

        /// <summary>
        /// The from value of a vector4
        /// </summary>
        public Vector4 m_fromVector;
        /// <summary>
        /// The to value of a vector4
        /// </summary>
        public Vector4 m_toVector;
        /// <summary>
        /// The name of the vector4
        /// </summary>
        public string m_vectorName;

        /// <summary>
        /// The from value of a float
        /// </summary>
        public float m_fromFloat;
        /// <summary>
        /// The to value of a float
        /// </summary>
        public float m_toFloat;
        /// <summary>
        /// The name of the float
        /// </summary>
        public string m_floatName;

        /// <summary>
        /// The from color
        /// </summary>
        public Color m_fromColor;
        /// <summary>
        /// The to color
        /// </summary>
        public Color m_toColor;
        /// <summary>
        /// The name of the color
        /// </summary>
        public string m_colorName;

        /// <summary>
        /// The MeshRenderer which has the material. If null, uses the one on the same GameObject
        /// </summary>
        public MeshRenderer m_render;

        protected override void doAwake()
        {
            if (m_render == null)
            {
                m_render = this.gameObject.GetComponent<MeshRenderer>();
            }
        }

        protected override void editorAwake()
        {
            doAwake();
        }


        protected override void updateTween(float f)
        {
            if (m_render != null)
            {
                if (!string.IsNullOrEmpty(m_vectorName))
                {
                    Vector4 v = Vector4.LerpUnclamped(m_fromVector, m_toVector, f);
                    m_render.sharedMaterial.SetVector(m_vectorName, v);
                }
                if (!string.IsNullOrEmpty(m_colorName))
                {
                    Color c = Color.LerpUnclamped(m_fromColor, m_toColor, f);
                    m_render.sharedMaterial.SetColor(m_colorName, c);
                }
                if (!string.IsNullOrEmpty(m_floatName))
                {
                    float v = Mathf.LerpUnclamped(m_fromFloat, m_toFloat, f);
                    m_render.sharedMaterial.SetFloat(m_floatName, v);
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Tween/TweenMaterial", false, (int)'m')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UITweenMaterial>(false);
        }
#endif
    }
}