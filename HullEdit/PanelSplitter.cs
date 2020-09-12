using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HullEdit
{
    public class PanelSplitter
    {
        public static PointCollection Line(Point start, Point end)
        {
            PointCollection splitter = new PointCollection();
            splitter.Add(start);
            splitter.Add(end);

            return splitter;
        }
        public static PointCollection SquareTongues(Point start, Point end, double height, double depth)
        {
            double verticalDir;
            double horizontalDir = 1;

            Point current;
            PointCollection splitter = new PointCollection();

            double panelHeight = Math.Abs(start.Y - end.Y);
            int tongues = (int)(panelHeight / height);
            double indent = (panelHeight - tongues * height) / 2;
            if (start.Y > end.Y)
                verticalDir = -1;
            else
                verticalDir = 1;

            splitter.Add(start);

            current = new Point(start.X, start.Y + verticalDir * indent);
            splitter.Add(current);

            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            for (int ii=0; ii<tongues-1; ii++)
            {
                current.Y += verticalDir * height;
                splitter.Add(current);
                current.X += horizontalDir * depth;
                splitter.Add(current);
                horizontalDir *= -1;
            }
            current.Y += verticalDir * height;
            splitter.Add(current);
            current.X += horizontalDir * depth / 2;
            splitter.Add(current);
            horizontalDir *= -1;

            splitter.Add(end);

            return splitter;
        }
    }
}
