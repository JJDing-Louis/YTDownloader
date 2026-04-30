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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            cB_FileName = new CheckBox();
            txt_Filename = new TextBox();
            cB_DownloadDate = new CheckBox();
            cB_DownloadResult = new CheckBox();
            cBO_DownloadResult = new ComboBox();
            tableLayoutPanel3 = new TableLayoutPanel();
            label1 = new Label();
            dTP_DownLoadStartDate = new DateTimePicker();
            dTP_DownLoadEndDate = new DateTimePicker();
            btn_Search = new Button();
            cB_MediaType = new CheckBox();
            tableLayoutPanel5 = new TableLayoutPanel();
            cB_Video = new CheckBox();
            cB_Audio = new CheckBox();
            groupBox1 = new GroupBox();
            dGV_SearchResult = new DataGridView();
            colSelect = new DataGridViewCheckBoxColumn();
            colIndex = new DataGridViewTextBoxColumn();
            colFileName = new DataGridViewTextBoxColumn();
            DownloadDateTime = new DataGridViewTextBoxColumn();
            colMediaType = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colTaskId = new DataGridViewTextBoxColumn();
            colTitle = new DataGridViewTextBoxColumn();
            colURL = new DataGridViewTextBoxColumn();
            colPath = new DataGridViewTextBoxColumn();
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
            tableLayoutPanel1.Size = new Size(1028, 750);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 9.998043F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38.16047F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 51.8591F));
            tableLayoutPanel2.Controls.Add(cB_FileName, 0, 0);
            tableLayoutPanel2.Controls.Add(txt_Filename, 1, 0);
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
            tableLayoutPanel2.Size = new Size(1022, 294);
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
            cB_FileName.CheckedChanged += cB_SearchCondition_CheckedChanged;
            // 
            // txt_Filename
            // 
            txt_Filename.Dock = DockStyle.Fill;
            txt_Filename.Enabled = false;
            txt_Filename.Location = new Point(105, 3);
            txt_Filename.Name = "txt_Filename";
            txt_Filename.Size = new Size(383, 23);
            txt_Filename.TabIndex = 1;
            // 
            // cB_DownloadDate
            // 
            cB_DownloadDate.AutoSize = true;
            cB_DownloadDate.Dock = DockStyle.Fill;
            cB_DownloadDate.Location = new Point(3, 76);
            cB_DownloadDate.Name = "cB_DownloadDate";
            cB_DownloadDate.Size = new Size(96, 87);
            cB_DownloadDate.TabIndex = 2;
            cB_DownloadDate.Text = "下載日期";
            cB_DownloadDate.UseVisualStyleBackColor = true;
            cB_DownloadDate.CheckedChanged += cB_SearchCondition_CheckedChanged;
            // 
            // cB_DownloadResult
            // 
            cB_DownloadResult.AutoSize = true;
            cB_DownloadResult.Location = new Point(3, 169);
            cB_DownloadResult.Name = "cB_DownloadResult";
            cB_DownloadResult.Size = new Size(74, 19);
            cB_DownloadResult.TabIndex = 4;
            cB_DownloadResult.Text = "下載結果";
            cB_DownloadResult.UseVisualStyleBackColor = true;
            cB_DownloadResult.CheckedChanged += cB_SearchCondition_CheckedChanged;
            // 
            // cBO_DownloadResult
            // 
            cBO_DownloadResult.Dock = DockStyle.Fill;
            cBO_DownloadResult.Enabled = false;
            cBO_DownloadResult.FormattingEnabled = true;
            cBO_DownloadResult.Location = new Point(105, 169);
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
            tableLayoutPanel3.Controls.Add(dTP_DownLoadStartDate, 0, 0);
            tableLayoutPanel3.Controls.Add(dTP_DownLoadEndDate, 2, 0);
            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Location = new Point(105, 76);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 1;
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel3.Size = new Size(383, 87);
            tableLayoutPanel3.TabIndex = 6;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = DockStyle.Fill;
            label1.Location = new Point(175, 0);
            label1.Name = "label1";
            label1.Size = new Size(32, 87);
            label1.TabIndex = 0;
            label1.Text = "~";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // dTP_DownLoadStartDate
            // 
            dTP_DownLoadStartDate.Dock = DockStyle.Fill;
            dTP_DownLoadStartDate.Enabled = false;
            dTP_DownLoadStartDate.Location = new Point(3, 3);
            dTP_DownLoadStartDate.Name = "dTP_DownLoadStartDate";
            dTP_DownLoadStartDate.Size = new Size(166, 23);
            dTP_DownLoadStartDate.TabIndex = 1;
            // 
            // dTP_DownLoadEndDate
            // 
            dTP_DownLoadEndDate.Dock = DockStyle.Fill;
            dTP_DownLoadEndDate.Enabled = false;
            dTP_DownLoadEndDate.Location = new Point(213, 3);
            dTP_DownLoadEndDate.Name = "dTP_DownLoadEndDate";
            dTP_DownLoadEndDate.Size = new Size(167, 23);
            dTP_DownLoadEndDate.TabIndex = 2;
            // 
            // btn_Search
            // 
            btn_Search.Location = new Point(494, 3);
            btn_Search.Name = "btn_Search";
            btn_Search.Size = new Size(75, 23);
            btn_Search.TabIndex = 7;
            btn_Search.Text = "查詢";
            btn_Search.UseVisualStyleBackColor = true;
            btn_Search.Click += btn_Search_Click;
            // 
            // cB_MediaType
            // 
            cB_MediaType.AutoSize = true;
            cB_MediaType.Location = new Point(3, 236);
            cB_MediaType.Name = "cB_MediaType";
            cB_MediaType.Size = new Size(74, 19);
            cB_MediaType.TabIndex = 8;
            cB_MediaType.Text = "媒體類型";
            cB_MediaType.UseVisualStyleBackColor = true;
            cB_MediaType.CheckedChanged += cB_SearchCondition_CheckedChanged;
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
            tableLayoutPanel5.Location = new Point(105, 236);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 1;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(383, 55);
            tableLayoutPanel5.TabIndex = 10;
            // 
            // cB_Video
            // 
            cB_Video.AutoSize = true;
            cB_Video.Enabled = false;
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
            cB_Audio.Enabled = false;
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
            groupBox1.Location = new Point(3, 378);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1022, 369);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "查詢結果";
            // 
            // dGV_SearchResult
            // 
            dGV_SearchResult.AllowUserToAddRows = false;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Microsoft JhengHei UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.False;
            dGV_SearchResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dGV_SearchResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dGV_SearchResult.Columns.AddRange(new DataGridViewColumn[] { colSelect, colIndex, colFileName, DownloadDateTime, colMediaType, colStatus, colTaskId, colTitle, colURL, colPath });
            dGV_SearchResult.Dock = DockStyle.Fill;
            dGV_SearchResult.Location = new Point(3, 19);
            dGV_SearchResult.Name = "dGV_SearchResult";
            dGV_SearchResult.RowHeadersVisible = false;
            dGV_SearchResult.Size = new Size(1016, 347);
            dGV_SearchResult.TabIndex = 0;
            dGV_SearchResult.CellPainting += SearchResult_CellPainting;
            dGV_SearchResult.CellValueChanged += SearchResult_CellValueChanged;
            dGV_SearchResult.ColumnHeaderMouseClick += SearchResult_ColumnHeaderMouseClick;
            dGV_SearchResult.CurrentCellDirtyStateChanged += SearchResult_CurrentCellDirtyStateChanged;
            // 
            // colSelect
            // 
            colSelect.HeaderText = "";
            colSelect.Name = "colSelect";
            colSelect.Resizable = DataGridViewTriState.False;
            colSelect.Width = 76;
            // 
            // colIndex
            // 
            colIndex.HeaderText = "#";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.Resizable = DataGridViewTriState.False;
            colIndex.Width = 40;
            // 
            // colFileName
            // 
            colFileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colFileName.HeaderText = "檔名";
            colFileName.Name = "colFileName";
            colFileName.ReadOnly = true;
            // 
            // DownloadDateTime
            // 
            DownloadDateTime.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DownloadDateTime.HeaderText = "下載日期";
            DownloadDateTime.Name = "DownloadDateTime";
            DownloadDateTime.ReadOnly = true;
            // 
            // colMediaType
            // 
            colMediaType.HeaderText = "類型";
            colMediaType.Name = "colMediaType";
            colMediaType.ReadOnly = true;
            colMediaType.Resizable = DataGridViewTriState.False;
            colMediaType.Width = 80;
            // 
            // colStatus
            // 
            colStatus.HeaderText = "狀態";
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            colStatus.Width = 200;
            // 
            // colTaskId
            // 
            colTaskId.HeaderText = "TaskId";
            colTaskId.Name = "colTaskId";
            colTaskId.Visible = false;
            // 
            // colTitle
            // 
            colTitle.HeaderText = "Title";
            colTitle.Name = "colTitle";
            colTitle.Visible = false;
            // 
            // colURL
            // 
            colURL.HeaderText = "URL";
            colURL.Name = "colURL";
            colURL.Visible = false;
            // 
            // colPath
            // 
            colPath.HeaderText = "Path";
            colPath.Name = "colPath";
            colPath.Visible = false;
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
            tableLayoutPanel4.Location = new Point(3, 303);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(1022, 69);
            tableLayoutPanel4.TabIndex = 3;
            // 
            // btn_ReDownload
            // 
            btn_ReDownload.Dock = DockStyle.Fill;
            btn_ReDownload.Location = new Point(3, 3);
            btn_ReDownload.Name = "btn_ReDownload";
            btn_ReDownload.Size = new Size(121, 63);
            btn_ReDownload.TabIndex = 0;
            btn_ReDownload.Text = "重新下載";
            btn_ReDownload.UseVisualStyleBackColor = true;
            btn_ReDownload.Click += btn_ReDownload_Click;
            // 
            // DownloadHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1028, 750);
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

        private void ConfigureSearchLayout()
        {
            tableLayoutPanel2.Padding = new Padding(0, 4, 0, 0);
            var searchButtonSize = GetButtonSize(btn_Search, 96, 32);
            var reDownloadButtonSize = GetButtonSize(btn_ReDownload, 120, searchButtonSize.Height);
            reDownloadButtonSize.Height = searchButtonSize.Height;
            var criteriaRowHeight = Math.Max(48, Font.Height + 24);
            var searchPanelHeight = tableLayoutPanel2.Padding.Top + criteriaRowHeight * tableLayoutPanel2.RowCount + 8;
            var actionRowHeight = reDownloadButtonSize.Height + 16;

            tableLayoutPanel1.RowStyles[0].SizeType = SizeType.Absolute;
            tableLayoutPanel1.RowStyles[0].Height = searchPanelHeight;
            tableLayoutPanel1.RowStyles[1].SizeType = SizeType.Absolute;
            tableLayoutPanel1.RowStyles[1].Height = actionRowHeight;
            tableLayoutPanel1.RowStyles[2].SizeType = SizeType.Percent;
            tableLayoutPanel1.RowStyles[2].Height = 100;

            tableLayoutPanel2.Dock = DockStyle.Top;
            tableLayoutPanel2.Height = searchPanelHeight;
            tableLayoutPanel2.ColumnStyles[0].SizeType = SizeType.Absolute;
            tableLayoutPanel2.ColumnStyles[0].Width = Math.Max(136, TextRenderer.MeasureText("下載結果", Font).Width + 34);
            tableLayoutPanel2.ColumnStyles[1].SizeType = SizeType.Absolute;
            tableLayoutPanel2.ColumnStyles[1].Width = 560;
            tableLayoutPanel2.ColumnStyles[2].SizeType = SizeType.Absolute;
            tableLayoutPanel2.ColumnStyles[2].Width = Math.Max(130, TextRenderer.MeasureText("查詢", Font).Width + 58);

            foreach (RowStyle rowStyle in tableLayoutPanel2.RowStyles)
            {
                rowStyle.SizeType = SizeType.Absolute;
                rowStyle.Height = criteriaRowHeight;
            }

            ConfigureCriteriaCheckBox(cB_FileName);
            ConfigureCriteriaCheckBox(cB_DownloadDate);
            ConfigureCriteriaCheckBox(cB_DownloadResult);
            ConfigureCriteriaCheckBox(cB_MediaType);

            ConfigureInputControl(txt_Filename);
            ConfigureInputControl(cBO_DownloadResult);

            tableLayoutPanel3.Dock = DockStyle.Fill;
            tableLayoutPanel3.Margin = new Padding(0);
            tableLayoutPanel3.ColumnStyles[0].SizeType = SizeType.Absolute;
            tableLayoutPanel3.ColumnStyles[0].Width =
                Math.Max(230, TextRenderer.MeasureText("2026年  4月30日", Font).Width + 64);
            tableLayoutPanel3.ColumnStyles[1].SizeType = SizeType.Absolute;
            tableLayoutPanel3.ColumnStyles[1].Width = 48;
            tableLayoutPanel3.ColumnStyles[2].SizeType = SizeType.Absolute;
            tableLayoutPanel3.ColumnStyles[2].Width = tableLayoutPanel3.ColumnStyles[0].Width;
            dTP_DownLoadStartDate.Dock = DockStyle.None;
            dTP_DownLoadStartDate.Anchor = AnchorStyles.Left;
            dTP_DownLoadStartDate.Width = (int)tableLayoutPanel3.ColumnStyles[0].Width;
            dTP_DownLoadStartDate.Height = dTP_DownLoadStartDate.PreferredSize.Height;
            dTP_DownLoadEndDate.Dock = DockStyle.None;
            dTP_DownLoadEndDate.Anchor = AnchorStyles.Left;
            dTP_DownLoadEndDate.Width = (int)tableLayoutPanel3.ColumnStyles[2].Width;
            dTP_DownLoadEndDate.Height = dTP_DownLoadEndDate.PreferredSize.Height;
            label1.Dock = DockStyle.Fill;
            label1.TextAlign = ContentAlignment.MiddleCenter;

            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Margin = new Padding(0, 0, 0, 0);
            cB_Audio.AutoSize = true;
            cB_Audio.Anchor = AnchorStyles.Left;
            cB_Video.AutoSize = true;
            cB_Video.Anchor = AnchorStyles.Left;

            btn_Search.Anchor = AnchorStyles.Left;
            btn_Search.Margin = new Padding(10, 0, 0, 0);
            btn_Search.Size = searchButtonSize;

            btn_ReDownload.Dock = DockStyle.None;
            btn_ReDownload.Anchor = AnchorStyles.Left;
            btn_ReDownload.Margin = new Padding(0);
            btn_ReDownload.Size = reDownloadButtonSize;
        }

        private void LockWindowSize()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = Size;
            MaximumSize = Size;
        }

        private static Size GetButtonSize(Button button, int minWidth, int minHeight)
        {
            var preferredSize = button.GetPreferredSize(Size.Empty);
            return new Size(
                Math.Max(minWidth, preferredSize.Width + 12),
                Math.Max(minHeight, preferredSize.Height + 6));
        }

        private static void ConfigureCriteriaCheckBox(CheckBox checkBox)
        {
            checkBox.AutoSize = false;
            checkBox.Dock = DockStyle.Fill;
            checkBox.Margin = new Padding(0);
            checkBox.TextAlign = ContentAlignment.MiddleLeft;
        }

        private static void ConfigureInputControl(Control control)
        {
            control.Dock = DockStyle.None;
            control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            control.Margin = new Padding(0, 0, 0, 0);
            control.Height = control.PreferredSize.Height;
        }

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private CheckBox cB_FileName;
        private System.Windows.Forms.TextBox txt_Filename;
        private CheckBox cB_DownloadDate;
        private CheckBox cB_DownloadResult;
        private System.Windows.Forms.ComboBox cBO_DownloadResult;
        private TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btn_ReDownload;
        private Label label1;
        private System.Windows.Forms.Button btn_Search;
        private System.Windows.Forms.DateTimePicker dTP_DownLoadStartDate;
        private System.Windows.Forms.DateTimePicker dTP_DownLoadEndDate;
        private GroupBox groupBox1;
        private DataGridView dGV_SearchResult;
        private DataGridViewCheckBoxColumn colSelect;
        private DataGridViewTextBoxColumn colIndex;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn DownloadDateTime;
        private DataGridViewTextBoxColumn colMediaType;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewTextBoxColumn colTaskId;
        private DataGridViewTextBoxColumn colTitle;
        private DataGridViewTextBoxColumn colURL;
        private DataGridViewTextBoxColumn colPath;
        private TableLayoutPanel tableLayoutPanel4;
        private CheckBox cB_MediaType;
        private CheckBox cB_Audio;
        private TableLayoutPanel tableLayoutPanel5;
        private CheckBox cB_Video;
    }
}
