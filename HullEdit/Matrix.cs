using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    class Matrix
    {
        public static void Multiply(double[,] left, double[,] right, out double[,] returnMatrix)
        {
            double[,] result;       // temp array so we can compute in place
            result = new double[left.GetLength(0), left.GetLength(1)];

            for (int arow = 0; arow < left.GetLength(0); arow++)
            {
                for (int bcol = 0; bcol < right.GetLength(1); bcol++)
                {
                    result[arow, bcol] = 0;
                    for (int acol = 0; acol < left.GetLength(1); acol++)
                    {
                        result[arow, bcol] += left[arow, acol] * right[acol, bcol];
                    }
                }
            }

            returnMatrix = result;
        }

        public static void Multiply(Point3DCollection left, double[,] right, out Point3DCollection returnMatrix)
        {
            Point3DCollection result = new Point3DCollection(left.Count);   // temp array so we can compute in place

            for (int arow = 0; arow < left.Count; arow++)
            {
                Point3D point = new Point3D();
                point.X = left[arow].X * right[0, 0] + left[arow].Y * right[1, 0] + left[arow].Z * right[2, 0];
                point.Y = left[arow].X * right[0, 1] + left[arow].Y * right[1, 1] + left[arow].Z * right[2, 1];
                point.Z = left[arow].X * right[0, 2] + left[arow].Y * right[1, 2] + left[arow].Z * right[2, 2];

                result[arow] = point;
            }

            returnMatrix = result;
        }

    }
}
