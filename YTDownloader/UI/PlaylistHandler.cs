using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTDownloader.Model;
using YTDownloader.Service;

namespace YTDownloader
{
    public partial class PlaylistHandler : Form
    {
        private ILogger logger = Program.Startup.Container.Resolve<ILogger<PlaylistHandler>>();
        private PlaylistHandlerInitializationService initializationService = Program.Startup.Container.Resolve<PlaylistHandlerInitializationService>();
        private IConfiguration config;
        private string ytDlpPath;
        private string ffmpegPath;
        private string downloadDir;
        private string playlistUrl;
        private Main mainForm;
        private List<PlaylistVideoItem> playlistVideos = new();


        public PlaylistHandler()
        {
            InitializeComponent();
            logger.LogInformation("PlaylistHandler form initialized.");

        }

        public PlaylistHandler(string playlistUrl, Main mainForm) : this()
        {
            this.playlistUrl = playlistUrl;
            this.mainForm = mainForm;
            Init();
            InitUI();
        }

        private void Init()
        {
            InitConfig();

        }

        private void InitUI()
        {
            InitDataGridView();
        }

        private void InitDataGridView()
        {
            dGV_PlayList.Columns.Clear();

            // 勾選欄
            dGV_PlayList.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "colSelected",
                HeaderText = "選取",
                Width = 50,
                ReadOnly = false,
                Resizable = DataGridViewTriState.False
            });

