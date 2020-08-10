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
        Panels m_panels;
        ObservableCollection<PanelDisplay> m_displayPanels = new ObservableCollection<PanelDisplay>();

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
            panel.X = x;
            panel.Y = y;
            panel.Click += PanelClick;
            
            m_displayPanels.Add(panel);
            canvas.Children.Add(panel);
            Canvas.SetLeft(panel, x);
            Canvas.SetTop(panel, y);
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

        private void PanelClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine(sender);

            //f (m_panels != null) m_panels.Draw(canvas);
        }
    }
}
