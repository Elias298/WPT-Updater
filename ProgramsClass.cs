using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;
using System.Text.RegularExpressions;

namespace WPT_Updater;



internal class ProgramsClass
{

    //local attributes

    public required string ProgramKey { get; set; }
    public required string ProgramName { get; set; }
    public string? InstalledVersion { get; set; } = "";
    public string? InstallDate { get; set; }

    //require web search attributes
    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }

    //authentication attributes
    public int? CheckBetas { get; set; }
    public string? _password { get; set; }

    //bool attribute
    public int? Hidden { get; set; }


    public static AppData dbhelper = new AppData();
    public static Installer downloader = new Installer(10);


    // Override ToString for easy debugging
    public override string ToString()
    {
        return $"Program: {ProgramName}, Installed Version: {InstalledVersion} Installed On: {InstallDate}\nOfficial WebPage: {OfficialPage}, Download Page: {DownloadPage}, Latest Version: {LatestVersion}, DownloadLink: {DownloadLink}\n\n";
    }


    //method to add 1 program given it's subkey
    public static async Task AddProgram(string subkeyPath)
    {

        using RegistryKey? subKey = Registry.LocalMachine.OpenSubKey(subkeyPath);

        // Get program details
        var (programName, installedVersion, installDateString) = GetLocalInfo(subkeyPath);
        
        var program = new ProgramsClass()
        {
            ProgramKey = subkeyPath,
            ProgramName = Regex.Replace(programName, @"\b\d+(\.\d+)+\b", ""),
            InstalledVersion = installedVersion,
            InstallDate = installDateString,
            Hidden = 0,
            CheckBetas = 0
        };
        await dbhelper.SyncNewProgram(program);
        //sync with UI
        
    }

    public static (string,string,string) GetLocalInfo(string subkeyPath)
    {
        using RegistryKey? subKey = Registry.LocalMachine.OpenSubKey(subkeyPath);

        // Get program details
        string? programName = subKey?.GetValue("DisplayName") as string;
        if (string.IsNullOrEmpty(programName))
        {
            programName = "N.A";
        }
        string? installedVersion = subKey?.GetValue("DisplayVersion") as string;
        if (string.IsNullOrEmpty(installedVersion))
        {
            installedVersion = "??.?";
        }
        string? installDateString = subKey?.GetValue("InstallDate") as string;
        if (string.IsNullOrEmpty(installDateString))
        {
            installDateString = "--------";
        }
        installDateString = installDateString[0..4] + "/" + installDateString[4..6] + "/" + installDateString[6..8];

        return (programName, installedVersion, installDateString);
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
                              int? hidden = null,
                              int? checkbetas = null)
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
        if (checkbetas != null){ CheckBetas = checkbetas; }

        await dbhelper.SyncEditedInfo(ProgramKey);
        //sync with UI

    }


    public async Task RemoveProgram()
    {
        await dbhelper.SyncRemoveProgram(ProgramKey);
        //sync with UI
    }


    public string? downloadedfilestr()
    {
        return Directory.GetFiles(Installer2.DownloadPath)
            .FirstOrDefault(file => Path.GetFileName(file).Contains(ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1)));
    }

    public async Task RefreshLocal()
    {
        var (programname, installedversion, installdate) = GetLocalInfo(this.ProgramKey);
        await this.EditProgramInfo(programName: programname, installedVersion: installedversion, installDate: installdate);

        string? downloadfile = this.downloadedfilestr();

        if ((InstalledVersion == LatestVersion && downloadfile != null) )
        {
            File.Delete(downloadfile);

        }

    }

    public async Task RefreshLocals(List<string> keyslist)
    {
        var ProgramsDict = dbhelper.GetAllPrograms();
        foreach (string key in keyslist)
        {
            var program = ProgramsDict[key];
            if (program.Hidden == 0)
            {
                await program.RefreshLocal();
            }
        }
    }



    public async Task CheckLatestVersion()
    {
        var webprogram = new WebScraping(this);
        await webprogram.CheckVersion();
        await this.EditProgramInfo(latestVersion: webprogram.LatestVersion, versionPage:webprogram.VersionPage , officialPage: webprogram.OfficialPage, downloadPage: webprogram.DownloadPage);
    }


    public async Task FetchUpdate()
    {
        var webprogram = new WebScraping(this);
        await webprogram.FetchUpdate();
        await this.EditProgramInfo(downloadPage: webprogram.DownloadPage, downloadLink: webprogram.DownloadLink);
    }

    public async Task DownloadUpdate()
    {
        if (!string.IsNullOrEmpty(DownloadLink))
        {
            var filename = DownloadLink.Substring(DownloadLink.LastIndexOf('/') + 1);
            int dot = filename.IndexOf('.');
            if (dot != -1)
            {
                var path = Installer.DownloadPath + filename.Substring(0, dot) + $"_v{LatestVersion}_" + ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1) + filename.Substring(dot);
                await downloader.DownloadFileAsync(DownloadLink, path);
            }
            else
            {
                string ext = "bin"; //get file extension using metadata
                var path = Installer.DownloadPath + ProgramName + $"_v{LatestVersion}_" + ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1) + "." + ext;
                await downloader.DownloadFileAsync(DownloadLink, path);
            }
             
        }
    }


    public static async Task FetchUpdates(List<string> keyslist)
    {
        var ProgramsDict = dbhelper.GetAllPrograms();
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            var file = program.downloadedfilestr();
            if ( !string.IsNullOrEmpty(program.LatestVersion) && program.Hidden==0 &&((file == null && program.LatestVersion!=program.InstalledVersion) || (file != null && !file.Contains($"{program.LatestVersion}"))))
            {
                await program.FetchUpdate();
            }
        }
    }


    public static async Task CheckLatestVersions(List<string> keyslist)
    {
        var ProgramsDict = dbhelper.GetAllPrograms();
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            if (program.Hidden == 0)
            {
                await program.CheckLatestVersion();
            }
        }
    }

    public static async Task DownloadUpdates(List<string> keyslist)
    {
        var ProgramsDict = dbhelper.GetAllPrograms();
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            var file = program.downloadedfilestr();
            if (program.Hidden==0)
            {
                await program.DownloadUpdate();
            }
        }
    }

    public static async Task Removeprograms(List<string> keyslist)
    {
        var ProgramsDict = dbhelper.GetAllPrograms();
        foreach (string key in keyslist)
        {
            ProgramsClass program = ProgramsDict[key];
            await program.RemoveProgram();
        }
    }




    //method to add programs from a given keylist
    public static async Task AddPrograms(List<string> keyslist)
    {
        foreach (string key in keyslist)
        {
            await AddProgram(key);
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
