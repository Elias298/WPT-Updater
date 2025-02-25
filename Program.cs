using System;
using System.Configuration;
using System.Data.SQLite;
using System.Threading.Tasks.Sources;

namespace WPT_Updater;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static async Task Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());


        //await Launch.Start();
        //WebScraping programWeb = await WebScraping.InitializeLinks("7zip");

        //string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Docker Desktop";
        //ProgramsClass program = ProgramsClass.ProgramsDict[key];
        //program.EditProgramInfo(programName : "lol");

        /*foreach (var programe in ProgramsClass.ProgramsDict)
        {
            Console.WriteLine(programe.Value);
        }*/

        
        /*string testUrl = "https://visualstudio.microsoft.com/downloads/";
        List<string> links = new();
        links.Add(testUrl);

        List<string> detectedVersions = await WebScraping.ScanForPagesVersions(links);

        Console.WriteLine("Potential Versions Found:");
        foreach (string version in detectedVersions)
        {
            Console.WriteLine(version);
        }*/
        

        Console.WriteLine("Done!");


    }
}
