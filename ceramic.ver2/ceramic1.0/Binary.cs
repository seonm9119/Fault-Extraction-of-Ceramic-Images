using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class Binary
    {
        int i, j = 0;


        public int[,] bin(int[,] arr, Bitmap bitmap)
        {

            int T = 128;
            int max = 0;
            int min = 255;


            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (arr[i, j] > max) max = arr[i, j];
                    else if (arr[i, j] < min) min = arr[i, j];
                }
            }

            T = (max + min) / 2;

            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (arr[i, j] > T) arr[i, j] = 255;
                    else arr[i, j] = 0;
                }
            }

            return arr;
        }

        public int[,] bin2(int[,] arr, Bitmap bitmap)
        {
            int T = 128;
            int max = 0;
            int min = 255;

            for (i = 0; i < bitmap.Height; i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (arr[i, j] > max) max = arr[i, j];
                    else if (arr[i, j] < min) min = arr[i, j];
                }
            }

            T = (max + min) / 2;



            for (i = bitmap.Height / 5; i < 4 * (bitmap.Height / 5); i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (arr[i, j] > T) arr[i, j] = 0;
                    else arr[i, j] = 255;
                }
            }


            return arr;
        }

        public int[] roi(int[,] arr, Bitmap bitmap)
        {
            int[] roiHeight = new int[2];
            int[,] Arr;
            Arr = bin2(arr, bitmap);

            // ROI 윗영역
            for (int i = bitmap.Height / 5; i < 4 * (bitmap.Height / 5); i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    if (Arr[i, j] == 0)
                    {
                        roiHeight[0] = i;
                        break;
                    }
                }
                if (roiHeight[0] == i) break;
            }

            // ROI 아랫영역
            for (int i = 4 * (bitmap.Height / 5) - 1; i > bitmap.Height / 5; i--)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    if (Arr[i, j] == 0)
                    {
                        roiHeight[1] = i;
                        break;
                    }
                }
                if (roiHeight[1] == i) break;
            }

            //roiHeight[0] -= 3;

            return roiHeight;
        }

        public int[,] bin3(int[,] arr, Bitmap bitmap)
        {
            int T = 128;
            int max = 0;
            int min = 255;
            int sum = 0;

            //for (i = 0; i < bitmap.Height; i++)
            //{
            //    for (j = 0; j < bitmap.Width; j++)
            //    {
            //        if (arr[i, j] > max) max = arr[i, j];
            //        else if (arr[i, j] < min) min = arr[i, j];
            //    }
            //}

            //T = (max + min) / 2;


            T = 150;


            for (i = 8 * (bitmap.Height / 17); i < 2 * (bitmap.Height / 3); i++)
            {
                for (j = 0; j < bitmap.Width; j++)
                {
                    if (arr[i, j] > T) arr[i, j] = 0;
                    else arr[i, j] = 255;
                }
            }


            return arr;
        }


        public int[] roi_8mm(int[,] arr, Bitmap bitmap)
        {
            int[] roiHeight = new int[2];
            int[,] Arr;
            Arr = bin3(arr, bitmap);

            // ROI 윗영역
            for (int i = 8 * (bitmap.Height / 17); i < 2 * (bitmap.Height / 3); i++)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    if (Arr[i, j] == 0)
                    {
                        roiHeight[0] = i;
                        break;
                    }
                }
                if (roiHeight[0] == i) break;
            }

            // ROI 아랫영역
            for (int i = 2 * (bitmap.Height / 3) - 1; i > 8 * (bitmap.Height / 17); i--)
            {
                for (int j = 0; j < bitmap.Width; j++)
                {
                    if (Arr[i, j] == 0)
                    {
                        roiHeight[1] = i;
                        break;
                    }
                }
                if (roiHeight[1] == i) break;
            }

            return roiHeight;
        }

    }
}
