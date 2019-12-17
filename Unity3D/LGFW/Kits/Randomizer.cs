using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A custom randomizer, the random number is m_a * m_seed + m_b
    /// </summary>
    public class Randomizer
    {
        private uint m_a;
        private uint m_b;
        private uint m_mod;
        private ulong m_seed;
        private Random.State m_state;
        private Random.State m_cachedState;
        private float m_cacheNormal1;
        private float m_cacheNormal2;
        private bool m_hasCache;
        private bool m_useUnityRandom;
        private bool m_fastMode;

        /// <summary>
        /// The fast mode. If you use Unity's random, enable this mode to make the generating faster.
        /// In fast mode, the seed of the randomizer won't be cached. So you can't use any other Unity's Random function during the fast mode enabled.
        /// Including another Randomizer using Unity's Random. This doesn't affect the Randomizer not using Unity's Random.
        /// </summary>
        /// <value>If the fast mode is enabled</value>
        public bool FastMode
        {
            get { return m_fastMode; }
            set
            {
                if (m_fastMode != value)
                {
                    m_fastMode = value;
                    if (m_useUnityRandom)
                    {
                        if (m_fastMode)
                        {
                            m_cachedState = Random.state;
                            Random.state = m_state;
                        }
                        else
                        {
                            m_state = Random.state;
                            Random.state = m_cachedState;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Constructor, initial seed is 0
        /// </summary>
        /// <param name="a">A large integer</param>
        /// <param name="b">A large integer</param>
        /// <param name="m">A large integer</param>
        public Randomizer(uint a, uint b, uint m)
        {
            m_seed = 0;
            m_a = a;
            m_b = b;
            m_mod = m;
            m_useUnityRandom = false;
        }

        /// <summary>
        /// Gets the default randomizer. Seed is 0, a is 0xf9f811ab, b is 0xfb8f0fff, m is 0xf48df80f
        /// </summary>
        /// <value>The randomizer</value>
        public static Randomizer Default
        {
            get
            {
                return new Randomizer(0xf9f811ab, 0xfb8f0fff, 0xf48df80f);
            }
        }

        /// <summary>
        /// Constructor, use the Unity's random generator
        /// </summary>
        public Randomizer()
        {
            m_seed = 0;
            m_useUnityRandom = true;
            m_state = Random.state;
        }

        public Randomizer(bool use)
        {

        }

        /// <summary>
        /// Sets the seed
        /// </summary>
        /// <param name="s">The seed</param>
        public void setSeed(int s)
        {
            if (m_useUnityRandom)
            {
                Random.InitState(s);
                m_state = Random.state;
            }
            else
            {
                m_seed = (ulong)s;
            }
        }

        /// <summary>
        /// Gets a random integer between [min, max)
        /// </summary>
        /// <param name="min">The min integer, inclusive</param>
        /// <param name="max">The max integer, exclusive</param>
        /// <returns>The random integer</returns>
        public int range(int min, int max)
        {
            if (m_useUnityRandom)
            {
                if (m_fastMode)
                {
                    return Random.Range(min, max);
                }
                m_cachedState = Random.state;
                Random.state = m_state;
                int ret = Random.Range(min, max);
                m_state = Random.state;
                Random.state = m_cachedState;
                return ret;
            }
            m_seed = (m_a * m_seed + m_b) % m_mod;
            if (min >= max)
            {
                return min;
            }
            ulong r = (ulong)(max - min);
            return min + (int)(m_seed % r);
        }

        /// <summary>
        /// Gets a random float number between [min, max]
        /// </summary>
        /// <param name="min">The min float, inclusive</param>
        /// <param name="max">The max float, inclusive</param>
        /// <returns>The random float</returns>
        public float range(float min, float max)
        {
            if (m_useUnityRandom)
            {
                if (m_fastMode)
                {
                    return Random.Range(min, max);
                }
                m_cachedState = Random.state;
                Random.state = m_state;
                float ret = Random.Range(min, max);
                m_state = Random.state;
                Random.state = m_cachedState;
                return ret;
            }
            m_seed = (m_a * m_seed + m_b) % m_mod;
            if (min >= max)
            {
                return min;
            }
            double r = m_seed / (double)m_mod;
            return Mathf.Lerp(min, max, (float)r);
        }

        /// <summary>
        /// Randoms a normal distribution number, with the give mean and standard deviation
        /// </summary>
        /// <returns>The number</returns>
        /// <param name="mean">Mean</param>
        /// <param name="sd">Standard deviation</param>
        public float randomNormalDistribution(float mean, float sd)
        {
            float ret = 0;
            if (m_hasCache)
            {
                ret = m_cacheNormal2 * m_cacheNormal1;
            }
            else
            {
                bool fast = FastMode;
                FastMode = true;
                float c = 0;
                do
                {
                    m_cacheNormal1 = range(-1.0f, 1.0f);
                    m_cacheNormal2 = range(-1.0f, 1.0f);
                    c = m_cacheNormal1 * m_cacheNormal1 + m_cacheNormal2 * m_cacheNormal2;
                } while (c == 0 || c >= 1.0f);
                if (!fast)
                {
                    FastMode = false;
                }
                c = Mathf.Sqrt(-2 * Mathf.Log(c) / c);
                ret = m_cacheNormal1 * c;
                m_cacheNormal1 = c;
            }
            m_hasCache = !m_hasCache;
            return ret * sd + mean;
        }

        /// <summary>
        /// Random a approximate normal distribution number, but the min will be mean - sd, and the max will be mean + sd
        /// </summary>
        /// <returns>The number</returns>
        /// <param name="mean">Mean</param>
        /// <param name="sd">Standrad deviation</param>
        /// <param name="times">The times of random, the larger number will make it more like normal distribution, but slower</param>
        public float rangeNormalDistribution(float mean, float sd, int times = 3)
        {
            float min = mean - sd;
            float max = mean + sd;
            float ret = 0;
            bool fast = FastMode;
            FastMode = true;
            for (int i = 0; i < times; ++i)
            {
                ret += range(min, max);
            }
            if (!fast)
            {
                FastMode = false;
            }
            return ret / times;
        }

        /// <summary>
        /// Compute the Shannon entropy for integer number generation of the randomizer, you can see if your parameter is good enough comparing to Unity's Random
        /// </summary>
        /// <param name="times">The total amount of random numbers</param>
        /// <param name="max">The range of the random number</param>
        /// <returns>The normalized entropy from 0 to 1, if times <= 0 or max <= 0, return 0</returns>
        public double entropy(int times, int max)
        {
            if (times <= 0 || max <= 0)
            {
                return 0;
            }
            int[] counts = new int[max];
            for (int i = 0; i < times; ++i)
            {
                int r = range(0, max);
                ++counts[r];
            }
            double e = 0;
            double std = -Mathf.Log(1.0f / max);
            for (int i = 0; i < counts.Length; ++i)
            {
                float f = (float)counts[i] / times;
                if (f != 0)
                {
                    e -= f * Mathf.Log(f);
                }
            }
            return e / std;
        }

        /// <summary>
        /// Compute the Shannon entropy for float number generation of the randomizer, you can see if your parameter is good enough comparing to Unity's Random
        /// </summary>
        /// <param name="times">The total amount of random numbers</param>
        /// <param name="bucket">The number of the bucket</param>
        /// <returns>The normalized entropy from 0 to 1, if times <= 0 or bucket <= 1, return 0</returns>
        public double floatEntropy(int times, int bucket)
        {
            if (times <= 0 || bucket <= 1)
            {
                return 0;
            }
            int[] counts = new int[bucket];
            for (int i = 0; i < times; ++i)
            {
                int r = (int)(range(0.0f, 1.0f) * bucket);
                if (r >= counts.Length)
                {
                    --r;
                }
                ++counts[r];
            }
            double e = 0;
            double std = -Mathf.Log(1.0f / counts.Length);
            for (int i = 0; i < counts.Length; ++i)
            {
                float f = (float)counts[i] / times;
                if (f != 0)
                {
                    e -= f * Mathf.Log(f);
                }
            }
            return e / std;
        }
    }
}
