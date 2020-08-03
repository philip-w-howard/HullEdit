using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HullEdit
{
    class Geometry
    {
        static public double QuadradicSolution(double a, double b, double c, int index)
        {
            if (b * b - 4 * a * c < 0) return Double.NaN;

            if (index == 0)
            {
                return (-b + Math.Sqrt(b * b - 4 * a * c)) / 2 * a;
            }
            else
            {
                return (-b - Math.Sqrt(b * b - 4 * a * c)) / 2 * a;
            }
        }

        static public void Intersection(double x1, double y1, double r1, double x2, double y2, double r2, out double xout, out double yout, int index)
        {
            if (x1 != x2)
            {
                double A = (r1 * r1 - r2 * r2 - x1 * x1 + x2 * x2 - y1 * y1 + y2 * y2) / (2 * x2 - 2 * x1);
                double B = (x1 - y2) / (x2 - x1);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * x1 * B - 2 * y1;
                double c = A * A - 2 * x1 * A + x1 * x1 - y1 * y1 - r1 * r1;

                yout = QuadradicSolution(a, b, c, index);
                xout = A + B * yout;
            }
            else
            {
                double A = (r1 * r1 - r2 * r2 - y1 * y1 + y2 * y2 - x1 * x1 + x2 * x2) / (2 * y2 - 2 * y1);
                double B = (x1 - x2) / (y2 - y1);
                double a = B * B + 1;
                double b = 2 * A * B - 2 * y1 * B - 2 * x1;
                double c = A * A - 2 * y1 * A + y1 * y1 - x1 * x1 - r1 * r1;

                xout = QuadradicSolution(a, b, c, index);
                yout = A + B * xout;
            }
        }
    }
}
