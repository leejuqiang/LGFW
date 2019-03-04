using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum ProfileTimeStamp
    {
        none,
        count,
        timeStamp,
    }

    public enum ProfileValueType
    {
        intType,
        floatType,
        stringType,
        boolType,

        intList,
        floatList,
        stringList,
        boolList,
        byteList,

        custom,
    }

    public class ProfileBase : MonoBehaviour
    {

        public ProfileTimeStamp m_timeStampMode;
        public string m_savePath;
        public bool m_saveAsBinary;
        public bool m_saveWhenExit = true;

        protected int m_version;
        protected Dictionary<string, object> m_dict = new Dictionary<string, object>();
        protected Dictionary<string, long> m_timeStamps = new Dictionary<string, long>();
        protected Dictionary<string, int> m_typeDict = new Dictionary<string, int>();
        protected BinSerialization m_serialization;

        void Awake()
        {
            if (m_saveAsBinary)
            {
                m_serialization = new BinSerialization(1024);
            }
            load();
        }

        void OnApplicationPause(bool pause)
        {
            if (pause && m_saveWhenExit)
            {
                save();
            }
        }

        void OnApplicationQuit()
        {
            if (m_saveWhenExit)
            {
                save();
            }
        }

        protected void changeTimeStamp(string key)
        {
            if (m_timeStampMode == ProfileTimeStamp.none)
            {
                return;
            }
            if (m_timeStampMode == ProfileTimeStamp.count)
            {
                long v = 0;
                m_timeStamps.TryGetValue(key, out v);
                ++v;
                m_timeStamps[key] = v;
            }
            else
            {
                m_timeStamps[key] = LGlobal.Instance.TimeStamp;
            }
        }

        public long getTimeStamp(string key)
        {
            long v = 0;
            m_timeStamps.TryGetValue(key, out v);
            return v;
        }

        public bool hasKey(string key)
        {
            return m_dict.ContainsKey(key);
        }

        public T getValue<T>(string key, T defaultValue)
        {
            object o = null;
            m_dict.TryGetValue(key, out o);
            return o == null ? defaultValue : (T)o;
        }

        public void setValue(string key, object v, ProfileValueType t)
        {
            m_dict[key] = v;
            m_typeDict[key] = (int)t;
            changeTimeStamp(key);
        }

        public void setSafeInt(string key, SafeInt si)
        {
            setValue(key, si.toList(), ProfileValueType.intList);
        }

        public void getSafeInt(string key, ref SafeInt si)
        {
            List<int> l = getValue<List<int>>(key, null);
            si.fromList(l);
        }

        public void setInt(string key, int v)
        {
            setValue(key, v, ProfileValueType.intType);
        }

        public int getInt(string key, int defaultValue = 0)
        {
            return getValue<int>(key, defaultValue);
        }

        public void setIntList(string key, List<int> l)
        {
            setValue(key, l, ProfileValueType.intList);
        }

        public List<int> getIntList(string key)
        {
            return getValue<List<int>>(key, null);
        }

        public void setFloat(string key, float v)
        {
            setValue(key, v, ProfileValueType.floatType);
        }

        public float getFloat(string key, float defaultValue = 0)
        {
            return getValue<float>(key, defaultValue);
        }

        public void setFloatList(string key, List<float> l)
        {
            setValue(key, l, ProfileValueType.floatList);
        }

        public List<float> getFloatList(string key)
        {
            return getValue<List<float>>(key, null);
        }

        public void setString(string key, string v)
        {
            setValue(key, v, ProfileValueType.stringType);
        }

        public string getString(string key, string defaultValue = "")
        {
            return getValue<string>(key, defaultValue);
        }

        public void setBool(string key, bool v)
        {
            setValue(key, v, ProfileValueType.boolType);
        }

        public bool getBool(string key, bool defaultValue = false)
        {
            return getValue<bool>(key, defaultValue);
        }

        public void setBoolList(string key, List<bool> l)
        {
            setValue(key, l, ProfileValueType.boolList);
        }

        public List<bool> getBoolList(string key)
        {
            return getValue<List<bool>>(key, null);
        }

        public void setByteList(string key, List<byte> l)
        {
            setValue(key, l, ProfileValueType.byteList);
        }

        public List<byte> getByteList(string key)
        {
            return getValue<List<byte>>(key, null);
        }

        private void versionToByte(byte[] b)
        {
            b[0] = (byte)(m_version & 0xff);
            b[1] = (byte)((m_version >> 8) & 0xff);
            b[2] = (byte)((m_version >> 16) & 0xff);
            b[3] = (byte)((m_version >> 24) & 0xff);
        }

        private int byteToVersion(byte[] b)
        {
            int v = b[0] | (b[1] << 8) | (b[2] << 16) | (b[3] << 24);
            return v;
        }

        protected virtual void beforeSave()
        {
            //todo
        }

        public virtual void save()
        {
            beforeSave();
            byte[] b = null;
            if (m_saveAsBinary)
            {
                saveBin();
                b = m_serialization.getData();
            }
            else
            {
                Dictionary<string, object> d = new Dictionary<string, object>();
                d["data"] = m_dict;
                d["time"] = m_timeStamps;
                d["type"] = m_typeDict;
                string json = Json.encode(d, true);
                b = System.Text.Encoding.UTF8.GetBytes(json);
            }
            b = encode(b, m_version);
            byte[] temp = new byte[b.Length + 4];
            versionToByte(temp);
            System.Array.Copy(b, 0, temp, 4, b.Length);
            LGFWKit.writeBytesToFile(SavePath, temp);
        }

        protected virtual byte[] encode(byte[] b, int version)
        {
            return b;
        }

        protected virtual byte[] decode(byte[] b, int version)
        {
            return b;
        }

        public void clearProfile()
        {
            m_dict.Clear();
            m_timeStamps.Clear();
            m_typeDict.Clear();
        }

        protected string SavePath
        {
            get
            {
                return LGFWKit.getSavePath() + "/" + m_savePath;
            }
        }

        public virtual void load()
        {
            string path = SavePath;
            if (LGFWKit.fileExists(path))
            {
                loadFromFile(path);
            }
            else
            {
                initProfile();
            }
        }

        protected virtual void initProfile()
        {
            //todo
        }

        protected virtual void afterLoad()
        {
            //todo
        }

        public virtual void loadFromFile(string path)
        {
            byte[] b = LGFWKit.readBytesFromFile(path);
            loadFromBytes(b);
        }

        public virtual void loadFromBytes(byte[] b)
        {
            clearProfile();
            int v = byteToVersion(b);
            byte[] temp = new byte[b.Length - 4];
            System.Array.Copy(b, 4, temp, 0, temp.Length);
            b = decode(temp, v);
            if (m_saveAsBinary)
            {
                m_serialization.setBuffer(b);
                loadBin();
            }
            else
            {
                string json = System.Text.Encoding.UTF8.GetString(b);
                Dictionary<string, object> d = (Dictionary<string, object>)Json.decode(json);
                Dictionary<string, object> td = (Dictionary<string, object>)d["time"];
                foreach (string k in td.Keys)
                {
                    m_timeStamps[k] = (long)td[k];
                }
                td = (Dictionary<string, object>)d["type"];
                Dictionary<string, object> data = (Dictionary<string, object>)d["data"];
                foreach (string k in td.Keys)
                {
                    ProfileValueType t = (ProfileValueType)td[k];
                    m_typeDict[k] = (int)t;
                    m_dict[k] = jsonToObject(data[k], t);
                }
            }
            while (v < m_version)
            {
                updateProfile(v, v + 1);
                ++v;
            }
            afterLoad();
        }

        protected virtual void updateProfile(int oldVersion, int newVersion)
        {
            //todo
        }

        protected List<T> objectToList<T>(object o)
        {
            List<object> lo = (List<object>)o;
            List<T> l = new List<T>();
            for (int i = 0; i < lo.Count; ++i)
            {
                l.Add((T)lo[i]);
            }
            return l;
        }

        protected object jsonToObject(object o, ProfileValueType t)
        {
            switch (t)
            {
                case ProfileValueType.intList:
                    return objectToList<int>(o);
                case ProfileValueType.stringList:
                    return objectToList<string>(o);
                case ProfileValueType.floatList:
                    return objectToList<float>(o);
                case ProfileValueType.boolList:
                    return objectToList<bool>(o);
                case ProfileValueType.byteList:
                    {
                        List<object> lo = (List<object>)o;
                        List<int> l = new List<int>();
                        for (int i = 0; i < lo.Count; ++i)
                        {
                            l.Add((byte)(int)lo[i]);
                        }
                        return l;
                    }
                case ProfileValueType.boolType:
                case ProfileValueType.floatType:
                case ProfileValueType.intType:
                case ProfileValueType.stringType:
                default:
                    return o;
            }
        }

        protected void saveBin()
        {
            m_serialization.reset();
            m_serialization.OnePackedInt = m_dict.Count;
            m_serialization.OneBool = m_timeStampMode != ProfileTimeStamp.none;
            foreach (string key in m_dict.Keys)
            {
                m_serialization.OneString = key;
                ProfileValueType t = (ProfileValueType)m_typeDict[key];
                m_serialization.OneByte = (byte)t;
                if (m_timeStampMode != ProfileTimeStamp.none)
                {
                    m_serialization.OnePackedLong = m_timeStamps[key];
                }
                if (t == ProfileValueType.custom)
                {
                    saveCustomObjectToBin(key, m_dict[key], m_serialization);
                }
                else
                {
                    saveObjectToBin(m_dict[key], t);
                }
            }
        }

        protected void loadBin()
        {
            int len = m_serialization.OnePackedInt;
            bool hasTime = m_serialization.OneBool;
            for (int i = 0; i < len; ++i)
            {
                string key = m_serialization.OneString;
                ProfileValueType t = (ProfileValueType)m_serialization.OneByte;
                m_typeDict[key] = (int)t;
                if (hasTime)
                {
                    m_timeStamps[key] = m_serialization.OnePackedLong;
                }
                object o = null;
                if (t == ProfileValueType.custom)
                {
                    o = loadCustomObjectFromBin(key, m_serialization);
                }
                else
                {
                    o = loadObjectFromBin(t);
                }
                m_dict[key] = o;
            }
        }

        protected virtual object loadCustomObjectFromBin(string k, BinSerialization b)
        {
            return null;
        }

        protected object loadObjectFromBin(ProfileValueType t)
        {
            switch (t)
            {
                case ProfileValueType.intType:
                    return m_serialization.OnePackedInt;
                case ProfileValueType.intList:
                    return m_serialization.IntList;
                case ProfileValueType.floatType:
                    return m_serialization.OneFloat;
                case ProfileValueType.floatList:
                    return m_serialization.FloatList;
                case ProfileValueType.boolType:
                    return m_serialization.OneBool;
                case ProfileValueType.boolList:
                    return m_serialization.BoolList;
                case ProfileValueType.byteList:
                    return m_serialization.ByteList;
                case ProfileValueType.stringType:
                    return m_serialization.OneString;
                case ProfileValueType.stringList:
                    return m_serialization.StringList;
                default:
                    return null;
            }
        }

        protected virtual void saveCustomObjectToBin(string k, object o, BinSerialization b)
        {
            //todo
        }

        protected void saveObjectToBin(object o, ProfileValueType t)
        {
            switch (t)
            {
                case ProfileValueType.intType:
                    m_serialization.OnePackedInt = (int)o;
                    break;
                case ProfileValueType.intList:
                    m_serialization.IntList = (List<int>)o;
                    break;
                case ProfileValueType.floatType:
                    m_serialization.OneFloat = (float)o;
                    break;
                case ProfileValueType.floatList:
                    m_serialization.FloatList = (List<float>)o;
                    break;
                case ProfileValueType.boolType:
                    m_serialization.OneBool = (bool)o;
                    break;
                case ProfileValueType.boolList:
                    m_serialization.BoolList = (List<bool>)o;
                    break;
                case ProfileValueType.byteList:
                    m_serialization.ByteList = (List<byte>)o;
                    break;
                case ProfileValueType.stringType:
                    m_serialization.OneString = (string)o;
                    break;
                case ProfileValueType.stringList:
                    m_serialization.StringList = (List<string>)o;
                    break;
                default:
                    break;
            }
        }
    }
}
