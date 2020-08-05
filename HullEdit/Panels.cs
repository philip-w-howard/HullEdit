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
        private Point3DCollection[] m_chines;           // [chine][index, axis]
        private const int POINTS_PER_CHINE = 50;
        private int m_numPanels;
        private Panel[] m_panels;

        public Panels(Hull hull)
        {
            m_numPanels = hull.numChines - 1;
            m_panels = new Panel[m_numPanels];

            m_chines = Geometry.PrepareChines(hull.CopyBulkheads(), POINTS_PER_CHINE);

            for (int ii=0; ii<m_numPanels; ii++)
            {
                m_panels[ii] = new Panel(m_chines[ii], m_chines[ii + 1]);
                m_panels[ii].ShiftTo(10, 10);
                m_panels[ii].scale = 0.1;
            }
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();

            m_panels[0].draw(canvas);
            //foreach(Panel panel in m_panels)
            //{
            //    panel.draw(canvas);
            //}
        }
    }
}
