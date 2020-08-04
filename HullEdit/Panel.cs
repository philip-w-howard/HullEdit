using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    class Panel
    {
        private const double MIN_EDGE_LENGTH = 0.1;

        PointCollection m_edge1;
        PointCollection m_edge2;

        public Panel(int numPoints, Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection1, intersection2;

            m_edge1 = new PointCollection(numPoints);
            m_edge2 = new PointCollection(numPoints);

            // See if we start at a point or an edge:
            if ((chine1[0] - chine2[0]).Length < MIN_EDGE_LENGTH)
            {
                Debug.WriteLine("Panel staring with a point");
                // Start both edges at (0,0)
                m_edge1.Add(new Point(0, 0));
                m_edge2.Add(new Point(0, 0));
            }
            else
            {
                Debug.WriteLine("Panel staring with a edge");
                // Make the edge the first segment in edge2
                m_edge1.Add(new Point(0, 0));
                m_edge2.Add(new Point(0, 0));

                r1 = (chine1[0] - chine2[0]).Length;
                m_edge2.Add(new Point(0, -r1));
            }

            for (int ii=1; ii<numPoints; ii++)
            {
                // advance edge1 by one point
                r1 = (chine1[ii - 1] - chine1[ii]).Length;
                r2 = (chine2[ii - 1] - chine1[ii]).Length;
                Geometry.Intersection(m_edge1[m_edge1.Count - 1], r1, m_edge2[m_edge2.Count - 1], r2, out intersection1, out intersection2);

                // choose between the two intersections
                if (intersection1.Y > intersection2.Y)
                    m_edge1.Add(intersection1);
                else if (intersection1.Y == intersection2.Y && intersection1.X > intersection2.X)
                    m_edge1.Add(intersection1);
                else
                    m_edge1.Add(intersection2);

                // advance edge2 by one point
                r1 = (chine2[ii - 1] - chine2[ii]).Length;
                r2 = (chine1[ii - 1] - chine2[ii]).Length;
                Geometry.Intersection(m_edge1[m_edge1.Count - 2], r1, m_edge2[m_edge2.Count - 1], r2, out intersection1, out intersection2);

                // choose between the two intersections
                if (intersection1.Y > intersection2.Y)
                    m_edge1.Add(intersection2);
                else if (intersection1.Y == intersection2.Y && intersection1.X < intersection2.X)
                    m_edge1.Add(intersection2);
                else
                    m_edge1.Add(intersection1);
            }
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
