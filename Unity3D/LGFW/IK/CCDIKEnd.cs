using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class CCDIKEnd : IKEnd
    {

        public int m_maxLoop = 5;

        protected override void ikPosition()
        {
            for (int i = 0; i < m_maxLoop; ++i)
            {
                IKBone b = m_lastBone;
                while (b != null)
                {
                    rotBone(b);
                    b = b.m_child;
                }
                if (checkEnd())
                {
                    return;
                }
            }
        }

        private void rotBone(IKBone b)
        {
            Vector3 p = m_trans.position;
            p = b.Trans.InverseTransformPoint(p);
            p.Normalize();
            Vector3 v = m_target - b.Trans.position;
            v.Normalize();
            LMath.lookAt(b.Trans, v, p, b.m_axis);
            b.limitRotation();
        }
    }
}
