using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
	[ExecuteInEditMode]
	public class JacobianIKEnd : IKEnd {

		public float m_h;

		private IKBone[] m_bones;
		private Vector3[] m_axis;
		private float[] m_os;
		private Vector3 m_offset;

		public override void initBone ()
		{
			base.initBone ();
			if (m_bones == null || m_bones.Length != m_boneNumber) {
				m_bones = new IKBone[m_boneNumber];
				m_axis = new Vector3[m_boneNumber];
				m_os = new float[m_boneNumber];
			}
			IKBone b = m_bone;
			for (int i = 0; i < m_boneNumber; ++i) {
				m_bones [i] = b;
				b = b.m_parent;
			}
		}

		private void computeAixs(int index)
		{
			Vector3 p = m_bones [index].Trans.position;
			Vector3 v1 = m_trans.position - p;
			Vector3 v2 = m_target - p;
			v2 = Vector3.Cross (v1, v2);
			if (Mathf.Approximately (m_axis [index].sqrMagnitude, 0)) {
				v2 = m_bones [index].Trans.InverseTransformDirection (m_bones [index].m_axis);
			} else {
				v2.Normalize ();
			}
			m_axis [index] = v2;
			v2 = Vector3.Cross (v2, v1);
			m_os[index] = v2.x * m_offset.x + v2.y * m_offset.y + v2.z * m_offset.z;
		}

		private void updateBone(int index)
		{
			Quaternion q = Quaternion.AngleAxis (m_os [index] * m_h, m_axis [index]);
			m_bones [index].Trans.rotation = q * m_bones [index].Trans.rotation;
			m_bones [index].limitRotation ();
		}

		private void loop()
		{
			m_offset = m_target - m_trans.position;
			for (int i = 0; i < m_boneNumber; ++i) {
				computeAixs (i);
			}
			for (int i = 0; i < m_boneNumber; ++i) {
				updateBone (i);
			}
		}

		protected override void ikPosition ()
		{
			for (int i = 0; i < 100; ++i) {
				loop ();
			}
		}
	}
}
