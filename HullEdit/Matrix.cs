using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HullEdit
{
    class Matrix
    {
        public static void Multiply(double[,] left, double[,] right, double[,] returnMatrix)
        {
            double[,] result;       // temp array so we can compute in place
            result = new double[returnMatrix.GetLength(0), returnMatrix.GetLength(1)];

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

            Array.Copy(result, returnMatrix, returnMatrix.Length);
        }

    }
}
