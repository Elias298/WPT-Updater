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

        
        //Console.WriteLine(string.Join(Environment.NewLine,ProgramsClass.instances));
        AppData.InitializeDatabase();
        ProgramsClass.AddPrograms(KeyStuff.GetInstalledProgramSubkeys());
        Console.WriteLine("Done!");


    }
}
