using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace HullEdit
{
    class HullDisplay : System.Windows.Controls.Control
    {
        private const int POINTS_PER_CHINE = 50;

        private double m_scale = 1;

        private double mActualHeight, mActualWidth;

        private Hull m_Hull;
        public Hull hull{ get { return m_Hull; } }


        const int RECT_SIZE = 8;

        private Rect[] m_handle;
        private int m_DraggingHandle;
        private bool m_Dragging;

        private int m_SelectedBulkhead;

        public int SelectedBulkhead
        {
            get { return m_SelectedBulkhead; }
            set { m_SelectedBulkhead = value; m_handle = null; }
        }

        private double m_dragStartX, m_dragStartY;

        public int numChines { get { return m_Hull.numChines; } }
        public int numBulkheads { get { return m_Hull.numBulkheads; } }

        private bool m_isEditable;
        public bool IsEditable
        {
            get { return m_isEditable; }
            set
            {
                m_isEditable = value;
                if (value == false) m_handle = null;
            }
        }

        public HullDisplay()
        {
            mActualHeight = ActualHeight;
            mActualWidth = ActualWidth;
        }

        public void SetHull(Hull hull)
        {
            m_Hull = hull;
            IsEditable = false;
            Draw();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (m_Hull == null || !m_Hull.IsValid) return;

            Debug.WriteLine("OnRender");

            Rect background = new Rect(new Point(0, 0), new Point(mActualWidth, mActualHeight));
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), null, background);

            Pen pen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                Bulkhead bulk = m_Hull.GetBulkhead(bulkhead);
                for (int chine = 0; chine < bulk.Count - 1; chine++)
                {
                    Point p1 = new Point(bulk.GetPoint(chine).X, bulk.GetPoint(chine).Y);
                    Point p2 = new Point(bulk.GetPoint(chine+1).X, bulk.GetPoint(chine+1).Y);

                    drawingContext.DrawLine(pen, p1, p2);
                }
            }

            pen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

            for (int chine = 0; chine < m_Hull.numChines; chine++)
            {
                Point3DCollection currChine = m_Hull.GetChine(chine);

                // FIXTHIS: use a foreach and simply remember the previous point
                for (int point = 0; point < currChine.Count - 1; point++)
                {
                    Point p1 = new Point(currChine[point].X, currChine[point].Y);
                    Point p2 = new Point(currChine[point + 1].X, currChine[point + 1].Y);

                    drawingContext.DrawLine(pen, p1, p2);
                }
            }

            if (IsEditable) DrawHandles(drawingContext);
        }

        private void DrawHandles(DrawingContext drawingContext)
        {
            // If necessary, create new handles for current SelectedBulkhead
            if (m_handle == null)
            {
                m_handle = new Rect[m_Hull.numChines];

                for (int ii = 0; ii < m_Hull.numChines; ii++)
                {
                    m_handle[ii] = new Rect();
                    m_handle[ii].Height = RECT_SIZE;
                    m_handle[ii].Width = RECT_SIZE;
                    m_handle[ii].X = m_Hull.GetBulkhead(SelectedBulkhead).GetPoint(ii).X - RECT_SIZE / 2;
                    m_handle[ii].Y = m_Hull.GetBulkhead(SelectedBulkhead).GetPoint(ii).Y - RECT_SIZE / 2;
                }
            }

            // Draw handles
            for (int ii = 0; ii < m_Hull.numChines; ii++)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.Red), 1), m_handle[ii]);
            }

        }
        public void Scale(double xSize, double ySize)
        {
            Size3D hullSize = m_Hull.GetSize();

            // Scale all the points to fit in the canvas
            double scale1 = xSize / hullSize.X;
            double scale2 = ySize / hullSize.Y;

            double new_scale = 0.9 * Math.Min(scale1, scale2);

            Debug.WriteLine("Scale: {0}", new_scale);

            m_Hull.Scale(new_scale, new_scale, new_scale);
        }


        public void Draw()
        {
            InvalidateVisual();
        }

        protected bool ClickedHandle(Point loc)
        {
            if (m_handle == null || m_handle[0] == null) return false;

            for (int ii = 0; ii < m_handle.Length; ii++)
            {
                if (m_handle[ii].X <= loc.X && m_handle[ii].X + m_handle[ii].Width >= loc.X &&
                    m_handle[ii].Y <= loc.Y && m_handle[ii].Y + m_handle[ii].Height >= loc.Y)
                {
                    m_DraggingHandle = ii;
                    m_Dragging = true;
                    m_dragStartX = loc.X;
                    m_dragStartY = loc.Y;

                    //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    return true;
                }
            }

            return false;
        }

        protected bool IsNearLine(double x1, double y1, double x2, double y2, double x3, double y3, double delta)
        {
            if (x1 == x2) // vertical line
            {
                // is point along segment?
                if ((y1 <= y3 && y2 >= y3) || (y1 >= y3 && y2 <= y3))
                {
                    if (Math.Abs(x1 - x3) <= delta) return true;
                }

                return false;
            }
            else if (y1 == y2) // horizontal line
            {
                // is point along segment?
                if ((x1 <= x3 && x2 >= x3) || (x1 >= x3 && x2 <= x3))
                {
                    if (Math.Abs(y1 - y3) <= delta) return true;
                }

                return false;
            }
            else // sloped line
            {
                double m1, m2;
                double b1, b2;
                double x, y;

                // compute slope between first two points:
                m1 = (y2 - y1) / (x2 - x1);

                // y intercept for first line
                b1 = -m1 * x1 + y1;

                // compute slope of second (perpendicular) line
                m2 = -1 / m1;

                // y intercept for second (perpendicular) line
                b2 = -m2 * x3 + y3;

                // Itersection of the two lines
                x = (b2 - b1) / (m1 - m2);
                y = m1 * x + b1;

                // is the intersection NOT within the line segment?
                if ((x <= x1 && x <= x2) || (x >= x1 && x >= x2)) return false;
                if ((y <= y1 && y <= y2) || (y >= y1 && y >= y2)) return false;

                // Is the intersection within delta of the point?
                double distance = Math.Sqrt((x - x3) * (x - x3) + (y - y3) * (y - y3));
                if (distance <= delta) return true;

                return false;
            }
        }
        protected bool ClickedBulkhead(Point loc)
        {
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < numChines - 1; chine++)
                {
                    Point3D p1 = m_Hull.GetBulkhead(bulkhead).GetPoint(chine);
                    Point3D p2 = m_Hull.GetBulkhead(bulkhead).GetPoint(chine+1);

                    if (IsNearLine(p1.X, p1.Y, p2.X, p2.Y, loc.X, loc.Y, 3))
                    {
                        m_SelectedBulkhead = bulkhead;
                        m_handle = null;
                        Draw();
                        return true;
                    }

                    //// check for reflected bulkheads in front and top views
                    //if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
                    //{
                    //    // Front
                    //    if (IsNearLine(m_drawnBulkheads[bulkhead][chine].X, m_drawnBulkheads[bulkhead][chine].Y,
                    //            m_drawnBulkheads[bulkhead][chine + 1].X, m_drawnBulkheads[bulkhead][chine + 1].Y,
                    //            mActualWidth - loc.X, loc.Y, 3))
                    //    {
                    //        m_SelectedBulkhead = bulkhead;
                    //        m_handle = null;
                    //        Draw();
                    //        return true;
                    //    }
                    //}
                    //else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
                    //{
                    //    // Top
                    //    if (IsNearLine(m_drawnBulkheads[bulkhead][chine].X, m_drawnBulkheads[bulkhead][chine].Y,
                    //            m_drawnBulkheads[bulkhead][chine + 1].X, m_drawnBulkheads[bulkhead][chine + 1].Y,
                    //            loc.X, loc.Y + mActualHeight / 2, 3))
                    //    {
                    //        m_SelectedBulkhead = bulkhead;
                    //        m_handle = null;
                    //        Draw();
                    //        return true;
                    //    }
                    //}
                }
            }
            return false;
        }
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            if (IsEditable)
            {
                if (ClickedHandle(loc))
                {
                    Debug.WriteLine("Clicked handle");
                }
                else if (ClickedBulkhead(loc))
                {
                    Debug.WriteLine("Clicked bulkhead");
                }
                else
                {
                    Debug.WriteLine("Clicked nothing");
                }
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            //Point loc = e.GetPosition(this);

            //if (m_Dragging)
            //{
            //    double x, y, z;

            //    if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
            //    {
            //        // Front
            //        x = -(m_dragStartX - loc.X) / m_scale;
            //        y = (m_dragStartY - loc.Y) / m_scale;
            //        z = 0;
            //    }
            //    else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 180)
            //    {
            //        // Side
            //        x = 0;
            //        y = (m_dragStartY - loc.Y) / m_scale;
            //        z = -(m_dragStartX - loc.X) / m_scale;
            //    }
            //    else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
            //    {
            //        // Top
            //        x = -(m_dragStartY - loc.Y) / m_scale;
            //        y = 0;
            //        z = -(m_dragStartX - loc.X) / m_scale;
            //    }
            //    else
            //    {
            //        x = 0;
            //        y = 0;
            //        z = 0;
            //    }

            //    m_Hull.UpdateBulkheadPoint(SelectedBulkhead, m_DraggingHandle, x, y, z);
            //    m_Dragging = false;

            //    // Note: RotateTo reloads m_drawnBulkheads from the m_Hull
            //    RotateTo(m_rotate_x, m_rotate_y, m_rotate_z);
            //    Scale();
            //    Draw();
            //}
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (m_Dragging)
            {
                Point loc = e.GetPosition(this);
                m_handle[m_DraggingHandle].X = loc.X - RECT_SIZE / 2;
                m_handle[m_DraggingHandle].Y = loc.Y - RECT_SIZE / 2;

                Draw();
            }

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine("MeasureOverride {0} {1}", availableSize.Width, availableSize.Height);
            return new Size(availableSize.Width, availableSize.Height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine("ArrangeOverride {0} {1}", finalSize.Width, finalSize.Height);

            if (mActualWidth != finalSize.Width || mActualHeight != finalSize.Height)
            {
                mActualWidth = finalSize.Width;
                mActualHeight = finalSize.Height;

                if (m_Hull != null && m_Hull.IsValid)
                {
                    // FIXTHIS: Scale();
                }
            }
            return finalSize;
        }
    }
}