            // 序號欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colIndex",
                HeaderText = "#",
                Width = 45,
                ReadOnly = true
            });

            // 影片唯一識別碼
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "Id",
                Width = 45,
                ReadOnly = true,
                Visible = false // 隱藏 ID 欄位，僅供內部使用
            });

            // 標題欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTitle",
                HeaderText = "標題",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });

            // 頻道欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colUploader",
                HeaderText = "頻道",
                Width = 150,
                ReadOnly = true
            });

            // 時長欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDuration",
                HeaderText = "時長",
                Width = 80,
                ReadOnly = true
            });

            // 時長欄
            dGV_PlayList.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colURL",
                HeaderText = "連結",
                Width = 80,
                ReadOnly = true,
                Visible = false // 隱藏 URL 欄位，僅供內部使用
            });
        }


        private void InitConfig()
        {
            logger.LogInformation("Initializing configuration...");
            config = initializationService.GetConfig();

            if (config == null)
            {
                logger.LogError("Configuration object is null.");
                MessageBox.Show("載入設定失敗（config 為 null）。", "初始化錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 以安全方式讀取設定 (支援 appsettings.json 的 "yt-dlp"/"ffmpeg" 與原本預期的 "ytDlpPath"/"ffmpegPath")
            var ytDlpRel = config["Path:yt-dlp"];
            var ffmpegRel = config["Path:ffmpeg"];

            if (string.IsNullOrWhiteSpace(ytDlpRel))
            {
                logger.LogError("yt-dlp path is not configured (Path:yt-dlp or Path:ytDlpPath).");
                MessageBox.Show("yt-dlp 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ytDlpPath = Path.Combine(Environment.CurrentDirectory, ytDlpRel.Trim());
                if (!File.Exists(ytDlpPath))
                {
                    logger.LogError("yt-dlp executable not found at path: {Path}", ytDlpPath);
                    MessageBox.Show($"yt-dlp 可執行檔未找到，請確認路徑：{ytDlpPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (string.IsNullOrWhiteSpace(ffmpegRel))
            {
                logger.LogError("ffmpeg path is not configured (Path:ffmpeg or Path:ffmpegPath).");
                MessageBox.Show("ffmpeg 路徑未在設定中指定。", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ffmpegPath = Path.Combine(Environment.CurrentDirectory, ffmpegRel.Trim());
                if (!File.Exists(ffmpegPath))
                {
                    logger.LogError("ffmpeg executable not found at path: {Path}", ffmpegPath);
                    MessageBox.Show($"ffmpeg 可執行檔未找到，請確認路徑：{ffmpegPath}", "配置錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            var downloadDirRel = config["Path:DownLoadDir"];
            downloadDir = string.IsNullOrWhiteSpace(downloadDirRel)
                ? Path.Combine(Environment.CurrentDirectory, "Downloads")
                : Path.Combine(Environment.CurrentDirectory, downloadDirRel.Trim());
        }

        /// <summary>
        /// 非同步取得播放清單資訊，並填入 DataGridView。
        /// 避免在 UI Thread 上使用 .Result/.GetAwaiter().GetResult()，防止 async deadlock。
        /// </summary>
        /// <returns>(IsSuccess, Message)</returns>
        public async Task<(bool IsSuccess, string Message)> GetPlaylistInfoAsync()
        {
            try
            {
                var YTDownloadService = new YtDlpDownloadService(ytDlpPath, ffmpegPath);
                var playlist = await YTDownloadService.GetPlaylistVideosAsync(playlistUrl);
                dGV_PlayList.Rows.Clear();
                if (playlist.Videos != null && playlist.Videos.Count > 0)
                {
                    //暫存目前取得的播放清單媒體資訊
                    playlistVideos = playlist.Videos.DeepClone();

                    //UI顯示
                    foreach (var video in playlist.Videos)
                    {
                        dGV_PlayList.Rows.Add(
                            video.IsSelected,       // CheckBox 欄
                            video.Index,            // #
                            video.Id,                   //Id（隱藏欄位）
                            video.DisplayTitle,     // 標題（自動 fallback 到 ID）
                            video.Uploader,         // 頻道
                            video.DurationString,    // 時長（mm:ss / hh:mm:ss）
                            video.WebpageUrl        // 連結（隱藏欄位）
                        );
                    }
                }
                return (playlist.IsSuccess, playlist.Message);
            }
            catch (Exception ex)
            {
                return (false, $"獲取播放列表資訊時發生錯誤: {ex.Message}");
            }
        }

        private void cB_SelectedAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (dGV_PlayList.RowCount > 0)
            {
                bool isChecked = cB_SelectedAll.Checked;
                foreach (DataGridViewRow row in dGV_PlayList.Rows)
                {
                    row.Cells["colSelected"].Value = isChecked;
                }
            }
        }

        private async void btn_Download_Click(object sender, EventArgs e)
        {
            // ── 1. 收集已勾選的項目 ─────────────────────────────────────────
            // 從 Grid 取出勾選列的 ID，再從暫存的 playlistVideos 查回完整物件
            var selectedItemsId = new List<string>();
            foreach (DataGridViewRow row in dGV_PlayList.Rows)
            {
                if (row.Cells["colSelected"].Value is true)
                    selectedItemsId.Add(row.Cells["colId"].Value?.ToString() ?? string.Empty);
            }

            if (selectedItemsId.Count == 0)
            {
                MessageBox.Show("請至少選取一個項目。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 根據 ID 從暫存的 playlistVideos 中找到對應的完整資訊
            var selectedVideoItems = selectedItemsId
                .Select(id => playlistVideos.FirstOrDefault(v => v.Id == id))
                .Where(v => v != null)
                .Cast<PlaylistVideoItem>()
                .ToList();

            if (selectedVideoItems.Count == 0)
            {
                MessageBox.Show("找不到對應的影片資訊，請重新載入播放清單。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ── 2. 判斷媒體類型 ──────────────────────────────────────────────
            var    mediaTypeValue   = mainForm.SelectedMediaTypeValue;
            bool   isAudio          = mediaTypeValue.Equals("Audio", StringComparison.OrdinalIgnoreCase);
            string mediaTypeDisplay = isAudio ? "音訊" : "視訊";

            // ── 3. 在 Main 下載清單中預先新增所有列（等待中）─────────────────
            var rowIndices = new int[selectedVideoItems.Count];
            for (int i = 0; i < selectedVideoItems.Count; i++)
                rowIndices[i] = mainForm.AddDownloadItem(selectedVideoItems[i].DisplayTitle, mediaTypeDisplay);

            // ── 4. 停用按鈕，防止重複觸發 ────────────────────────────────────
            btn_Download.Enabled = false;

            var downloadService = new YtDlpDownloadService(ytDlpPath, ffmpegPath);
            var semaphore       = new SemaphoreSlim(3);   // 最多 3 個並發
            var tasks           = new List<Task>();

            for (int i = 0; i < selectedVideoItems.Count; i++)
            {
                // 使用區域變數，避免 closure 捕捉迴圈變數
                int    rowIndex = rowIndices[i];
                var    item     = selectedVideoItems[i];
                string dir      = downloadDir;

                // ── 5. 建立可重複使用的下載 Action（暫停後「繼續」時由 Main 呼叫）──
                Func<CancellationToken, Task> downloadAction = async (ct) =>
                {
                    mainForm.UpdateDownloadProgress(rowIndex, 0, "下載中");

                    DownloadResult result;
                    if (isAudio)
                    {
                        result = await downloadService.DownloadAudioAsync(
                            url:               item.WebpageUrl!,
                            outputFolder:      dir,
                            onProgress:        pct => mainForm.UpdateDownloadProgress(rowIndex, pct, "下載中"),
                            cancellationToken: ct);
                    }
                    else
                    {
                        result = await downloadService.DownloadVideoAsync(
                            url:               item.WebpageUrl!,
                            outputFolder:      dir,
                            onProgress:        pct => mainForm.UpdateDownloadProgress(rowIndex, pct, "下載中"),
                            cancellationToken: ct);
                    }

                    // ct 被取消 = 使用者主動按了「暫停」→ Main 已將狀態設為「已暫停」，不覆蓋
                    if (ct.IsCancellationRequested) return;

                    if (result.IsSuccess)
                    {
                        mainForm.UpdateDownloadProgress(rowIndex, 100, "完成");
                        mainForm.SetActionButton(rowIndex, "—");   // 完成後停用按鈕
                    }
                    else
                    {
                        mainForm.UpdateDownloadProgress(rowIndex, 0, "失敗");
                        mainForm.SetActionButton(rowIndex, "重試");
                        logger.LogError("下載失敗 [{Title}]：{Message}", item.DisplayTitle, result.Message);
                    }
                };

                // 向 Main 登錄此任務的控制器，讓「繼續」按鈕可重啟
                var controller = mainForm.RegisterDownload(rowIndex, downloadAction);

                // ── 6. 加入初始批次下載 Task（受 semaphore 並發控制）─────────
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await downloadAction(controller.Cts.Token);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            // 等候所有初始任務完成（使用者自行「繼續」的任務不在此等候）
            await Task.WhenAll(tasks);

            // ── 7. 初始批次結束 ───────────────────────────────────────────────
            btn_Download.Enabled = true;
            logger.LogInformation("播放清單批次下載完成，共 {Count} 個項目。", selectedVideoItems.Count);
            MessageBox.Show(
                $"批次下載完成！共 {selectedVideoItems.Count} 個項目。\n（若有已暫停的項目，可在主視窗按「繼續」繼續下載。）",
                "下載完成",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btn_Cancel_Click(object sender, EventArgs e) => this.Close();

    }
}
