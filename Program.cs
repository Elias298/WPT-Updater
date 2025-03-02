using System;
using System.Configuration;
using System.Data.SQLite;
using System.Threading.Tasks.Sources;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace WPT_Updater;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        await Task.Delay(1);
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());


        //await Launch.Start();

        Auth.SetProfileNumber();

        await WebScraping.SelGotoPage();

    }
}
