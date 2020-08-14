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
    public class HullDisplay : System.Windows.Controls.Control
    {
        private const int POINTS_PER_CHINE = 50;

        private double m_scale = 1;

        private Hull m_Hull;
        public Hull hull{ get { return m_Hull; } }

        public int numChines { get { return m_Hull.numChines; } }
        public int numBulkheads { get { return m_Hull.numBulkheads; } }

        public HullDisplay()
        {
            m_scale = 1;
        }

        public void SetHull(Hull hull)
        {
            m_Hull = hull;
            m_scale = 1;
            Draw();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (m_Hull == null || !m_Hull.IsValid) return;

            Debug.WriteLine("OnRender");

            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
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
        }

        public void Rotate(double x, double y, double z)
        {
            m_Hull.Rotate(x, y, z);
            InvalidateVisual();
        }

        protected Size size2d()
        {
            Size3D hullSize = m_Hull.GetSize();

            return new Size(hullSize.X, hullSize.Y);
        }
        protected void Scale(Size size)
        {
            Size3D hullSize = m_Hull.GetSize();

            // Scale all the points to fit in the canvas
            double scale1 = size.Width / hullSize.X;
            double scale2 = size.Height / hullSize.Y;

            double new_scale = 0.9 * Math.Min(scale1, scale2);

            Debug.WriteLine("Scale: {0}", new_scale);

            m_scale *= new_scale;

            m_Hull.Scale(new_scale, new_scale, new_scale);
        }


        public void Draw()
        {
            InvalidateVisual();
        }

        //protected bool ClickedHandle(Point loc)
        //{
        //    if (m_handle == null || m_handle[0] == null) return false;

        //    for (int ii = 0; ii < m_handle.Length; ii++)
        //    {
        //        if (m_handle[ii].X <= loc.X && m_handle[ii].X + m_handle[ii].Width >= loc.X &&
        //            m_handle[ii].Y <= loc.Y && m_handle[ii].Y + m_handle[ii].Height >= loc.Y)
        //        {
        //            m_DraggingHandle = ii;
        //            m_Dragging = true;
        //            m_dragStartX = loc.X;
        //            m_dragStartY = loc.Y;

        //            //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        //            return true;
        //        }
        //    }

        //    return false;
        //}

        public bool NearBulkhead(Point loc, double margin, out int selectedBulkhead)
        {
            selectedBulkhead = -1;
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < numChines - 1; chine++)
                {
                    Point3D p1 = m_Hull.GetBulkhead(bulkhead).GetPoint(chine);
                    Point3D p2 = m_Hull.GetBulkhead(bulkhead).GetPoint(chine+1);

                    if (Geometry.IsNearLine(p1.X, p1.Y, p2.X, p2.Y, loc.X, loc.Y, margin))
                    {
                        selectedBulkhead = bulkhead;
                        return true;
                    }
                }
            }
            return false;
        }
        //protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    Point loc = e.GetPosition(this);

        //    if (IsEditable)
        //    {
        //        if (ClickedHandle(loc))
        //        {
        //            Debug.WriteLine("Clicked handle");
        //        }
        //        else if (ClickedBulkhead(loc))
        //        {
        //            Debug.WriteLine("Clicked bulkhead");
        //        }
        //        else
        //        {
        //            Debug.WriteLine("Clicked nothing");
        //        }
        //        e.Handled = true;
        //    }
        //}
        //protected override void OnPreviewMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    //Point loc = e.GetPosition(this);

        //    //if (m_Dragging)
        //    //{
        //    //    double x, y, z;

        //    //    if (m_rotate_x == 0 && m_rotate_y == 180 && m_rotate_z == 180)
        //    //    {
        //    //        // Front
        //    //        x = -(m_dragStartX - loc.X) / m_scale;
        //    //        y = (m_dragStartY - loc.Y) / m_scale;
        //    //        z = 0;
        //    //    }
        //    //    else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 180)
        //    //    {
        //    //        // Side
        //    //        x = 0;
        //    //        y = (m_dragStartY - loc.Y) / m_scale;
        //    //        z = -(m_dragStartX - loc.X) / m_scale;
        //    //    }
        //    //    else if (m_rotate_x == 0 && m_rotate_y == 90 && m_rotate_z == 90)
        //    //    {
        //    //        // Top
        //    //        x = -(m_dragStartY - loc.Y) / m_scale;
        //    //        y = 0;
        //    //        z = -(m_dragStartX - loc.X) / m_scale;
        //    //    }
        //    //    else
        //    //    {
        //    //        x = 0;
        //    //        y = 0;
        //    //        z = 0;
        //    //    }

        //    //    m_Hull.UpdateBulkheadPoint(SelectedBulkhead, m_DraggingHandle, x, y, z);
        //    //    m_Dragging = false;

        //    //    // Note: RotateTo reloads m_drawnBulkheads from the m_Hull
        //    //    RotateTo(m_rotate_x, m_rotate_y, m_rotate_z);
        //    //    Scale();
        //    //    Draw();
        //    //}
        //}

        //protected override void OnPreviewMouseMove(MouseEventArgs e)
        //{
        //    if (m_Dragging)
        //    {
        //        Point loc = e.GetPosition(this);
        //        m_handle[m_DraggingHandle].X = loc.X - RECT_SIZE / 2;
        //        m_handle[m_DraggingHandle].Y = loc.Y - RECT_SIZE / 2;

        //        Draw();
        //    }

        //}

        protected override Size MeasureOverride(Size availableSize)
        {
            Debug.WriteLine("HullDisplay.MeasureOverride {0} {1}", availableSize.Width, availableSize.Height);

            if (m_Hull.IsValid)
            {
                Scale(availableSize);
                return availableSize;
            }
            else
            {
                return availableSize;
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Debug.WriteLine("HullDisplay.ArrangeOverride {0} {1}", finalSize.Width, finalSize.Height);

            if (m_Hull != null && m_Hull.IsValid)
            {
                Scale(finalSize);
                return size2d();
            }
            else
            {
                return finalSize;
            }
        }
    }
}
