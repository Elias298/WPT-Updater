using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using HtmlAgilityPack;

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
using static OpenQA.Selenium.BiDi.Modules.BrowsingContext.InnerTextLocator;
using OpenQA.Selenium.BiDi.Modules.Script;
using OpenQA.Selenium.BiDi.Modules.Input;


namespace WPT_Updater;

internal class WebScraping
{
    //attributes from ProgramsClass
    public string? ProgramName { get; set; }
    public string InstalledVersionstr { get; set; }

    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }
    public int? CheckBetas { get; set; }
    public WebScraping(ProgramsClass program)
    {
        InstalledVersionstr = program.InstalledVersion;
        ProgramName = program.ProgramName;
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
        //Gather all useful links
        List<string> Urls = new List<string>();
        if (string.IsNullOrEmpty(OfficialPage)) { await this.GetOfficialPage(); }
        if (!string.IsNullOrEmpty(OfficialPage)) { Urls.Add(OfficialPage); }

        if (string.IsNullOrEmpty(DownloadPage)) { await this.GetDownloadPage(); }
        if (!string.IsNullOrEmpty(DownloadPage)) { Urls.Add(DownloadPage); }

        if (string.IsNullOrEmpty(VersionPage)) { Urls.AddRange(await FirstWebResults($"{ProgramName} latest version", 3)); }
        else { Urls.Add(VersionPage); }

        Urls = Urls.Distinct().ToList();

        //Beta/alpha keywords:
        List<string> BetaKeywords = new List<string>
        {"alpha","beta","rc","pre-","pre release","-b","b-",
        "preview","test","testing","unstable","experimental",
        "early access","canary","nightly","snapshot","debug",
        "draft","staging","revision","rev","demo","test","night"};


        int GetFormat(Version version)
        {
            if (version.Revision != -1) { return 4; }
            else if(version.Build != -1){ return 3; }
            else { return 2; }
        }

        string GetOwnText(HtmlNode node)
        {
            return string.Concat(node.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Text)
                .Select(n => n.InnerText));
        }
        bool TryGetRegex(string node, Regex regex, out string? ver)
        {
            ver = null;
            var match = regex.Match(node);
            if (!match.Success) { return false; }

            var next = match.NextMatch();
            if (!next.Success)
            {
                ver = match.ToString();
                return true;
            }
            return false;
        }



        //Regex regex = CreateRegex(InstalledVersionstr);
        Regex regex = new Regex(@"(?<!\.)\d+(?:\.\d+){1,3}(?!\.)");

        int totalmatches = 0;
        Dictionary<Version,(bool,float,string)> results = new();
        Version InstalledVersion = Version.Parse(InstalledVersionstr);
        string? firstPage = null;

        foreach (string url in Urls)
        {
            var source = await GetPageSource(url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source);

            foreach (var node in doc.DocumentNode.Descendants())
            {

                if (node.NodeType != HtmlNodeType.Element)
                    continue;
                string ownText = GetOwnText(node);

                if (TryGetRegex(ownText, regex, out string? a))
                {
                    
                    totalmatches += 1;
                    string ver = a!;
                    Version version = Version.Parse(ver);
                    if (version <= InstalledVersion || version.Major>=InstalledVersion.Major*10)
                    {
                        if ((version == InstalledVersion) && firstPage==null) {firstPage = url;}
                        continue;
                    }

                    string regexstring = regex.ToString();
                    var specificwords = new List<string> { regexstring+"b", "b"+regexstring, regexstring + "a", "a" + regexstring };
                    var betakeywords = BetaKeywords;
                    betakeywords.AddRange(specificwords);

                    bool containsbeta = betakeywords.Any(s => ownText.Contains(s));
                    bool beta = false;
                    if (containsbeta) {beta = true;}

                    if (results.ContainsKey(version))
                    {
                        bool currentbeta = (results[version]).Item1;
                        var currentcount = (results[version]).Item2;
                        string website = (results[version]).Item3;

                        if (beta) { results[version]=( true,currentcount+1, website ); }
                        else { results[version] = ( currentbeta,currentcount+1, website ); }
                    }
                    else
                    {
                        results[version] = (beta , 0, url);
                    }
                }
            }

        }

        if (CheckBetas == 0)
        {
            foreach (Version ver in results.Keys.ToList())
            {
                if (results[ver].Item1) { results.Remove(ver); }
            }
        }
        

        if (results.Count == 0)
        {
            this.LatestVersion = InstalledVersion.ToString();
            this.VersionPage = firstPage;
            return;
        }

        List < Version > versionstrie = results.Keys.ToList();
        versionstrie.Sort();

        float matchesratio = 100 / totalmatches;
        float i = totalmatches;
        foreach (Version version in versionstrie)
        {
            i = i / 2;
            bool beta = results[version].Item1;
            float count = results[version].Item2;
            string website = results[version].Item3;
            if (GetFormat(version) != GetFormat(InstalledVersion)){ count = count / 2; } //remove points if format doesnt match

            //final points
            results[version] = (beta, (count * matchesratio)*i, website);
        }

        var maxItem = results.MaxBy(kv => kv.Value.Item2);
        Version max = maxItem.Key;
        string latestversion = max.ToString();
        this.LatestVersion = latestversion;
        this.VersionPage = results[max].Item3;
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
                foreach (System.Text.RegularExpressions.Match match in versionedPattern.Matches(pageSource))
                {
                    downloadLinks.Add(match.Value);
                }
            }

            // If no versioned links were found, fall back to general download pattern
            if (!downloadLinks.Any())
            {
                Regex fallbackPattern = new Regex(@"https:\/\/[^\s\'\""]+\.(exe|zip|dmg)", RegexOptions.IgnoreCase);
                foreach (System.Text.RegularExpressions.Match match in fallbackPattern.Matches(pageSource))
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
                string? href = result.GetAttribute("href");
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
