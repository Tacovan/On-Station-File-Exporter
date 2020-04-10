using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class FixedPoint
    {
        public int LineNumber { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool Used { get; set; } = false;

        public FixedPoint()
        {
        }

        // Reads a fixed point entry and returns the last line of that entry
        public int Read(List<Line> lines,int index)
        { 
            LineNumber = lines[index].LineNumber;
            if ( lines[index].Token=="StationName")
            {
                Name = lines[index].ValueS;
            }
            else
            {
                throw new Exception("Expecting StationName= at " + lines[index].LineNumber);
            }
            index++;
            if (lines[index].Token == "ConstraintComment")
            {
                Comment= lines[index].ValueS;
            }
            else
            {
                throw new Exception("Expecting ConstraintComment= at " + lines[index].LineNumber);
            }
            index++;
            if (lines[index].Token == "StationLocation")
            {
                var numbers = lines[index].ValueS.Split(' ');
                X = double.Parse(numbers[0]);
                Y = double.Parse(numbers[1]);
                Z = double.Parse(numbers[2]);
            }
            else
            {
                throw new Exception("Expecting StationLocation= at " + lines[index].LineNumber);
            }
            return index;
        }

        public void WriteLine(StreamWriter writer)
        {
            writer.WriteLine("  " + Name + "," + X.ToString() + "," + Y.ToString() + "," + Z.ToString());
        }

        public static void CreateWallsFile(List<FixedPoint> fixedPoints, string path)
        {
            StreamWriter writer = System.IO.File.CreateText(path);
            writer.WriteLine(";Fixed Point List");
            writer.WriteLine(";Export from "+App.VersionAndContactInfo);
            writer.WriteLine("#Units Meters order=ENU");
            fixedPoints.ForEach(f =>
            {
                writer.WriteLine("#Fix    " + f.Name + "  " + f.X + "  " + f.Y + "  " + f.Z);
            });
            writer.Close();
        }
    }
}
