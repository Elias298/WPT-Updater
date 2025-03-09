using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;


namespace WPT_Updater;

internal class WebScraping
{
    public List<string> Urls = new();

    public Task<List<string>>? VersionsfromPages;

    //attributes from ProgramsClass
    public string? ProgramName { get; set; }
    public string? InstalledVersion { get; set; }

    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }
    public int? CheckBetas { get; set; }


    public WebScraping(ProgramsClass program) 
    {
        ProgramName = program.ProgramName;
        InstalledVersion = program.InstalledVersion;

        LatestVersion = program.LatestVersion;
        OfficialPage = program.OfficialPage;
        VersionPage = program.VersionPage;
        DownloadPage = program.DownloadPage;
        DownloadLink = program.DownloadLink;
        CheckBetas = program.CheckBetas;
    }


    public async Task CheckVersion()
    {
        if (OfficialPage == null) { await this.GetOfficialPage(); }
        if (OfficialPage != null) { Urls.Add(OfficialPage); }

        if (OfficialPage == null) { await this.GetDownloadPage(); }
        if (DownloadPage != null) { Urls.Add(DownloadPage); }

        if (VersionPage == null) { Urls.AddRange(await FirstWebResults($"{ProgramName} latest version", 3)); }
        else { Urls.Add(VersionPage); }

        //Use Urls list and start version finding algorithm
        string foundversion = "##.##.#";

        

        this.LatestVersion = foundversion;
    }

    public async Task GetOfficialPage()
    {
        await Task.Delay(1);

        //code to find officialpage
        string officialpage = "https://.....";


        this.OfficialPage = officialpage;
    }

    public async Task GetDownloadPage()
    {
        await Task.Delay(1);

        //code to find officialpage
        string downloadpage = "https://.....";


        this.DownloadPage = downloadpage;
    }

    public async Task FetchUpdate()
    {
        await Task.Delay(1);

        //start from this.DownloadPage
        //if nothing found recursively check daughter pages or other "download pages" from google search
        //code
        string versionpage = "https://.....";
        string downloadlink = "https://.....";
        string downloadpage = "https://.....";


        this.VersionPage = versionpage;
        this.DownloadLink = downloadlink;
        this.DownloadPage = downloadpage;
    }




    public static async Task<List<string>> FirstWebResults(string websearch, int number) 
    {
        await Task.Delay(1);
        List<string> results = new();
        return results;

    }



    //The following 2 methods are just prototypes and shall be changed later
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


    public static async Task SelGotoPage()
    {
        var options = new ChromeOptions();
        //options.AddArgument("--window-position=-32000,-32000");
        options.AddArgument($"--user-data-dir=C:\\Users\\{Auth.UserName}\\AppData\\Local\\Google\\Chrome\\User Data");
        options.AddArgument($"--profile-directory=Profile {Auth.ProfileNumber}"); // Change to "Default" or your profile name
        using (IWebDriver driver = new ChromeDriver(options))
        {
            driver.Navigate().GoToUrl("https://www.mathworks.com/");
            await Task.Delay(10000);
            Console.WriteLine("Selenium is working!");
        }
    }


}