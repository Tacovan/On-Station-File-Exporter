using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Shot
    {
        // 
        public string Flags
        {
            get;
            set;
        }

        public bool Dive { get; set; } = false;
        public string From;
        public string To;
        public double Tape;
        public double AzFront;
        public double AzBack;
        public double IncFront;
        public double IncBack;
        public string DataOrder { get; set; }
        public string Comment { get; set; }
        public bool HasBacksight
        {
            get
            {
                return this.AzBack.CompareTo(double.NaN) != 0 || this.IncBack.CompareTo(double.NaN) != 0;
            }
        }

        void SetDiveMeasurements(string azimuth, string depth)
        {
            this.AzFront = Line.ParseDOrNan(azimuth);
            this.Depth= Line.ParseDOrNan(depth);
            this.AzBack = double.NaN;
            this.IncFront = double.NaN;
            this.IncBack = double.NaN;
        }

        public double Depth
        {
            get; set;
        } = double.NaN;

        void SetMeasurements(string a1,string a2,string i1,string i2)
        {
            this.AzFront = Line.ParseDOrNan(a1);
            this.AzBack= Line.ParseDOrNan(a2);
            this.IncFront= Line.ParseDOrNan(i1);
            this.IncBack = Line.ParseDOrNan(i2);
        }

        public Shot(Line line,string dataorder)
        {
            if ( line.Token=="DiveShot" )
            {
                Dive = true;
            }
            DataOrder =dataorder;
            string[] values = line.ValueS.Split(' ');
            From = values[0];
            To = values[1];
            if ( dataorder[0]!='T')
            {
                throw new Exception("Expected Tape to be first dataorder measurement at " + line.LineNumber);
            }
            Tape = double.Parse(values[2]);
            if (Dive)
            {
                if (DataOrder[1] == 'A' && DataOrder[2] == 'I')
                {
                    SetDiveMeasurements(values[3], values[4]);
                }
                else if (DataOrder[1] == 'I' && DataOrder[2] == 'A')
                {
                    SetDiveMeasurements(values[4], values[3]);
                }
            }
            else
            {
                if (DataOrder[1] == 'A' && DataOrder[2] == 'I')
                {
                    SetMeasurements(values[3], values[5], values[4], values[6]);
                }
                else if (DataOrder[1] == 'I' && DataOrder[2] == 'A')
                {
                    SetMeasurements(values[4], values[6], values[3], values[5]);
                }
                else
                {
                    throw new Exception("Expected Azimuth or Inclination to follow tape at " + line.LineNumber);
                }
            }

            // Read the flags. These indicate things like whether it's a surface survey and whether it counts in the overall length
            // Format is (ABC) where ABC are various flags
            if ( values.Length==8)
            {
                string flags = values[7];
                if ( flags[0]=='(')
                {
                    Flags = flags.Substring(1, flags.Length - 2);
                }
            }
        }

        public static void WriteDouble(StreamWriter writer,double d,bool comma)
        {
            if ( d.CompareTo(double.NaN)==0)
            {
                writer.Write("*");
            }
            else
            {
                writer.Write(d.ToString());
            }
            if (comma)
            {
                writer.Write(",");
            }
        }

        // Writes a right justified string of the given width
        public static void WriteCompassString(StreamWriter w,string s,int width)
        {
            string padded = "                " + s;
            w.Write(padded.Substring(padded.Length - width));
        }
        public static void WriteCompassString(StreamWriter w,double d,int width)
        {
            if ( d.CompareTo(double.NaN)==0)
            {
                WriteCompassString(w,"-9999.00",width);
            }
            else
            {
                WriteCompassString(w, String.Format("{0:0.###}",d), width);
            }
        }

        public void WriteCompassLine(StreamWriter writer, List<Comment> comments, Wall wall,bool hasBacksights,bool excludeLength,bool toFeet)
        {
            // Actually we should create an alias here but I don't think On Station allowed huge station names
            if ( From.Length>12)
            {
                From = From.Substring(From.Length-11, 11);
                Console.WriteLine("Error, truncated name");
            }
            if ( To.Length>12)
            {
                To = To.Substring(To.Length-11, 11);
                Console.WriteLine("Error, truncated name");
            }

            WriteCompassString(writer,From, 15);  // UP to 12 characters
            WriteCompassString(writer, To, 15);
            WriteCompassString(writer, MakeFeet(Tape,toFeet), 10);
            WriteCompassString(writer, AzFront, 10);
            WriteCompassString(writer, IncFront, 10);
            if ( wall==null)
            {
                WriteCompassString(writer, double.NaN, 10);
                WriteCompassString(writer, double.NaN, 10);
                WriteCompassString(writer, double.NaN, 10);
                WriteCompassString(writer, double.NaN, 10);
            }
            else
            {
                WriteCompassString(writer, MakeFeet(wall.Left,toFeet), 10);
                WriteCompassString(writer, MakeFeet(wall.Right,toFeet), 10);
                WriteCompassString(writer, MakeFeet(wall.Up,toFeet), 10);
                WriteCompassString(writer, MakeFeet(wall.Down,toFeet), 10);
            }
            // Backsights must come after the wall data.
            if (hasBacksights)
            {
                WriteCompassString(writer, AzBack, 10);
                WriteCompassString(writer, IncBack, 10);
            }
            if (excludeLength)
            {
                writer.Write("#|L ");   // Flags
            }
            if (comments.Count > 0)
            {
                writer.Write("  ");
                string commentTotal = "";
                comments.ForEach(comment =>
                {
                    commentTotal += comment.Station+": "+comment.CommentText+" ";
                    comment.Used = true;
                });
                if (commentTotal.Length>79)
                {
                    commentTotal = commentTotal.Substring(0,79);
                }
                writer.Write(commentTotal);
            }
            writer.WriteLine();
        }

        static public double MakeFeet(double measurement,bool toFeet)
        {
            if (!toFeet)
            {
                return measurement;
            }
            else
            {
                return measurement / 0.3048;
            }
        }

        public void WriteLine(StreamWriter writer,Wall wall,bool hasBacksights)
        {
            writer.Write("  ");
            writer.Write(From+",");
            writer.Write(To+",");
            WriteDouble(writer, Tape,true);
            WriteDouble(writer, AzFront, true);
            if (hasBacksights)
            {
                WriteDouble(writer, AzBack, true);
            }
            WriteDouble(writer, IncFront, hasBacksights || (wall!=null) );
            if (hasBacksights)
            {
                WriteDouble(writer, IncBack, wall!=null);
            }
            if (wall != null)
            {
                WriteDouble(writer, wall.Left, true);
                WriteDouble(writer, wall.Right, true);
                WriteDouble(writer, wall.Up, true);
                WriteDouble(writer, wall.Down, false);
            }
            writer.WriteLine();
        }

        static public void WriteWallsString(StreamWriter svxFile, string s, int length)
        {
            string temp = s + "                       ";
            svxFile.Write(temp.Substring(0, length));
        }
        static public void WriteWallsString(StreamWriter svxFile, double number, int length)
        {
            if ( number.CompareTo(double.NaN)==0)
            {
                WriteWallsString(svxFile, "--", length);
            }
            WriteWallsString(svxFile,number.ToString(), length);
        }

        static public void WriteWallsString(StreamWriter svxFile, double front, double back, int length)
        {
            String s = "";
            if (front.CompareTo(double.NaN) != 0)
            {
                s=front.ToString();
            }
            if (back.CompareTo(double.NaN) != 0)
            {
                s = s + "/";
                s=s+back.ToString();
            }
            WriteWallsString(svxFile, s, length);
        }

        public void WriteToWallsFile(StreamWriter svxFile, Wall wall,List<Comment> comments,bool hasBacksights,ShotRenamer renamer)
        {
            foreach (var comment in comments)
            {
                comment.WriteToWalls(svxFile);
            }
            WriteWallsString(svxFile,renamer.GetShortName(From), 15);
            WriteWallsString(svxFile, renamer.GetShortName(To), 15);
            WriteWallsString(svxFile, Tape, 15);
            WriteWallsString(svxFile, AzFront,AzBack,20);
            WriteWallsString(svxFile, IncFront, IncBack,20);
            if (wall!=null)
            {
                wall.WriteToWallsFile(svxFile);
            }
            svxFile.WriteLine();
        }
    }
}
