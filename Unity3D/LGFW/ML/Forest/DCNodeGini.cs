using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LGFW
{
    public class DCNodeGini : DCTreeNodeBase
    {
        public DCNodeGini(int depth, double maxError) : base(depth, maxError, DCTreeType.gini)
        {
        }

        protected double giniForList(Dictionary<int, int> labelCount, List<DCTreeData> l)
        {
            countLabel(l, labelCount);
            double count = 0;
            foreach (int i in labelCount.Values)
            {
                double d = (double)i / l.Count;
                count += d * d;
            }
            return 1 - count;
        }

        protected override double getError(int dimension, double threshold)
        {
            double g1 = giniForList(m_labelCounts[0], m_segments[0]);
            double g2 = giniForList(m_labelCounts[1], m_segments[1]);
            return g1 * m_segments[0].Count / m_dataList.Count + g2 * m_segments[1].Count / m_dataList.Count;
        }

    }
}
