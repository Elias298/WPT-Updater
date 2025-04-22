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


        }


        private void LoadGridData()
        {
            allPrograms = GetInstalledProgramsFromDatabase();
            dataGridViewInstalled.AutoGenerateColumns = true;
            dataGridViewInstalled.DataSource = allPrograms;

            if (dataGridViewInstalled.Columns["ProgramKey"] != null)
                dataGridViewInstalled.Columns["ProgramKey"].Visible = false;

            if (dataGridViewInstalled.Columns["CheckBetas"] != null)
                dataGridViewInstalled.Columns["CheckBetas"].HeaderText = "Hidden";

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

        private void EditCell(string programKey, string columnName, string newValue)
        {
            // Call the dbhelper method with the row (ProgramKey), column name, and new value
            //ProgramsClass.dbhelper.UpdateCell(programKey, columnName, newValue);
        }


        // Handling right-click for the installed programs grid
        private void dataGridViewInstalled_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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

                // Show the context menu at the current mouse position
                contextMenuStripInstalled.Show(Cursor.Position);
            }
        }

        // Handling right-click for the updates programs grid
        private void dataGridViewUpdates_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
            {
                // Clear previous selection and select the right-clicked row
                dataGridViewUpdates.ClearSelection();
                dataGridViewUpdates.Rows[e.RowIndex].Selected = true;

                // Retrieve the ProgramKey from the right-clicked row's bound data (GridClass object)
                string programKey = ((GridClass)dataGridViewUpdates.Rows[e.RowIndex].DataBoundItem).ProgramKey;

                // Retrieve the Program object using the ProgramKey
                var program = ProgramsClass.dbhelper.GetProgram(programKey);

                // Store the Program object in the context menu's Tag for later use
                contextMenuStripUpdates.Tag = program;

                // Show the context menu at the current mouse position
                contextMenuStripUpdates.Show(Cursor.Position);
            }
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


        private void OnDownload_Click(object? sender, EventArgs e)
        {
            if (contextMenuStripUpdates.Tag is ProgramsClass program)
            {
                MessageBox.Show($"Download: {program.ProgramName}");
            }
        }

        private void OnPause_Click(object? sender, EventArgs e)
        {
            if (contextMenuStripUpdates.Tag is ProgramsClass program)
            {
                MessageBox.Show($"Pause: {program.ProgramName}");
            }
        }

        private void OnResume_Click(object? sender, EventArgs e)
        {
            if (contextMenuStripUpdates.Tag is ProgramsClass program)
            {
                MessageBox.Show($"Resume: {program.ProgramName}");
            }
        }

        private void OnCancel_Click(object? sender, EventArgs e)
        {
            if (contextMenuStripUpdates.Tag is ProgramsClass program)
            {
                MessageBox.Show($"Cancel: {program.ProgramName}");
            }
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
