using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Base class for a memory pool item represents for a GameObject
    /// </summary>
    public class GameObjectMPItem : MonoBehaviour, IMemoryPoolItem
    {
        public virtual void onInit()
        {

        }

        public virtual void onClear()
        {

        }

        public virtual void onDestroy()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}