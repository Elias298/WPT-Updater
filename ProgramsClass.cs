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
    public string? ProgramName { get; set; }
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
    public static void GetProgramInfo(string subkeyPath)
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
        if (!string.IsNullOrEmpty(programName))//"if" will later become an "else"
        {
            var program = new ProgramsClass()
            {
                ProgramKey = subkeyPath,
                ProgramName = programName,
                InstalledVersion = installedVersion,
                InstallDate = installDateString[0..4] + "/" + installDateString[4..6] + "/" + installDateString[6..8],
                Hidden = 0
                //LatestVersion = WebScraping.GetVersion(programName),
                //OfficialPage = WebScraping.GetOfficialPage(programName),
                //VersionPage = WebScraping.GetVersionPage(programName),
                //DownloadPage = WebScraping.GetDownloadPage(programName),
                //DownloadLink = WebScraping.GetDownloadLink(programName)
            };
            ProgramsDict.Add(subkeyPath, program);
            dbhelper.SyncNewProgram(program);
            //sync with UI
        }
    }

    //method to update an added program
    public void EditProgramInfo(
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

        dbhelper.SyncEditedInfo(ProgramKey);
        //sync with UI

    }


    public void RemoveProgram()
    {
        dbhelper.SyncRemoveProgram(ProgramKey);
        ProgramsDict.Remove(ProgramKey);
        //sync with UI
    }


    //method to add programs from a given keylist
    public static void AddPrograms(List<string> keyslist)
    {
        foreach (string key in keyslist)
        {
            GetProgramInfo(key);
        }
    }









}

public class KeyStuff
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
