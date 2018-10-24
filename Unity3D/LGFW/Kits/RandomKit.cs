using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// A kit for random function
    /// </summary>
    public class RandomKit
    {

        private Random.State m_state;
        private Random.State m_cacheState;
        private int m_seed;
        private float m_cacheNormal1;
        private float m_cacheNormal2;
        private bool m_hasInit;
        private bool m_hasCache;
        private bool m_continuous;

        /// <summary>
        /// The seed of random
        /// </summary>
        /// <value>The seed</value>
        public int Seed
        {
            get { return m_seed; }
        }

        /// <summary>
        /// If the kit is in continuous mode. Continuous mode will be faster, suitable for random mass numbers at once
        /// </summary>
        /// <value>If in continuous mode</value>
        public bool Continuous
        {
            get { return m_continuous; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LGFW.RandomKit"/> class.
        /// </summary>
        public RandomKit()
        {
            m_hasInit = false;
            m_hasCache = false;
        }

        /// <summary>
        /// Enables the continuous mode, must call disableContinuous before using unity's Random class
        /// </summary>
        public void enableContinuous()
        {
            if (m_hasInit && !m_continuous)
            {
                m_cacheState = Random.state;
                Random.state = m_state;
            }
        }

        /// <summary>
        /// Disables the continuous mode
        /// </summary>
        public void disableContinuous()
        {
            if (m_continuous)
            {
                m_state = Random.state;
                Random.state = m_cacheState;
            }
        }

        /// <summary>
        /// Sets the seed of random
        /// </summary>
        /// <param name="seed">The seed</param>
        public void setSeed(int seed)
        {
#if UNITY_EDITOR
            if (m_continuous)
            {
                Debug.LogError("can't set seed while enabling continuous");
                return;
            }
#endif
            m_seed = seed;
            m_cacheState = Random.state;
            Random.InitState(seed);
            m_state = Random.state;
            Random.state = m_cacheState;
            m_hasInit = true;
            m_hasCache = false;
            m_continuous = false;
        }

        /// <summary>
        /// Same as unity's Random.range
        /// </summary>
        /// <param name="min">Minimum</param>
        /// <param name="max">Max</param>
        public float range(float min, float max)
        {
            if (!m_hasInit || m_continuous)
            {
                return Random.Range(min, max);
            }
            m_cacheState = Random.state;
            Random.state = m_state;
            float r = Random.Range(min, max);
            m_state = Random.state;
            Random.state = m_cacheState;
            return r;
        }

        /// <summary>
        /// Same as unity's Random.range
        /// </summary>
        /// <param name="min">Minimum</param>
        /// <param name="max">Max</param>
        public int range(int min, int max)
        {
            if (!m_hasInit || m_continuous)
            {
                return Random.Range(min, max);
            }
            m_cacheState = Random.state;
            Random.state = m_state;
            int r = Random.Range(min, max);
            m_state = Random.state;
            Random.state = m_cacheState;
            return r;
        }

        /// <summary>
        /// Randoms a normal distribution number, with the give mean and standard deviation
        /// </summary>
        /// <returns>The number</returns>
        /// <param name="mean">Mean</param>
        /// <param name="sd">Standard deviation</param>
        public float randomNormalDistribution(float mean, float sd)
        {
            bool cache = m_hasInit && !m_continuous && !m_hasCache;
            if (cache)
            {
                m_cacheState = Random.state;
                Random.state = m_state;
            }
            float ret = 0;
            if (m_hasCache)
            {
                ret = m_cacheNormal2 * m_cacheNormal1;
            }
            else
            {
                float c = 0;
                do
                {
                    m_cacheNormal1 = Random.Range(-1.0f, 1.0f);
                    m_cacheNormal2 = Random.Range(-1.0f, 1.0f);
                    c = m_cacheNormal1 * m_cacheNormal1 + m_cacheNormal2 * m_cacheNormal2;
                } while (c == 0 || c >= 1.0f);
                c = Mathf.Sqrt(-2 * Mathf.Log(c) / c);
                ret = m_cacheNormal1 * c;
                m_cacheNormal1 = c;
            }
            m_hasCache = !m_hasCache;
            if (cache)
            {
                m_state = Random.state;
                Random.state = m_cacheState;
            }
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
            bool cache = m_hasInit && !m_continuous;
            if (cache)
            {
                m_cacheState = Random.state;
                Random.state = m_state;
            }
            float min = mean - sd;
            float max = mean + sd;
            float ret = 0;
            for (int i = 0; i < times; ++i)
            {
                ret += Random.Range(min, max);
            }
            if (cache)
            {
                m_state = Random.state;
                Random.state = m_cacheState;
            }
            return ret / times;
        }
    }
}
