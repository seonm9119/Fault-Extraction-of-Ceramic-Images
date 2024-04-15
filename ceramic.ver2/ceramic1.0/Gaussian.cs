using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class Gaussian
    {

        double sigma = 1;

        public int[,] gaussianArr(int[,] Array, Bitmap bitmap)
        {
            int x = (int)Math.Round(-4 * sigma);
            int size = (int)Math.Round(8 * sigma + 1);
            double[] g1 = new double[size];
            double[,] g = new double[size, size];



            for (int i = 0; i < size; i++)
            {
                g1[i] = (1 / (Math.Sqrt(2 * Math.PI) * sigma)) * (Math.Exp(-1 * ((x * x) / (2 * sigma * sigma))));
                x++;
            }



            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    g[j, i] = g1[j] * g1[i];
                }
            }

            int[,] Result = convolve3(Array, bitmap.Width, bitmap.Height, g, size, size);

            return Result;
        }

        int[,] convolve3(int[,] G, int Width, int Height, double[,] G1, int maskCol, int maskRow)
        {
            int[,] R = new int[Height, Width];
            int x, y, r, c;
            int xPad = maskCol / 2;
            int yPad = maskRow / 2;
            double sum;

            for (y = 0; y < Height - 2 * yPad; y++)
                for (x = 0; x < Width - 2 * xPad; x++)
                {
                    sum = 0.0;
                    for (r = 0; r < maskRow; r++)
                        for (c = 0; c < maskCol; c++) sum += G[y + r, x + c] * G1[r, c];

                    if (sum > 255.0) sum = 255.0;
                    if (sum < 0.0) sum = 0.0;
                    R[y + yPad, x + xPad] = (int)sum;
                }

            for (y = 0; y < yPad; y++)
            {
                for (x = xPad; x < Width - xPad; x++)
                {
                    R[y, x] = R[yPad, x];   //  ㅡ ↑
                    R[Height - 1 - y, x] = R[Height - 1 - yPad, x]; // ㅡ ↓
                }
            }

            for (x = 0; x < xPad; x++)
            {
                for (y = 0; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];   // | <-
                    R[y, Width - 1 - x] = R[y, Width - 1 - xPad];   // -> |
                }
            }

            return R;
        }
    }
}
