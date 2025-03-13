using System;
using System.ComponentModel;
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
        /*foreach (var program in ProgramsClass.ProgramsDict.Values.ToList())
        {
            Console.WriteLine(program);
        }*/

        //Download download = new()

        var program1 = new ProgramsClass()
        {
            ProgramKey = "key",
            ProgramName = "name",
            DownloadLink = "https://www.win-rar.com/fileadmin/winrar-versions/winrar/winrar-x64-710tr.exe"
        };

        var program2 = new ProgramsClass()
        {
            ProgramKey = "key",
            ProgramName = "name",
            DownloadLink = "https://7-zip.org/a/7z2409-x64.exe"
        };

        var fw1 = new Download(program1);
        var fw2 = new Download(program2);


        // Start the download...
        await fw1.Start();
        await fw2.Start();

        // Simulate pause...
        Thread.Sleep(500);
        fw1.Pause();
        fw2.Pause();
        Thread.Sleep(2000);

        // Start the download from where we left, and when done print to console.
        await fw1.Start().ContinueWith(t => Console.WriteLine("Done"));
        await fw2.Start().ContinueWith(t => Console.WriteLine("Done"));
        Console.ReadKey();
    }
}
