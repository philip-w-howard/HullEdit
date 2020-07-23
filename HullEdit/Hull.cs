using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HullEdit
{
    class Hull
    {
        public int numChines { get; private set; }
        public int numBulkheads { get; private set; }

        private double[][,] m_bulkheads;        // [bulkhead][chine, axis]

        private bool m_IsValid;

        public bool IsValid {  get { return m_IsValid; } }

        public Hull() { m_IsValid = false; }

        public string LoadFromHullFile(string filename)
        {
            m_IsValid = false;

            string[] lines = System.IO.File.ReadAllLines(filename);
            if (lines.Length < 1) return "Invalid file format";
            int num_chines = numChines;

            if (!int.TryParse(lines[0], out num_chines)) return "Invalid file format 1";
            numChines = num_chines;
            numBulkheads = 5;
            m_bulkheads = new double[numBulkheads][,];
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                m_bulkheads[bulkhead] = new double[numChines, 3];
            }

            if (lines.Length < numBulkheads * numChines * 3 + 1) return "Invalid file format 2";

            int index = 1;
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < numChines; chine++)
                {
                    for (int axis = 0; axis < 3; axis++)
                    {
                        if (!double.TryParse(lines[index], out m_bulkheads[bulkhead][chine, axis]))
                            return "Invalid file format on line " + index;
                        index++;
                    }
                }
            }

            m_IsValid = true;

            return "";
        }

        public void CopyBulkheads(double[][,] bulkheads)
        {
            for (int bulkhead = 0; bulkhead < numBulkheads; bulkhead++)
            {
                for (int chine = 0; chine < m_bulkheads[bulkhead].GetLength(0); chine++)
                {
                    for (int axis = 0; axis < 3; axis++)
                    {
                        bulkheads[bulkhead][chine, axis] = m_bulkheads[bulkhead][chine, axis];
                    }
                }
            }
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
            m_bulkheads[bulkhead][chine, 0] = x;
            m_bulkheads[bulkhead][chine, 1] = y;
            m_bulkheads[bulkhead][chine, 2] = z;
        }
        public void ShiftBulkheadPoint(int bulkhead, int chine, double x, double y, double z)
        {
            m_bulkheads[bulkhead][chine, 0] += x;
            m_bulkheads[bulkhead][chine, 1] += y;
            m_bulkheads[bulkhead][chine, 2] += z;
        }

    }
}
