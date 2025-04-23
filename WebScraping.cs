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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;


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

        if (string.IsNullOrEmpty(DownloadPage)) { await this.GetDownloadPage(); }
        if (!string.IsNullOrEmpty(DownloadPage)) { Urls.Add(DownloadPage); }

        if (string.IsNullOrEmpty(OfficialPage)) { await this.GetOfficialPage(); }
        if (!string.IsNullOrEmpty(OfficialPage)) { Urls.Add(OfficialPage); }

        if (string.IsNullOrEmpty(VersionPage)) { Urls.AddRange(await FirstWebResults($"{ProgramName} latest version", 3)); }
        else { Urls.Add(VersionPage); }

        Urls = Urls.Distinct().ToList();

        //Beta/alpha keywords:
        List<string> BetaKeywords = new List<string>
        {"alpha","beta","rc","pre-","pre release","-b","b-",
        "preview","test","testing","unstable","experimental",
        "early access","canary","nightly","snapshot","debug",
        "draft","staging","revision","rev","demo","test","night"};

        //attributes for link checking
        var tagAttrMap = new Dictionary<string, string[]>
        {
            ["a"] = new[] { "href" },
            ["link"] = new[] { "href" },
            ["img"] = new[] { "src", "srcset" },
            ["script"] = new[] { "src" },
            ["iframe"] = new[] { "src" },
            ["source"] = new[] { "src", "srcset" },
            ["video"] = new[] { "src" },
            ["audio"] = new[] { "src" },
            ["embed"] = new[] { "src" },
            ["object"] = new[] { "data", "codebase" },
            ["form"] = new[] { "action" },
            ["track"] = new[] { "src" },
            ["meta"] = new[] { "content" },
            ["button"] = new[] { "onclick" },
            ["div"] = new[] { "onclick" },  // JS-based buttons
            ["span"] = new[] { "onclick" },
            ["body"] = new[] { "onload" },
        };



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



        //Version search setup
        Regex regex = new Regex(@"(?<!\.)\d+(?:\.\d+){1,3}(?!\.)");
        int totalmatches = 0;
        Dictionary<Version,(bool,float,string)> results = new();
        Version InstalledVersion = Version.Parse(InstalledVersionstr);
        string? firstPage = null;

        //Link searcg setup
        Regex linkregex = new Regex(@"https:\/\/[^\s""'<>]+|\/[^\s""'<>]*\.(exe|zip)");
        Dictionary<string, string> linkresults = new();


        foreach (string url in Urls)
        {
            var source = await GetPageSource(url);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source);

            Uri uri = new Uri(url);
            string baseurl = uri.GetLeftPart(UriPartial.Authority);


            //1st method to get links:
            if (linkresults.Count()==0)
            {
                foreach (var match in linkregex.Matches(source))
                {
                    string? found = match.ToString();
                    if (found == null) continue;
                    if (!found.Contains("https"))
                    {
                        found = baseurl + found;
                        linkresults[found] = url;
                    }
                    else
                    {
                        var o = found.ToLower();
                        string? ext = null;
                        if (o.Contains(".exe"))
                        {
                            ext = ".exe";
                        }
                        else if (o.Contains(".zip")) { ext = ".zip"; }
                        else if ((o.Contains("id=") && o.Contains("download"))) { linkresults[found] = url; }
                        if (ext != null && !o.EndsWith(ext))
                        {
                            int index = o.IndexOf(ext);
                            string result = found.Substring(0, index + 4);
                            linkresults[result] = url;
                        }
                        else if(ext!=null)
                        {
                            linkresults[found] = url;
                        }
                    }
                }
            }



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

        //adding the link to the program start
        foreach (string url in linkresults.Keys.ToList())
        {
            this.DownloadLink = url;
            this.DownloadPage = linkresults[url];
            Console.WriteLine($"{url} {linkresults[url]}");
            break;
        }
        //adding link to the program end

        if (CheckBetas == 0)
        {
            foreach (Version ver in results.Keys.ToList())
            {
                if (results[ver].Item1) { results.Remove(ver); }
            }
        }
        
        // attention pr check link
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
