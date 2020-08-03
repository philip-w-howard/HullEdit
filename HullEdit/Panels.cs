using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HullEdit
{
    class Panels
    {
        private double[][,] m_chines;           // [chine][index, axis]
        private const int POINTS_PER_CHINE = 50;
        private int m_pointsInChine;
        private int m_numPanels;
        private Panel[] m_panels;

        public Panels(Hull hull)
        {
            m_numPanels = hull.numChines - 1;
            m_panels = new Panel[m_numPanels];

            PrepareChines(hull);

            for (int ii=0; ii<m_numPanels; ii++)
            {
                m_panels[ii] = new Panel(m_pointsInChine, m_chines[ii], m_chines[ii + 1]);
            }
        }

        // Note: This code is largely duplicated in HullDisplay. Should it go in Hull?
        protected void PrepareChines(Hull hull)
        {
            m_chines = new double[hull.numChines][,];
            double[,] chine_data = new double[hull.numBulkheads, 3];
            for (int chine = 0; chine < hull.numChines; chine++)
            {
                m_chines[chine] = new double[POINTS_PER_CHINE, 3];
                for (int bulkhead = 0; bulkhead < hull.numBulkheads; bulkhead++)
                {
                    for (int axis = 0; axis < 3; axis++)
                    {
                        chine_data[bulkhead, axis] = hull.GetBulkheadPoint(bulkhead, chine, axis);
                    }
                }
                Splines spline = new Splines(hull.numBulkheads, Splines.RELAXED, chine_data);
                m_pointsInChine = spline.GetPoints(m_chines[chine]);
            }
        }

    }
}
