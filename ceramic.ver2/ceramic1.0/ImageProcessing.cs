using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class ImageProcessing
    {

        public int width;
        public int height;
        public bool cut_test;
        public int ct;

        public int[,] bin(int[,] arr, int width, int height, int color1, int color2)
        {
            int[,] binArr = new int[width, height];

            int sum = 0;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    sum += arr[x, y];


            int T = (int)(sum / (width * height));

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (arr[x, y] > T)
                        binArr[x, y] = color1;
                    else
                        binArr[x, y] = color2;

            return binArr;

        }

        public int[,] test(int[,] grayArr, bool cut_test, int ct, int height)
        {

            this.height = height/5;
            int tt = this.height / 2;
            int[,] testArr = new int[width, this.height];

            if (ct < tt)
                return grayArr;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < this.height; y++)
                   testArr[x, y] = grayArr[x, ct-tt + y];


            return testArr;

        }


        public int[,] cut3(int[,] grayArr, int width, int color)
        {
            int[,] cutArr = new int[width, height];
            int[] length = new int[height];
            int[,] binArr = bin2(grayArr, width);
            for (int x = 50; x < width-50; x++)
                for (int y = 0; y < height; y++)
                    if (binArr[x, y] == color)
                        length[y]++;

            int start=0, end=height-1;
  
               
                for (start = 0; start < height; start++)
                    if (length[start] > 0)
                        break;

              
                for (end = height - 1; end > 0; end--)
                    if (length[end] > 0)
                        break;

                Console.WriteLine(start);
                Console.WriteLine(end);

           

            this.height = end - start;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    cutArr[x, y] = grayArr[x, start + y];

            return cutArr;


        }

        public int[,] bin2(int[,] arr, int width, int height, int color1, int color2)
        {
            int[,] binArr = new int[width, height];

            int sum = 0;
            int cnt = 0;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (arr[x, y] != 0)
                    {
                        sum += arr[x, y];
                        cnt++;
                    }
                    


            int T = (int)(sum / (cnt));

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (arr[x, y] > T)
                        binArr[x, y] = color1;
                    else
                        binArr[x, y] = color2;

            return binArr;

        }

        public int[,] bin2(int[,] arr, int width)
        {
            int[,] binArr = new int[width, height];

            int T = 128;
            int max = 0;
            int min = 255;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (arr[x, y] > max) max = arr[x, y];
                    else if (arr[x, y] < min) min = arr[x, y];

       

            T = (max + min) / 2;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (arr[x, y] > T) binArr[x, y] = 0;
                    else binArr[x, y] = 255;

          

 

            return binArr;

        }



        public int[,] gaussian2(int[,] arr, int width, int height)
        {
            double sigma = 1;
            int z = (int)Math.Round(-4 * sigma);
            int size = (int)Math.Round(8 * sigma + 1);

            double[] g1 = new double[size];
            double[,] mask = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                g1[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigma)) * (Math.Exp(-1 * ((z * z) / (2 * sigma * sigma))));
                z++;
            }

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    mask[i, j] = g1[i] * g1[j];



            int[,] gaussianArr = new int[width, height];
            int xPad = size / 2;
            int yPad = size / 2;
            double sum;

            for (int x = 0; x < width - 2 * xPad; x++)
                for (int y = 0; y < height - 2 * yPad; y++)
                {
                    sum = 0.0;
                    if (arr[x, y] != 0)
                    {
                        for (int c = 0; c < size; c++)
                            for (int r = 0; r < size; r++)
                                sum += arr[x + c, y + r] * mask[c, r];


                        if (sum > 255.0) sum = 255.0;
                        if (sum < 0.0) sum = 0.0;
                        gaussianArr[x + xPad, y + yPad] = (int)sum;
                    }
                }

            //gaussianArr = padding(gaussianArr, width, height, xPad, yPad);

            return gaussianArr;

        }

        public int[,] sobel2(int[,] arr, int width, int height)
        {

            double[,] sobel_X = {{-1.0, 0.0, 1.0},
                                 {-2.0, 0.0, 2.0},
                                 {-1.0, 0.0, 1.0}};


            double[,] sobel_Y = {{-1.0, -2.0, -1.0},
                                 {0.0, 0.0, 0.0},
                                 {1.0, 2.0, 1.0}};


            int[,] sobelArr = new int[width, height];
            int size = 3;
            int xPad = size / 2;
            int yPad = size / 2;
            double sum, sum1, sum2;

            for (int x = 0; x < width - 2 * xPad; x++)
                for (int y = 0; y < height - 2 * yPad; y++)
                {
                    sum1 = sum2 = 0.0;

                    if (arr[x, y] != 0)
                    {
                        for (int c = 0; c < size; c++)
                            for (int r = 0; r < size; r++)
                            {
                                sum1 += arr[x + c, y + r] * sobel_X[c, r];
                                sum2 += arr[x + c, y + r] * sobel_Y[c, r];
                            }

                        sum = Math.Abs(sum1) + Math.Abs(sum2);
                        if (sum > 255.0) sum = 255.0;
                        if (sum < 0.0) sum = 0.0;

                        sobelArr[x + xPad, y + yPad] = (int)sum;
                    }
                }



            //sobelArr = padding(sobelArr, width, height, xPad, yPad);

            return sobelArr;
        }

        int[,] padding(int[,] arr, int width, int height, int xPad, int yPad)
        {
            for (int x = 0; x < xPad; x++)
                for (int y = 0; y < height; y++)
                {
                    arr[x, y] = arr[xPad, y];
                    arr[width - 1 - x, y] = arr[width - 1 - xPad, y];
                }

            for (int y = 0; y < yPad; y++)
                for (int x = 0; x < width; x++)
                {
                    arr[x, y] = arr[x, yPad];
                    arr[x, height - 1 - y] = arr[x, height - 1 - yPad];
                }


            return arr;
        }

        public int[,] gaussian(int[,] arr, int width, int height)
        {
            double sigma = 1;
            int z = (int)Math.Round(-4 * sigma);
            int size = (int)Math.Round(8 * sigma + 1);

            double[] g1 = new double[size];
            double[,] mask = new double[size, size];

            for (int i = 0; i < size; i++)
            {
                g1[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigma)) * (Math.Exp(-1 * ((z * z) / (2 * sigma * sigma))));
                z++;
            }

            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    mask[i, j] = g1[i] * g1[j];



            int[,] gaussianArr = new int[width, height];
            int xPad = size / 2;
            int yPad = size / 2;
            double sum;

            for (int x = 0; x < width - 2 * xPad; x++)
                for (int y = 0; y < height - 2 * yPad; y++)
                {
                    sum = 0.0;
                    for (int c = 0; c < size; c++)
                        for (int r = 0; r < size; r++)
                            sum += arr[x + c, y + r] * mask[c, r];

                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    gaussianArr[x + xPad, y + yPad] = (int)sum;
                }

            gaussianArr = padding(gaussianArr, width, height, xPad, yPad);

            return gaussianArr;

        }

        public int[,] sobel(int[,] arr, int width, int height)
        {

            double[,] sobel_X = {{-1.0, 0.0, 1.0},
                                 {-2.0, 0.0, 2.0},
                                 {-1.0, 0.0, 1.0}};


            double[,] sobel_Y = {{-1.0, -2.0, -1.0},
                                 {0.0, 0.0, 0.0},
                                 {1.0, 2.0, 1.0}};


            int[,] sobelArr = new int[width, height];
            int size = 3;
            int xPad = size / 2;
            int yPad = size / 2;
            double sum, sum1, sum2;

            for (int x = 0; x < width - 2 * xPad; x++)
                for (int y = 0; y < height - 2 * yPad; y++)
                {
                    sum1 = sum2 = 0.0;
                    for (int c = 0; c < size; c++)
                        for (int r = 0; r < size; r++)
                        {
                            sum1 += arr[x + c, y + r] * sobel_X[c, r];
                            sum2 += arr[x + c, y + r] * sobel_Y[c, r];
                        }
                    sum = Math.Abs(sum1) + Math.Abs(sum2);
                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    sobelArr[x + xPad, y + yPad] = (int)sum;
                }



            sobelArr = padding(sobelArr, width, height, xPad, yPad);

            return sobelArr;
        }


        public int[,] mask(int[,] Array)
        {
           

            double[,] sobel_X = {{-1.0, 0.0, 1.0},
                                 {-2.0, 0.0, 2.0},
                                 {-1.0, 0.0, 1.0}};




            double[,] sobel_Y = {{-1.0, -2.0, -1.0},
                                 {0.0, 0.0, 0.0},
                                 {1.0, 2.0, 1.0}};

           
            int[,] Result = convolve2(Array, height, width, sobel_X, sobel_Y, 3, 3);
           

            return Result;
        }

        int[,] convolve2(int[,] G, int Width, int Height, double[,] M1, double[,] M2, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y, r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            double sum, sum1, sum2;

            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    sum1 = sum2 = 0.0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++)
                        {
                            sum1 += G[y + r, x + c] * M1[r, c];
                            sum2 += G[y + r, x + c] * M2[r, c];
                        }
                    sum = Math.Abs(sum1) + Math.Abs(sum2);
                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    R[y + yPad, x + xPad] = (int)sum;
                }

            for (y = 0; y < yPad; y++)
            {
                for (x = xPad; x < Width - xPad; x++)
                {
                    R[y, x] = R[yPad, x];
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x];
                }
            }

            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[y, Width - 1 - x] = R[y, Width - 1 - xPad];
                }
            }

            return R;
        }

        public int[,] Stretching(Bitmap gBitmap)
        {
            Bitmap bitmap = new Bitmap(gBitmap, new Size(gBitmap.Width, gBitmap.Height));
            int width = gBitmap.Width;
            int height = gBitmap.Height;

            Bitmap output = new Bitmap(width, height);
            int X_mid, X_min = int.MaxValue, X_max = int.MinValue;
            int D_min, D_max;
            int a;
            int I_max, I_min, I_mid;
            int sum1 = 0, sum2 = 0;
            int r = 0;
            double usum1 = 0, usum2 = 0, L, M, H, M1, M2;
            Color color;
            int count = 0;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (bitmap.GetPixel(x, y).R != 0)
                    {
                        count++;
                        color = bitmap.GetPixel(x, y);
                        r += color.R;
                        if (X_min > color.R)
                            X_min = color.R;

                        if (X_max < color.R)
                            X_max = color.R;
                    }

            X_mid = r / count; //전체 평균 명암
            D_max = Math.Abs(X_max - X_mid);
            D_min = X_mid - X_min;

            if (X_mid > 128) a = 255 - X_mid;
            else if (X_mid <= D_min) a = D_min;
            else if (X_mid >= D_max) a = D_max;
            else a = X_mid;

            I_max = X_mid + a;
            I_min = X_mid - a;
            I_mid = (I_max + I_min) / 2;
            Console.WriteLine("mid : " + I_mid + "min : " + I_min + "max" + I_max);

            L = I_min;
            M = I_mid;
            H = I_max;
            M1 = (L + M) / 2;
            M2 = (M + H) / 2;


            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int br = gBitmap.GetPixel(x, y).R;

                    if (br != 0)
                    {
                        double ul = 0, um = 0, uy = 0, u1 = 0, u2 = 0;

                        if (br <= M1) //하한
                        {

                            sum1 += br;
                            if (L >= br)
                                ul = 1.0;
                            else if (L < br && M1 > br)
                                ul = (br - M1) / (M1 - L) + 1;
                            else
                                ul = 0;


                            if (M1 == br)
                                um = 1;
                            else if (L < br && M1 > br)
                                um = -1 * (br - L) / (M1 - L) + 1;
                            else
                                um = 0;

                            // min-max 중심법(추론법)
                            if (um >= 0.5 && ul >= 0.5)
                                uy = Min(um, ul);
                            else if (um >= 0.5 && ul <= 0.5)
                                uy = Min(um, ul);
                            else if (um <= 0.5 && ul >= 0.5)
                                uy = Min(um, ul);
                            else if (um <= 0.5 && ul <= 0.5)
                                uy = Min(um, ul);


                            u1 = Max(um, ul, uy);
                        }


                        if (br >= M2) //상한
                        {
                            sum2 += br;
                            if (M2 == br)
                                ul = 1.0;
                            else if (M2 < br && H > br)
                                ul = (br - H) / (H - M2) + 1;
                            else
                                ul = 0;


                            if (H <= br)
                                um = 1;
                            else if (H > br && M2 < br)
                                um = -1 * (br - M2) / (H - M2) + 1;
                            else
                                um = 0;

                            // min-max 중심법(추론법)
                            if (um >= 0.5 && ul >= 0.5)
                                uy = Min(um, ul);
                            else if (um >= 0.5 && ul <= 0.5)
                                uy = Min(um, ul);
                            else if (um <= 0.5 && ul >= 0.5)
                                uy = Min(um, ul);
                            else if (um <= 0.5 && ul <= 0.5)
                                uy = Min(um, ul);

                            u2 = Max(um, ul, uy);
                        }
                        // 무게중심법을 하기위함
                        usum2 += u2;
                        usum1 += u1;

                    }
                }


            int alpha = (int)((sum1 - usum1) / usum1); //하한 무게중심법
            int beta = (int)((sum2 - usum2) / usum2); //상한 무게중심법


            if (beta > 255) beta = 255;
            if (alpha < 0) alpha = 0;


            int[] LUT = new int[256];


            for (int x = 255; x > beta; x--) LUT[x] = 255;
            for (int x = alpha; x <= beta; x++)
                LUT[x] = (int)((x - alpha) * 255.0 / (beta - alpha));

            int[,] result = new int[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (bitmap.GetPixel(x, y) != Color.Black)
                    {
                        int gray = (int)(0.299 * bitmap.GetPixel(x, y).R + 0.587 * bitmap.GetPixel(x, y).G + 0.114 * bitmap.GetPixel(x, y).B);
                        //output.SetPixel(x, y, Color.FromArgb((byte)LUT[gray], (byte)LUT[gray], (byte)LUT[gray]));
                        result[x, y] = LUT[gray];
                    }


            return result;
        }

        double Max(double a, double b)
        {
            if (a > b)
                return a;
            return b;
        }

        double Min(double a, double b)
        {
            if (a > b)
                return b;
            return a;
        }

        double Max(double a, double b, double c)
        {
            if (a > b && a > c)
                return a;
            if (b > c)
                return b;
            return c;
        }


        public int[,] expanding (int[,] arr, int width, int height)
        {
            int margine = 2;

            for (int x = margine; x < width; x++)
                for (int y = margine; y < height; y++)
                {
                    if (arr[x, y] == 255)
                    {
                        for (int i = 0; i < margine; i++)
                        {
                            arr[x - i, y] = 255;
                            arr[x, y - i] = 255;
                        }

                    }

                }

            for (int x = margine; x < width; x++)
                for (int y = height - margine; y >= 0; y--)
                {
                    if (arr[x, y] == 255)
                    {
                        for (int i = 0; i < margine; i++)
                        {
                            arr[x - i, y] = 255;
                            arr[x, y + i] = 255;
                        }

                    }
                }

            margine = 2;

            for (int y = margine; y < height; y++)
                for (int x = width - margine; x >= 0; x--)
                {
                    if (arr[x, y] == 255)
                    {
                        for (int i = 0; i < margine; i++)
                        {
                            arr[x + i, y] = 255;
                            arr[x, y - i] = 255;
                        }

                    }
                }

            for (int y = height - margine; y >= 0; y--)
                for (int x = width - margine; x >= 0; x--)
                {
                    if (arr[x, y] == 255)
                    {
                        for (int i = 0; i < margine; i++)
                        {
                            arr[x + i, y] = 255;
                            arr[x, y + i] = 255;
                        }

                    }
                }

            return arr;
        }
        public Bitmap erase(Bitmap bitmap, int[,] arr, int width, int height)
        {

            arr = expanding(arr, width, height);


            int[,] aa = new int[width, height];
            int[,] bb = new int[width, height];
            int[,] cc = new int[width, height];
            int[,] dd = new int[width, height];

            int[,] newstr = new int[width, height];
            int[,] roiarr = new int[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    aa[x, y] = 0;
                    bb[x, y] = 0;
                    cc[x, y] = 0;
                    dd[x, y] = 0;
                    newstr[x, y] = 0;
                    //roiarr[x, y] = cutArr3[x, y];


                }

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (arr[x, y] == 0)
                        aa[x, y] = 255;

                    else
                    {

                        goto a;
                    }

                }
            a: continue;
            }

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    if (arr[x, y] == 0)
                        bb[x, y] = 255;
                    else
                        goto b;
                }
            b: continue;
            }

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    if (arr[x, y] == 0)
                        cc[x, y] = 255;

                    else
                    {
                        goto c;
                    }

                }
            c: continue;
            }

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    if (arr[x, y] == 0)
                        dd[x, y] = 255;
                    else
                    {

                        goto d;
                    }
                }
            d: continue;
            }

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (cc[x, y] == 255 || aa[x, y] == 255 || bb[x, y] == 255 || dd[x, y] == 255)
                        newstr[x, y] = 255;
                }



            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (newstr[x, y] == 255)
                        bitmap.SetPixel(x, y, Color.Black);

                }

         


            return bitmap;
        }

        public Bitmap setImage(Bitmap bitmap, int[,] arr, int width, int height)
        {
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (arr[x, y] == 255)
                        bitmap.SetPixel(x, y, Color.Black);

                }

            return bitmap;
        }
    }
}
