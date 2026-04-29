using Autofac;
using JJNET.Utility.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using YTDownloader.Controller;
using YTDownloader.Model;
using YTDownloader.Service;
using YTDownloader.Tool;
using YTDownloader.UI;
using YTDownloader.UI.CustomUI;

namespace YTDownloader
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private readonly ConfigService _configService;
        private readonly OptionService _optionService;
        private IConfiguration config = null!;
        private Dictionary<string, List<KeyValuePair<string, string>>> options = new();
        private string DownloadFolder = string.Empty;
        private string ytDlpPath = string.Empty;
        private string ffmpegPath = string.Empty;
        private ConfigModel _settings;

        private const string OptionListMediaType = "ListMediaType";
        private const string OptionListDownloadStatus = "ListDownloadStatus";

        /// <summary>以 Task ID 為 Key，記錄每筆下載任務的控制器。</summary>
        private readonly Dictionary<long, DownloadTaskController> _downloadControllers = new();

        /// <summary>跨批次共用的並發信號量，最大同時下載數由 Config.json 控制。</summary>
        private readonly SemaphoreSlim _downloadSemaphore;

        /// <summary>共用的下載服務實例（延遲建立，確保 ytDlpPath / ffmpegPath 已讀取完畢）。</summary>
        private YtDlpDownloadService? _downloadService;

        private readonly Dictionary<string, HashSet<string>> _reservedDownloadFileNamesByFolder = new(StringComparer.OrdinalIgnoreCase);

        private PlaylistHandlerForm? _playlistHandlerForm;
        private DownloadHistoryForm? _downloadHistoryForm;
        private ConfigForm? _configForm;

        public MainForm()
        {
            _configService = new ConfigService();
            _optionService = Program.Startup.Container.Resolve<OptionService>();
            _logger = Program.Startup.Container.Resolve<ILogger<MainForm>>();
        }

        public MainForm(
            ConfigService configService,
            OptionService optionService,
            ILogger<MainForm> logger)
        {
            _configService = configService;
            _settings = _configService.Load();
            _optionService = optionService;
            this._logger = logger;
            _downloadSemaphore = CreateDownloadSemaphore(_settings);

            GUITool.ApplyStartupFont(this, _settings);
            InitializeComponent();
            LockWindowSize();
            ApplyStartupSettings();
            logger.LogInformation("MainForm form initialized.");
            Init();

        }
        
        #region Init

        private void Init()
        {
            InitConfig();
            InitOptions();
            InitUI();
        }

        private void InitUI()
        {
            InitDataGridView();
        }

        private void LockWindowSize()
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = Size;
            MaximumSize = Size;
        }

        private void ApplyStartupSettings()
        {
            GUITool.Apply(this, _settings);
            ApplyDownloadFolderSetting(_settings.Save);
        }

        private static SemaphoreSlim CreateDownloadSemaphore(ConfigModel settings)
        {
            var maxCount = Math.Clamp(settings.Thread.MaxCount, 1, 32);
            return new SemaphoreSlim(maxCount, maxCount);
        }

        private void ApplyDownloadFolderSetting(SaveConfigModel save)
        {
            if (string.IsNullOrWhiteSpace(save.DownloadPath))
                return;

            DownloadFolder = GUITool.ResolveAppPath(save.DownloadPath.Trim());
            Directory.CreateDirectory(DownloadFolder);
        }

        private void InitDataGridView()
        {
            dGV_DownloadList.Columns.Clear();

            // # 序號
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 40,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            // 檔名
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "檔名",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            // 類型（音訊 / 視訊）
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMediaType",
                HeaderText = "類型",
                Width = 55,
                ReadOnly = true,
                Resizable = DataGridViewTriState.False
            });

            // 進度條（自訂繪製）
            dGV_DownloadList.Columns.Add(new DataGridViewProgressBarColumn
            {
                Name = "colProgress",
                HeaderText = "進度",
                Width = 160,
                Resizable = DataGridViewTriState.False
            });

            // 狀態文字
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "狀態",
                Width = 200,
                ReadOnly = true,
                Resizable = DataGridViewTriState.True,
                DefaultCellStyle = new DataGridViewCellStyle
                { Alignment = DataGridViewContentAlignment.MiddleLeft }
            });

            // 操作按鈕（暫停 / 繼續 / 重試 / —）
            dGV_DownloadList.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colAction",
                HeaderText = "操作",
                Width = 60,
                Resizable = DataGridViewTriState.False,
                UseColumnTextForButtonValue = false   // 使用每格的 Value 作為按鈕文字
            });

            // 取消按鈕
            dGV_DownloadList.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "colCancel",
                HeaderText = "",
                Width = 55,
                Resizable = DataGridViewTriState.False,
                UseColumnTextForButtonValue = false
            });

            // 隱藏的任務 ID（用於刪除列後仍能找到正確的 controller）
            dGV_DownloadList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTaskId",
                HeaderText = "TaskId",
                Visible = false
            });

            dGV_DownloadList.CellContentClick += OnDownloadListCellContentClick;

            dGV_DownloadList.Rows.Clear();
        }

        private void InitOptions()
        {
            _logger.LogInformation("Initializing options...");

            try
            {
                options = _optionService.GetOptions();
                BindComboBox(cB_ListMediaType, OptionListMediaType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize options.");
                MessageBox.Show("選項載入失敗，請確認資料庫連線。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitConfig()
        {
            _logger.LogInformation("Initializing configuration...");
            config = ParameterTool.GetConfiguration();

            if (config == null)
            {
                _logger.LogError("Configuration object is null.");
                MessageBox.Show("載入設定失敗（config 為 null）。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 以安全方式讀取設定 (支援 appsettings.json 的 "yt-dlp"/"ffmpeg" 與原本預期的 "ytDlpPath"/"ffmpegPath")
            var ytDlpRel = config["Path:yt-dlp"];
            var ffmpegRel = config["Path:ffmpeg"];
            var downloadFolder = config["Path:DownLoadDir"];

            if (string.IsNullOrWhiteSpace(ytDlpRel))
            {
                _logger.LogError("yt-dlp path is not configured (Path:yt-dlp or Path:ytDlpPath).");
                MessageBox.Show("yt-dlp 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ytDlpPath = Path.Combine(Environment.CurrentDirectory, ytDlpRel.Trim());
                if (!File.Exists(ytDlpPath))
                {
                    _logger.LogError("yt-dlp executable not found at path: {Path}", ytDlpPath);
                    MessageBox.Show($"yt-dlp 可執行檔未找到，請確認路徑：{ytDlpPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (string.IsNullOrWhiteSpace(ffmpegRel))
            {
                _logger.LogError("ffmpeg path is not configured (Path:ffmpeg or Path:ffmpegPath).");
                MessageBox.Show("ffmpeg 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ffmpegPath = Path.Combine(Environment.CurrentDirectory, ffmpegRel.Trim());
                if (!File.Exists(ffmpegPath))
                {
                    _logger.LogError("ffmpeg executable not found at path: {Path}", ffmpegPath);
                    MessageBox.Show($"ffmpeg 可執行檔未找到，請確認路徑：{ffmpegPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (!string.IsNullOrWhiteSpace(DownloadFolder))
            {
                _logger.LogInformation("Download folder loaded from Config.json: {Path}", DownloadFolder);
            }
            else if (string.IsNullOrWhiteSpace(downloadFolder))
            {
                _logger.LogError("downloadFolder path is not configured (Path:DownLoadDir or Path:ffmpegPath).");
                MessageBox.Show("DownLoadDir 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DownloadFolder = Path.Combine(Environment.CurrentDirectory, downloadFolder.Trim());
                if (!Directory.Exists(DownloadFolder))
                {
                    _logger.LogError("Download folder not found at path: {Path}", DownloadFolder);
                    MessageBox.Show($"下載資料夾未找到，請確認路徑：{DownloadFolder}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BindComboBox(ComboBox comboBox, string key)
        {
            var items = GetOptions(key);
            if (items.Count == 0)
            {
                _logger.LogWarning("Options key '{Key}' is missing or empty.", key);
                return;
            }

            comboBox.DisplayMember = "Key";
            comboBox.Items.AddRange(items.Cast<object>().ToArray());
            if (comboBox.Items.Count > 0) { comboBox.SelectedIndex = 0; }
        }

        #endregion Init

        #region Option helpers

        /// <summary>
        /// 取得指定外部選單的全部項目。Key 為 Desc（顯示文字），Value 為 Name（資料庫值）。
        /// </summary>
        private IReadOnlyList<KeyValuePair<string, string>> GetOptions(string listName)
        {
            return options != null && options.TryGetValue(listName, out var items) && items != null
                ? items
                : Array.Empty<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// 依 Name（資料庫值）取得 Desc（顯示文字）。
        /// </summary>
        private string GetOptionDesc(string listName, string name)
        {
            var desc = GetOptions(listName)
                .FirstOrDefault(item => item.Value == name)
                .Key;

            return string.IsNullOrWhiteSpace(desc) ? name : desc;
        }

        /// <summary>
        /// 依 Desc（顯示文字）取得 Name（資料庫值）。
        /// </summary>
        private string GetOptionName(string listName, string desc)
        {
            var name = GetOptions(listName)
                .FirstOrDefault(item => item.Key == desc)
                .Value;

            return string.IsNullOrWhiteSpace(name) ? desc : name;
        }

        private static string GetSelectedOptionName(ComboBox comboBox)
        {
            return comboBox.SelectedItem is KeyValuePair<string, string> kv
                ? kv.Value
                : string.Empty;
        }

        private static string GetSelectedOptionDesc(ComboBox comboBox)
        {
            return comboBox.SelectedItem is KeyValuePair<string, string> kv
                ? kv.Key
                : string.Empty;
        }

        #endregion
        
        #region UI Functions

        private async void btn_Download_Click(object sender, EventArgs e)
        {
            try
            {
                var URL = tB_URL.Text;
                if (string.IsNullOrWhiteSpace(URL))
                {
                    MessageBox.Show("請輸入有效的 URL。", "輸入錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var ffmpegDir = File.Exists(ffmpegPath)
                    ? Path.GetDirectoryName(ffmpegPath)!
                    : ffmpegPath;
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegDir);

                var SourceType = await YTDownloadService.DetectResourceAsync(URL);
                switch (SourceType.ResourceType)
                {
                    case UrlResourceType.SingleVideo:
                        _logger.LogInformation($"檢測到單一影片：{SourceType.Title}", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var playlist = await YTDownloadService.GetPlaylistVideosAsync(URL);
                        var video =  playlist.Videos.FirstOrDefault() ?? throw new Exception("無法取得影片資訊");
                        var request = new DownloadRequest
                        {
                            Title            = video.Title ?? "未知標題",
                            WebpageUrl       = URL,
                            MediaType        = Enum.TryParse<MediaType>(GetSelectedOptionName(cB_ListMediaType), ignoreCase: false, out var mediaType)
                                ? mediaType
                                : throw new NotSupportedException($"未知的媒體類型值：{GetSelectedOptionName(cB_ListMediaType)}"),
                            MediaTypeDisplay = GetSelectedOptionDesc(cB_ListMediaType),
                            DownloadDir      = DownloadFolder
                        };
                        EnqueueDownloads(new[] { request });
                        break;
                    case UrlResourceType.Playlist:
                        _logger.LogInformation($"檢測到播放清單：{SourceType.PlaylistTitle}，共 {SourceType.PlaylistCount} 部影片");
                        //如果沒有被正確關閉，則先釋放資源
                        if (_playlistHandlerForm is { IsDisposed: false })
                            _playlistHandlerForm.Close();
                        _playlistHandlerForm = new PlaylistHandlerForm(
                            URL,
                            this,
                            Enum.TryParse<MediaType>(GetSelectedOptionName(cB_ListMediaType), ignoreCase: false, out var playlistMediaType)
                                ? playlistMediaType
                                : throw new NotSupportedException($"未知的媒體類型值：{GetSelectedOptionName(cB_ListMediaType)}"),
                            GetSelectedOptionDesc(cB_ListMediaType));
                        var (isSuccess, msg) = await _playlistHandlerForm.GetPlaylistInfoAsync();
                        if (isSuccess)
                        {
                            _logger.LogInformation($"成功獲取播放清單資訊：{msg}");
                            _playlistHandlerForm.Location = new Point(700, 0);
                            _playlistHandlerForm.Disposed += new EventHandler(playlistHandlerForm_Disposed);
                            _playlistHandlerForm.Show();
                        }
                        else
                        {
                            _logger.LogError($"獲取播放清單資訊失敗：{msg}");
                            _playlistHandlerForm.Dispose();
                            MessageBox.Show(
                                $"無法載入播放清單，請確認連結是否正確。\n\n原因：{msg}",
                                "播放清單載入失敗",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                        break;
                    default:
                        MessageBox.Show("無法識別的 URL 類型。", "資源檢測", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve selected options.");
            }
        }

        private void playlistHandlerForm_Disposed(object? sender, EventArgs e)
        {
            _playlistHandlerForm = null;
        }
        
        private void downloadHistoryForm_Disposed(object? sender, EventArgs e)
        {
            _downloadHistoryForm = null;
        }

        private void btn_OpenDownloadForder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", DownloadFolder);
        }

        /// <summary>
        /// 處理下載清單的按鈕點擊（暫停 / 繼續 / 重試 / 取消）。
        /// </summary>
        private void OnDownloadListCellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dGV_DownloadList.Columns[e.ColumnIndex].Name;
            if (colName != "colAction" && colName != "colCancel") return;

            var row = dGV_DownloadList.Rows[e.RowIndex];

            // 取得此列的 Task ID（TextBox 儲存格可能回傳 int 或 string）
            var taskIdCell = row.Cells["colTaskId"].Value;
            long taskId;
            if (taskIdCell is long directLongId)
                taskId = directLongId;
            else if (taskIdCell is int directId)
                taskId = directId;
            else if (taskIdCell is string s && long.TryParse(s, out long parsedId))
                taskId = parsedId;
            else
                return;

            // ── 取消按鈕 ──────────────────────────────────────────
            if (colName == "colCancel")
            {
                if (_downloadControllers.TryGetValue(taskId, out var cancelCtrl))
                {
                    cancelCtrl.Cts.Cancel();
                    _downloadControllers.Remove(taskId);
                }
                dGV_DownloadList.Rows.RemoveAt(e.RowIndex);
                RenumberRows();
                return;
            }

            // ── colAction ─────────────────────────────────────────
            if (!_downloadControllers.TryGetValue(taskId, out var controller)) return;

            var btnText = row.Cells["colAction"].Value?.ToString() ?? "";
            switch (btnText)
            {
                // ── 暫停 ─────────────────────────────────────────
                case "暫停":
                    controller.Cts.Cancel();
                    controller.IsPaused = true;
                    row.Cells["colAction"].Value = "繼續";
                    UpdateDownloadProgress(taskId, controller.LastPercent, "Pause");
                    break;

                // ── 繼續 / 重試 ──────────────────────────────────
                case "繼續":
                case "重試":
                    controller.Cts = new CancellationTokenSource();
                    controller.IsPaused = false;
                    // 重啟前先清理：避免不完整輸出或殘留串流檔干擾 yt-dlp 判斷
                    CleanTempFilesBeforeRestart(controller);
                    row.Cells["colAction"].Value = "暫停";
                    UpdateDownloadProgress(taskId, controller.LastPercent, "InProgress");
                    var cts = controller.Cts;
                    _ = Task.Run(async () => await controller.RestartAction(cts.Token));
                    break;

                // ── 已完成 / 其他（不動作）───────────────────────
                default:
                    break;
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 依 Task ID 找出對應的 DataGridView 列（列刪除後仍穩定）。
        /// </summary>
        private DataGridViewRow? FindRowByTaskId(long taskId)
        {
            foreach (DataGridViewRow row in dGV_DownloadList.Rows)
            {
                var cellVal = row.Cells["colTaskId"].Value;
                // DataGridViewTextBoxColumn 可能以 int 或 string 儲存
                if (cellVal is long lid && lid == taskId) return row;
                if (cellVal is int id && id == taskId) return row;
                if (cellVal is string s && long.TryParse(s, out long sid) && sid == taskId) return row;
            }
            return null;
        }

        /// <summary>
        /// 重新編排 colIndex（刪除列後維持連續序號）。
        /// </summary>
        private void RenumberRows()
        {
            for (int i = 0; i < dGV_DownloadList.Rows.Count; i++)
                dGV_DownloadList.Rows[i].Cells["colIndex"].Value = i + 1;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 新增一筆下載項目至清單，回傳穩定的 Task ID（不受列刪除影響）。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public long AddDownloadItem(string title, string mediaType)
        {
            // 如果不是在 UI 執行緒，使用 Invoke 切換到 UI 執行緒執行，確保 DataGridView 操作安全。
            if (dGV_DownloadList.InvokeRequired)
                return (long)dGV_DownloadList.Invoke(new Func<long>(() => AddDownloadItem(title, mediaType)));

            long taskId = CreateTaskId();
            dGV_DownloadList.Rows.Add(
                dGV_DownloadList.Rows.Count + 1,  // colIndex
                title,                                                      // colTitle
                mediaType,                                          // colMediaType
                0.0,                                                       // colProgress（double，ProgressBarCell 讀取）
                GetOptionDesc(OptionListDownloadStatus, "Waiting"), // colStatus
                "暫停",                                                 // colAction 按鈕文字
                "取消",                                                 // colCancel 按鈕文字
                taskId                                                  // colTaskId（隱藏）
            );
            return taskId;
        }

        /// <summary>
        /// 更新指定任務的進度條與狀態欄位，並同步更新控制器的 LastPercent。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void UpdateDownloadProgress(long taskId, double percent, string status)
        {
            var parts = status.Split('：', 2);
            var statusName = GetOptionName(OptionListDownloadStatus, parts[0]);
            var statusDesc = GetOptionDesc(OptionListDownloadStatus, statusName);
            var displayStatus = parts.Length == 2 ? $"{statusDesc}：{parts[1]}" : statusDesc;

            UpdateDownloadProgressInDatabase(taskId, percent, status);

            // 如果不是在 UI 執行緒，使用 Invoke 切換到 UI 執行緒執行，確保 DataGridView 操作安全。
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => UpdateDownloadProgressInGrid(taskId, percent, displayStatus)));
                return;
            }

            UpdateDownloadProgressInGrid(taskId, percent, displayStatus);
        }

        private void UpdateDownloadProgressInGrid(long taskId, double percent, string status)
        {
            var row = FindRowByTaskId(taskId);
            if (row == null) return;

            row.Cells["colProgress"].Value = percent;   // double → DataGridViewProgressBarCell
            row.Cells["colStatus"].Value = status;

            // 同步控制器的最後進度（供「繼續」時顯示）
            if (_downloadControllers.TryGetValue(taskId, out var ctrl))
                ctrl.LastPercent = percent;

            // 依狀態設定列底色
            row.DefaultCellStyle.BackColor = GetStatusBackColor(status);
        }

        private Color GetStatusBackColor(string status)
        {
            var statusName = GetOptionName(OptionListDownloadStatus, status.Split('：', 2)[0]);
            return statusName switch
            {
                "Complete"   => Color.FromArgb(200, 240, 200),
                "Fail"       => Color.FromArgb(255, 200, 200),
                "InProgress" => Color.FromArgb(230, 240, 255),
                "Pause"      => Color.FromArgb(255, 248, 220),
                _            => dGV_DownloadList.DefaultCellStyle.BackColor
            };
        }

        private void UpdateDownloadProgressInDatabase(long taskId, double percent, string status)
        {
            try
            {
                var progress = Math.Clamp((int)Math.Round(percent, MidpointRounding.AwayFromZero), 0, 100);
                var databaseStatus = GetOptionName(OptionListDownloadStatus, status.Split('：', 2)[0]);
                DateTime? completeDateTime = databaseStatus == "Complete" ? DateTime.UtcNow : null;
                DBTool.UpdateDownloadProgress(taskId, progress, databaseStatus, completeDateTime);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "更新 DownloadHistory 進度失敗。TaskID: {TaskID}, Progress: {Progress}, Status: {Status}", taskId, percent, status);
            }
        }

        /// <summary>
        /// 向 MainForm 登錄一筆下載任務的控制器（含重啟 Action），回傳可操控該任務的 controller。
        /// </summary>
        public DownloadTaskController RegisterDownload(long taskId, Func<CancellationToken, Task> restartAction)
        {
            var controller = new DownloadTaskController { RestartAction = restartAction };
            _downloadControllers[taskId] = controller;
            return controller;
        }

        /// <summary>
        /// 設定指定任務的操作按鈕文字。執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        public void SetActionButton(long taskId, string text)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => SetActionButton(taskId, text)));
                return;
            }

            var row = FindRowByTaskId(taskId);
            if (row != null)
                row.Cells["colAction"].Value = text;
        }

        /// <summary>
        /// 將錯誤訊息設為狀態欄的 ToolTip，讓使用者滑鼠停留時可看到失敗原因。
        /// 執行緒安全（可從非 UI 執行緒呼叫）。
        /// </summary>
        private void SetStatusTooltip(long taskId, string message)
        {
            if (dGV_DownloadList.InvokeRequired)
            {
                dGV_DownloadList.Invoke(new Action(() => SetStatusTooltip(taskId, message)));
                return;
            }

            var row = FindRowByTaskId(taskId);
            if (row != null)
                row.Cells["colStatus"].ToolTipText = message;
        }

        /// <summary>
        /// 重啟下載前，清除可能使 yt-dlp 誤判「已完成」的殘留檔案：
        /// <list type="bullet">
        ///   <item><b>.part</b> 部分下載暫存檔 — 允許 yt-dlp 重新下載（不嘗試 resume）。</item>
        ///   <item><b>不完整的最終音訊輸出</b>（如 .mp3）— 若同一 stem 仍有原始串流檔（如 .webm），
        ///         代表 ffmpeg 轉檔未完成；刪除後 yt-dlp 重啟時可利用現有串流檔直接重新轉檔，
        ///         不需重新下載串流。</item>
        /// </list>
        /// 僅對音訊任務執行；視訊任務不清除中間串流，避免誤刪最終輸出。
        /// </summary>
        private void CleanTempFilesBeforeRestart(DownloadTaskController controller)
        {
            var req = controller.OriginalRequest;
            if (req == null
                || string.IsNullOrWhiteSpace(req.DownloadDir)
                || !Directory.Exists(req.DownloadDir))
                return;

            // 對「標題」與「檔名」均做相同的正規化後再比對，
            // 解決 yt-dlp 將受限字元替換為全形字元（如 / → ／）
            // 而本端僅替換為底線所造成的前綴不吻合問題。
            var matchName = string.IsNullOrWhiteSpace(req.FileName)
                ? req.Title
                : Path.GetFileNameWithoutExtension(req.FileName);
            var normalizedTitle = NormalizeForFileMatch(matchName);
            if (string.IsNullOrWhiteSpace(normalizedTitle)) return;

            // yt-dlp 的「影片串流」暫存副檔名（轉音訊前的原始檔）
            var videoStreamExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".webm", ".mkv", ".mp4", ".m4a", ".ogg", ".opus", ".flv", ".avi"
            };

            // ffmpeg 轉出的最終音訊副檔名
            var audioOutputExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp3", ".m4a", ".flac", ".wav", ".opus", ".aac"
            };

            int deletedCount = 0;

            foreach (var file in Directory.GetFiles(req.DownloadDir))
            {
                var fileName = Path.GetFileName(file);
                var ext      = Path.GetExtension(file);
                var stem     = Path.GetFileNameWithoutExtension(file);

                // 正規化後做前綴比對，容忍 yt-dlp 全形字元 vs. 底線的差異
                if (!NormalizeForFileMatch(fileName).StartsWith(normalizedTitle, StringComparison.OrdinalIgnoreCase))
                    continue;

                // ── 1. 刪除 .part 部分下載暫存檔 ─────────────────────────────
                if (ext.Equals(".part", StringComparison.OrdinalIgnoreCase))
                {
                    TryDeleteTempFile(file, ref deletedCount);
                    continue;
                }

                // ── 2. 音訊任務：偵測並刪除不完整的最終音訊輸出 ──────────────
                //    判定條件：最終音訊檔（.mp3 等）與原始串流檔（.webm 等）同時存在
                //    → ffmpeg 轉檔被中斷，音訊輸出不完整，需強制重新轉檔
                if (req.MediaType == MediaType.Audio && audioOutputExts.Contains(ext))
                {
                    bool videoStreamExists = videoStreamExts.Any(ve =>
                        File.Exists(Path.Combine(req.DownloadDir, stem + ve)));

                    if (videoStreamExists)
                        TryDeleteTempFile(file, ref deletedCount);
                }
            }

            if (deletedCount > 0)
                _logger.LogInformation(
                    "重啟前清除 {Count} 個殘留暫存檔（檔名：{FileName}）",
                    deletedCount, req.FileName);
            else
                _logger.LogDebug(
                    "重啟前未找到需清除的暫存檔（正規化標題：{NTitle}）",
                    normalizedTitle);
        }

        private void TryDeleteTempFile(string path, ref int count)
        {
            try
            {
                File.Delete(path);
                count++;
                _logger.LogInformation("已刪除暫存檔：{File}", Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("無法刪除暫存檔 [{File}]：{Msg}", Path.GetFileName(path), ex.Message);
            }
        }

        /// <summary>
        /// 將字串正規化以便與 yt-dlp 產生的檔名做前綴比對。
        /// <para>
        /// yt-dlp 在 Windows 上對受限字元的處理方式因版本而異：
        /// 可能替換為 <c>_</c>（底線）或對應的全形字元（如 <c>/</c> → <c>／</c> U+FF0F）。
        /// 此方法將 ASCII 受限字元及其全形對應字元統一對應至 <c>_</c>，
        /// 確保無論 yt-dlp 採用哪種替換策略，標題與檔名的前綴比對都能成立。
        /// </para>
        /// </summary>
        private static string NormalizeForFileMatch(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return new string(s.Select(c => c switch
            {
                '/' or '／'  => '_',   // SOLIDUS           / FULLWIDTH SOLIDUS
                '\\' or '＼' => '_',   // REVERSE SOLIDUS   / FULLWIDTH REVERSE SOLIDUS
                ':' or '：'  => '_',   // COLON             / FULLWIDTH COLON
                '*' or '＊'  => '_',   // ASTERISK          / FULLWIDTH ASTERISK
                '?' or '？'  => '_',   // QUESTION MARK     / FULLWIDTH QUESTION MARK
                '"' or '＂'  => '_',   // QUOTATION MARK    / FULLWIDTH QUOTATION MARK
                '<' or '＜'  => '_',   // LESS-THAN SIGN    / FULLWIDTH LESS-THAN SIGN
                '>' or '＞'  => '_',   // GREATER-THAN SIGN / FULLWIDTH GREATER-THAN SIGN
                '|' or '｜'  => '_',   // VERTICAL LINE     / FULLWIDTH VERTICAL LINE
                _            => c
            }).ToArray());
        }

        /// <summary>
        /// 音訊下載成功完成後，清除 yt-dlp 或 ffmpeg 未自動刪除的中間影片串流暫存檔
        /// （如 .mp4、.webm、.mkv 等），保持下載目錄乾淨。
        /// <para>
        /// 此情況的發生原因：yt-dlp 以 <c>--extract-audio</c> 下載時，正常流程應在
        /// ffmpeg 轉檔後自動移除原始串流檔，但部分 yt-dlp / 包裝函式庫版本或特定格式
        /// 組合下，原始串流檔會被保留。
        /// </para>
        /// </summary>
        private void CleanVideoStreamsAfterAudioDownload(DownloadRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DownloadDir) || !Directory.Exists(req.DownloadDir))
                return;

            var matchName = string.IsNullOrWhiteSpace(req.FileName)
                ? req.Title
                : Path.GetFileNameWithoutExtension(req.FileName);
            var normalizedTitle = NormalizeForFileMatch(matchName);
            if (string.IsNullOrWhiteSpace(normalizedTitle)) return;

            // 確定是影片容器格式（不會是音訊下載的最終輸出）
            var videoOnlyExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp4", ".webm", ".mkv", ".flv", ".avi", ".ts", ".mov"
            };

            int deletedCount = 0;
            foreach (var file in Directory.GetFiles(req.DownloadDir))
            {
                var fileName = Path.GetFileName(file);
                var ext      = Path.GetExtension(file);

                if (!NormalizeForFileMatch(fileName).StartsWith(normalizedTitle, StringComparison.OrdinalIgnoreCase)) continue;
                if (!videoOnlyExts.Contains(ext)) continue;

                TryDeleteTempFile(file, ref deletedCount);
            }

            if (deletedCount > 0)
                _logger.LogInformation(
                    "音訊下載完成後清除 {Count} 個殘留串流暫存檔（檔名：{FileName}）",
                    deletedCount, req.FileName);
        }

        private static long CreateTaskId()
        {
            return new IDGenerator().GetID("TaskID", "任務編號");
        }

        private static DownloadHistory CreateDownloadHistory(
            long taskId,
            DownloadRequest req,
            string status,
            string? progress = null,
            DateTime? completeDateTime = null)
        {
            return new DownloadHistory
            {
                TaskID = taskId,
                URL = req.WebpageUrl,
                Status = status,
                Title = req.Title,
                FileName = req.FileName,
                Path = req.DownloadDir,
                Progress = progress,
                Type = req.MediaType.ToString(),
                DownloadDateTime = DateTime.UtcNow,
                CompleteDateTime = completeDateTime
            };
        }

        private string CreateUniqueDownloadFileName(DownloadRequest req)
        {
            var baseName = SanitizeFileName(req.Title);
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "download";

            var reservedFileNames = GetReservedFileNames(req.DownloadDir);
            var extension = GetDefaultExtension(req.MediaType);
            if (!string.IsNullOrWhiteSpace(extension)
                && baseName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                baseName = Path.GetFileNameWithoutExtension(baseName);
            }

            var candidate = $"{baseName}{extension}";
            var index = 0;

            while (reservedFileNames.Contains(candidate) || File.Exists(Path.Combine(req.DownloadDir, candidate)))
            {
                index++;
                candidate = $"{baseName}({index}){extension}";
            }

            return candidate;
        }

        private HashSet<string> GetReservedFileNames(string downloadDir)
        {
            var key = Path.GetFullPath(downloadDir);
            if (!_reservedDownloadFileNamesByFolder.TryGetValue(key, out var fileNames))
            {
                fileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _reservedDownloadFileNamesByFolder[key] = fileNames;
            }

            return fileNames;
        }

        private static string CreateOutputTemplate(string fileName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            return $"{nameWithoutExtension}.%(ext)s";
        }

        private static string GetDefaultExtension(MediaType mediaType)
        {
            return mediaType switch
            {
                MediaType.Audio => ".mp3",
                MediaType.Video => ".mp4",
                _               => string.Empty
            };
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
            var sanitized = new string(fileName
                .Select(c => invalidChars.Contains(c) ? '_' : c)
                .ToArray())
                .Trim();

            return string.IsNullOrWhiteSpace(sanitized) ? "download" : sanitized;
        }

        /// <summary>
        /// 接收來自 PlaylistHandlerForm（或其他來源）的下載請求，
        /// 加入清單並立即以背景工作排入佇列執行。
        /// 可在視窗已關閉後安全呼叫（執行緒安全）。
        /// </summary>
        public void EnqueueDownloads(IEnumerable<DownloadRequest> requests)
        {
            // ffmpegPath 指向可執行檔；YtDlpDownloadService 的第二參數需要「資料夾」路徑
            var ffmpegDir = File.Exists(ffmpegPath)
                ? Path.GetDirectoryName(ffmpegPath)!
                : ffmpegPath;

            _downloadService ??= new YtDlpDownloadService(ytDlpPath, ffmpegDir);
            foreach (var req in requests)
            {
                req.FileName = CreateUniqueDownloadFileName(req);
                GetReservedFileNames(req.DownloadDir).Add(req.FileName);

                // 建立 UI 下載項目時取號，讓 controller 與 DownloadHistory 共用同一組任務編號
                long taskId = AddDownloadItem(req.FileName, req.MediaTypeDisplay);
                DBTool.InsertDownloadHistory(CreateDownloadHistory(taskId, req, "Waiting", "0"));

                // 捕捉區域變數，避免 closure 捕捉迴圈變數
                var capturedReq = req;
                var svc = _downloadService;

                Func<CancellationToken, Task> downloadAction = async (ct) =>
                {
                    UpdateDownloadProgress(taskId, 0, "InProgress");

                    DownloadResult result = capturedReq.MediaType switch
                    {
                        MediaType.Audio => await svc.DownloadAudioAsync(
                            url:               capturedReq.WebpageUrl,
                            outputFolder:      capturedReq.DownloadDir,
                            outputTemplate:    CreateOutputTemplate(capturedReq.FileName),
                            knownTitle:        capturedReq.Title,
                            onProgress:        pct => UpdateDownloadProgress(taskId, pct, "InProgress"),
                            cancellationToken: ct),

                        MediaType.Video => await svc.DownloadVideoAsync(
                            url:               capturedReq.WebpageUrl,
                            outputFolder:      capturedReq.DownloadDir,
                            outputTemplate:    CreateOutputTemplate(capturedReq.FileName),
                            knownTitle:        capturedReq.Title,
                            onProgress:        pct => UpdateDownloadProgress(taskId, pct, "InProgress"),
                            cancellationToken: ct),

                        _ => throw new NotSupportedException(
                                 $"尚未支援的媒體類型：{capturedReq.MediaType}")
                    };

                    // 被取消（暫停 / 取消按鈕）→ 不覆蓋狀態
                    if (ct.IsCancellationRequested) return;

                    if (result.IsSuccess)
                    {
                        UpdateDownloadProgress(taskId, 100, "Complete");
                        SetActionButton(taskId, "—");
                        // 音訊下載完成後，清除 yt-dlp/ffmpeg 未自動刪除的中間影片串流暫存檔
                        if (capturedReq.MediaType == MediaType.Audio)
                            CleanVideoStreamsAfterAudioDownload(capturedReq);
                    }
                    else
                    {
                        // 取第一行作為簡短狀態文字，完整訊息放 Tooltip
                        var firstLine = (result.Message ?? "未知錯誤")
                            .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault() ?? "未知錯誤";

                        UpdateDownloadProgress(taskId, 0, $"Fail：{firstLine}");
                        SetActionButton(taskId, "重試");
                        SetStatusTooltip(taskId, result.Message ?? "未知錯誤");
                        _logger.LogError("下載失敗 [{Title}]：{Message}", capturedReq.Title, result.Message);
                    }
                };

                //註冊至 MainForm，取得控制器以供暫停/繼續使用
                var controller = RegisterDownload(taskId, downloadAction);
                controller.OriginalRequest = capturedReq;   // 供重啟前清除暫存檔使用

                // 排入背景，受全域信號量並發控制
                _ = Task.Run(async () =>
                {
                    await _downloadSemaphore.WaitAsync();
                    try { await downloadAction(controller.Cts.Token); }
                    finally { _downloadSemaphore.Release(); }
                });
            }
        }

        #endregion

        private void MSItem_DownloadHistory_Click(object sender, EventArgs e)
        {
            if(_downloadHistoryForm is { IsDisposed: false })
            {
                _downloadHistoryForm.Activate();
                return;
            }
            _downloadHistoryForm = new DownloadHistoryForm();
            _downloadHistoryForm.Location = new Point(700, 0);
            _downloadHistoryForm.Disposed += new EventHandler(downloadHistoryForm_Disposed);
            _downloadHistoryForm.Show();
        }

        private void MSItem_Config_Click(object? sender, EventArgs e)
        {
            if (_configForm is { IsDisposed: false })
            {
                _configForm.Activate();
                return;
            }

            _configForm = new ConfigForm(_configService);
            _configForm.Location = new Point(700, 0);
            _configForm.FormClosed += (_, _) =>
            {
                _settings = _configService.Load();
                ApplyStartupSettings();
            };
            _configForm.Disposed += (_, _) => _configForm = null;
            _configForm.Show(this);
        }

        private void btn_ClearCompleteTask_Click(object sender, EventArgs e)
        {
            //TODO: 清除已完成的下載任務
        }

        private void btn_CancelAll_Click(object sender, EventArgs e)
        {
            //TODO: 清除所有下載任務
        }
    }
}
