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

        public double scale { get; set; }

        protected Panel m_panel;

        public double X { get; set; }
        public double Y { get; set; }

        public PanelDisplay(Panel p, double scale)
        {
            m_panel = p.Copy();
            this.scale = scale;
        }

        private Point ScaledPoint(Point point)
        {
            point.X *= scale;
            point.Y *= scale;
            return point;
        }

        public Size size
        {
            get { return m_panel.GetSize(); }
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
                line.Point = ScaledPoint(m_panel.point(ii));
                lines.Add(line);
            }

            PathFigure path = new PathFigure();
            path.StartPoint = ScaledPoint(m_panel.point(0));
            path.Segments = lines;

            PathFigureCollection actualPath = new PathFigureCollection();
            actualPath.Add(path);

            PathGeometry drawing = new PathGeometry();
            drawing.Figures = actualPath;

            drawingContext.DrawGeometry(Background, new Pen(Foreground, 1.0), drawing);
        }

        public PanelDisplay Copy()
        {
            PanelDisplay newDisplay = new PanelDisplay(m_panel.Copy(), scale);
            newDisplay.X = this.X;
            newDisplay.Y = this.Y;

            return newDisplay;
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

        public void Rotate(double angle)
        {
            Size size = m_panel.GetSize();

            m_panel.Rotate(new Point(size.Width/2, size.Height/2), angle);
            InvalidateVisual();
        }

        public void HorizontalFlip()
        {
            m_panel.HorizontalFlip();
            InvalidateVisual();
        }

        public void VerticalFlip()
        {
            m_panel.VerticalFlip();
            InvalidateVisual();
        }
    }
}
