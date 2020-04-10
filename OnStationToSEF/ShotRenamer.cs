using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnStationExporter
{
    public class ShotRenamer
    {
        int Length;
        Dictionary<string, string> renamed=new Dictionary<string, string>();
        public ShotRenamer(Folder folder,int length)
        {
            Length = length;
            RecursiveFindNames(folder);
        }

        void RecursiveFindNames(Folder folder)
        {
            folder.SubFolders.ForEach(x => RecursiveFindNames(x));
            foreach (Survey s in folder.Surveys)
            {
                foreach (var shot in s.Shots)
                {
                    Add(shot.To);
                    Add(shot.From);
                }
            }
        }

        void Add(string name)
        {
            if (name.Length > Length && !renamed.ContainsKey(name))
            {
                string shortName = name.Substring(name.Length - 8, 8);
                int index = 1;
                while (renamed.Values.Contains(shortName))
                {
                    shortName = name.Substring(0, 5) + index.ToString();
                    index++;
                }
                // make a new name
                renamed[name] = shortName;
            }
        }

        public string GetShortName(string name)
        {
            if ( renamed.ContainsKey(name))
            {
                return renamed[name];
            }
            return name;
        }

    }

}
