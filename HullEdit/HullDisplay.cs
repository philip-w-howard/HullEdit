using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HullEdit
{
    class HullDisplay : System.Windows.Controls.Control
    {
        private double[][,] m_chines;           // [chine][index, axis]
        private double[][,] m_drawnBulkheads;   // [bulkhead][chine, axis]
        private const int POINTS_PER_CHINE = 50;
        private int points_in_chine;

        private double m_rotate_x, m_rotate_y, m_rotate_z;
        private double m_translate_x, m_translate_y, m_translate_z;
        private double m_scale = 1;

        private double mActualHeight, mActualWidth;

        private Hull m_Hull;

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
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (m_Hull == null) return;

            Debug.WriteLine("OnRender");

            Rect background = new Rect(new Point(0, 0), new Point(mActualWidth, mActualHeight));
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), null, background);

            Pen pen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_drawnBulkheads[bulkhead].GetLength(0) - 1; chine++)
                {
                    if (chine != m_drawnBulkheads[bulkhead].GetLength(0) / 2 - 1)
                    {
                        Point p1 = new Point(m_drawnBulkheads[bulkhead][chine, 0], m_drawnBulkheads[bulkhead][chine, 1]);
                        Point p2 = new Point(m_drawnBulkheads[bulkhead][chine + 1, 0], m_drawnBulkheads[bulkhead][chine + 1, 1]);

                        drawingContext.DrawLine(pen, p1, p2);
                    }
                }
            }

            pen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

            for (int chine = 0; chine < m_Hull.numChines * 2; chine++)
            {
                for (int point = 0; point < points_in_chine - 1; point++)
                {
                    Point p1 = new Point(m_chines[chine][point, 0], m_chines[chine][point, 1]);
                    Point p2 = new Point(m_chines[chine][point + 1, 0], m_chines[chine][point + 1, 1]);

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
                    m_handle[ii].X = m_drawnBulkheads[SelectedBulkhead][ii, 0] - RECT_SIZE / 2;
                    m_handle[ii].Y = m_drawnBulkheads[SelectedBulkhead][ii, 1] - RECT_SIZE / 2;
                }
            }

            // Draw handles
            for (int ii = 0; ii < m_Hull.numChines; ii++)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.Red), 1), m_handle[ii]);
            }

        }
        private void LoadBulkheads()
        {
            // reset values that depend on the bulkheads
            m_handle = null;
            m_scale = 1.0;

            m_drawnBulkheads = new double[m_Hull.numBulkheads][,];
            int centerChine = m_Hull.numChines;

            for (int ii = 0; ii < m_Hull.numBulkheads; ii++)
            {
                m_drawnBulkheads[ii] = new double[m_Hull.numChines * 2, 3];
            }

            m_Hull.CopyBulkheads(m_drawnBulkheads);

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_Hull.numChines; chine++)
                {
                    for (int axis = 0; axis < 3; axis++)
                    {
                        m_drawnBulkheads[bulkhead][chine + centerChine, axis] = m_drawnBulkheads[bulkhead][chine, axis];
                    }

                    // mirror the X
                    m_drawnBulkheads[bulkhead][chine + centerChine, 0] *= -1;
                }
            }
        }

        public void Scale()
        {
            // Get size
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;

            double chine_min_x = double.MaxValue;
            double chine_min_y = double.MaxValue;
            double chine_max_x = double.MinValue;
            double chine_max_y = double.MinValue;

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_drawnBulkheads[bulkhead].GetLength(0); chine++)
                {
                    double x = m_drawnBulkheads[bulkhead][chine, 0];
                    double y = m_drawnBulkheads[bulkhead][chine, 1];
                    if (x > max_x) max_x = x;
                    if (y > max_y) max_y = y;
                    if (x < min_x) min_x = x;
                    if (y < min_y) min_y = y;
                }
            }
            //        private double[][,] m_chines;           // [chine][index, axis]

            for (int chine = 0; chine < m_Hull.numChines * 2; chine++)
            {
                for (int point = 0; point < points_in_chine; point++)
                {
                    double x = m_chines[chine][point, 0];
                    double y = m_chines[chine][point, 1];
                    if (x > chine_max_x) chine_max_x = x;
                    if (y > chine_max_y) chine_max_y = y;
                    if (x < chine_min_x) chine_min_x = x;
                    if (y < chine_min_y) chine_min_y = y;
                }
            }
            // Scale all the points to fit in the canvas
            double scale1 = mActualWidth / (max_x - min_x);
            double scale2 = mActualHeight / (max_y - min_y);
            double scale3 = mActualWidth / (chine_max_x - chine_min_x);
            double scale4 = mActualHeight / (chine_max_y - chine_min_y);

            double new_scale;

            new_scale = Math.Min(Math.Min(scale1, scale2), Math.Min(scale3, scale4));
            //if (scale2 < new_scale) new_scale = scale2;
            new_scale = 0.9 * new_scale;

            m_scale *= new_scale;
            Debug.WriteLine("Scale: {0}", m_scale);

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_drawnBulkheads[bulkhead].GetLength(0); chine++)
                {
                    m_drawnBulkheads[bulkhead][chine, 0] *= new_scale;
                    m_drawnBulkheads[bulkhead][chine, 1] *= new_scale;
                    m_drawnBulkheads[bulkhead][chine, 2] *= new_scale;
                }
            }

            for (int chine = 0; chine < m_Hull.numChines * 2; chine++)
            {
                for (int point = 0; point < points_in_chine; point++)
                {
                    m_chines[chine][point, 0] *= new_scale;
                    m_chines[chine][point, 1] *= new_scale;
                    m_chines[chine][point, 2] *= new_scale;
                }
            }

            CenterTo(mActualWidth / 2, mActualHeight / 2, 0);
        }

        protected void PrepareChines()
        {
            m_chines = new double[m_Hull.numChines * 2][,];
            double[,] chine_data = new double[m_Hull.numBulkheads, 3];
            for (int chine = 0; chine < m_Hull.numChines * 2; chine++)
            {
                int actual_chine = chine;
                if (chine >= m_Hull.numChines) actual_chine = chine - m_Hull.numChines;

                m_chines[chine] = new double[POINTS_PER_CHINE, 3];
                for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
                {
                    for (int axis = 0; axis < 3; axis++)
                    {
                        chine_data[bulkhead, axis] = m_drawnBulkheads[bulkhead][actual_chine, axis];
                    }

                    if (chine >= m_Hull.numChines) chine_data[bulkhead, 0] *= -1;
                }
                Splines spline = new Splines(m_Hull.numBulkheads, Splines.RELAXED, chine_data);
                points_in_chine = spline.GetPoints(m_chines[chine]);
            }
        }

        protected void RotateDrawing_X(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[0, 0] = 1.0;
            rotate[1, 1] = Math.Cos(angle);
            rotate[2, 2] = Math.Cos(angle);
            rotate[1, 2] = Math.Sin(angle);
            rotate[2, 1] = -Math.Sin(angle);

            CenterTo(0, 0, 0);

            for (int ii = 0; ii < m_Hull.numBulkheads; ii++)
            {
                Matrix.Multiply(m_drawnBulkheads[ii], rotate, m_drawnBulkheads[ii]);
            }

            for (int ii = 0; ii < m_Hull.numChines * 2; ii++)
            {
                Matrix.Multiply(m_chines[ii], rotate, m_chines[ii]);
            }
        }

        protected void RotateDrawing_Y(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[1, 1] = 1.0;
            rotate[0, 0] = Math.Cos(angle);
            rotate[2, 2] = Math.Cos(angle);
            rotate[2, 0] = Math.Sin(angle);
            rotate[0, 2] = -Math.Sin(angle);

            CenterTo(0, 0, 0);

            for (int ii = 0; ii < m_Hull.numBulkheads; ii++)
            {
                Matrix.Multiply(m_drawnBulkheads[ii], rotate, m_drawnBulkheads[ii]);
            }

            for (int ii = 0; ii < m_Hull.numChines * 2; ii++)
            {
                Matrix.Multiply(m_chines[ii], rotate, m_chines[ii]);
            }
        }

        protected void RotateDrawing_Z(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[2, 2] = 1.0;
            rotate[0, 0] = Math.Cos(angle);
            rotate[1, 1] = Math.Cos(angle);
            rotate[0, 1] = Math.Sin(angle);
            rotate[1, 0] = -Math.Sin(angle);

            CenterTo(0, 0, 0);

            for (int ii = 0; ii < m_Hull.numBulkheads; ii++)
            {
                Matrix.Multiply(m_drawnBulkheads[ii], rotate, m_drawnBulkheads[ii]);
            }

            for (int ii = 0; ii < m_Hull.numChines * 2; ii++)
            {
                Matrix.Multiply(m_chines[ii], rotate, m_chines[ii]);
            }
        }

        public void RotateTo(double x, double y, double z)
        {
            LoadBulkheads();
            PrepareChines();

            m_rotate_x = x;
            m_rotate_y = y;
            m_rotate_z = z;

            // NOTE: Could optimize by multiplying the three rotation matrices before rotating the points
            RotateDrawing_Z(z);
            RotateDrawing_X(x);
            RotateDrawing_Y(y);
        }

        private void CenterTo(double centerX, double centerY, double centerZ)
        {
            // Get size
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;
            double max_z = double.MinValue;

            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_drawnBulkheads[bulkhead].GetLength(0); chine++)
                {
                    double x = m_drawnBulkheads[bulkhead][chine, 0];
                    double y = m_drawnBulkheads[bulkhead][chine, 1];
                    double z = m_drawnBulkheads[bulkhead][chine, 2];
                    if (x > max_x) max_x = x;
                    if (y > max_y) max_y = y;
                    if (z > max_z) max_z = z;
                    if (x < min_x) min_x = x;
                    if (y < min_y) min_y = y;
                    if (z < min_z) min_z = z;
                }
            }

            m_translate_x = centerX - (max_x + min_x) / 2;
            m_translate_y = centerY - (max_y + min_y) / 2;
            m_translate_z = centerZ - (max_z + min_z) / 2;

            Debug.WriteLine("CenterTo ({0},{1},{2}) Shift ({3},{4},{5})",
                centerX, centerY, centerZ, m_translate_x, m_translate_y, m_translate_z);
            TranslateTo(m_translate_x, m_translate_y, m_translate_z);
        }

        private void TranslateTo(double shiftX, double shiftY, double shiftZ)
        {
            for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_drawnBulkheads[bulkhead].GetLength(0); chine++)
                {
                    m_drawnBulkheads[bulkhead][chine, 0] += shiftX;
                    m_drawnBulkheads[bulkhead][chine, 1] += shiftY;
                    m_drawnBulkheads[bulkhead][chine, 2] += shiftZ;
                }
            }

            for (int ii = 0; ii < m_Hull.numChines * 2; ii++)
            {
                for (int point = 0; point < points_in_chine; point++)
                {
                    m_chines[ii][point, 0] += shiftX;
                    m_chines[ii][point, 1] += shiftY;
                    m_chines[ii][point, 2] += shiftZ;
                }
            }
        }

        public void Draw()
        {
            InvalidateVisual();
        }

        protected bool ClickedHandle(Point loc)
        {
            Debug.WriteLine("Checking handles");
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
                Debug.WriteLine("Checking Vertical: ({0}, {1}), {2}", y1, y2, y3);
                // is point along segment?
                if ((y1 <= y3 && y2 >= y3) || (y1 >= y3 && y2 <= y3))
                {
                    Debug.WriteLine("Along segment");
                    if (Math.Abs(x1 - x3) <= delta) return true;
                }

                return false;
            }
            else if (y1 == y2) // horizontal line
            {
                Debug.WriteLine("Checking Horizontal: ({0}, {1}), {2}", x1, x2, x3);
                // is point along segment?
                if ((x1 <= x3 && x2 >= x3) || (x1 >= x3 && x2 <= x3))
                {
                    Debug.WriteLine("Along segment");
                    if (Math.Abs(y1 - y3) <= delta) return true;
                }

                return false;
            }
            else // sloped line
            {
                double m1, m2;
                double b1, b2;
                double x, y;

                Debug.WriteLine("Checking Sloped: ({0}, {1}), ({2}, {3}), ({4}, {5})", x1, y1, x2, y2, x3, y3);

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
            Debug.WriteLine("Checking bulkheads");
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < numChines - 1; chine++)
                {
                    Debug.WriteLine("Checking Bulkhead {0} Chine {1}", bulkhead, chine);
                    if (IsNearLine(m_drawnBulkheads[bulkhead][chine, 0], m_drawnBulkheads[bulkhead][chine, 1],
                            m_drawnBulkheads[bulkhead][chine + 1, 0], m_drawnBulkheads[bulkhead][chine + 1, 1],
                            loc.X, loc.Y, 3))
                    {
                        Debug.WriteLine("Selected bulkhead {0}", bulkhead);

                        m_SelectedBulkhead = bulkhead;
                        m_handle = null;
                        Draw();
                        return true;
                    }

                    // check for reflected bulkheads in front and top views
                    if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
                    {
                        // Front
                        if (IsNearLine(m_drawnBulkheads[bulkhead][chine, 0], m_drawnBulkheads[bulkhead][chine, 1],
                                m_drawnBulkheads[bulkhead][chine + 1, 0], m_drawnBulkheads[bulkhead][chine + 1, 1],
                                mActualWidth - loc.X, loc.Y, 3))
                        {
                            Debug.WriteLine("Selected bulkhead {0}", bulkhead);

                            m_SelectedBulkhead = bulkhead;
                            m_handle = null;
                            Draw();
                            return true;
                        }
                    }
                    else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
                    {
                        // Top
                        if (IsNearLine(m_drawnBulkheads[bulkhead][chine, 0], m_drawnBulkheads[bulkhead][chine, 1],
                                m_drawnBulkheads[bulkhead][chine + 1, 0], m_drawnBulkheads[bulkhead][chine + 1, 1],
                                loc.X, loc.Y + mActualHeight / 2, 3))
                        {
                            Debug.WriteLine("Selected bulkhead {0}", bulkhead);

                            m_SelectedBulkhead = bulkhead;
                            m_handle = null;
                            Draw();
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            Point loc = e.GetPosition(this);

            Debug.WriteLine("PreviewMouseDown ({0},{1}) {2}", loc.X, loc.Y, e.ButtonState);

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
            Point loc = e.GetPosition(this);

            if (m_Dragging)
            {
                double x, y, z;

                if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
                {
                    // Front
                    x = -(m_dragStartX - loc.X) / m_scale;
                    y = (m_dragStartY - loc.Y) / m_scale;
                    z = 0;
                }
                else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 180)
                {
                    // Side
                    x = 0;
                    y = (m_dragStartY - loc.Y) / m_scale;
                    z = -(m_dragStartX - loc.X) / m_scale;
                }
                else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
                {
                    // Top
                    x = -(m_dragStartY - loc.Y) / m_scale;
                    y = 0;
                    z = -(m_dragStartX - loc.X) / m_scale;
                }
                else
                {
                    x = 0;
                    y = 0;
                    z = 0;
                }

                Debug.WriteLine("Shifting [{0} {1}] by {2} {3} {4}", SelectedBulkhead, m_DraggingHandle, x, y, z);

                m_Hull.ShiftBulkheadPoint(SelectedBulkhead, m_DraggingHandle, x, y, z);
                m_Dragging = false;

                // Note: RotateTo reloads m_drawnBulkheads from the m_Hull
                RotateTo(m_rotate_x, m_rotate_y, m_rotate_z);
                Scale();
                Draw();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (m_Dragging)
            {
                Point loc = e.GetPosition(this);
                m_handle[m_DraggingHandle].X = loc.X - RECT_SIZE / 2;
                m_handle[m_DraggingHandle].Y = loc.Y - RECT_SIZE / 2;

                Debug.WriteLine("Moved {0} to {1},{2}", m_DraggingHandle, loc.X, loc.Y);
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
                    Scale();
                }
            }
            return finalSize;
        }
    }
}
