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
    /// Interaction logic for PanelLayoutWindow.xaml
    /// </summary>
    public partial class PanelSetupWindow : Window
    {
        public double PanelWidth { get; private set; }
        public double PanelHeight { get; private set; }

        public int NumPanelsHorizontal { get; private set; }
        public int NumPanelsVertical { get; private set; }

        public PanelSetupWindow()
        {
            PanelWidth = 96;
            PanelHeight = 48;
            NumPanelsHorizontal = 1;
            NumPanelsVertical = 1;

            InitializeComponent();

            PanelWidth_Input.Text = "" + PanelWidth;
            PanelHeight_Input.Text = "" + PanelHeight;
            NumWidth_Input.Text = "" + NumPanelsHorizontal;
            NumHeight_Input.Text = "" + NumPanelsVertical;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            double size;
            int intSize;
            if (Double.TryParse(PanelWidth_Input.Text, out size)) PanelWidth = size;
            if (Double.TryParse(PanelHeight_Input.Text, out size)) PanelHeight = size;
            if (Int32.TryParse(NumWidth_Input.Text, out intSize)) NumPanelsHorizontal = intSize;
            if (Int32.TryParse(NumHeight_Input.Text, out intSize)) NumPanelsVertical = intSize;

            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
