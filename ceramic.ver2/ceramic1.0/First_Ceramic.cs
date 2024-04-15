using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class First_Ceramic
    {
        Image image;
        Bitmap bitmap;
        Bitmap roiBitmap;
        Bitmap imgBitmap;
        Bitmap roi8Bitmap;
        Bitmap rBitmap;
        Color color;
        Bitmap roiOriginal;

        int[,] arr;  // Width, Height
        int[,] arr2; // Height, Width
        int[,] arr3; // H, W
        int[,] arr8;
        int[,] ar;
        int i, j;
        int width, height;

        int[,] roi;
        int[,] roi8;
        public int[] roiHeight = new int[2];
        public int[] roi8Height = new int[2];
        int CT;


       

        public First_Ceramic(Image image, int CT)
        {

            this.image = image;
            this.CT = CT;

            width = 318;
            height = 284;
            bitmap = new Bitmap(this.image, new Size(width, height));
            imgBitmap = new Bitmap(width, height);
            rBitmap = new Bitmap(width, height);


            arr = new int[imgBitmap.Width, imgBitmap.Height];
            arr2 = new int[imgBitmap.Height, imgBitmap.Width];
            arr3 = new int[imgBitmap.Height, imgBitmap.Width];
            arr8 = new int[imgBitmap.Height, imgBitmap.Width];


        }

        public Bitmap run()
        {

            gray();
            Gaussian g = new Gaussian();
            arr2 = g.gaussianArr(arr2, bitmap);

            Mask m = new Mask();
            arr2 = m.mask(arr2, bitmap);

            roiGray();
            Binary b = new Binary();
            roiHeight = b.roi(arr2, bitmap);


            roi = new int[roiHeight[1] - roiHeight[0], bitmap.Width];
            Console.WriteLine("길이" + (roiHeight[1] - roiHeight[0]));
            roiBitmap = new Bitmap(image, new Size(width, roiHeight[1] - roiHeight[0]));
            ar = new int[roiHeight[1] - roiHeight[0], bitmap.Width];


            roiArr(arr3);

            roiArrToBitmap(roi);
            rBitmap = roiBitmap;
            for (i = 0; i < rBitmap.Height; i++)
            {
                for (j = 0; j < rBitmap.Width; j++)
                {
                    color = rBitmap.GetPixel(j, i);
                    arr3[i, j] = (int)color.R;
                }
            }

            int h = 0;

            for (i = roiHeight[0]; i < roiHeight[1]; i++)
            {
                for (j = 0; j < roiBitmap.Width; j++)
                {
                    ar[h, j] = arr3[i, j];
                }
                h++;
            }


            roiOriginal = roiArrToBitmap2(ar);

            roi = m.mask(roi, roiBitmap);

            roiArrToBitmap(roi);


            FuzzyStretching fuzzyStretching = new FuzzyStretching();
            roiBitmap = fuzzyStretching.Stretching(roiBitmap);


            Erase er = new Erase(roiBitmap, ar,CT);

            PCM2 pCM2 = new PCM2(bitmap);
            Bitmap pcmImage = pCM2.PCMprocessing();

            //PCM pcm = new PCM(roiOriginal);
            //Bitmap[] pcmimage = pcm.run();

            //Bitmap[] pcmImage = pcm.run();
            ////Erase er = new Erase(pcmImage[0], ar);

            //return fullImageProcessing(pcmImage);
            //return roiImageProcessing(pcmimage[4]);
            //return er.eraser();
            return pcmImage;



        }

        private int[,] gray(Bitmap bitmap)
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

        private Bitmap display(int[,] arr, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            Color color;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    color = Color.FromArgb(arr[x, y], arr[x, y], arr[x, y]);
                    bitmap.SetPixel(x, y, color);
                }

            return bitmap;
        }

        private void gray() // arr값 gray
        {
            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    color = bitmap.GetPixel(j, i);
                    arr2[i, j] = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                }
            }
        }

        private void arrToBitmap(int[,] arr) // arr 값 > bitmap
        {
            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    color = Color.FromArgb(arr[i, j], arr[i, j], arr[i, j]);
                    bitmap.SetPixel(j, i, color);
                }
            }

        }

        private void roiGray() // arr값 gray
        {
            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    color = bitmap.GetPixel(j, i);
                    arr3[i, j] = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                }
            }
        }

        private void roiArrToBitmap(int[,] arr) // arr 값 > bitmap
        {
            for (i = 0; i < arr.GetLength(0); i++)
            {
                for (j = 0; j < arr.GetLength(1); j++)
                {
                    color = Color.FromArgb(arr[i, j], arr[i, j], arr[i, j]);
                    roiBitmap.SetPixel(j, i, color);
                }
            }
        }

        private Bitmap roiArrToBitmap2(int[,] arr) // arr 값 > bitmap
        {
            Bitmap newimage = new Bitmap(arr.GetLength(1), arr.GetLength(0));
            for (i = 0; i < arr.GetLength(0); i++)
            {
                for (j = 0; j < arr.GetLength(1); j++)
                {
                    color = Color.FromArgb(arr[i, j], arr[i, j], arr[i, j]);
                    newimage.SetPixel(j, i, color);
                }
            }

            return newimage;
        }

        private void roiArr(int[,] arr)
        {
            int h = 0;

            for (i = roiHeight[0]; i < roiHeight[1]; i++)
            {
                for (j = 0; j < roiBitmap.Width; j++)
                {
                    roi[h, j] = arr[i, j];
                }
                h++;
            }
        }

        private void roi8Gray() // arr값 gray
        {
            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    color = bitmap.GetPixel(j, i);
                    arr8[i, j] = (int)color.R;
                }
            }
        }


        private void roi8ArrToBitmap(int[,] arr) // arr 값 > bitmap
        {
            for (i = 0; i < arr.GetLength(0); i++)
            {
                for (j = 0; j < arr.GetLength(1); j++)
                {
                    color = Color.FromArgb(arr[i, j], arr[i, j], arr[i, j]);
                    roi8Bitmap.SetPixel(j, i, color);
                }
            }
        }

        private void roi8Arr(int[,] arr)
        {
            int h = 0;

            for (i = roi8Height[0]; i < roi8Height[1]; i++)
            {
                for (j = 0; j < roi8Bitmap.Width; j++)
                {
                    roi8[h, j] = arr[i, j];
                }
                h++;
            }

        }

        private Bitmap roiImageProcessing(Bitmap pcmImage)
        {

            Color color;
            int width = pcmImage.Width;
            int height = pcmImage.Height;

            Bitmap finalROI = new Bitmap(width, height);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    color = pcmImage.GetPixel(x, y);

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                        finalROI.SetPixel(x, y, color);
                    else
                        finalROI.SetPixel(x, y, roiOriginal.GetPixel(x, y));

                }
            return finalROI;
        }

        private Bitmap fullImageProcessing(Bitmap pcmImage)
        {

            Color color;
            int width = pcmImage.Width;
            int height = pcmImage.Height;

            Bitmap finalImage = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    finalImage.SetPixel(x, y, bitmap.GetPixel(x, y));
                }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    color = pcmImage.GetPixel(x, y);

                    if (color.R != 0 || color.G != 0 || color.B != 0)
                        finalImage.SetPixel(x, y + roiHeight[0], color);

                }

            return finalImage;
        }
    }
}
