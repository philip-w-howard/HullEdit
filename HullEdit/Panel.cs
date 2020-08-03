using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HullEdit
{
    class Panel
    {
        PointCollection m_edge1;
        PointCollection m_edge2;

        public Panel(int numPoints, double[,] chine1, double[,] chine2)
        {
            m_edge1 = new PointCollection(numPoints);
            m_edge2 = new PointCollection(numPoints);
        }

        public void rotate(double angle)
        {

        }

        public void shift(double x, double y)
        {

        }

        public void draw(DrawingContext drawingContext)
        {

        }

    }
}
