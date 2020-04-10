using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Survey
    {
        public Survey()
        {
        }

        public string SurveyName
        {
            get; set;
        }
        public string SurveyDate { get; set; }

        public string SurveyDescription { get; set; }

        public double Declination { get; set; }

        public string DataOrder { get; set; }

        // Some people like to track instruments in case something turns out to be faulty.
        #region Instrument Details
        public string LengthUnits { get; set; }
        public string AzimuthUnits { get; set; }
        public string InclinationUnits { get; set; }

        public string FrontCompassName { get; set; }
        public double FrontCompassCorrection { get; set; } = Double.NaN;
        public double FrontCompassStandardError { get; set; } = Double.NaN;

        public string BackCompassName { get; set; }
        public double BackCompassCorrection { get; set; } = Double.NaN;
        public double BackCompassStandardError { get; set; } = Double.NaN;

        public string FrontClinoName { get; set; }
        public double FrontClinoCorrection { get; set; } = Double.NaN;
        public double FrontClinoStandardError { get; set; } = Double.NaN;

        public string BackClinoName { get; set; }
        public double BackClinoCorrection { get; set; } = Double.NaN;
        public double BackClinoStandardError { get; set; } = Double.NaN;

        public string TapeName { get; set; }
        public double TapeCorrection { get; set; } = Double.NaN;
        public double TapeStandardError { get; set; } = Double.NaN;
        #endregion

        public TeamMember[] SurveyTeam = { new TeamMember(), new TeamMember(), new TeamMember(), new TeamMember(), new TeamMember(), new TeamMember() };

        public List<Shot> Shots = new List<Shot>();
        public List<Comment> Comments = new List<Comment>();
        public List<Wall> Walls = new List<Wall>();
        public bool HasBacksights()
        {
            return Shots.Any(shot => shot.HasBacksight);
        }

        public int Read(List<Line> lines, int start)
        {
            Console.WriteLine("Found survey at line " + lines[start].LineNumber);
            if (!lines[start].TokenValue("Begin", "Survey"))
            {
                throw new Exception("Expecting Begin=Survey at line " + lines[start].LineNumber);
            }
            start++;
            while (!lines[start].TokenValue("End", "Survey"))
            {
                switch (lines[start].Token)
                {
                    case "SurveyName":
                        SurveyName = lines[start].ValueS;
                        break;
                    case "SurveyDate":
                        SurveyDate = lines[start].ValueS;
                        break;
                    case "SurveyDescription":
                        SurveyDescription = lines[start].ValueS;
                        break;
                    case "Declination":
                        Declination = lines[start].ValueDouble;
                        break;
                    case "DataOrder":
                        DataOrder = lines[start].ValueS;
                        break;
                    case "LengthUnits":
                        LengthUnits = lines[start].ValueS;
                        break;
                    case "AzimuthUnits":
                        AzimuthUnits = lines[start].ValueS;
                        if (this.AzimuthUnits != "D")
                        {
                            throw new Exception("Found Azimuth units other than degress. Haven't encountered this yet. Contact tacovan@gmail.com to get this added.");
                        }
                        break;
                    case "InclinationUnits":
                        InclinationUnits = lines[start].ValueS;
                        if (this.AzimuthUnits != "D")
                        {
                            throw new Exception("Found inclination units other than degress. Haven't encountered this yet. Contact tacovan@gmail.com to get this added.");
                        }
                        break;

                    case "FrontCompass":
                        FrontCompassName = lines[start].ValueS;
                        break;
                    case "FrontCompassCorrection":
                        FrontCompassCorrection = lines[start].ValueDouble;
                        break;
                    case "FrontCompassStandardError":
                        FrontCompassStandardError = lines[start].ValueDouble;
                        break;

                    case "BackCompass":
                        BackCompassName = lines[start].ValueS;
                        break;
                    case "BackCompassCorrection":
                        BackCompassCorrection = lines[start].ValueDouble;
                        break;
                    case "BackCompassStandardError":
                        BackCompassStandardError = lines[start].ValueDouble;
                        break;

                    case "BackClino":
                        BackClinoName = lines[start].ValueS;
                        break;
                    case "BackClinoCorrection":
                        BackClinoCorrection = lines[start].ValueDouble;
                        break;
                    case "BackClinoStandardError":
                        BackClinoStandardError = lines[start].ValueDouble;
                        break;

                    case "FrontClino":
                        FrontClinoName = lines[start].ValueS;
                        break;
                    case "FrontClinoCorrection":
                        FrontClinoCorrection = lines[start].ValueDouble;
                        break;
                    case "FrontClinoStandardError":
                        FrontClinoStandardError = lines[start].ValueDouble;
                        break;

                    case "Tape":
                        TapeName = lines[start].ValueS;
                        break;
                    case "TapeCorrection":
                        TapeCorrection = lines[start].ValueDouble;
                        break;
                    case "TapeStandardError":
                        TapeStandardError = lines[start].ValueDouble;
                        break;
                    case "Begin":
                        start = ParseBlock(lines, start);
                        break;
                    case "Person1":
                        SurveyTeam[0].Name = lines[start].ValueS;
                        break;
                    case "Person2":
                        SurveyTeam[1].Name = lines[start].ValueS;
                        break;
                    case "Person3":
                        SurveyTeam[2].Name = lines[start].ValueS;
                        break;
                    case "Person4":
                        SurveyTeam[3].Name = lines[start].ValueS;
                        break;
                    case "Person5":
                        SurveyTeam[4].Name = lines[start].ValueS;
                        break;
                    case "Person6":
                        SurveyTeam[5].Name = lines[start].ValueS;
                        break;
                    case "Duty1":
                        SurveyTeam[0].Duty = lines[start].ValueS;
                        break;
                    case "Duty2":
                        SurveyTeam[1].Duty = lines[start].ValueS;
                        break;
                    case "Duty3":
                        SurveyTeam[2].Duty = lines[start].ValueS;
                        break;
                    case "Duty4":
                        SurveyTeam[3].Duty = lines[start].ValueS;
                        break;
                    case "Duty5":
                        SurveyTeam[4].Duty = lines[start].ValueS;
                        break;
                    case "Duty6":
                        SurveyTeam[5].Duty= lines[start].ValueS;
                        break;
                }
                start++;
            }
            if (_shots.Count != 0)
            {
                this._shots.ForEach(s =>
                {
                    this.Shots.Add(new Shot(s, this.DataOrder));
                });
                this._comments.ForEach(comment =>
                {
                    this.Comments.Add(new Comment(comment));
                });
                this._walls.ForEach(wall =>
                {
                    this.Walls.Add(new Wall(wall, this.DataOrder));
                });
            }
            return start;
        }

        public string SurveyNameStripped
        {
            get
            {
                string noSpaceName = SurveyName.Replace(" ", "");
                noSpaceName = noSpaceName.Replace(",", "");
                noSpaceName = noSpaceName.Replace("*", "");
                noSpaceName = noSpaceName.Replace(".", "");
                return noSpaceName;
            }
        }

        // Exports the survey directly into Compass format so that we don't lose fidelity
        public List<string> WriteToCompass(StreamWriter fileStream)
        {
            fileStream.WriteLine("Compass Folder Export from " + App.VersionAndContactInfo);
            List<string> statusStrings=new List<string>();

            fileStream.WriteLine("SURVEY NAME: " + SurveyNameStripped);
            if (this.SurveyDate != "")
            {
                // mm/dd/yyyy
                string[] split = SurveyDate.Split('/');
                if (split[1].Length == 1)
                {
                    split[1] = "0" + split[1];
                }
                if (split[2].Length == 1)
                {
                    split[2] = "0" + split[2];
                }
                fileStream.Write("SURVEY DATE: "  + split[1] + " " + split[2] + " " + split[0]+"  ");  // MM DD YYYY
            }
            fileStream.WriteLine("COMMENT: " + this.SurveyDescription);

            fileStream.WriteLine("SURVEY TEAM:");
            bool leadingComma = false;
            // Maximum of 5. Format is Name(duty)
            for (int i=0;i<this.SurveyTeam.Length;i++ )
            {
                if ( SurveyTeam[i].Name!="")
                {
                    if (leadingComma)
                    {
                        fileStream.Write(',');
                    }
                    fileStream.Write(SurveyTeam[i].CompassExportString);
                    leadingComma = true;
                }
            }
            fileStream.WriteLine();

            // Survey header
            fileStream.Write("DECLINATION: " + Declination.ToString() + "  ");
            fileStream.Write("FORMAT: D");
            fileStream.Write(this.LengthUnits == "M"?"MM":"DD");  // Length and passage units Meters or decimal feet (I=Feet and inches)
            fileStream.Write("D"); // inclincation units
            fileStream.Write("LRUD");
            fileStream.Write("LAD");
            if (HasBacksights())
            {
                fileStream.Write("adBF");   // Backsights, LRUD with from station
            }
            else
            {
                fileStream.Write("NF");   // No backsights, LRUD with from station
            }
            fileStream.Write("  CORRECTIONS: ");
            fileStream.Write(TapeCorrection.ToString() + " " + FrontCompassCorrection.ToString() +"  "+FrontClinoCorrection.ToString());
            if ( HasBacksights())
            {
                fileStream.Write("  CORRECTIONS2: ");
                fileStream.Write(BackCompassCorrection.ToString() + "  " + BackClinoCorrection.ToString());
            }
            fileStream.WriteLine();  // Terminate
            fileStream.WriteLine();  // Blank line
            // Header is purely descriptive
            Shot.WriteCompassString(fileStream,"FROM", 15);
            Shot.WriteCompassString(fileStream, "TO", 15);
            Shot.WriteCompassString(fileStream, "LENGTH", 10);
            Shot.WriteCompassString(fileStream, "COMPASS", 10);
            Shot.WriteCompassString(fileStream, "CLINO", 10);
            Shot.WriteCompassString(fileStream, "LEFT", 10);
            Shot.WriteCompassString(fileStream, "RIGHT", 10);
            Shot.WriteCompassString(fileStream, "UP", 10);
            Shot.WriteCompassString(fileStream, "DOWN", 10);
            if (HasBacksights())   // Compass edits these in the right order, but assumes they are here in the file
            {
                Shot.WriteCompassString(fileStream, "BACKCOMP", 10);
                Shot.WriteCompassString(fileStream, "BACKCLIN", 10);
            }
            Shot.WriteCompassString(fileStream, "FLAGS", 7);
            Shot.WriteCompassString(fileStream, "COMMENTS", 10);
            fileStream.WriteLine();
            fileStream.WriteLine();

            foreach (var shot in Shots)
            {
                bool endOfLeg = !this.Shots.Any(x => x.From == shot.To);
                var comments = FindCommentsForShot(shot,endOfLeg);
                var wall = Walls.FirstOrDefault(x => x.Station == shot.From);
                if (wall != null)
                {
                    wall.Used = true;
                }
                shot.WriteCompassLine(fileStream, comments,wall,HasBacksights(),false,LengthUnits=="M");
                if ( endOfLeg)
                {
                    // Put the wall here as a station with a FROM no TO. Must follow the station with the TO
                    var wallEnd = Walls.FirstOrDefault(x => x.Station == shot.To);
                    if (wallEnd != null && !wallEnd.Used)
                    {
                        wallEnd.WriteCompassTerminalStation(fileStream,comments,HasBacksights(),LengthUnits=="M");
                        wallEnd.Used = true;
                    }
                }
            }
            fileStream.WriteLine('\f');  // Formfeed


            // Make sure we didn't miss data. Be careful, because a wall has to show up with a FROM and no TO, and has to be under the station with the TO
            // and we are already checking that
            foreach (var wall in Walls)
            {
                if (!wall.Used)
                {
                    // Place comment after the line
                    throw new Exception("Found a wall we didn't use at line " + wall.LineNumber);
                }
            }


            foreach (var comment in Comments)
            {
                if (!comment.Used)
                {
                    throw new Exception("Found a comment we didn't use at line" + comment.LineNumber);
                }
            }
            return statusStrings;

        }

        public bool ReferencesKnownStation(List<string> knownStations)
        {
            return Shots.Any(shot =>
                knownStations.Contains(shot.To) || knownStations.Contains(shot.From));
        }
            
        public void AddKnownStations(List<string> knownStations)
        {
            Shots.ForEach(shot =>
            {
                if (!knownStations.Contains(shot.From))
                {
                    knownStations.Add(shot.From);
                }
                if (!knownStations.Contains(shot.To))
                {
                    knownStations.Add(shot.To);
                }
            });
        }

        public static string MakeUniqueName( List<string> uniqueNames,string starting)
        {
            string root = starting.Substring(0, Math.Min(8, starting.Length));
            if ( !uniqueNames.Contains(root.ToUpper()))
            {
                uniqueNames.Add(root.ToUpper());
                return root;
            }

            root = starting.Substring(0, Math.Min(6, starting.Length));
            int index = 1;
            while (uniqueNames.Contains( (root+index.ToString()).ToUpper()) )
            {
                index++;
            }
            string name = root + index.ToString();
            uniqueNames.Add(name.ToUpper());
            return name;
        }

        public string WriteToWalls(string fullPathToFolder,StreamWriter wpjFile,List<string> usedFileNames,string subFolderName,ShotRenamer renamer)
        {
            string fileName = MakeUniqueName(usedFileNames,SurveyNameStripped);

            // Add the survey to the project file
            wpjFile.WriteLine(".SURVEY " + SurveyName);
            wpjFile.WriteLine(".NAME " + fileName);
            wpjFile.WriteLine(".STATUS 8");  // no idea what this means.
            wpjFile.WriteLine(".PATH " + subFolderName);

            // Create the survey file
            StreamWriter svxFile = File.CreateText(fullPathToFolder + "\\" + fileName+".srv");
            svxFile.WriteLine(";Exported Survey from " + App.VersionAndContactInfo);
            svxFile.WriteLine(";Survey: "+SurveyName);
            svxFile.WriteLine(";Description: " + SurveyDescription);
            // Add people as comments
            foreach (var person in SurveyTeam)
            {
                if ( person.Name!="")
                {
                    svxFile.WriteLine("; " + person.Name + " (" + person.Duty + ")");
                }
            }
            if (TapeName != "")
            {
                svxFile.Write(";Tape: " + TapeName);
            }
            if ( FrontCompassName!="")
            {
                svxFile.Write(";Front compass: " + FrontCompassName);
            }
            if (BackCompassName != "")
            {
                svxFile.Write(";Back compass: " + BackCompassName);
            }
            if (FrontClinoName != "")
            {
                svxFile.Write(";Front clino: " + FrontClinoName);
            }
            if (BackClinoName!= "")
            {
                svxFile.Write(";Back clino: " + BackClinoName);
            }
            svxFile.Write("#Units "+((LengthUnits=="M")?"Meters":"Feet"));
            svxFile.Write(" Order=DAV ");
            svxFile.Write("LRUD = F");
            if (Declination.CompareTo(0) != 0)
            {
                svxFile.Write(" Decl=" + Declination);
            }
            if (TapeCorrection.CompareTo(0)!=0 )
            {
                svxFile.Write(" INCD=" + TapeCorrection.ToString());
            }
            if (FrontCompassCorrection.CompareTo(0) != 0)
            {
                svxFile.Write(" INCA=" + FrontCompassCorrection.ToString());
            }
            if (BackCompassCorrection.CompareTo(0) != 0)
            {
                svxFile.Write(" INCAB=" + BackCompassCorrection.ToString());
            }
            if (FrontClinoCorrection.CompareTo(0) != 0)
            {
                svxFile.Write(" INCV=" + FrontClinoCorrection.ToString());
            }
            if (BackClinoCorrection.CompareTo(0) != 0)
            {
                svxFile.Write(" INCVB=" + BackClinoCorrection.ToString());
            }
            if (HasBacksights())
            {
                svxFile.Write(" typeAB=N,2 typeVB=N,2");  // Backsights are normal (rather than corrected with a tolerance of 2
            }
            svxFile.WriteLine();
            
            string[] s = this.SurveyDate.Split('/');
            svxFile.WriteLine("#Date " + s[0] + '-' + s[1] + '-' + s[2]);
            svxFile.WriteLine(";FROM TO DIST AZ INCL   WALLS");
            this.Shots.ForEach(shot =>
            {
                bool endOfLeg = !this.Shots.Any(x => x.From == shot.To);
                var comments = FindCommentsForShot(shot, endOfLeg);
                var wall = FindWallForShot(shot);
                shot.WriteToWallsFile( svxFile,wall,comments, HasBacksights(),renamer);
                comments.ForEach(comment =>
                {
                    comment.Used = true;
                });
                if (wall != null)
                {
                    wall.Used = true;
                }
                if (endOfLeg)
                {
                    // Put the wall here as a station with a FROM not a TO. Must follow the station with the TO
                    var wallEnd = Walls.FirstOrDefault(x => x.Station == shot.To);
                    if (wallEnd != null && !wallEnd.Used)
                    {
                        wallEnd.WriteEndToWallsFile(svxFile,renamer);
                        wallEnd.Used = true;
                    }
                }
            });
            svxFile.WriteLine();
            svxFile.Close();
            return fileName;
        }

        /*
        public string WriteSurvex(string folderPath)
        {
            string fileName = this.SurveyName + ".svx";
            StreamWriter svxFile = File.CreateText(folderPath + "\\" + fileName);
            svxFile.WriteLine("Exported Survey from " + App.VersionAndContactInfo);
            svxFile.WriteLine();
            svxFile.WriteLine(";"+SurveyName);
            svxFile.WriteLine(";" +SurveyDescription);
            svxFile.WriteLine(";from    to   length   compass    clino      comments");
            svxFile.Close();
            return fileName;
        }
        */

        void WriteOptional(StreamWriter writer, string token, string value)
        {
            if (value != "")
            {
                writer.WriteLine(token + " " + value);
            }
        }
        void WriteOptional(StreamWriter writer, string token, double value)
        {
            if (value.CompareTo(double.NaN)!=0)
            {
                writer.WriteLine(token + " " + value.ToString());
            }
        }

        /*
        internal void WriteSEF(StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("**** "+SurveyName+" *****");
            writer.WriteLine("#ctsurvey " + this.SurveyName);
            WriteOptional(writer, "#com", SurveyDescription);
            if (this.SurveyDate != "")
            {
                // mm/dd/yyyy
                string[] split = SurveyDate.Split('/');
                if (split[1].Length == 1)
                {
                    split[1] = "0" + split[1];
                }
                if (split[2].Length == 1)
                {
                    split[2] = "0" + split[2];
                }
                 writer.WriteLine("#date " + split[1] + "/" + split[2] + "/" + split[0]);
            }
            WriteOptional(writer, "#mtid", TapeName);
            WriteOptional(writer, "#mtc", TapeCorrection);

            // Forward instruments
            WriteOptional(writer, "#fcid", FrontCompassName);
            WriteOptional(writer, "#fcc", FrontCompassCorrection);
            WriteOptional(writer, "#fiid", FrontClinoName);
            WriteOptional(writer, "#fic", FrontClinoCorrection);

            // Backward instruments
            if ( HasBacksights())
            {
                WriteOptional(writer, "#biid", BackClinoName);
                WriteOptional(writer, "#bic", BackClinoCorrection);
                WriteOptional(writer, "#bcid", BackCompassName);
                WriteOptional(writer, "#bcc", BackCompassCorrection);
            }


            WriteOptional(writer, "#clinerr", FrontClinoStandardError);
            WriteOptional(writer, "#comperr", FrontCompassStandardError);

            writer.WriteLine("#units " + (this.LengthUnits == "M" ? "meters" : "feet"));

            // Write the team
            for (int i=0;i<6;i++)
            {
                WriteOptional(writer,"#person", SurveyTeam[i].Name);
                if (SurveyTeam[i].Name != "")
                {
                    WriteOptional(writer, "#duty", SurveyTeam[i].Duty);
                }
            }

            // Write the data. Just use a standard order
            if (HasBacksights())
            {
                writer.WriteLine("#data from,to,dist,fazi,bazi,finc,binc,left,right,ceil,floor");
            }
            else
            {
                writer.WriteLine("#data from,to,dist,fazi,finc,left,right,ceil,floor");
            }

            foreach (var shot in Shots)
            {
                bool endOfLeg = !this.Shots.Any(x => x.From == shot.To);
                var comments = FindCommentsForShot(shot,endOfLeg);
                comments.ForEach(comment =>
                {
                    comment.Used = true;
                    comment.WriteLine(writer);
                });

                var wall = FindWallForShot(shot);
                if ( wall!=null)
                {
                    wall.Used = true;
                }
                shot.WriteLine(writer,wall, HasBacksights());

                if ( endOfLeg )
                {
                    // Put the wall here as a station with a FROM not a TO. Must follow the station with the TO
                    var wallEnd = Walls.FirstOrDefault(x => x.Station == shot.To);
                    if (wallEnd != null && !wallEnd.Used)
                    {
                        wallEnd.WriteTerminalStation(writer, HasBacksights());
                        wallEnd.Used = true;
                    }
                }
            }

            // Make sure we didn't miss date. Be careful, because a wall has to show up with a FROM and no TO, and has to be under the station with the TO
            // and we are already checking that
            foreach (var wall in Walls)
            {
                if (!wall.Used)
                {
                    // Place comment after the line
                    throw new Exception("Found a wall we didn't use at line " + wall.LineNumber);
                }
            }


            foreach (var comment in Comments)
            {
                if ( !comment.Used)
                {
                    throw new Exception("Found a comment we didn't use at line" + comment.LineNumber);
                }
            }
            writer.WriteLine("#endctsurvey " + this.SurveyName);
        }
        */



        // Finds all unused comments referencing the FROM station. Unless it's the end of a survey leg in which case the final station
        // is never a FROM station, and we also allow a TO station
        List<Comment> FindCommentsForShot(Shot shot, bool endOfLeg)
        {
            return Comments.Where(comment=> 
                (!comment.Used && 
                ( (comment.Station == shot.From) || (endOfLeg && comment.Station==shot.To))
             )).ToList();
        }

        Wall FindWallForShot(Shot shot)
        {
            return Walls.FirstOrDefault(x => x.Station == shot.From && shot.AzFront.CompareTo(x.AzFront)==0 && shot.IncFront.CompareTo(x.IncFront)==0  && !x.Used);
        }

        List<Line> _shots = new List<Line>();
        List<Line> _walls = new List<Line>();
        List<Line> _comments = new List<Line>();

        int ParseBlock(List<Line> lines, int start)
        {
            if (lines[start].Token != ("Begin"))
            {
                throw new Exception("Expecting Begin=Survey at line " + lines[start].LineNumber);
            }
            string blockType = lines[start].ValueS;
            List<Line> block = new List<Line>();
            start++;
            while (!lines[start].TokenValue("End", blockType))
            {
                block.Add(lines[start]);
                start++;
            }
            switch (blockType)
            {
                case "Shots":
                    _shots = block;
                    break;
                case "Comments":
                    _comments = block;
                    break;
                case "Walls":
                    _walls = block;
                    break;
                default:
                    throw new Exception("Unexpected block type: " + blockType + " at line " + lines[start].LineNumber);
            }
            return start;
        }
    }

    public class TeamMember
    {
        public string Name { get; set; } = "";
        public string Duty { get; set; } = "";
        public TeamMember()
        {

        }

        public string CompassExportString
        {
            get
            {
                if (Duty != "")
                {
                    return Name + "(" + Duty + ")";
                }
                else
                {
                    return Name;
                }
            }
        }
    }
}
