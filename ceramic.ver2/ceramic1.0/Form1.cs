using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ceramic1._0
{
    public partial class Form1 : Form
    {
        Image image;
        Bitmap originalBm;
        Bitmap copyBm;
        Bitmap pcmImage;


        Bitmap roiImage;
        Bitmap bitcut, bit10, bit50, bit70, bit100;
        Bitmap[] bitmap; 

        int width, height;
        int roiwidth, roiheight;
        int[,] grayArr;
        int CT;



        int[,] gray(Bitmap bitmap)
        {
            Color color;
            int[,] arr = new int[bitmap.Width, bitmap.Height];

            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    color = bitmap.GetPixel(x, y);
                    arr[x, y] = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                }

            return arr;
            
        }

        Bitmap display(int[,] arr, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width,height);
            Color color;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    color = Color.FromArgb(arr[x, y], arr[x, y], arr[x, y]);
                    bitmap.SetPixel(x, y, color);
                }

            return bitmap;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select File";
            openFileDialog1.Filter = "All File(*.*) |*.*| Bitmap File(*.bmp) | *.bmp | Jpeg File(*.jpg) | *.jpg";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string strFilename = openFileDialog1.FileName;
                image = Image.FromFile(strFilename);
                originalBm = new Bitmap(image);
                copyBm = new Bitmap(image);
                width = originalBm.Width;
                height = originalBm.Height;

                FuzzyStretching fuzzyStretching = new FuzzyStretching();
                copyBm = fuzzyStretching.Stretching(copyBm);

                pictureBox1.Image = originalBm;
               


            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            PCM pcm = new PCM(copyBm);

            bitmap = pcm.run();

            this.CT = pcm.CT;

            pcmImage = bitmap[0];
            grayArr = gray(pcmImage);


            ROI roi = new ROI();
            int[] size = roi.run(grayArr, width, height, CT);
            this.roiwidth = width;
            this.roiheight = size[2];

            this.roiImage = new Bitmap(roiwidth, roiheight);
            this.bitcut = new Bitmap(roiwidth, roiheight);
            this.bit10 = new Bitmap(roiwidth, roiheight);
            this.bit50 = new Bitmap(roiwidth, roiheight);
            this.bit70 = new Bitmap(roiwidth, roiheight);
            this.bit100 = new Bitmap(roiwidth, roiheight);

            


            Color c_cut, c_10, c_50, c_70, c_100, color;
            for (int x = 0; x < roiwidth; x++)
                for (int y = 0; y < roiheight; y++)
                {
                    color = originalBm.GetPixel(x, size[0] + y);
                    c_cut = bitmap[0].GetPixel(x, size[0] + y); //bitcut
                    c_10 = bitmap[1].GetPixel(x, size[0] + y); //bit10
                    c_50 = bitmap[2].GetPixel(x, size[0] + y);
                    c_70 = bitmap[3].GetPixel(x, size[0] + y);
                    c_100 = bitmap[4].GetPixel(x, size[0] + y);
                    

                    
                    roiImage.SetPixel(x, y, color);
                    bitcut.SetPixel(x, y, c_cut);
                    bit10.SetPixel(x, y, c_10);
                    bit50.SetPixel(x, y, c_50);
                    bit70.SetPixel(x, y, c_70);
                    bit100.SetPixel(x, y, c_100);


                    if (c_10.R == 0 && c_10.G == 0 && c_10.B == 0)                       
                        bit10.SetPixel(x, y, color);
                    
                    if (c_50.R == 0 && c_50.G == 0 && c_50.B == 0)
                        bit50.SetPixel(x, y, color);

                    if (c_70.R == 0 && c_70.G == 0 && c_70.B == 0)
                        bit70.SetPixel(x, y, color);

                }


            
            pictureBox2.Image = roiImage;
            pictureBox3.Image = bit10;
            pictureBox4.Image = bit50;
            pictureBox5.Image = bit70;
            pictureBox6.Image = bit100;
            pictureBox8.Image = bitmap[4];


        }


        private void rOIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Bitmap testbit1, testbit2, testbit3, testbit4, testbit5;
            //int[,] tarr1, tarr2, tarr3, tarr4, tarr5;



            ////int[,] roigrayArr = gray(roiImage);
            //int[,] roigrayArr = gray(bit50);



            ////binary
            //ImageProcessing ip = new ImageProcessing();
            //tarr1 = ip.bin2(roigrayArr, roiwidth, roiheight, 255, 0);
            //testbit1 = display(tarr1, roiwidth, roiheight);
            //pictureBox3.Image = testbit1;

            ////sobel
            //tarr2 = ip.sobel2(roigrayArr, roiwidth, roiheight);
            //tarr2 = ip.gaussian2(tarr2, roiwidth, roiheight);
            //tarr2 = ip.gaussian2(tarr2, roiwidth, roiheight);
            ////tarr2 = ip.gaussian(tarr2, roiwidth, roiheight);
            ////tarr2 = ip.gaussian(tarr2, roiwidth, roiheight);
            ////tarr2 = ip.gaussian(tarr2, roiwidth, roiheight);
            //testbit2 = display(tarr2, roiwidth, roiheight);
            //pictureBox4.Image = testbit2;

            /////////////////////////////TEST//////////////////////////////////////


            ////fuzzy binary
            //tarr3 = ip.Stretching(testbit2);
            //tarr3 = ip.bin(tarr3, roiwidth, roiheight, 255, 0);

            ////Bitmap bi = ip.erase(roiImage, tarr3, roiwidth, roiheight);

            //pictureBox5.Image = display(tarr3, roiwidth, roiheight);
            ////pictureBox5.Image = bi;

            ////int[,] testarr = gray(pcmroiImage);
            ////int[,] testarr2 = gray(pcmroiImage2);
            ////int[,] testarr3 = gray(pcmroiImage4);


            ////int tt = roiheight / 2;
            ////if (testarr[roiwidth / 2, 0] == 0)
            ////{

            ////    for (int x = 0; x < roiwidth; x++)
            ////        for (int y = tt; y < roiheight; y++)
            ////        {
            ////            if (testarr2[x, y] == 0)
            ////                testarr[x, y] = 0;
            ////        }

            ////    for (int x = 0; x < roiwidth; x++)
            ////        for (int y = roiheight - 11; y < roiheight; y++)
            ////        {
            ////            testarr[x, y] = 0;
            ////        }
            ////    //for (int x = 0; x < roiwidth; x++)
            ////    //    for (int y = tt * 2; y < roiheight; y++)
            ////    //    {
            ////    //        if (testarr3[x, y] == 0)
            ////    //            testarr[x, y] = 0;
            ////    //    }
            ////}
            ////else
            ////{
            ////    for (int x = 0; x < roiwidth; x++)
            ////        for (int y = 0; y < tt; y++)
            ////        {
            ////            if (testarr2[x, y] == 0)
            ////                testarr[x, y] = 0;
            ////        }


            ////}

        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            First_Ceramic first = new First_Ceramic(image, CT);

       

            pictureBox8.Image = first.run();
        }
    }
}
