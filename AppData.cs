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
    public static string DbPath = "Programs.db";
    private readonly string connectionString = $"Data Source={DbPath}";
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
        Log.WriteLine("(Re)initializing database");
        File.Delete(DbPath);
        using (var connection = new SQLiteConnection($"Data Source={DbPath}"))
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
        Log.WriteLine("Database initialized");
    }

    //Add a program in the databse
    public async Task SyncNewProgram(ProgramsClass program)
    {
        Log.WriteLine($"Adding {program.ProgramName} to database");
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(insertQuery, program);
        }
        Log.WriteLine($"{program.ProgramName} succesfully added");

    }


    //Edit program info
    public async Task SyncEditedInfo(ProgramsClass program)
    {
        Log.WriteLine($"Syncing {program.ProgramName} new info with database");
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(UpdateQuery, program);
        }
        Log.WriteLine($"{program.ProgramName} info succesfully synced");
    }


    //Remove a program
    public async Task SyncRemoveProgram(ProgramsClass program)
    {
        Log.WriteLine($"Removing {program.ProgramName} from database");
        using (var connection = new SQLiteConnection(connectionString))
        {
            await connection.ExecuteAsync(DeleteQuery, program);
        }
        Log.WriteLine($"{program.ProgramName} removed from database");
    }

    // Retrieve all programs from the database
    public Dictionary<string, ProgramsClass> GetAllPrograms()
    {
        Log.WriteLine("Retreiving programs from database");
        using (var connection = new SQLiteConnection(connectionString))
        {
            //connection.Open();
            // Execute the query and map the results to a list of ProgramsClass objects
            var programsDictionary = connection
                .Query<ProgramsClass>(selectQuery)
                .ToDictionary(p => p.ProgramKey);
            Log.WriteLine("programs successfully added to dictionary");
            return programsDictionary!;
        }
    }





}
