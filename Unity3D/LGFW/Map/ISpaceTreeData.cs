using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Interface for data used in space tree
    /// </summary>
    public interface ISpaceTreeData
    {
        /// <summary>
        /// Gets the data's position
        /// </summary>
        /// <returns></returns>
        Vector3 getPosition();
        /// <summary>
        /// Gets the half size of this data
        /// </summary>
        /// <returns></returns>
        Vector3 getHalfSize();
        /// <summary>
        /// Gets the minimun xyz point of this data
        /// </summary>
        /// <returns></returns>
        Vector3 getMinPoint();
        /// <summary>
        /// Gets the max xyz point of this data
        /// </summary>
        /// <returns></returns>
        Vector3 getMaxPoint();
    }
}
