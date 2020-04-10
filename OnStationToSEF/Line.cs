using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class Line
    {
        public Line(string raw,int lineNumber)
        {
            int index=raw.IndexOf('=');
            Token = raw.Substring(0, index );
            ValueS = raw.Substring(index + 1);
            LineNumber = lineNumber + 1;
        }
        public string Token 
        {
            get;
            set;
        }
        public int LineNumber { get; set; }

        public bool TokenValue(string t,string v)
        {
            return ValueS == v && Token == t;
        }

        public string ValueS
        {
            get;
            set;
        }

        // Format of a comment is Comment=Station,:
        public string CommentStation
        {
            get
            {
                int index = ValueS.IndexOf(',');
                return ValueS.Substring(0,index);
            }
        }
        public string CommentText
        {
            get
            {
                int index = ValueS.IndexOf(',');
                string text=ValueS.Substring(index+1);
                if (text.Length>0 && text[0]==':')
                {
                    return text.Substring(1);   // Strip of : if it exists
                }
                return text;
            }
        }
        public double ValueDouble
        {
            get
            {
                return double.Parse(ValueS);
            }
        }

        public int ValueInt
        {
            get
            {
                return int.Parse(ValueS);
            }
        }

        public static double ParseDOrNan(string v)
        {
            if ( v=="NAN")
            {
                return double.NaN;
            }
            return double.Parse(v);
        }
    }
}
