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
    /// Interaction logic for PanelSnipWindow.xaml
    /// </summary>
    public partial class PanelSnipWindow : Window
    {
        public bool OK;
        public double start;
        public double radius;
        public double depth;

        public PanelSnipWindow()
        {
            OK = false;
            InitializeComponent();
        }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            double value;

            OK = true;

            if (Double.TryParse(distanceValue.Text, out value))
            {
                start = value;
            }
            else
            {
                OK = false;
            }
            if (Double.TryParse(radiusValue.Text, out value))
            {
                radius = value;
            }
            else
            {
                OK = false;
            }
            if (Double.TryParse(depthValue.Text, out value))
            {
                depth = value;
            }
            else
            {
                OK = false;
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            OK = false;
        }
    }
}
