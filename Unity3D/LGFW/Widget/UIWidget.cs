using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for UI component
    /// </summary>
    public class UIWidget : MonoBehaviour
    {

        protected bool m_hasAwake;

        /// <summary>
        /// Execute the Awake function of this component
        /// </summary>
        public void forceAwake()
        {
            m_hasAwake = false;
            Awake();
        }

        public void Awake()
        {
            if (!m_hasAwake)
            {
                doAwake();
                m_hasAwake = true;
            }
        }

        protected virtual void doAwake()
        {
            //todo
        }
    }
}
