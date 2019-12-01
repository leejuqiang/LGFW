using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MeshEditorPlane : MonoBehaviour
    {

        public int m_order;
        public Vector3 m_referencePoint;
        //public UIImage m_texture;
        public MeshEditorNode[] m_externalNodes;
        public MeshFilter m_importedMesh;
        public MeshEditorBone m_planeBone;

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

#if UNITY_EDITOR
        public List<MeshEditorNode> collectNodes(List<int> indexList, List<Vector2> uv, bool hasNormal)
        {
            List<MeshEditorNode> l = new List<MeshEditorNode>();
            MeshEditorNode[] ns = this.gameObject.GetComponentsInChildren<MeshEditorNode>(false);
            for (int i = 0; i < ns.Length; ++i)
            {
               // uv[ns[i].m_cacheIndex] = ns[i].computeUV(m_texture);
            }
            l.AddRange(ns);
            if (m_externalNodes != null)
            {
                l.AddRange(m_externalNodes);
            }
            if (l.Count <= 0)
            {
                return l;
            }
            Vector3 c = Trans.InverseTransformPoint(l[0].Trans.position);
            Vector3 r = Trans.InverseTransformPoint(m_referencePoint);
            r -= c;
            for (int i = 1; i < l.Count; ++i)
            {
                for (int j = i + 1; j < l.Count; ++j)
                {
                    Vector3 v1 = Trans.InverseTransformPoint(l[i].Trans.position);
                    Vector3 v2 = Trans.InverseTransformPoint(l[j].Trans.position);
                    v1 -= c;
                    v2 -= c;
                    v1 = Vector3.Cross(v1, v2);
                    float f = Vector3.Dot(v1, r);
                    if (f > 0)
                    {
                        MeshEditorNode t = l[i];
                        l[i] = l[j];
                        l[j] = t;
                    }
                }
            }
            for (int i = 1, size = l.Count - 1; i < size; ++i)
            {
                indexList.Add(l[0].m_cacheIndex);
                indexList.Add(l[i].m_cacheIndex);
                indexList.Add(l[i + 1].m_cacheIndex);
            }
            if (hasNormal)
            {
                for (int i = 0; i < l.Count; ++i)
                {
                    Vector3 v = Trans.InverseTransformPoint(l[i].Trans.position);
                    int j = i + 1;
                    if (j >= l.Count)
                    {
                        j = 0;
                    }
                    Vector3 v1 = Trans.InverseTransformPoint(l[j].Trans.position);
                    j = i - 1;
                    if (j < 0)
                    {
                        j = l.Count - 1;
                    }
                    Vector3 v2 = Trans.InverseTransformPoint(l[j].Trans.position);
                    v1 -= v;
                    v2 -= v;
                    v = Vector3.Cross(v1, v2);
                    if (Vector3.Dot(v, r) > 0)
                    {
                        v = -v;
                    }
                    v.Normalize();
                    l[i].addNormal(v);
                }
            }
            return l;
        }

        void OnDrawGizmos()
        {
            if (LEditorKits.isGameObjectSelected(this.gameObject))
            {
                Color c = Gizmos.color;
                Gizmos.color = Color.yellow;
                float r = UnityEditor.HandleUtility.GetHandleSize(m_referencePoint) * 0.1f;
                Gizmos.DrawSphere(m_referencePoint, r);
                Gizmos.color = c;
            }
        }
#endif
    }
}
