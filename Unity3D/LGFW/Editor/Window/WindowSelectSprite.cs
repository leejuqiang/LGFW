using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LGFW
{
    public class WindowSelectSprite : WindowSelectItem
    {

        private const float m_spriteSize = 100;
        private const float m_cellSizeX = 110;
        private const float m_cellSizeY = 130;
        private const float m_scrollViewPosX = 5;
        private const float m_scrollViewPosY = 25;
        private const float m_searchLabelWidth = 50;

        private UIAtlas m_atlas;
        private Vector2 m_scrollPos;
        private string m_searchText;

        private static WindowSelectSprite m_instance;

        public static void showWindow(UIAtlas atlas, OnSelectItem callback, params object[] dataList)
        {
            if (m_instance == null)
            {
                m_instance = EditorWindow.GetWindow<WindowSelectSprite>(true, "Select a Sprite");
            }
            m_instance.open(callback, dataList);
            m_instance.m_atlas = atlas;
            m_instance.Show();
        }

        protected override void open(OnSelectItem callback, object[] dataList)
        {
            base.open(callback, dataList);
            m_scrollPos = Vector2.zero;
            m_searchText = "";
        }

        public static void close()
        {
            if (m_instance != null)
            {
                m_instance.Close();
            }
        }

        protected override void onClose()
        {
            base.onClose();
            m_instance = null;
            m_atlas = null;
        }

        protected Rect getImageRect(UIAtlasSprite uas, float x, float y)
        {
            Rect rc = new Rect(x, y, m_spriteSize, m_spriteSize);
            float k = uas.m_uv.width * uas.m_atlas.m_material.mainTexture.width / (uas.m_uv.height * uas.m_atlas.m_material.mainTexture.height);
            if (k >= 1)
            {
                rc.height = m_spriteSize / k;
                rc.y += (m_spriteSize - rc.height) * 0.5f;
            }
            else
            {
                rc.width = m_spriteSize * k;
                rc.x += (m_spriteSize - rc.width) * 0.5f;
            }
            return rc;
        }

        protected Rect getLabelRect(float x, float y)
        {
            Rect rc = new Rect(x, y + m_spriteSize, m_spriteSize, m_cellSizeY - m_spriteSize);
            return rc;
        }

        private bool matchSearch(string name)
        {
            if (string.IsNullOrEmpty(m_searchText))
            {
                return true;
            }
            return name.Contains(m_searchText);
        }

        public void OnGUI()
        {
            if (m_atlas == null || m_atlas.m_material == null || m_atlas.m_material.mainTexture == null)
            {
                return;
            }
            UIAtlasSprite selectSprite = null;
            Rect searchRc = new Rect(0, 0, m_searchLabelWidth, m_scrollViewPosY - 5);
            GUI.Label(searchRc, "Search");
            searchRc.xMin = m_searchLabelWidth + 10;
            searchRc.xMax = position.width;
            m_searchText = GUI.TextField(searchRc, m_searchText);
            Rect rc = new Rect(m_scrollViewPosX, m_scrollViewPosY, this.position.width - m_scrollViewPosX, this.position.height - m_scrollViewPosY);
            int height = (int)(rc.width / m_cellSizeX);
            int row = m_atlas.SpriteCount / height;
            if (height * row < m_atlas.SpriteCount)
            {
                ++row;
            }
            height = (int)(row * m_cellSizeY);
            Rect viewRc = new Rect(0, 0, rc.width, height);
            m_scrollPos = GUI.BeginScrollView(rc, m_scrollPos, viewRc);
            float x = 0;
            float y = 0;
            for (int i = 0; i < m_atlas.SpriteCount; ++i)
            {
                UIAtlasSprite uas = m_atlas.getSpriteByIndex(i);
                if (!matchSearch(uas.m_name))
                {
                    continue;
                }
                Rect imageRc = new Rect(x, y, m_spriteSize, m_spriteSize);
                if (GUI.Button(imageRc, ""))
                {
                    selectSprite = uas;
                }
                imageRc = InspectorUIAtlas.guiDrawSprite(imageRc, uas.m_atlas, uas);
                Rect labelRc = getLabelRect(x, y);
                GUI.Label(labelRc, uas.m_name);

                x += m_cellSizeX;
                if (x + m_cellSizeX > rc.width)
                {
                    x = 0;
                    y += m_cellSizeY;
                }
            }
            GUI.EndScrollView();
            if (selectSprite != null)
            {
                m_selectCallback(selectSprite, m_customMessage);
                this.Close();
            }
        }
    }
}
