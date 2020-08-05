using System;
using System.Collections.Generic;
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

        public PanelsLayoutWindow()
        {
            InitializeComponent();
        }

        public void SetPanels(Panels p)
        {
            m_panels = p;
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

        private void DrawClick(object sender, RoutedEventArgs e)
        {
            if (m_panels != null) m_panels.Draw(canvas);
        }
    }
}
