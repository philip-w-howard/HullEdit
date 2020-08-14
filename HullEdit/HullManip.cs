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

        private int m_SelectedBulkhead;
        private int m_DraggingHandle;
        private bool m_Dragging;
        private Hull m_Hull;
        private HullDisplay m_HullDisplay;

        private double m_dragStartX, m_dragStartY;

        private bool m_isEditable;
        public bool IsEditable
        {
            get { return m_isEditable; }
            set
            {
                m_isEditable = value;
                if (m_HullDisplay != null)
                {
                    if (m_isEditable)
                        m_HullDisplay.SelectedBulkhead = m_SelectedBulkhead;
                    else
                        m_HullDisplay.SelectedBulkhead = HullDisplay.NOT_SELECTED;
                }
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
        public void Draw()
        {
            InvalidateVisual();
            if (m_HullDisplay != null) m_HullDisplay.Draw();
        }

        public void Rotate(double x, double y, double z)
        {
            m_HullDisplay.Rotate(x, y, z);
            perspective = PerspectiveType.PERSPECTIVE;
            IsEditable = false;
            InvalidateVisual();
        }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            UIElement content = (UIElement)this.Content;
            Point loc = e.GetPosition(content);

            if (IsEditable)
            {
                if (m_HullDisplay.ClickedHandle(loc, out m_DraggingHandle))
                {
                    Debug.WriteLine("Clicked handle");
                    m_Dragging = true;
                    m_dragStartX = loc.X;
                    m_dragStartY = loc.Y;


                }
                else if (m_HullDisplay.NearBulkhead(loc, CLICK_WIDTH, out m_SelectedBulkhead))
                {
                    Debug.WriteLine("Clicked bulkhead");
                    m_HullDisplay.SelectedBulkhead = m_SelectedBulkhead;
                    Draw();
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
                // FIXTHIS: update location in HullDisplay
                //Point loc = e.GetPosition(this);
                //m_handle[m_DraggingHandle].X = loc.X - RECT_SIZE / 2;
                //m_handle[m_DraggingHandle].Y = loc.Y - RECT_SIZE / 2;

                //Draw();
            }

        }

    }
}
