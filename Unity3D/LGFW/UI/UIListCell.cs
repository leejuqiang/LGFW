using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    /// <summary>
    /// The base class of the cell in an UIList
    /// </summary>
    public abstract class UIListCell : BaseMono
    {
        /// <summary>
        /// The index of this cell's data in the data list
        /// </summary>
        /// <value></value>
        public int DataIndex
        {
            get; set;
        }

        /// <summary>
        /// The index of this cell in the list
        /// /// </summary>
        /// <value></value>
        public int CellIndex
        {
            get; set;
        }

        /// <summary>
        /// The RectTransform of the cell
        /// </summary>
        /// <value>The transform</value>
        public RectTransform Trans
        {
            get { return m_trans; }
        }

        protected RectTransform m_trans;

        protected override void doAwake()
        {
            m_trans = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Called when set a data to the cell, override this function to implement your own UI
        /// </summary>
        /// <param name="data">The data</param>
        public abstract void setData(object data);
    }
}