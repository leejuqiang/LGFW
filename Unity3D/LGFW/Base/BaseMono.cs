using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The base class for a custom MonoBehaviour, the purpose is that you can call Start() and Awake() any times,
    /// doAwake() and doStart() are only called once. So your code should be in these 2 functions.
    /// In editor mode, editorAwake() and editorStart() are called instead.
    /// </summary>
    public class BaseMono : MonoBehaviour
    {
        protected int m_flag = 0;

        public void Awake()
        {
            if (!hasAwaken())
            {
                m_flag |= 1;
                if (!Application.isPlaying)
                {
                    editorAwake();
                }
                else
                {
                    doAwake();
                }
            }
        }

        /// <summary>
        /// Resets the flags for hasAwaken() and hasStart().
        /// Calling this then calling Awake() and Start() makes you be able to call doAwake() and doStart() again
        /// </summary>
        public void resetFlag()
        {
            m_flag = 0;
        }

        /// <summary>
        /// If Awake() has been called, works both for editor mode and player mode
        /// </summary>
        /// <returns>True if has been called</returns>
        public bool hasAwaken()
        {
            return (m_flag & 1) != 0;
        }

        /// <summary>
        /// If Start() has been called, works both for editor mode and player mode
        /// </summary>
        /// <returns>True if has been called</returns>
        public bool hasStarted()
        {
            return (m_flag & 2) != 0;
        }

        protected virtual void doAwake()
        {

        }

        protected virtual void editorAwake()
        {

        }

        public void Start()
        {
            if (!hasStarted())
            {
                m_flag |= 2;
                if (!Application.isPlaying)
                {

                    editorStart();
                }
                else
                {
                    doStart();
                }
            }
        }

        protected virtual void doStart()
        {

        }

        protected virtual void editorStart()
        {

        }

    }
}