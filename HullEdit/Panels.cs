using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    public class Panels
    {
        private const int POINTS_PER_CHINE = 50;

        private List<Panel> m_panels;
        private List<Panel> m_bulkheads;

        public List<Panel> panels { get { return m_panels; } }
        public List<Panel> bulkheads {  get { return m_bulkheads; } }

        public Panels(Hull hull)
        {
            Hull highResHull = hull.Copy();
            highResHull.PrepareChines(POINTS_PER_CHINE);

            int numPanels = highResHull.numChines - 1;

            m_panels = new List<Panel>();

            for (int ii = 0; ii < numPanels; ii++)
            {
                Panel panel = new Panel(highResHull.GetChine(ii), highResHull.GetChine(ii + 1));
                panel.name = "Chine " + (ii + 1);
                m_panels.Add(panel);
            }

            //*********************************
            // bulkheads:
            int numBulkheads = hull.numBulkheads;

            if (hull.GetBulkhead(numBulkheads - 1).type == Bulkhead.BulkheadType.BOW) numBulkheads--;

            m_bulkheads = new List<Panel>();
            Hull fullHull = hull.CopyToFullHull();

            for (int bulkhead=0; bulkhead< fullHull.numBulkheads; bulkhead++)
            {
                int numChines = fullHull.numChines;

                if (fullHull.GetBulkhead(bulkhead).type != Bulkhead.BulkheadType.BOW)
                {
                    Bulkhead bulk = fullHull.GetBulkhead(bulkhead);
                    Point3DCollection points = new Point3DCollection();

                    Point3D basePoint = bulk.GetPoint(0);

                    for (int chine = 0; chine < numChines; chine++)
                    {
                        Point3D point = bulk.GetPoint(chine);
                        if (bulk.type == Bulkhead.BulkheadType.TRANSOM)
                        {
                            point.Y = basePoint.Y + (point.Y - basePoint.Y) / Math.Sin(bulk.TransomAngle);
                        }
                        points.Add(bulk.GetPoint(chine));
                    }

                    // close the shape
                    if (points[0].X != 0) points.Add(points[0]);

                    Panel panel = new Panel(points);
                    panel.name = "Bulkhead " + (bulkhead + 1);
                    m_bulkheads.Add(panel);
                }
            }


        }
    }
}
