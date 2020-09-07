using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Media3D;

namespace HullEdit
{
    public class Bulkhead : INotifyPropertyChanged
    {
        public enum BulkheadType { BOW, VERTICAL, TRANSOM };
        public int Count { get { return m_points.Count; } }

        private double m_transom_angle;

        public double TransomAngle { get { return m_transom_angle; } }

        public BulkheadType type { get; set; }

        private Point3DCollection m_points;

        internal bool m_IsValid;
        public bool IsValid { get { return m_IsValid; } }

        public Bulkhead()
        {
            m_IsValid = false;
        }

        public Bulkhead(SerializableBulkhead bulk)
        {
            m_IsValid = bulk.isValid;
            m_transom_angle = bulk.transom_angle;
            m_points = bulk.points.Clone();
            type = bulk.bulkheadType;
        }
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

            //if (type != BulkheadType.BOW)
            {
                Point3DCollection newPoints = new Point3DCollection();

                // FIXTHIS: Eliminate duplicate at bottom/top?
                foreach (Point3D p in m_points)
                {
                    // mirror the X
                    Point3D point = new Point3D(-p.X, p.Y, p.Z);
                    newPoints.Add(point);
                }

                // closed at top or bottom, remove duplicates.
                //if (newPoints[newPoints.Count - 1] == m_points[m_points.Count - 1]) newPoints.RemoveAt(newPoints.Count - 1);
                //if (newPoints[0] == m_points[0]) newPoints.RemoveAt(0);

                foreach (Point3D p in newPoints)
                {
                    bulk.m_points.Insert(0, p);
                }

                bulk.m_IsValid = true;
            }

            return bulk;
        }
        public Point3D GetPoint(int index)
        {
            return m_points[index];
        }

        private double NewZPoint(Point3D basePoint, Point3D newPoint)
        {
            return basePoint.Z + (newPoint.Y - basePoint.Y) * Math.Cos(m_transom_angle) / Math.Sin(m_transom_angle);
        }
        public void UpdatePoint(int chine, double x, double y, double z)
        {
            Point3D basePoint = m_points[0];
            Point3D point = m_points[chine];

            switch (type)
            {
                case BulkheadType.BOW:
                    point.X = 0;                    // force all points to be on the X axix
                    point.Y += y;
                    point.Z += z;
                    break;
                case BulkheadType.VERTICAL:
                    point.X += x;
                    point.Y += y;
                    point.Z = basePoint.Z;          // force all points to be vertical relative to base point
                    break;
                case BulkheadType.TRANSOM:
                    if (x == 0 && y==0 && z==0)
                    {
                        // Simply force Z to be on the plane of the transom
                        if (chine != 0)
                        {
                            point.Z = NewZPoint(basePoint, point);
                        }
                    }
                    else if (x == 0)
                    {
                        // assume updating from side view
                        // Believe the user's y coordinate and then compute Z to be on the plane.
                        point.Y += y;
                        point.Z = NewZPoint(basePoint, point);
                    }
                    else if (y == 0)
                    {
                        // assume updating from top view
                        // Can't update Z or Y from top view
                        point.X += x;
                    }
                    else if (z == 0)
                    {
                        // assume updating from front view
                        // can update both x and y
                        point.X += x;
                        point.Y += y;
                        point.Z = NewZPoint(basePoint, point);
                    }
                    else
                    {
                        throw new Exception("Perspective updates not implemented");
                    }
                    break;
            }

            m_points[chine] = point;

            Notify("Bulkhead");
        }

        public void UpdateMirroredPoint(int chine, double x, double y, double z)
        {
            int actualChine;
            if (chine >= m_points.Count)
            {
                actualChine = chine - m_points.Count;
                x = -x;
            }
            else
            {
                actualChine = (m_points.Count -1) - chine;
            }

            UpdatePoint(actualChine, x, y, z);
        }

        public void ShiftTo(Vector3D zero)
        {
            for (int ii=0; ii<m_points.Count; ii++)
            {
                m_points[ii] += zero;
            }
        }
        public void LoadFromHullFile(StreamReader file, int numChines, BulkheadType type)
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
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead X value");
                point.X = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead Y value");
                point.Y = value;

                line = file.ReadLine();
                if (!double.TryParse(line, out value)) throw new Exception("Unable to read bulkhead Z value");
                point.Z = value;

                m_points.Add(point);
            }

            ComputeAngle();
            StraightenBulkhead();

            m_IsValid = true;
            Notify("Bulkhead");
        }

        protected void ComputeAngle()
        {
            m_transom_angle = 0;
            if (type == BulkheadType.TRANSOM)
            {
                double delta, max_delta;
                int max_index = 1;

                // find greatest delta_z
                max_delta = 0;
                for (int ii = 1; ii < m_points.Count; ii++)
                {
                    delta = Math.Abs(m_points[ii - 1].Z - m_points[ii].Z);
                    if (delta > max_delta)
                    {
                        delta = max_delta;
                        max_index = ii;
                    }
                }

                double delta_y = m_points[0].Y - m_points[max_index].Y;
                double delta_z = m_points[0].Z - m_points[max_index].Z;

                if (delta_z == 0)
                    type = BulkheadType.VERTICAL;
                else
                    m_transom_angle = Math.Atan2(delta_y, delta_z);
            }
        }

        public void StraightenBulkhead()
        {
            for (int chine=0; chine<m_points.Count; chine++)
            {
                // FIXTHIS: uncomment after UpdatePoint works with TRANSOM points. UpdatePoint(chine, 0, 0, 0);
            }
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

            Notify("Bulkhead");
        }

        public class SerializableBulkhead
        {
            public double transom_angle;
            public BulkheadType bulkheadType;
            public Point3DCollection points;
            public bool isValid;

            public SerializableBulkhead()
            {
            }

            public SerializableBulkhead(Bulkhead bulkhead)
            {
                transom_angle = bulkhead.TransomAngle;
                bulkheadType = bulkhead.type;
                isValid = bulkhead.IsValid;
                points = new Point3DCollection();
                if (isValid) points = bulkhead.m_points.Clone();
            }
        }
    }
}
