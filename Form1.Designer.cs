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
            tabControl1 = new TabControl();
            tabPage2 = new TabPage();
            dataGridView1 = new DataGridView();
            tabPage3 = new TabPage();
            dataGridView3 = new DataGridView();
            tabPage1 = new TabPage();
            dataGridView2 = new DataGridView();
            button1 = new Button();
            kebabMenuStrip = new ContextMenuStrip(components);
            Scan_New_Apps = new ToolStripMenuItem();
            Hidden_Apps = new ToolStripMenuItem();
            Select_account = new ToolStripMenuItem();
            Download_Path = new ToolStripMenuItem();
            Filters = new ToolStripMenuItem();
            panel1 = new Panel();
            textBox1 = new TextBox();
            tabControl1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView3).BeginInit();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView2).BeginInit();
            kebabMenuStrip.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
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
            // tabPage2
            // 
            tabPage2.Controls.Add(dataGridView1);
            tabPage2.Location = new Point(4, 32);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1242, 516);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Updates";
            tabPage2.UseVisualStyleBackColor = true;
            tabPage2.Click += tabPage2_Click;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(1242, 518);
            dataGridView1.TabIndex = 1;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(dataGridView3);
            tabPage3.Location = new Point(4, 32);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1242, 516);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Betas";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // dataGridView3
            // 
            dataGridView3.AllowUserToOrderColumns = true;
            dataGridView3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView3.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView3.Location = new Point(0, 0);
            dataGridView3.Name = "dataGridView3";
            dataGridView3.Size = new Size(1242, 510);
            dataGridView3.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dataGridView2);
            tabPage1.Location = new Point(4, 32);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1242, 516);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Installed";
            tabPage1.UseVisualStyleBackColor = true;
            tabPage1.Click += tabPage1_Click;
            // 
            // dataGridView2
            // 
            dataGridView2.AllowUserToOrderColumns = true;
            dataGridView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView2.Location = new Point(0, 0);
            dataGridView2.Name = "dataGridView2";
            dataGridView2.Size = new Size(1242, 514);
            dataGridView2.TabIndex = 2;
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
            tabControl1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView3).EndInit();
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView2).EndInit();
            kebabMenuStrip.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private DataGridView dataGridView1;
        private Button button1;
        private Panel panel1;
        private DataGridView dataGridView3;
        private DataGridView dataGridView2;
        private ContextMenuStrip kebabMenuStrip;
        private ToolStripMenuItem Scan_New_Apps;
        private ToolStripMenuItem Hidden_Apps;
        private ToolStripMenuItem Select_account;
        private ToolStripMenuItem Download_Path;
        private TextBox textBox1;
        private ToolStripMenuItem Filters;
    }
}
