using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Folder
    {
        public Folder()
        {
            FolderName = "";
        }
        public string ExportFileName
        {
            get;
            set;
        }
        public bool Sorted { get; set; } = false;    // used for Compass

        List<Line> folderLines=new List<Line>();
        public List<Survey> Surveys = new List<Survey>();
        // Parses folders and subfolders into blocks of lines. Pass it the line with the Begin block.
        // Returns index of line with the End Block
        public int Read(List<Line> lines,int start)
        {
            if ( !lines[start].TokenValue("Begin","Folder"))
            {
                throw new Exception("Invalid argument. Expected a folder");
            }
            start++;
            while (!lines[start].TokenValue("End","Folder"))
            {
                if ( lines[start].Token=="FolderName")
                {
                    FolderName = lines[start].ValueS;
                }
                if (lines[start].TokenValue("Begin","Folder"))
                {
                    Folder subFolder = new Folder();
                    start = subFolder.Read(lines, start);
                    SubFolders.Add(subFolder);
                }
                else
                {
                    folderLines.Add(lines[start]);
                }
                start++;
            }
            Console.WriteLine("Folder " + this.FolderName + " starts at line " + this.folderLines[0].LineNumber+" and has " + this.folderLines.Count + " lines");
            return start;
        }

        public void ProcessSurveys()
        {
            Console.WriteLine("ProcessSurveys for folder " + this.FolderName);
            for (int i=0;i<this.folderLines.Count;i++)
            {
                if ( this.folderLines[i].TokenValue("Begin","Survey"))
                {
                    Survey s = new Survey();
                    i=s.Read(this.folderLines, i);
                    this.Surveys.Add(s);
                }
            }
            Console.WriteLine("Found " + this.Surveys.Count);

            this.SubFolders.ForEach(subFolder => subFolder.ProcessSurveys());
        }

        public int GetTotalSubfolders()
        {
            return this.SubFolders.Count + this.SubFolders.Sum((subfolder) => subfolder.GetTotalSubfolders());
        }

        public int GetTotalSurveys()
        {
            return this.Surveys.Count + this.SubFolders.Sum((subfolder) => subfolder.GetTotalSurveys());
        }

        public int GetTotalShots()
        {
            return this.Surveys.Sum(survey=>survey.Shots.Count)+ this.SubFolders.Sum((subfolder) => subfolder.GetTotalShots());
        }

        // There is a single root folder
        public static Folder LoadRootFolderAndChildren
            (List<Line> lines)
        {
            int index = 0;
            while (index < lines.Count)
            {
                if (lines[index].TokenValue("Begin", "Folder"))
                {
                    Folder f = new Folder();
                    index=f.Read(lines, index)+1;
                    return f;
                }
                index++;
            }
            throw new Exception("Could not find a root folder");
        }

        /*
        // Creates a new folder based on the FolderName and writes a .svx index file
        // as well as all the survey files.  Returns path of the form newfolder/index.svx
        public string WriteSurvex(string parentPath)
        {
            string CreateFolderName = FolderName;

            string newPath = parentPath + "\\" + CreateFolderName;
            System.IO.Directory.CreateDirectory(newPath);
            string shortFileName = "index.svx";
            string IndexFilePath= newPath + "\\" +shortFileName;
            StreamWriter IndexFileStream=File.CreateText(IndexFilePath);

            IndexFileStream.WriteLine(";Folder Export from "+App.VersionAndContactInfo);
            IndexFileStream.WriteLine(";Surveys in this folder");
            Surveys.ForEach(survey =>
            {
                string surveyFilePath=survey.WriteSurvex(newPath);
                IndexFileStream.WriteLine("*include " + surveyFilePath);
            });
            IndexFileStream.WriteLine();
            IndexFileStream.WriteLine(";SubFolders");
            SubFolders.ForEach(subFolder =>
            {
                string indexFilePath= subFolder.WriteSurvex(newPath);
                IndexFileStream.WriteLine("*include " + indexFilePath);
            });
            IndexFileStream.Close();
            return CreateFolderName + "/"+ shortFileName;
        }

    */
/*        public void WriteSEF(StreamWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine();
            writer.WriteLine( "----- " + FolderName + " -----" );
            writer.WriteLine("#dir "+ FolderName);

            Surveys.ForEach(survey =>
            {
                survey.WriteSEF(writer);
            });

            SubFolders.ForEach(subFolder =>
            {
                subFolder.WriteSEF(writer);
            });
            writer.WriteLine("#up");  // go back to this directory
        }
        */

        public void RecursiveAddNonEmptyFolders(List<Folder> flatFolders)
        {
            if (this.Surveys.Count > 0)
            {
                flatFolders.Add(this);
            }
            this.SubFolders.ForEach(x => x.RecursiveAddNonEmptyFolders(flatFolders));
        }

        public string FolderNameStripped
        {
            get
            {
                string name = this.FolderName.Replace(" ", "");
                name = name.Replace(",", "");
                return name;
            }
        }
        public void MakeCompassName(List<string> usedFolderNames)
        {
            var rand = new Random();
            string name = FolderNameStripped + ".dat";
            while (usedFolderNames.Contains(name))
            {
                // Handle duplicate folder names since we are flattening a tree
                name = "Renamed"+rand.Next().ToString().Substring(0,3);
            }
            ExportFileName = name;
            usedFolderNames.Add(name);
        }

        public List<string> WriteToCompass(string folderPath)
        {
            List<string> errors = new List<string>();
            StreamWriter fileStream = File.CreateText(folderPath + "/" + ExportFileName);
            Surveys.ForEach(survey =>
            {
                List<string> surveyErrors=survey.WriteToCompass(fileStream);
                surveyErrors.ForEach(error =>
                {
                    errors.Add(error);
                });
            });

            fileStream.Close();
            return errors;
        }

        public void WriteToWalls(string folderName,StreamWriter wpjFile,List<string> uniqueNames,ShotRenamer renamer)
        {
            System.IO.Directory.CreateDirectory(folderName+"/"+FolderNameStripped);

            wpjFile.WriteLine(".BOOK " + this.FolderName);
            wpjFile.WriteLine(".NAME "+Survey.MakeUniqueName(uniqueNames, this.FolderNameStripped));
            wpjFile.WriteLine(".STATUS 8");
            this.SubFolders.ForEach(subFolder =>
            {
                subFolder.WriteToWalls(folderName, wpjFile, uniqueNames,renamer);
            });
            this.Surveys.ForEach(survey =>
            {
                survey.WriteToWalls(folderName+"/"+FolderNameStripped, wpjFile, uniqueNames, FolderNameStripped,renamer);
            });
            wpjFile.WriteLine(".ENDBOOK    ;"+this.FolderName);
        }


        // It doesn't look like compass can handle folders that aren't referenced by the folder above them (forward references).
        internal static bool SortForCompass(List<Folder> flatFolders,List<FixedPoint> fixedPoints,out List<Folder> sorted)
        {
            Console.WriteLine("Ordering folders");
            if ( flatFolders.Count<2)
            {
                sorted = flatFolders.ToList();
                return true;
            }

            List<Folder> bestOrder = flatFolders.ToList();
            int best = 0;
            // Try each folder as a starting point. Then see if we can fit all the other folders under it
            for (int startWith=0;startWith<flatFolders.Count;startWith++)
            {
                List<Folder> unsorted = flatFolders.ToList();
                Console.WriteLine("  Trying parent:" + unsorted[startWith].FolderName);

                sorted = new List<Folder>();
                sorted.Add(unsorted[startWith]);
                unsorted.RemoveAt(startWith);   // put the starting folder
                List<string> knownStations = new List<string>();
                sorted[0].AddKnownStations(knownStations);
                for (int i = 0; i < unsorted.Count; i++)
                {
                    if ( unsorted[i].ResolvedBy(knownStations,out Survey problem))
                    {
                        sorted.Add(unsorted[i]);
                        unsorted[i].AddKnownStations(knownStations);
                        unsorted.RemoveAt(i);
                        i = -1;  // Go back to the start of the list
                    }
                    else
                    {
                        Console.WriteLine("       Can't add "+unsorted[i].FolderName+" because of " + problem.SurveyName);
                    }
                }
                if (unsorted.Count == 0)
                {
                    Console.Write("That worked.");
                    return true;  // Found a workable order
                }
                else
                {
                    if (sorted.Count>best)
                    {
                        Console.WriteLine("**** New best order ("+unsorted.Count+" unsorted)");
                        best = sorted.Count;
                        bestOrder = sorted.ToList();
                        unsorted.ForEach(unsortedItem => bestOrder.Add(unsortedItem));
                    }
                }
            }
            sorted = bestOrder;
            return false;
        }

        public static List<Folder> CollapseIntoSingleFolder(List<Folder> sortedFolders)
        {
            Folder f = new Folder();
            sortedFolders.ForEach(folder =>
            {
                folder.Surveys.ForEach(survey =>
               {
                   f.Surveys.Add(survey);
               });
            });
            return new List<Folder> { f };
        }

        public bool ReferencesFixedPoint(FixedPoint pt)
        {
            List<string> stations = new List<string> { pt.Name };
            return Surveys.Any(x => x.ReferencesKnownStation(stations));
        }

        // Returns true if every station in this survey is tied to one of the stations listed
        private bool ResolvedBy(List<string> knownStations,out Survey problem)
        {
            List<string> parentStationsThisFolder = new List<string>();
            foreach (var survey in Surveys)
            {
                if (!survey.ReferencesKnownStation(knownStations) && !survey.ReferencesKnownStation(parentStationsThisFolder))
                {
                    problem = survey;
                    return false;
                }
                survey.AddKnownStations(parentStationsThisFolder);
            }
            problem = null;
            return true;
        }

        public void AddKnownStations(List<string> knownStations)
        {
            Surveys.ForEach(s => s.AddKnownStations(knownStations));
        }

        public string FolderName
        {
            get;set;
        }

        public List<Folder> SubFolders { get; set; } = new List<Folder>();
    }
}
