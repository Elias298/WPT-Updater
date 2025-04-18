using System;
using System.Configuration;
using System.Data.SQLite;
using System.Threading.Tasks.Sources;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
namespace WPT_Updater;

internal class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.


        /*List<string> programs = new();
        programs.Add("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Docker Desktop");
        programs.Add("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\WinRAR archiver");
        programs.Add("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1");
        programs.Add("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Notepad++");
        programs.Add("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7-Zip");
        awaitProgramsClass.AddPrograms(programs);*/

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());




    }
}
