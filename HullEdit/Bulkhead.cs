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
    public class Bulkhead : INotifyPropertyChanged
    {
        public enum BulkheadType { BOW, VERTICAL, TRANSOM };
        public int Count { get { return m_points.Count; } }
        private double m_transom_angle;

        public BulkheadType type { get; private set; }

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

        public Bulkhead Copy()
        {
            Bulkhead bulkhead = new Bulkhead();
            bulkhead.type = type;
            bulkhead.m_IsValid = m_IsValid;
            bulkhead.m_points = m_points.Clone();

            return bulkhead;
        }

        public Bulkhead CopyWithReflection()
        {
            Bulkhead bulk = Copy();

            if (type != BulkheadType.BOW)
            {
                // FIXTHIS: reverse before adding?
                // FIXTHIS: Eliminate duplicate at bottom/top?
                foreach (Point3D p in m_points)
                {
                    // mirror the X
                    Point3D point = new Point3D(-p.X, p.Y, p.Z);
                    bulk.m_points.Add(point);
                }
            }

            return bulk;
        }
        public Point3D GetPoint(int index)
        {
            return m_points[index];
        }

        public void UpdatePoint(int chine, double x, double y, double z)
        {
            // FIXTHIS: handle TRANSOM by keeping it on the plane
            if (type == BulkheadType.VERTICAL) z = 0;
            if (type == BulkheadType.BOW) x = 0;

            Point3D point = m_points[chine];

            point.X += x;
            point.Y += y;
            point.Z += z;

            m_points[chine] = point;

            Notify("Bulkhead");
        }

        public void ShiftTo(Vector3D zero)
        {
            for (int ii=0; ii<m_points.Count; ii++)
            {
                m_points[ii] += zero;
            }
        }
        public string LoadFromHullFile(StreamReader file, int numChines, BulkheadType type)
        {
            m_IsValid = false;
            this.type = type;
            m_points = new Point3DCollection();

            string line;
            for (int chine = 0; chine < numChines; chine++)
            {
                Point3D point = new Point3D();
                double value;
                line = file.ReadLine();
                if (!double.TryParse(line, out value)) return "Unable to read X value";
                point.X = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) return "Unable to read Y value";
                point.Y = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) return "Unable to read Z value";
                point.Z = value;

                m_points.Add(point);
            }

            m_IsValid = true;
            Notify("Bulkhead");

            return "";
        }

        public void UpdateWithMatrix(double[,] matrix)
        {
            Point3DCollection result = new Point3DCollection(m_points.Count);   // temp array so we can compute in place

            foreach (Point3D point in m_points)
            {
                Point3D new_points = new Point3D();
                new_points.X = point.X * matrix[0, 0] + point.Y * matrix[1, 0] + point.Z * matrix[2, 0];
                new_points.Y = point.X * matrix[0, 1] + point.Y * matrix[1, 1] + point.Z * matrix[2, 1];
                new_points.Z = point.X * matrix[0, 2] + point.Y * matrix[1, 2] + point.Z * matrix[2, 2];

                result.Add(new_points);
            }

            m_points = result;
        }

    }
}
