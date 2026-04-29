namespace YTDownloader
{
    partial class DownloadHistoryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            cB_FileName = new CheckBox();
            textBox1 = new TextBox();
            cB_DownloadDate = new CheckBox();
            cB_DownloadResult = new CheckBox();
            cBO_DownloadResult = new ComboBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            label1 = new Label();
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            btn_Search = new Button();
            cB_MediaType = new CheckBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            cB_Video = new CheckBox();
            cB_Audio = new CheckBox();
            groupBox1 = new GroupBox();
            dGV_SearchResult = new DataGridView();
            tableLayoutPanel4 = new TableLayoutPanel();
            btn_ReDownload = new Button();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dGV_SearchResult).BeginInit();
            tableLayoutPanel4.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 2);
            tableLayoutPanel1.Controls.Add(tableLayoutPanel4, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(1028, 450);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 9.998043F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.16047F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51.8591F));
            tableLayoutPanel2.Controls.Add(cB_FileName, 0, 0);
            tableLayoutPanel2.Controls.Add(textBox1, 1, 0);
            tableLayoutPanel2.Controls.Add(cB_DownloadDate, 0, 1);
            tableLayoutPanel2.Controls.Add(cB_DownloadResult, 0, 2);
            tableLayoutPanel2.Controls.Add(cBO_DownloadResult, 1, 2);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel3, 1, 1);
            tableLayoutPanel2.Controls.Add(btn_Search, 2, 0);
            tableLayoutPanel2.Controls.Add(cB_MediaType, 0, 3);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel5, 1, 3);
            tableLayoutPanel2.Dock = DockStyle.Left;
            tableLayoutPanel2.Location = new Point(3, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 31.6091957F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 22.9885063F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 20.1149426F));
            tableLayoutPanel2.Size = new Size(1022, 174);
            tableLayoutPanel2.TabIndex = 1;
            // 
            // cB_FileName
            // 
            cB_FileName.AutoSize = true;
            cB_FileName.Location = new Point(3, 3);
            cB_FileName.Name = "cB_FileName";
            cB_FileName.Size = new Size(74, 19);
            cB_FileName.TabIndex = 0;
            cB_FileName.Text = "檔案名稱";
            cB_FileName.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(105, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(383, 23);
            textBox1.TabIndex = 1;
            // 
            // cB_DownloadDate
            // 
            cB_DownloadDate.AutoSize = true;
            cB_DownloadDate.Dock = DockStyle.Fill;
            cB_DownloadDate.Location = new Point(3, 46);
            cB_DownloadDate.Name = "cB_DownloadDate";
            cB_DownloadDate.Size = new Size(96, 49);
            cB_DownloadDate.TabIndex = 2;
            cB_DownloadDate.Text = "下載日期";
            cB_DownloadDate.UseVisualStyleBackColor = true;
            // 
            // cB_DownloadResult
            // 
            cB_DownloadResult.AutoSize = true;
            cB_DownloadResult.Location = new Point(3, 101);
            cB_DownloadResult.Name = "cB_DownloadResult";
            cB_DownloadResult.Size = new Size(74, 19);
            cB_DownloadResult.TabIndex = 4;
            cB_DownloadResult.Text = "下載結果";
            cB_DownloadResult.UseVisualStyleBackColor = true;
            // 
            // cBO_DownloadResult
            // 
            cBO_DownloadResult.Dock = DockStyle.Fill;
            cBO_DownloadResult.FormattingEnabled = true;
            cBO_DownloadResult.Location = new Point(105, 101);
            cBO_DownloadResult.Name = "cBO_DownloadResult";
            cBO_DownloadResult.Size = new Size(383, 23);
            cBO_DownloadResult.TabIndex = 5;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tableLayoutPanel3.Controls.Add(label1, 1, 0);
            tableLayoutPanel3.Controls.Add(dateTimePicker1, 0, 0);
            tableLayoutPanel3.Controls.Add(dateTimePicker2, 2, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(105, 46);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(383, 49);
            tableLayoutPanel3.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(175, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 49);
            label1.TabIndex = 0;
            label1.Text = "~";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Dock = DockStyle.Fill;
            dateTimePicker1.Location = new Point(3, 3);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(166, 23);
            dateTimePicker1.TabIndex = 1;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Dock = DockStyle.Fill;
            dateTimePicker2.Location = new Point(213, 3);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(167, 23);
            dateTimePicker2.TabIndex = 2;
            // 
            // btn_Search
            // 
            btn_Search.Location = new Point(494, 3);
            btn_Search.Name = "btn_Search";
            btn_Search.Size = new Size(75, 23);
            btn_Search.TabIndex = 7;
            btn_Search.Text = "查詢";
            btn_Search.UseVisualStyleBackColor = true;
            // 
            // cB_MediaType
            // 
            cB_MediaType.AutoSize = true;
            cB_MediaType.Location = new Point(3, 141);
            cB_MediaType.Name = "cB_MediaType";
            cB_MediaType.Size = new Size(74, 19);
            cB_MediaType.TabIndex = 8;
            cB_MediaType.Text = "媒體類型";
            cB_MediaType.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 5;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            tableLayoutPanel5.Controls.Add(cB_Video, 1, 0);
            tableLayoutPanel5.Controls.Add(cB_Audio, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(105, 141);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(383, 30);
            tableLayoutPanel5.TabIndex = 10;
            // 
            // cB_Video
            // 
            cB_Video.AutoSize = true;
            cB_Video.Location = new Point(79, 3);
            cB_Video.Name = "cB_Video";
            cB_Video.Size = new Size(50, 19);
            cB_Video.TabIndex = 10;
            cB_Video.Text = "視訊";
            cB_Video.UseVisualStyleBackColor = true;
            // 
            // cB_Audio
            // 
            cB_Audio.AutoSize = true;
            cB_Audio.Location = new Point(3, 3);
            cB_Audio.Name = "cB_Audio";
            cB_Audio.Size = new Size(50, 19);
            cB_Audio.TabIndex = 9;
            cB_Audio.Text = "音訊";
            cB_Audio.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(dGV_SearchResult);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 228);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1022, 219);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "查詢結果";
            // 
            // dGV_SearchResult
            // 
            dGV_SearchResult.AllowUserToAddRows = false;
            dGV_SearchResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dGV_SearchResult.Dock = DockStyle.Fill;
            dGV_SearchResult.Location = new Point(3, 19);
            dGV_SearchResult.Name = "dGV_SearchResult";
            dGV_SearchResult.Size = new Size(1016, 197);
            dGV_SearchResult.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 8;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5F));
            tableLayoutPanel4.Controls.Add(btn_ReDownload, 0, 0);
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 183);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(1022, 39);
            tableLayoutPanel4.TabIndex = 3;
            // 
            // btn_ReDownload
            // 
            btn_ReDownload.Dock = DockStyle.Fill;
            btn_ReDownload.Location = new Point(3, 3);
            btn_ReDownload.Name = "btn_ReDownload";
            btn_ReDownload.Size = new Size(121, 33);
            btn_ReDownload.TabIndex = 0;
            btn_ReDownload.Text = "重新下載";
            btn_ReDownload.UseVisualStyleBackColor = true;
            // 
            // DownloadHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1028, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "DownloadHistoryForm";
            Text = "下載紀錄";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel2.PerformLayout();
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dGV_SearchResult).EndInit();
            tableLayoutPanel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private CheckBox cB_FileName;
        private TextBox textBox1;
        private CheckBox cB_DownloadDate;
        private CheckBox cB_DownloadResult;
        private ComboBox cBO_DownloadResult;
        private TableLayoutPanel tableLayoutPanel3;
        private Button btn_ReDownload;
        private Label label1;
        private Button btn_Search;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private GroupBox groupBox1;
        private DataGridView dGV_SearchResult;
        private TableLayoutPanel tableLayoutPanel4;
        private CheckBox cB_MediaType;
        private CheckBox cB_Audio;
        private TableLayoutPanel tableLayoutPanel5;
        private CheckBox cB_Video;
    }
}