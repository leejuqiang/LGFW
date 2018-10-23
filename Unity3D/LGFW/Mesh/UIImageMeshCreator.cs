using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class UIImageMeshCreator
    {

        public List<Vector3> m_vertices = new List<Vector3>();
        public List<Vector2> m_uvs = new List<Vector2>();
        public List<int> m_indexes = new List<int>();

        private int m_radarLine;
        private float m_raderFillValue;
        private List<float> m_sliceWebX = new List<float>();
        private List<float> m_sliceWebY = new List<float>();

        public void updateVertexNormal(Rect pos)
        {
            m_vertices.Clear();
            m_vertices.Add(new Vector3(pos.xMin, pos.yMin, 0));
            m_vertices.Add(new Vector3(pos.xMin, pos.yMax, 0));
            m_vertices.Add(new Vector3(pos.xMax, pos.yMax, 0));
            m_vertices.Add(new Vector3(pos.xMax, pos.yMin, 0));
        }

        public void makeSliceWeb(Rect pos, Rect innerPos, Rect uv, Rect innerUV)
        {
            m_sliceWebX.Clear();
            m_sliceWebX.Add(pos.xMin);
            m_sliceWebX.Add(uv.xMin);
            if (pos.xMin != innerPos.xMin && uv.xMin != innerUV.xMin)
            {
                m_sliceWebX.Add(innerPos.xMin);
                m_sliceWebX.Add(innerUV.xMin);
            }
            if (pos.xMax != innerPos.xMax && uv.xMax != innerUV.xMax)
            {
                m_sliceWebX.Add(innerPos.xMax);
                m_sliceWebX.Add(innerUV.xMax);
            }
            m_sliceWebX.Add(pos.xMax);
            m_sliceWebX.Add(uv.xMax);

            m_sliceWebY.Clear();
            m_sliceWebY.Add(pos.yMin);
            m_sliceWebY.Add(uv.yMin);
            if (pos.yMin != innerPos.yMin && uv.yMin != innerUV.yMin)
            {
                m_sliceWebY.Add(innerPos.yMin);
                m_sliceWebY.Add(innerUV.yMin);
            }
            if (pos.yMax != innerPos.yMax && uv.yMax != innerUV.yMax)
            {
                m_sliceWebY.Add(innerPos.xMax);
                m_sliceWebY.Add(innerUV.yMax);
            }
            m_sliceWebY.Add(pos.yMax);
            m_sliceWebY.Add(uv.yMax);
        }

        public void updateVertexWeb(Rect pos, int row, int column)
        {
            m_vertices.Clear();
            Vector2 step = new Vector2(pos.width / column, pos.height / row);
            float sx = pos.xMin;
            Vector3 v = new Vector3(sx, pos.yMin, 0);

            for (int x = -1; x < row; ++x)
            {
                for (int y = -1; y < column; ++y)
                {
                    m_vertices.Add(new Vector3(v.x, v.y, 0));
                    v.x += step.x;
                }
                v.x = sx;
                v.y += step.y;
            }
        }

        public void updateVertexSlice()
        {
            m_vertices.Clear();
            for (int i = 0; i < m_sliceWebY.Count; i += 2)
            {
                for (int j = 0; j < m_sliceWebX.Count; j += 2)
                {
                    m_vertices.Add(new Vector3(m_sliceWebX[j], m_sliceWebY[i], 0));
                }
            }
        }

        public void updateVertexRadar(Rect pos, float v, bool clockwise)
        {
            m_radarLine = 0;
            m_vertices.Clear();
            if (v <= 0)
            {
                m_raderFillValue = 0;
                return;
            }
            if (v >= 1)
            {
                updateVertexNormal(pos);
                m_raderFillValue = 1;
                return;
            }
            Vector3 c = pos.center;
            Vector3 cut = Vector3.zero;
            m_vertices.Add(c);
            float w = pos.width * 0.5f;
            float h = pos.height * 0.5f;
            float tempV = clockwise ? v : (1 - v);
            float t = Mathf.Abs(Mathf.Tan(Mathf.PI * 2 * tempV));
            float k = w / h;
            float ret = 0;

            if (tempV <= 0.25f)
            {
                if (t <= k)
                {
                    m_radarLine = 0;
                    cut.y = pos.yMax;
                    ret = h * t;
                    cut.x = ret + c.x;
                    ret /= w;
                }
                else
                {
                    m_radarLine = 1;
                    cut.x = pos.xMax;
                    ret = w / t;
                    cut.y = ret + c.y;
                    ret = 1 - ret / h;
                }
            }
            else if (tempV <= 0.5f)
            {
                if (t <= k)
                {
                    m_radarLine = 3;
                    cut.y = pos.yMin;
                    ret = h * t;
                    cut.x = ret + c.x;
                    ret = 1 - ret / w;
                }
                else
                {
                    m_radarLine = 2;
                    cut.x = pos.yMax;
                    ret = w / t;
                    cut.y = c.y - ret;
                    ret /= h;
                }
            }
            else if (tempV <= 0.75f)
            {
                if (t <= k)
                {
                    m_radarLine = 4;
                    cut.y = pos.yMin;
                    ret = h * t;
                    cut.x = c.x - ret;
                    ret /= w;
                }
                else
                {
                    m_radarLine = 5;
                    cut.x = pos.xMin;
                    ret = w / t;
                    cut.y = c.y - ret;
                    ret = 1 - ret / h;
                }
            }
            else
            {
                if (t <= k)
                {
                    m_radarLine = 7;
                    cut.y = pos.yMax;
                    ret = h * t;
                    cut.x = c.x - ret;
                    ret = 1 - ret / w;
                }
                else
                {
                    m_radarLine = 6;
                    cut.x = pos.xMin;
                    ret = w / t;
                    cut.y = c.y + ret;
                    ret /= h;
                }
            }
            m_vertices.Add(new Vector3(c.x, pos.yMax, 0));
            if (clockwise)
            {
                if (m_radarLine > 0)
                {
                    m_vertices.Add(new Vector3(pos.xMax, pos.yMax, 0));
                    if (m_radarLine > 2)
                    {
                        m_vertices.Add(new Vector3(pos.xMax, pos.yMin, 0));
                        if (m_radarLine > 4)
                        {
                            m_vertices.Add(new Vector3(pos.xMin, pos.yMin, 0));
                            if (m_radarLine > 6)
                            {
                                m_vertices.Add(new Vector3(pos.xMin, pos.yMax, 0));
                            }
                        }
                    }
                }
            }
            else
            {
                if (m_radarLine < 7)
                {
                    m_vertices.Add(new Vector3(pos.xMin, pos.yMax, 0));
                    if (m_radarLine < 5)
                    {
                        m_vertices.Add(new Vector3(pos.xMin, pos.yMin, 0));
                        if (m_radarLine < 3)
                        {
                            m_vertices.Add(new Vector3(pos.xMax, pos.yMin, 0));
                            if (m_radarLine < 1)
                            {
                                m_vertices.Add(new Vector3(pos.xMax, pos.yMax, 0));
                            }
                        }
                    }
                }
            }
            m_vertices.Add(cut);
            m_raderFillValue = ret;
        }

        public void updateVertexTile(Rect pos, int row, int column)
        {
            m_vertices.Clear();
            Vector2 lb = new Vector2(pos.xMin, pos.yMin);
            Vector2 step = new Vector2(pos.width / column, pos.height / row);
            Vector2 rt = new Vector2(lb.x + step.x, lb.y + step.y);
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    m_vertices.Add(new Vector3(lb.x, lb.y, 0));
                    m_vertices.Add(new Vector3(lb.x, rt.y, 0));
                    m_vertices.Add(new Vector3(rt.x, rt.y, 0));
                    m_vertices.Add(new Vector3(rt.x, lb.y, 0));
                    lb.x = rt.x;
                    rt.x += step.x;
                }
                lb.x = pos.xMin;
                rt.x = lb.x + step.x;
                lb.y = rt.y;
                rt.y += step.y;
            }
        }

        public void updateUVNormal(Rect uv)
        {
            m_uvs.Clear();
            m_uvs.Add(new Vector2(uv.xMin, uv.yMin));
            m_uvs.Add(new Vector2(uv.xMin, uv.yMax));
            m_uvs.Add(new Vector2(uv.xMax, uv.yMax));
            m_uvs.Add(new Vector2(uv.xMax, uv.yMin));
        }

        public void updateUVWeb(Rect uv, int row, int column)
        {
            m_uvs.Clear();
            Vector2 step = new Vector2(uv.width / column, uv.height / row);
            float sx = uv.xMin;
            Vector2 v = new Vector2(sx, uv.yMin);

            for (int x = -1; x < row; ++x)
            {
                for (int y = -1; y < column; ++y)
                {
                    m_uvs.Add(v);
                    v.x += step.x;
                }
                v.x = sx;
                v.y += step.y;
            }
        }

        public void updateUVSlice()
        {
            m_uvs.Clear();
            for (int i = 1; i < m_sliceWebY.Count; i += 2)
            {
                for (int j = 1; j < m_sliceWebX.Count; j += 2)
                {
                    m_uvs.Add(new Vector2(m_sliceWebX[j], m_sliceWebY[i]));
                }
            }
        }

        public void updateUVRadar(Rect uv, bool clockwise)
        {
            m_uvs.Clear();
            if (m_raderFillValue <= 0)
            {
                return;
            }
            if (m_raderFillValue >= 1)
            {
                updateUVNormal(uv);
                return;
            }
            Vector2 c = uv.center;
            m_uvs.Add(c);
            m_uvs.Add(new Vector2(c.x, uv.yMax));
            if (clockwise)
            {
                if (m_radarLine > 0)
                {
                    m_uvs.Add(new Vector2(uv.xMax, uv.yMax));
                    if (m_radarLine > 2)
                    {
                        m_uvs.Add(new Vector2(uv.xMax, uv.yMin));
                        if (m_radarLine > 4)
                        {
                            m_uvs.Add(new Vector2(uv.xMin, uv.yMin));
                            if (m_radarLine > 6)
                            {
                                m_uvs.Add(new Vector2(uv.xMin, uv.yMax));
                            }
                        }
                    }
                }
            }
            else
            {
                if (m_radarLine < 7)
                {
                    m_uvs.Add(new Vector2(uv.xMin, uv.yMax));
                    if (m_radarLine < 5)
                    {
                        m_uvs.Add(new Vector2(uv.xMin, uv.yMin));
                        if (m_radarLine < 3)
                        {
                            m_uvs.Add(new Vector2(uv.xMax, uv.yMin));
                            if (m_radarLine < 1)
                            {
                                m_uvs.Add(new Vector2(uv.xMax, uv.yMax));
                            }
                        }
                    }
                }
            }
            Vector2 s = Vector2.zero;
            Vector2 e = Vector2.zero;
            switch (m_radarLine)
            {
                case 0:
                    s.x = c.x;
                    s.y = uv.yMax;
                    e.x = uv.xMax;
                    e.y = s.y;
                    break;
                case 1:
                    s.x = uv.xMax;
                    s.y = uv.yMax;
                    e.x = s.x;
                    e.y = c.y;
                    break;
                case 2:
                    s.x = uv.xMax;
                    s.y = c.y;
                    e.x = s.x;
                    e.y = uv.yMin;
                    break;
                case 3:
                    s.x = uv.xMax;
                    s.y = uv.yMin;
                    e.x = c.x;
                    e.y = s.y;
                    break;
                case 4:
                    s.x = c.x;
                    s.y = uv.yMin;
                    e.x = uv.xMin;
                    e.y = s.y;
                    break;
                case 5:
                    s.x = uv.xMin;
                    s.y = uv.yMin;
                    e.x = s.x;
                    e.y = c.y;
                    break;
                case 6:
                    s.x = uv.xMin;
                    s.y = c.y;
                    e.x = s.x;
                    e.y = uv.yMax;
                    break;
                case 7:
                    s.x = uv.xMin;
                    s.y = uv.yMax;
                    e.x = c.x;
                    e.y = s.y;
                    break;
                default:
                    break;
            }
            m_uvs.Add(Vector2.Lerp(s, e, m_raderFillValue));
        }

        public void updateUVTile(Rect uv, int row, int column)
        {
            m_uvs.Clear();
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    m_uvs.Add(new Vector2(uv.xMin, uv.yMin));
                    m_uvs.Add(new Vector2(uv.xMin, uv.yMax));
                    m_uvs.Add(new Vector2(uv.xMax, uv.yMax));
                    m_uvs.Add(new Vector2(uv.xMax, uv.yMin));
                }
            }
        }

        public Color32[] updateColor(Color c)
        {
            Color32 c32 = c;
            Color32[] ret = new Color32[m_vertices.Count];
            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = c32;
            }
            return ret;
        }

        public void updateIndexNormal()
        {
            m_indexes.Clear();
            m_indexes.Add(0);
            m_indexes.Add(1);
            m_indexes.Add(2);
            m_indexes.Add(0);
            m_indexes.Add(2);
            m_indexes.Add(3);
        }

        public void updateIndexWeb(int row, int column)
        {
            int len = column + 1;
            m_indexes.Clear();
            for (int i = 0; i < row; ++i)
            {
                for (int j = 0; j < column; ++j)
                {
                    int s = j + i * len;
                    m_indexes.Add(s);
                    m_indexes.Add(s + len);
                    m_indexes.Add(s + len + 1);
                    m_indexes.Add(s);
                    m_indexes.Add(s + len + 1);
                    m_indexes.Add(s + 1);
                }
            }
        }

        public void updateIndexSlice()
        {
            m_indexes.Clear();
            int c = m_sliceWebX.Count / 2;
            int r = m_sliceWebY.Count / 2;
            int y = r - 1;
            int x = c - 1;
            for (int i = 0; i < y; ++i)
            {
                for (int j = 0; j < x; ++j)
                {
                    int s = j + i * c;
                    int s1 = s + c;
                    m_indexes.Add(s);
                    m_indexes.Add(s1);
                    m_indexes.Add(s1 + 1);
                    m_indexes.Add(s);
                    m_indexes.Add(s1 + 1);
                    m_indexes.Add(s + 1);
                }
            }
        }

        public void updateIndexRadar(int len, bool clockwise)
        {
            m_indexes.Clear();
            if (len == 0)
            {
                return;
            }
            if (len < 0)
            {
                updateIndexNormal();
            }
            len -= 1;
            if (clockwise)
            {
                for (int i = 1; i < len; ++i)
                {
                    m_indexes.Add(0);
                    m_indexes.Add(i);
                    m_indexes.Add(i + 1);
                }
            }
            else
            {
                for (int i = 1; i < len; ++i)
                {
                    m_indexes.Add(0);
                    m_indexes.Add(i + 1);
                    m_indexes.Add(i);
                }
            }
        }

        public void updateIndexTile(int row, int column)
        {
            m_indexes.Clear();
            int len = row * column * 4;
            for (int i = 0; i < len; i += 4)
            {
                m_indexes.Add(i);
                m_indexes.Add(i + 1);
                m_indexes.Add(i + 2);
                m_indexes.Add(i);
                m_indexes.Add(i + 2);
                m_indexes.Add(i + 3);
            }
        }
    }
}
