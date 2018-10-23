using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Tween for rotation
    /// </summary>
    public class UITweenRotate : UITween
    {

        /// <summary>
        /// From rotation
        /// </summary>
        public Vector3 m_from;
        /// <summary>
        /// To rotation
        /// </summary>
        public Vector3 m_to;

        private Transform m_trans;

        protected override void resetFromCurrentValue()
        {
            base.resetFromCurrentValue();
            m_from = m_trans.localRotation.eulerAngles;
        }

        protected override void resetToCurrentValue()
        {
            base.resetToCurrentValue();
            m_to = m_trans.localRotation.eulerAngles;
        }

        protected override void doAwake()
        {
            base.doAwake();
            m_trans = this.transform;
        }

        protected override void applyFactor(float f)
        {
            Vector3 v = Vector3.Lerp(m_from, m_to, f);
            m_trans.localRotation = Quaternion.Euler(v);
        }
    }
}
