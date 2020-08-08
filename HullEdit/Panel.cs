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
    public class Panel
    {
        public double scale { get; set; }

        private const double MIN_EDGE_LENGTH = 0.1;

        protected PointCollection m_panelPoints;

        public PointCollection points { get { return m_panelPoints; } }

        public Panel(Point3DCollection chine1, Point3DCollection chine2)
        {
            Panelize(chine1, chine2);
            HorizontalizePanel();
            ShiftTo(0, 0);
        }

        protected void Panelize(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection_a1, intersection_a2;
            Point intersection_b1, intersection_b2;
            PointCollection edge2 = new PointCollection();

            scale = 1.0;

            m_panelPoints = new PointCollection();

            // See if we start at a point or an edge:
            if ((chine1[0] - chine2[0]).Length < MIN_EDGE_LENGTH)
            {
                // Start both edges at (0,0)
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));
            }
            else
            {
                // Make the edge the first segment in edge2
                m_panelPoints.Add(new Point(0, 0));
                edge2.Add(new Point(0, 0));

                r1 = (chine1[0] - chine2[0]).Length;
                edge2.Add(new Point(0, -r1));
            }

            // Compute next point, and favor positive X direction
            // advance edge1 by one point
            r1 = (chine1[0] - chine1[1]).Length;
            r2 = (chine2[0] - chine1[1]).Length;
            Geometry.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);

            // advance edge2 by one point
            r1 = (chine2[0] - chine2[1]).Length;
            r2 = (chine1[0] - chine2[1]).Length;
            Geometry.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

            if (intersection_a1.X >= intersection_a2.X)
                m_panelPoints.Add(intersection_a1);
            else
                m_panelPoints.Add(intersection_a2);

            if (intersection_b1.X >= intersection_b2.X)
                edge2.Add(intersection_b1);
            else
                edge2.Add(intersection_b2);

            for (int ii = 2; ii < chine1.Count; ii++)
            {
                // advance edge1 by one point
                r1 = (chine1[ii - 1] - chine1[ii]).Length;
                r2 = (chine2[ii - 1] - chine1[ii]).Length;
                Geometry.Intersection(m_panelPoints[m_panelPoints.Count - 1], r1, edge2[edge2.Count - 1], r2, out intersection_a1, out intersection_a2);

                // advance edge2 by one point
                r1 = (chine2[ii - 1] - chine2[ii]).Length;
                r2 = (chine1[ii - 1] - chine2[ii]).Length;
                Geometry.Intersection(edge2[edge2.Count - 1], r1, m_panelPoints[m_panelPoints.Count - 1], r2, out intersection_b1, out intersection_b2);

                Vector v_1 = m_panelPoints[m_panelPoints.Count - 1] - m_panelPoints[m_panelPoints.Count - 2];
                Vector v_1a = intersection_a1 - m_panelPoints[m_panelPoints.Count - 1];
                Vector v_1b = intersection_a2 - m_panelPoints[m_panelPoints.Count - 1];

                Vector v_2 = edge2[edge2.Count - 1] - edge2[edge2.Count - 2];
                Vector v_2a = intersection_b1 - edge2[edge2.Count - 1];
                Vector v_2b = intersection_b2 - edge2[edge2.Count - 1];

                double a1 = Math.Abs(Vector.AngleBetween(v_1, v_1a));
                double a2 = Math.Abs(Vector.AngleBetween(v_1, v_1b));
                double b1 = Math.Abs(Vector.AngleBetween(v_2, v_2a));
                double b2 = Math.Abs(Vector.AngleBetween(v_2, v_2b));

                if (a1 < a2)
                    m_panelPoints.Add(intersection_a1);
                else
                    m_panelPoints.Add(intersection_a2);

                if (b1 < b2)
                    edge2.Add(intersection_b1);
                else
                    edge2.Add(intersection_b2);

                ShiftTo(0, 0);
            }

            // NOTE: Should check for closed tail?
            for (int ii=edge2.Count-1; ii>=0; ii--)
            {
                m_panelPoints.Add(edge2[ii]);
            }

        }

        public void HorizontalizePanel()
        {
            double x = m_panelPoints[m_panelPoints.Count/2].X - m_panelPoints[0].X;
            double y = m_panelPoints[m_panelPoints.Count/2].Y - m_panelPoints[0].Y;

            double angle;

            angle = Math.Atan2(y, x);
            Rotate(new Point(0, 0), -angle);

            ShiftTo(0, 0);
        }

        public void Rotate(Point origin, double angle)
        {
            double[,] rotate = new double[2, 2];

            Shift(-origin.X, -origin.Y);

            rotate[0, 0] = Math.Cos(angle);
            rotate[1, 1] = Math.Cos(angle);
            rotate[0, 1] = Math.Sin(angle);
            rotate[1, 0] = -Math.Sin(angle);

            Matrix.Multiply(m_panelPoints, rotate, out m_panelPoints);

            ShiftTo(0, 0);
        }
        private void ShiftTo(double x, double y)
        {
            double min_x, min_y;
            Geometry.ComputeMin(m_panelPoints, out min_x, out min_y);

            x -= min_x;
            y -= min_y;

            if (x != 0 || y != 0) Shift(x, y);
        }
        private void Shift(double x, double y)
        {
            Geometry.TranslateShape(m_panelPoints, x, y);
        }

        //protected void DrawEdge(PointCollection edge, Canvas canvas, Brush brush)
        //{
        //    for (int ii = 0; ii < edge.Count - 1; ii++)
        //    {
        //        Line myLine = new Line();

        //        myLine.Stroke = brush;

        //        myLine.X1 = edge[ii].X * scale;
        //        myLine.Y1 = edge[ii].Y * scale;
        //        myLine.X2 = edge[ii + 1].X * scale;
        //        myLine.Y2 = edge[ii + 1].Y * scale;

        //        myLine.StrokeThickness = 1;

        //        canvas.Children.Add(myLine);
        //    }

        //}
        //public void draw(Canvas canvas)
        //{
        //    DrawEdge(m_panelPoints, canvas, System.Windows.Media.Brushes.Red);
        //}
    }
}
