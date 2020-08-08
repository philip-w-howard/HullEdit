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
        private Panel[] m_panels;
        public Panel[] panel { get { return m_panels; } }

        public Panels(Hull hull)
        {
            Point3DCollection[] chines;
            int numPanels = hull.numChines - 1;

            m_panels = new Panel[numPanels];

            chines = Geometry.PrepareChines(hull.CopyBulkheads(), POINTS_PER_CHINE);

            for (int ii=0; ii<numPanels; ii++)
            {
                m_panels[ii] = new Panel(chines[ii], chines[ii + 1]);
                m_panels[ii].scale = 3;
            }
        }

        //public void Draw(Canvas canvas)
        //{
        //    canvas.Children.Clear();

        //    foreach (Panel panel in m_panels)
        //    {
        //        panel.draw(canvas);
        //    }
        //}
    }
}
