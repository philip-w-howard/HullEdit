﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class ResizeWindowData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _checked;
        private double _width;
        private double _height;
        private double _length;

        public ResizeWindowData() { }

        public bool Proportional
        {
            get { return _checked; }
            set
            {
                _checked = value;
                Notify("Proportional");
            }
        }
        public double Width
        {
            get { return _width; }
            set
            {
                if (Proportional)
                {
                    double ratio = value / _width;
                    _length *= ratio;
                    Notify("Length");
                    _height *= ratio;
                    Notify("Height");
                }
                _width = value;
                Notify("Width");
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                if (Proportional)
                {
                    double ratio = value / _height;
                    _length *= ratio;
                    Notify("Length");
                    _width *= ratio;
                    Notify("Width");
                }
                _height = value;
                Notify("Height");
            }
        }
        public double Length
        {
            get { return _length; }
            set {
                if (Proportional)
                {
                    double ratio = value / _length;
                    _height *= ratio;
                    Notify("Height");
                    _width *= ratio;
                    Notify("Width");
                }
                _length = value;
                Notify("Length");
            }
        }

        private void Notify(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }
    /// <summary>
    /// Interaction logic for ResizeWindow.xaml
    /// </summary>
    public partial class ResizeWindow : Window
    {
        public ResizeWindow(Hull hull)
        {
            InitializeComponent();
            ResizeWindowData resizeData = (ResizeWindowData)this.FindResource("ResizeData");

            if (resizeData != null)
            {
                bool proportional = resizeData.Proportional;

                // Need to turn off Proportional for initial setup
                resizeData.Proportional = false;

                Size3D size = hull.GetSize();
                resizeData.Width = size.X*2;    // multiply by 2 because this is half-hull
                resizeData.Height = size.Y;
                resizeData.Length = size.Z;
                resizeData.Proportional = true;

                // Reset proportional
                resizeData.Proportional = proportional;
            }
        }

        public bool OK { get; set; }

        private void OKClick(object sender, RoutedEventArgs e)
        {
            OK = true;
            this.Close();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            OK = false;
            this.Close();
        }
    }
}
