using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class Mask
    {
        public int[,] mask(int[,] Array, Bitmap bitmap)
        {
            

            double[,] sobel_X = {{-1.0, 0.0, 1.0},
                                 {-2.0, 0.0, 2.0},
                                 {-1.0, 0.0, 1.0}};



            double[,] sobel_Y = {{-1.0, -2.0, -1.0},
                                 {0.0, 0.0, 0.0},
                                 {1.0, 2.0, 1.0}};

           
            int[,] Result = convolve2(Array, bitmap.Width, bitmap.Height, sobel_X, sobel_Y, 3, 3);


            return Result;
        }

        int[,] convolve(int[,] G, int Width, int Height, double[,] M, int maskCol, int maskRow, int biasValue)
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
                        for (c = 0; c < maskCol; c++)
                            sum += G[y + r, x + c] * M[r, c];
                    sum += biasValue;
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
                for (y = yPad; y < Height; y++)
                {
                    R[y, x] = R[y, xPad];
                    R[y, Width - 1 - x] = R[y, Width - 1 - xPad];
                }
            }

            return R;
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
    }
}
