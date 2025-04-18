using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WPT_Updater;

internal class Launch
{

    public static async Task Start()
    {
        Log.WriteLine("WPT-Updater started");
        Log.WriteLine("Checking if first run");
        bool firstrundone;
        bool.TryParse(ConfigurationManager.AppSettings["Firstrundone"], out firstrundone);


        // 1st use initialization:
        if (!firstrundone)
        {
            Log.WriteLine("First run, starting initialization");
            await DoFirstTimeStuff();
        }
        else 
        {
            Log.WriteLine("Not first run");
        }


        List<ProgramsClass> AddedPrograms = ProgramsClass.dbhelper.GetAllPrograms();
        var installed_programs = KeyStuff.GetInstalledProgramSubkeys();

        /*foreach(ProgramsClass program in AddedPrograms)
        {
            if (!installed_programs.Contains(program.ProgramKey))
            {
                await program.RemoveProgram();
                AddedPrograms.Remove(program);
            }
        }*/
        
        foreach(var process in Process.GetProcessesByName("chrome"))
        {
            process.Kill();
        }

        await ProgramsClass.RefreshLocals(AddedPrograms);
        await ProgramsClass.CheckLatestVersions(AddedPrograms);
        //await ProgramsClass.FetchUpdates(AddedPrograms);

    }


    public static async Task DoFirstTimeStuff()
    {
        await Task.Delay(1);
        Log.WriteLine("Opening config");
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["Firstrundone"].Value = "true";
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");
        Log.WriteLine("First run marked as done");
        //await Installer2.SetDownloadPath();
        //await Auth.SetProfileNumber();     


    }

    

}
