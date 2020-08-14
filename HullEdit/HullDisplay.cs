using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace HullEdit
{
    public class HullDisplay : Control, INotifyPropertyChanged
    {
        private const int POINTS_PER_CHINE = 50;
        private const int HANDLE_SIZE = 5;
        private int m_SelectedBulkhead;
        public static int NOT_SELECTED = -1;

        public int SelectedBulkhead
        {
            get { return m_SelectedBulkhead; }
            set { m_SelectedBulkhead = value; m_handles = null; }
        }

        private double m_scale = 1;
        public double Scale {  get { return m_scale; } }

        private Hull m_Hull;
        public Hull hull{ get { return m_Hull; } }
        private List<Rect> m_handles;

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public int numChines { get { return m_Hull.numChines; } }
        public int numBulkheads { get { return m_Hull.numBulkheads; } }

        public HullDisplay()
        {
            m_scale = 1;
            m_SelectedBulkhead = NOT_SELECTED;
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

            DrawHandles(drawingContext);
        }

        private void DrawHandles(DrawingContext drawingContext)
        {
            if (m_SelectedBulkhead >= 0)
            {
                // If necessary, create new handles for current SelectedBulkhead
                if (m_handles == null)
                {
                    m_handles = new List<Rect>();

                    for (int ii = 0; ii < m_Hull.numChines; ii++)
                    {
                        Rect rect = new Rect();
                        rect.Height = HANDLE_SIZE;
                        rect.Width = HANDLE_SIZE;
                        rect.X = m_Hull.GetBulkhead(m_SelectedBulkhead).GetPoint(ii).X - HANDLE_SIZE / 2;
                        rect.Y = m_Hull.GetBulkhead(m_SelectedBulkhead).GetPoint(ii).Y - HANDLE_SIZE / 2;
                        m_handles.Add(rect);
                    }
                }

                // Draw handles
                foreach (Rect rect in m_handles)
                {
                    drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), new Pen(new SolidColorBrush(Colors.Red), 1), rect);
                }
            }
        }

        public void MoveHandle(int index, Point loc)
        {
            Rect rect = m_handles[index];
            rect.Location = loc;


            m_handles[index] = rect;
            Draw();
        }
        public void Rotate(double x, double y, double z)
        {
            m_Hull.Rotate(x, y, z);

            if (m_handles != null)
            {
                double[,] rotate = Geometry.CreateRotateMatrix(x, y, z);
                Matrix.Multiply(m_handles, rotate, out m_handles);
            }
            InvalidateVisual();
        }

        protected Size size2d()
        {
            Size3D hullSize = m_Hull.GetSize();

            return new Size(hullSize.X, hullSize.Y);
        }
        protected void Rescale(Size size)
        {
            Size3D hullSize = m_Hull.GetSize();

            // Scale all the points to fit in the canvas
            double scale1 = size.Width / hullSize.X;
            double scale2 = size.Height / hullSize.Y;

            double new_scale = 0.9 * Math.Min(scale1, scale2);

            m_scale *= new_scale;

            m_Hull.Scale(new_scale, new_scale, new_scale);

            if (m_handles != null)
            {
                List<Rect> newHandles = new List<Rect>();
                foreach (Rect rect in m_handles)
                {
                    Point p = new Point();
                    p.X = rect.Location.X * new_scale;
                    p.Y = rect.Location.Y * new_scale;

                    newHandles.Add(new Rect(p, rect.Size));
                }

                m_handles = newHandles;
            }
        }


        public void Draw()
        {
            InvalidateVisual();
        }

        public bool ClickedHandle(Point loc, out int handleIndex)
        {
            handleIndex = 0;

            if (m_handles == null || m_handles[0] == null) return false;

            foreach (Rect rect in m_handles)
            {
                if (rect.X <= loc.X && rect.X + rect.Width >= loc.X &&
                    rect.Y <= loc.Y && rect.Y + rect.Height >= loc.Y)
                {
                    //<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    return true;
                }
                handleIndex++;
            }

            return false;
        }

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

        protected override Size MeasureOverride(Size availableSize)
        {
            if (m_Hull.IsValid)
            {
                Rescale(availableSize);
                return availableSize;
            }
            else
            {
                return availableSize;
            }
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (m_Hull != null && m_Hull.IsValid)
            {
                Rescale(finalSize);
                return size2d();
            }
            else
            {
                return finalSize;
            }
        }
    }
}
