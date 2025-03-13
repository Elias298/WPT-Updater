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

internal class Program
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

        
    }
}
