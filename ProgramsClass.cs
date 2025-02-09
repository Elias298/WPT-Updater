using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;

namespace WPT_Updater;



internal class ProgramsClass
{

    //local attributes
    public string? ProgramName { get; set; }          
    public string? InstalledVersion { get; set; } 
    public string? InstallDate { get; set; }        
    public string? IconPath { get; set; }
    public bool Hidden = false;
    public bool authentication = false;

    //require web search attributes
    public string? LatestVersion { get; set; }
    public string? OfficialPage {get; set;}
    public string? VersionPage {get; set;}
    public string? DownloadPage {get; set;}
    public string? DownloadLink {get; set;}

    public static List<ProgramsClass> instances = new List<ProgramsClass>();


    // Override ToString for easy debugging
    public override string ToString()
    {
        return $"Program: {ProgramName}, Installed Version: {InstalledVersion} Installed On: {InstallDate} \n\n";
    }


    // Method to get installed programs from the registry
    public static void GetInstalledPrograms()
    {

        // Registry paths where installed programs are listed
        string[] registryPaths = {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var registryPath in registryPaths)
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryPath))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        using (RegistryKey? subKey = key.OpenSubKey(subKeyName))
                        {
                            // Get program details
                            string? programName = subKey.GetValue("DisplayName") as string;
                            string? installedVersion = subKey.GetValue("DisplayVersion") as string;
                            string? installDateString = subKey.GetValue("InstallDate") as string;
                            if (string.IsNullOrEmpty(installDateString)){
                                installDateString = "--------";
                            }
                                // Only add programs with a valid name
                                if (!string.IsNullOrEmpty(programName)) 
                                {
                                var program = new ProgramsClass()
                                {
                                    ProgramName = programName,
                                    InstalledVersion = installedVersion,
                                    InstallDate= installDateString[0..4] + "/" + installDateString[4..6] + "/" + installDateString[6..8]

                                };
                                instances.Add(program);
                                }
                        }
                    }
                }
            }
        }
    }









}
