using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Comment
    {
        public string Station { get; set; }
        public string CommentText { get; set; }
        public int LineNumber;
        public Comment(Line line)
        {
            Station = line.CommentStation;
            CommentText = line.CommentText;
            LineNumber = line.LineNumber;
        }

        public bool Used
        {
            get; set;
        } = false;

        public void WriteLine(StreamWriter writer)
        {
            writer.WriteLine(Station + ": " + CommentText);
        }

        public void WriteToWalls(StreamWriter svxFile)
        {
            svxFile.WriteLine("; " +Station + ": " + CommentText);
        }
    }
}
