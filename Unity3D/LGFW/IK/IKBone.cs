using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class IKBone : MonoBehaviour
    {

        public IKBone m_parent;
        public IKBone m_child;
        public Vector3 m_axis;
        public Vector3 m_stretchDirection;
        public Quaternion m_stretchRotation;

        public Vector2 m_rotLimitX = new Vector2(-180, 180);
        public Vector2 m_rotLimitY = new Vector2(-180, 180);
        public Vector2 m_rotLimitZ = new Vector2(-180, 180);
        public bool m_root;

        private Transform m_trans;

        public Transform Trans
        {
            get
            {
                if (m_trans == null)
                {
                    m_trans = this.transform;
                }
                return m_trans;
            }
        }

        public void findParent()
        {
            Transform t = Trans.parent;
            if (t != null)
            {
                m_parent = t.gameObject.GetComponentInParent<IKBone>();
            }
            else
            {
                m_parent = null;
            }
            if (m_parent != null && !m_parent.m_root)
            {
                m_parent.m_child = this;
            }
        }

        public void limitRotation()
        {
            Vector3 v = Trans.localRotation.eulerAngles;
            if (v.x > 180)
            {
                v.x -= 360;
            }
            if (v.y > 180)
            {
                v.y -= 360;
            }
            if (v.z > 180)
            {
                v.z -= 360;
            }
            bool change = false;
            if (v.x < m_rotLimitX.x)
            {
                v.x = m_rotLimitX.x;
                change = true;
            }
            else if (v.x > m_rotLimitX.y)
            {
                v.x = m_rotLimitX.y;
                change = true;
            }
            if (v.y < m_rotLimitY.x)
            {
                v.y = m_rotLimitY.x;
                change = true;
            }
            else if (v.y > m_rotLimitY.y)
            {
                v.y = m_rotLimitY.y;
                change = true;
            }
            if (v.z < m_rotLimitZ.x)
            {
                v.z = m_rotLimitZ.x;
                change = true;
            }
            else if (v.z > m_rotLimitZ.y)
            {
                v.z = m_rotLimitZ.y;
                change = true;
            }
            if (change)
            {
                Trans.localRotation = Quaternion.Euler(v);
            }
        }

        public void setStretchDirection(Vector3 p)
        {
            if (m_root)
            {
                return;
            }
            m_stretchDirection = Trans.InverseTransformPoint(p);
            m_stretchDirection.Normalize();
        }

        public void setStretchRotate()
        {
            if (m_parent == null || m_parent.m_root)
            {
                return;
            }
            Vector3 v = Trans.position - m_parent.Trans.position;
            v.Normalize();
            LMath.lookAt(Trans, v, m_stretchDirection, m_axis);
            limitRotation();
            m_stretchRotation = Trans.localRotation;
        }

        public void stretch()
        {
            Trans.localRotation = m_stretchRotation;
        }
    }
}