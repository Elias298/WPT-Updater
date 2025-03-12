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

        var program = new ProgramsClass()
        {
            ProgramKey = "key",
            ProgramName = "name",
            DownloadLink = "https://ash-speed.hetzner.com/100MB.bin"
        };


        var program2 = new ProgramsClass()
        {
            ProgramKey = "key",
            ProgramName = "name",
            DownloadLink = "https://7-zip.org/a/7z2409-x64.exe"
        };
        var fw = new Download(program);
        var fw2 = new Download(program2);
        await fw2.Start();
        Console.WriteLine("lol");
        await fw.Start();
        Console.WriteLine("lol2");
    }
}
