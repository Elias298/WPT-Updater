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
    public string InstalledVersion { get; set; } = "";
    public string? InstallDate { get; set; }

    //require web search attributes
    public string? LatestVersion { get; set; }
    public string? OfficialPage { get; set; }
    public string? VersionPage { get; set; }
    public string? DownloadPage { get; set; }
    public string? DownloadLink { get; set; }

    //authentication attributes
    public int? CheckBetas { get; set; }

    //bool attribute
    public int? Hidden { get; set; }

    //public static Dictionary<string,ProgramsClass> AllPrograms = new();
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

        // Get program details
        var (programName, installedVersion, installDateString) = GetLocalInfo(subkeyPath);
        if (installedVersion=="??.?")
        {
            Log.WriteLine($"No version found for {programName}, not added");
            return;
        }

        if (programName=="N.A")
        {
            Log.WriteLine($"No program name found for key {subkeyPath}");
            return;
        }

        
        var program = new ProgramsClass()
        {
            ProgramKey = subkeyPath,
            ProgramName = programName,
            InstalledVersion = installedVersion,
            InstallDate = installDateString,
            Hidden = 0,
            CheckBetas = 0
        };
        Log.WriteLine($"{program.ProgramName} object was created");
        await dbhelper.SyncNewProgram(program);
        //sync with UI
        
    }

    public static (string,string,string) GetLocalInfo(string subkeyPath)
    {
        Log.WriteLine($"Fetching registry info for {subkeyPath}");
        using RegistryKey? subKey = Registry.LocalMachine.OpenSubKey(subkeyPath);

        // Get program details

        var programName = "N.A";
        string? capturedprogramName = subKey?.GetValue("DisplayName") as string;
        if (!string.IsNullOrEmpty(capturedprogramName))
        {
            programName = Regex.Replace(capturedprogramName, @"\b\d+(\.\d+)+\b", "");
        }

        string? capturedversion = subKey?.GetValue("DisplayVersion") as string;
        string installedVersion = "??.?";
        if (!string.IsNullOrEmpty(capturedversion)) 
        {
            var parts = capturedversion.Split(".");
            var trimmed = parts.Take(4);
            string smaller = string.Join(".", trimmed);
            installedVersion = smaller;
            Version parsed = Version.Parse(smaller);
            installedVersion = parsed.ToString();
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
        Log.WriteLine($"Editing {ProgramName}:");
        if (programName != null){
            Log.Write($"{ProgramName} --> {programName} ; ");
            ProgramName = programName;}

        if (installedVersion != null){
            Log.Write($"{InstalledVersion} --> {installedVersion} ; ");
            InstalledVersion = installedVersion;}

        if (installDate != null){
            Log.Write($"{InstallDate} --> {installDate} ; ");
            InstallDate = installDate;}

        if (latestVersion != null){
            Log.Write($"{LatestVersion} --> {latestVersion} ; ");
            LatestVersion = latestVersion;}

        if (officialPage != null){
            Log.Write($"{OfficialPage} --> {officialPage} ; ");
            OfficialPage = officialPage;}

        if (versionPage != null){
            Log.Write($"{VersionPage} --> {versionPage} ; ");
            VersionPage = versionPage;}

        if (downloadPage != null){
            Log.Write($"{DownloadPage} --> {downloadPage} ; ");
            DownloadPage = downloadPage;}

        if (downloadLink != null){
            Log.Write($"{DownloadLink} --> {downloadLink} ; ");
            DownloadLink = downloadLink;}

        if (hidden != null){
            Log.Write($"{Hidden} --> {hidden} ; ");
            Hidden = hidden;}

        if (checkbetas != null){
            Log.Write($"{CheckBetas} --> {checkbetas} ; ");
            CheckBetas = checkbetas;}

        Log.WriteLine("");

        await dbhelper.SyncEditedInfo(this);
        //sync with UI

    }


    public async Task RemoveProgram()
    {
        await dbhelper.SyncRemoveProgram(this);
        Log.WriteLine($"{this.ProgramName} was removed from the database");
        //sync with UI
    }


    public string? downloadedfilestr()
    {
        string? filename = Directory.GetFiles(Installer.DownloadPath)
            .FirstOrDefault(file => Path.GetFileName(file).Contains(ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1)));
        if (!string.IsNullOrEmpty(filename))
        {
            Log.WriteLine($" Found file '{filename}' for {ProgramName}");
        }
        
        return filename;

    }

    public async Task RefreshLocal()
    {
        Log.WriteLine($"Refreshing {ProgramName}:");
        var (programname, installedversion, installdate) = GetLocalInfo(this.ProgramKey);
        await this.EditProgramInfo(programName: programname, installedVersion: installedversion, installDate: installdate);

        string? downloadfile = this.downloadedfilestr();

        if ((InstalledVersion == LatestVersion && downloadfile != null) )
        {
            File.Delete(downloadfile);
            Log.WriteLine($"Deleted file'{downloadfile}' for {this.ProgramName}");

        }

    }

    public static async Task RefreshLocals(List<ProgramsClass> programslist)
    {
        Log.WriteLine("Refreshing:");
        foreach (ProgramsClass program in programslist)
        {
            if (program.Hidden == 0)
            {
                await program.RefreshLocal();
            }
        }
    }



    public async Task CheckLatestVersion()
    {
        Log.WriteLine($"Version checking for {ProgramName} started:");
        var webprogram = new WebScraping(this);
        await webprogram.CheckVersion();
        await this.EditProgramInfo(latestVersion: webprogram.LatestVersion, versionPage:webprogram.VersionPage , officialPage: webprogram.OfficialPage, downloadPage: webprogram.DownloadPage);
        Log.WriteLine($"Version checking done for {ProgramName}");
    }


    public async Task FetchUpdate()
    {
        Log.WriteLine($"Fetching update for {ProgramName} started:");
        var webprogram = new WebScraping(this);
        await webprogram.FetchUpdate();
        await this.EditProgramInfo(downloadPage: webprogram.DownloadPage, downloadLink: webprogram.DownloadLink);
        Log.WriteLine($"Fetching update done for {ProgramName}");
    }

    public async Task DownloadUpdate()
    {
        Log.WriteLine($"Starting update download for {ProgramName}:");
        if (!string.IsNullOrEmpty(DownloadLink))
        {
            var filename = DownloadLink.Substring(DownloadLink.LastIndexOf('/') + 1);
            int dot = filename.IndexOf('.');
            if (dot != -1)
            {
                var path = Installer.DownloadPath + filename.Substring(0, dot) + $"_v{LatestVersion}_" + ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1) + filename.Substring(dot);
                await downloader.DownloadFileAsync(DownloadLink, path);
                Log.WriteLine($"download of {DownloadLink} started in {path}");
            }
            else
            {
                string ext = "bin"; //get file extension using metadata
                var path = Installer.DownloadPath + ProgramName + $"_v{LatestVersion}_" + ProgramKey.Substring(ProgramKey.LastIndexOf("""\""") + 1) + "." + ext;
                await downloader.DownloadFileAsync(DownloadLink, path);
                Log.WriteLine($"download of {DownloadLink} started in {path} (no extension found)");
            }
             
        }
        else { Log.WriteLine($"No download link found for {ProgramName}"); }
    }


    public static async Task FetchUpdates(List<ProgramsClass> programslist)
    {
        Log.WriteLine("Fetching updates:");
        foreach (ProgramsClass program in programslist)
        {
            var file = program.downloadedfilestr();
            if ( !string.IsNullOrEmpty(program.LatestVersion) && program.Hidden==0 &&((file == null && program.LatestVersion!=program.InstalledVersion) || (file != null && !file.Contains($"{program.LatestVersion}"))))
            {
                await program.FetchUpdate();
            }
        }
    }


    public static async Task CheckLatestVersions(List<ProgramsClass> programslist)
    {
        Log.WriteLine("Checking latest versions:");
        foreach (ProgramsClass program in programslist)
        {
            if (program.Hidden == 0)
            {
                await program.CheckLatestVersion();
            }
        }
    }

    public static async Task DownloadUpdates(List<ProgramsClass> programslist)
    {
        Log.WriteLine("Starting updates downloads:");
        foreach (ProgramsClass program in programslist)
        {
            if (program.Hidden==0)
            {
                await program.DownloadUpdate();
            }
        }
    }

    public static async Task Removeprograms(List<ProgramsClass> programslist)
    {
        Log.WriteLine("Removing programs:");
        foreach (ProgramsClass program in programslist)
        {
            await program.RemoveProgram();
        }
    }




    //method to add programs from a given keylist
    public static async Task AddPrograms(List<string> keyslist)
    {
        Log.WriteLine("Adding programs:");
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

        Log.WriteLine("Getting registry keys:");
        foreach (var registryPath in registryPaths)
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryPath);
            Log.WriteLine($"in {registryPath}:");
            if (key != null)
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    subkeys.Add($"{registryPath}\\{subKeyName}");
                    Log.WriteLine($"Found: {registryPath}\\{subKeyName}");
                    
                }
            }
        }
        return subkeys;
    }

    public static Dictionary<string,string> GetInstalledProgramNames()
    {
        var allkeys = GetInstalledProgramSubkeys();
        Dictionary<string, string> names = new();
        foreach(var key in allkeys)
        {
            names[key] = ProgramsClass.GetLocalInfo(key).Item1;
        }
        return names;
    }

}
