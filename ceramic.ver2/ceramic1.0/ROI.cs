using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class ROI
    {
        public int width;
        public int height;
        public int[,] roiArr;
        public int start, end;

        

        public int[]run(int[,] arr, int width, int height, int CT)
        {
            this.width = width;
            this.height = height;

            bool white = up_cut(arr, width, height, 255);
            int[] size = down_cut(roiArr, width, height, white, CT);

            return size;

        }

  

        bool up_cut(int[,] arr, int width, int height, int color)
        {
            int[] length = new int[height];
            int[,] binArr = bin(arr, width, height, 255, 0);
            bool white = true;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (binArr[x, y] == color)
                        length[y]++;


            int cut_point;
            start = 0;
            end = height - 1;

            //위에가 하얀경우 --- 위에서 부터 탐색 -- 아래가 짤림
            if (length[start] > length[end])
            {
                white = true;
                for (cut_point = 0; cut_point < height; cut_point++)
                    if (length[cut_point] == 0)
                        break;

                Console.WriteLine("1번" + cut_point);
                this.height = cut_point;

                
            }
            //아래가 하얀경우
            else
            {
                white = false;
                for (cut_point = end; cut_point >= start; cut_point--)
                    if (length[cut_point] == 0)
                        break;

                Console.WriteLine("2번" + cut_point);
                this.height = height - cut_point;
                start = cut_point;

                
            }

            roiArr = new int[width, this.height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < this.height; y++)
                    roiArr[x, y] = arr[x, start + y];

            return white;

        }

        int[] down_cut(int[,] arr, int width, int height, bool white, int CT)
        {

            int tmpheight = height / 3;
            int tmpwidth = width / 3;
            int[] size = new int [3];

            //위에가 하얀경우 : white == true -- 위에서부터 탐색 -- 위에가 짤림
            if (white)
            {
                Console.WriteLine("down 1");
                for (int y = tmpheight; y < this.height; y++)
                    if (arr[tmpwidth, y] == CT)
                    {
                        Console.WriteLine(y);
                        start = y;
                        break;
                    }

                this.height -= start;
                Console.WriteLine("height" + this.height);



            }
            //아래가 하얀경우
            else
            {
                Console.WriteLine("down 2");
                for (int y = this.height-tmpheight; y >=0; y--)
                    if (arr[tmpwidth, y] == CT)
                    {
                        Console.WriteLine(y);
                        end = y;
                        break;
                    }

                this.height = end;
                Console.WriteLine("height" + this.height);

            }


            size[0] = start;
            size[1] = end;
            size[2] = this.height;
            return size;
        }


        int[,] bin(int[,] arr, int width, int height, int color1, int color2)
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





    }
}
