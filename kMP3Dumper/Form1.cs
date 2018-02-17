using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kMP3Dumper
{
    public partial class Form1 : Form
    {

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        public Form1()
        {
            InitializeComponent();
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ffmpeg.exe")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\ffmpeg.exe", Properties.Resources.ffmpeg); }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ffplay.exe")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\ffplay.exe", Properties.Resources.ffplay); }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\ffprobe.exe")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\ffprobe.exe", Properties.Resources.ffprobe); }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\youtube-dl.exe")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\youtube-dl.exe", Properties.Resources.youtube_dl); }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\taglib-sharp.dll")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\taglib-sharp.dll", Properties.Resources.taglib_sharp); }
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\policy.2.0.taglib-sharp.dll")) { File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "\\policy.2.0.taglib-sharp.dll", Properties.Resources.policy_2_0_taglib_sharp); }
            Control.CheckForIllegalCrossThreadCalls = false;
            comboBox1.SelectedIndex = 0;
            panel1.MouseDown += Form1_MouseDown;
            panel3.MouseDown += Form1_MouseDown;
            label6.MouseDown += Form1_MouseDown;
            label7.MouseDown += Form1_MouseDown;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if ((textBox1.Text.Length > 3) && (textBox2.Text.Length > 3))
            {
                if (AppDomain.CurrentDomain.BaseDirectory != textBox2.Text) //Will fix in next release
                {
                new Thread((ThreadStart)(() =>
                {
                    var procargs = "";
                    if (radioButton2.Checked) { procargs = Properties.Resources.ytdl_args + " --yes-playlist --audio-quality " + trackBar1.Value.ToString() + " --audio-format " + comboBox1.SelectedItem.ToString() + " " + textBox1.Text; }
                    if (radioButton3.Checked) { procargs = Properties.Resources.ytdl_args + " --no-playlist --audio-quality " + trackBar1.Value.ToString() + " --audio-format " + comboBox1.SelectedItem.ToString() + " " + textBox1.Text; }
                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "youtube-dl.exe",
                            Arguments = procargs,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };
                    Log.Clear();
                    button2.Text = "Downloading...";
                    button2.Enabled = false;
                    button1.Enabled = false;
                    groupBox5.Enabled = false;
                    textBox1.ReadOnly = true;
                    textBox2.ReadOnly = true;
                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        Log.AppendText(proc.StandardOutput.ReadLine() + Environment.NewLine);
                    }
                    string[] files;
                    if (comboBox1.SelectedItem.ToString() == "opus")
                    {
                        files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.opus");
                    } else if (comboBox1.SelectedItem.ToString() == "vorbis")
                    {
                        files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.ogg");
                    }
                    else
                    {
                        files = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*." + comboBox1.SelectedItem.ToString());
                    }
                    foreach (string dlfile in files)
                    {
                        File.Copy(dlfile, textBox2.Text + "\\" + dlfile.Replace(AppDomain.CurrentDomain.BaseDirectory, ""));
                        File.Delete(dlfile);
                    }
                    string[] albart_imgs = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.jpg");
                    foreach (string aaimg in albart_imgs)
                    {
                        File.Delete(aaimg);
                    }
                    button2.Text = "Download";
                    button2.Enabled = true;
                    button1.Enabled = true;
                    groupBox5.Enabled = true;
                    textBox1.ReadOnly = false;
                    textBox2.ReadOnly = false;
                    MessageBox.Show("Done!", "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Information);
                })).Start();
            } else { MessageBox.Show("Please select a save location outside of the program's installed location!", "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Exclamation); }
            } else
            {
                MessageBox.Show("Please provide a valid URL and Save Path!", "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            savePath.ShowDialog();
            textBox2.Text = savePath.SelectedPath;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(LoadTags.FileName))
                {
                    TagLib.File file = TagLib.File.Create(LoadTags.FileName);
                    try { file.Tag.Title = textBox3.Text; } catch { }
                    try { file.Tag.Performers = null; file.Tag.Performers = new[] { textBox4.Text }; } catch { }
                    try { file.Tag.Album = textBox5.Text; } catch { }
                    try { file.Save(); } catch { }
                    MessageBox.Show("Tags applied!", "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong." + Environment.NewLine + "--------------------------------------" + Environment.NewLine + ex, "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LoadTags.ShowDialog();
            AlbumArt.Image = null;
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            try
            {
                if (File.Exists(LoadTags.FileName))
                {
                    TagLib.File file = TagLib.File.Create(LoadTags.FileName);
                    try { textBox3.Text = file.Tag.Title; } catch { }
                    try { textBox4.Text = file.Tag.Performers[0]; } catch { }
                    try { textBox5.Text = file.Tag.Album; } catch { }
                    try { MemoryStream ms = new MemoryStream(file.Tag.Pictures[0].Data.Data); AlbumArt.Image = System.Drawing.Image.FromStream(ms); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong." + Environment.NewLine + "--------------------------------------" + Environment.NewLine + ex, "kPanel - MP3 Dumper", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/knackrack615/kMP3Dumper");
        }

        private void Log_TextChanged(object sender, EventArgs e)
        {
            Log.SelectionStart = Log.Text.Length;
            Log.ScrollToCaret();
        }
    }
}
