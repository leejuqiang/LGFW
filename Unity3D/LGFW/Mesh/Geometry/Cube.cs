using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace LGFW
{
    /// <summary>
    /// The faces of the cube
    /// </summary>
    public enum CubeFace
    {
        front,
        back,
        left,
        right,
        top,
        bottom,
    }

    /// <summary>
    /// The texture's rotation for a face
    /// </summary>
    public enum CubeFaceRotate
    {
        none,
        left,
        right,
        rotateInvert,
        verticalInvert,
        horizontalInvert,
    }

    /// <summary>
    /// A mesh represents for a cube
    /// </summary>
    [ExecuteInEditMode]
    public class Cube : GeometryMesh
    {

        [SerializeField]
        protected Vector3 m_anchor = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField]
        protected Vector3 m_size;
        [SerializeField]
        protected UIAtlas m_atlas;
        [SerializeField]
        [HideInInspector]
        protected string[] m_sprites = new string[6];
        [SerializeField]
        protected Color[] m_colorsOfFaces = new Color[] { Color.white, Color.white, Color.white, Color.white, Color.white, Color.white };
        [SerializeField]
        protected CubeFaceRotate[] m_faceRotate = new CubeFaceRotate[6];
        protected UIAtlasSprite[] m_atlasSprites = new UIAtlasSprite[6];

        /// <summary>
        /// The anchor of the cube
        /// </summary>
        /// <value>The anchor</value>
        public Vector3 Anchor
        {
            get { return m_anchor; }
            set
            {
                if (m_anchor != value)
                {
                    m_anchor = value;
                    m_flag |= FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The size of the cube
        /// </summary>
        /// <value>The size</value>
        public Vector3 Size
        {
            get { return m_size; }
            set
            {
                if (m_size != value)
                {
                    m_size = value;
                    m_flag |= FLAG_VERTEX;
                }
            }
        }

        /// <summary>
        /// The atlas of this cube
        /// </summary>
        /// <value>The atlas</value>
        public UIAtlas Atlas
        {
            get { return m_atlas; }
            set
            {
                if (m_atlas != value)
                {
                    m_atlas = value;
                    onAtlasChanged();
                }
            }
        }

        protected void onAtlasChanged()
        {
            if (m_atlas != null)
            {
                for (int i = 0; i < m_sprites.Length; ++i)
                {
                    m_atlasSprites[i] = m_atlas.getSprite(m_sprites[i]);
                }
            }
            else
            {
                for (int i = 0; i < m_sprites.Length; ++i)
                {
                    m_atlasSprites[i] = null;
                }
            }
            m_flag |= FLAG_UV;
        }

        protected override void doAwake()
        {
            base.doAwake();
            onAtlasChanged();
        }

        /// <inheritdoc/>
        public override void reset()
        {
            base.reset();
        }

        /// <summary>
        /// Gets the color of a face
        /// </summary>
        /// <returns>The color of the face</returns>
        /// <param name="face">The face</param>
        public Color getColorOfFace(CubeFace face)
        {
            return m_colorsOfFaces[(int)face];
        }

        /// <summary>
        /// Sets the color of a face
        /// </summary>
        /// <param name="c">The color</param>
        /// <param name="face">The face</param>
        public void setColorOfFace(Color c, CubeFace face)
        {
            if (c != m_colorsOfFaces[(int)face])
            {
                m_colorsOfFaces[(int)face] = c;
                m_flag |= FLAG_COLOR;
            }
        }

        /// <summary>
        /// Gets the sprite of a face
        /// </summary>
        /// <returns>The sprite of the face</returns>
        /// <param name="face">The face</param>
        public string getSpriteOfFace(CubeFace face)
        {
            return m_sprites[(int)face];
        }

        /// <summary>
        /// Sets the sprite of a face
        /// </summary>
        /// <param name="sprite">The sprite</param>
        /// <param name="face">The face</param>
        public void setSpriteOfFace(string sprite, CubeFace face)
        {
            if (m_sprites[(int)face] != sprite)
            {
                m_sprites[(int)face] = sprite;
                if (m_atlas != null)
                {
                    m_atlasSprites[(int)face] = m_atlas.getSprite(m_sprites[(int)face]);
                }
                m_flag |= FLAG_UV;
            }
        }

        /// <summary>
        /// Sets a sprite to all faces
        /// </summary>
        /// <param name="sprite">The sprite</param>
        public void setSpriteForAllFace(string sprite)
        {
            for (int i = 0; i < m_sprites.Length; ++i)
            {
                m_sprites[i] = sprite;
            }
            onAtlasChanged();
            m_flag |= FLAG_UV;
        }

        /// <summary>
        /// Sets a color to all faces
        /// </summary>
        /// <param name="c">The color</param>
        public void setColorForAllFace(Color c)
        {
            for (int i = 0; i < m_colorsOfFaces.Length; ++i)
            {
                m_colorsOfFaces[i] = c;
            }
            m_flag |= FLAG_COLOR;
        }

        /// <summary>
        /// Gets the rotation of a face
        /// </summary>
        /// <returns>The rotation of the face</returns>
        /// <param name="f">The face</param>
        public CubeFaceRotate getRotateOfFace(CubeFace f)
        {
            return m_faceRotate[(int)f];
        }

        /// <summary>
        /// Sets the rotation of a face
        /// </summary>
        /// <param name="r">The rotation</param>
        /// <param name="f">The face</param>
        public void setRotateOfFace(CubeFaceRotate r, CubeFace f)
        {
            if (m_faceRotate[(int)f] != r)
            {
                m_faceRotate[(int)f] = r;
                m_flag |= FLAG_UV;
            }
        }

        /// <summary>
        /// Sets a rotation to all faces
        /// </summary>
        /// <param name="r">The rotation</param>
        public void setRotateForAllFace(CubeFaceRotate r)
        {
            for (int i = 0; i < m_faceRotate.Length; ++i)
            {
                m_faceRotate[i] = r;
            }
            m_flag |= FLAG_UV;
        }

        protected Vector3 getLeftBottomFront()
        {
            Vector3 v = Vector3.zero;
            v.x = -m_anchor.x * m_size.x;
            v.y = -m_anchor.y * m_size.y;
            v.z = -m_anchor.z * m_size.z;
            return v;
        }

        protected void addUVByRotate(Rect rc, CubeFaceRotate r)
        {
            if (r == CubeFaceRotate.none)
            {
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
            }
            else if (r == CubeFaceRotate.left)
            {
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
            }
            else if (r == CubeFaceRotate.right)
            {
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
            }
            else if (r == CubeFaceRotate.rotateInvert)
            {
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
            }
            else if (r == CubeFaceRotate.verticalInvert)
            {
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
            }
            else
            {
                m_uvs.Add(new Vector2(rc.xMax, rc.yMin));
                m_uvs.Add(new Vector2(rc.xMax, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMax));
                m_uvs.Add(new Vector2(rc.xMin, rc.yMin));
            }
        }

        protected override void updateUV()
        {
            Rect uv = new Rect(0, 0, 1, 1);
            for (int i = 0; i < m_atlasSprites.Length; ++i)
            {
                if (m_atlasSprites[i] != null)
                {
                    addUVByRotate(m_atlasSprites[i].m_uv, m_faceRotate[i]);
                }
                else
                {
                    addUVByRotate(uv, m_faceRotate[i]);
                }
            }
        }

        protected override void updateColor()
        {
            for (int i = 0; i < m_colorsOfFaces.Length; ++i)
            {
                Color32 c32 = m_colorsOfFaces[i];
                m_colors.Add(c32);
                m_colors.Add(c32);
                m_colors.Add(c32);
                m_colors.Add(c32);
            }
        }

        protected override void updateNormal()
        {
            Vector3 v = new Vector3(0, 0, -1);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);

            v.Set(0, 0, 1);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);

            v.Set(-1, 0, 0);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);

            v.Set(1, 0, 0);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);

            v.Set(0, 1, 0);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);

            v.Set(0, -1, 0);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
            m_normals.Add(v);
        }

        protected override void updateIndex()
        {
            for (int i = 0; i < 6; ++i)
            {
                int index = i * 4;
                m_indexes.Add(index);
                m_indexes.Add(index + 1);
                m_indexes.Add(index + 2);
                m_indexes.Add(index);
                m_indexes.Add(index + 2);
                m_indexes.Add(index + 3);
            }
        }

        protected override void updateVertex()
        {
            Vector3 p = getLeftBottomFront();
            Vector3 p2 = p + m_size;
            m_vertices.Add(p);
            m_vertices.Add(new Vector3(p.x, p2.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p.y, p.z));

            m_vertices.Add(new Vector3(p2.x, p.y, p2.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p.x, p.y, p2.z));

            m_vertices.Add(new Vector3(p.x, p.y, p2.z));
            m_vertices.Add(new Vector3(p.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p.x, p2.y, p.z));
            m_vertices.Add(p);

            m_vertices.Add(new Vector3(p2.x, p.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p2.x, p.y, p2.z));

            m_vertices.Add(new Vector3(p.x, p2.y, p.z));
            m_vertices.Add(new Vector3(p.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p2.z));
            m_vertices.Add(new Vector3(p2.x, p2.y, p.z));

            m_vertices.Add(new Vector3(p.x, p.y, p2.z));
            m_vertices.Add(new Vector3(p.x, p.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p.y, p.z));
            m_vertices.Add(new Vector3(p2.x, p.y, p2.z));
            Texture2D tex = null;
            if (m_atlas != null)
            {
                tex = m_atlas.AtlasTexture;
            }
            if (Renderer.sharedMaterial != null)
            {
                m_render.sharedMaterial.mainTexture = tex;
            }
        }

