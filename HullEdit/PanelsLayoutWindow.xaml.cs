using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace HullEdit
{
    /// <summary>
    /// Interaction logic for PanelsWindow.xaml
    /// </summary>
    public partial class PanelsLayoutWindow : Window
    {
        private Brush DEFAULT_BACKGROUND = Brushes.White;
        private Brush DEFAULT_FOREGROUND = Brushes.Black;
        private Brush SELECTED_FOREGROUND = Brushes.Red;
        private double MIN_ROTATE_DRAG = 3;
        private double ROTATE_STEP = Math.PI / 180;
        private bool m_dragging;
        private bool m_rotating;

        private PanelDisplay m_selectedPanel;
        Panels m_panels;
        ObservableCollection<PanelDisplay> m_displayPanels = new ObservableCollection<PanelDisplay>();

        Point m_dragLoc;

        public PanelsLayoutWindow(Panels panels)
        {
            m_panels = panels;
            InitializeComponent();

            double y = 10;
            foreach (Panel p in panels.panels)
            {
                DisplayPanel(p, 10, y);
                y += 40;
            }

            foreach (Panel p in panels.bulkheads)
            {
                DisplayPanel(p, 10, y);
                y += 40;
            }
        }


        public void DisplayPanel(Panel p, double x, double y)
        {
            PanelDisplay panel = new PanelDisplay(p);
            panel.Background = DEFAULT_BACKGROUND;
            panel.Foreground = DEFAULT_FOREGROUND;

            panel.X = x;
            panel.Y = y;
            panel.PreviewMouseDown += Panel_PreviewMouseDown;
            panel.PreviewMouseMove += Panel_PreviewMouseMove;

            m_displayPanels.Add(panel);
            canvas.Children.Add(panel);
            Canvas.SetLeft(panel, x);
            Canvas.SetTop(panel, y);
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
                }
            }
        }

        private void Panel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(sender);

            // If the conversion fails, we didn't select a panel
            PanelDisplay selectedPanel = sender as PanelDisplay;

            if (selectedPanel != null)
            {
                // Unselect all panels
                foreach (PanelDisplay panel in m_displayPanels)
                {
                    panel.Foreground = DEFAULT_FOREGROUND;
                }

                m_selectedPanel = selectedPanel;
                m_selectedPanel.Foreground = SELECTED_FOREGROUND;
                m_dragLoc= e.GetPosition(canvas);
            }

            Debug.WriteLine("MouseDown 1 {0} {1}", m_dragging, m_rotating);
            if (m_selectedPanel != null && selectedPanel == m_selectedPanel)
                m_dragging = true;
            else if (m_selectedPanel != null)
                m_rotating = true;

            Debug.WriteLine("MouseDown 2 {0} {1}", m_dragging, m_rotating);
            //e.Handled = true;
        }

        private void openClick(object sender, RoutedEventArgs e)
        {

        }

        private void saveClick(object sender, RoutedEventArgs e)
        {

        }

        private void ZoomClick(object sender, RoutedEventArgs e)
        {

        }

        private void LayoutClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
