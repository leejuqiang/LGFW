using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MeshEditor : MonoBehaviour
    {

        public bool m_hasNormal;
        public bool m_hasBones;
        public Color m_color = Color.white;
        public string m_savePath;
        public SkinnedMeshRenderer m_skinMeshRender;

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

        private int sortPlane(MeshEditorPlane l, MeshEditorPlane r)
        {
            return l.m_order - r.m_order;
        }

        private void handleImportVertex(Vector3[] vs, Transform t, List<Vector3> l)
        {
            for (int i = 0; i < vs.Length; ++i)
            {
                Vector3 v = t.TransformPoint(vs[i]);
                l.Add(Trans.InverseTransformPoint(v));
            }
        }

        private void handleImportWeight(MeshEditorBone b, int len, List<BoneWeight> l)
        {
            BoneWeight bw = new BoneWeight();
            bw.boneIndex0 = b.m_index;
            bw.weight0 = 1;
            for (int i = 0; i < len; ++i)
            {
                l.Add(bw);
            }
        }

        private void handleImportUV(Vector2[] uv, List<Vector2> l)
        {
            for (int i = 0; i < uv.Length; ++i)
            {
                l.Add(uv[i]);
            }
        }

        private void handleImportCol(Color32[] col, List<Color32> l)
        {
            for (int i = 0; i < col.Length; ++i)
            {
                l.Add(col[i]);
            }
        }

        private void handleImportNormal(Vector3[] nor, Transform t, List<Vector3> l)
        {
            for (int i = 0; i < nor.Length; ++i)
            {
                Vector3 v = nor[i];
                v = t.TransformDirection(v);
                v = Trans.InverseTransformDirection(v);
                l.Add(v);
            }
        }

        private Transform createBoneByPath(LinkedList<Transform> l)
        {
            LinkedListNode<Transform> n = l.First;
            Transform t = m_skinMeshRender.transform;
#if UNITY_EDITOR
            while (n != null)
            {
                Transform tc = LEditorKits.getChildTransformByName(t, n.Value.name);
                if (tc == null)
                {
                    GameObject go = new GameObject(n.Value.name);
                    tc = go.transform;
                    tc.parent = t;
                }
                tc.localPosition = n.Value.localPosition;
                tc.localScale = n.Value.localScale;
                tc.localRotation = n.Value.localRotation;
                t = tc;
                n = n.Next;
            }
#endif
            return t;
        }

        private void createBones(MeshEditorBone[] bones)
        {
#if UNITY_EDITOR
            if (m_skinMeshRender == null)
            {
                return;
            }
            Transform[] ts = new Transform[bones.Length];
            for (int i = 0; i < bones.Length; ++i)
            {
                LinkedList<Transform> l = bones[i].getPath(Trans);
                Transform t = createBoneByPath(l);
                ts[i] = t;
            }
            m_skinMeshRender.bones = ts;
#endif
        }

        public void createMesh()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(m_savePath))
            {
                return;
            }
            Transform tran = this.transform;
            MeshEditorPlane[] ps = this.gameObject.GetComponentsInChildren<MeshEditorPlane>(false);
            MeshEditorNode[] ns = this.gameObject.GetComponentsInChildren<MeshEditorNode>(false);
            MeshEditorBone[] bones = null;
            Matrix4x4[] bonePoses = null;


            List<Vector3> ver = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normal = new List<Vector3>();
            List<Color32> col = new List<Color32>();

            List<BoneWeight> weights = new List<BoneWeight>();
            Color32 c = m_color;

            for (int i = 0; i < ns.Length; ++i)
            {
                ns[i].init(i);
                Vector3 v = ns[i].Trans.position;
                v = tran.InverseTransformPoint(v);
                ver.Add(v);
                uv.Add(Vector2.zero);
                if (ns[i].m_customColor)
                {
                    col.Add(ns[i].m_color);
                }
                else
                {
                    col.Add(c);
                }
            }
            if (m_hasNormal)
            {
                for (int i = 0; i < ns.Length; ++i)
                {
                    normal.Add(Vector3.zero);
                }
            }
            if (m_hasBones)
            {
                bones = this.gameObject.GetComponentsInChildren<MeshEditorBone>(false);
                bonePoses = new Matrix4x4[bones.Length];
                for (int i = 0; i < bones.Length; ++i)
                {
                    bones[i].m_index = i;
                    bonePoses[i] = bones[i].transform.worldToLocalMatrix * Trans.localToWorldMatrix;
                }
                for (int i = 0; i < ns.Length; ++i)
                {
                    weights.Add(ns[i].computeWeights());
                }
            }

            List<int> indexL = new List<int>();
            System.Array.Sort(ps, sortPlane);
            int indexOffset = ver.Count;
            for (int i = 0; i < ps.Length; ++i)
            {
                if (ps[i].m_importedMesh != null)
                {
                    Mesh m = ps[i].m_importedMesh.sharedMesh;
                    if (m == null)
                    {
                        continue;
                    }
                    handleImportVertex(m.vertices, ps[i].m_importedMesh.transform, ver);
                    handleImportUV(m.uv, uv);
                    handleImportCol(m.colors32, col);
                    if (m_hasNormal)
                    {
                        handleImportNormal(m.normals, ps[i].m_importedMesh.transform, normal);
                    }
                    int[] tri = m.triangles;
                    for (int j = 0; j < tri.Length; ++j)
                    {
                        indexL.Add(indexOffset + tri[j]);
                    }
                    if (m_hasBones)
                    {
                        handleImportWeight(ps[i].m_planeBone, m.vertices.Length, weights);
                    }
                    indexOffset += m.vertices.Length;
                }
                else
                {
                    ps[i].collectNodes(indexL, uv, m_hasNormal);
                }
            }
            if (m_hasNormal)
            {
                for (int i = 0; i < ns.Length; ++i)
                {
                    normal[i] = ns[i].getNormal();
                }
            }

            Mesh me = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(m_savePath);
            if (me == null)
            {
                me = new Mesh();
                UnityEditor.AssetDatabase.CreateAsset(me, m_savePath);
            }
            me.MarkDynamic();
            me.Clear();
            me.vertices = ver.ToArray();
            me.uv = uv.ToArray();
            me.colors32 = col.ToArray();
            if (m_hasNormal)
            {
                me.normals = normal.ToArray();
            }
            if (m_hasBones)
            {
                me.boneWeights = weights.ToArray();
                me.bindposes = bonePoses;
                createBones(bones);
            }
            me.triangles = indexL.ToArray();
            UnityEditor.EditorUtility.SetDirty(me);
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("LGFW/Editor/MeshEditor", false, (int)'m')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<MeshEditor>(true);
        }
#endif
    }
}
