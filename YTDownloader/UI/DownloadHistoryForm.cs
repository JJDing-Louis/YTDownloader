using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YTDownloader.Tool;

namespace YTDownloader
{
    public partial class DownloadHistoryForm : Form
    {
        
        public DownloadHistoryForm()
        {
            GUITool.ApplyStartupFontFromConfig(this);
            InitializeComponent();
            GUITool.ApplyFromConfig(this);
        }
        
        public DownloadHistoryForm(MainForm main) : this()
        {
            ///TODO:
            ///讀歷史紀錄，並顯示

            Init();
        }

        private void Init()
        {
            
        }

        private void InitUI()
        {
            
        }

        private void InitConfig()
        {
            
        }
        
        private void InitOptions()
        {
            
        }
    }
}
