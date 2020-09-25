using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace HullEdit
{
    class ItemList : ObservableCollection<String>
    {
        public ItemList() : base()
        { }
    }
    /// <summary>
    /// Interaction logic for PanelsWindow.xaml
    /// </summary>
    public partial class PanelsLayoutWindow : Window
    {
        private const int POINTS_PER_CHINE = 50;

        private Brush DEFAULT_BACKGROUND = Brushes.White;
        private Brush DEFAULT_FOREGROUND = Brushes.Black;
        private Brush SELECTED_FOREGROUND = Brushes.Red;
        private double SCALE_MOVE = 0.25;
        private double MIN_ROTATE_DRAG = 3;
        private double ROTATE_STEP = Math.PI / 180;
        private bool m_dragging;
        private bool m_rotating;

        private double m_scale;

        // values from setup window
        private double m_panelWidth;
        private double m_panelHeight;
        private int m_NumHorizontalPanels;
        private int m_NumVerticalPanels;
        private double m_overallScale;

        private double scale
        {
            get { return m_scale; }
            set
            {
                m_scale = value;
                ScaleTransform scaler = new ScaleTransform();
                scaler.ScaleX = scale;
                scaler.ScaleY = scale;
                canvas.LayoutTransform = scaler;
            }
        }

        private PanelDisplay m_selectedPanel;
        List<Panel> m_panels;
        ObservableCollection<PanelDisplay> m_displayPanels = new ObservableCollection<PanelDisplay>();

        Point m_dragLoc;

        public PanelsLayoutWindow() { }

        public PanelsLayoutWindow(Hull hull)
        {
            m_scale = 1;
            Panelize(hull);
            InitializeComponent();

            ItemList panelList = (ItemList)this.FindResource("PanelList");
            if (panelList != null)
            {
                panelList.Clear();

                foreach (Panel panel in m_panels)
                {
                    panelList.Add(panel.name);
                }
            }

            UpdatePanelLayout();
        }


        public void DisplayPanel(Panel p, double x, double y)
        {
            PanelDisplay panel = new PanelDisplay(p, m_overallScale);
            panel.Background = DEFAULT_BACKGROUND;
            panel.Foreground = DEFAULT_FOREGROUND;

            panel.X = x;
            panel.Y = y;

            DisplayPanel(panel);
        }

        public void DisplayPanel(PanelDisplay panel)
        {
            panel.Background = DEFAULT_BACKGROUND;
            panel.Foreground = DEFAULT_FOREGROUND;

            panel.PreviewMouseDown += Panel_PreviewMouseDown;
            panel.PreviewMouseMove += Panel_PreviewMouseMove;

            ContextMenu menu = (ContextMenu)this.FindResource("EditMenu");
            if (menu != null)
            {
                panel.ContextMenu = menu;
            }

            m_displayPanels.Add(panel);
            canvas.Children.Add(panel);
            Canvas.SetLeft(panel, panel.X);
            Canvas.SetTop(panel, panel.Y);
        }

        private void UpdatePanelLayout()
        {
            // FIXTHIS: need to use data binding for this
            double oldScale = m_overallScale;

            PanelSetupWindow setup = new PanelSetupWindow();
            setup.ShowDialog();

            m_panelWidth = setup.PanelWidth;
            m_panelHeight = setup.PanelHeight;
            m_NumHorizontalPanels = setup.NumPanelsHorizontal;
            m_NumVerticalPanels = setup.NumPanelsVertical;
            m_overallScale = setup.OverallScale;

            double scaleRatio = m_overallScale / oldScale;

            canvas.Children.Clear();

            Point sheetLoc = new Point(0, 0);
            double panelWidth = m_panelWidth * m_overallScale;
            double panelHeight = m_panelHeight * m_overallScale;

            for (int row = 0; row < m_NumVerticalPanels; row++)
            {
                for (int col = 0; col < m_NumHorizontalPanels; col++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = panelWidth;
                    rect.Height = panelHeight;
                    rect.Stroke = new SolidColorBrush(Colors.Blue);
                    rect.Fill = new SolidColorBrush(Colors.White);
                    Canvas.SetLeft(rect, sheetLoc.X);
                    Canvas.SetTop(rect, sheetLoc.Y);
                    canvas.Children.Add(rect);

                    sheetLoc.X += panelWidth;
                }

                sheetLoc.X = 0;
                sheetLoc.Y += panelHeight;
            }

            foreach (PanelDisplay panel in m_displayPanels)
            {
                panel.scale = m_overallScale;
                panel.X *= scaleRatio;
                panel.Y *= scaleRatio;

                canvas.Children.Add(panel);
                Canvas.SetLeft(panel, panel.X);
                Canvas.SetTop(panel, panel.Y);
            }

            InvalidateVisual();
        }
        private void Panel_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            m_dragging = false;
            m_rotating = false;
        }

        private void Panel_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point loc = e.GetPosition(canvas);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (m_selectedPanel != null && m_dragging)
                {
                    // handle dragging
                    m_selectedPanel.X += loc.X - m_dragLoc.X;
                    m_selectedPanel.Y += loc.Y - m_dragLoc.Y;
                    m_dragLoc = loc;
                    Canvas.SetLeft(m_selectedPanel, m_selectedPanel.X);
                    Canvas.SetTop(m_selectedPanel, m_selectedPanel.Y);
                    ResizeCanvas();

                }
                else if (m_selectedPanel != null && m_rotating)
                {
                    // Handle rotations
                    double distance = loc.X - m_dragLoc.X;
                    Debug.WriteLine("Rotate: {0}", distance);

                    if (Math.Abs(distance) > MIN_ROTATE_DRAG)
                    {
                        m_dragLoc = loc;

                        if (distance > 0)
                            m_selectedPanel.Rotate(ROTATE_STEP);

                        else
                            m_selectedPanel.Rotate(-ROTATE_STEP);

                    }
                    ResizeCanvas();

                }
            }
        }

        private void Panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(sender);

            // If the conversion fails, we didn't select a panel
            PanelDisplay selectedPanel = sender as PanelDisplay;

            //           if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selectedPanel != null)
                {
                    // Unselect all panels
                    foreach (PanelDisplay panel in m_displayPanels)
                    {
                        panel.Foreground = DEFAULT_FOREGROUND;
                    }

                    m_selectedPanel = selectedPanel;
                    m_selectedPanel.Foreground = SELECTED_FOREGROUND;
                    m_dragLoc = e.GetPosition(canvas);
                }

                Debug.WriteLine("MouseDown 1 {0} {1}", m_dragging, m_rotating);
                if (m_selectedPanel != null && selectedPanel == m_selectedPanel)
                    m_dragging = true;
                else if (m_selectedPanel != null)
                    m_rotating = true;

                Debug.WriteLine("MouseDown 2 {0} {1}", m_dragging, m_rotating);
            }
        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.Filter = "AVS Panels files (*.avsp)|*.avsp|All files (*.*)|*.*";
            openDlg.FilterIndex = 0;
            openDlg.RestoreDirectory = true;

            Nullable<bool> result = openDlg.ShowDialog();
            if (result == true)
            {
                SerializablePanelsLayoutWindow tempWindow;

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SerializablePanelsLayoutWindow));

                using (Stream reader = new FileStream(openDlg.FileName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    tempWindow = (SerializablePanelsLayoutWindow)serializer.Deserialize(reader);

                    m_panelWidth = tempWindow.m_panelWidth;
                    m_panelHeight = tempWindow.m_panelHeight;
                    m_NumHorizontalPanels = tempWindow.m_NumHorizontalPanels;
                    m_NumVerticalPanels = tempWindow.m_NumVerticalPanels;
                    m_overallScale = tempWindow.m_overallScale;

                    m_scale = tempWindow.scale;
                    m_panels = new List<Panel>();
                    m_displayPanels = new ObservableCollection<PanelDisplay>();

                    foreach (Panel.SerializablePanel p in tempWindow.panels)
                    {
                        m_panels.Add(new Panel(p));
                    }

                    foreach (PanelDisplay.SerializablePanelDisplay disp in tempWindow.displayPanels)
                    {
                        DisplayPanel(new PanelDisplay(disp));
                    }

                    ItemList panelList = (ItemList)this.FindResource("PanelList");
                    if (panelList != null)
                    {
                        panelList.Clear();

                        foreach (Panel panel in m_panels)
                        {
                            panelList.Add(panel.name);
                        }
                    }

                    //UpdatePanelLayout();
                }
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Panels files (*.avsp)|*.avsp|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(SerializablePanelsLayoutWindow));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    writer.Serialize(output, new SerializablePanelsLayoutWindow(this));
                }
            }

        }

        private void ZoomClick(object sender, RoutedEventArgs e)
        {
            scale += 1;

            ResizeCanvas();
        }

        private void LayoutClick(object sender, RoutedEventArgs e)
        {
            UpdatePanelLayout();
            ResizeCanvas();
        }

        private void HorizontalFlipClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Horizontal flip: {0}", sender);

            if (m_selectedPanel != null)
            {
                m_selectedPanel.HorizontalFlip();
                Debug.WriteLine("Horizontal Flip");
                ResizeCanvas();
                //InvalidateVisual();
            }
        }
        private void VerticalFlipClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                m_selectedPanel.VerticalFlip();
                Debug.WriteLine("Vertical Flip");
                ResizeCanvas();
                //InvalidateVisual();
            }
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                PanelDisplay panel = m_selectedPanel.Copy();
                DisplayPanel(panel);
                m_selectedPanel.Foreground = DEFAULT_FOREGROUND;
                panel.Foreground = SELECTED_FOREGROUND;

                m_selectedPanel = panel;
            }
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                m_displayPanels.Remove(m_selectedPanel);
                canvas.Children.Remove(m_selectedPanel);
                ResizeCanvas();
            }
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Add Click {0} {1}", sender, PanelSelection.SelectedValue);
            string selection = PanelSelection.SelectedValue as String;
            Point loc = Mouse.GetPosition(canvas);

            if (selection != null)
            {
                Panel panel = null;
                foreach (Panel p in m_panels)
                {
                    if (selection == p.name)
                    {
                        panel = p;
                        break;
                    }
                }

                if (panel != null)
                {
                    // Move over by half the size so new panel shows up centered on the mouse
                    Size size = panel.GetSize();
                    loc.X -= size.Width / 2;
                    loc.Y -= size.Height / 2;

                    DisplayPanel(panel, loc.X, loc.Y);
                    ResizeCanvas();
                }
            }
        }

        private void AddAllClick(object sender, RoutedEventArgs e)
        {
            double y = 10;
            foreach (Panel p in m_panels)
            {
                DisplayPanel(p, 10, y);
                y += 15;
            }

            ResizeCanvas();
        }

        private void ResizeCanvas()
        {
            double maxX = viewerGrid.ActualWidth / scale;
            double maxY = viewerGrid.ActualHeight / scale;

            maxX = Math.Max(maxX, m_NumHorizontalPanels * m_panelWidth * m_overallScale);
            maxY = Math.Max(maxY, m_NumVerticalPanels * m_panelHeight * m_overallScale);

            //maxX = viewer.ActualWidth/scale;
            //maxY = viewer.ActualHeight/scale;

            foreach (PanelDisplay panel in m_displayPanels)
            {
                Size size = panel.size;
                maxX = Math.Max(maxX, size.Width + panel.X);
                maxY = Math.Max(maxY, size.Height + panel.Y);
            }

            Debug.WriteLine("Resize: Prev: {0} Viewer: {1} New: {2}", canvas.ActualHeight, viewerGrid.ActualHeight, maxY);

            canvas.Width = maxX;
            canvas.Height = maxY;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResizeCanvas();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeCanvas();
        }

        private void viewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                    scale += scale * SCALE_MOVE;
                else if (e.Delta < 0)
                    scale -= scale * SCALE_MOVE;
                Debug.WriteLine("Zoom {0} {1}", e.Delta, scale);
                ResizeCanvas();
                e.Handled = true;
            }
            //base.MouseWheel(sender, e);
        }

        private void outputClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveDlg.FilterIndex = 2;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                using (System.IO.StreamWriter output = new System.IO.StreamWriter(saveDlg.FileName))
                {
                    foreach (PanelDisplay panel in m_displayPanels)
                    {
                        output.Write(panel.ToString());
                    }
                }
            }
        }
        private void outputSVG(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "SVG files (*.svg)|*.svg|All files (*.*)|*.*";
            saveDlg.FilterIndex = 1;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                SVGWriter output = new SVGWriter(saveDlg.FileName);
                foreach (PanelDisplay panel in m_displayPanels)
                {
                    output.Write(panel);
                }

                output.Close();
            }
        }

        private void Panelize(Hull hull)
        {
            Hull highResHull = hull.Copy();
            highResHull.PrepareChines(POINTS_PER_CHINE);

            int numPanels = highResHull.numChines() - 1;

            m_panels = new List<Panel>();

            for (int ii = 0; ii < numPanels; ii++)
            {
                Panel panel = new Panel(highResHull.GetChine(ii), highResHull.GetChine(ii + 1));
                panel.name = "Chine " + (ii + 1);
                m_panels.Add(panel);
            }

            //*********************************
            // bulkheads:
            int numBulkheads = hull.numBulkheads();

            if (hull.GetBulkhead(numBulkheads - 1).type == Bulkhead.BulkheadType.BOW) numBulkheads--;

            Hull fullHull = hull.CopyToFullHull();

            for (int bulkhead = 0; bulkhead < fullHull.numBulkheads(); bulkhead++)
            {
                int numChines = fullHull.numChines();

                if (fullHull.GetBulkhead(bulkhead).type != Bulkhead.BulkheadType.BOW)
                {
                    Bulkhead bulk = fullHull.GetBulkhead(bulkhead);
                    Point3DCollection points = new Point3DCollection();

                    Point3D basePoint = bulk.GetPoint(0);

                    for (int chine = 0; chine < numChines; chine++)
                    {
                        Point3D point = bulk.GetPoint(chine);
                        if (bulk.type == Bulkhead.BulkheadType.TRANSOM)
                        {
                            point.Y = basePoint.Y + (point.Y - basePoint.Y) / Math.Sin(bulk.TransomAngle);
                        }
                        points.Add(bulk.GetPoint(chine));
                    }

                    // close the shape
                    if (points[0].X != 0) points.Add(points[0]);

                    Panel panel = new Panel(points);
                    panel.name = "Bulkhead " + (bulkhead + 1);
                    m_panels.Add(panel);
                }
            }
        }
        public class SerializablePanelsLayoutWindow
        {
            // values from setup window
            public double m_panelWidth;
            public double m_panelHeight;
            public int m_NumHorizontalPanels;
            public int m_NumVerticalPanels;
            public double m_overallScale;

            public double scale;
            public List<Panel.SerializablePanel> panels;
            public List<PanelDisplay.SerializablePanelDisplay> displayPanels;

            public SerializablePanelsLayoutWindow() { }
            public SerializablePanelsLayoutWindow(PanelsLayoutWindow window)
            {
                m_panelWidth = window.m_panelWidth;
                m_panelHeight = window.m_panelHeight;
                m_NumHorizontalPanels = window.m_NumHorizontalPanels;
                m_NumVerticalPanels = window.m_NumVerticalPanels;
                m_overallScale = window.m_overallScale;

                scale = window.m_scale;
                panels = new List<Panel.SerializablePanel>();
                displayPanels = new List<PanelDisplay.SerializablePanelDisplay>();

                foreach (Panel p in window.m_panels)
                {
                    panels.Add(new Panel.SerializablePanel(p));
                }

                foreach (PanelDisplay disp in window.m_displayPanels)
                {
                    displayPanels.Add(new PanelDisplay.SerializablePanelDisplay(disp));
                }
            }
        }

        private void SplitClick(object sender, RoutedEventArgs e)
        {
            if (m_selectedPanel != null)
            {
                PanelSnipWindow snip = new PanelSnipWindow();
                snip.ShowDialog();

                if (snip.OK)
                {
                    Panel panel_1, panel_2;

                    if (m_selectedPanel.Split(snip.start, snip.numTongues, snip.depth, out panel_1, out panel_2))
                    {
                        DisplayPanel(panel_1, m_selectedPanel.X, m_selectedPanel.Y);
                        DisplayPanel(panel_2, m_selectedPanel.X + snip.start, m_selectedPanel.Y);
                        canvas.Children.Remove(m_selectedPanel);
                        m_displayPanels.Remove(m_selectedPanel);
                        m_selectedPanel = null;
                    }
                }
            }
        }
    }
}
