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

        ProgramName = program.ProgramName;
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
        //options.AddArgument("--headless=new");
        options.AddArgument($"--user-data-dir=C:\\Users\\{Auth.UserName}\\AppData\\Local\\Google\\Chrome\\User Data");
        options.AddArgument($"--profile-directory=Profile {Auth.ProfileNumber}");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddExcludedArgument("enable-automation");
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
        foreach (string url in Urls)
        {
            var urlsource = await GetPageSource(url);
            pagesversions.Add(ScanForVersions(urlsource));
        }

        List<string> allversions = pagesversions.SelectMany(sublist => sublist).ToList();
        Dictionary<string, int> points = allversions.Distinct().ToDictionary(x => x, x => 0);

        Func<string, int> GetPoints = version => version.Count(c => c == '.');

        if (!string.IsNullOrEmpty(InstalledVersion))
        {
            // Compare Installed Version using Version class
            if (Version.TryParse(InstalledVersion, out Version installedVersionObj))
            {
                foreach (string version in points.Keys.ToList())
                {
                    if (Version.TryParse(version, out Version scrapedVersionObj))
                    {
                        // Compare versions directly using CompareTo()
                        if (scrapedVersionObj.CompareTo(installedVersionObj) > 0)
                        {
                            points[version] += 20;  // Newer versions get higher points
                        }
                        else if (scrapedVersionObj.CompareTo(installedVersionObj) == 0)
                        {
                            points[version] += 10;  // Same version gets moderate points
                        }
                        else
                        {
                            points[version] += 5;   // Older versions get the lowest points
                        }
                    }
                }
            }
        }

        // Sort versions using Version class to ensure proper ordering
        var sortedVersions = points.Keys
                                    .Select(version => new Version(version))
                                    .OrderByDescending(version => version)  // Sort by version, descending order (latest first)
                                    .Select(version => version.ToString())
                                    .ToList();

        string latestversion = sortedVersions.First();  // The latest version is the first item in the sorted list

        // Find version page as well this.VersionPage = ...
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

    public async Task GetDownloadLink()
{
    if (string.IsNullOrEmpty(VersionPage) && string.IsNullOrEmpty(DownloadPage))
    {
        Console.WriteLine("No version or download page available to fetch the download link.");
        return;
    }

    List<string> urlsToCheck = new List<string>();
    if (!string.IsNullOrEmpty(VersionPage)) urlsToCheck.Add(VersionPage);
    if (!string.IsNullOrEmpty(DownloadPage)) urlsToCheck.Add(DownloadPage);

    string latestDownloadLink = string.Empty;
    string? version = this.LatestVersion;

    foreach (string url in urlsToCheck)
    {
        string pageSource = await GetPageSource(url);
        if (!string.IsNullOrEmpty(pageSource))
        {
            var downloadLinks = new List<string>();

            if (!string.IsNullOrEmpty(version))
            {
                // Prioritize links that include the latest version in the filename
                Regex versionedPattern = new Regex($@"https:\/\/[^\s\'\""]*{Regex.Escape(version)}[^\s\'\""]*\.(exe|zip|dmg)", RegexOptions.IgnoreCase);
                foreach (Match match in versionedPattern.Matches(pageSource))
                {
                    downloadLinks.Add(match.Value);
                }
            }

            // If no versioned links were found, fall back to general download pattern
            if (!downloadLinks.Any())
            {
                Regex fallbackPattern = new Regex(@"https:\/\/[^\s\'\""]+\.(exe|zip|dmg)", RegexOptions.IgnoreCase);
                foreach (Match match in fallbackPattern.Matches(pageSource))
                {
                    downloadLinks.Add(match.Value);
                }
            }

            if (downloadLinks.Any())
            {
                latestDownloadLink = downloadLinks.First();
                break;
            }
        }
    }

    if (!string.IsNullOrEmpty(latestDownloadLink))
    {
        this.DownloadLink = latestDownloadLink;
        Console.WriteLine($"Download link for the latest version: {this.DownloadLink}");
    }
    else
    {
        Console.WriteLine("No download link found for the latest version.");
    }
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
            await Task.Run(() => wait.Until(d => d.FindElement(By.CssSelector("div.tF2Cxc"))));

            // Find all search result links (excluding ads)
            var searchResults = driver.FindElements(By.CssSelector("div.tF2Cxc a")); // Exclude ads

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


    public static async Task GPTdriver(string query)
    {
        await Task.Delay(0);
        using (IWebDriver driver = new ChromeDriver(Seloptions()))
        {
            driver.Navigate().GoToUrl("https://chatgpt.com/");

            IWebElement inputBox = driver.FindElement(By.ClassName("placeholder"));
            inputBox.SendKeys(query + OpenQA.Selenium.Keys.Enter);

            Thread.Sleep(10000);

            var messages = driver.FindElements(By.CssSelector("div.markdown"));
            string lastMessage = messages[^1].Text;
            Console.WriteLine("ChatGPT's Response: " + lastMessage);
        }

    }




}
