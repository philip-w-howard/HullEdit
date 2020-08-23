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

        public enum OutputFormatType { DECIMAL_FULL, DECIMAL_2, DECIMAL_3, DECIMAL_4, EIGHTHS, SIXTEENTHS, DECIMAL_8THS, DECIMAL_16THS };
        public OutputFormatType OutputFormat { get; set; }

        private double m_scale;
        public double scale
        {
            get { return m_scale; }
            set { m_scale = value; InvalidateVisual(); }
        }

        protected Panel m_panel;

        public double X { get; set; }
        public double Y { get; set; }

        public PanelDisplay(Panel p, double scale)
        {
            m_panel = p.Copy();
            this.scale = scale;
            OutputFormat = OutputFormatType.SIXTEENTHS;
        }

        private Point ScaledPoint(Point point)
        {
            point.X *= scale;
            point.Y *= scale;
            return point;
        }

        public Size size
        {
            get
            {
                Size s = m_panel.GetSize();
                s.Width *= scale;
                s.Height *= scale;
                return s;
            }
        }

        // Draw the panel.
        // This requires a horrible hierarchy of classes, but it allows us to draw a filled figure,
        // which makes it clickable.
        protected override void OnRender(DrawingContext drawingContext)
        {
            Debug.WriteLine("PanelDisplay.OnRender");
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
            newDisplay.scale = this.scale;
            newDisplay.OutputFormat = this.OutputFormat;

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

            Size s = size;

            Width = s.Width;
            Height = s.Height;

            Debug.WriteLine("PanelDisplay MeasureOverride {0} {1}", Width, Height);
            return s;
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

        private String formatPoint(Point point)
        {
            point.X += X;
            point.Y += Y;

            String result = "";
            int value;
            int ones;
            int fraction;
            switch (OutputFormat)
            {
                case OutputFormatType.SIXTEENTHS:
                    value = (int)Math.Round(point.X * 16);
                    ones = value / 16;
                    fraction = value % 16;
                    result += ones + " " + fraction + "/16, ";

                    value = (int)Math.Round(point.Y * 16.0);
                    ones = value / 16;
                    fraction = value % 16;
                    result += ones + " " + fraction + "/16\n";
                    break;
                default:
                    result += point.X + ", ";
                    result += point.Y + "/n";
                    break;
            }
            return result;
        }
        public override string ToString()
        {
            String result = "\n";

            for (int ii=0; ii<m_panel.NumPoints; ii++)
            {
                result += formatPoint(m_panel.point(ii));
            }

            return result;
        }
    }
}
