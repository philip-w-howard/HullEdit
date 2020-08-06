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
            Panelize(chine1, chine2);
            //HorizontalizePanels();
        }

        protected void Panelize(Point3DCollection chine1, Point3DCollection chine2)
        {
            double r1, r2;
            Point intersection_a1, intersection_a2;
            Point intersection_b1, intersection_b2;

            scale = 1.0;

            m_edge1 = new PointCollection();
            m_edge2 = new PointCollection();

            // See if we start at a point or an edge:
            if ((chine1[0] - chine2[0]).Length < MIN_EDGE_LENGTH)
            {
                Debug.WriteLine("\nPanel staring with a point");
                // Start both edges at (0,0)
                m_edge1.Add(new Point(0, 0));
                m_edge2.Add(new Point(0, 0));
            }
            else
            {
                Debug.WriteLine("\nPanel staring with a edge");
                // Make the edge the first segment in edge2
                m_edge1.Add(new Point(0, 0));
                m_edge2.Add(new Point(0, 0));

                r1 = (chine1[0] - chine2[0]).Length;
                m_edge2.Add(new Point(0, -r1));
            }

            // Compute next point, and favor positive X direction
            // advance edge1 by one point
            r1 = (chine1[0] - chine1[1]).Length;
            r2 = (chine2[0] - chine1[1]).Length;
            Geometry.Intersection(m_edge1[m_edge1.Count - 1], r1, m_edge2[m_edge2.Count - 1], r2, out intersection_a1, out intersection_a2);

            // advance edge2 by one point
            r1 = (chine2[0] - chine2[1]).Length;
            r2 = (chine1[0] - chine2[1]).Length;
            Geometry.Intersection(m_edge2[m_edge2.Count - 1], r1, m_edge1[m_edge1.Count - 1], r2, out intersection_b1, out intersection_b2);

            Debug.WriteLine("First Points: ({0})  ({1})  ({2})  ({3})", intersection_a1, intersection_a2, intersection_b1, intersection_b2);

            if (intersection_a1.X >= intersection_a2.X)
                m_edge1.Add(intersection_a1);
            else
                m_edge1.Add(intersection_a2);

            if (intersection_b1.X >= intersection_b2.X)
                m_edge2.Add(intersection_b1);
            else
                m_edge2.Add(intersection_b2);

            for (int ii = 2; ii < chine1.Count; ii++)
            {
                double result; //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                // advance edge1 by one point
                r1 = (chine1[ii - 1] - chine1[ii]).Length;
                r2 = (chine2[ii - 1] - chine1[ii]).Length;
                result = Geometry.Intersection(m_edge1[m_edge1.Count - 1], r1, m_edge2[m_edge2.Count - 1], r2, out intersection_a1, out intersection_a2);
                if (result != 0) return;       //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                // advance edge2 by one point
                r1 = (chine2[ii - 1] - chine2[ii]).Length;
                r2 = (chine1[ii - 1] - chine2[ii]).Length;
                result = Geometry.Intersection(m_edge2[m_edge2.Count - 1], r1, m_edge1[m_edge1.Count - 1], r2, out intersection_b1, out intersection_b2);
                if (result != 0) return;       //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

                Debug.WriteLine("Points: ({0})  ({1})  ({2})  ({3})", intersection_a1, intersection_a2, intersection_b1, intersection_b2);

                Vector v_1 = m_edge1[m_edge1.Count - 1] - m_edge1[m_edge1.Count - 2];
                Vector v_1a = intersection_a1 - m_edge1[m_edge1.Count - 1];
                Vector v_1b = intersection_a2 - m_edge1[m_edge1.Count - 1];

                Vector v_2 = m_edge2[m_edge2.Count - 1] - m_edge2[m_edge2.Count - 2];
                Vector v_2a = intersection_b1 - m_edge2[m_edge2.Count - 1];
                Vector v_2b = intersection_b2 - m_edge2[m_edge2.Count - 1];

                Debug.WriteLine("Vectors: ({0})  ({1})  ({2})      ({3})  ({4})  ({5})", v_1, v_1a, v_1b, v_2, v_2a, v_2b);

                double a1 = Math.Abs(Vector.AngleBetween(v_1, v_1a));
                double a2 = Math.Abs(Vector.AngleBetween(v_1, v_1b));
                double b1 = Math.Abs(Vector.AngleBetween(v_2, v_2a));
                double b2 = Math.Abs(Vector.AngleBetween(v_2, v_2b));

                Debug.WriteLine("Angles: {0}  {1}    {2}  {3}", a1, a2, b1, b2);

                if (a1 < a2)
                    m_edge1.Add(intersection_a1);
                else
                    m_edge1.Add(intersection_a2);

                if (b1 < b2)
                    m_edge2.Add(intersection_b1);
                else
                    m_edge2.Add(intersection_b2);
            }

            // Close the tail?
            if ( (m_edge1[m_edge1.Count - 1] - m_edge2[m_edge2.Count - 1]).Length > MIN_EDGE_LENGTH)
            {
                m_edge2.Add(m_edge1[m_edge1.Count - 1]);
            }

        }

        public void HorizontalizePanel()
        {
            double x = m_edge1[m_edge1.Count - 1].X - m_edge1[0].X;
            double y = m_edge1[m_edge1.Count - 1].Y - m_edge1[0].Y;

            double angle;

            angle = Math.Atan2(y, x);
            Rotate(new Point(0, 0), -2*Math.PI*angle);
        }
        private void RotateEdge(PointCollection points, Point origin, double angle)
        {
            angle /= 2 * Math.PI;

            for (int ii = 0; ii < points.Count; ii++)
            {
                double cos = Math.Cos(angle);
                double sin = Math.Sin(angle);

                Point newPoint = new Point(cos * points[ii].X - sin * points[ii].Y, sin * points[ii].X + cos * points[ii].Y);
                newPoint.X += origin.X;
                newPoint.Y += origin.Y;

                Debug.WriteLine("old: ({0}) angle: {1} new: ({2})", points[ii], angle * 2 * Math.PI, newPoint);
                points[ii] = newPoint;
            }
        }

        public void Rotate(Point origin, double angle)
        {
            RotateEdge(m_edge1, origin, angle);
            RotateEdge(m_edge2, origin, angle);
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
            DrawEdge(m_edge1, canvas, System.Windows.Media.Brushes.Red);
            DrawEdge(m_edge2, canvas, System.Windows.Media.Brushes.Blue);
        }

    }
}
