using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EEE527_Mini_Project
{
    public partial class Form1 : Form
    {
        string filepath = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "DATA COMMUNICATION MINI PROJECT";

            nudNumStation.Maximum = 10000;
            nudMinStationDist.Maximum = 10000;
            nudMaxStationDist.Maximum = 10000;
            nudDuration.Maximum = 10000;
            nudBandwitdh.Maximum = 10000;

            nudNumStation.Value = 50;
            nudMinStationDist.Value = 10;
            nudMaxStationDist.Value = 100;
            nudDuration.Value = 10;
            nudBandwitdh.Value = 1;

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            filepath = ofd.FileName;
            pictureBox1.Image = Bitmap.FromFile(filepath);
            lblPicinfo.Text = $"Path: {filepath}\nSize: {new FileInfo(filepath).Length.ToString()} bytes";
        }

        private void btnSimulate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(filepath)) throw new Exception("Image not found");

                Bitmap b = new Bitmap(filepath);
                int NumStation = (int)nudNumStation.Value;
                int MinStationDist = (int)nudMinStationDist.Value;
                int MaxStationDist = (int)nudMaxStationDist.Value;
                int Bandwidth = (int)nudBandwitdh.Value;
                int Duration = (int)nudDuration.Value;

                ALOHA.Execute(b, NumStation, MinStationDist, MaxStationDist, Bandwidth, Duration, richTextBox1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}