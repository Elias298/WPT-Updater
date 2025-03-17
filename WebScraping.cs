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
using OpenQA.Selenium.Support.UI;
using static OpenQA.Selenium.BiDi.Modules.Network.UrlPattern;
using System.Diagnostics;
using System.ComponentModel.Design.Serialization;
using System.Collections;


namespace WPT_Updater;

internal class WebScraping
{
    //attributes from ProgramsClass
    public string? ProgramName { get; set; }
    public string? InstalledVersion { get; set; } = "";

    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }
    public int? CheckBetas { get; set; }
    public WebScraping(ProgramsClass program)
    {

        ProgramName = Regex.Replace(program.ProgramName, @"\b\d+(\.\d+)+\b", "");
        InstalledVersion = program.InstalledVersion;
        LatestVersion = program.LatestVersion;
        OfficialPage = program.OfficialPage;
        VersionPage = program.VersionPage;
        DownloadPage = program.DownloadPage;
        DownloadLink = program.DownloadLink;
        CheckBetas = program.CheckBetas;
    }

    public static ChromeOptions Seloptions()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument($"--user-data-dir=C:\\Users\\{Auth.UserName}\\AppData\\Local\\Google\\Chrome\\User Data");
        options.AddArgument($"--profile-directory=Profile {Auth.ProfileNumber}");
        return options;

    }

    public async Task CheckVersion()
    {
        List<string> Urls = new List<string>();
        if (string.IsNullOrEmpty(OfficialPage)) { await this.GetOfficialPage(); }
        if (!string.IsNullOrEmpty(OfficialPage)) { Urls.Add(OfficialPage); }

        if (string.IsNullOrEmpty(DownloadPage)) { await this.GetDownloadPage(); }
        if (!string.IsNullOrEmpty(DownloadPage)) { Urls.Add(DownloadPage); }

        if (string.IsNullOrEmpty(VersionPage)) { Urls.AddRange(await FirstWebResults($"{ProgramName} latest version", 3)); }
        else { Urls.Add(VersionPage); }

        List<List<string>> pagesversions = new();
        foreach(string url in Urls)
        {
            var urlsource = await GetPageSource(url);
            pagesversions.Add(ScanForVersions(urlsource));
        }
        
        List<string> allversions = pagesversions.SelectMany(sublist => sublist).ToList();
        Dictionary<string, int> points = allversions.Distinct().ToDictionary(x => x, x => 0);
        //Console.WriteLine(string.Join(" ", points.Keys.ToList()));

        Func<string, int> GetPoints = version => version.Count(c => c == '.');

        if (!string.IsNullOrEmpty(InstalledVersion))
        {
            int Format = GetPoints(InstalledVersion);

            foreach (string version in points.Keys.ToList())
            {
                int format = GetPoints(version);
                if (format == Format)
                {
                    points[version] += 20;
                }
                else if (Math.Abs(format-Format)==1)
                {
                    points[version] += 10;
                }
                else
                {
                    points.Remove(version);
                }
            }
            

        }


        //get list of top 3 most recurring items with their count, use top3[i].Value and top3[i].Count
        /* var top3 = allversions
            .GroupBy(n => n)                   
            .OrderByDescending(g => g.Count())
            .Take(3)                            
            .Select(g => new { Value = g.Key, Count = g.Count() })
            .ToList();

        points[top3[0].Value] += 15;
        points[top3[1].Value] += 10;
        points[top3[2].Value] += 5;*/


        string latestversion = points.OrderByDescending(kv => kv.Value).First().Key;
        this.LatestVersion = latestversion;
    }

    public async Task GetOfficialPage()
    {
        List<string> officialpage = await FirstWebResults($"{ProgramName} official website", 1);
        this.OfficialPage = officialpage[0];
    }

    public async Task GetDownloadPage()
    {
        List<string> downloadpage = await FirstWebResults($"{ProgramName} official download", 1);
        this.DownloadPage = downloadpage[0];
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




    public static async Task<List<string>> FirstWebResults(string searchQuery, int n)
    {
        IWebDriver driver = new ChromeDriver(Seloptions());

        try
        {
            // Navigate to Google
            driver.Navigate().GoToUrl("https://www.google.com");

            // Find the search box and enter the query
            IWebElement searchBox = driver.FindElement(By.Name("q"));
            searchBox.SendKeys(searchQuery);
            searchBox.Submit();

            // Wait for the results to load asynchronously
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            await Task.Run(() => wait.Until(d => d.FindElement(By.CssSelector("div.g"))));

            // Find all search result links (excluding ads)
            var searchResults = driver.FindElements(By.CssSelector("div.g:not(.uEierd):not(.commercial-unit-desktop-top) a")); // Exclude ads

            // Extract the first n links (ignoring ads)
            List<string> links = new List<string>();
            int count = 0;
            foreach (var result in searchResults)
            {
                string href = result.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && !href.Contains("googleadservices.com")) // Additional ad filtering
                {
                    links.Add(href);
                    count++;
                    if (count >= n) break; // Stop after collecting n links
                }
            }

            return links;
        }
        finally
        {
            // Close the browser
            driver.Quit();
        }
    }



    public static List<string> ScanForVersions(string source)
    {

        List<string> versions = new();
        Regex versionRegex = new(@"\b\d+(\.\d+)+\b");

        foreach (Match match in versionRegex.Matches(source))
        {
            versions.Add(match.Value);
        }
        return versions;
        
    }


    public static async Task<string> GetPageSourceSel(string url)
    {
        using (IWebDriver driver = new ChromeDriver(Seloptions()))
        {
            driver.Navigate().GoToUrl(url);
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            await Task.Run(() => wait.Until(d => d.FindElement(By.TagName("body"))));
            string pagesource = driver.PageSource;
            return pagesource;
        }
    }
    public static async Task<string> GetPageSourceHC(string url)
    {
        using HttpClient client = new();
        {
            string pagesource = await client.GetStringAsync(url);
            return pagesource;
        }
        
    }

    public static async Task<string> GetPageSource(string url)
    {
        try
        {
            string pagesource = await GetPageSourceHC(url);
            Log.WriteLine($"HttpClient successfully fetched source of URL: {url}");
            return pagesource;

        }
        catch
        {
            Log.WriteLine($"HttpClient failed to fetch source of URL: {url}");
            try
            {
                string pagesource = await GetPageSourceSel(url);
                Log.WriteLine($"Selenium successfully fetched source of URL: {url}");
                return pagesource;
            }
            catch
            {
                Log.WriteLine($"Selenium failed to fetch source of URL: {url}");
                Log.WriteLine($"unable to find pagesource of URL: {url}");
                return "";
            }
        }
    }



}