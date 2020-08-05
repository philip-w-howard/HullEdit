using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace HullEdit
{
    class Panel
    {
        public double scale { get; set; }

        private const double MIN_EDGE_LENGTH = 0.1;

        private PointCollection m_edge1;
        private PointCollection m_edge2;

        public Panel(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection1, intersection2;

            scale = 1.0;

            m_edge1 = new PointCollection();
            m_edge2 = new PointCollection();

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

            for (int ii=1; ii<chine1.Count; ii++)
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
                    m_edge2.Add(intersection2);
                else if (intersection1.Y == intersection2.Y && intersection1.X < intersection2.X)
                    m_edge2.Add(intersection2);
                else
                    m_edge2.Add(intersection1);
            }
        }

        public void rotate(double angle)
        {

        }

        public void ShiftTo(double x, double y)
        {
            double min_x1, min_y1, min_x2, min_y2;
            Geometry.ComputeMin(m_edge1, out min_x1, out min_y1);
            Geometry.ComputeMin(m_edge2, out min_x2, out min_y2);

            x -= Math.Min(min_x1, min_x2);
            y -= Math.Min(min_y1, min_y2);

            Shift(x, y);
        }
        public void Shift(double x, double y)
        {
            Geometry.TranslateShape(m_edge1, x, y);
            Geometry.TranslateShape(m_edge2, x, y);
        }

        protected void DrawEdge(PointCollection edge, Canvas canvas, Brush brush)
        {
            for (int ii = 0; ii < edge.Count - 1; ii++)
            {
                Line myLine = new Line();

                myLine.Stroke = brush;

                myLine.X1 = edge[ii].X * scale;
                myLine.Y1 = edge[ii].Y * scale;
                myLine.X2 = edge[ii + 1].X * scale;
                myLine.Y2 = edge[ii + 1].Y * scale;

                myLine.StrokeThickness = 1;

                canvas.Children.Add(myLine);
            }

        }
        public void draw(Canvas canvas)
        {
           // DrawEdge(m_edge1, canvas, System.Windows.Media.Brushes.Red);
            DrawEdge(m_edge2, canvas, System.Windows.Media.Brushes.Blue);
        }

    }
}
