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
        await Task.Delay(1);
    }
}
