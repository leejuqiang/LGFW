using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace LGFW
{
    public enum UITextCharType
    {
        none,
        character,
        image,
        color,
        effectColor,
        style,
        size,
        newLine,
        wrapWordStart,
        wrapWordEnd,
    }

    public class UITextLine : MemoryPoolItem
    {
        public int m_start;
        public int m_end;
        public float m_top;
        public float m_bottom;
        public float m_width;

        public override void onInit()
        {
            base.onInit();
            m_top = 0;
            m_bottom = 0;
            m_width = 0;
        }
    }

    public class UITextChar : MemoryPoolItem
    {
        public int m_value;
        public UITextCharType m_type;
        public CharacterInfo m_info;
        public bool m_isDefault;

        public bool isWrapWord()
        {
            if (m_type == UITextCharType.character)
            {
                return !LGlobal.isCharacterWrapByWord(m_value);
            }
            if (m_type == UITextCharType.image)
            {
                return true;
            }
            if (m_type == UITextCharType.newLine)
            {
                return true;
            }
            return false;
        }

        public void computeImageSize(UIText t)
        {
            UIImage ti = t.getTextImageByIndex(m_value);
            if (ti == null)
            {
                m_info.minX = 0;
                m_info.minY = 0;
                m_info.maxX = 0;
                m_info.maxY = 0;
            }
            else
            {
                Vector3 lb = ti.LeftBottomInWorldSpace;
                Vector3 rt = ti.RightTopInWorldSpace;
                Vector3 p = ti.Trans.position;
                lb = t.Trans.InverseTransformPoint(lb);
                rt = t.Trans.InverseTransformPoint(rt);
                p = t.Trans.InverseTransformPoint(p);
                m_info.maxX = (int)(rt.x - lb.x);
                m_info.minY = (int)(lb.y - p.y);
                m_info.maxY = (int)(rt.y - p.y);
            }
        }

        public float Width
        {
            get
            {
                if (m_type == UITextCharType.character)
                {
                    return m_info.advance;
                }
                if (m_type == UITextCharType.image)
                {
                    return m_info.maxX;
                }
                return 0;
            }
        }

        public float Top
        {
            get
            {
                if (m_type == UITextCharType.character || m_type == UITextCharType.image)
                {
                    return m_info.maxY;
                }
                return 0;
            }
        }

        public float Bottom
        {
            get
            {
                if (m_type == UITextCharType.character || m_type == UITextCharType.image)
                {
                    return Mathf.Abs(m_info.minY);
                }
                return 0;
            }
        }
    }

    public class UIFontRequestString
    {
        public int m_fontSize;
        public FontStyle m_style;
        public StringBuilder m_string = new StringBuilder();

        public UIFontRequestString(int size, FontStyle style)
        {
            m_style = style;
            m_fontSize = size;
        }
    }

    public class UITextMeshCreator
    {

        public const char SPECIAL_CHAR = '[';
        public const char SPECIAL_CHAR_END = ']';
        public const char WRAP_WORD = '^';

        public const string FORMAT_COLOR = "color";
        public const string FORMAT_SIZE = "size";
        public const string FORMAT_STYLE = "style";
        public const string FORMAT_IAMGE = "image";
        public const string FORMAT_EFFECT_COLOR = "ecolor";

        public List<Vector3> m_vertices = new List<Vector3>();
        public List<Vector2> m_uvs = new List<Vector2>();
        public List<int> m_index = new List<int>();

        private List<UIFontRequestString> m_fontRequestStrings = new List<UIFontRequestString>();
        private FlushMemoryPool m_pool = new FlushMemoryPool();
        private FlushMemoryPool m_linePool = new FlushMemoryPool();

        public Rect m_textArea;
        public string m_displayText;

        private bool isNewLineChar(char c)
        {
            return c == '\r' || c == '\n';
        }

        private string findSpecialCharEnd(string text, ref int index)
        {
            int i = text.IndexOf(SPECIAL_CHAR_END, index + 1);
            if (i >= 0)
            {
                string s = text.Substring(index + 1, i - index - 1);
                index = i;
                return s;
            }
            return null;
        }

        protected int handleSpecialChar(UITextChar ch, int index, string text)
        {
            string s = findSpecialCharEnd(text, ref index);
            if (string.IsNullOrEmpty(s))
            {
                ch.m_type = UITextCharType.none;
                return index;
            }
            ch.m_isDefault = true;
            if (s.StartsWith(FORMAT_COLOR))
            {
                ch.m_type = UITextCharType.color;
                s = s.Substring(FORMAT_COLOR.Length);
                if (s.Length > 0)
                {
                    ch.m_isDefault = false;
                    ch.m_value = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                }
            }
            else if (s.StartsWith(FORMAT_EFFECT_COLOR))
            {
                ch.m_type = UITextCharType.effectColor;
                s = s.Substring(FORMAT_EFFECT_COLOR.Length);
                if (s.Length > 0)
                {
                    ch.m_isDefault = false;
                    ch.m_value = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                }
            }
            else if (s.StartsWith(FORMAT_IAMGE))
            {
                ch.m_type = UITextCharType.image;
                s = s.Substring(FORMAT_IAMGE.Length);
                if (s.Length > 0)
                {
                    ch.m_isDefault = false;
                    ch.m_value = int.Parse(s);
                }
            }
            else if (s.StartsWith(FORMAT_SIZE))
            {
                ch.m_type = UITextCharType.size;
                s = s.Substring(FORMAT_SIZE.Length);
                if (s.Length > 0)
                {
                    ch.m_isDefault = false;
                    ch.m_value = int.Parse(s);
                }
            }
            else if (s.StartsWith(FORMAT_STYLE))
            {
                ch.m_type = UITextCharType.style;
                s = s.Substring(FORMAT_STYLE.Length);
                if (s.Length > 0)
                {
                    ch.m_isDefault = false;
                    ch.m_value = int.Parse(s);
                }
            }
            return index;
        }

        private bool isItASpeicalChar(char ch)
        {
            return ch == SPECIAL_CHAR || ch == WRAP_WORD;
        }

        private void makeChars(string text, bool formatText)
        {
            m_pool.flush(false);
            if (formatText)
            {
                bool isTrans = false;
                bool isWrap = false;
                StringBuilder sb = new StringBuilder();
                bool isSpecial = false;
                for (int i = 0; i < text.Length; ++i)
                {
                    if (isTrans)
                    {
                        isTrans = false;
                        isSpecial = false;
                    }
                    else
                    {
                        if (text[i] == '\\')
                        {
                            isTrans = true;
                            continue;
                        }
                        isSpecial = isItASpeicalChar(text[i]);
                    }
                    UITextChar ch = m_pool.getAnItem<UITextChar>();
                    if (isSpecial)
                    {
                        if (text[i] == SPECIAL_CHAR)
                        {
                            i = handleSpecialChar(ch, i, text);
                        }
                        else if (text[i] == WRAP_WORD)
                        {
                            ch.m_type = isWrap ? UITextCharType.wrapWordEnd : UITextCharType.wrapWordStart;
                            isWrap = !isWrap;
                        }
                    }
                    else
                    {
                        if (isNewLineChar(text[i]))
                        {
                            ch.m_type = UITextCharType.newLine;
                        }
                        else
                        {
                            ch.m_type = UITextCharType.character;
                            ch.m_value = (int)text[i];
                        }
                        sb.Append(text[i]);
                    }
                }
                m_displayText = sb.ToString();
            }
            else
            {
                for (int i = 0; i < text.Length; ++i)
                {
                    UITextChar ch = m_pool.getAnItem<UITextChar>();
                    if (isNewLineChar(text[i]))
                    {
                        ch.m_type = UITextCharType.newLine;
                    }
                    else
                    {
                        ch.m_type = UITextCharType.character;
                        ch.m_value = (int)text[i];
                    }
                }
                m_displayText = text;
            }
        }

        private void requestAllTextsToFont(Font f)
        {
            for (int i = 0; i < m_fontRequestStrings.Count; ++i)
            {
                UIFontRequestString rs = m_fontRequestStrings[i];
                f.RequestCharactersInTexture(rs.m_string.ToString(), rs.m_fontSize, rs.m_style);
            }
        }

        private void addToRequestText(char ch, int size, FontStyle style)
        {
            for (int i = 0; i < m_fontRequestStrings.Count; ++i)
            {
                if (m_fontRequestStrings[i].m_fontSize == size && m_fontRequestStrings[i].m_style == style)
                {
                    m_fontRequestStrings[i].m_string.Append(ch);
                    return;
                }
            }
            UIFontRequestString rs = new UIFontRequestString(size, style);
            rs.m_string.Append(ch);
            m_fontRequestStrings.Add(rs);
        }

        private void requestToFont(bool format, int size, FontStyle style, Font f)
        {
            m_fontRequestStrings.Clear();
            if (format)
            {
                int s = size;
                FontStyle st = style;
                for (int i = 0; i < m_pool.Count; ++i)
                {
                    UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                    if (tc.m_type == UITextCharType.character)
                    {
                        addToRequestText((char)tc.m_value, s, st);
                    }
                    else if (tc.m_type == UITextCharType.size)
                    {
                        if (tc.m_isDefault)
                        {
                            s = size;
                        }
                        else
                        {
                            s = tc.m_value;
                        }
                    }
                    else if (tc.m_type == UITextCharType.style)
                    {
                        if (tc.m_isDefault)
                        {
                            st = style;
                        }
                        else
                        {
                            st = (FontStyle)tc.m_value;
                        }
                    }
                }
                requestAllTextsToFont(f);
            }
            else
            {
                f.RequestCharactersInTexture(m_displayText, size, style);
            }
        }

        public void onFontRebuild(bool format, Font f, int size, FontStyle style)
        {
            if (format)
            {
                requestAllTextsToFont(f);
            }
            else
            {
                f.RequestCharactersInTexture(m_displayText, size, style);
            }
        }

        private void computeSize(UIText t)
        {
            int s = t.FontSize;
            FontStyle st = t.Style;
            for (int i = 0; i < m_pool.Count; ++i)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                if (tc.m_type == UITextCharType.character)
                {
                    t.TextFont.m_font.GetCharacterInfo((char)tc.m_value, out tc.m_info, s, st);
                }
                else if (tc.m_type == UITextCharType.size)
                {
                    if (tc.m_isDefault)
                    {
                        s = t.FontSize;
                    }
                    else
                    {
                        s = tc.m_value;
                    }
                }
                else if (tc.m_type == UITextCharType.style)
                {
                    if (tc.m_isDefault)
                    {
                        st = t.Style;
                    }
                    else
                    {
                        st = (FontStyle)tc.m_value;
                    }
                }
                else if (tc.m_type == UITextCharType.image)
                {
                    tc.computeImageSize(t);
                }
            }
        }

        private void computeLineHeight(UITextLine tl, float minTop, float minBot)
        {
            for (int i = tl.m_start; i <= tl.m_end; ++i)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                float top = tc.Top;
                float bot = tc.Bottom;
                tl.m_top = Mathf.Max(top, tl.m_top);
                tl.m_bottom = Mathf.Max(bot, tl.m_bottom);
            }
            tl.m_top = Mathf.Max(tl.m_top, minTop);
            tl.m_bottom = Mathf.Max(tl.m_bottom, minBot);
        }

        private int findWrapWord(int index, int start, bool isWrapMode, out float width)
        {
            width = 0;
            while (index >= start)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(index);
                if (isWrapMode)
                {
                    if (tc.m_type == UITextCharType.wrapWordStart)
                    {
                        return index;
                    }
                }
                else
                {
                    if (tc.isWrapWord())
                    {
                        return index;
                    }
                }
                --index;
                width += tc.Width;
            }
            return index;
        }

        private void makeLines(UIText t)
        {
            m_linePool.flush(false);
            if (m_pool.Count <= 0)
            {
                return;
            }
            UITextLine tl = m_linePool.getAnItem<UITextLine>();
            tl.m_start = 0;
            bool lineHasWrapWord = true;
            bool wrapMode = false;
            for (int i = 0; i < m_pool.Count; ++i)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                float w = tc.Width;
                tl.m_width += w;
                bool newLine = tc.m_type == UITextCharType.newLine;
                if (t.MaxWidth > 0)
                {
                    if (tc.m_type == UITextCharType.wrapWordEnd)
                    {
                        wrapMode = false;
                    }
                    if (tl.m_width > t.MaxWidth)
                    {
                        if (t.WrapByWord)
                        {
                            float ww = 0;
                            int wrap = tl.m_start;
                            if (lineHasWrapWord)
                            {
                                wrap = findWrapWord(i, tl.m_start, wrapMode, out ww);
                            }
                            else
                            {
                                wrap = (!wrapMode && tc.isWrapWord()) ? i : tl.m_start;
                            }
                            if (wrap > tl.m_start)
                            {
                                tl.m_width -= ww;
                                if (wrap == i)
                                {
                                    tl.m_width -= w;
                                    --i;
                                }
                                else
                                {
                                    i = wrap;
                                }
                                newLine = true;
                            }
                            else
                            {
                                lineHasWrapWord = false;
                            }
                        }
                        else
                        {
                            if (tl.m_start < i)
                            {
                                --i;
                                tl.m_width -= w;
                            }
                            else
                            {
                                i = tl.m_start;
                            }
                            newLine = true;
                        }
                    }
                    if (tc.m_type == UITextCharType.wrapWordStart)
                    {
                        wrapMode = true;
                    }
                }
                tl.m_end = i;
                if (newLine)
                {
                    tl = m_linePool.getAnItem<UITextLine>();
                    tl.m_start = i + 1;
                    lineHasWrapWord = true;
                }
            }
            float minTop = t.FontSize * 0.8f;
            float minBot = t.FontSize * 0.2f;
            for (int i = 0; i < m_linePool.Count; ++i)
            {
                tl = m_linePool.getItemByIndex<UITextLine>(i);
                computeLineHeight(tl, minTop, minBot);
            }
        }

        public void processText(UIText t)
        {
            m_textArea.Set(0, 0, 0, 0);
            makeChars(t.Text, t.FormatText);
            requestToFont(t.FormatText, t.FontSize, t.Style, t.TextFont.m_font);
            computeSize(t);
            makeLines(t);
        }

        public void updateVertex(Rect pos, UIText t)
        {
            m_vertices.Clear();
            Vector3 p = new Vector3(0, pos.yMax, 0);
            Rect rc = new Rect();
            Vector2 effcetO = t.EffectOffset;
            m_textArea.xMin = pos.xMax;
            m_textArea.xMax = pos.xMin;
            m_textArea.yMax = pos.yMax;
            for (int l = 0; l < m_linePool.Count; ++l)
            {
                UITextLine tl = m_linePool.getItemByIndex<UITextLine>(l);
                p.y -= tl.m_top;
                if (t.TextAlignment == UIAlignmentX.left)
                {
                    p.x = pos.xMin;
                }
                else if (t.TextAlignment == UIAlignmentX.center)
                {
                    p.x = pos.xMin + (pos.width - tl.m_width) * 0.5f;
                }
                else
                {
                    p.x = pos.xMax - tl.m_width;
                }
                m_textArea.xMin = Mathf.Min(m_textArea.xMin, p.x);
                m_textArea.xMax = Mathf.Max(m_textArea.xMax, p.x + tl.m_width);
                for (int i = tl.m_start; i <= tl.m_end; ++i)
                {
                    UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                    if (tc.m_type == UITextCharType.character)
                    {
                        rc.xMin = p.x + tc.m_info.minX;
                        rc.xMax = p.x + tc.m_info.maxX;
                        rc.yMin = p.y + tc.m_info.minY;
                        rc.yMax = p.y + tc.m_info.maxY;

                        if (t.Effect == UITextEffect.shadow)
                        {
                            m_vertices.Add(new Vector3(rc.xMin + effcetO.x, rc.yMin + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMin + effcetO.x, rc.yMax + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax + effcetO.x, rc.yMax + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax + effcetO.x, rc.yMin + effcetO.y, 0));
                        }
                        else if (t.Effect == UITextEffect.outline)
                        {
                            m_vertices.Add(new Vector3(rc.xMin - effcetO.x, rc.yMin, 0));
                            m_vertices.Add(new Vector3(rc.xMin - effcetO.x, rc.yMax, 0));
                            m_vertices.Add(new Vector3(rc.xMax - effcetO.x, rc.yMax, 0));
                            m_vertices.Add(new Vector3(rc.xMax - effcetO.x, rc.yMin, 0));

                            m_vertices.Add(new Vector3(rc.xMin + effcetO.x, rc.yMin, 0));
                            m_vertices.Add(new Vector3(rc.xMin + effcetO.x, rc.yMax, 0));
                            m_vertices.Add(new Vector3(rc.xMax + effcetO.x, rc.yMax, 0));
                            m_vertices.Add(new Vector3(rc.xMax + effcetO.x, rc.yMin, 0));

                            m_vertices.Add(new Vector3(rc.xMin, rc.yMin - effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMin, rc.yMax - effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax, rc.yMax - effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax, rc.yMin - effcetO.y, 0));

                            m_vertices.Add(new Vector3(rc.xMin, rc.yMin + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMin, rc.yMax + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax, rc.yMax + effcetO.y, 0));
                            m_vertices.Add(new Vector3(rc.xMax, rc.yMin + effcetO.y, 0));
                        }

                        m_vertices.Add(new Vector3(rc.xMin, rc.yMin, 0));
                        m_vertices.Add(new Vector3(rc.xMin, rc.yMax, 0));
                        m_vertices.Add(new Vector3(rc.xMax, rc.yMax, 0));
                        m_vertices.Add(new Vector3(rc.xMax, rc.yMin, 0));
                    }
                    else if (tc.m_type == UITextCharType.image)
                    {
                        UIImage ti = t.getTextImageByIndex(tc.m_value);
                        if (ti != null)
                        {
                            Vector3 ipos = new Vector3(p.x, p.y + tc.m_info.minY, 0);
                            ipos = t.Trans.TransformPoint(ipos);
                            ti.setLeftBottomInWorldSpace(ipos, false);
                        }
                    }
                    p.x += tc.Width;
                }
                p.y -= tl.m_bottom;
                p.y -= t.LineSpace;
            }
            m_textArea.yMin = p.y + t.LineSpace;
        }

        public void updateUV(UIText t)
        {
            m_uvs.Clear();
            int count = 0;
            if (t.Effect == UITextEffect.shadow)
            {
                count = 4;
            }
            else if (t.Effect == UITextEffect.outline)
            {
                count = 16;
            }
            for (int i = 0; i < m_pool.Count; ++i)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                if (tc.m_type == UITextCharType.character)
                {
                    m_uvs.Add(tc.m_info.uvBottomLeft);
                    m_uvs.Add(tc.m_info.uvTopLeft);
                    m_uvs.Add(tc.m_info.uvTopRight);
                    m_uvs.Add(tc.m_info.uvBottomRight);
                    for (int j = 0; j < count; ++j)
                    {
                        m_uvs.Add(m_uvs[m_uvs.Count - 4]);
                    }
                }
            }
        }

        private Color intToColor(int i)
        {
            Color c = Color.white;
            c.r = ((i >> 24) & 0xff) / 255.0f;
            c.g = ((i >> 16) & 0xff) / 255.0f;
            c.b = ((i >> 8) & 0xff) / 255.0f;
            c.a = (i & 0xff) / 255.0f;
            return c;
        }

        public Color32[] updateColors(Color textColor, Color effectColor, UIText t)
        {
            List<Color32> l = new List<Color32>();
            Color32 c = textColor;
            Color32 ec = effectColor;
            for (int i = 0; i < m_pool.Count; ++i)
            {
                UITextChar tc = m_pool.getItemByIndex<UITextChar>(i);
                if (tc.m_type == UITextCharType.character)
                {
                    if (t.Effect == UITextEffect.outline)
                    {
                        for (int j = 0; j < 16; ++j)
                        {
                            l.Add(ec);
                        }
                    }
                    else if (t.Effect == UITextEffect.shadow)
                    {
                        for (int j = 0; j < 4; ++j)
                        {
                            l.Add(ec);
                        }
                    }
                    for (int j = 0; j < 4; ++j)
                    {
                        l.Add(c);
                    }
                }
                else if (tc.m_type == UITextCharType.color)
                {
                    if (tc.m_isDefault)
                    {
                        textColor = t.CurrentColor;
                    }
                    else
                    {
                        textColor = intToColor(tc.m_value);
                    }
                    textColor.a *= t.Alpha;
                    c = textColor;
                }
                else if (tc.m_type == UITextCharType.effectColor)
                {
                    if (tc.m_isDefault)
                    {
                        effectColor = t.EffectColor;
                    }
                    else
                    {
                        effectColor = intToColor(tc.m_value);
                    }
                    effectColor *= t.Alpha;
                    ec = effectColor;
                }
            }
            return l.ToArray();
        }

        public void updateIndex()
        {
            m_index.Clear();
            for (int i = 0; i < m_vertices.Count; i += 4)
            {
                m_index.Add(i);
                m_index.Add(i + 1);
                m_index.Add(i + 2);
                m_index.Add(i);
                m_index.Add(i + 2);
                m_index.Add(i + 3);
            }
        }
    }
}
