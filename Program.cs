using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.SQLite;
using System.Threading.Tasks.Sources;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
namespace WPT_Updater;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        int numLinks = 5; // Number of links to extract
        string searchQuery = "7zip official page";

        ChromeOptions options = new ChromeOptions();
        //options.AddArgument("--headless"); // Run in headless mode (remove if you want to see the browser)
        options.AddArgument($"--user-data-dir=C:\\Users\\{Auth.UserName}\\AppData\\Local\\Google\\Chrome\\User Data");
        options.AddArgument($"--profile-directory=Profile {Auth.ProfileNumber}");

        using IWebDriver driver = new ChromeDriver(options);
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(4));

        try
        {
            driver.Navigate().GoToUrl("https://www.google.com");

            // Accept cookies if prompted
            await AcceptCookiesIfNeeded(driver, wait);

            // Find the search box and enter the query
            IWebElement searchBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Name("q")));
            searchBox.SendKeys(searchQuery);
            searchBox.SendKeys(OpenQA.Selenium.Keys.Enter);

            // Wait for results to load
            await Task.Delay(1000); // Small wait for results to stabilize

            // Get search result links
            IReadOnlyCollection<IWebElement> searchResults = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("h3")));

            List<string> links = new List<string>();
            foreach (var result in searchResults.Take(numLinks))
            {
                try
                {
                    IWebElement parent = result.FindElement(By.XPath("./ancestor::a"));
                    string url = parent.GetAttribute("href");
                    if (!string.IsNullOrEmpty(url))
                    {
                        links.Add(url);
                    }
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }

            // Print extracted links
            Console.WriteLine($"Top {numLinks} Search Results:");
            foreach (var link in links)
            {
                Console.WriteLine(link);
            }
        }
        finally
        {
            driver.Quit();
        }
    }

    static async Task AcceptCookiesIfNeeded(IWebDriver driver, WebDriverWait wait)
    {
        try
        {
            var acceptButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[aria-label='Accept all']")));
            acceptButton.Click();
            await Task.Delay(500); // Allow time for cookie modal to close
        }
        catch (WebDriverTimeoutException)
        {
            // No cookie prompt appeared, continue
        }
    }



}
