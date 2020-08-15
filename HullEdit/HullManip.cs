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

        protected override void OnRender(DrawingContext drawingContext)
        {
            
            if (m_Hull == null || !m_Hull.IsValid) return;

            Rect background = new Rect(new Point(0, 0), new Point(ActualWidth, ActualHeight));
            drawingContext.DrawRectangle(this.Background, null, background);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            UIElement content = (UIElement)this.Content;
            Point loc = e.GetPosition(content);
            int bulkhead;

            if (IsEditable)
            {
                if (m_HullDisplay.ClickedHandle(loc, out m_DraggingHandle))
                {
                    m_Dragging = true;
                    m_dragStartX = loc.X;
                    m_dragStartY = loc.Y;
                    Debug.WriteLine("clicked handle {0}", m_DraggingHandle);
                }
                else if (m_HullDisplay.NearBulkhead(loc, CLICK_WIDTH, out bulkhead))
                {
                    m_SelectedBulkhead = bulkhead;
                    m_HullDisplay.SelectedBulkhead = m_SelectedBulkhead;
                    Draw();
                }
                else
                {
                    m_Dragging = false;
                }
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            UIElement content = (UIElement)this.Content;
            Point loc = e.GetPosition(content);

            Debug.WriteLine("dropped handle {0} {1}", m_DraggingHandle, m_Dragging);
            if (m_Dragging)
            {
                double x, y, z;

                if (perspective == PerspectiveType.FRONT)
                {
                    // Front
                    x = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
                    y = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
                    z = 0;
                }
                else if (perspective == PerspectiveType.SIDE)
                {
                    // Side
                    x = 0;
                    y = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
                    z = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
                }
                else if (perspective == PerspectiveType.TOP)
                {
                    // Top
                    x = (m_dragStartY - loc.Y) / m_HullDisplay.Scale;
                    y = 0;
                    z = -(m_dragStartX - loc.X) / m_HullDisplay.Scale;
                }
                else
                {
                    x = 0;
                    y = 0;
                    z = 0;
                }

                m_Hull.UpdateMirroredBulkheadPoint(m_SelectedBulkhead, m_DraggingHandle, x, y, z);
                
                m_Dragging = false;

                //FIXTHIS: need to recompute chines?
                Draw();
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UIElement content = (UIElement)this.Content;
                Point loc = e.GetPosition(content);

                if (m_Dragging)
                {
                    m_HullDisplay.MoveHandle(m_DraggingHandle, loc);
                    Draw();
                }
            }
        }

    }
}
