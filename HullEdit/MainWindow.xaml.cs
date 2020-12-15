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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

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

            UpdateManips();

            myHull.PropertyChanged += hull_PropertyChanged;
        }

        private void UpdateManips()
        {
            FrontManip.SetHull(myHull);
            FrontManip.SetHullDisplay(FrontDisplay);

            TopManip.SetHull(myHull);
            TopManip.SetHullDisplay(TopDisplay);

            SideManip.SetHull(myHull);
            SideManip.SetHullDisplay(SideDisplay);

            PerspectiveManip.SetHull(myHull);
            PerspectiveManip.SetHullDisplay(PerspectiveDisplay);

            UpdateDisplays();

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
                myHull.LoadFromHullFile(openFileDialog.FileName);

                PerspectiveManip.perspective = HullManip.PerspectiveType.PERSPECTIVE;
                PerspectiveManip.IsEditable = false;
                UpdateManips();

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
                Hull.SerializableHull tempHull;

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(Hull.SerializableHull));

                using (Stream reader = new FileStream(openDlg.FileName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    tempHull = (Hull.SerializableHull)serializer.Deserialize(reader);
                    myHull = new Hull(tempHull);
                    myHull.PropertyChanged += hull_PropertyChanged;

                    PerspectiveManip.perspective = HullManip.PerspectiveType.PERSPECTIVE;
                    PerspectiveManip.IsEditable = false;
                    UpdateManips();

                    PanelsMenu.IsEnabled = true;
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
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(Hull.SerializableHull));

                using (FileStream output = new FileStream(saveDlg.FileName, FileMode.Create))
                {
                    writer.Serialize(output, new Hull.SerializableHull(myHull));
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
            PanelsLayoutWindow layout = new PanelsLayoutWindow(myHull);
            layout.Width = 600;
            layout.Height = 400;

            layout.Show();
        }

        private void ResizeClick(object sender, RoutedEventArgs e)
        {
            Size3D originalSize = myHull.GetSize();
            originalSize.X *= 2;    // compensate because this is a half-hull

            ResizeWindow resize = new ResizeWindow(myHull);

            resize.ShowDialog();

            if (resize.OK)
            {
                ResizeWindowData resizeData = (ResizeWindowData)resize.FindResource("ResizeData");
                double scale_x = 1.0;
                double scale_y = 1.0;
                double scale_z = 1.0;

                if (resizeData != null)
                {
                    scale_x = resizeData.Width / originalSize.X;
                    scale_y = resizeData.Height / originalSize.Y;
                    scale_z = resizeData.Length / originalSize.Z;

                    myHull.Scale(scale_x, scale_y, scale_z);
                    UpdateDisplays();
                }
            }
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            PointCollection points = new PointCollection();
            points.Add(new Point(0, 0));
            points.Add(new Point(1, 0));
            points.Add(new Point(1, 1));
            points.Add(new Point(0, 1));
            points.Add(new Point(0, 0));
            double leftAngle =0, rightAngle=0;

            PointCollection newShape = Geometry.ParallelShape(points, 0.25, false);
            Console.WriteLine("Tool Path: {0}", newShape);
        }
    }
}
