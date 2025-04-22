namespace WPT_Updater
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            button1 = new Button();
            kebabMenuStrip = new ContextMenuStrip(components);
            Scan_New_Apps = new ToolStripMenuItem();
            Hidden_Apps = new ToolStripMenuItem();
            Select_account = new ToolStripMenuItem();
            Download_Path = new ToolStripMenuItem();
            Filters = new ToolStripMenuItem();
            panel1 = new Panel();
            textBox1 = new TextBox();
            tabPage1 = new TabPage();
            dataGridViewInstalled = new DataGridView();
            tabPage2 = new TabPage();
            dataGridViewUpdates = new DataGridView();
            tabControl1 = new TabControl();
            contextMenuStripInstalled = new ContextMenuStrip(components);
            contextMenuStripUpdates = new ContextMenuStrip(components);
            kebabMenuStrip.SuspendLayout();
            panel1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewInstalled).BeginInit();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewUpdates).BeginInit();
            tabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.ContextMenuStrip = kebabMenuStrip;
            button1.Location = new Point(1219, 0);
            button1.Name = "button1";
            button1.Size = new Size(31, 22);
            button1.TabIndex = 2;
            button1.Text = "⋮";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // kebabMenuStrip
            // 
            kebabMenuStrip.Items.AddRange(new ToolStripItem[] { Scan_New_Apps, Hidden_Apps, Select_account, Download_Path, Filters });
            kebabMenuStrip.Name = "kebabMenuStrip";
            kebabMenuStrip.Size = new Size(173, 114);
            kebabMenuStrip.Opening += kebabMenuStrip_Opening;
            // 
            // Scan_New_Apps
            // 
            Scan_New_Apps.Name = "Scan_New_Apps";
            Scan_New_Apps.Size = new Size(172, 22);
            Scan_New_Apps.Text = "Scan for new Apps";
            // 
            // Hidden_Apps
            // 
            Hidden_Apps.Name = "Hidden_Apps";
            Hidden_Apps.Size = new Size(172, 22);
            Hidden_Apps.Text = "Hidden apps";
            // 
            // Select_account
            // 
            Select_account.Name = "Select_account";
            Select_account.Size = new Size(172, 22);
            Select_account.Text = "Select Account";
            // 
            // Download_Path
            // 
            Download_Path.Name = "Download_Path";
            Download_Path.Size = new Size(172, 22);
            Download_Path.Text = "Download Path";
            // 
            // Filters
            // 
            Filters.Name = "Filters";
            Filters.Size = new Size(172, 22);
            Filters.Text = "Filters";
            // 
            // panel1
            // 
            panel1.AccessibleName = "btnKebabMenu";
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(button1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1250, 22);
            panel1.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.Cursor = Cursors.IBeam;
            textBox1.Location = new Point(1060, 3);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "⌕ search";
            textBox1.Size = new Size(153, 16);
            textBox1.TabIndex = 3;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dataGridViewInstalled);
            tabPage1.Location = new Point(4, 32);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1242, 516);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Installed";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += tabPage1_Click;
            // 
            // dataGridViewInstalled
            // 
            dataGridViewInstalled.AllowUserToOrderColumns = true;
            dataGridViewInstalled.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewInstalled.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewInstalled.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewInstalled.Location = new Point(0, 0);
            dataGridViewInstalled.Name = "dataGridViewInstalled";
            dataGridViewInstalled.Size = new Size(1242, 514);
            dataGridViewInstalled.TabIndex = 2;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridViewUpdates);
            tabPage2.Location = new Point(4, 32);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1242, 516);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Updates";
            tabPage2.UseVisualStyleBackColor = true;
            tabPage2.Click += tabPage2_Click;
            // 
            // dataGridViewUpdates
            // 
            dataGridViewUpdates.AllowUserToOrderColumns = true;
            dataGridViewUpdates.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewUpdates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewUpdates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewUpdates.Location = new Point(0, 0);
            dataGridViewUpdates.Name = "dataGridViewUpdates";
            dataGridViewUpdates.Size = new Size(1242, 518);
            dataGridViewUpdates.TabIndex = 1;
            dataGridViewUpdates.CellContentClick += dataGridView1_CellContentClick;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Font = new Font("Segoe UI", 10F);
            tabControl1.Location = new Point(0, 22);
            tabControl1.Name = "tabControl1";
            tabControl1.Padding = new Point(20, 6);
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1250, 552);
            tabControl1.TabIndex = 0;
            // 
            // contextMenuStripInstalled
            // 
            contextMenuStripInstalled.Name = "contextMenuStripInstalled";
            contextMenuStripInstalled.Size = new Size(61, 4);
            // 
            // contextMenuStripUpdates
            // 
            contextMenuStripUpdates.Name = "contextMenuStripUpdates";
            contextMenuStripUpdates.Size = new Size(61, 4);
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1250, 574);
            Controls.Add(tabControl1);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "WPT-Updater";
            Load += Form1_Load;
            kebabMenuStrip.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewInstalled).EndInit();
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewUpdates).EndInit();
            tabControl1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private Panel panel1;
        private ContextMenuStrip kebabMenuStrip;
        private ToolStripMenuItem Scan_New_Apps;
        private ToolStripMenuItem Hidden_Apps;
        private ToolStripMenuItem Select_account;
        private ToolStripMenuItem Download_Path;
        private TextBox textBox1;
        private ToolStripMenuItem Filters;
        private TabPage tabPage1;
        private DataGridView dataGridViewInstalled;
        private TabPage tabPage2;
        private DataGridView dataGridViewUpdates;
        private TabControl tabControl1;
        private ContextMenuStrip contextMenuStripInstalled;
        private ContextMenuStrip contextMenuStripUpdates;
    }
}
