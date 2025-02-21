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


        await Launch.Start();

        //string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Docker Desktop";
        //ProgramsClass program = ProgramsClass.ProgramsDict[key];
        //program.EditProgramInfo(programName : "lol");

        /*foreach (var programe in ProgramsClass.ProgramsDict)
        {
            Console.WriteLine(programe.Value);
        }*/

        Console.WriteLine("Done!");


    }
}
