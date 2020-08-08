using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HullEdit
{
    public class PanelDisplay : Button
    {
        protected Panel m_panel;
        public double X { get; set; }
        public double Y { get; set; }

        public PanelDisplay(Panel p)
        {
            m_panel = p;
        }

        // Draw the panel.
        // This requires a horrible hierarchy of classes, but it allows us to draw a filled figure,
        // which makes it clickable.
        protected override void OnRender(DrawingContext drawingContext)
        {
            // Need a PathGeometry
            LineSegment line = new LineSegment();
            PathSegmentCollection lines = new PathSegmentCollection();

            for (int ii = 1; ii < m_panel.points.Count - 1; ii++)
            {
                line = new LineSegment();
                line.Point = m_panel.points[ii];
                lines.Add(line);
            }

            line = new LineSegment();
            line.Point = m_panel.points[0];
            lines.Add(line);

            PathFigure path = new PathFigure();
            path.StartPoint = m_panel.points[0];
            path.Segments = lines;

            PathFigureCollection actualPath = new PathFigureCollection();
            actualPath.Add(path);

            PathGeometry drawing = new PathGeometry();
            drawing.Figures = actualPath;

            drawingContext.DrawGeometry(new System.Windows.Media.SolidColorBrush(Colors.White), new Pen(System.Windows.Media.Brushes.Black, 1.0), drawing);
            

            //if (m_panel == null) return;

            //Rect background = new Rect(new Point(0, 0), new Point(Width, Height));
            //SolidColorBrush brush = new SolidColorBrush(Colors.White);
            //brush.Opacity = .5;

            ////drawingContext.DrawRectangle(brush, null, background);

            //Pen pen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            //for (int ii = 0; ii < m_panel.points.Count - 1; ii++)
            //{
            //    Point p1 = new Point(m_panel.points[ii].X * m_panel.scale, m_panel.points[ii].Y * m_panel.scale);
            //    Point p2 = new Point(m_panel.points[ii + 1].X * m_panel.scale, m_panel.points[ii + 1].Y * m_panel.scale);
            //    drawingContext.DrawLine(pen, p1, p2);
            //}
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            double size_x, size_y;
            Geometry.ComputeSize(m_panel.points, out size_x, out size_y);

            size_x *= m_panel.scale;
            size_y *= m_panel.scale;
            Width = size_x;
            Height = size_y;

            Debug.WriteLine("PanelDisplay MeasureOverride {0} {1}", size_x, size_y);
            return new Size(size_x, size_y);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine("PanelDisplay ArrangeOverride {0} {1}", finalSize.Width, finalSize.Height);

            return finalSize;
        }
    }
}
