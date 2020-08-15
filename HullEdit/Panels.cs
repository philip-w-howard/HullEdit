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
        private const double DEFAULT_SCALE = 3;

        private List<Panel> m_panels;
        private List<Panel> m_bulkheads;

        public List<Panel> panels { get { return m_panels; } }
        public List<Panel> bulkheads {  get { return m_bulkheads; } }

        public Panels(Hull hull)
        {
            int numPanels = hull.numChines - 1;

            m_panels = new List<Panel>();

            for (int ii = 0; ii < numPanels; ii++)
            {
                Panel panel = new Panel(hull.GetChine(ii), hull.GetChine(ii + 1));
                panel.scale = DEFAULT_SCALE;
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
                Point3D point;

                if (fullHull.GetBulkhead(bulkhead).type != Bulkhead.BulkheadType.BOW)
                {
                    Bulkhead bulk = fullHull.GetBulkhead(bulkhead);
                    Point3DCollection points = new Point3DCollection();

                    // Add reflection
                    for (int chine = 0; chine < numChines; chine++)
                    {
                        points.Add(bulk.GetPoint(chine));
                    }

                    // close the shape
                    if (points[0].X != 0) points.Add(points[0]);

                    Panel panel = new Panel(points);
                    panel.scale = DEFAULT_SCALE;
                    m_bulkheads.Add(panel);
                }
            }


        }
    }
}
