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
    string selectQuery = "SELECT * FROM Programs;";
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
                _username,
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
                @_username,
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
                    _username = @_username,
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
            connection.Open();
            connection.Execute(@"
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
                            _username TEXT,
                            _password TEXT,
                            Hidden INT
                        )");
        }
    }

    //Add a program in the databse
    public void SyncNewProgram(ProgramsClass program)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {

            connection.Open();
            connection.Execute(insertQuery, program);
        }

    }


    //Edit program info
    public void SyncEditedInfo(string ProgramKey)
    {
        ProgramsClass program = ProgramsClass.ProgramsDict[ProgramKey];
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            connection.Execute(UpdateQuery, program);
        }
    }


    //Remove a program
    public void SyncRemoveProgram(string programKey)
    {
        ProgramsClass program = ProgramsClass.ProgramsDict[programKey];
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            connection.Execute(DeleteQuery, program);
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
