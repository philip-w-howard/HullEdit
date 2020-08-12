using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HullEdit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Hull myHull;
        private double m_xAngle, m_yAngle, m_zAngle;

        public MainWindow()
        {
            InitializeComponent();
            myHull = new Hull();

            FrontDisplay.SetHull(myHull);
            TopDisplay.SetHull(myHull);
            SideDisplay.SetHull(myHull);
            PerspectiveDisplay.SetHull(myHull);

            myHull.PropertyChanged += hull_PropertyChanged;
        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hull files (*.hul)|*.hul|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                if (myHull == null) myHull = new Hull();
                myHull.LoadFromHullFile(openFileDialog.FileName);

                
                Hull displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 0, 180);
                FrontDisplay.SetHull(displayHull);
                FrontDisplay.Draw();

                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 90, 180);
                SideDisplay.SetHull(displayHull);
                SideDisplay.Draw();

                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 90, 90);
                TopDisplay.SetHull(displayHull);
                TopDisplay.Draw();

                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(10, 30, 190);
                PerspectiveDisplay.SetHull(displayHull);
                PerspectiveDisplay.Draw();

                PanelsMenu.IsEnabled = true;
            }

        }

        private void saveClick(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateDrawings()
        {
            if (myHull != null && myHull.IsValid)
            {
                //FrontDisplay.RotateTo(0, 0, 180);
                //FrontDisplay.Scale();
                FrontDisplay.Draw();

                //SideDisplay.RotateTo(0, 90, 180);
                //SideDisplay.Scale();
                SideDisplay.Draw();

                //TopDisplay.RotateTo(0, 90, 90);
                //TopDisplay.Scale();
                TopDisplay.Draw();

                //PerspectiveDisplay.RotateTo(m_xAngle, m_yAngle, m_zAngle);
                //PerspectiveDisplay.Scale();
                PerspectiveDisplay.Draw();
            }
        }

        private void HullMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PerspectiveDisplay.IsEditable = false;

            Debug.WriteLine("HullMouseDown");

            if (sender == FrontDisplay)
            {
                PerspectiveDisplay.SetHull(FrontDisplay.hull.Copy());
            }
            else if (sender == TopDisplay)
            {
                PerspectiveDisplay.SetHull(TopDisplay.hull.Copy());
            }
            else if (sender == SideDisplay)
            {
                PerspectiveDisplay.SetHull(SideDisplay.hull.Copy());
            }

            PerspectiveDisplay.IsEditable = true;
            PerspectiveDisplay.Draw();
        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if ((string)button.Content == "+X")
                PerspectiveDisplay.hull.Rotate(5, 0, 0);
            else if ((string)button.Content == "-X")
                PerspectiveDisplay.hull.Rotate(-5, 0, 0);
            else if ((string)button.Content == "+Y")
                PerspectiveDisplay.hull.Rotate(0, 5, 0);
            else if ((string)button.Content == "-Y")
                PerspectiveDisplay.hull.Rotate(0, -5, 0);
            else if ((string)button.Content == "+Z")
                PerspectiveDisplay.hull.Rotate(0, 0, 5);
            else if ((string)button.Content == "-Z")
                PerspectiveDisplay.hull.Rotate(0, 0, -5);

            PerspectiveDisplay.IsEditable = false;

            PerspectiveDisplay.Draw();
        }

        private void PanelsClick(object sender, RoutedEventArgs e)
        {
            Panels p = new Panels(myHull);
            PanelsLayoutWindow layout = new PanelsLayoutWindow(p);
            layout.Show();
        }

        void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateDrawings();
        }
    }
}
