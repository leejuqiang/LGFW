using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class MeshExporter : MonoBehaviour
    {

        public string m_path;
        public bool m_hasNormal = true;
        public MeshFilter[] m_filters;

#if UNITY_EDITOR
        private List<Vector3> m_vertexs = new List<Vector3>();
        private List<Vector2> m_uvs = new List<Vector2>();
        private List<Color32> m_colors = new List<Color32>();
        private List<int> m_indexes = new List<int>();
        private List<Vector3> m_normals = new List<Vector3>();

        private void exportOne(Mesh m, Transform t)
        {
            Transform trans = this.transform;
            Vector3[] vs = m.vertices;
            Vector2[] us = m.uv;
            Color32[] cols = m.colors32;
            int[] tri = m.triangles;
            Vector3[] nor = m_hasNormal ? m.normals : null;
            int offset = m_vertexs.Count;
            for (int i = 0; i < vs.Length; ++i)
            {
                Vector3 v = t.TransformPoint(vs[i]);
                v = trans.InverseTransformPoint(v);
                m_vertexs.Add(v);
                m_uvs.Add(us[i]);
                m_colors.Add(cols[i]);
                if (m_hasNormal)
                {
                    m_normals.Add(nor[i]);
                }
            }
            for (int i = 0; i < tri.Length; ++i)
            {
                m_indexes.Add(tri[i] + offset);
            }
        }

        public void export()
        {
            if (string.IsNullOrEmpty(m_path))
            {
                return;
            }
            Mesh me = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(m_path);
            if (me == null)
            {
                me = new Mesh();
                UnityEditor.AssetDatabase.CreateAsset(me, m_path);
            }
            me.Clear();
            m_vertexs.Clear();
            m_uvs.Clear();
            m_colors.Clear();
            m_indexes.Clear();
            m_normals.Clear();

            for (int i = 0; i < m_filters.Length; ++i)
            {
                Mesh m = m_filters[i].sharedMesh;
                if (m != null)
                {
                    exportOne(m, m_filters[i].transform);
                }
            }
            me.vertices = m_vertexs.ToArray();
            me.uv = m_uvs.ToArray();
            me.colors32 = m_colors.ToArray();
            if (m_hasNormal)
            {
                me.normals = m_normals.ToArray();
            }
            me.triangles = m_indexes.ToArray();
            UnityEditor.EditorUtility.SetDirty(me);
        }

        [UnityEditor.MenuItem("LGFW/Editor/MeshExporter", false, (int)'m')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedOjbects<MeshExporter>(false);
        }
#endif
    }
}
