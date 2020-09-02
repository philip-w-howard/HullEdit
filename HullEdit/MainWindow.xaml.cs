using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
        public MainWindow()
        {
            InitializeComponent();
            myHull = new Hull();

            FrontDisplay.SetHull(myHull);
            FrontManip.SetHull(myHull);
            FrontManip.SetHullDisplay(FrontDisplay);

            TopDisplay.SetHull(myHull);
            TopManip.SetHull(myHull);
            TopManip.SetHullDisplay(TopDisplay);

            SideDisplay.SetHull(myHull);
            SideManip.SetHull(myHull);
            SideManip.SetHullDisplay(SideDisplay);

            PerspectiveDisplay.SetHull(myHull);
            PerspectiveManip.SetHull(myHull);
            PerspectiveManip.SetHullDisplay(PerspectiveDisplay);

            myHull.PropertyChanged += hull_PropertyChanged;
        }

        private void UpdateDisplays()
        {
            Hull displayHull = myHull.CopyToFullHull();
            displayHull.Rotate(0, 0, 180);
            FrontDisplay.SetHull(displayHull);
            Debug.WriteLine("Front size: ({0})", displayHull.GetSize());

            displayHull = myHull.CopyToFullHull();
            displayHull.Rotate(0, 90, 90);
            TopDisplay.SetHull(displayHull);

            displayHull = myHull.CopyToFullHull();
            displayHull.Rotate(0, 90, 180);
            SideDisplay.SetHull(displayHull);

            if (PerspectiveManip.perspective == HullManip.PerspectiveType.FRONT)
            {
                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 0, 180);
                Debug.WriteLine("Perspective Front size: ({0})", displayHull.GetSize());
                PerspectiveDisplay.SetHull(displayHull);
                Debug.WriteLine("Perspective Front size: ({0})", displayHull.GetSize());
                PerspectiveManip.perspective = HullManip.PerspectiveType.FRONT;
                PerspectiveManip.IsEditable = true;
            }
            else if (PerspectiveManip.perspective == HullManip.PerspectiveType.TOP)
            {
                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 90, 90);
                PerspectiveDisplay.SetHull(displayHull);
                PerspectiveManip.perspective = HullManip.PerspectiveType.TOP;
                PerspectiveManip.IsEditable = true;
            }
            else if (PerspectiveManip.perspective == HullManip.PerspectiveType.SIDE)
            {
                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(0, 90, 180);
                PerspectiveDisplay.SetHull(displayHull);
                PerspectiveManip.perspective = HullManip.PerspectiveType.SIDE;
                PerspectiveManip.IsEditable = true;
            }
            else // must be PERSPECTIVE
            {
                displayHull = myHull.CopyToFullHull();
                displayHull.Rotate(10, 30, 190);
                PerspectiveDisplay.SetHull(displayHull);
            }
            FrontManip.Draw();
            TopManip.Draw();
            SideManip.Draw();
            PerspectiveManip.Draw();
        }

        private void importClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Hull files (*.hul)|*.hul|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                if (myHull == null) myHull = new Hull();
                myHull.LoadFromHullFile(openFileDialog.FileName);

                PerspectiveManip.perspective = HullManip.PerspectiveType.PERSPECTIVE;
                PerspectiveManip.IsEditable = false;
                UpdateDisplays();

                PanelsMenu.IsEnabled = true;
            }

        }

        private void openClick(object sender, RoutedEventArgs e)
        {
            // destroy any previous hull
            myHull = null;

            OpenFileDialog openDlg = new OpenFileDialog();

            openDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            openDlg.FilterIndex = 0;
            openDlg.RestoreDirectory = true;

            Nullable<bool> result = openDlg.ShowDialog();
            if (result == true)
            {
                Hull tempHull;

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

                using (Stream reader = new FileStream(openDlg.FileName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    tempHull = (Hull)serializer.Deserialize(reader);
                    myHull = tempHull;
                    PerspectiveManip.perspective = HullManip.PerspectiveType.PERSPECTIVE;
                    PerspectiveManip.IsEditable = false;
                    UpdateDisplays();
                }
            }
        }

        private void saveClick(object sender, RoutedEventArgs e)
        {
            if (myHull == null || !myHull.IsValid) return;

            SaveFileDialog saveDlg = new SaveFileDialog();

            saveDlg.Filter = "AVS Hull files (*.avsh)|*.avsh|All files (*.*)|*.*";
            saveDlg.FilterIndex = 0;
            saveDlg.RestoreDirectory = true;

            Nullable<bool> result = saveDlg.ShowDialog();
            if (result == true)
            {
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Hull));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    writer.Serialize(output, myHull);
                }
            }
        }

        private void UpdateDrawings()
        {
            if (myHull != null && myHull.IsValid)
            {
                //FrontDisplay.RotateTo(0, 0, 180);
                //FrontDisplay.Scale();
                FrontManip.Draw();

                //SideDisplay.RotateTo(0, 90, 180);
                //SideDisplay.Scale();
                SideManip.Draw();

                //TopDisplay.RotateTo(0, 90, 90);
                //TopDisplay.Scale();
                TopManip.Draw();

                //PerspectiveDisplay.RotateTo(m_xAngle, m_yAngle, m_zAngle);
                //PerspectiveDisplay.Scale();
                PerspectiveManip.Draw();
            }
        }

        private void HullMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PerspectiveManip.IsEditable = false;

            if (sender == FrontManip)
            {
                PerspectiveManip.perspective = HullManip.PerspectiveType.FRONT;
            }
            else if (sender == TopManip)
            {
                PerspectiveManip.perspective = HullManip.PerspectiveType.TOP;
            }
            else if (sender == SideManip)
            {
                PerspectiveManip.perspective = HullManip.PerspectiveType.SIDE;
            }

            UpdateDisplays();
            PerspectiveManip.IsEditable = true;
            PerspectiveManip.Draw();
        }

        private void RotateClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if ((string)button.Content == "+X")
                PerspectiveManip.Rotate(5, 0, 0);
            else if ((string)button.Content == "-X")
                PerspectiveManip.Rotate(-5, 0, 0);
            else if ((string)button.Content == "+Y")
                PerspectiveManip.Rotate(0, 5, 0);
            else if ((string)button.Content == "-Y")
                PerspectiveManip.Rotate(0, -5, 0);
            else if ((string)button.Content == "+Z")
                PerspectiveManip.Rotate(0, 0, 5);
            else if ((string)button.Content == "-Z")
                PerspectiveManip.Rotate(0, 0, -5);

            PerspectiveManip.Draw();
        }

        private void PanelsClick(object sender, RoutedEventArgs e)
        {
            Panels p = new Panels(myHull);

            PanelsLayoutWindow layout = new PanelsLayoutWindow(p);
            layout.Width = 600;
            layout.Height = 400;

            layout.Show();
        }

        void hull_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BulkheadData")
            {
                Debug.WriteLine("Update chines");
                UpdateDisplays();
            }
            UpdateDrawings();
        }
    }
}
