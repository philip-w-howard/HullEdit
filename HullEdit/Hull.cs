using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    public class Hull : INotifyPropertyChanged
    {
        public enum BulkheadType { BOW, VERTICAL, TRANSOM };
        public int numBulkheads { get; private set; }
        public int numChines { get; private set; }

        private Point3DCollection[] m_bulkheads;        // [bulkhead]
        private BulkheadType[] m_bulkheadType;

        public BulkheadType GetBulkheadType(int index)
        {
            return m_bulkheadType[index];
        }

        public int HullData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        void Notify(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private bool m_IsValid;
        public bool IsValid {  get { return m_IsValid; } }

        public Hull() { m_IsValid = false; }

        public string LoadFromHullFile(string filename)
        {
            m_IsValid = false;

            using (StreamReader file = File.OpenText(filename))
            {
                string line;
                int num_chines = numChines;

                line = file.ReadLine();
                if (!int.TryParse(line, out num_chines)) return "Invalid file format 1";
                numChines = num_chines;
                numBulkheads = 5;
                m_bulkheads = new Point3DCollection[numBulkheads];
                m_bulkheadType = new BulkheadType[numBulkheads];

                for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
                {
                    m_bulkheads[bulkhead] = new Point3DCollection(numChines);
                }

                m_bulkheadType[0] = BulkheadType.BOW;
                for (int bulkhead = 1; bulkhead < numBulkheads; bulkhead++)
                {
                    m_bulkheadType[bulkhead] = BulkheadType.VERTICAL;
                }
                m_bulkheadType[numBulkheads - 1] = BulkheadType.TRANSOM;

                int index = 1;
                for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
                {
                    for (int chine = 0; chine < numChines; chine++)
                    {
                        Point3D point = new Point3D();
                        double value;
                        line = file.ReadLine();
                        if (!double.TryParse(line, out value))
                            return "Invalid file format on line " + index;
                        point.X = value;
                        index++;

                        line = file.ReadLine();
                        if (!double.TryParse(line, out value))
                            return "Invalid file format on line " + index;
                        point.Y = value;
                        index++;

                        line = file.ReadLine();
                        if (!double.TryParse(line, out value))
                            return "Invalid file format on line " + index;
                        point.Z = value;
                        index++;

                        m_bulkheads[bulkhead].Add(point);
                    }
                }
            }
            m_IsValid = true;
            HullData++;
            Notify("HullData");

            return "";
        }

        public Point3DCollection[] CopyBulkheads()
        {
            // not sure why "return m_bulkheads.Clone() does not work

            Point3DCollection[] bulkheads = new Point3DCollection[numBulkheads];
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                bulkheads[bulkhead] = m_bulkheads[bulkhead].Clone();
            }
            return bulkheads;

        }

        public Point3DCollection GetBulkhead(int index)
        {
            return m_bulkheads[index].Clone();
        }

        public Point3D GetBulkheadPoint(int bulkhead, int chine)
        {
            return m_bulkheads[bulkhead][chine];
        }

        //public void GetBulkheadPoints(int bulkhead, double[,] points)
        //{
        //    for (int ii=0; ii<numChines; ii++)
        //    {
        //        points[ii, 0] = m_drawnBulkheads[bulkhead][ii, 0];
        //        points[ii, 1] = m_drawnBulkheads[bulkhead][ii, 1];
        //    }
        //}

        public void SetBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            Point3D point = new Point3D();
            point.X = x;
            point.Y = y;
            point.Z = z;

            m_bulkheads[bulkhead][chine] = point;

            HullData++;
            Notify("HullData");
        }
        public void ShiftBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            if (m_bulkheadType[bulkhead] == BulkheadType.VERTICAL) z = 0;
            if (m_bulkheadType[bulkhead] == BulkheadType.BOW) x = 0;

            Point3D point = m_bulkheads[bulkhead][chine];

            point.X += x;
            point.Y += y;
            point.Z += z;

            m_bulkheads[bulkhead][chine] = point;

            HullData++;
            Notify("HullData");
        }
    }
}
