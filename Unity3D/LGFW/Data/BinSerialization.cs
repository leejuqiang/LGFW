using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A serialization for binary data
    /// </summary>
    public class BinSerialization
    {

        /// <summary>
        /// The buffer of the binary data
        /// </summary>
        public byte[] m_buffer;
        private int m_offset;

        /// <summary>
        /// The current offset of the pointer in buffer
        /// </summary>
        /// <value>The offset</value>
        public int Offset
        {
            get { return m_offset; }
        }

        /// <summary>
        /// Sets the offset of the pointer in buffer 
        /// </summary>
        /// <param name="offset">The Offset</param>
        public void setOffset(int offset)
        {
            m_offset = offset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.BinSerialization"/> class.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer</param>
        public BinSerialization(int bufferSize)
        {
            m_buffer = new byte[bufferSize];
            m_offset = 0;
        }

        private void checkBuff(int increaseLen)
        {
            int len = m_offset + increaseLen;
            int nL = m_buffer.Length;
            if (len > nL)
            {
                do
                {
                    nL = nL << 1;
                } while (len > nL);
                byte[] temp = new byte[nL];
                System.Array.Copy(m_buffer, temp, m_offset);
                m_buffer = temp;
            }
        }

        /// <summary>
        /// Gets the length of data
        /// </summary>
        /// <value>The length</value>
        public int Length
        {
            get { return m_offset; }
        }

        /// <summary>
        /// Gets the data, this won't return the m_buffer, but will create a new byte array with the data in it
        /// </summary>
        /// <returns>The new array with the data</returns>
        public byte[] getData()
        {
            byte[] b = new byte[m_offset];
            System.Array.Copy(m_buffer, b, m_offset);
            return b;
        }

        /// <summary>
        /// Sets the buffer, if the buffer is too small, it will be resized to fit the data
        /// </summary>
        /// <param name="b">The byte array with the data</param>
        public void setBuffer(byte[] b)
        {
            int len = m_buffer.Length;
            while (len < b.Length)
            {
                len = len << 1;
            }
            if (len > m_buffer.Length)
            {
                m_buffer = new byte[len];
            }
            System.Array.Copy(b, m_buffer, b.Length);
            m_offset = 0;
        }

        /// <summary>
        /// Reset the pointer to the start of the buffer
        /// </summary>
        public void reset()
        {
            m_offset = 0;
        }

        /// <summary>
        /// Gets or sets a float.
        /// </summary>
        /// <value>The float</value>
        public float OneFloat
        {
            get
            {
                float f = System.BitConverter.ToSingle(m_buffer, m_offset);
                m_offset += 4;
                return f;
            }

            set
            {
                byte[] b = System.BitConverter.GetBytes(value);
                writeByteArray(b);
            }
        }

        private void writeByteArray(byte[] b)
        {
            checkBuff(b.Length);
            for (int i = 0; i < b.Length; ++i, ++m_offset)
            {
                m_buffer[m_offset] = b[i];
            }
        }

        /// <summary>
        /// Gets or sets a double.
        /// </summary>
        /// <value>The double</value>
        public double OneDouble
        {
            get
            {
                double d = System.BitConverter.ToDouble(m_buffer, m_offset);
                m_offset += 8;
                return d;
            }

            set
            {
                byte[] b = System.BitConverter.GetBytes(value);
                writeByteArray(b);
            }
        }

        /// <summary>
        /// Gets or sets a byte.
        /// </summary>
        /// <value>The byte</value>
        public byte OneByte
        {
            get
            {
                byte b = m_buffer[m_offset];
                ++m_offset;
                return b;
            }
            set
            {
                checkBuff(1);
                m_buffer[m_offset] = value;
                ++m_offset;
            }
        }

        /// <summary>
        /// Gets or sets a string.
        /// </summary>
        /// <value>The string</value>
        public string OneString
        {
            get
            {
                int start = m_offset;
                if (m_buffer[m_offset] == 0)
                {
                    ++m_offset;
                    return "";
                }
                while (m_buffer[m_offset] != 0)
                {
                    ++m_offset;
                }
                string s = System.Text.Encoding.UTF8.GetString(m_buffer, start, m_offset - start);
                ++m_offset;
                return s;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    byte[] b = System.Text.Encoding.UTF8.GetBytes(value);
                    checkBuff(b.Length + 1);
                    writeByteArray(b);
                }
                m_buffer[m_offset] = 0;
                ++m_offset;
            }
        }

        /// <summary>
        /// Gets or sets a bool
        /// </summary>
        /// <value>The bool</value>
        public bool OneBool
        {
            get
            {
                return OneByte != 0;
            }
            set
            {
                OneByte = (byte)(value ? 1 : 0);
            }
        }

        /// <summary>
        /// Gets or sets a bool list
        /// </summary>
        /// <value>The bool list</value>
        public List<bool> BoolList
        {
            get
            {
                List<bool> l = new List<bool>();
                l.AddRange(BoolArray);
                return l;
            }

            set
            {
                BoolArray = value.ToArray();
            }
        }

        /// <summary>
        /// Gets or sets a bool array
        /// </summary>
        /// <value>The bool array</value>
        public bool[] BoolArray
        {
            get
            {
                byte[] b = ByteArray;
                bool[] ret = new bool[b.Length];
                for (int i = 0; i < ret.Length; ++i)
                {
                    ret[i] = b[i] != 0;
                }
                return ret;
            }

            set
            {
                byte[] b = new byte[value.Length];
                for (int i = 0; i < b.Length; ++i)
                {
                    b[i] = value[i] ? (byte)1 : (byte)0;
                }
                ByteArray = b;
            }
        }

        /// <summary>
        /// Gets or sets a string list
        /// </summary>
        /// <value>The string list</value>
        public List<string> StringList
        {
            get
            {
                int len = OnePackedInt;
                List<string> l = new List<string>();
                for (int i = 0; i < len; ++i)
                {
                    l.Add(OneString);
                }
                return l;
            }

            set
            {
                OnePackedInt = value.Count;
                for (int i = 0; i < value.Count; ++i)
                {
                    OneString = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a string array
        /// </summary>
        /// <value>The string array</value>
        public string[] StringArray
        {
            get
            {
                int len = OnePackedInt;
                string[] ret = new string[len];
                for (int i = 0; i < len; ++i)
                {
                    ret[i] = OneString;
                }
                return ret;
            }

            set
            {
                OnePackedInt = value.Length;
                for (int i = 0; i < value.Length; ++i)
                {
                    OneString = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a int list
        /// </summary>
        /// <value>The int list</value>
        public List<int> IntList
        {
            get
            {
                int len = OnePackedInt;
                List<int> l = new List<int>();
                for (int i = 0; i < len; ++i)
                {
                    l.Add(OnePackedInt);
                }
                return l;
            }

            set
            {
                OnePackedInt = value.Count;
                for (int i = 0; i < value.Count; ++i)
                {
                    OnePackedInt = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a int array
        /// </summary>
        /// <value>The int array</value>
        public int[] IntArray
        {
            get
            {
                int len = OnePackedInt;
                int[] ret = new int[len];
                for (int i = 0; i < len; ++i)
                {
                    ret[i] = OnePackedInt;
                }
                return ret;
            }

            set
            {
                OnePackedInt = value.Length;
                for (int i = 0; i < value.Length; ++i)
                {
                    OnePackedInt = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a float list
        /// </summary>
        /// <value>The float list</value>
        public List<float> FloatList
        {
            get
            {
                int len = OnePackedInt;
                List<float> l = new List<float>();
                for (int i = 0; i < len; ++i)
                {
                    l.Add(OneFloat);
                }
                return l;
            }

            set
            {
                OnePackedInt = value.Count;
                for (int i = 0; i < value.Count; ++i)
                {
                    OneFloat = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a float array
        /// </summary>
        /// <value>The float array</value>
        public float[] FloatArray
        {
            get
            {
                int len = OnePackedInt;
                float[] ret = new float[len];
                for (int i = 0; i < len; ++i)
                {
                    ret[i] = OneFloat;
                }
                return ret;
            }

            set
            {
                OnePackedInt = value.Length;
                for (int i = 0; i < value.Length; ++i)
                {
                    OneFloat = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets a byte list
        /// </summary>
        /// <value>The byte list</value>
        public List<byte> ByteList
        {
            get
            {
                int len = OnePackedInt;
                List<byte> l = new List<byte>();
                for (int i = 0; i < len; ++i)
                {
                    l.Add(OneByte);
                }
                return l;
            }

            set
            {
                OnePackedInt = value.Count;
                writeByteArray(value.ToArray());
            }
        }

        /// <summary>
        /// Gets or sets a byte array
        /// </summary>
        /// <value>The byte array</value>
        public byte[] ByteArray
        {
            get
            {
                int len = OnePackedInt;
                byte[] ret = new byte[len];
                for (int i = 0; i < len; ++i)
                {
                    ret[i] = OneByte;
                }
                return ret;
            }

            set
            {
                OnePackedInt = value.Length;
                writeByteArray(value);
            }
        }

        /// <summary>
        /// Gets or sets a quaternion
        /// </summary>
        /// <value>The quaternion</value>
        public Quaternion OneQuaternion
        {
            get
            {
                Quaternion q = Quaternion.identity;
                q.x = OneFloat;
                q.y = OneFloat;
                q.z = OneFloat;
                q.w = OneFloat;
                return q;
            }

            set
            {
                OneFloat = value.x;
                OneFloat = value.y;
                OneFloat = value.z;
                OneFloat = value.w;
            }
        }

        /// <summary>
        /// Gets or sets a vector3
        /// </summary>
        /// <value>The vector3</value>
        public Vector3 OneVector3
        {
            get
            {
                Vector3 v = Vector3.zero;
                v.x = OneFloat;
                v.y = OneFloat;
                v.z = OneFloat;
                return v;
            }

            set
            {
                OneFloat = value.x;
                OneFloat = value.y;
                OneFloat = value.z;
            }
        }

        /// <summary>
        /// Gets or sets a vector4
        /// </summary>
        /// <value>The vector4</value>
        public Vector4 OneVector4
        {
            get
            {
                Vector4 v = Vector4.zero;
                v.x = OneFloat;
                v.y = OneFloat;
                return v;
            }

            set
            {
                OneFloat = value.x;
                OneFloat = value.y;
            }
        }

        /// <summary>
        /// Gets or sets a vector2
        /// </summary>
        /// <value>The vector2</value>
        public Vector2 OneVector2
        {
            get
            {
                Vector2 v = Vector2.zero;
                v.x = OneFloat;
                v.y = OneFloat;
                return v;
            }

            set
            {
                OneFloat = value.x;
                OneFloat = value.y;
            }
        }

        /// <summary>
        /// Gets or sets a short
        /// </summary>
        /// <value>The short</value>
        public short OneShort
        {
            get
            {
                short s = m_buffer[m_offset];
                s = (short)(s << 8);
                ++m_offset;
                s |= (short)m_buffer[m_offset];
                ++m_offset;
                return s;
            }

            set
            {
                checkBuff(2);
                m_buffer[m_offset] = (byte)(value >> 8);
                ++m_offset;
                m_buffer[m_offset] = (byte)(value & 0xff);
                ++m_offset;
            }
        }

        /// <summary>
        /// Gets or sets a packed int, a packed int will cost at least 2 bytes in memory
        /// </summary>
        /// <value>The one packed int</value>
        public int OnePackedInt
        {
            get
            {
                return (int)OnePackedLong;
            }

            set
            {
                OnePackedLong = value;
            }
        }

        /// <summary>
        /// Gets or sets a packed long, a packed long will cost at least 2 bytes in memory
        /// </summary>
        /// <value>The one packed long</value>
        public long OnePackedLong
        {
            get
            {
                int o = m_buffer[m_offset];
                ++m_offset;
                bool positive = (o & 0xf0) == 0;
                o &= 0xf;
                long ret = 0;
                for (int i = 0, s = 0; i < o; ++i, ++m_offset, s += 8)
                {
                    long b = m_buffer[m_offset];
                    ret |= b << s;
                }
                return positive ? ret : -ret;
            }

            set
            {
                checkBuff(9);
                int o = m_offset;
                m_buffer[o] = 0;
                bool positive = true;
                if (value < 0)
                {
                    value = -value;
                    positive = false;
                }
                ++m_offset;
                while (value != 0)
                {
                    ++m_buffer[o];
                    m_buffer[m_offset] = (byte)(value & 0xff);
                    ++m_offset;
                    value = value >> 8;
                }
                if (!positive)
                {
                    m_buffer[o] = (byte)(m_buffer[o] | 0x1f);
                }
            }
        }

    }
}
