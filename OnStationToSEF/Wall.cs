using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Wall
    {
        public string Station { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Up { get; set; }
        public double Down { get; set; }
        public double AzFront { get; set; }
        public double IncFront { get; set; }
        public bool Used { get; set; } = false;
        public int LineNumber { get; set; }

        public Wall(Line line, string dataorder)
        {
            LineNumber = line.LineNumber;
            string[] values = line.ValueS.Split(' ');
            Station= values[0];
            // TAIUDLR
            AzFront = Line.ParseDOrNan(values[1]);
            IncFront = Line.ParseDOrNan(values[2]);

            int upIndex = dataorder.IndexOf('U'); //
            {
                Up = Line.ParseDOrNan(values[upIndex]);
            }
            int downIndex = dataorder.IndexOf('D'); //
            {
                Down = Line.ParseDOrNan(values[downIndex]);
            }
            int leftIndex= dataorder.IndexOf('L'); //
            {
                Left = Line.ParseDOrNan(values[leftIndex]);
            }
            int rightIndex = dataorder.IndexOf('R'); //
            {
                Right = Line.ParseDOrNan(values[rightIndex]);
            }


        }

        public void WriteTerminalStation(StreamWriter writer, bool hasBacksights)
        {
            writer.Write("  ");

            // Output from Walls is
            // #data type,from,to,dist,fazi,finc,bazi,binc,iab,tab,left,right,ceil,floor
            // *,5,5,*,*,0,*,*,*,*,0,2,3,0

            // Add the station name twice. Walls expects this and outputs it
            writer.Write(Station + ","+Station +",*,*,*,");  // from, to, length, comp, incl,
            if (hasBacksights)
            {
                writer.Write("*,*,");  // two more for compBack and Incl Back
            }
            Shot.WriteDouble(writer, Left,true);
            Shot.WriteDouble(writer, Right, true);
            Shot.WriteDouble(writer, Up, true);
            Shot.WriteDouble(writer, Down,false);
            writer.WriteLine();
        }

        internal void WriteCompassTerminalStation(StreamWriter fileStream,List<Comment> comments,bool HasBacksights,bool toFeet)
        {
            Shot.WriteCompassString(fileStream, Station, 15);  // From and To should be the same
            Shot.WriteCompassString(fileStream, Station, 15);

            Shot.WriteCompassString(fileStream, 0.0, 10);  // Length
            double az = AzFront;
            if (az.CompareTo(double.NaN)==0)
            {
                az = 0;
            }
            double inc = IncFront;
            if (inc.CompareTo(double.NaN) == 0)
            {
                inc = 0;
            }
            Shot.WriteCompassString(fileStream, az, 10);  // Compass
            Shot.WriteCompassString(fileStream, inc, 10);  // Clino
            Shot.WriteCompassString(fileStream, Shot.MakeFeet(Left,toFeet), 10);  // LRUD
            Shot.WriteCompassString(fileStream, Shot.MakeFeet(Right,toFeet), 10);  // LRUD
            Shot.WriteCompassString(fileStream, Shot.MakeFeet(Up,toFeet), 10);  // LRUD
            Shot.WriteCompassString(fileStream, Shot.MakeFeet(Down,toFeet), 10);  // LRUD
            if (HasBacksights)
            {
                Shot.WriteCompassString(fileStream, double.NaN, 10);  // CompassBack
                Shot.WriteCompassString(fileStream, double.NaN, 10);  // ClinoBack
            }

            if ( comments.Count!=0)
            {
                string allComments=" (Final wall data) ";
                comments.ForEach(comment =>
                {
                    allComments += comment.Station + ": " + comment.CommentText + " ";
                    comment.Used = true;
                });
                if (allComments.Length > 79)
                {
                    allComments = allComments.Substring(0, 79);
                }
                fileStream.Write(allComments);
            }
            fileStream.WriteLine();
        }

        public void WriteEndToWallsFile(StreamWriter svxFile,ShotRenamer renamer)
        {
            svxFile.Write(renamer.GetShortName(Station));
            WriteToWallsFile(svxFile);
            svxFile.WriteLine();
        }
        public void WriteToWallsFile(StreamWriter svxFile)
        {
            svxFile.Write("  <");
            if ( Left.CompareTo(double.NaN)==0)
            {
                svxFile.Write("--,");
            }
            else
            {
                svxFile.Write(Left.ToString()+",");
            }
            if (Right.CompareTo(double.NaN) == 0)
            {
                svxFile.Write("--,");
            }
            else
            {
                svxFile.Write(Right.ToString() + ",");
            }
            if (Up.CompareTo(double.NaN) == 0)
            {
                svxFile.Write("--,");
            }
            else
            {
                svxFile.Write(Up.ToString() + ",");
            }
            if (Down.CompareTo(double.NaN) == 0)
            {
                svxFile.Write("--");
            }
            else
            {
                svxFile.Write(Down.ToString());
            }
            svxFile.Write(">");
        }
    }

}
