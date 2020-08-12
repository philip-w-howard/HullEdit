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
        private const int POINTS_PER_CHINE = 50;

        public int numBulkheads { get; private set; }
        public int numChines { get; private set; }

        private List<Bulkhead> m_bulkheads;
        private List<Point3DCollection> m_chines;
        public Bulkhead GetBulkhead(int index) { return m_bulkheads[index]; }
        public Point3DCollection GetChine(int index) { return m_chines[index]; }

        private bool m_IsValid;
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
                int num_chines = numChines;

                line = file.ReadLine();
                if (!int.TryParse(line, out num_chines)) throw new Exception("Invalid HUL file format");

                numChines = num_chines;
                numBulkheads = 5;

                Bulkhead bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, numChines, Bulkhead.BulkheadType.BOW);
                m_bulkheads.Add(bulkhead);

                for (int ii = 1; ii < numBulkheads-1; ii++)
                {
                    bulkhead = new Bulkhead();
                    bulkhead.LoadFromHullFile(file, numChines, Bulkhead.BulkheadType.VERTICAL);
                    m_bulkheads.Add(bulkhead);
                }

                bulkhead = new Bulkhead();
                bulkhead.LoadFromHullFile(file, numChines, Bulkhead.BulkheadType.TRANSOM);
                m_bulkheads.Add(bulkhead);
            }
            PrepareChines(POINTS_PER_CHINE);
            RepositionToZero();

            m_IsValid = true;
            HullData++;
            Notify("HullData");
        }

        //public List<Bulkhead> CopyBulkheads()
        //{
        //    // not sure why "return m_bulkheads.Clone() does not work
        //    List<Bulkhead> bulkheads = new List<Bulkhead>(m_bulkheads);
        //    return bulkheads;
        //}

        // Returns a list of "full" bulkheads (instead of the half bulkheads that are normally stored)
        public Hull CopyToFullHull()
        {
            Hull fullHull = new Hull();

            if (IsValid)
            {
                fullHull.numBulkheads = numBulkheads;
                fullHull.numChines = numChines;
                fullHull.m_chines = null;
                fullHull.m_bulkheads = new List<Bulkhead>();

                foreach (Bulkhead bulk in m_bulkheads)
                {
                    fullHull.m_bulkheads.Add(bulk.CopyWithReflection());
                }

                fullHull.PrepareChines(POINTS_PER_CHINE);

                fullHull.RepositionToZero();

                fullHull.m_IsValid = true;
            }
            return fullHull;
        }

        //public Bulkhead GetBulkhead(int index)
        //{
        //    return m_bulkheads[index].Copy();
        //}

        //public Point3D GetBulkheadPoint(int bulkhead, int chine)
        //{
        //    return m_bulkheads[bulkhead].GetPoint(chine);
        //}

        public void UpdateBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            m_bulkheads[bulkhead].UpdatePoint(chine, x, y, z);
            HullData++;
            Notify("HullData");
        }

        protected void RotateDrawing_X(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[0, 0] = 1.0;
            rotate[1, 1] = Math.Cos(angle);
            rotate[2, 2] = Math.Cos(angle);
            rotate[1, 2] = Math.Sin(angle);
            rotate[2, 1] = -Math.Sin(angle);

            //CenterTo(0, 0, 0);

            UpdateWithMatrix(rotate);
        }

        protected void RotateDrawing_Y(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[1, 1] = 1.0;
            rotate[0, 0] = Math.Cos(angle);
            rotate[2, 2] = Math.Cos(angle);
            rotate[2, 0] = Math.Sin(angle);
            rotate[0, 2] = -Math.Sin(angle);

            //CenterTo(0, 0, 0);

            UpdateWithMatrix(rotate);
        }

        protected void RotateDrawing_Z(double angle)
        {
            double[,] rotate = new double[3, 3];

            angle = angle * Math.PI / 180.0;

            rotate[2, 2] = 1.0;
            rotate[0, 0] = Math.Cos(angle);
            rotate[1, 1] = Math.Cos(angle);
            rotate[0, 1] = Math.Sin(angle);
            rotate[1, 0] = -Math.Sin(angle);

            UpdateWithMatrix(rotate);
        }

        private void UpdateWithMatrix(double [,] matrix)
        {
            for (int ii = 0; ii < numBulkheads; ii++)
            {
                m_bulkheads[ii].UpdateWithMatrix(matrix);
            }

            if (m_chines != null)
            {
                for (int ii = 0; ii < numChines; ii++)
                {
                    Point3DCollection newChine;
                    Matrix.Multiply(m_chines[ii], matrix, out newChine);
                    m_chines[ii] = newChine;
                }
            }
        }
        public void Rotate(double x, double y, double z)
        {
            // NOTE: Could optimize by multiplying the three rotation matrices before rotating the points
            RotateDrawing_Z(z);
            RotateDrawing_X(x);
            RotateDrawing_Y(y);

            RepositionToZero();
        }

        public Size3D GetSize()
        {
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
    }
}
