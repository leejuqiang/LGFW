using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum MeshEditorNodeUV
    {
        custom,
        selfPosition,
        uvNode,
    }

    [ExecuteInEditMode]
    public class MeshEditorNode : MonoBehaviour
    {

        public Vector2 m_customUV;
        public MeshEditorNodeUV m_uvType = MeshEditorNodeUV.selfPosition;
        public MeshEditorNode[] m_samePositionNodes;
        public Transform m_uvNode;
        public bool m_customColor;
        public Color m_color = Color.white;

        public List<MeshEditorBone> m_bones = new List<MeshEditorBone>();
        public List<float> m_weights = new List<float>();

        private Transform m_trans;
        private List<Vector3> m_normals = new List<Vector3>();

        [System.NonSerialized]
        public int m_cacheIndex = -1;

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
        public void init(int index)
        {
            m_cacheIndex = index;
            m_normals.Clear();
        }

        public void addNormal(Vector3 v)
        {
            m_normals.Add(v);
        }

        public Vector3 getNormal()
        {
            Vector3 v = Vector3.zero;
            for (int i = 0; i < m_normals.Count; ++i)
            {
                v += m_normals[i];
            }
            v /= m_normals.Count;
            return v;
        }

        // public Vector2 computeUV(UIImage image)
        // {
        //     if (m_uvType == MeshEditorNodeUV.custom)
        //     {
        //         return m_customUV;
        //     }
        //     else
        //     {
        //         if (image == null)
        //         {
        //             return Vector2.zero;
        //         }
        //         if (m_uvType == MeshEditorNodeUV.selfPosition)
        //         {
        //             return computeUVByTrans(Trans, image);
        //         }
        //         else
        //         {
        //             return computeUVByTrans(m_uvNode, image);
        //         }
        //     }
        // }

        // private Vector2 computeUVByTrans(Transform t, UIImage image)
        // {
        //     Vector3 v = t.position;
        //     v = image.Trans.InverseTransformPoint(v);
        //     Rect rc = image.LocalPosition;
        //     v.x = (v.x - rc.xMin) / rc.width;
        //     v.y = (v.y - rc.yMin) / rc.height;
        //     if (image is UISprite)
        //     {
        //         UISprite s = (UISprite)image;
        //         UIAtlasSprite a = s.AtlasSprite;
        //         if (a != null)
        //         {
        //             v.x = a.m_uv.xMin + v.x * a.m_uv.width;
        //             v.y = a.m_uv.yMin + v.y * a.m_uv.height;
        //         }
        //     }
        //     return v;
        // }

        void Update()
        {
            if (m_samePositionNodes != null)
            {
                foreach (MeshEditorNode n in m_samePositionNodes)
                {
                    n.Trans.position = Trans.position;
                }
            }
        }

        public BoneWeight computeWeights()
        {
            BoneWeight bw = new BoneWeight();
            for (int i = m_bones.Count - 1; i >= 0; --i)
            {
                if (m_bones[i] == null)
                {
                    m_bones.RemoveAt(i);
                    m_weights.RemoveAt(i);
                }
            }
            if (m_bones.Count <= 0)
            {
                return bw;
            }
            while (m_weights.Count > m_bones.Count)
            {
                m_weights.RemoveAt(m_weights.Count - 1);
            }
            while (m_weights.Count < m_bones.Count)
            {
                m_weights.Add(0);
            }
            float t = 0;
            for (int i = 0; i < m_weights.Count; ++i)
            {
                t += m_weights[i];
            }
            if (t <= 0)
            {
                m_weights[0] = 1;
                return bw;
            }
            for (int i = 0; i < m_weights.Count; ++i)
            {
                m_weights[i] /= t;
            }
            if (m_bones.Count > 0)
            {
                bw.boneIndex0 = m_bones[0].m_index;
                bw.weight0 = m_weights[0];
            }
            if (m_bones.Count > 1)
            {
                bw.boneIndex1 = m_bones[1].m_index;
                bw.weight1 = m_weights[1];
            }
            if (m_bones.Count > 2)
            {
                bw.boneIndex2 = m_bones[2].m_index;
                bw.weight2 = m_weights[2];
            }
            if (m_bones.Count > 3)
            {
                bw.boneIndex3 = m_bones[3].m_index;
                bw.weight3 = m_weights[3];
            }
            return bw;
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
            v.x += r;
            v.y += r;
            UnityEditor.Handles.Label(v, this.gameObject.name);

            if (m_uvNode != null)
            {
            }
            Gizmos.color = c;
        }
#endif
    }
}
