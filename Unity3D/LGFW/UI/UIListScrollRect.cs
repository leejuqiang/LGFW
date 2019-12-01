using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LGFW
{
    public class UIListScrollRect : ScrollRect
    {

        public bool LoopList
        {
            get; set;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            // if (!LoopList)
            {
                base.OnBeginDrag(eventData);
            }
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            // if (!LoopList)
            {
                base.OnEndDrag(eventData);
            }
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!LoopList)
            {
                base.OnDrag(eventData);
            }
        }
    }
}