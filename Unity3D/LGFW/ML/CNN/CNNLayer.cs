using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public enum CNNLayerType
    {
        sigmoidFilter = -1,
        reluFilter = -2,
        maxPooling = 1,
        averagePooling = 2,
    }

    /// <summary>
    /// The base class for a configuration
    /// </summary>
    public class CNNLayerConfig
    {
        /// <summary>
        /// The type of this layer
        /// </summary>
        public int m_type;
    }

    /// <summary>
    /// A filter layer configration
    /// </summary>
    [System.Serializable]
    public class CNNFilterLayerConfig : CNNLayerConfig
    {
        /// <summary>
        /// Filter count
        /// </summary>
        public int m_filterCount = 3;
        /// <summary>
        /// The filter's weights' size
        /// </summary>
        public Vector2Int m_filterStride = new Vector2Int(3, 3);
        /// <summary>
        /// The step of filter moving
        /// </summary>
        public Vector2Int m_moveStep = Vector2Int.one;
        /// <summary>
        /// If true, the filter won't move out of the input area
        /// </summary>
        public bool m_stayInside = true;

        /// <summary>
        /// If it's a multistep filter layer
        /// </summary>
        /// <returns></returns>
        public bool isMultiStep()
        {
            return !m_stayInside || m_moveStep != Vector2Int.one;
        }

        public CNNFilterLayerConfig(CNNLayerType type, int filterCount, Vector2Int stride)
        {
            m_type = (int)type;
            m_filterCount = filterCount;
            m_filterStride = stride;
            m_moveStep = Vector2Int.one;
            m_stayInside = true;
        }
    }

    /// <summary>
    /// Configuration for pooling layer
    /// </summary>
    [System.Serializable]
    public class CNNPoolLayerConfig : CNNLayerConfig
    {
        /// <summary>
        /// The pooling size for a block
        /// </summary>
        public Vector2Int m_poolStride = new Vector2Int(2, 2);

        public CNNPoolLayerConfig(CNNLayerType type, Vector2Int stride)
        {
            m_type = (int)type;
            m_poolStride = stride;
        }
    }

    public abstract class CNNLayer
    {

        public CNNLayer m_previous;
        public CNNLayer m_next;
        public CNNBPCache m_bpCache;

        public CNNDualArray m_output;

        public abstract CNNDualArray output(CNNDualArray input);

        public virtual void randomParams(RandomKit k)
        {
        }

        public int totalOutputSize()
        {
            return m_output.m_array.Length;
        }

        public virtual void clearBP()
        {
        }

        public virtual void setTrainMode(bool isTraining)
        {
            if (isTraining)
            {
                m_bpCache = new CNNBPCache();
                m_bpCache.m_output = m_output;
                int inputLen = 0;
                int inputNum = 0;
                if (m_previous != null)
                {
                    inputNum = m_previous.m_output.m_number;
                    inputLen = m_previous.m_output.m_length;
                }
                m_bpCache.init(inputNum, inputLen);
            }
            else
            {
                m_bpCache = null;
            }
        }

        public virtual void applyGD(double rate)
        {
        }

        public virtual void clearGD()
        {
        }

        public virtual void bpInToE()
        {
        }

        public virtual void setBpParams()
        {
        }

        public virtual void setOutToE(double[] outToE)
        {
        }

        public virtual void initBP()
        {
        }

        public virtual List<object> toJson()
        {
            return new List<object>();
        }

        public virtual void fromJson(List<object> l)
        {
        }

        public virtual double getWeightSquare()
        {
            return 0;
        }

        public virtual void addRegularizationToGD(double v) { }
    }
}
