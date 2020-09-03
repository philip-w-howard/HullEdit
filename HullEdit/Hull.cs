using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;


namespace HullEdit
{
    [DataContract()]
    public class Hull : INotifyPropertyChanged
    {
        private const int POINTS_PER_CHINE = 50;

        public int numBulkheads()
        {
            if (!IsValid) return 0;
            if (m_bulkheads == null) return 0;
            return m_bulkheads.Count;
        }

        public int numChines()
        {
            if (numBulkheads() == 0) return 0;

            return m_bulkheads[0].Count;
        }

        [DataMember(Name = "bulkheads")]
        public List<Bulkhead> m_bulkheads;
        private List<Point3DCollection> m_chines;

        public Bulkhead GetBulkhead(int index) { return m_bulkheads[index]; }
        public Point3DCollection GetChine(int index) { return m_chines[index]; }

        [DataMember(Name = "isValid")]
        internal bool m_IsValid;
        public bool IsValid
        {
            get
            {
                if (!m_IsValid) return false;
                foreach (Bulkhead bulk in m_bulkheads)
                {
                    if (!bulk.IsValid) return false;
                }
                return true;
            }
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

        public Hull() { m_IsValid = false; }

        public void LoadFromHullFile(string filename)
        {
            m_IsValid = false;
            m_chines = null;
            m_bulkheads = new List<Bulkhead>();

            using (StreamReader file = File.OpenText(filename))
            {
                string line;
                int num_chines;
                int numBulkheads = 5;

                line = file.ReadLine();
                if (!int.TryParse(line, out num_chines)) throw new Exception("Invalid HUL file format");

                Bulkhead bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.BOW);
                m_bulkheads.Add(bulkhead);

                for (int ii = 1; ii < numBulkheads-1; ii++)
                {
                    bulkhead = new Bulkhead();
                    bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.VERTICAL);
                    m_bulkheads.Add(bulkhead);
                }

                bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, num_chines, Bulkhead.BulkheadType.TRANSOM);
                m_bulkheads.Add(bulkhead);
            }
            PrepareChines(POINTS_PER_CHINE);
            RepositionToZero();

