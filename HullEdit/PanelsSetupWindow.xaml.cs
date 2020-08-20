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
    public class SetupData
    {
        public double panelWidth { get; set; }
        public double panelHeight { get; set; }
        public int numPanelsHorizontal { get; set; }
        public int numPanelsVertical { get; set; }
        public double overallScale { get; set; }
    };

    /// <summary>
    /// Interaction logic for PanelLayoutWindow.xaml
    /// </summary>
    public partial class PanelSetupWindow : Window
    {
        public double PanelWidth { get; private set; }
        public double PanelHeight { get; private set; }

        public int NumPanelsHorizontal { get; private set; }
        public int NumPanelsVertical { get; private set; }

        public double OverallScale { get; private set; }

        public PanelSetupWindow()
        {
            InitializeComponent();

            UpdateProperties();
        }

        private void UpdateProperties()
        {
            double size;
            int intSize;
            if (Double.TryParse(PanelWidth_Input.Text, out size)) PanelWidth = size;
            if (Double.TryParse(PanelHeight_Input.Text, out size)) PanelHeight = size;
            if (Int32.TryParse(NumWidth_Input.Text, out intSize)) NumPanelsHorizontal = intSize;
            if (Int32.TryParse(NumHeight_Input.Text, out intSize)) NumPanelsVertical = intSize;
            if (Double.TryParse(OverallScale_Input.Text, out size)) OverallScale = size;
        }
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            UpdateProperties();
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
