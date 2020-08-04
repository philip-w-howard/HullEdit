using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    class Panels
    {
        private Point3DCollection[] m_chines;           // [chine][index, axis]
        private const int POINTS_PER_CHINE = 50;
        private int m_pointsInChine;
        private int m_numPanels;
        private Panel[] m_panels;

        public Panels(Hull hull)
        {
            Point p1, p2;

            Geometry.Intersection(new Point(0, 0), 1, new Point(1.5, 0), 1, out p1, out p2);
            Debug.WriteLine("Intersections: {0} {1}", p1, p2);

            Geometry.Intersection(new Point(0, 0), 1, new Point(0, 1.5), 1, out p1, out p2);
            Debug.WriteLine("Intersections: {0} {1}", p1, p2);

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
            m_chines = new Point3DCollection[hull.numChines];
            Point3DCollection chine_data = new Point3DCollection(hull.numBulkheads);
            for (int chine = 0; chine < hull.numChines; chine++)
            {
                int actual_chine = chine;
                if (chine >= hull.numChines) actual_chine = chine - hull.numChines;

                m_chines[chine] = new Point3DCollection(POINTS_PER_CHINE);
                chine_data.Clear();
                for (int bulkhead = 0; bulkhead < hull.numBulkheads; bulkhead++)
                {
                    chine_data.Add(hull.GetBulkheadPoint(bulkhead, chine));

                    if (chine >= hull.numChines)
                    {
                        Point3D point = chine_data[bulkhead];
                        point.X = -point.X;
                        chine_data[bulkhead] = point;
                    }
                }
                Splines spline = new Splines(hull.numBulkheads, Splines.RELAXED, chine_data);
                m_pointsInChine = spline.GetPoints(POINTS_PER_CHINE, m_chines[chine]);
            }
        }
    }
}
