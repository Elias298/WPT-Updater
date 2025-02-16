using System;
using System.Data.SQLite;
using System.Threading.Tasks.Sources;

namespace WPT_Updater;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());

        //AppData.InitializeDatabase();
        //ProgramsClass.AddPrograms(KeyStuff.GetInstalledProgramSubkeys());


        string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Notepad++";
        ProgramsClass program = ProgramsClass.ProgramsDict[key];
        program.EditProgramInfo(programName : "niggers");


        /*foreach (var program in ProgramsClass.ProgramsDict)
        {
            Console.WriteLine(program.Value);
        }*/
        Console.WriteLine("Done!");


    }
}
