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
    static async Task Main()
    {
        await Task.Delay(1);
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.

        //ApplicationConfiguration.Initialize();
        //Application.Run(new Form1());


        //await Launch.Start();
        var sevenz = ProgramsClass.dbhelper.GetAllPrograms()["SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\7-Zip"];
        var downloadManager = new Installer(4);
        await downloadManager.DownloadFileAsync("https://www.7-zip.org/a/7z2409-x64.exe", """C:\Users\Elias\Desktop\lol\7z ip.exe""");
        await downloadManager.DownloadFileAsync("https://www.win-rar.com/fileadmin/winrar-versions/winrar/winrar-x64-710fr.exe", """C:\Users\Elias\Desktop\lol\winrar.exe""");
        //await downloadManager.DownloadFileAsync("https://javadl.oracle.com/webapps/download/AutoDL?BundleId=251654_7ed26d28139143f38c58992680c214a5", """C:\Users\Elias\Desktop\lol\java.exe""");
    }
}
