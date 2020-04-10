using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace OnStationExporter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new ConverterDataVM();
        }

        ConverterDataVM VM
        {
            get { return (ConverterDataVM)DataContext; }
        }
        async void OnBrowseCDIFile(Object sender,EventArgs e)
        {
            OpenFileDialog dlg=new OpenFileDialog();
            dlg.DefaultExt = ".cdi";
            if (dlg.ShowDialog()==true)
            {
                string error=await VM.ParseFile(dlg.FileName);
                if (error != "")
                {
                    MessageBox.Show(this, "Unable to read file: " + error);
                }
            }
        }

        /*
        async void OnSaveSEFFile(Object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = ".sef";
            dialog.OverwritePrompt = true;
            dialog.AddExtension = true;
            if (dialog.ShowDialog() == true)
            {
                string error = await VM.SaveToSEF(dialog.FileName);
                CommonShowResult(error);
            }
        }
        */

        void CommonShowResult(string error)
        { 
            if ( error!="")
            {
                MessageBox.Show(this,"Unable to save file. If you can't resolve this contact tacovan@gmail.com. I'll be happy to try help you preserve your old survey data. Error reported was: " + error,"Error");
            }
            else
            {
                MessageBox.Show(this,"Conversion Completed. Please check your cave carefully for any errors. If you encounter problems with the conversion process contact tacovan@gmail.com.","Success");
                VM.Reset();
            }
        }

        public async void OnSaveWallsFile(Object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Walls export folder";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string error = await VM.SaveToWalls(dialog.FileName);
                CommonShowResult(error);
            }
        }


        async void OnSaveCompassFile(Object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.Title = "Select Compass export folder";
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string error = await VM.SaveToCompass(dialog.FileName);
                CommonShowResult(error);
            }
        }

        /*
        async void OnSaveSurvexFile(Object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string error = await VM.GenerateSurvexProject(dialog.FileName);
                if (error != "")
                {
                    MessageBox.Show(this, "Unable to save file: " + error, "Error");
                }
                else
                {
                    MessageBox.Show(this, "Conversion Completed. Enjoy your shiny old data.", "Success");
                }
            }
        }*/
    }

        public class ConverterDataVM : INotifyPropertyChanged
    {
        public ConverterDataVM()
        {

        }
        public string VersionAnContactInfo
        {
            get { return App.VersionAndContactInfo; }
        }

        /*
        public async Task<string> GenerateSurvexProject(string folderName)
        {
            this.ShowSave = false;
            this.ProgressList.Clear();
            this.ProgressList.Add("Generating Survex Project at " + folderName);
            await Task.Delay(1);
            try
            {
                string RootSVX = folderName + "\\all.svx";
                StreamWriter rootWriter = File.CreateText(RootSVX);
                rootWriter.WriteLine(";Survex Export from " + App.VersionAndContactInfo);
                rootWriter.WriteLine();
                rootWriter.WriteLine(";No Surface Data included");
                rootWriter.WriteLine();
                rootWriter.WriteLine(";Cave Data. Folders correspond to On Station Folder Structure");
                string rootFolderIndexFilePath = RootFolder.WriteSurvex(folderName);
                rootWriter.WriteLine("*include " + rootFolderIndexFilePath);
                rootWriter.WriteLine();
                rootWriter.Close();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        */
         
        public async Task<string> SaveToWalls(string folderName)
        {
            this.ProgressList.Add("Generating Walls project in:" + folderName);
            await Task.Delay(1);
            this.ShowSave = false;
            try
            {
                string sourceFileName = System.IO.Path.GetFileNameWithoutExtension(this.FileName);
                StreamWriter writer = File.CreateText(folderName + "/" + sourceFileName + ".wpj");
                writer.WriteLine(";Walls Project Export from " + App.VersionAndContactInfo);
                writer.WriteLine(".BOOK ProjectRoot");
                writer.WriteLine(".NAME OnStationExport");
                writer.WriteLine(".OPTIONS flag=Fixed");     // No idea what this does
                writer.WriteLine(".STATUS 3");     // No idea what this does

                List<string> uniqueNames = new List<string>();
                if (FixedPoints.Count!=0)
                {
                    writer.WriteLine(".BOOK FixedPoints");
                    writer.WriteLine(".NAME Folder");
                    uniqueNames.Add("Folder".ToUpper());
                    writer.WriteLine(".STATUS 8");
                    writer.WriteLine(".SURVEY FixedPointList");
                    writer.WriteLine(".NAME Fixed");
                    uniqueNames.Add("fixed".ToUpper());
                    writer.WriteLine(".STATUS 8");
                    writer.WriteLine(".ENDBOOK");

                    FixedPoint.CreateWallsFile(FixedPoints,folderName + "/" + "fixed.srv");
                }
                ShotRenamer shotRenamer = new ShotRenamer(this.RootFolder,8);
                RootFolder.WriteToWalls(folderName,writer,uniqueNames,shotRenamer);

                writer.WriteLine(".ENDBOOK");
                writer.Close();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }


        // https://fountainware.com/compass/HTML_Help/Compass_Editor/surveyfileformat.htm
        public async Task<string> SaveToCompass(string folderName)
        {
            // SEF isn't a great it's really not a viable option for interchange.
            // Also, compass isn't smart enough to calculate forward references, so if a DAT file doesn't tie in
            // to the one above it, Compass can't handle it. I know, it's absolutely moronic. But there you have it.
            this.ProgressList.Add("Generating Compass project in:" + folderName);
            await Task.Delay(1);
            this.ShowSave = false;
            try
            {
                // Compass has one level of folders. So we flatten the folder heirarachy, determine the folder order
                // and then do the export
                List<Folder> flatFolders = new List<Folder>();
                List<Folder> sortedFolders = new List<Folder>();
                RootFolder.RecursiveAddNonEmptyFolders(flatFolders);
                bool singleFolder = true;
                bool sorted = Folder.SortForCompass(flatFolders, this.FixedPoints, out sortedFolders);
                if (sortedFolders.Count > 1)
                {
                    string sortedText;
                    if (!sorted)
                    {
                        sortedText = "I was not able to find an order that worked. You will need to change the order of your .dat files by hand and maybe move some surveys around.\n\n";
                    }
                    else
                    {
                        sortedText = "I found an order that should work. However, there's still a small chance you'll have to change the order of things.\n\n";
                    }


                    singleFolder = System.Windows.MessageBox.Show(
                        "There are " + sortedFolders.Count + " folders in this project. I try to put each folder in a seperate compass .dat file to save as much of your organization as possible.\n\n" +
                        "Unfortunately, Compass requires that all surveys in a .dat must be referenced by a .dat file above. Otherwise you will get a compile error (shots with no tie in).\n\n"+
                        sortedText+
                        "Alternatively, I can write all the On Station folders into a single compass .dat file. You'll lose your On Station folder structure but it will work.\n\n" +
                        "YES (Recommended)=Put each On Station folder into a seperate .dat file\n"+
                        "NO=Merge all On Station folders into a single compass .dat file\n", 
                        "WARNING", MessageBoxButton.YesNo) == MessageBoxResult.No;
                }
                if ( singleFolder)
                {
                    sortedFolders = Folder.CollapseIntoSingleFolder(sortedFolders);
                    sortedFolders[0].FolderName = "Merged";
                }
                string sourceFileName = System.IO.Path.GetFileNameWithoutExtension(this.FileName);
                StreamWriter writer = File.CreateText(folderName + "/" + sourceFileName + ".mak");
                writer.WriteLine("/Compass Project Export from " + App.VersionAndContactInfo);
                writer.WriteLine();
                List<string> usedFolderNames = new List<string>();
                sortedFolders.ForEach(folder =>
                {
                    folder.MakeCompassName(usedFolderNames);
                    List<string> errors = folder.WriteToCompass(folderName);
                    if (errors.Count != 0)
                    {
                        errors.ForEach(error =>
                        {
                            this.ProgressList.Add(error);
                        });
                    }

                    // Add the fixed points. We put them on the first folder that references the station that the fixed point refers to
                    writer.Write("#" + folder.ExportFileName);
                    FixedPoints.ForEach(fixedPoint =>
                    {
                        if (!fixedPoint.Used && folder.ReferencesFixedPoint(fixedPoint))
                        {
                            fixedPoint.Used = true;
                            string fixedString = "," + fixedPoint.Name + "[M," + fixedPoint.X.ToString()+" " + fixedPoint.Y.ToString() +" "+ fixedPoint.Z.ToString() + "]";
                            writer.Write(fixedString);
                        }
                    });
                    writer.WriteLine(";");
                });

                FixedPoints.ForEach((f) =>
                {
                    if (!f.Used)
                    {
                        ProgressList.Add("ERROR: Fixed point not connected to any stations: " + f.Name);
                    }
                });
                await Task.Delay(1);    //update progrss.
                writer.Close();
            } catch (Exception e)
            {
                return e.Message;
            }
            return "";
        }

        // Unfortunately SEF files never met their promise. Both Compass and Walls lose bits of information like
        // folder names etc... Better to just write native data.
/*        public async Task<string> SaveToSEF(string fileName)
        {
            // https://www.fountainware.com/compass/Documents/SEF.txt

            this.ShowSave = false;
            this.ProgressList.Clear();
            this.ProgressList.Add("Generating SEF File " + fileName);
            await Task.Delay(1);
            try
            {
                StreamWriter rootWriter = File.CreateText(fileName);
                rootWriter.Write("On Station Data exported to SEF by On Station Export");
                rootWriter.Write("Questions or problems with the export? Contact tacovan @gmail.com");
                rootWriter.WriteLine();
                rootWriter.WriteLine("#dir ProjectRoot");
                if (this.FixedPoints.Count == 0)
                {
                    rootWriter.WriteLine("No fixed points");
                }
                else
                {
                    rootWriter.WriteLine("*** Fixed Points ***");
                    rootWriter.WriteLine("#dir FixedPoints");
                    rootWriter.WriteLine("#cpoint FixedPointList");
                    rootWriter.WriteLine("#units meters");
                    rootWriter.WriteLine("#elevunits meters");
                    rootWriter.WriteLine("#data station,east,north,elev");
                    FixedPoints.ForEach(fixedPoint =>
                    {
                        fixedPoint.WriteLine(rootWriter);
                    });
                    rootWriter.WriteLine("#endcpoint");
                    rootWriter.WriteLine("#up");
                }

                RootFolder.WriteSEF(rootWriter);
                rootWriter.Close();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
  */      


        Folder RootFolder;

        public async Task<string> ParseFile(string fileName)
        {
            this.ShowProgress = true;
            this.ShowBrowse = false;
            this.FileName = fileName;
            this.ProgressList.Add("Opening " + fileName);
            await Task.Delay(1);
            try
            {
                string[] lines = System.IO.File.ReadAllLines(fileName);
                this.ProgressList.Add("Read " + lines.Length + " lines");
                this.ShowSave = true;
                await Task.Delay(1);

                // Clear out proprietary data blocks
                List<Line> cleanedLines= RemoveProprietaryData( lines);

                // Read folders
                RootFolder = Folder.LoadRootFolderAndChildren(cleanedLines);
                int total = RootFolder.GetTotalSubfolders()+1;
                this.ProgressList.Add("Found "+total+" folders");
                await Task.Delay(1);

                // Process surveys from folders
                RootFolder.ProcessSurveys();
                int surveys = RootFolder.GetTotalSurveys();
                int shots = RootFolder.GetTotalShots();
                this.ProgressList.Add("Found " + surveys + " surveys containing "+shots+" shots");
                await Task.Delay(1);


                // Fixed Points
                FixedPoints=FindFixedPoints(cleanedLines);
                this.ProgressList.Add("Found " + FixedPoints.Count+ " fixed points");


                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        List<FixedPoint> FixedPoints;

        List<FixedPoint> FindFixedPoints(List<Line> lines)
        {
            List<FixedPoint> pointList = new List<FixedPoint>();
            for (int i=0;i<lines.Count;i++)
            {
                if ( lines[i].TokenValue("Begin","Constrained Stations"))
                {
                    i++;
                    while (!lines[i].TokenValue("End", "Constrained Stations"))
                    {
                        var point=new FixedPoint();
                        i=point.Read(lines, i);
                        pointList.Add( point);
                        i++;
                    }
                }
            }
            return pointList;
        }
        List<Line> RemoveProprietaryData(string[] lines)
        {
            int BlocksRemoved = 0;
            List<Line> Cleaned = new List<Line>();
            for (int i = 0; i < lines.Length; i++)
            {
                Line line = ReadLine(lines[i],i);
                if (line.Token == "ProprietaryStart")
                {
                    // Skip lines until we find the end of the proprietary block
                    i++;
                    BlocksRemoved++;
                    Line skipline = ReadLine(lines[i], i);
                    while (skipline.Token != "ProprietaryEnd" && skipline.ValueS != line.ValueS)
                    {
                        i++;
                        skipline = ReadLine(lines[i],i);
                    }
                }
                else
                {
                    Cleaned.Add(line);
                }
            }
            this.ProgressList.Add("Removed " + BlocksRemoved + " OnStation-specific data blocks (things like color schemes and column widths)");
            return Cleaned;
        }

        Line ReadLine(string line,int index)
        {
            return new Line(line,index);
        }

        string _fileName="";
        public string FileName
        {
            get { return _fileName; }
            set { _fileName = value; NotifyPropertyChanged(); }
        }

        bool _showBrowse = true;
        public bool ShowBrowse
        {
            get { return _showBrowse; }
            set
            {
                _showBrowse = value;
                NotifyPropertyChanged();
            }
        }

        bool _showProgress = false;
        public bool ShowProgress
        {
            get { return _showProgress; }
            set
            {
                _showProgress = value;
                NotifyPropertyChanged();
            }
        }

        bool _showSave = false;
        public bool ShowSave
        {
            get { return _showSave; }
            set
            {
                _showSave = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<string> ProgressList
        {
            get;
        } = new ObservableCollection<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Reset()
        {
            ShowBrowse = true;
            ShowSave = false;
            ShowProgress = false;
        }
    }
}
