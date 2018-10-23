using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    [ExecuteInEditMode]
    public class MeshEditorBone : MonoBehaviour
    {

        public Vector3 m_dir;

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

        [System.NonSerialized]
        public int m_index = -1;

#if UNITY_EDITOR

        void Update()
        {
            for (int i = 0, len = Trans.childCount; i < len; ++i)
            {
                Trans.GetChild(i).gameObject.SendMessage("OnUpdate", this, SendMessageOptions.DontRequireReceiver);
            }
        }

        void OnUpdate(MeshEditorBone b)
        {
            Trans.localPosition = b.m_dir;
        }

        public LinkedList<Transform> getPath(Transform root)
        {
            LinkedList<Transform> l = new LinkedList<Transform>();
            Transform t = this.transform;
            while (t != root && t != null)
            {
                l.AddFirst(t);
                t = t.parent;
            }
            return l;
        }

        void OnDrawGizmos()
        {
            Color c = Gizmos.color;
            Gizmos.color = Color.white;
            if (LEditorKits.isGameObjectSelected(this.gameObject))
            {
                Gizmos.color = Color.red;
            }
            Vector3 v = this.transform.position;
            float r = UnityEditor.HandleUtility.GetHandleSize(v) * 0.1f;
            Gizmos.DrawSphere(v, r);
            Vector3 d = this.transform.TransformPoint(m_dir);
            Gizmos.DrawLine(v, d);
            UnityEditor.Handles.Label(v, this.gameObject.name);
            Gizmos.color = c;
        }
#endif
    }
}
