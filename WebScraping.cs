using System;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace WPT_Updater
{
    internal class WebScraping
    {
        public List<string>? Urls { get; set; }

        public Task<List<string>>? VersionsfromPages;

        //attributes from ProgramsClass
        public string? ProgramName { get; set; }
        public string? InstalledVersion { get; set; }
        public string? LatestVersion { get; set; }
        public string? OfficialPage { get; set; }
        public string? VersionPage { get; set; }
        public string? DownloadPage { get; set; }
        public string? DownloadLink { get; set; }


        public WebScraping(ProgramsClass program) 
        {
            ProgramName = program.ProgramName;
            InstalledVersion = program.InstalledVersion;

            LatestVersion = program.LatestVersion;
            OfficialPage = program.OfficialPage;
            VersionPage = program.VersionPage;
            DownloadLink = program.DownloadLink;

        }

        //initialize links for 1 app
        public async Task InitializeLinks()
        {
            await Task.Delay(1);
            LatestVersion = null;
            OfficialPage = null;
            VersionPage = null;
            DownloadLink = null;
            //to be implemented later
        }



        public async Task<List<string>> FindUrls() 
        {
            await Task.Delay(1);
            List<string> lol = new();
            return lol; 
        }



        public static async Task<List<string>> ScanForPagesVersions(List<string> urls)
        {

            List<string> versions = new();
            Regex versionRegex = new(@"\b\d+(\.\d+)+\b");
            using HttpClient client = new();
            if (urls!=null)
            {
                foreach (string url in urls)
                {
                    try
                    {
                        string pageContent = await client.GetStringAsync(url);

                        foreach (Match match in versionRegex.Matches(pageContent))
                        {
                            versions.Add(match.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching page: {ex.Message}");
                    }
                }
            }
            return versions;
        }
    }
}
