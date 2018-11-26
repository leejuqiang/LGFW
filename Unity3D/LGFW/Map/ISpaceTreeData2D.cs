using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// Interface for data used in space tree
    /// </summary>
    public interface ISpaceTreeData2D
    {
        /// <summary>
        /// Gets the data's position
        /// </summary>
        /// <returns></returns>
        Vector2 getPosition();
        /// <summary>
        /// Gets the half size of this data
        /// </summary>
        /// <returns></returns>
        Vector2 getHalfSize();
        /// <summary>
        /// Gets the minimun xy point of this data
        /// </summary>
        /// <returns></returns>
        Vector2 getMinPoint();
        /// <summary>
        /// Gets the max xy point of this data
        /// </summary>
        /// <returns></returns>
        Vector2 getMaxPoint();
    }
}
