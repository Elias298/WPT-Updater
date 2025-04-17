using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPT_Updater
{
    public class GridClass
    {
        public required string ProgramKey { get; set; }
        public required string ProgramName { get; set; }
        public string? InstalledVersion { get; set; } = "";
        public string? InstallDate { get; set; }

        //require web search attributes
        public string? LatestVersion { get; set; }
        public string? OfficialPage { get; set; }
        public string? VersionPage { get; set; }
        public string? DownloadPage { get; set; }
        public string? DownloadLink { get; set; }

        //authentication attributes
        public int? CheckBetas { get; set; }
    }
}
