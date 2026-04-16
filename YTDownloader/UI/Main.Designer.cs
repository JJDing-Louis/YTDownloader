namespace YTDownloader
{
    partial class Main
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
            menuStrip1 = new MenuStrip();
            MSItem_Config = new ToolStripMenuItem();
            MSItem_DownloadHistory = new ToolStripMenuItem();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            btn_Download = new Button();
            btn_OpenDownloadForder = new Button();
            gB_ListMediaType = new GroupBox();
            cB_ListMediaType = new ComboBox();
            groupBox4 = new GroupBox();
            tB_URL = new TextBox();
            groupBox1 = new GroupBox();
            dataGridView1 = new DataGridView();
            tableLayoutPanel3 = new TableLayoutPanel();
            btn_ClearCompleteTask = new Button();
            btn_CancelAll = new Button();
            menuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            gB_ListMediaType.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            tableLayoutPanel3.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { MSItem_Config, MSItem_DownloadHistory });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(958, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // MSItem_Config
            // 
            MSItem_Config.Name = "MSItem_Config";
            MSItem_Config.Size = new Size(43, 20);
            MSItem_Config.Text = "設定";
            // 
            // MSItem_DownloadHistory
            // 
            MSItem_DownloadHistory.Name = "MSItem_DownloadHistory";
            MSItem_DownloadHistory.Size = new Size(67, 20);
            MSItem_DownloadHistory.Text = "下載紀錄";
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel3, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 24);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.Size = new Size(958, 611);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 4;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.Controls.Add(btn_Download, 2, 0);
            tableLayoutPanel2.Controls.Add(btn_OpenDownloadForder, 3, 0);
            tableLayoutPanel2.Controls.Add(gB_ListMediaType, 0, 0);
            tableLayoutPanel2.Controls.Add(groupBox4, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new Size(952, 55);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // btn_Download
            // 
            btn_Download.Dock = DockStyle.Fill;
            btn_Download.Location = new Point(763, 3);
            btn_Download.Name = "btn_Download";
            btn_Download.Size = new Size(89, 49);
            btn_Download.TabIndex = 2;
            btn_Download.Text = "下載";
            btn_Download.UseVisualStyleBackColor = true;
            btn_Download.Click += btn_Download_Click;
            // 
            // btn_OpenDownloadForder
            // 
            btn_OpenDownloadForder.Dock = DockStyle.Fill;
            btn_OpenDownloadForder.Location = new Point(858, 3);
            btn_OpenDownloadForder.Name = "btn_OpenDownloadForder";
            btn_OpenDownloadForder.Size = new Size(91, 49);
            btn_OpenDownloadForder.TabIndex = 3;
            btn_OpenDownloadForder.Text = "開啟資料夾";
            btn_OpenDownloadForder.UseVisualStyleBackColor = true;
            // 
            // gB_ListMediaType
            // 
            gB_ListMediaType.Controls.Add(cB_ListMediaType);
            gB_ListMediaType.Dock = DockStyle.Fill;
            gB_ListMediaType.Location = new Point(3, 3);
            gB_ListMediaType.Name = "gB_ListMediaType";
            gB_ListMediaType.Size = new Size(136, 49);
            gB_ListMediaType.TabIndex = 4;
            gB_ListMediaType.TabStop = false;
            gB_ListMediaType.Text = "媒體類型";
            // 
            // cB_ListMediaType
            // 
            cB_ListMediaType.Dock = DockStyle.Fill;
            cB_ListMediaType.FormattingEnabled = true;
            cB_ListMediaType.Location = new Point(3, 19);
            cB_ListMediaType.Name = "cB_ListMediaType";
            cB_ListMediaType.Size = new Size(130, 23);
            cB_ListMediaType.TabIndex = 0;
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(tB_URL);
            groupBox4.Dock = DockStyle.Fill;
            groupBox4.Location = new Point(145, 3);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(612, 49);
            groupBox4.TabIndex = 6;
            groupBox4.TabStop = false;
            groupBox4.Text = "網址";
            // 
            // tB_URL
            // 
            tB_URL.Dock = DockStyle.Fill;
            tB_URL.Location = new Point(3, 19);
            tB_URL.Name = "tB_URL";
            tB_URL.Size = new Size(606, 23);
            tB_URL.TabIndex = 0;
            tB_URL.Text = "https://www.youtube.com/playlist?list=PLrEnWoR732-BHrPp_Pm8_VleD68f9s14-";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dataGridView1);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 125);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(952, 483);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "下載清單";
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(3, 19);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(946, 461);
            dataGridView1.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 10;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.Controls.Add(btn_ClearCompleteTask, 0, 0);
            tableLayoutPanel3.Controls.Add(btn_CancelAll, 1, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(3, 64);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(952, 55);
            tableLayoutPanel3.TabIndex = 2;
            // 
            // btn_ClearCompleteTask
            // 
            btn_ClearCompleteTask.Dock = DockStyle.Fill;
            btn_ClearCompleteTask.Location = new Point(3, 3);
            btn_ClearCompleteTask.Name = "btn_ClearCompleteTask";
            btn_ClearCompleteTask.Size = new Size(89, 49);
            btn_ClearCompleteTask.TabIndex = 0;
            btn_ClearCompleteTask.Text = "清空已完成";
            btn_ClearCompleteTask.UseVisualStyleBackColor = true;
            // 
            // btn_CancelAll
            // 
            btn_CancelAll.Dock = DockStyle.Fill;
            btn_CancelAll.Location = new Point(98, 3);
            btn_CancelAll.Name = "btn_CancelAll";
            btn_CancelAll.Size = new Size(89, 49);
            btn_CancelAll.TabIndex = 1;
            btn_CancelAll.Text = "全部取消";
            btn_CancelAll.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(958, 635);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(menuStrip1);
            Name = "Main";
            Text = "Youtube下載器";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            gB_ListMediaType.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            tableLayoutPanel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem MSItem_Config;
        private ToolStripMenuItem MSItem_DownloadHistory;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btn_Download;
        private Button btn_OpenDownloadForder;
        private GroupBox groupBox1;
        private DataGridView dataGridView1;
        private GroupBox gB_ListMediaType;
        private ComboBox cB_ListMediaType;
        private GroupBox groupBox4;
        private TextBox tB_URL;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btn_ClearCompleteTask;
        private Button btn_CancelAll;
    }
}
