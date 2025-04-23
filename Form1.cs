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

                dataGridViewUpdates.ColumnHeaderMouseClick += dataGridViewUpdates_ColumnHeaderMouseClick;
                dataGridViewInstalled.ColumnHeaderMouseClick += dataGridViewInstalled_ColumnHeaderMouseClick;

                // Clear and add the "Hide Column" menu item dynamically
                contextMenuStripColumn.Items.Clear();
                contextMenuStripColumn.Items.Add("Hide Column", null, HideColumnToolStripMenuItem_Click);


        }

        // These lists will store the indices of hidden rows for Updates and Installed grids
        private List<int> hiddenRowIndicesUpdates = new List<int>();
            private List<int> hiddenRowIndicesInstalled = new List<int>();


            private void LoadGridData()
            {
                allPrograms = GetInstalledProgramsFromDatabase();
                dataGridViewInstalled.AutoGenerateColumns = true;
                dataGridViewInstalled.DataSource = allPrograms;

                if (dataGridViewInstalled.Columns["ProgramKey"] != null)
                    dataGridViewInstalled.Columns["ProgramKey"].Visible = false;

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

                UpdateProgressDisplay();
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

                return connection.Query<GridClass>("SELECT * FROM Programs WHERE CheckBetas = 0").ToList();
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

                // Retrieve the ProgramKey from the right-clicked row's bound data (GridClass object)
                string programKey = ((GridClass)dataGridViewInstalled.Rows[e.RowIndex].DataBoundItem).ProgramKey;

                // Retrieve the Program object using the ProgramKey
                var program = ProgramsClass.dbhelper.GetProgram(programKey);

                // Store the Program object in the context menu's Tag for later use
                contextMenuStripInstalled.Tag = program;

                // Clear existing items before adding new ones
                contextMenuStripInstalled.Items.Clear();

                // Preserve existing menu item: Check for Updates
                contextMenuStripInstalled.Items.Add("Check for Updates");

                // Add new menu item: Hide Row
                contextMenuStripInstalled.Items.Add("Hide Row", null, (s, ev) =>
                {
                    if (dataGridViewInstalled.CurrentRow != null)
                    {
                        dataGridViewInstalled.CurrentRow.Visible = false;
                        hiddenRowIndicesInstalled.Add(dataGridViewInstalled.CurrentRow.Index);
                    }
                });

                // Show the context menu at the current mouse position
                contextMenuStripInstalled.Show(Cursor.Position);
            }
        }


        // Handling right-click for the updates programs grid
        private void dataGridViewUpdates_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dataGridViewUpdates.ClearSelection();
                    dataGridViewUpdates.Rows[e.RowIndex].Selected = true;

                    string key = ((GridClass)dataGridViewUpdates.Rows[e.RowIndex].DataBoundItem).ProgramKey;
                    var program = ProgramsClass.dbhelper.GetProgram(key);
                    contextMenuStripUpdates.Tag = program;

                    // Clear previous items
                    contextMenuStripUpdates.Items.Clear();

                    // Always add "Hide Row" option
                    contextMenuStripUpdates.Items.Add("Hide Row", null, (s, ev) =>
                    {
                        if (dataGridViewUpdates.CurrentRow != null)
                            dataGridViewUpdates.CurrentRow.Visible = false;
                    });

                    // Get current download state
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

                    // Show the context menu at the mouse cursor
                    contextMenuStripUpdates.Show(Cursor.Position);
                }
            }

            private bool areHiddenAppsVisible = false; // Flag to track the visibility of hidden apps

            private void ToggleHiddenApps_Click(object sender, EventArgs e)
            {
                if (areHiddenAppsVisible)
                {
                    // Hide the rows that were previously hidden
                    RestoreHiddenRows(dataGridViewUpdates, hiddenRowIndicesUpdates);
                    RestoreHiddenRows(dataGridViewInstalled, hiddenRowIndicesInstalled);
                }
                else
                {
                    // Invert the visibility (show hidden and hide visible)
                    InvertRowVisibility(dataGridViewUpdates, hiddenRowIndicesUpdates);
                    InvertRowVisibility(dataGridViewInstalled, hiddenRowIndicesInstalled);
                }

                // Toggle the state
                areHiddenAppsVisible = !areHiddenAppsVisible;
            }
            
            private void InvertRowVisibility(DataGridView gridView, List<int> hiddenIndices)
            {
                foreach (DataGridViewRow row in gridView.Rows)
                {
                    int rowIndex = row.Index;
                    if (row.Visible)
                    {
                        row.Visible = false;
                        hiddenIndices.Add(rowIndex); // Add to the hidden list
                    }
                    else
                    {
                        row.Visible = true;
                    }
                }
            }

            private List<int> hiddenRowIndexes = new List<int>(); // To track hidden rows by index
            private List<string> hiddenColumnNames = new List<string>(); // To track hidden columns by name

        private void RestoreHiddenRows(DataGridView gridView, List<int> hiddenIndices)
            {
                foreach (int rowIndex in hiddenIndices)
                {
                    gridView.Rows[rowIndex].Visible = false;
                }
            }

        private void dataGridViewUpdates_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.ColumnIndex >= 0)
            {
                var column = dataGridViewUpdates.Columns[e.ColumnIndex];
                contextMenuStripColumn.Tag = column;
                contextMenuStripColumn.Show(Cursor.Position);
            }
        }

        private void dataGridViewInstalled_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.ColumnIndex >= 0)
            {
                var column = dataGridViewInstalled.Columns[e.ColumnIndex];
                contextMenuStripColumn.Tag = column;
                contextMenuStripColumn.Show(Cursor.Position);
            }
        }


        private void HideColumnToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            // Get the column that was right-clicked using the ColumnIndex
            int columnIndex = dataGridViewUpdates.SelectedCells[0].ColumnIndex;

            if (columnIndex >= 0)  // Ensure it's a valid column
            {
                string columnName = dataGridViewUpdates.Columns[columnIndex].Name;

                // Hide the column
                dataGridViewUpdates.Columns[columnName].Visible = false;

                // Add the column name to the hidden columns list
                if (!hiddenColumnNames.Contains(columnName))
                {
                    hiddenColumnNames.Add(columnName);
                }
            }
        }


        private void HideRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the row that was right-clicked using the RowIndex
            int rowIndex = dataGridViewUpdates.SelectedCells[0].RowIndex;

            if (rowIndex >= 0)  // Ensure it's a valid row
            {
                // Hide the row
                dataGridViewUpdates.Rows[rowIndex].Visible = false;

                // Add the row index to the hidden rows list
                if (!hiddenRowIndexes.Contains(rowIndex))
                {
                    hiddenRowIndexes.Add(rowIndex);
                }
            }
        }

        // Track hidden rows and columns for both DataGridViews
        private List<int> hiddenRowIndexesInstalled = new List<int>();
        private List<string> hiddenColumnNamesInstalled = new List<string>();

        private List<int> hiddenRowIndexesUpdates = new List<int>();
        private List<string> hiddenColumnNamesUpdates = new List<string>();

        private void hiddenAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show hidden rows and columns in Installed tab
            foreach (var rowIndex in hiddenRowIndexesInstalled)
            {
                dataGridViewInstalled.Rows[rowIndex].Visible = true;
            }

            foreach (var columnName in hiddenColumnNamesInstalled)
            {
                dataGridViewInstalled.Columns[columnName].Visible = true;
            }

            // Show hidden rows and columns in Updates tab
            foreach (var rowIndex in hiddenRowIndexesUpdates)
            {
                dataGridViewUpdates.Rows[rowIndex].Visible = true;
            }

            foreach (var columnName in hiddenColumnNamesUpdates)
            {
                dataGridViewUpdates.Columns[columnName].Visible = true;
            }

            // Optionally: You could also refresh both DataGridViews
            dataGridViewInstalled.Refresh();
            dataGridViewUpdates.Refresh();
        }

        // Hide rows and columns in Installed tab
        private void HideRowColumnInstalled(object sender, EventArgs e)
        {
            var selectedRowIndex = dataGridViewInstalled.SelectedCells[0].RowIndex;
            var selectedColumnName = dataGridViewInstalled.Columns[dataGridViewInstalled.SelectedCells[0].ColumnIndex].Name;

            // Hide the row
            if (!hiddenRowIndexesInstalled.Contains(selectedRowIndex))
            {
                hiddenRowIndexesInstalled.Add(selectedRowIndex);
                dataGridViewInstalled.Rows[selectedRowIndex].Visible = false;
            }

            // Hide the column
            if (!hiddenColumnNamesInstalled.Contains(selectedColumnName))
            {
                hiddenColumnNamesInstalled.Add(selectedColumnName);
                dataGridViewInstalled.Columns[selectedColumnName].Visible = false;
            }
        }

        // Hide rows and columns in Updates tab
        private void HideRowColumnUpdates(object sender, EventArgs e)
        {
            var selectedRowIndex = dataGridViewUpdates.SelectedCells[0].RowIndex;
            var selectedColumnName = dataGridViewUpdates.Columns[dataGridViewUpdates.SelectedCells[0].ColumnIndex].Name;

            // Hide the row
            if (!hiddenRowIndexesUpdates.Contains(selectedRowIndex))
            {
                hiddenRowIndexesUpdates.Add(selectedRowIndex);
                dataGridViewUpdates.Rows[selectedRowIndex].Visible = false;
            }

            // Hide the column
            if (!hiddenColumnNamesUpdates.Contains(selectedColumnName))
            {
                hiddenColumnNamesUpdates.Add(selectedColumnName);
                dataGridViewUpdates.Columns[selectedColumnName].Visible = false;
            }
        }

        private async void Check_For_Updates_Click(object sender, EventArgs e)
        {
            // Get all programs and store in a list
            var programsList = ProgramsClass.dbhelper.GetAllPrograms();

            // Run the check for updates using the list
            await ProgramsClass.CheckLatestVersions(programsList);
        }

        private void Hidden_Apps_Click(object sender, EventArgs e)
        {
            // Show hidden rows and columns in Installed tab
            foreach (var rowIndex in hiddenRowIndexesInstalled)
            {
                dataGridViewInstalled.Rows[rowIndex].Visible = true;
            }

            foreach (var columnName in hiddenColumnNamesInstalled)
            {
                dataGridViewInstalled.Columns[columnName].Visible = true;
            }

            // Show hidden rows and columns in Updates tab
            foreach (var rowIndex in hiddenRowIndexesUpdates)
            {
                dataGridViewUpdates.Rows[rowIndex].Visible = true;
            }

            foreach (var columnName in hiddenColumnNamesUpdates)
            {
                dataGridViewUpdates.Columns[columnName].Visible = true;
            }

            // Optionally: You could also refresh both DataGridViews
            dataGridViewInstalled.Refresh();
            dataGridViewUpdates.Refresh();
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

            private void scanForNewAppsToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Application.Run(new ButtonBoxForm());
            
            }

            private void selectAccountToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Auth.SetProfileNumber();
            }

            private void downloadPathToolStripMenuItem_Click(object sender, EventArgs e)
            {
                Installer.SetDownloadPath();
            }



            private void dataGridView_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
            {
                // Right-click on headers handled here (existing code remains unchanged)
            }

            private void UpdateProgramHiddenStatus(string programKey, bool isHidden)
            {
                using var connection = new SQLiteConnection("Data Source=Programs.db");
                connection.Open();

                connection.Execute("UPDATE Programs SET CheckBetas = @Hidden WHERE ProgramKey = @Key",
                    new { Hidden = isHidden ? 1 : 0, Key = programKey });
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
