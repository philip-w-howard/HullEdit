using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    public class HullManip : ContentControl

    {
        private const int RECT_SIZE = 5;
        private const double CLICK_WIDTH = 3;
        public enum PerspectiveType { FRONT, TOP, SIDE, PERSPECTIVE };

        public PerspectiveType perspective { get; set; }

        private Rect[] m_handle;
        private int m_DraggingHandle;
        private bool m_Dragging;
        private Hull m_Hull;
        private HullDisplay m_HullDisplay;

        private int m_SelectedBulkhead;

        public int SelectedBulkhead
        {
            get { return m_SelectedBulkhead; }
            set { m_SelectedBulkhead = value; m_handle = null; }
        }

        private double m_dragStartX, m_dragStartY;

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

        public HullManip()
        {
            m_isEditable = false;
        }

        public void SetHull(Hull hull)
        {
            m_Hull = hull;
            IsEditable = false;
        }
        public void SetHullDisplay(HullDisplay hull)
        {
            m_HullDisplay = hull;
            IsEditable = false;
        }


        private void DrawHandles(DrawingContext drawingContext)
        {
            // If necessary, create new handles for current SelectedBulkhead
            if (m_handle == null)
            {
                m_handle = new Rect[m_HullDisplay.numChines];

                for (int ii = 0; ii < m_HullDisplay.numChines; ii++)
                {
                    m_handle[ii] = new Rect();
                    m_handle[ii].Height = RECT_SIZE;
                    m_handle[ii].Width = RECT_SIZE;
                    m_handle[ii].X = m_HullDisplay.hull.GetBulkhead(SelectedBulkhead).GetPoint(ii).X - RECT_SIZE / 2;
                    m_handle[ii].Y = m_HullDisplay.hull.GetBulkhead(SelectedBulkhead).GetPoint(ii).Y - RECT_SIZE / 2;
                }
            }

            // Draw handles
            for (int ii = 0; ii < m_HullDisplay.numChines; ii++)
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.Red), 1), m_handle[ii]);
            }
        }
        public void Draw()
        {
            InvalidateVisual();
        }

        public void Rotate(double x, double y, double z)
        {
            m_HullDisplay.Rotate(x, y, z);
            perspective = PerspectiveType.PERSPECTIVE;
            m_isEditable = false;
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

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            int selectedBulkhead;
            Point loc = e.GetPosition(this);

            if (IsEditable)
            {
                if (ClickedHandle(loc))
                {
                    Debug.WriteLine("Clicked handle");
                }
                else if (m_HullDisplay.NearBulkhead(loc, CLICK_WIDTH, out selectedBulkhead))
                {
                    Debug.WriteLine("Clicked bulkhead");
                    m_SelectedBulkhead = selectedBulkhead;
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
                //double x, y, z;

                //if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
                //{
                //    // Front
                //    x = -(m_dragStartX - loc.X) / m_scale;
                //    y = (m_dragStartY - loc.Y) / m_scale;
                //    z = 0;
                //}
                //else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 180)
                //{
                //    // Side
                //    x = 0;
                //    y = (m_dragStartY - loc.Y) / m_scale;
                //    z = -(m_dragStartX - loc.X) / m_scale;
                //}
                //else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
                //{
                //    // Top
                //    x = -(m_dragStartY - loc.Y) / m_scale;
                //    y = 0;
                //    z = -(m_dragStartX - loc.X) / m_scale;
                //}
                //else
                //{
                //    x = 0;
                //    y = 0;
                //    z = 0;
                //}

                //m_Hull.UpdateBulkheadPoint(SelectedBulkhead, m_DraggingHandle, x, y, z);
                m_Dragging = false;

                // Note: RotateTo reloads m_drawnBulkheads from the m_Hull
                //RotateTo(m_rotate_x, m_rotate_y, m_rotate_z);
                //Scale();
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

                Draw();
            }

        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    if (m_Hull == null || !m_Hull.IsValid) return;

        //    Debug.WriteLine("OnRender");

        //    Rect background = new Rect(new Point(0, 0), new Point(mActualWidth, mActualHeight));
        //    drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), null, background);

        //    Pen pen = new Pen(System.Windows.Media.Brushes.Black, 1.0);

        //    for (int bulkhead = 0; bulkhead < m_Hull.numBulkheads; bulkhead++)
        //    {
        //        for (int chine = 0; chine < m_drawnBulkheads[bulkhead].Count - 1; chine++)
        //        {
        //            if (chine != m_drawnBulkheads[bulkhead].Count / 2 - 1)
        //            {
        //                Point p1 = new Point(m_drawnBulkheads[bulkhead].GetPoint(chine).X, m_drawnBulkheads[bulkhead].GetPoint(chine).Y);
        //                Point p2 = new Point(m_drawnBulkheads[bulkhead].GetPoint(chine + 1).X, m_drawnBulkheads[bulkhead].GetPoint(chine + 1).Y);

        //                drawingContext.DrawLine(pen, p1, p2);
        //            }
        //        }
        //    }

        //    pen = new Pen(System.Windows.Media.Brushes.Gray, 1.0);

        //    for (int chine = 0; chine < m_Hull.numChines * 2; chine++)
        //    {
        //        for (int point = 0; point < m_chines[chine].Count - 1; point++)
        //        {
        //            Point p1 = new Point(m_chines[chine][point].X, m_chines[chine][point].Y);
        //            Point p2 = new Point(m_chines[chine][point + 1].X, m_chines[chine][point + 1].Y);

        //            drawingContext.DrawLine(pen, p1, p2);
        //        }
        //    }

        //    if (IsEditable) DrawHandles(drawingContext);
        //}
        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    Debug.WriteLine("HullManip.MeasureOverride {0} {1}", availableSize.Width, availableSize.Height);

        //    UIElement content = (UIElement)this.Content;

        //    content.Measure(availableSize);
        //    return content.DesiredSize;
        //}
        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    Debug.WriteLine("HullManip.ArrangeOverride {0} {1}", finalSize.Width, finalSize.Height);
        //    UIElement content = (UIElement)this.Content;
        //    Point location = new Point();

        //    content.Arrange(new Rect(location, content.DesiredSize));
        //    return finalSize;

        //    //if (mActualWidth != finalSize.Width || mActualHeight != finalSize.Height)
        //    //{
        //    //    mActualWidth = finalSize.Width;
        //    //    mActualHeight = finalSize.Height;

        //    //    if (m_Hull != null && m_Hull.IsValid)
        //    //    {
        //    //        Scale();
        //    //    }
        //    //}
        //    //return finalSize;
        //}

    }
}
