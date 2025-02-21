using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPT_Updater;

internal class Launch
{

    public static async Task Start()
    {
        bool firstrundone;
        bool.TryParse(ConfigurationManager.AppSettings["Firstrundone"], out firstrundone);
        Console.WriteLine($"{firstrundone}");

        // 1st use initialization:
        if (!firstrundone) { await DoFirstTimeStuff(); }
        //average use
        else { await ProgramsClass.RefreshPrograms(ProgramsClass.ProgramsDict.Keys.ToList());}
    }


    public static async Task DoFirstTimeStuff()
    {
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["Firstrundone"].Value = "true";
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");

        AppData.InitializeDatabase();
        await ProgramsClass.AddPrograms(KeyStuff.GetInstalledProgramSubkeys());    

    }

    

}
