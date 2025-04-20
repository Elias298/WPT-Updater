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

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGridData();

            dataGridViewInstalled.CellMouseClick += dataGridView_CellMouseClick;
            dataGridViewInstalled.CellMouseDown += dataGridViewInstalled_CellMouseDown;

            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 5000;
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
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

        private void dataGridViewInstalled_CellMouseDown(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var column = dataGridViewInstalled.Columns[e.ColumnIndex];
                if (column.Name == "DownloadStatus")
                {
                    var row = dataGridViewInstalled.Rows[e.RowIndex];
                    var program = (GridClass)row.DataBoundItem;

                    ContextMenuStrip contextMenu = new();

                    ToolStripMenuItem startItem = new("Start Download");
                    startItem.Click += (s, ev) => StartDownload(program, row);

                    contextMenu.Items.Add(startItem);

                    if (activeDownloads.ContainsKey(program.ProgramKey))
                    {
                        ToolStripMenuItem pauseItem = new("Pause");
                        ToolStripMenuItem resumeItem = new("Resume");

                        pauseItem.Click += (s, ev) => PauseDownload(program.ProgramKey);
                        resumeItem.Click += (s, ev) => ResumeDownload(program, row);

                        contextMenu.Items.Add(pauseItem);
                        contextMenu.Items.Add(resumeItem);
                    }

                    var cellRect = dataGridViewInstalled.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);
                    var dropPoint = dataGridViewInstalled.PointToScreen(new Point(cellRect.Left, cellRect.Bottom));
                    contextMenu.Show(dropPoint);
                }
            }
        }

        private void StartDownload(GridClass program, DataGridViewRow row)
        {
            string targetPath = Path.Combine(Path.GetTempPath(), program.ProgramName + ".zip");

            WebClient client = new();
            activeDownloads[program.ProgramKey] = client;
            downloadProgress[program.ProgramKey] = 0f;

            client.DownloadProgressChanged += (s, e) =>
            {
                float progress = e.ProgressPercentage;
                downloadProgress[program.ProgramKey] = progress;
                row.Cells["DownloadStatus"].Value = progress.ToString("0.00");
            };

            client.DownloadFileCompleted += (s, e) =>
            {
                activeDownloads.Remove(program.ProgramKey);
                row.Cells["DownloadStatus"].Value = "100.00";
            };

            try
            {
                client.DownloadFileAsync(new Uri(program.LatestVersion), targetPath); // Assume LatestVersion contains URL
            }
            catch
            {
                row.Cells["DownloadStatus"].Value = "Error";
            }
        }

        private void PauseDownload(string programKey)
        {
            if (activeDownloads.TryGetValue(programKey, out var client))
            {
                client.CancelAsync();
                activeDownloads.Remove(programKey);
            }
        }

        private void ResumeDownload(GridClass program, DataGridViewRow row)
        {
            StartDownload(program, row);
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
