using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SubtitlesOverlay
{
    public partial class Settings : Form
    {
        public static Settings settingsInstance;
        public Settings()
        {
            InitializeComponent();
            settingsInstance = this;
        }

        private void closeSettings_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        private void opacityTrack_Scroll(object sender, EventArgs e)
        {
            MainOverlay.mainInstance.OpacityChange((int)opacityTrack.Value);
        }

        private void fontTrack_Scroll(object sender, EventArgs e)
        {
            MainOverlay.mainInstance.SubitilesFontSizeChange((int)fontTrack.Value);
        }
    }
}