#if UNITY_EDITOR

        protected T[] resizeArray<T>(T[] arr)
        {
            if (arr.Length < 6)
            {
                T[] temp = new T[6];
                System.Array.Copy(arr, temp, arr.Length);
                for (int i = arr.Length; i < 6; ++i)
                {
                    temp[i] = arr[arr.Length - 1];
                }
                return temp;
            }
            if (arr.Length > 6)
            {
                T[] temp = new T[6];
                System.Array.Copy(arr, temp, 6);
                return temp;
            }
            return arr;
        }

        public override void onEditorChanged()
        {
            m_sprites = resizeArray<string>(m_sprites);
            m_colorsOfFaces = resizeArray<Color>(m_colorsOfFaces);
            m_faceRotate = resizeArray<CubeFaceRotate>(m_faceRotate);
            onAtlasChanged();
        }

        public override void onSelectedSprite(UIAtlasSprite s, int id)
        {
            m_sprites[id] = s == null ? "" : s.m_name;
            m_atlasSprites[id] = s;
            EditorChanged = true;
        }

        public override UIAtlas editorGetAtlas()
        {
            return m_atlas;
        }

        public override void getSelectSprite(List<string> labels, List<UIAtlasSprite> values, List<int> ids)
        {
            for (int i = 0; i < m_atlasSprites.Length; ++i)
            {
                labels.Add(((CubeFace)i).ToString());
                values.Add(m_atlasSprites[i]);
                ids.Add(i);
            }
        }

        [UnityEditor.MenuItem("LGFW/Geometry/Cube", false, (int)'c')]
        public static void addToGameObjects()
        {
            LEditorKits.addComponentToSelectedObjects<Cube>(true);
        }
#endif
    }
}
