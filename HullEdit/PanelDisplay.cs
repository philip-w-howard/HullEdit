using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
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

        public PanelDisplay(SerializablePanelDisplay display)
        {
            OutputFormat = display.OutputFormat;
            m_scale = display.scale;
            X = display.X;
            Y = display.Y;
            m_panel = new Panel(display.panel);
        }

        private Point ScaledPoint(Point point)
        {
            point.X *= scale;
            point.Y *= scale;
            return point;
        }
        private Point AbsolutePoint(Point point)
        {
            point.X += X;
            point.Y += Y;
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

        public PointCollection GetPoints()
        {
            PointCollection points = new PointCollection();
            
            for (int ii = 1; ii < m_panel.NumPoints; ii++)
            {
                Point p = AbsolutePoint(m_panel.point(ii));
                points.Add(p);
            }

            // close the path
            points.Add(AbsolutePoint(m_panel.point(0)));

            return points;
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
            point.X += X/scale;
            point.Y += Y/scale;

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

            if (m_panel.name != null && m_panel.name.Length > 0) result += m_panel.name;
            result += " (" + X + ", " + Y + ")\n";

            for (int ii=0; ii<m_panel.NumPoints; ii++)
            {
                result += formatPoint(m_panel.point(ii));
            }

            return result;
        }

        public class SerializablePanelDisplay
        {
            public OutputFormatType OutputFormat;
            public double scale;
            public Panel.SerializablePanel panel;
            public double X, Y;

            public SerializablePanelDisplay() { }
            public SerializablePanelDisplay(PanelDisplay display)
            {
                scale = display.scale;
                panel = new Panel.SerializablePanel(display.m_panel);
                X = display.X;
                Y = display.Y;
            }
        }

        public bool Split(double start, double radius, double depth, out Panel panel_1, out Panel panel_2)
        {
            bool addTo_1 = true;
            PointCollection points_1 = new PointCollection();
            PointCollection points_2 = new PointCollection();
            Point points_2_start = new Point(0,0);
            Point top = new Point();
            Point bottom = new Point();


            for (int ii=0; ii<m_panel.NumPoints-1; ii++)
            {
                Point first = m_panel.point(ii);
                Point second = m_panel.point(ii + 1);
                Point startPoint = new Point();
                double min = Math.Min(first.X, second.X);
                double max = Math.Max(first.X, second.X);
                if (min <= start && max >= start)
                {
                    if (first.X == start)
                    {
                        startPoint = first;
                    }
                    else if (second.X == start)
                    {
                        startPoint = second;
                    }
                    else
                    {
                        // need to find point on line between first and second
                        // NOTE: because of the above conditions, we can't have a vertical line. They are ruled out.
                        double slope = (second.Y - first.Y) / (second.X - first.X);
                        startPoint = new Point(start, first.Y + slope * (start - first.X));
                    }

                    if (addTo_1)
                    {
                        top = startPoint;
                        if (first != startPoint) points_1.Add(first);
                        points_2.Add(top);
                    }
                    else
                    {
                        bottom = startPoint;

                        if (first != startPoint) points_2.Add(first);
                        PointCollection splitter_1 = PanelSplitter.Tongues(top, bottom, (int)radius, depth);
                        IEnumerable<Point> splitter_2 = splitter_1.Reverse();

                        foreach (Point p in splitter_1)
                        {
                            points_1.Add(p);
                        }

                        foreach (Point p in splitter_2)
                        {
                            points_2.Add(p);
                        }
                    }

                    addTo_1 = !addTo_1;
                }
                else
                {
                    if (addTo_1)
                        points_1.Add(m_panel.point(ii));
                    else
                        points_2.Add(m_panel.point(ii));
                }
            }

            // close the panels
            points_1.Add(points_1[0]);
            //points_2.Add(top);

            panel_1 = new Panel(points_1);
            panel_2 = new Panel(points_2);

            panel_1.name = m_panel.name + "A";
            panel_2.name = m_panel.name + "B";

            return true;
        }

    }
}
