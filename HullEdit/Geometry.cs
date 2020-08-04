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
            if (b * b - 4 * a * c < 0) return Double.NaN;

            double root = Math.Sqrt(b * b - 4 * a * c);
            x1 = (-b + root) / (2 * a);
            x2 = (-b - root) / (2 * a);

            return x1;
        }

        static public void Intersection(Point p1, double r1, Point p2, double r2, out Point intersection1, out Point intersection2)
        {
            intersection1 = new Point();
            intersection2 = new Point();

            if (p1.X != p2.X)
            {
                double A = (r1 * r1 - r2 * r2 - p1.X * p1.X + p2.X * p2.X - p1.Y * p1.Y + p2.Y * p2.Y) / (2 * p2.X - 2 * p1.X);
                double B = (p1.Y - p2.Y) / (p2.X - p1.X);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * p1.X * B - 2 * p1.Y;
                double c = A * A - 2 * p1.X * A + p1.X * p1.X - p1.Y * p1.Y - r1 * r1;

                double y1, y2;

                QuadradicSolution(a, b, c, out y1, out y2);
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
                double c = A * A - 2 * p1.Y * A + p1.Y * p1.Y - p1.X * p1.X - r1 * r1;
                double x1, x2;

                QuadradicSolution(a, b, c, out x1, out x2);
                intersection1.X = x1;
                intersection1.Y = A + B * intersection1.X;

                intersection2.X = x2;
                intersection2.Y = A + B * intersection2.X;
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

        public static Point3DCollection[] PrepareChines(Point3DCollection[] bulkheads, int points_per_chine)
        {
            int nChines = bulkheads[0].Count;

            Point3DCollection[] chines = new Point3DCollection[nChines];
            Point3DCollection chine_data = new Point3DCollection(bulkheads.Length);
            for (int chine = 0; chine < nChines; chine++)
            {
                chines[chine] = new Point3DCollection(points_per_chine);
                chine_data.Clear();
                for (int bulkhead = 0; bulkhead < bulkheads.Length; bulkhead++)
                {
                    chine_data.Add(bulkheads[bulkhead][chine]);
                }
                Splines spline = new Splines(bulkheads.Length, Splines.RELAXED, chine_data);
                spline.GetPoints(points_per_chine, chines[chine]);
            }

            return chines;
        }
    }
}
