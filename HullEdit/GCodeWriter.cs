using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HullEdit
{
    class GCodeWriter
    {
        private System.IO.StreamWriter gCodeFile;
        private const int PlungeSpeed = 8;
        private const int CutSpeed = 30;

        public GCodeWriter(string filename)
        {
            gCodeFile = new System.IO.StreamWriter(filename);
            gCodeFile.WriteLine("%");
            gCodeFile.WriteLine("O1234 ({0})", filename);
            gCodeFile.WriteLine("G20 G90 G40 G17 M05");     // inches, absolute, no offset, XY plane, Spindle ON
            gCodeFile.WriteLine("G00 Z0.25 M03");           // 0.25 inches above the surface
        }

        public void Close()
        {
            if (gCodeFile != null)
            {
                gCodeFile.WriteLine("G01 Z0.5 M05");        // lift out of material and turn spindle off
                gCodeFile.WriteLine("G91 G28 X0");          // return to zero
                gCodeFile.WriteLine("G91 G28 Z0");
                gCodeFile.WriteLine("G90");                 // absolute
                gCodeFile.WriteLine("G40");                 // no offset
                gCodeFile.WriteLine("M30");                 // end of program
                gCodeFile.Close();
            }
        }

        public void Write(PanelDisplay panel)
        {
            double offset = 0.125;
            PointCollection points = panel.GetPoints();
            Boolean first = true;
            Point firstPoint = new Point(0, 0);

            // Get the tool path, which is an offset from the panel points
            PointCollection path = Geometry.ParallelShape(points, offset, false);

            foreach (Point p in path)
            {
                if (first)
                {
                    firstPoint = p;
                    gCodeFile.WriteLine("G01 Z0.25 F{0}", PlungeSpeed);             // Lift cutter
                    gCodeFile.WriteLine("G00 X{0} Y{1}", p.X, p.Y);                 // cut 1/8" into surface
                    gCodeFile.WriteLine("G01 Z-0.125 F{0}", PlungeSpeed);           // cut 1/8" into surface
                    gCodeFile.WriteLine("G01 X{0} Y{1} F{2}", p.X, p.Y, CutSpeed);  // set cutspeed 
                    first = false;
                }
                else
                {
                    gCodeFile.WriteLine("G01 X{0} Y{1}", p.X, p.Y);
                }
            }
            gCodeFile.WriteLine("G01 X{0} Y{1}", firstPoint.X, firstPoint.Y);
            gCodeFile.WriteLine("G01 Z0.25 F{0}", PlungeSpeed);  // Lift cutter
        }

    }
}
