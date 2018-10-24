using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using System.Reflection;
#endif 

namespace LGFW
{
    /// <summary>
    /// A layout dependence represent for a screen, the height will be the screen height assigned in LGlobal, and the width will be assigned according the aspect
    /// </summary>
    [ExecuteInEditMode]
    public class UIScreen : UIRect
    {

        /// <summary>
        /// The size of the screen
        /// </summary>
        /// <value>The size</value>
        public override Vector2 Size
        {
            get
            {
                return base.Size;
            }
        }

        protected override void doAwake()
        {
            base.doAwake();
            reset();
        }

        private Vector2 getScreenSize()
        {
#if UNITY_EDITOR
            System.Type t = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            MethodInfo minfo = t.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            object v = minfo.Invoke(null, null);
            return (Vector2)v;
#else
			return new Vector2(Screen.width, Screen.height);
#endif
        }

        /// <summary>
        /// Reset the size of the screen
        /// </summary>
        public void reset()
        {
            if (Application.isPlaying && LGlobal.Instance != null)
            {
                Vector2 s = getScreenSize();
                m_size.y = LGlobal.Instance.m_screenHeight;
                m_size.x = s.x / s.y * m_size.y;
            }
            updateLocalPosition();
            increaseChangeCount();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Layout/UIScreen", false, (int)'s')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<UIScreen>(true);
        }
#endif
    }
}
