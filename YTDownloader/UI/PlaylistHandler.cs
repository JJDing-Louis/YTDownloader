using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YTDownloader
{
    public partial class PlaylistHandler : Form
    {
        private string playlistUrl;
        private Main mainForm;


        public PlaylistHandler()
        {
            InitializeComponent();
        }

        public PlaylistHandler(string playlistUrl, Main mainForm) : this()
        {
            this.playlistUrl = playlistUrl;
            this.mainForm = mainForm;
        }

        public bool GetPlaylistInfo()
        {
            return true;
        }
    }
}