            m_IsValid = true;
            HullData++;
            Notify("HullData");
        }

        // Returns a list of "full" bulkheads (instead of the half bulkheads that are normally stored)
        public Hull CopyToFullHull()
        {
            Hull fullHull = new Hull();

            if (IsValid)
            {
                fullHull.m_chines = null;
                fullHull.m_bulkheads = new List<Bulkhead>();

                foreach (Bulkhead bulk in m_bulkheads)
                {
                    fullHull.m_bulkheads.Add(bulk.CopyWithReflection());
                }

                fullHull.m_IsValid = true;

                fullHull.PrepareChines(POINTS_PER_CHINE);
                fullHull.RepositionToZero();
            }
            return fullHull;
        }
        public Hull Copy()
        {
            Hull copy = new Hull();

            if (IsValid)
            {
                copy.m_chines = null;

                // need to manually make a deep copy
                copy.m_bulkheads = new List<Bulkhead>();
                foreach (Bulkhead bulk in m_bulkheads)
                {
                    copy.m_bulkheads.Add(bulk.Copy());
                }

                // need to manually make a deep copy
                copy.m_chines = new List<Point3DCollection>(m_chines);
                foreach (Point3DCollection chine in m_chines)
                {
                    copy.m_chines.Add(chine.Clone());
                }

                copy.m_IsValid = true;
                copy.RepositionToZero();
            }
            return copy;
        }

         public void UpdateBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            m_bulkheads[bulkhead].UpdatePoint(chine, x, y, z);
            HullData++;
            Notify("BulkheadData");
        }

        public void UpdateMirroredBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            m_bulkheads[bulkhead].UpdateMirroredPoint(chine, x, y, z);
            HullData++;
            Notify("BulkheadData");
        }

        private void UpdateWithMatrix(double [,] matrix)
        {
            for (int ii = 0; ii < numBulkheads(); ii++)
            {
                m_bulkheads[ii].UpdateWithMatrix(matrix);
            }

            if (m_chines != null)
            {
                for (int ii = 0; ii < numChines(); ii++)
                {
                    Point3DCollection newChine;
                    Matrix.Multiply(m_chines[ii], matrix, out newChine);
                    m_chines[ii] = newChine;
                }
            }
        }
        public void Rotate(double x, double y, double z)
        {
            double[,] rotate;

            rotate = Geometry.CreateRotateMatrix(x, y, z);
            UpdateWithMatrix(rotate);

            RepositionToZero();
        }

        public Size3D GetSize()
        {
            if (!IsValid) return new Size3D(0, 0, 0);

            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;
            double max_x = double.MinValue;
            double max_y = double.MinValue;
            double max_z = double.MinValue;

            foreach (Bulkhead bulk in m_bulkheads)
            {
                for (int ii = 0; ii < bulk.Count; ii++)
                {
                    Point3D point = bulk.GetPoint(ii);
                    max_x = Math.Max(max_x, point.X);
                    max_y = Math.Max(max_y, point.Y);
                    max_z = Math.Max(max_z, point.Z);

                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                    min_z = Math.Min(min_z, point.Z);
                }

            }

            if (m_chines != null)
            {
                foreach (Point3DCollection chine in m_chines)
                {
                    foreach (Point3D point in chine)
                    {
                        max_x = Math.Max(max_x, point.X);
                        max_y = Math.Max(max_y, point.Y);
                        max_z = Math.Max(max_z, point.Z);

                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);
                    }
                }
            }

            return new Size3D(max_x - min_x, max_y - min_y, max_z - min_z);
        }

        protected Point3D GetMin()
        {
            double min_x = double.MaxValue;
            double min_y = double.MaxValue;
            double min_z = double.MaxValue;

            foreach (Bulkhead bulk in m_bulkheads)
            {
                for (int ii = 0; ii < bulk.Count; ii++)
                {
                    Point3D point = bulk.GetPoint(ii);
                    min_x = Math.Min(min_x, point.X);
                    min_y = Math.Min(min_y, point.Y);
                    min_z = Math.Min(min_z, point.Z);
                }

            }

            if (m_chines != null)
            {
                foreach (Point3DCollection chine in m_chines)
                {
                    foreach (Point3D point in chine)
                    {
                        min_x = Math.Min(min_x, point.X);
                        min_y = Math.Min(min_y, point.Y);
                        min_z = Math.Min(min_z, point.Z);
                    }
                }
            }

            return new Point3D(min_x, min_y, min_z);
        }

        private void RepositionToZero()
        {
            if (!IsValid) return;

            Point3D zero = GetMin();

            Vector3D zeroVect = new Vector3D(-zero.X, -zero.Y, -zero.Z);

            foreach (Bulkhead bulk in m_bulkheads)
            {
                bulk.ShiftTo(zeroVect);
            }

            if (m_chines != null)
            {
                for (int ii=0; ii<m_chines.Count; ii++)
                {
                    Point3DCollection newChine = new Point3DCollection(m_chines.Count);
                    foreach (Point3D point in m_chines[ii])
                    {
                        newChine.Add(point + zeroVect);
                    }

                    m_chines[ii] = newChine;
                }
            }
        }

        public void PrepareChines(int points_per_chine)
        {
            int nChines = m_bulkheads[0].Count;

            m_chines = new List<Point3DCollection>();

            for (int chine = 0; chine < nChines; chine++)
            {
                Point3DCollection newChine = new Point3DCollection(points_per_chine);
                Point3DCollection chine_data = new Point3DCollection(m_bulkheads.Count);

                for (int bulkhead = 0; bulkhead < m_bulkheads.Count; bulkhead++)
                {
                    chine_data.Add(m_bulkheads[bulkhead].GetPoint(chine));
                }
                Splines spline = new Splines(chine_data, Splines.RELAXED);
                spline.GetPoints(points_per_chine, newChine);
                m_chines.Add(newChine);
            }

            RepositionToZero();
        }

        public void Scale(double x, double y, double z)
        {
            double[,] scale = new double[3, 3];

            scale[0, 0] = x;
            scale[1, 1] = y;
            scale[2, 2] = z;

            UpdateWithMatrix(scale);

            RepositionToZero();
        }
        public class SerializableHull
        {
            public List<Bulkhead.SerializableBulkhead> bulkheads;
            public bool isValid;

            public SerializableHull()
            { }

            public SerializableHull(Hull hull)
            {
                isValid = hull.m_IsValid;
                bulkheads = new List<Bulkhead.SerializableBulkhead>();

                foreach (Bulkhead bulkhead in hull.m_bulkheads)
                {
                    bulkheads.Add(new Bulkhead.SerializableBulkhead(bulkhead));
                }
            }
        }
    }
}
