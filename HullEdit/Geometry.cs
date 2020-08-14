using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    class Geometry
    {
        static public double QuadradicSolution(double a, double b, double c, out double x1, out double x2)
        {
            x1 = Double.NaN;
            x2 = Double.NaN;
            if (b * b - 4 * a * c < 0)
            {
                return Double.NaN;
            }

            double root = Math.Sqrt(b * b - 4 * a * c);
            x1 = (-b + root) / (2 * a);
            x2 = (-b - root) / (2 * a);

            return 0;
        }

        static public double Intersection(Point p1, double r1, Point p2, double r2, out Point intersection1, out Point intersection2)
        {
            intersection1 = new Point();
            intersection2 = new Point();

            if (p1.X != p2.X)
            {
                //double A = (r1 * r1 - r2 * r2 - p1.X * p1.X + p2.X * p2.X - p1.Y * p1.Y + p2.Y * p2.Y) / (2 * p2.X - 2 * p1.X);
                double A = (r1 * r1 - r2 * r2 - p1.X * p1.X + p2.X * p2.X - p1.Y * p1.Y + p2.Y * p2.Y) / (2 * p2.X - 2 * p1.X);
                double B = (p1.Y - p2.Y) / (p2.X - p1.X);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * p1.X * B - 2 * p1.Y;
                double c = A * A - 2 * p1.X * A + p1.X * p1.X + p1.Y * p1.Y - r1 * r1;

                double y1, y2;

                if (QuadradicSolution(a, b, c, out y1, out y2) != 0) return Double.NaN; ;

                if (y1 == Double.NaN || y2 == Double.NaN) return Double.NaN;  //<<<<<<<<<<<<<<<<<<<<<<<<<

                intersection1.Y = y1;
                intersection1.X = A + B * intersection1.Y;

                intersection2.Y = y2;
                intersection2.X = A + B * intersection2.Y;
            }
            else
            {
                double A = (r1 * r1 - r2 * r2 - p1.Y * p1.Y + p2.Y * p2.Y - p1.X * p1.X + p2.X * p2.X) / (2 * p2.Y - 2 * p1.Y);
                double B = (p1.X - p2.X) / (p2.Y - p1.Y);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * p1.Y * B - 2 * p1.X;
                double c = A * A - 2 * p1.Y * A + p1.Y * p1.Y + p1.X * p1.X - r1 * r1;
                double x1, x2;

                if (QuadradicSolution(a, b, c, out x1, out x2) != 0) return Double.NaN;
                intersection1.X = x1;
                intersection1.Y = A + B * intersection1.X;

                intersection2.X = x2;
                intersection2.Y = A + B * intersection2.X;
            }

            return 0;
        }

        // Determine if the point (p3_x,p3_y) is near the line defined by (p1_x, p1_y) and (p2_x, p2_y)
        static public bool IsNearLine(double p1_x, double p1_y, double p2_x, double p2_y, double p3_x, double p3_y, double delta)
        {
            if (p1_x == p2_x) // vertical line
            {
                // is point along segment?
                if ((p1_y <= p3_y && p2_y >= p3_y) || (p1_y >= p3_y && p2_y <= p3_y))
                {
                    if (Math.Abs(p1_x - p3_x) <= delta) return true;
                }

                return false;
            }
            else if (p1_y == p2_y) // horizontal line
            {
                // is point along segment?
                if ((p1_x <= p3_x && p2_x >= p3_x) || (p1_x >= p3_x && p2_x <= p3_x))
                {
                    if (Math.Abs(p1_y - p3_y) <= delta) return true;
                }

                return false;
            }
            else // sloped line
            {
                double m1, m2;
                double b1, b2;
                double x, y;

                // compute slope between first two points:
                m1 = (p2_y - p1_y) / (p2_x - p1_x);

                // y intercept for first line
                b1 = -m1 * p1_x + p1_y;

                // compute slope of second (perpendicular) line
                m2 = -1 / m1;

                // y intercept for second (perpendicular) line
                b2 = -m2 * p3_x + p3_y;

                // Itersection of the two lines
                x = (b2 - b1) / (m1 - m2);
                y = m1 * x + b1;

                // is the intersection NOT within the line segment?
                if ((x <= p1_x && x <= p2_x) || (x >= p1_x && x >= p2_x)) return false;
                if ((y <= p1_y && y <= p2_y) || (y >= p1_y && y >= p2_y)) return false;

                // Is the intersection within delta of the point?
                double distance = Math.Sqrt((x - p3_x) * (x - p3_x) + (y - p3_y) * (y - p3_y));
                if (distance <= delta) return true;

                return false;
            }
        }

        static public void ComputeSize(Point3DCollection points, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point3D point in points)
            {
                max_x = Math.Max(max_x, point.X);
                max_y = Math.Max(max_y, point.Y);
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }

            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        static public void ComputeSize(Point3DCollection[] shape, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point3DCollection points in shape)
            {
                foreach (Point3D point in points)
                {
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                }
            }
            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }
        static public void ComputeSize(PointCollection points, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (Point point in points)
            {
                max_x = Math.Max(max_x, point.X);
                max_y = Math.Max(max_y, point.Y);
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }

            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }
        static public void ComputeSize(PointCollection[] shape, out double size_x, out double size_y)
        {
            size_x = Double.NaN;
            size_y = Double.NaN;

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            foreach (PointCollection points in shape)
            {
                foreach (Point point in points)
                {
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                }
            }
            size_x = max_x - min_x;
            size_y = max_y - min_y;
        }

        static public void ComputeMin(PointCollection points, out double min_x, out double min_y)
        {
            min_x = double.MaxValue;
            min_y = double.MaxValue;

            foreach (Point point in points)
            {
                min_x = Math.Min(min_x, point.X);
                min_y = Math.Min(min_y, point.Y);
            }
        }

        static public void ResizeShape(Point3DCollection[] shape, double scale)
        {
            double x, y, z;
            foreach (Point3DCollection points in shape)
            {
                for (int ii = 0; ii < points.Count; ii++)
                {
                    x = points[ii].X * scale;
                    y = points[ii].Y * scale;
                    z = points[ii].Z * scale;
                    points[ii] = new Point3D(x, y, z);
                }
            }
        }

        static public void ResizeShape(PointCollection[] shape, double scale)
        {
            double x, y;
            foreach (PointCollection points in shape)
            {
                for (int ii = 0; ii < points.Count; ii++)
                {
                    x = points[ii].X * scale;
                    y = points[ii].Y * scale;
                    points[ii] = new Point(x, y);
                }
            }
        }

        static public void TranslateShape(PointCollection points, double move_x, double move_y)
        {
            double x, y;
            for (int ii = 0; ii < points.Count; ii++)
            {
                x = points[ii].X + move_x;
                y = points[ii].Y + move_y;
                points[ii] = new Point(x, y);
            }
        }

    }
}
