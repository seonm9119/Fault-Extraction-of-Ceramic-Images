using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class FuzzyStretching
    {
        public Bitmap Stretching(Bitmap gBitmap)
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


            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (bitmap.GetPixel(x, y) != Color.Black)
                    {
                        int gray = (int)(0.299 * bitmap.GetPixel(x, y).R + 0.587 * bitmap.GetPixel(x, y).G + 0.114 * bitmap.GetPixel(x, y).B);
                        output.SetPixel(x, y, Color.FromArgb((byte)LUT[gray], (byte)LUT[gray], (byte)LUT[gray]));
                    }

            gBitmap = new Bitmap(output);


            return gBitmap;
        }

        public double Max(double a, double b)
        {
            if (a > b)
                return a;
            return b;
        }

        public double Min(double a, double b)
        {
            if (a > b)
                return b;
            return a;
        }

        public double Max(double a, double b, double c)
        {
            if (a > b && a > c)
                return a;
            if (b > c)
                return b;
            return c;
        }
    }
}
