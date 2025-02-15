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

internal class AppData{

    private readonly string connectionString = "Data Source=Programs.db";
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
                hidden
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
                @hidden
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
                    hidden = @hidden
                WHERE
                    ProgramKey = @ProgramKey;";



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
                            hidden INT
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

    public void SyncProgramUpdate(ProgramsClass program)
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            connection.Execute(UpdateQuery, program);
        }
    }

    // Insert or update a program in the database
            /*public void SyncProgram(ProgramsClass program)
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    var existingProgram = connection.QueryFirstOrDefault<ProgramsClass>(
                        "SELECT * FROM Programs WHERE ProgramName = @ProgramName",
                        new { program.ProgramName });

                    if (existingProgram == null)
                    {
                        // Insert new program
                        connection.Execute(
                            "INSERT INTO Programs (ProgramName, ProgramVersion, LatestVersion, InstallDate) " +
                            "VALUES (@ProgramName, @ProgramVersion, @LatestVersion, @InstallDate)",
                            program);
                    }
                    else
                    {
                        // Update existing program
                        connection.Execute(
                            "UPDATE Programs SET ProgramVersion = @ProgramVersion, LatestVersion = @LatestVersion, " +
                            "InstallDate = @InstallDate WHERE ProgramName = @ProgramName",
                            program);
                    }
                }
            }*/


            // Get all programs from the database
    public IEnumerable<ProgramsClass> GetAllPrograms()
    {
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            return connection.Query<ProgramsClass>("SELECT * FROM Programs");
        }
    }
    
    

}
