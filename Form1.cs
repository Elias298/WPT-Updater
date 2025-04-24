    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Data.SQLite;
    using Dapper;
    using static WPT_Updater.KeyStuff;

    namespace WPT_Updater
    {
        public partial class Form1 : Form
        {
            private List<GridClass> allPrograms = new();
            private System.Windows.Forms.Timer? refreshTimer;
            private Dictionary<string, float> downloadProgress = new();
            private Dictionary<string, WebClient> activeDownloads = new();
            private string? selectedProgramKeyForUpdates;

            public Form1()
            {
                InitializeComponent();
            }

            private void Form1_Load(object sender, EventArgs e)
            {
                // Load data into the grids for installed programs and updates
                LoadGridData();           // Loads data for the Installed tab
                LoadUpdatesGridData();    // Loads data for the Updates tab

                // Attach right-click event handler to data grid views
                dataGridViewInstalled.CellMouseClick += dataGridViewInstalled_CellMouseClick;
                dataGridViewUpdates.CellMouseClick += dataGridViewUpdates_CellMouseClick;

                // For Installed tab context menu
                contextMenuStripInstalled = new ContextMenuStrip();
                contextMenuStripInstalled.Items.Add("Check for Update", null, OnCheckForUpdate_Click);

                // For Updates tab context menu
                contextMenuStripUpdates = new ContextMenuStrip();
                contextMenuStripUpdates.Items.Add("Download", null, OnDownload_Click);
                contextMenuStripUpdates.Items.Add("Pause", null, OnPause_Click);
                contextMenuStripUpdates.Items.Add("Resume", null, OnResume_Click);
                contextMenuStripUpdates.Items.Add("Cancel", null, OnCancel_Click);

                // Set up a timer to refresh data every 5 seconds (adjust as needed)
                refreshTimer = new System.Windows.Forms.Timer();
                refreshTimer.Interval = 5000; // 5 seconds
                refreshTimer.Tick += RefreshTimer_Tick; // Event handler for the timer tick
                refreshTimer.Start();

                // Updating the Database on changes done to cells in the DataGridView
                dataGridViewInstalled.CellValueChanged += DataGridView_CellValueChanged;
                dataGridViewUpdates.CellValueChanged += DataGridView_CellValueChanged;

                Scan_New_Apps.Click += Scan_New_Apps_Click;
                Select_account.Click += Select_account_Click;
                Download_Path.Click += Download_Path_Click;
                Check_For_Updates.Click += Check_For_Updates_Click;
        }

        // These lists will store the indices of hidden rows for Updates and Installed grids
        private List<int> hiddenRowIndicesUpdates = new List<int>();
            private List<int> hiddenRowIndicesInstalled = new List<int>();


        private void LoadGridData()
        {
            // Get installed programs from the database, ensuring the data type is GridClass
            allPrograms = GetInstalledProgramsFromDatabase();

            // Bind the data to both DataGridViews
            dataGridViewInstalled.AutoGenerateColumns = true;
            dataGridViewInstalled.DataSource = allPrograms;

            dataGridViewUpdates.AutoGenerateColumns = true;
            dataGridViewUpdates.DataSource = allPrograms;

            // Hide ProgramKey column in both DataGridViews
            if (dataGridViewInstalled.Columns["ProgramKey"] != null)
                dataGridViewInstalled.Columns["ProgramKey"].Visible = false;

            if (dataGridViewUpdates.Columns["ProgramKey"] != null)
                dataGridViewUpdates.Columns["ProgramKey"].Visible = false;

            // Set column visibility and properties for other columns
            if (dataGridViewInstalled.Columns["CheckBetas"] != null)
            {
                dataGridViewInstalled.Columns["CheckBetas"].HeaderText = "Check Betas";
                dataGridViewInstalled.Columns["CheckBetas"].ReadOnly = true;
            }

            if (dataGridViewUpdates.Columns["CheckBetas"] != null)
            {
                dataGridViewUpdates.Columns["CheckBetas"].HeaderText = "Check Betas";
                dataGridViewUpdates.Columns["CheckBetas"].ReadOnly = true;
            }

            // Optionally, add the DownloadStatus column to the installed grid if needed
            if (!dataGridViewInstalled.Columns.Contains("DownloadStatus"))
            {
                DataGridViewTextBoxColumn progressColumn = new()
                {
                    Name = "DownloadStatus",
                    HeaderText = "Progress (%)",
                    ReadOnly = true
                };
                dataGridViewInstalled.Columns.Insert(0, progressColumn);
            }

            // Ensure all columns are correctly loaded and set their visibility
            UpdateProgressDisplay();
        }




        private async void Hidden_Apps_Click(object sender, EventArgs e)
        {
            // Reverse the Hidden flag for all programs in the list
            foreach (var program in allPrograms)
            {
                program.Hidden = program.Hidden == 1 ? 0 : 1;
            }

            // Update the Hidden status in the database for all programs
            await UpdateHiddenStatusInDatabase(allPrograms);

            // Refresh DataGridViews to reflect the changes
            LoadGridData();
        }





        private async Task UpdateHiddenStatusInDatabase(List<GridClass> programsList)
        {
            string connectionString = "Data Source=Programs.db";

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var program in programsList)
                {
                    // Update the Hidden column in the database for each program
                    string updateQuery = "UPDATE Programs SET Hidden = @Hidden WHERE ProgramKey = @ProgramKey";
                    await connection.ExecuteAsync(updateQuery, new { Hidden = program.Hidden, ProgramKey = program.ProgramKey });
                }
            }

            Log.WriteLine("Hidden column updated successfully.");
        }


        private void LoadUpdatesGridData()
            {
                var updates = allPrograms
                    .Where(p => p.InstalledVersion != p.LatestVersion)
                    .ToList();

                dataGridViewUpdates.AutoGenerateColumns = true;
                dataGridViewUpdates.DataSource = updates;

                if (dataGridViewUpdates.Columns["ProgramKey"] != null)
                    dataGridViewUpdates.Columns["ProgramKey"].Visible = false;

                if (!dataGridViewUpdates.Columns.Contains("DownloadStatus"))
                {
                    DataGridViewTextBoxColumn progressColumn = new()
                    {
                        Name = "DownloadStatus",
                        HeaderText = "Progress (%)",
                        ReadOnly = true
                    };
                    dataGridViewUpdates.Columns.Insert(0, progressColumn);
                }

                foreach (DataGridViewRow row in dataGridViewUpdates.Rows)
                {
                    if (row.DataBoundItem is GridClass program)
                    {
                        string key = program.ProgramKey;
                        if (downloadProgress.ContainsKey(key))
                            row.Cells["DownloadStatus"].Value = downloadProgress[key].ToString("0.00");
                        else
                            row.Cells["DownloadStatus"].Value = "0.00";
                    }
                }
            }


        private List<GridClass> GetInstalledProgramsFromDatabase()
        {
            string connectionString = "Data Source=Programs.db";

            using var connection = new SQLiteConnection(connectionString);
            connection.Open();

            return connection.Query<GridClass>("SELECT * FROM Programs").ToList();
        }



        private void ApplySearchFilter()
            {
                string searchText = textBox1.Text.ToLower();

                var filtered = allPrograms
                    .Where(p => p.ProgramName != null && p.ProgramName.ToLower().Contains(searchText))
                    .ToList();

                dataGridViewInstalled.DataSource = filtered;
            }

            private void RefreshTimer_Tick(object? sender, EventArgs e)
            {
                var newData = GetInstalledProgramsFromDatabase();

                if (newData.Count != allPrograms.Count ||
                    !newData.SequenceEqual(allPrograms, new GridClassComparer()))
                {
                    allPrograms = newData;
                    ApplySearchFilter();
                }

                UpdateProgressDisplay();
            }

            private void UpdateProgressDisplay()
            {
                foreach (DataGridViewRow row in dataGridViewInstalled.Rows)
                {
                    if (row.DataBoundItem is GridClass program)
                    {
                        string key = program.ProgramKey;
                        if (downloadProgress.ContainsKey(key))
                        {
                            row.Cells["DownloadStatus"].Value = downloadProgress[key].ToString("0.00");
                        }
                        else
                        {
                            row.Cells["DownloadStatus"].Value = "0.00";
                        }
                    }
                }
            }

            private void DataGridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
            {
                // Ensure we're not in a header row or invalid index
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    var grid = sender as DataGridView;
                    var row = grid.Rows[e.RowIndex];
                    var column = grid.Columns[e.ColumnIndex];

                    // Extract the ProgramKey from the row (assuming it's stored in the "ProgramKey" column)
                    string programKey = row.Cells["ProgramKey"].Value.ToString();

                    // Get the name of the column that was modified
                    string columnName = column.Name;

                    // Get the new value in the cell
                    string newValue = row.Cells[e.ColumnIndex].Value?.ToString() ?? "";

                    // Call the method to update the database
                    EditCell(programKey, columnName, newValue);
                }
            }

            private async void EditCell(string programKey, string columnName, string newValue)
            {
                // Call the dbhelper method with the row (ProgramKey), column name, and new value
                await ProgramsClass.dbhelper.EditCell(programKey, columnName, newValue);
            }


        // Handling right-click for the installed programs grid
        private void dataGridViewInstalled_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                // Clear previous selection and select the right-clicked row
                dataGridViewInstalled.ClearSelection();
                dataGridViewInstalled.Rows[e.RowIndex].Selected = true;

                // Retrieve the bound GridClass object
                var gridProgram = dataGridViewInstalled.Rows[e.RowIndex].DataBoundItem as GridClass;

                if (gridProgram == null)
                    return;

                // Store GridClass object in context menu Tag
                contextMenuStripInstalled.Tag = gridProgram;

                // Clear existing context menu items
                contextMenuStripInstalled.Items.Clear();

                // Add "Check for Updates"
                contextMenuStripInstalled.Items.Add("Check for Updates");

                // Add "Hide Row"
                contextMenuStripInstalled.Items.Add("Hide Row", null, (s, ev) =>
                {
                    if (contextMenuStripInstalled.Tag is GridClass programToHide)
                    {
                        // Update hidden status in DB
                        ProgramsClass.dbhelper.SetProgramHidden(programToHide.ProgramKey, 1);

                        // Reload and bind only visible programs
                        dataGridViewInstalled.DataSource = ProgramsClass.dbhelper
                            .GetAllPrograms()
                            .Where(p => p.Hidden == 0)
                            .ToList();
                    }
                });

                // Show context menu at cursor position
                contextMenuStripInstalled.Show(Cursor.Position);
            }
        }



        private void dataGridViewUpdates_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                dataGridViewUpdates.ClearSelection();
                dataGridViewUpdates.Rows[e.RowIndex].Selected = true;

                var gridProgram = dataGridViewUpdates.Rows[e.RowIndex].DataBoundItem as GridClass;

                if (gridProgram == null)
                    return;

                // Get full program info from db
                var program = ProgramsClass.dbhelper.GetProgram(gridProgram.ProgramKey);
                if (program == null)
                    return;

                contextMenuStripUpdates.Tag = program;

                // Clear previous context menu items
                contextMenuStripUpdates.Items.Clear();

                // Add "Hide Row"
                contextMenuStripUpdates.Items.Add("Hide Row", null, (s, ev) =>
                {
                    if (contextMenuStripUpdates.Tag is ProgramsClass p)
                    {
                        ProgramsClass.dbhelper.SetProgramHidden(p.ProgramKey, 1);

                        // Rebind DataGridView with visible items only
                        dataGridViewUpdates.DataSource = ProgramsClass.dbhelper
                            .GetAllPrograms()
                            .Where(pr => pr.Hidden == 0)
                            .ToList();
                    }
                });

                // Add download options based on current download state
                var state = program.GetDownloadState();

                switch (state)
                {
                    case DownloadTask.DownloadState.NotAdded:
                    case DownloadTask.DownloadState.NotStarted:
                        contextMenuStripUpdates.Items.Add("Download", null, OnDownload_Click);
                        break;

                    case DownloadTask.DownloadState.Downloading:
                        contextMenuStripUpdates.Items.Add("Pause", null, OnPause_Click);
                        contextMenuStripUpdates.Items.Add("Cancel", null, OnCancel_Click);
                        break;

                    case DownloadTask.DownloadState.Paused:
                        contextMenuStripUpdates.Items.Add("Resume", null, OnResume_Click);
                        contextMenuStripUpdates.Items.Add("Cancel", null, OnCancel_Click);
                        break;

                    case DownloadTask.DownloadState.Completed:
                        contextMenuStripUpdates.Items.Add("Re-download", null, OnDownload_Click);
                        break;

                    case DownloadTask.DownloadState.Canceled:
                        contextMenuStripUpdates.Items.Add("Retry Download", null, OnDownload_Click);
                        break;
                }

                // Show context menu at cursor
                contextMenuStripUpdates.Show(Cursor.Position);
            }
        }


        private void HideRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewInstalled.SelectedRows.Count > 0)
            {
                var row = dataGridViewInstalled.SelectedRows[0];
                var program = row.DataBoundItem as GridClass;

                if (program != null)
                {
                    // Set Hidden column to 1 (hide the app)
                    program.Hidden = 1;

                    using (var connection = new SQLiteConnection("Data Source=Programs.db"))
                    {
                        connection.Open();
                        connection.Execute("UPDATE Programs SET Hidden = 1 WHERE ProgramKey = @ProgramKey", new { ProgramKey = program.ProgramKey });
                    }

                    // Reload the grid to reflect the updated hidden state
                    LoadGridData();
                }
            }
        }    
        private void LoadInstalledTab()
        {
            var allPrograms = ProgramsClass.dbhelper.GetAllPrograms();
            dataGridViewInstalled.DataSource = allPrograms
                .Where(p => p.Hidden == 0)
                .ToList();
        }

        private void LoadUpdatesTab()
        {
            var allPrograms = ProgramsClass.dbhelper.GetAllPrograms();
            dataGridViewUpdates.DataSource = allPrograms
                .Where(p => p.Hidden == 0 && !string.IsNullOrEmpty(p.LatestVersion) && p.InstalledVersion != p.LatestVersion)
                .ToList();
        }

        private async void Check_For_Updates_Click(object sender, EventArgs e)
        {
            var programsList = ProgramsClass.dbhelper.GetAllPrograms();
            await ProgramsClass.CheckLatestVersions(programsList);

            // Reload UI after checking
            LoadInstalledTab();
            LoadUpdatesTab();
        }

        private async void OnCheckForUpdate_Click(object? sender, EventArgs e)
            {
                if (contextMenuStripInstalled.Tag is ProgramsClass program)
                {
                    await program.CheckLatestVersion();

                    // Refresh both tabs, especially Updates tab to reflect changes if any
                    LoadGridData();         // Refresh Installed tab
                    LoadUpdatesGridData();  // Refresh Updates tab

                    // Optional: Let the user know the result
                    if (!string.IsNullOrEmpty(program.LatestVersion) &&
                        Version.TryParse(program.LatestVersion, out var latest) &&
                        Version.TryParse(program.InstalledVersion, out var installed) &&
                        latest > installed)
                    {
                        MessageBox.Show($"{program.ProgramName} has an update!\nInstalled: {installed}\nLatest: {latest}");
                    }
                    else
                    {
                        MessageBox.Show($"No updates found for {program.ProgramName}.");
                    }
                }
            }


            private async void OnDownload_Click(object? sender, EventArgs e)
            {
                if (contextMenuStripUpdates.Tag is ProgramsClass program)
                {
                    // Create a progress reporter that updates the DataGridView
                    var progress = new Progress<float>(p =>
                    {
                        UpdateDownloadProgress(program.ProgramKey, p);
                    });

                    await program.DownloadUpdate(progress);

                    RefreshUpdatesTab();
                }
            }

            private void UpdateDownloadProgress(string programKey, float progressPercentage)
            {
                foreach (DataGridViewRow row in dataGridViewUpdates.Rows)
                {
                    if (row.DataBoundItem is GridClass program && program.ProgramKey == programKey)
                    {
                        row.Cells["DownloadStatus"].Value = $"{(progressPercentage * 100):0.0}%";
                        break;
                    }
                }
            }


            private void OnPause_Click(object? sender, EventArgs e)
            {
                if (contextMenuStripUpdates.Tag is ProgramsClass program)
                {
                    program.PauseUpdate();
                    RefreshUpdatesTab();
                }
            }

            private void OnResume_Click(object? sender, EventArgs e)
            {
                if (contextMenuStripUpdates.Tag is ProgramsClass program)
                {
                    program.ResumeUpdate();
                    RefreshUpdatesTab();
                }
            }

            private void OnCancel_Click(object? sender, EventArgs e)
            {
                if (contextMenuStripUpdates.Tag is ProgramsClass program)
                {
                    program.CancelUpdate();
                    RefreshUpdatesTab();
                }
            }

            private void RefreshUpdatesTab()
            {
                LoadUpdatesGridData(); // or whatever method you use to refresh that DataGridView
            }

        private void Scan_New_Apps_Click(object sender, EventArgs e)
        {
            ButtonBoxForm form = new ButtonBoxForm();
            form.Show(); // Opens the form in non-modal mode
        }



        private bool isProfileSet = false; // Flag to track if the profile has been set

        private void Select_account_Click(object sender, EventArgs e)
        {
            if (!isProfileSet)
            {
                // Run SetProfileNumber asynchronously to avoid blocking the UI
                Task.Run(() =>
                {
                    Auth.SetProfileNumber();
                    isProfileSet = true; // Set the flag once the profile is set
                });
            }
            else
            {
                MessageBox.Show("Profile has already been set.");
            }
        }




        private void Download_Path_Click(object sender, EventArgs e)
            {
                Installer.SetDownloadPath();
            }



            private void dataGridView_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
            {
                // Right-click on headers handled here (existing code remains unchanged)
            }

        private void UpdateHiddenStatusInDatabase()
        {
            string connectionString = "Data Source=Programs.db";
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                foreach (var program in allPrograms)
                {
                    var query = "UPDATE Programs SET Hidden = @Hidden WHERE ProgramKey = @ProgramKey";
                    connection.Execute(query, new { Hidden = program.Hidden, ProgramKey = program.ProgramKey });
                }
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e) => ApplySearchFilter();

            private void tabPage1_Click(object sender, EventArgs e) { }
            private void tabPage2_Click(object sender, EventArgs e) { }
            private void button1_Click(object sender, EventArgs e) { }
            private void groupBox1_Enter(object sender, EventArgs e) { }
            private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

            private void button1_Click_1(object sender, EventArgs e)
            {
                kebabMenuStrip.Show(button1, new Point(0, button1.Height));
            }

            private void kebabMenuStrip_Opening(object sender, CancelEventArgs e) { }
        }

        public class GridClassComparer : IEqualityComparer<GridClass>
        {
            public bool Equals(GridClass? x, GridClass? y)
            {
                if (x == null || y == null) return false;

                return x.ProgramKey == y.ProgramKey &&
                       x.ProgramName == y.ProgramName &&
                       x.InstalledVersion == y.InstalledVersion &&
                       x.LatestVersion == y.LatestVersion &&
                       x.InstallDate == y.InstallDate;
            }

            public int GetHashCode(GridClass obj)
            {
                return HashCode.Combine(obj.ProgramKey, obj.ProgramName, obj.InstalledVersion, obj.LatestVersion, obj.InstallDate);
            }
        }
    }
