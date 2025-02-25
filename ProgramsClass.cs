using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;

namespace WPT_Updater;



internal class ProgramsClass
{

    //local attributes

    public required string ProgramKey { get; set; }
    public required string ProgramName { get; set; }
    public string? InstalledVersion { get; set; }
    public string? InstallDate { get; set; }

    //require web search attributes
    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }

    //authentication attributes
    public string? _username { get; set; }
    public string? _password { get; set; }

    //bool attribute
    public int? Hidden { get; set; }

    public static AppData dbhelper = new AppData();
    public static Dictionary<string, ProgramsClass> ProgramsDict = dbhelper.GetAllPrograms();


    // Override ToString for easy debugging
    public override string ToString()
    {
        return $"Program: {ProgramName}, Installed Version: {InstalledVersion} Installed On: {InstallDate} \n\n";
    }


    //method to add 1 program given it's subkey
    public static async Task GetProgramInfo(string subkeyPath)
    {

        using RegistryKey? subKey = Registry.LocalMachine.OpenSubKey(subkeyPath);

        // Get program details
        string? programName = subKey?.GetValue("DisplayName") as string;
        string? installedVersion = subKey?.GetValue("DisplayVersion") as string;
        string? installDateString = subKey?.GetValue("InstallDate") as string;
        if (string.IsNullOrEmpty(installDateString))
        {
            installDateString = "--------";
        }
        if (!string.IsNullOrEmpty(programName))
        {
            var program = new ProgramsClass()
            {
                ProgramKey = subkeyPath,
                ProgramName = programName,
                InstalledVersion = installedVersion,
                InstallDate = installDateString[0..4] + "/" + installDateString[4..6] + "/" + installDateString[6..8],
                Hidden = 0
            };
            ProgramsDict.Add(subkeyPath, program);
            await dbhelper.SyncNewProgram(program);
            //sync with UI
        }
    }

    

    //method to update an added program
    public async Task EditProgramInfo(
                              string? programName = null,
                              string? installedVersion = null,
                              string? installDate = null,
                              string? latestVersion = null,
                              string? officialPage = null,
                              string? versionPage = null,
                              string? downloadPage = null,
                              string? downloadLink = null,
                              int? hidden = null)
    {
        if (programName != null) { ProgramName = programName; }
        if (installedVersion != null) { InstalledVersion = installedVersion; }
        if (installDate != null) { InstallDate = installDate; }
        if (latestVersion != null) { LatestVersion = latestVersion; }
        if (officialPage != null) { OfficialPage = officialPage; }
        if (versionPage != null) { VersionPage = versionPage; }
        if (downloadPage != null) { DownloadPage = downloadPage; }
        if (downloadLink != null) { DownloadLink = downloadLink; }
        if (hidden != null) { Hidden = hidden; }

        await dbhelper.SyncEditedInfo(ProgramKey);
        //sync with UI

    }


    public async Task RemoveProgram()
    {
        await dbhelper.SyncRemoveProgram(ProgramKey);
        ProgramsDict.Remove(ProgramKey);
        //sync with UI
    }


    public async Task RefreshProgram()
    {
        await Task.Delay(1);
        //check if program links got updated
        //if yes grab new links and update program info

    }
    public async Task InitializeLinks() 
    {

        var programlinks = new WebScraping(this);
        await programlinks.InitializeLinks();

        await this.EditProgramInfo(latestVersion:programlinks.LatestVersion,
                                   officialPage:programlinks.OfficialPage,
                                   versionPage:programlinks.VersionPage,
                                   downloadPage:programlinks.DownloadPage,
                                   downloadLink:programlinks.DownloadLink);
    }


    public static async Task RefreshPrograms(List<string> keyslist)
    {
        foreach(string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            await program.RefreshProgram();
        }
    }

    public static async Task Removeprograms(List<string> keyslist)
    {
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            await program.RemoveProgram();
        }
    }

    public static async Task InitializeLinkss(List<string> keyslist)
    {
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            await program.InitializeLinks();
        }
    }


    //method to add programs from a given keylist
    public static async Task AddPrograms(List<string> keyslist)
    {
        foreach (string key in keyslist)
        {
            await GetProgramInfo(key);
        }
    }
}


internal class KeyStuff
{

    // Method to get all subkey names from the registry
    public static List<string> GetInstalledProgramSubkeys()
    {
        var subkeys = new List<string>();

        // Registry paths where installed programs are listed
        string[] registryPaths = [
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        ];

        foreach (var registryPath in registryPaths)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryPath);
            if (key != null)
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    subkeys.Add($"{registryPath}\\{subKeyName}");
                }
            }
        }
        return subkeys;
    }
}
