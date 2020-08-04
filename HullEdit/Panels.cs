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
            m_numPanels = hull.numChines - 1;
            m_panels = new Panel[m_numPanels];

            m_chines = Geometry.PrepareChines(hull.CopyBulkheads(), POINTS_PER_CHINE);

            for (int ii=0; ii<m_numPanels; ii++)
            {
                m_panels[ii] = new Panel(m_pointsInChine, m_chines[ii], m_chines[ii + 1]);
            }
        }
    }
}
