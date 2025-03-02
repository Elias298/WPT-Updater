using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dapper;
using static System.Net.Mime.MediaTypeNames;
namespace WPT_Updater;

internal class AppData
{

    private readonly string connectionString = "Data Source=Programs.db";
    public readonly string selectQuery = "SELECT * FROM Programs;";
    public readonly string insertQuery = @"
            INSERT INTO Programs (
                ProgramKey,
                ProgramName,
                InstalledVersion,
                InstallDate,
                LatestVersion,
                OfficialPage,
                VersionPage,
                DownloadPage,
                DownloadLink,
                CheckBetas,
                _password,
                Hidden
            )
            VALUES (
                @ProgramKey,
                @ProgramName,
                @InstalledVersion,
                @InstallDate,
                @LatestVersion,
                @OfficialPage,
                @VersionPage,
                @DownloadPage,
                @DownloadLink,
                @CheckBetas,
                @_password,
                @Hidden
            );";

    public readonly string UpdateQuery = @"
                UPDATE Programs
                SET
                    ProgramName = @ProgramName,
                    InstalledVersion = @InstalledVersion,
                    InstallDate = @InstallDate,
                    LatestVersion = @LatestVersion,
                    OfficialPage = @OfficialPage,
                    VersionPage = @VersionPage,
                    DownloadPage = @DownloadPage,
                    DownloadLink = @DownloadLink,
                    CheckBetas = @CheckBetas,
                    _password = @_password,
                    Hidden = @Hidden
                WHERE
                    ProgramKey = @ProgramKey;";

    public readonly string DeleteQuery = @"
                DELETE FROM Programs
                WHERE ProgramKey = @ProgramKey;";




    // Initialize the database (create table if it doesn't exist)
    public static void InitializeDatabase()
    {
        using (var connection = new SQLiteConnection("Data Source=Programs.db"))
        {
            connection.ExecuteAsync(@"
                        CREATE TABLE Programs (
                            ProgramKey TEXT PRIMARY KEY,
                            ProgramName TEXT ,
                            InstalledVersion TEXT ,
                            InstallDate TEXT ,
                            LatestVersion TEXT,
                            OfficialPage TEXT,
                            VersionPage TEXT,
                            DownloadPage TEXT,
                            DownloadLink TEXT,
                            CheckBetas INT,
                            _password TEXT,
                            Hidden INT
                        )");
        }
    }

    //Add a program in the databse
    public async Task SyncNewProgram(ProgramsClass program)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(insertQuery, program);
        }

    }


    //Edit program info
    public async Task SyncEditedInfo(string ProgramKey)
    {
        ProgramsClass program = ProgramsClass.ProgramsDict[ProgramKey];
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(UpdateQuery, program);
        }
    }


    //Remove a program
    public async Task SyncRemoveProgram(string programKey)
    {
        ProgramsClass program = ProgramsClass.ProgramsDict[programKey];
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(DeleteQuery, program);
        }
    }

    // Retrieve all programs from the database
    public Dictionary<string, ProgramsClass> GetAllPrograms()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            // Execute the query and map the results to a list of ProgramsClass objects
            var programsDictionary = connection
                .Query<ProgramsClass>(selectQuery)
                .ToDictionary(p => p.ProgramKey);
            return programsDictionary!;
        }
    }





}
