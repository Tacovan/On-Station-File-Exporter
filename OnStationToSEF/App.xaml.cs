using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OnStationExporter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string VersionAndContactInfo
        {
            get
            {
                return "On Station Exporter Version 1.0 by Taco van Ieperen (tacovan@gmail.com).";
            }
        }
    }
}
