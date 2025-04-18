using System;
using System.Collections.Generic;
using System.Linq;  // For LINQ filtering
using System.Windows.Forms;
using System.Data.SQLite;
using Dapper;

namespace WPT_Updater
{
    public partial class Form1 : Form
    {
        private List<GridClass> allPrograms = new(); // Store the full list from database

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            LoadGridData();
            dataGridView1.CellMouseClick += dataGridView1_CellMouseClick; // Hook right-click event
            await Launch.Start();
        }

        private void LoadGridData()
        {
            allPrograms = GetInstalledProgramsFromDatabase();
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = allPrograms;
        }

        private List<GridClass> GetInstalledProgramsFromDatabase()
        {
            var programs = new List<GridClass>();
            string connectionString = "Data Source="+AppData.DbPath;

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                var result = connection.Query<GridClass>("SELECT * FROM Programs").ToList();
                programs.AddRange(result);
            }

            return programs;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBox1.Text.ToLower();

            var filtered = allPrograms
                .Where(p => p.ProgramName != null && p.ProgramName.ToLower().Contains(searchText))
                .ToList();

            dataGridView1.DataSource = filtered;
        }

        private void dataGridView1_CellMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip menu = new ContextMenuStrip();

                if (e.RowIndex == -1 && e.ColumnIndex >= 0) // Column header
                {
                    var hideCol = new ToolStripMenuItem("Hide Column");
                    hideCol.Click += (s, ev) => dataGridView1.Columns[e.ColumnIndex].Visible = false;

                    var showAllCols = new ToolStripMenuItem("Show All Columns");
                    showAllCols.Click += (s, ev) =>
                    {
                        foreach (DataGridViewColumn col in dataGridView1.Columns)
                            col.Visible = true;
                    };

                    menu.Items.Add(hideCol);
                    menu.Items.Add(showAllCols);

                    var headerCell = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, -1, true);
                    var dropPoint = dataGridView1.PointToScreen(new Point(headerCell.Left, headerCell.Bottom));
                    menu.Show(dropPoint);
                }
                else if (e.ColumnIndex == -1 && e.RowIndex >= 0) // Row header
                {
                    var hideRow = new ToolStripMenuItem("Hide Row");
                    hideRow.Click += (s, ev) => dataGridView1.Rows[e.RowIndex].Visible = false;

                    var showAllRows = new ToolStripMenuItem("Show All Rows");
                    showAllRows.Click += (s, ev) =>
                    {
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                            row.Visible = true;
                    };

                    menu.Items.Add(hideRow);
                    menu.Items.Add(showAllRows);

                    var rowCell = dataGridView1.GetCellDisplayRectangle(-1, e.RowIndex, true);
                    var dropPoint = dataGridView1.PointToScreen(new Point(rowCell.Right, rowCell.Top));
                    menu.Show(dropPoint);
                }
            }
        }

        private void tabPage1_Click(object sender, EventArgs e) { }
        private void tabPage2_Click(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void button1_Click_1(object sender, EventArgs e)
        {
            kebabMenuStrip.Show(button1, new Point(0, button1.Height));
        }

        private void kebabMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e) { }
    }
}
