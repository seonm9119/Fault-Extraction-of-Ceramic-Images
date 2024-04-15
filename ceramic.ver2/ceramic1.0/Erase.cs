using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class Erase
    {
        private Bitmap OriginalImage;
        private int WIDTH, HEIGHT;
        private int[,] GrayArray;
        private int[,] ar;
        int CT;

        public Erase(Bitmap OriginalImage, int[,] ar, int CT)
        {
            this.OriginalImage = OriginalImage;
            this.WIDTH = OriginalImage.Width;
            this.HEIGHT = OriginalImage.Height;
            this.ar = ar;
            this.CT = CT;
       

        }

        public int[,] Gray()
        {
            int[,] GrayArray = new int[WIDTH, HEIGHT];
            Color color;
            int Gray;

            for (int x = 0; x < WIDTH; x++)
                for (int y = 0; y < HEIGHT; y++)
                {
                    color = OriginalImage.GetPixel(x, y);
                    Gray = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                    GrayArray[x, y] = Gray;
                    //Console.WriteLine(Gray);

                }

            return GrayArray;
        }

        public Bitmap eraser()
        {

            //PCM pcm = new PCM(OriginalImage);

            int T = 110;
            //Console.WriteLine("T :" +T);
            this.GrayArray = Gray();
            int[,] EraseArray = new int[HEIGHT, WIDTH];


            int[,] rightArray = new int[WIDTH, HEIGHT];
            for (int y = 0; y < HEIGHT - 2; y++)
                for (int x = 0; x < WIDTH - 1; x++)
                {

                    if (GrayArray[x, y] > T)
                    {
                        rightArray[x + 1, y] = 255;
                    }

                    if (rightArray[x, y] == 255)
                    {
                        rightArray[x + 1, y] = 255;
                    }


                }

            int[,] downArray = new int[WIDTH, HEIGHT];
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT - 1; y++)
                {
                    if (GrayArray[x, y] > T)
                    {
                        downArray[x, y + 1] = 255;
                    }

                    if (downArray[x, y] == 255)
                    {
                        downArray[x, y + 1] = 255;
                    }

                }
            }


            int[,] leftArray = new int[WIDTH, HEIGHT];
            for (int y = 0; y < HEIGHT; y++)
                for (int x = WIDTH - 1; x > 2; x--)
                {
                    if (GrayArray[x - 2, y] > T)
                    {
                        leftArray[x - 1, y] = 255;
                    }

                    if (leftArray[x, y] == 255)
                    {
                        leftArray[x - 1, y] = 255;
                    }


                }


            int[,] upArray = new int[WIDTH, HEIGHT];
            for (int x = WIDTH-5; x > 2; x--)
                for (int y = HEIGHT - 5; y > 2; y--)
                {
                    if (GrayArray[x, y - 2] > T)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            upArray[x, y - 1] = 255;
                            upArray[x - 1, y] = 255;
                        }
                    }

                    if (upArray[x, y] == 255)
                    {
                        upArray[x, y - 1] = 255;
                    }

                }


         

            for (int x = 0; x < WIDTH - 1; x++)
                for (int y = 0; y < HEIGHT - 1; y++)
                {
                    //if (rightArray[x, y] == 255 && leftArray[x, y] == 255)
                    //if (downArray[x, y] == 255 && upArray[x, y] == 255)

                    if (downArray[x, y] == 255 && upArray[x, y] == 255)
                    {
                        EraseArray[y, x] = ar[y, x];
                        EraseArray[y, x + 1] = ar[y, x + 1];
                        EraseArray[y + 1, x] = ar[y + 1, x];
                        EraseArray[y, x] = ar[y, x];
                        //EraseArray[y, x] = 255;
                    }
                    else
                    {
                        //EraseArray[y, x] = ar[y, x];
                    }

                }



            Bitmap NewImage = new Bitmap(WIDTH, HEIGHT);


            for (int x = 0; x < HEIGHT; x++)
                for (int y = 0; y < WIDTH; y++)
                    NewImage.SetPixel(y, x,
                        Color.FromArgb(EraseArray[x, y], EraseArray[x, y], EraseArray[x, y]));


            return NewImage;
        }

    }

}
