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
using System.Numerics;

namespace Minecraft_PixelArt_Maker
{
    public partial class Form1 : Form
    {
        static string WorkingFolder = "C:\\Blocks\\";
        static Bitmap[] bitmaps = new Bitmap[1000];
        static Color[] colors = new Color[1000];
        static int BitMapCount = 0;
        static Vector4 multipliers;
        const string ExceptionBoxTitle = "Exception ocurred";
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
            //Load Block Bitmaps and size of colors array
            try
            {
                Array.Resize(ref bitmaps, 1000);
                Array.Resize(ref colors, 1000);
                BitMapCount = 0;
                foreach (string file in Directory.GetFiles(WorkingFolder))
                {
                    bitmaps[BitMapCount] = new Bitmap(file);
                    BitMapCount++;
                }
                Array.Resize(ref bitmaps, BitMapCount);
                Array.Resize(ref colors, BitMapCount);
                int i = 0;
                foreach (Bitmap bitmap in bitmaps)
                {
                    colors[i] = GetAverageColor(bitmap);
                    i++;
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, ExceptionBoxTitle);
            }
        }

        static Color GetAverageColor(Bitmap image)
        {
            try
            {
                decimal alpha = 0, red = 0, green = 0, blue = 0, pixuls = 0;
                for (int i = 0; i < image.Width; i++)
                {
                    for (int j = 0; j < image.Height; j++)
                    {
                        /*i = width, j = height */
                        Color pixel = image.GetPixel(i, j);
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
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Idk how tf you managed to do this");
                return Color.FromArgb(69, 69, 69, 69);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                multipliers = new Vector4((float)numericUpDown1.Value, (float)numericUpDown2.Value, (float)numericUpDown3.Value, (float)numericUpDown4.Value);
                Thread thread = new Thread(DoTheThing);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, ExceptionBoxTitle);
            }
        }
        public void DoTheThing()
        {
            try
            {
                GetReady();
                string selectedPath = "";
                OpenFileDialog openFileDialog1 = new OpenFileDialog
                {
                    Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tiff",
                    RestoreDirectory = true
                };
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    selectedPath = openFileDialog1.FileName;
                    openFileDialog1.Dispose();
                }
                else
                {
                    openFileDialog1.Dispose();
                    Thread.CurrentThread.Abort();
                    Thread.Sleep(-1);
                }
                Bitmap selectedImage = new Bitmap(selectedPath);
                Bitmap colorConvertedImage = new Bitmap(selectedImage.Width, selectedImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (progressBar1.InvokeRequired)
                    progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Maximum = selectedImage.Width; }));
                for (int i = 0; i < selectedImage.Width; i++)
                {
                    for (int j = 0; j < selectedImage.Height; j++)
                    {
                        /*i = width, j = height*/
                        colorConvertedImage.SetPixel(i, j, Method1(selectedImage.GetPixel(i, j)));
                    }
                    progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Value = i + 1; }));
                }
                //stage 2
                //winforms cross thread shit
                if (pictureBox2.InvokeRequired)
                    pictureBox2.Invoke(new MethodInvoker(delegate { pictureBox2.BackgroundImage = colorConvertedImage; progressBar2.Maximum = colorConvertedImage.Width; }));
                Bitmap result = new Bitmap(colorConvertedImage.Width * 16, colorConvertedImage.Height * 16, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(result);
                for (int i = 0; i < colorConvertedImage.Width; i++)
                {
                    for (int j = 0; j < colorConvertedImage.Height; j++)
                    {
                        /*i = width, j = height*/
                        int indexer = 0;
                        Color gotPixel = colorConvertedImage.GetPixel(i, j);
                        foreach (Color color in colors)
                        {
                            if (gotPixel == color)
                            {
                                Point point = new Point(i * 16, j * 16);
                                g.DrawImage(bitmaps[indexer], point);
                            }
                            indexer++;
                        }
                    }
                    progressBar2.Invoke(new MethodInvoker(delegate { progressBar2.Value = i + 1; }));
                }
                g.Dispose();
                if (pictureBox3.InvokeRequired)
                {
                    pictureBox3.Invoke(new MethodInvoker(delegate { pictureBox3.BackgroundImage = result; }));
                }
            }
            catch (Exception exc)
            {
                if (exc.Message == "Parameter is not valid.")
                {
                    MessageBox.Show("An exception occured because the image being processed is too big. Please try to lower the resolution of the image and try again.", "Exception");
                }
                else if (exc.Message == "Thread was being aborted.")
                {

                }
                else
                {
                    MessageBox.Show(exc.Message, ExceptionBoxTitle);
                }
            }
        }
        static Color Method1(Color input)
        {
            try
            {
                byte a = input.A;
                byte r = input.R;
                byte g = input.G;
                byte b = input.B;
                Vector4 aways;
                int BestAway = 2949000;
                Color BestMatch = Color.Black;
                foreach (Color color in colors)
                {
                    aways = new Vector4(a - color.A, r - color.R, g - color.G, b - color.B);
                    if (aways.X < 0)
                    {
                        aways.X *= -1;
                    }
                    if (aways.Y < 0)
                    {
                        aways.Y *= -1;
                    }
                    if (aways.Z < 0)
                    {
                        aways.Z *= -1;
                    }
                    if (aways.W < 0)
                    {
                        aways.W *= -1;
                    }
                    aways *= multipliers;
                    int awaysSum = (int)(aways.X + aways.Y + aways.Z + aways.W);
                    if (awaysSum < BestAway)
                    {
                        BestAway = awaysSum;
                        BestMatch = color;
                    }
                }
                return BestMatch;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Idk how tf you managed to do this");
                return Color.FromArgb(69, 69, 69, 69);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.ShowDialog();
                Image save = pictureBox3.BackgroundImage;
                save.Save(saveFileDialog1.FileName);
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, ExceptionBoxTitle);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                Image img = pictureBox3.BackgroundImage;
                Clipboard.SetImage(img);
                img.Dispose();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, ExceptionBoxTitle);
            }
        }
    }
}