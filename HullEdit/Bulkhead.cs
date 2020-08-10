using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    class Bulkhead : INotifyPropertyChanged
    {
        public enum BulkheadType { BOW, VERTICAL, TRANSOM };
        public int numChines { get; private set; }

        private Point3DCollection m_points;

        private bool m_IsValid;
        public bool IsValid { get { return m_IsValid; } }

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public string LoadFromHullFile(string filename)
        {
            //m_IsValid = false;

            //string[] lines = System.IO.File.ReadAllLines(filename);
            //if (lines.Length < 1) return "Invalid file format";
            //int num_chines = numChines;

            //if (!int.TryParse(lines[0], out num_chines)) return "Invalid file format 1";
            //numChines = num_chines;
            //numBulkheads = 5;
            //m_bulkheads = new Point3DCollection[numBulkheads];
            //m_bulkheadType = new BulkheadType[numBulkheads];

            //for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            //{
            //    m_bulkheads[bulkhead] = new Point3DCollection(numChines);
            //}

            //m_bulkheadType[0] = BulkheadType.BOW;
            //for (int bulkhead = 1; bulkhead < numBulkheads; bulkhead++)
            //{
            //    m_bulkheadType[bulkhead] = BulkheadType.VERTICAL;
            //}
            //m_bulkheadType[numBulkheads - 1] = BulkheadType.TRANSOM;

            //if (lines.Length < numBulkheads * numChines * 3 + 1) return "Invalid file format 2";

            //int index = 1;
            //for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            //{
            //    for (int chine = 0; chine < numChines; chine++)
            //    {
            //        Point3D point = new Point3D();
            //        double value;
            //        if (!double.TryParse(lines[index], out value))
            //            return "Invalid file format on line " + index;
            //        point.X = value;
            //        index++;

            //        if (!double.TryParse(lines[index], out value))
            //            return "Invalid file format on line " + index;
            //        point.Y = value;
            //        index++;

            //        if (!double.TryParse(lines[index], out value))
            //            return "Invalid file format on line " + index;
            //        point.Z = value;
            //        index++;

            //        m_bulkheads[bulkhead].Add(point);
            //    }
            //}

            //m_IsValid = true;
            //HullData++;
            //Notify("HullData");

            return "";
        }

    }
}
