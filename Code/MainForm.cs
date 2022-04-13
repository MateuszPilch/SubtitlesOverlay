using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace SubtitlesOverlay
{
    public partial class MainOverlay : Form
    {
        public static MainOverlay mainInstance;
        private Settings settingsForm;
        
        private DateTime currentTime = new DateTime(), totalTime = new DateTime();
        private List<SubtitlesFormat> sfList = new List<SubtitlesFormat>();

        const int cGrip = 16;
        const int cCaption = 32;

        int currentIndex = 0;
        bool isPlaying = false;

        public MainOverlay()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            mainInstance = this;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                
                if (pos.Y < this.ClientSize.Height - cCaption)
                {
                    m.Result = (IntPtr)2;
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17;
                    return;
                }
            }
            base.WndProc(ref m);
        }
        private void ParseSubtitles()
        {
            openFileDialog.ShowDialog();
            string fileName = openFileDialog.FileName;
            FileStream fileSource = null;

            try
            {
                fileSource = File.OpenRead(fileName);
                using var sr = new StreamReader(fileSource,true);
                string line;
                int currentParseType = 0;

                sfList.Clear();
                while ((line = sr.ReadLine()) != null)
                {
                    switch (currentParseType)
                    {
                        case 0:
                            if (line != "")
                            {
                                sfList.Add(new SubtitlesFormat());
                                currentParseType++;
                            }
                        break;

                        case 1:
                            sfList[^1].startTime = DateTime.ParseExact(line.Split("-->")[0].Trim(), "HH:mm:ss,fff", CultureInfo.CurrentCulture);
                            sfList[^1].endTime = DateTime.ParseExact(line.Split("-->")[1].Trim(), "HH:mm:ss,fff", CultureInfo.CurrentCulture);
                            currentParseType++;
                        break;

                        case 2:
                            if(line != "")
                            {
                                sfList[^ 1].subtitlesContent = String.Concat(sfList[^1].subtitlesContent, "\n", line);
                            }
                            else
                            {
                                currentParseType = 0;
                            }
                        break;         
                    }
                }
                
                totalTime = sfList[^1].endTime;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (fileSource != null)
            {
                fileSource.Close();
            }

        }
        private void TimeController()
        {
            if(isPlaying && sfList.Count > 0 && TimeSpan.Compare(currentTime.TimeOfDay, totalTime.TimeOfDay) < 0)
            {
                currentTime = currentTime.AddMilliseconds(250); 
                timelineSub.Value = Math.Clamp((int)((currentTime.TimeOfDay.TotalMilliseconds / totalTime.TimeOfDay.TotalMilliseconds) * timelineSub.Maximum),0,timelineSub.Maximum);
                timeSub.Text = currentTime.ToString("HH:mm:ss");
                
                if (currentIndex  < sfList.Count)
                {
                    if(TimeSpan.Compare(currentTime.TimeOfDay, sfList[currentIndex].startTime.TimeOfDay) >= 0)
                    {
                        subtitlesContainer.Text = sfList[currentIndex].subtitlesContent;
                    }

                    if(TimeSpan.Compare(currentTime.TimeOfDay, sfList[currentIndex].endTime.TimeOfDay) >= 0)
                    {
                        subtitlesContainer.Text = "";
                        
                        currentIndex++;
                        
                    }
                }
            }
        }
        private void ButtonsVisibility(bool buttonStatus)
        {
            importButton.Visible = buttonStatus;
            playButton.Visible = buttonStatus;
            timeSub.Visible = buttonStatus;
            timelineSub.Visible = buttonStatus;
            settingsButton.Visible = buttonStatus;
            closeButton.Visible = buttonStatus;
        }
        private void playButton_Click(object sender, EventArgs e)
        {
            isPlaying = !isPlaying;

            if (isPlaying)
            {
                playButton.Image = Properties.Resources.Pause_Icon;
            }
            else
            {
                playButton.Image = Properties.Resources.Play_Icon;
            }
        }
        
        private void importButton_Click(object sender, EventArgs e)
        {
            ParseSubtitles();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            if(settingsForm == null)
            {
                settingsForm = new Settings();
            }
            settingsForm.Show();
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timelineSub_Scroll(object sender, EventArgs e)
        {
            currentTime = new DateTime().AddMilliseconds(((double)timelineSub.Value / (double)timelineSub.Maximum) * totalTime.TimeOfDay.TotalMilliseconds);
            timeSub.Text = currentTime.ToString("HH:mm:ss");
            subtitlesContainer.Text = "";

            for (int i = 0; i < sfList.Count; i++)
            {
                if (TimeSpan.Compare(currentTime.TimeOfDay, sfList[i].startTime.TimeOfDay) >= 0 && TimeSpan.Compare(currentTime.TimeOfDay, sfList[i].endTime.TimeOfDay) <= 0)
                {
                    currentIndex = i;
                    subtitlesContainer.Text = sfList[currentIndex].subtitlesContent;

                    break;
                }
            }
        }
        private void mainTime_Tick(object sender, EventArgs e)
        {
            TimeController();
        }
        private void MainOverlay_Activated(object sender, EventArgs e)
        {
            ButtonsVisibility(true);
        }
        private void MainOverlay_Deactivate(object sender, EventArgs e)
        {
            ButtonsVisibility(false);
        }
        public void OpacityChange(int newOpacity)
        {
            this.Opacity = newOpacity * 0.01;
        }
        public void SubitilesFontSizeChange(int newSize)
        {
            subtitlesContainer.Font = new Font(subtitlesContainer.Font.Name, newSize);
        }
    }
    public class SubtitlesFormat: Form
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string subtitlesContent { get; set; }
    }
}