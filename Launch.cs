﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.DevTools.V131.Runtime;

namespace WPT_Updater;

internal class Launch
{

    public static async Task Start()
    {
        bool firstrundone;
        bool.TryParse(ConfigurationManager.AppSettings["Firstrundone"], out firstrundone);

        // 1st use initialization:
        if (!firstrundone) { await DoFirstTimeStuff(); }
        else 
        {
            
        }

        if (!File.Exists("Programs.db"))
        {
            AppData.InitializeDatabase();
        }

        //await ProgramsClass.CheckLatestVersions(ProgramsClass.ProgramsDict.Keys.ToList());

        //await ProgramsClass.FetchUpdates(ProgramsClass.OutdatedPrograms);


    }


    public static async Task DoFirstTimeStuff()
    {
        await Task.Delay(1);
        //mark first run as done
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["Firstrundone"].Value = "true";
        config.Save(ConfigurationSaveMode.Modified);
        ConfigurationManager.RefreshSection("appSettings");

        //await Auth.SetProfileNumber();     


    }

    

}
