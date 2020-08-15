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
        protected const int DEFAULT_WIDTH = 600;
        protected const int DEFAULT_HEIGHT = 400;

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

            for (int ii = 1; ii < m_panel.NumPoints; ii++)
            {
                line = new LineSegment();
                line.Point = m_panel.point(ii);
                lines.Add(line);
            }

            PathFigure path = new PathFigure();
            path.StartPoint = m_panel.point(0);
            path.Segments = lines;

            PathFigureCollection actualPath = new PathFigureCollection();
            actualPath.Add(path);

            PathGeometry drawing = new PathGeometry();
            drawing.Figures = actualPath;

            drawingContext.DrawGeometry(new System.Windows.Media.SolidColorBrush(Colors.White), new Pen(System.Windows.Media.Brushes.Black, 1.0), drawing);
        }
        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine("PanelDisplay.MeasureOverride {0}", availableSize);

            if (double.IsInfinity(availableSize.Width) || double.IsInfinity(availableSize.Height))
            {
                availableSize.Width = DEFAULT_WIDTH;
                availableSize.Height = DEFAULT_HEIGHT;
            }

            if (m_panel == null) return availableSize;

            Size size = m_panel.GetSize();

            Width = size.Width;
            Height = size.Height;

            Debug.WriteLine("PanelDisplay MeasureOverride {0} {1}", Width, Height);
            return size;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine("PanelDisplay ArrangeOverride {0} {1}", finalSize.Width, finalSize.Height);

            return finalSize;
        }
    }
}
