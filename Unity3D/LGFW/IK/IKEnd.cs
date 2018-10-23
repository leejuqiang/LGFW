using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    [ExecuteInEditMode]
    public abstract class IKEnd : MonoBehaviour
    {

        public float m_offsetLimitSquare = 0.01f;
        public float m_maxRange;
        public IKBone m_bone;

        public Transform m_attachTarget;

        protected IKBone m_lastBone;
        protected Transform m_trans;
        protected Vector3 m_target;
        protected int m_boneNumber;

        public int BoneNumber
        {
            get { return m_boneNumber; }
        }

        protected bool checkEnd()
        {
            Vector3 p = m_trans.position - m_target;
            return p.sqrMagnitude < m_offsetLimitSquare;
        }

        protected virtual void Awake()
        {
            m_trans = this.transform;
            m_boneNumber = 0;
            IKBone b = m_bone;
            while (b != null && !b.m_root)
            {
                ++m_boneNumber;
                if (b.m_parent.m_root)
                {
                    m_lastBone = b;
                    break;
                }
                b = b.m_parent;
            }
        }

        public virtual void initBone()
        {
            m_boneNumber = 0;
            Transform t = m_trans.parent;
            m_bone = null;
            if (t != null)
            {
                m_bone = t.gameObject.GetComponentInParent<IKBone>();
            }
            if (m_bone == null || m_bone.m_root)
            {
                m_bone = null;
            }
            IKBone b = m_bone;
            while (b != null && !b.m_root)
            {
                ++m_boneNumber;
                b.findParent();
                if (b.m_parent.m_root)
                {
                    m_lastBone = b;
                }
                b = b.m_parent;
            }
        }

        public void initBoneConfig()
        {
            IKBone b = m_bone;
            b.setStretchDirection(m_trans.position);
            b = b.m_parent;
            while (b != null && !b.m_root)
            {
                b.setStretchDirection(b.m_child.Trans.position);
                b = b.m_parent;
            }
            b = m_bone;
            while (b != null && !b.m_root)
            {
                b.setStretchRotate();
                b = b.m_parent;
            }
            m_maxRange = (m_trans.position - m_lastBone.Trans.position).sqrMagnitude;
        }

        private void stretchToTarget()
        {
            IKBone b = m_bone;
            while (b != null && !b.m_root)
            {
                b.stretch();
                b = b.m_parent;
            }
            Vector3 v = m_target - m_lastBone.Trans.position;
            v.Normalize();
            LMath.lookAt(m_lastBone.Trans, v, m_lastBone.m_stretchDirection, m_lastBone.m_axis);
            m_lastBone.limitRotation();
        }

        public void setTargetPosition(Vector3 t)
        {
            m_target = t;
            Vector3 v = t - m_lastBone.Trans.position;
            if (v.sqrMagnitude > m_maxRange)
            {
                stretchToTarget();
            }
            else
            {
                ikPosition();
            }
        }

        protected abstract void ikPosition();

        void Update()
        {
            if (m_attachTarget != null && m_attachTarget.gameObject.activeSelf)
            {
                setTargetPosition(m_attachTarget.position);
            }
        }
    }
}