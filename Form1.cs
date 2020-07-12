#define DEBUG
using System;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace Minecraft_PixelArt_Maker
{
    public partial class Form1 : Form
    {
        static string WorkingFolder = "C:\\Blocks\\";
        static Bitmap[] bitmaps = new Bitmap[1000];
        static Color[] colours = new Color[1000];
        static int BitMapCount = 0;
        static Decimal mult_A;
        static Decimal mult_R;
        static Decimal mult_G;
        static Decimal mult_B;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            WorkingFolder = folderBrowserDialog1.SelectedPath;
        }
        static void GetReady()
        {
            Array.Resize(ref bitmaps, 1000);
            Array.Resize(ref colours, 1000);
            BitMapCount = 0;
            foreach (string file in Directory.GetFiles(WorkingFolder))
            {
                bitmaps[BitMapCount] = new Bitmap(file);
                BitMapCount++;
            }
            Array.Resize(ref bitmaps, BitMapCount);
            Array.Resize(ref colours, BitMapCount);
            Console.WriteLine(bitmaps.Length + " Bitmaps");
            int i = 0;
            Bitmap[] bitboi = bitmaps;
            foreach (Bitmap bitmap in bitboi)
            {
                colours[i] = GetAverageColor(bitmap);
                i++;
            }
        }

        static Color GetAverageColor(Bitmap image)
        {
            decimal alpha = 0, red = 0, green = 0, blue = 0, pixuls = 0;
            Bitmap img = image;
            for (int i = 0; i < img.Width; i++)
            {
                Console.WriteLine(i);
                for (int j = 0; j < img.Height; j++)
                {
                    /*i = width, j = height */
                    Color pixel = img.GetPixel(i, j);
                    alpha += pixel.A;
                    red += pixel.R;
                    green += pixel.G;
                    blue += pixel.B;
                    pixuls++;
                }
            }
            Color average = Color.FromArgb(Convert.ToInt32(alpha / pixuls), Convert.ToInt32(red / pixuls), Convert.ToInt32(green / pixuls), Convert.ToInt32(blue / pixuls));
            return average;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mult_A = numericUpDown4.Value;
            mult_R = numericUpDown3.Value;
            mult_G = numericUpDown2.Value;
            mult_B = numericUpDown1.Value;
            Thread thread = new Thread(DoTheThing);
            thread.SetApartmentState(ApartmentState.STA); //
            thread.Start();
        }
        public void DoTheThing()
        {
            GetReady();
            string selectedPath = "";
            OpenFileDialog saveFileDialog1 = new OpenFileDialog();
            saveFileDialog1.Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tiff";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedPath = saveFileDialog1.FileName;
            }
            Bitmap imge = new Bitmap(selectedPath);
            Bitmap idk = new Bitmap(imge.Width, imge.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Maximum = imge.Width; }));
            }
            for (int i = 0; i < imge.Width; i++)
            {
                for (int j = 0; j < imge.Height; j++)
                {
                    /*i = width, j = height*/
                    idk.SetPixel(i, j, Method1(imge.GetPixel(i, j)));
                }
                progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Value = i + 1; }));
            }
            //stage 2
            //winforms cross thread shit
            if (pictureBox2.InvokeRequired)
            {
                pictureBox2.Invoke(new MethodInvoker(delegate { pictureBox2.BackgroundImage = idk; progressBar2.Maximum = idk.Width; }));
            }
            Bitmap res = new Bitmap(idk.Width * 16, idk.Height * 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            for (int i = 0; i < idk.Width; i++)
            {
                for (int j = 0; j < idk.Height; j++)
                {
                    /*i = width, j = height*/
                    int ind = 0;
                    foreach (Color color in colours)
                    {
                        if (idk.GetPixel(i, j) == color)
                        {
                            Point point = new Point(i * 16, j * 16);
                            Graphics g = Graphics.FromImage(res);
                            g.DrawImage(bitmaps[ind], point);
                            g.Dispose();
                        }
                        ind++;
                    }
                }
                progressBar2.Invoke(new MethodInvoker(delegate { progressBar2.Value = i + 1; }));
            }
            if (pictureBox3.InvokeRequired)
            {
                pictureBox3.Invoke(new MethodInvoker(delegate { pictureBox3.BackgroundImage = res; }));
            }
        }
        static Color Method1(Color input)
        {
            int a = input.A;
            int r = input.R;
            int g = input.G;
            int b = input.B;
            int i = 0;
            Decimal away_a;
            Decimal away_r;
            Decimal away_g;
            Decimal away_b;
            Decimal away;
            Decimal BestAway = 2949000;
            Color BestMatch = Color.Black;
            foreach (Color color in colours)
            {
                away_a = a - color.A;
                if (away_a < 0) { away_a *= -1; }
                away_a *= mult_A;
                away_r = r - color.R;
                if (away_r < 0) { away_r *= -1; }
                away_r *= mult_R;
                away_g = g - color.G;
                if (away_g < 0) { away_g *= -1; }
                away_g *= mult_G;
                away_b = b - color.B;
                if (away_b < 0) { away_b *= -1; }
                away_b *= mult_B;
                away = 0;
                away = away_a;
                away += away_r + away_g + away_b;
                if (away < 0)
                {
                    away *= -1;
                }
                if (away < BestAway)
                {
                    //Console.WriteLine("New best match, Last away" + BestAway + ", New away " + away);
                    //Console.WriteLine("old color =" + BestMatch.ToString() + ", New color =" + color.ToString());
                    BestAway = away;
                    BestMatch = color;
                }
                i++;
            }
            return BestMatch;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            Image save = pictureBox3.BackgroundImage;
            save.Save(saveFileDialog1.FileName);
        }
    }
}