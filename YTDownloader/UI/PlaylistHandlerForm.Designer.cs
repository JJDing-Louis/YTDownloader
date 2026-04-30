namespace YTDownloader
{
    partial class PlaylistHandlerForm
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
            groupBox1 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            btn_Download = new Button();
            btn_Cancel = new Button();
            groupBox2 = new GroupBox();
            dGV_PlayList = new DataGridView();
            colSelected = new DataGridViewCheckBoxColumn();
            colIndex = new DataGridViewTextBoxColumn();
            colId = new DataGridViewTextBoxColumn();
            colTitle = new DataGridViewTextBoxColumn();
            colUploader = new DataGridViewTextBoxColumn();
            colDuration = new DataGridViewTextBoxColumn();
            colURL = new DataGridViewTextBoxColumn();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dGV_PlayList).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(groupBox2, 0, 1);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel1.Size = new Size(800, 450);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tableLayoutPanel2);
            groupBox1.Dock = DockStyle.Fill;
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(794, 61);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "操作";
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 10;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            tableLayoutPanel2.Controls.Add(btn_Download, 0, 0);
            tableLayoutPanel2.Controls.Add(btn_Cancel, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 19);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(788, 39);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // btn_Download
            // 
            btn_Download.Dock = DockStyle.Fill;
            btn_Download.Location = new Point(3, 3);
            btn_Download.Name = "btn_Download";
            btn_Download.Size = new Size(72, 33);
            btn_Download.TabIndex = 0;
            btn_Download.Text = "下載";
            btn_Download.UseVisualStyleBackColor = true;
            btn_Download.Click += btn_Download_Click;
            // 
            // btn_Cancel
            // 
            btn_Cancel.Dock = DockStyle.Fill;
            btn_Cancel.Location = new Point(81, 3);
            btn_Cancel.Name = "btn_Cancel";
            btn_Cancel.Size = new Size(72, 33);
            btn_Cancel.TabIndex = 1;
            btn_Cancel.Text = "取消";
            btn_Cancel.UseVisualStyleBackColor = true;
            btn_Cancel.Click += btn_Cancel_Click;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(dGV_PlayList);
            groupBox2.Dock = DockStyle.Fill;
            groupBox2.Location = new Point(3, 70);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(794, 377);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "清單";
            // 
            // dGV_PlayList
            // 
            dGV_PlayList.AllowUserToAddRows = false;
            dGV_PlayList.AllowUserToDeleteRows = false;
            dGV_PlayList.ColumnHeadersHeight = 30;
            dGV_PlayList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dGV_PlayList.Columns.AddRange(new DataGridViewColumn[] { colSelected, colIndex, colId, colTitle, colUploader, colDuration, colURL });
            dGV_PlayList.Dock = DockStyle.Fill;
            dGV_PlayList.Location = new Point(3, 19);
            dGV_PlayList.Name = "dGV_PlayList";
            dGV_PlayList.RowHeadersVisible = false;
            dGV_PlayList.Size = new Size(788, 355);
            dGV_PlayList.TabIndex = 0;
            dGV_PlayList.CellPainting += PlayList_CellPainting;
            dGV_PlayList.CellValueChanged += PlayList_CellValueChanged;
            dGV_PlayList.ColumnHeaderMouseClick += PlayList_ColumnHeaderMouseClick;
            dGV_PlayList.CurrentCellDirtyStateChanged += PlayList_CurrentCellDirtyStateChanged;
            // 
            // colSelected
            // 
            colSelected.HeaderText = "";
            colSelected.Name = "colSelected";
            colSelected.Resizable = DataGridViewTriState.False;
            colSelected.Width = 76;
            // 
            // colIndex
            // 
            colIndex.HeaderText = "#";
            colIndex.Name = "colIndex";
            colIndex.ReadOnly = true;
            colIndex.Width = 45;
            // 
            // colId
            // 
            colId.HeaderText = "Id";
            colId.Name = "colId";
            colId.ReadOnly = true;
            colId.Visible = false;
            colId.Width = 45;
            // 
            // colTitle
            // 
            colTitle.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTitle.HeaderText = "標題";
            colTitle.Name = "colTitle";
            colTitle.ReadOnly = true;
            // 
            // colUploader
            // 
            colUploader.HeaderText = "頻道";
            colUploader.Name = "colUploader";
            colUploader.ReadOnly = true;
            colUploader.Width = 150;
            // 
            // colDuration
            // 
            colDuration.HeaderText = "時長";
            colDuration.Name = "colDuration";
            colDuration.ReadOnly = true;
            colDuration.Width = 80;
            // 
            // colURL
            // 
            colURL.HeaderText = "連結";
            colURL.Name = "colURL";
            colURL.ReadOnly = true;
            colURL.Visible = false;
            colURL.Width = 80;
            // 
            // PlaylistHandlerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "PlaylistHandlerForm";
            Text = "播放清單處理器";
            tableLayoutPanel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dGV_PlayList).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBox1;
        private TableLayoutPanel tableLayoutPanel2;
        private Button btn_Download;
        private Button btn_Cancel;
        private GroupBox groupBox2;
        private DataGridView dGV_PlayList;
        private DataGridViewCheckBoxColumn colSelected;
        private DataGridViewTextBoxColumn colIndex;
        private DataGridViewTextBoxColumn colId;
        private DataGridViewTextBoxColumn colTitle;
        private DataGridViewTextBoxColumn colUploader;
        private DataGridViewTextBoxColumn colDuration;
        private DataGridViewTextBoxColumn colURL;
    }
}
