namespace YTDownloader.UI
{
    partial class ConfigForm
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
            rootLayoutPanel = new TableLayoutPanel();
            _categoryList = new ListBox();
            _contentPanel = new Panel();
            _savePanel = new Panel();
            saveSectionLayoutPanel = new TableLayoutPanel();
            saveGroupBox = new GroupBox();
            saveFieldsLayoutPanel = new TableLayoutPanel();
            savePathLabel = new Label();
            _downloadPathTextBox = new TextBox();
            browseFolderButton = new Button();
            cacheManagementGroupBox = new GroupBox();
            cacheManagementFlowPanel = new FlowLayoutPanel();
            cacheClearButton = new Button();
            historyClearButton = new Button();
            _threadPanel = new Panel();
            threadSectionLayoutPanel = new TableLayoutPanel();
            threadGroupBox = new GroupBox();
            threadFieldsLayoutPanel = new TableLayoutPanel();
            initialThreadLabel = new Label();
            _initialThreadNumeric = new NumericUpDown();
            maxThreadLabel = new Label();
            _maxThreadNumeric = new NumericUpDown();
            _appearancePanel = new Panel();
            appearanceSectionLayoutPanel = new TableLayoutPanel();
            appearanceGroupBox = new GroupBox();
            appearanceFieldsLayoutPanel = new TableLayoutPanel();
            _isDarkModeCheckBox = new CheckBox();
            backColorLabel = new Label();
            _backColorTextBox = new TextBox();
            colorButton = new Button();
            backColorPreviewLabel = new Label();
            _backColorPreview = new Panel();
            backgroundImageLabel = new Label();
            _backgroundImageTextBox = new TextBox();
            browseImageButton = new Button();
            fontGroupBox = new GroupBox();
            fontFieldsLayoutPanel = new TableLayoutPanel();
            fontLabel = new Label();
            _fontComboBox = new ComboBox();
            fontSizeLabel = new Label();
            _fontSizeNumeric = new NumericUpDown();
            _generalPanel = new Panel();
            generalSectionLayoutPanel = new TableLayoutPanel();
            generalGroupBox = new GroupBox();
            generalFieldsLayoutPanel = new TableLayoutPanel();
            languageLabel = new Label();
            _languageComboBox = new ComboBox();
            buttonPanel = new FlowLayoutPanel();
            saveButton = new Button();
            applyButton = new Button();
            cancelButton = new Button();
            rootLayoutPanel.SuspendLayout();
            _contentPanel.SuspendLayout();
            _savePanel.SuspendLayout();
            saveSectionLayoutPanel.SuspendLayout();
            saveGroupBox.SuspendLayout();
            saveFieldsLayoutPanel.SuspendLayout();
            cacheManagementGroupBox.SuspendLayout();
            cacheManagementFlowPanel.SuspendLayout();
            _threadPanel.SuspendLayout();
            threadSectionLayoutPanel.SuspendLayout();
            threadGroupBox.SuspendLayout();
            threadFieldsLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_initialThreadNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_maxThreadNumeric).BeginInit();
            _appearancePanel.SuspendLayout();
            appearanceSectionLayoutPanel.SuspendLayout();
            appearanceGroupBox.SuspendLayout();
            appearanceFieldsLayoutPanel.SuspendLayout();
            fontGroupBox.SuspendLayout();
            fontFieldsLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_fontSizeNumeric).BeginInit();
            _generalPanel.SuspendLayout();
            generalSectionLayoutPanel.SuspendLayout();
            generalGroupBox.SuspendLayout();
            generalFieldsLayoutPanel.SuspendLayout();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayoutPanel
            // 
            rootLayoutPanel.ColumnCount = 2;
            rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayoutPanel.Controls.Add(_categoryList, 0, 0);
            rootLayoutPanel.Controls.Add(_contentPanel, 1, 0);
            rootLayoutPanel.Controls.Add(buttonPanel, 0, 1);
            rootLayoutPanel.Dock = DockStyle.Fill;
            rootLayoutPanel.Location = new Point(0, 0);
            rootLayoutPanel.Name = "rootLayoutPanel";
            rootLayoutPanel.Padding = new Padding(16, 14, 16, 12);
            rootLayoutPanel.RowCount = 2;
            rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayoutPanel.RowStyles.Add(new RowStyle());
            rootLayoutPanel.Size = new Size(860, 460);
            rootLayoutPanel.TabIndex = 0;
            // 
            // _categoryList
            // 
            _categoryList.Dock = DockStyle.Fill;
            _categoryList.FormattingEnabled = true;
            _categoryList.IntegralHeight = false;
            _categoryList.ItemHeight = 15;
            _categoryList.Items.AddRange(new object[] { "一般", "外觀", "下載執行序", "儲存" });
            _categoryList.Location = new Point(19, 17);
            _categoryList.Name = "_categoryList";
            _categoryList.Size = new Size(154, 364);
            _categoryList.TabIndex = 0;
            _categoryList.SelectedIndexChanged += CategoryList_SelectedIndexChanged;
            // 
            // _contentPanel
            // 
            _contentPanel.Controls.Add(_savePanel);
            _contentPanel.Controls.Add(_threadPanel);
            _contentPanel.Controls.Add(_appearancePanel);
            _contentPanel.Controls.Add(_generalPanel);
            _contentPanel.Dock = DockStyle.Fill;
            _contentPanel.Location = new Point(179, 17);
            _contentPanel.Name = "_contentPanel";
            _contentPanel.Padding = new Padding(16, 0, 0, 0);
            _contentPanel.Size = new Size(662, 364);
            _contentPanel.TabIndex = 1;
            // 
            // _savePanel
            // 
            _savePanel.Controls.Add(saveSectionLayoutPanel);
            _savePanel.Dock = DockStyle.Fill;
            _savePanel.Location = new Point(16, 0);
            _savePanel.Name = "_savePanel";
            _savePanel.Size = new Size(646, 364);
            _savePanel.TabIndex = 3;
            // 
            // saveSectionLayoutPanel
            // 
            saveSectionLayoutPanel.ColumnCount = 1;
            saveSectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            saveSectionLayoutPanel.Controls.Add(saveGroupBox, 0, 0);
            saveSectionLayoutPanel.Controls.Add(cacheManagementGroupBox, 0, 1);
            saveSectionLayoutPanel.Dock = DockStyle.Fill;
            saveSectionLayoutPanel.Location = new Point(0, 0);
            saveSectionLayoutPanel.Name = "saveSectionLayoutPanel";
            saveSectionLayoutPanel.RowCount = 4;
            saveSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            saveSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            saveSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            saveSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            saveSectionLayoutPanel.Size = new Size(646, 364);
            saveSectionLayoutPanel.TabIndex = 0;
            // 
            // saveGroupBox
            // 
            saveGroupBox.Controls.Add(saveFieldsLayoutPanel);
            saveGroupBox.Dock = DockStyle.Top;
            saveGroupBox.Location = new Point(3, 3);
            saveGroupBox.Name = "saveGroupBox";
            saveGroupBox.Size = new Size(640, 92);
            saveGroupBox.TabIndex = 0;
            saveGroupBox.TabStop = false;
            saveGroupBox.Text = "儲存";
            // 
            // saveFieldsLayoutPanel
            // 
            saveFieldsLayoutPanel.ColumnCount = 3;
            saveFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            saveFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            saveFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
            saveFieldsLayoutPanel.Controls.Add(savePathLabel, 0, 0);
            saveFieldsLayoutPanel.Controls.Add(_downloadPathTextBox, 1, 0);
            saveFieldsLayoutPanel.Controls.Add(browseFolderButton, 2, 0);
            saveFieldsLayoutPanel.Dock = DockStyle.Fill;
            saveFieldsLayoutPanel.Location = new Point(3, 19);
            saveFieldsLayoutPanel.Name = "saveFieldsLayoutPanel";
            saveFieldsLayoutPanel.Padding = new Padding(12);
            saveFieldsLayoutPanel.RowCount = 1;
            saveFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            saveFieldsLayoutPanel.Size = new Size(634, 70);
            saveFieldsLayoutPanel.TabIndex = 0;
            // 
            // savePathLabel
            // 
            savePathLabel.Dock = DockStyle.Fill;
            savePathLabel.Location = new Point(15, 12);
            savePathLabel.Name = "savePathLabel";
            savePathLabel.Size = new Size(104, 46);
            savePathLabel.TabIndex = 0;
            savePathLabel.Text = "下載資料夾";
            savePathLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _downloadPathTextBox
            // 
            _downloadPathTextBox.Dock = DockStyle.Fill;
            _downloadPathTextBox.Location = new Point(125, 15);
            _downloadPathTextBox.Name = "_downloadPathTextBox";
            _downloadPathTextBox.Size = new Size(418, 23);
            _downloadPathTextBox.TabIndex = 1;
            // 
            // browseFolderButton
            // 
            browseFolderButton.Dock = DockStyle.Fill;
            browseFolderButton.Location = new Point(549, 15);
            browseFolderButton.Name = "browseFolderButton";
            browseFolderButton.Size = new Size(70, 40);
            browseFolderButton.TabIndex = 2;
            browseFolderButton.Text = "...";
            browseFolderButton.UseVisualStyleBackColor = true;
            browseFolderButton.Click += BrowseFolderButton_Click;
            // 
            // cacheManagementGroupBox
            // 
            cacheManagementGroupBox.Controls.Add(cacheManagementFlowPanel);
            cacheManagementGroupBox.Dock = DockStyle.Top;
            cacheManagementGroupBox.Location = new Point(3, 203);
            cacheManagementGroupBox.Name = "cacheManagementGroupBox";
            cacheManagementGroupBox.Size = new Size(640, 92);
            cacheManagementGroupBox.TabIndex = 1;
            cacheManagementGroupBox.TabStop = false;
            cacheManagementGroupBox.Text = "暫存管理";
            // 
            // cacheManagementFlowPanel
            // 
            cacheManagementFlowPanel.Controls.Add(cacheClearButton);
            cacheManagementFlowPanel.Controls.Add(historyClearButton);
            cacheManagementFlowPanel.Dock = DockStyle.Fill;
            cacheManagementFlowPanel.Location = new Point(3, 19);
            cacheManagementFlowPanel.Name = "cacheManagementFlowPanel";
            cacheManagementFlowPanel.Padding = new Padding(12);
            cacheManagementFlowPanel.Size = new Size(634, 70);
            cacheManagementFlowPanel.TabIndex = 0;
            cacheManagementFlowPanel.WrapContents = false;
            // 
            // cacheClearButton
            // 
            cacheClearButton.AutoSize = true;
            cacheClearButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cacheClearButton.Location = new Point(15, 15);
            cacheClearButton.MinimumSize = new Size(100, 32);
            cacheClearButton.Name = "cacheClearButton";
            cacheClearButton.Size = new Size(100, 32);
            cacheClearButton.TabIndex = 0;
            cacheClearButton.Text = "快取清除";
            cacheClearButton.UseVisualStyleBackColor = true;
            cacheClearButton.Click += CacheClearButton_Click;
            // 
            // historyClearButton
            // 
            historyClearButton.AutoSize = true;
            historyClearButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            historyClearButton.Location = new Point(123, 15);
            historyClearButton.Margin = new Padding(8, 3, 3, 3);
            historyClearButton.MinimumSize = new Size(120, 32);
            historyClearButton.Name = "historyClearButton";
            historyClearButton.Size = new Size(120, 32);
            historyClearButton.TabIndex = 1;
            historyClearButton.Text = "歷史紀錄清出";
            historyClearButton.UseVisualStyleBackColor = true;
            historyClearButton.Click += HistoryClearButton_Click;
            // 
            // _threadPanel
            // 
            _threadPanel.Controls.Add(threadSectionLayoutPanel);
            _threadPanel.Dock = DockStyle.Fill;
            _threadPanel.Location = new Point(16, 0);
            _threadPanel.Name = "_threadPanel";
            _threadPanel.Size = new Size(646, 364);
            _threadPanel.TabIndex = 2;
            // 
            // threadSectionLayoutPanel
            // 
            threadSectionLayoutPanel.ColumnCount = 1;
            threadSectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            threadSectionLayoutPanel.Controls.Add(threadGroupBox, 0, 0);
            threadSectionLayoutPanel.Dock = DockStyle.Fill;
            threadSectionLayoutPanel.Location = new Point(0, 0);
            threadSectionLayoutPanel.Name = "threadSectionLayoutPanel";
            threadSectionLayoutPanel.RowCount = 4;
            threadSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            threadSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            threadSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            threadSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            threadSectionLayoutPanel.Size = new Size(646, 364);
            threadSectionLayoutPanel.TabIndex = 0;
            // 
            // threadGroupBox
            // 
            threadGroupBox.Controls.Add(threadFieldsLayoutPanel);
            threadGroupBox.Dock = DockStyle.Top;
            threadGroupBox.Location = new Point(3, 3);
            threadGroupBox.Name = "threadGroupBox";
            threadGroupBox.Size = new Size(640, 128);
            threadGroupBox.TabIndex = 0;
            threadGroupBox.TabStop = false;
            threadGroupBox.Text = "下載執行序";
            // 
            // threadFieldsLayoutPanel
            // 
            threadFieldsLayoutPanel.ColumnCount = 2;
            threadFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            threadFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            threadFieldsLayoutPanel.Controls.Add(initialThreadLabel, 0, 0);
            threadFieldsLayoutPanel.Controls.Add(_initialThreadNumeric, 1, 0);
            threadFieldsLayoutPanel.Controls.Add(maxThreadLabel, 0, 1);
            threadFieldsLayoutPanel.Controls.Add(_maxThreadNumeric, 1, 1);
            threadFieldsLayoutPanel.Dock = DockStyle.Fill;
            threadFieldsLayoutPanel.Location = new Point(3, 19);
            threadFieldsLayoutPanel.Name = "threadFieldsLayoutPanel";
            threadFieldsLayoutPanel.Padding = new Padding(12);
            threadFieldsLayoutPanel.RowCount = 2;
            threadFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            threadFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            threadFieldsLayoutPanel.Size = new Size(634, 106);
            threadFieldsLayoutPanel.TabIndex = 0;
            // 
            // initialThreadLabel
            // 
            initialThreadLabel.Dock = DockStyle.Fill;
            initialThreadLabel.Location = new Point(15, 12);
            initialThreadLabel.Name = "initialThreadLabel";
            initialThreadLabel.Size = new Size(134, 36);
            initialThreadLabel.TabIndex = 0;
            initialThreadLabel.Text = "初始數量";
            initialThreadLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _initialThreadNumeric
            // 
            _initialThreadNumeric.Location = new Point(155, 15);
            _initialThreadNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            _initialThreadNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _initialThreadNumeric.Name = "_initialThreadNumeric";
            _initialThreadNumeric.Size = new Size(80, 23);
            _initialThreadNumeric.TabIndex = 1;
            _initialThreadNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // maxThreadLabel
            // 
            maxThreadLabel.Dock = DockStyle.Fill;
            maxThreadLabel.Location = new Point(15, 48);
            maxThreadLabel.Name = "maxThreadLabel";
            maxThreadLabel.Size = new Size(134, 46);
            maxThreadLabel.TabIndex = 2;
            maxThreadLabel.Text = "最大數量";
            maxThreadLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _maxThreadNumeric
            // 
            _maxThreadNumeric.Location = new Point(155, 51);
            _maxThreadNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
            _maxThreadNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _maxThreadNumeric.Name = "_maxThreadNumeric";
            _maxThreadNumeric.Size = new Size(80, 23);
            _maxThreadNumeric.TabIndex = 3;
            _maxThreadNumeric.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // _appearancePanel
            // 
            _appearancePanel.Controls.Add(appearanceSectionLayoutPanel);
            _appearancePanel.Dock = DockStyle.Fill;
            _appearancePanel.Location = new Point(16, 0);
            _appearancePanel.Name = "_appearancePanel";
            _appearancePanel.Size = new Size(646, 364);
            _appearancePanel.TabIndex = 1;
            // 
            // appearanceSectionLayoutPanel
            // 
            appearanceSectionLayoutPanel.ColumnCount = 1;
            appearanceSectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            appearanceSectionLayoutPanel.Controls.Add(appearanceGroupBox, 0, 0);
            appearanceSectionLayoutPanel.Controls.Add(fontGroupBox, 0, 1);
            appearanceSectionLayoutPanel.Dock = DockStyle.Fill;
            appearanceSectionLayoutPanel.Location = new Point(0, 0);
            appearanceSectionLayoutPanel.Name = "appearanceSectionLayoutPanel";
            appearanceSectionLayoutPanel.RowCount = 4;
            appearanceSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            appearanceSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            appearanceSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            appearanceSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            appearanceSectionLayoutPanel.Size = new Size(646, 364);
            appearanceSectionLayoutPanel.TabIndex = 0;
            // 
            // appearanceGroupBox
            // 
            appearanceGroupBox.Controls.Add(appearanceFieldsLayoutPanel);
            appearanceGroupBox.Dock = DockStyle.Top;
            appearanceGroupBox.Location = new Point(3, 3);
            appearanceGroupBox.Name = "appearanceGroupBox";
            appearanceGroupBox.Size = new Size(640, 180);
            appearanceGroupBox.TabIndex = 0;
            appearanceGroupBox.TabStop = false;
            appearanceGroupBox.Text = "外觀";
            // 
            // appearanceFieldsLayoutPanel
            // 
            appearanceFieldsLayoutPanel.ColumnCount = 3;
            appearanceFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            appearanceFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            appearanceFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
            appearanceFieldsLayoutPanel.Controls.Add(_isDarkModeCheckBox, 1, 0);
            appearanceFieldsLayoutPanel.Controls.Add(backColorLabel, 0, 1);
            appearanceFieldsLayoutPanel.Controls.Add(_backColorTextBox, 1, 1);
            appearanceFieldsLayoutPanel.Controls.Add(colorButton, 2, 1);
            appearanceFieldsLayoutPanel.Controls.Add(backColorPreviewLabel, 0, 2);
            appearanceFieldsLayoutPanel.Controls.Add(_backColorPreview, 1, 2);
            appearanceFieldsLayoutPanel.Controls.Add(backgroundImageLabel, 0, 3);
            appearanceFieldsLayoutPanel.Controls.Add(_backgroundImageTextBox, 1, 3);
            appearanceFieldsLayoutPanel.Controls.Add(browseImageButton, 2, 3);
            appearanceFieldsLayoutPanel.Dock = DockStyle.Fill;
            appearanceFieldsLayoutPanel.Location = new Point(3, 19);
            appearanceFieldsLayoutPanel.Name = "appearanceFieldsLayoutPanel";
            appearanceFieldsLayoutPanel.Padding = new Padding(12, 10, 12, 10);
            appearanceFieldsLayoutPanel.RowCount = 4;
            appearanceFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            appearanceFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            appearanceFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            appearanceFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            appearanceFieldsLayoutPanel.Size = new Size(634, 158);
            appearanceFieldsLayoutPanel.TabIndex = 0;
            // 
            // _isDarkModeCheckBox
            // 
            _isDarkModeCheckBox.AutoSize = true;
            appearanceFieldsLayoutPanel.SetColumnSpan(_isDarkModeCheckBox, 2);
            _isDarkModeCheckBox.Location = new Point(125, 13);
            _isDarkModeCheckBox.Name = "_isDarkModeCheckBox";
            _isDarkModeCheckBox.Size = new Size(98, 19);
            _isDarkModeCheckBox.TabIndex = 0;
            _isDarkModeCheckBox.Text = "啟用黑暗模式";
            _isDarkModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // backColorLabel
            // 
            backColorLabel.Dock = DockStyle.Fill;
            backColorLabel.Location = new Point(15, 44);
            backColorLabel.Name = "backColorLabel";
            backColorLabel.Size = new Size(104, 34);
            backColorLabel.TabIndex = 1;
            backColorLabel.Text = "背景顏色";
            backColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _backColorTextBox
            // 
            _backColorTextBox.Dock = DockStyle.Fill;
            _backColorTextBox.Location = new Point(125, 47);
            _backColorTextBox.Name = "_backColorTextBox";
            _backColorTextBox.Size = new Size(418, 23);
            _backColorTextBox.TabIndex = 2;
            _backColorTextBox.TextChanged += BackColorTextBox_TextChanged;
            // 
            // colorButton
            // 
            colorButton.Dock = DockStyle.Fill;
            colorButton.Location = new Point(549, 47);
            colorButton.Name = "colorButton";
            colorButton.Size = new Size(70, 28);
            colorButton.TabIndex = 3;
            colorButton.Text = "...";
            colorButton.UseVisualStyleBackColor = true;
            colorButton.Click += ColorButton_Click;
            // 
            // backColorPreviewLabel
            // 
            backColorPreviewLabel.Dock = DockStyle.Fill;
            backColorPreviewLabel.Location = new Point(15, 78);
            backColorPreviewLabel.Name = "backColorPreviewLabel";
            backColorPreviewLabel.Size = new Size(104, 34);
            backColorPreviewLabel.TabIndex = 4;
            backColorPreviewLabel.Text = "顏色預覽";
            backColorPreviewLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _backColorPreview
            // 
            _backColorPreview.BorderStyle = BorderStyle.FixedSingle;
            _backColorPreview.Dock = DockStyle.Fill;
            _backColorPreview.Location = new Point(125, 81);
            _backColorPreview.Name = "_backColorPreview";
            _backColorPreview.Size = new Size(418, 28);
            _backColorPreview.TabIndex = 5;
            // 
            // backgroundImageLabel
            // 
            backgroundImageLabel.Dock = DockStyle.Fill;
            backgroundImageLabel.Location = new Point(15, 112);
            backgroundImageLabel.Name = "backgroundImageLabel";
            backgroundImageLabel.Size = new Size(104, 36);
            backgroundImageLabel.TabIndex = 6;
            backgroundImageLabel.Text = "背景圖片";
            backgroundImageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _backgroundImageTextBox
            // 
            _backgroundImageTextBox.Dock = DockStyle.Fill;
            _backgroundImageTextBox.Location = new Point(125, 115);
            _backgroundImageTextBox.Name = "_backgroundImageTextBox";
            _backgroundImageTextBox.Size = new Size(418, 23);
            _backgroundImageTextBox.TabIndex = 7;
            // 
            // browseImageButton
            // 
            browseImageButton.Dock = DockStyle.Fill;
            browseImageButton.Location = new Point(549, 115);
            browseImageButton.Name = "browseImageButton";
            browseImageButton.Size = new Size(70, 30);
            browseImageButton.TabIndex = 8;
            browseImageButton.Text = "...";
            browseImageButton.UseVisualStyleBackColor = true;
            browseImageButton.Click += BrowseImageButton_Click;
            // 
            // fontGroupBox
            // 
            fontGroupBox.Controls.Add(fontFieldsLayoutPanel);
            fontGroupBox.Dock = DockStyle.Top;
            fontGroupBox.Location = new Point(3, 203);
            fontGroupBox.Name = "fontGroupBox";
            fontGroupBox.Size = new Size(640, 112);
            fontGroupBox.TabIndex = 1;
            fontGroupBox.TabStop = false;
            fontGroupBox.Text = "字型";
            // 
            // fontFieldsLayoutPanel
            // 
            fontFieldsLayoutPanel.ColumnCount = 2;
            fontFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            fontFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            fontFieldsLayoutPanel.Controls.Add(fontLabel, 0, 0);
            fontFieldsLayoutPanel.Controls.Add(_fontComboBox, 1, 0);
            fontFieldsLayoutPanel.Controls.Add(fontSizeLabel, 0, 1);
            fontFieldsLayoutPanel.Controls.Add(_fontSizeNumeric, 1, 1);
            fontFieldsLayoutPanel.Dock = DockStyle.Fill;
            fontFieldsLayoutPanel.Location = new Point(3, 19);
            fontFieldsLayoutPanel.Name = "fontFieldsLayoutPanel";
            fontFieldsLayoutPanel.Padding = new Padding(12, 10, 12, 10);
            fontFieldsLayoutPanel.RowCount = 2;
            fontFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            fontFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            fontFieldsLayoutPanel.Size = new Size(634, 90);
            fontFieldsLayoutPanel.TabIndex = 0;
            // 
            // fontLabel
            // 
            fontLabel.Dock = DockStyle.Fill;
            fontLabel.Location = new Point(15, 10);
            fontLabel.Name = "fontLabel";
            fontLabel.Size = new Size(104, 34);
            fontLabel.TabIndex = 0;
            fontLabel.Text = "字型";
            fontLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fontComboBox
            // 
            _fontComboBox.Dock = DockStyle.Fill;
            _fontComboBox.FormattingEnabled = true;
            _fontComboBox.Location = new Point(125, 13);
            _fontComboBox.Name = "_fontComboBox";
            _fontComboBox.Size = new Size(494, 23);
            _fontComboBox.TabIndex = 1;
            // 
            // fontSizeLabel
            // 
            fontSizeLabel.Dock = DockStyle.Fill;
            fontSizeLabel.Location = new Point(15, 44);
            fontSizeLabel.Name = "fontSizeLabel";
            fontSizeLabel.Size = new Size(104, 36);
            fontSizeLabel.TabIndex = 2;
            fontSizeLabel.Text = "字型大小";
            fontSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _fontSizeNumeric
            // 
            _fontSizeNumeric.Dock = DockStyle.Left;
            _fontSizeNumeric.Location = new Point(125, 47);
            _fontSizeNumeric.Maximum = new decimal(new int[] { 48, 0, 0, 0 });
            _fontSizeNumeric.Minimum = new decimal(new int[] { 8, 0, 0, 0 });
            _fontSizeNumeric.Name = "_fontSizeNumeric";
            _fontSizeNumeric.Size = new Size(80, 23);
            _fontSizeNumeric.TabIndex = 3;
            _fontSizeNumeric.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // _generalPanel
            // 
            _generalPanel.Controls.Add(generalSectionLayoutPanel);
            _generalPanel.Dock = DockStyle.Fill;
            _generalPanel.Location = new Point(16, 0);
            _generalPanel.Name = "_generalPanel";
            _generalPanel.Size = new Size(646, 364);
            _generalPanel.TabIndex = 0;
            // 
            // generalSectionLayoutPanel
            // 
            generalSectionLayoutPanel.ColumnCount = 1;
            generalSectionLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            generalSectionLayoutPanel.Controls.Add(generalGroupBox, 0, 0);
            generalSectionLayoutPanel.Dock = DockStyle.Fill;
            generalSectionLayoutPanel.Location = new Point(0, 0);
            generalSectionLayoutPanel.Name = "generalSectionLayoutPanel";
            generalSectionLayoutPanel.RowCount = 4;
            generalSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
            generalSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
            generalSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            generalSectionLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            generalSectionLayoutPanel.Size = new Size(646, 364);
            generalSectionLayoutPanel.TabIndex = 0;
            // 
            // generalGroupBox
            // 
            generalGroupBox.Controls.Add(generalFieldsLayoutPanel);
            generalGroupBox.Dock = DockStyle.Top;
            generalGroupBox.Location = new Point(3, 3);
            generalGroupBox.Name = "generalGroupBox";
            generalGroupBox.Size = new Size(640, 92);
            generalGroupBox.TabIndex = 0;
            generalGroupBox.TabStop = false;
            generalGroupBox.Text = "一般";
            // 
            // generalFieldsLayoutPanel
            // 
            generalFieldsLayoutPanel.ColumnCount = 2;
            generalFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            generalFieldsLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            generalFieldsLayoutPanel.Controls.Add(languageLabel, 0, 0);
            generalFieldsLayoutPanel.Controls.Add(_languageComboBox, 1, 0);
            generalFieldsLayoutPanel.Dock = DockStyle.Fill;
            generalFieldsLayoutPanel.Location = new Point(3, 19);
            generalFieldsLayoutPanel.Name = "generalFieldsLayoutPanel";
            generalFieldsLayoutPanel.Padding = new Padding(12);
            generalFieldsLayoutPanel.RowCount = 1;
            generalFieldsLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            generalFieldsLayoutPanel.Size = new Size(634, 70);
            generalFieldsLayoutPanel.TabIndex = 0;
            // 
            // languageLabel
            // 
            languageLabel.Dock = DockStyle.Fill;
            languageLabel.Location = new Point(15, 12);
            languageLabel.Name = "languageLabel";
            languageLabel.Size = new Size(104, 46);
            languageLabel.TabIndex = 0;
            languageLabel.Text = "顯示語言";
            languageLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // _languageComboBox
            // 
            _languageComboBox.Dock = DockStyle.Left;
            _languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _languageComboBox.FormattingEnabled = true;
            _languageComboBox.Items.AddRange(new object[] { "zh-TW", "en-US" });
            _languageComboBox.Location = new Point(125, 15);
            _languageComboBox.Name = "_languageComboBox";
            _languageComboBox.Size = new Size(160, 23);
            _languageComboBox.TabIndex = 1;
            // 
            // buttonPanel
            // 
            buttonPanel.AutoSize = true;
            buttonPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            rootLayoutPanel.SetColumnSpan(buttonPanel, 2);
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(applyButton);
            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Dock = DockStyle.Fill;
            buttonPanel.FlowDirection = FlowDirection.RightToLeft;
            buttonPanel.Location = new Point(19, 387);
            buttonPanel.MinimumSize = new Size(0, 58);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new Padding(0, 8, 0, 0);
            buttonPanel.Size = new Size(822, 58);
            buttonPanel.TabIndex = 2;
            buttonPanel.WrapContents = false;
            // 
            // saveButton
            // 
            saveButton.AutoSize = true;
            saveButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            saveButton.Location = new Point(734, 8);
            saveButton.Margin = new Padding(8, 0, 0, 0);
            saveButton.MinimumSize = new Size(88, 32);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(88, 32);
            saveButton.TabIndex = 0;
            saveButton.Text = "儲存並關閉";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += SaveButton_Click;
            // 
            // applyButton
            // 
            applyButton.AutoSize = true;
            applyButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            applyButton.Location = new Point(638, 8);
            applyButton.Margin = new Padding(8, 0, 0, 0);
            applyButton.MinimumSize = new Size(88, 32);
            applyButton.Name = "applyButton";
            applyButton.Size = new Size(88, 32);
            applyButton.TabIndex = 1;
            applyButton.Text = "套用";
            applyButton.UseVisualStyleBackColor = true;
            applyButton.Click += ApplyButton_Click;
            // 
            // cancelButton
            // 
            cancelButton.AutoSize = true;
            cancelButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            cancelButton.Location = new Point(542, 8);
            cancelButton.Margin = new Padding(8, 0, 0, 0);
            cancelButton.MinimumSize = new Size(88, 32);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new Size(88, 32);
            cancelButton.TabIndex = 2;
            cancelButton.Text = "取消";
            cancelButton.UseVisualStyleBackColor = true;
            cancelButton.Click += CancelButton_Click;
            // 
            // ConfigForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(860, 460);
            Controls.Add(rootLayoutPanel);
            MinimumSize = new Size(760, 430);
            Name = "ConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "偏好設定";
            rootLayoutPanel.ResumeLayout(false);
            rootLayoutPanel.PerformLayout();
            _contentPanel.ResumeLayout(false);
            _savePanel.ResumeLayout(false);
            saveSectionLayoutPanel.ResumeLayout(false);
            saveGroupBox.ResumeLayout(false);
            saveFieldsLayoutPanel.ResumeLayout(false);
            saveFieldsLayoutPanel.PerformLayout();
            cacheManagementGroupBox.ResumeLayout(false);
            cacheManagementFlowPanel.ResumeLayout(false);
            cacheManagementFlowPanel.PerformLayout();
            _threadPanel.ResumeLayout(false);
            threadSectionLayoutPanel.ResumeLayout(false);
            threadGroupBox.ResumeLayout(false);
            threadFieldsLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_initialThreadNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)_maxThreadNumeric).EndInit();
            _appearancePanel.ResumeLayout(false);
            appearanceSectionLayoutPanel.ResumeLayout(false);
            appearanceGroupBox.ResumeLayout(false);
            appearanceFieldsLayoutPanel.ResumeLayout(false);
            appearanceFieldsLayoutPanel.PerformLayout();
            fontGroupBox.ResumeLayout(false);
            fontFieldsLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_fontSizeNumeric).EndInit();
            _generalPanel.ResumeLayout(false);
            generalSectionLayoutPanel.ResumeLayout(false);
            generalGroupBox.ResumeLayout(false);
            generalFieldsLayoutPanel.ResumeLayout(false);
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel rootLayoutPanel;
        private ListBox _categoryList;
        private Panel _contentPanel;
        private Panel _savePanel;
        private TableLayoutPanel saveSectionLayoutPanel;
        private GroupBox saveGroupBox;
        private TableLayoutPanel saveFieldsLayoutPanel;
        private Label savePathLabel;
        private TextBox _downloadPathTextBox;
        private Button browseFolderButton;
        private GroupBox cacheManagementGroupBox;
        private FlowLayoutPanel cacheManagementFlowPanel;
        private Button cacheClearButton;
        private Button historyClearButton;
        private Panel _threadPanel;
        private TableLayoutPanel threadSectionLayoutPanel;
        private GroupBox threadGroupBox;
        private TableLayoutPanel threadFieldsLayoutPanel;
        private Label initialThreadLabel;
        private NumericUpDown _initialThreadNumeric;
        private Label maxThreadLabel;
        private NumericUpDown _maxThreadNumeric;
        private Panel _appearancePanel;
        private TableLayoutPanel appearanceSectionLayoutPanel;
        private GroupBox appearanceGroupBox;
        private TableLayoutPanel appearanceFieldsLayoutPanel;
        private CheckBox _isDarkModeCheckBox;
        private Label backColorLabel;
        private TextBox _backColorTextBox;
        private Button colorButton;
        private Label backColorPreviewLabel;
        private Panel _backColorPreview;
        private Label backgroundImageLabel;
        private TextBox _backgroundImageTextBox;
        private Button browseImageButton;
        private GroupBox fontGroupBox;
        private TableLayoutPanel fontFieldsLayoutPanel;
        private Label fontLabel;
        private ComboBox _fontComboBox;
        private Label fontSizeLabel;
        private NumericUpDown _fontSizeNumeric;
        private Panel _generalPanel;
        private TableLayoutPanel generalSectionLayoutPanel;
        private GroupBox generalGroupBox;
        private TableLayoutPanel generalFieldsLayoutPanel;
        private Label languageLabel;
        private ComboBox _languageComboBox;
        private FlowLayoutPanel buttonPanel;
        private Button saveButton;
        private Button applyButton;
        private Button cancelButton;
    }
}
