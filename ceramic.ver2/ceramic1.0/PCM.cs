using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class PCM
    {
        Bitmap bitmap;
        private int WIDTH, HEIGHT;
        public int CT;

        private const int CLUSTER = 11;


        //데이터
        private int DATA;
        private double[] data_set;

        //전형성
        private double[,] pretypicity_set;
        private double[,] typicity_set;

        //중심벡터
        private double[] center;

        //클러스트 부피값
        private double[] volume;

        private double m;

        private int[,] temp;

        public PCM(Bitmap bitmap)
        {
            
            this.bitmap = bitmap;
            WIDTH = bitmap.Width;
            HEIGHT = bitmap.Height;

            DATA = WIDTH * HEIGHT;
            data_set = new double[DATA];
            pretypicity_set = new double[CLUSTER, DATA];
            typicity_set = new double[CLUSTER, DATA];
            center = new double[CLUSTER];

            volume = new double[CLUSTER];
            m = 2;

            temp = new int[WIDTH, HEIGHT];


        }


        public Bitmap[] run()
        {
            init();

            for (int i = 0; i < 1; i++)
            {
                compute_center();
                compute_typicity();
                if (center[CLUSTER-1]>255)
                    break;
            }

            return image_processing();


        }

        public void init()
        {
            // step 1. 데이터셋 초기화
            int index = 0;
            for (int x = 0; x < WIDTH; x++)
                for (int y = 0; y < HEIGHT; y++)
                        data_set[index++] = bitmap.GetPixel(x, y).R;



            DATA = index;


            // step 2. 전형성 초기화
            Random random = new Random();
            for (int i = 0; i < CLUSTER; i++)
                for (int j = 0; j < DATA; j++)
                    pretypicity_set[i, j] = random.NextDouble();

        }

        public void compute_typicity()
        {
            double distance = 0;

            for (int i = 0; i < CLUSTER; i++)
            {
                compute_volume(i);
                for (int j = 0; j < DATA; j++)
                {
                    distance = Math.Pow(Math.Abs(data_set[j] - center[i]), 2);
                    typicity_set[i, j] = 1 / (1 + Math.Pow((distance / volume[i]), 1 / (m - 1)));
                }


            }

        }


        public void compute_volume(int i)
        {
            double distance, tsum;

            double sum1 = 0, sum2 = 0;
            for (int j = 0; j < DATA; j++)
            {
                distance = Math.Pow(Math.Abs(data_set[j] - center[i]), 2);
                tsum = Math.Pow(pretypicity_set[i, j], m);
                sum1 += tsum * distance;
                sum2 += tsum;
            }
            volume[i] = sum1 / sum2;



        }
        public void compute_center()
        {

            for (int i = 0; i < CLUSTER; i++)
            {
                double tsum, sum1 = 0, sum2 = 0;

                for (int j = 0; j < DATA; j++)
                {

                    tsum = Math.Pow(pretypicity_set[i, j], m);
                    sum1 += tsum * data_set[j];
                    sum2 += tsum;
                }

                center[i] = Math.Abs(sum1 / sum2);
               

            }

            int T = CLUSTER / 2;
            for (int i = 0; i < CLUSTER; i++)
            {
                if (i < T)
                    center[i] = center[T] - (center[T] / CLUSTER * Math.Abs(i - T));

                else
                    center[i] = center[T] + (center[T] / CLUSTER * Math.Abs(i - T));

                
                Console.WriteLine(center[i]);
            }

       
        }

     

        public Boolean compute_error()
        {
            //double error = 0;
            for (int i = 0; i < CLUSTER; i++)
                for (int j = 0; j < DATA; j++)
                {

                    pretypicity_set[i, j] = typicity_set[i, j];


                }

            return true;
        }

        public Bitmap[] image_processing() // 계산완료 후 이미지처리
        {
            int[] ct = new int[DATA];
            for (int i = 0; i < DATA; i++)
            {
                double max = double.MinValue;
                int count = 0;
                for (int j = 0; j < CLUSTER; j++)
                {

                    if (max < typicity_set[j, i])
                    {
                        max = typicity_set[j, i];
                        count = j;
                    }
                }

                ct[i] = count;
            }



            int p = 0;  // 픽셀.

            Bitmap[] bitmap = new Bitmap[5];
            Bitmap bitcut = new Bitmap(WIDTH, HEIGHT);
            Bitmap bit10 = new Bitmap(WIDTH, HEIGHT);
            Bitmap bit50 = new Bitmap(WIDTH, HEIGHT);
            Bitmap bit70 = new Bitmap(WIDTH, HEIGHT);
            Bitmap bit100 = new Bitmap(WIDTH, HEIGHT);

            int[] tmp = new int[CLUSTER];

            for (int i = 0; i < CLUSTER; i++)
                tmp[i] = 0;

            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                        switch (ct[p++])
                        {
                            case 0:
                                tmp[0]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.Black);
                                bit100.SetPixel(i, j, Color.Aqua);
                                bitcut.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                                

                            break;

                            case 1:
                                tmp[1]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.Black);
                                bit100.SetPixel(i, j, Color.GreenYellow);
                                bitcut.SetPixel(i, j, Color.FromArgb(23, 23, 23));

                            break;

                            case 2:
                                tmp[2]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.Black);
                                bit100.SetPixel(i, j, Color.Gold);
                                bitcut.SetPixel(i, j, Color.FromArgb(46, 46, 46));
                                break;

                            case 3:
                                tmp[3]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.Black);
                                bit100.SetPixel(i, j, Color.OrangeRed);
                                bitcut.SetPixel(i, j, Color.FromArgb(69, 69, 69));
                            break;

                            case 4:
                                tmp[4]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.HotPink);
                                bit100.SetPixel(i, j, Color.HotPink);
                                bitcut.SetPixel(i, j, Color.FromArgb(92, 92, 92));
                            break;

                            case 5:
                                tmp[5]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Black);
                                bit70.SetPixel(i, j, Color.Purple);
                                bit100.SetPixel(i, j, Color.Purple);
                                bitcut.SetPixel(i, j, Color.FromArgb(115, 115, 115));   
                            break;

                            case 6:
                                tmp[6]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Blue);
                                bit70.SetPixel(i, j, Color.Blue);
                                bit100.SetPixel(i, j, Color.Blue);
                                bitcut.SetPixel(i, j, Color.FromArgb(138, 138, 138)); 
                            break;

                            case 7:
                                tmp[7]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Green);
                                bit70.SetPixel(i, j, Color.Green);
                                bit100.SetPixel(i, j, Color.Green);
                                bitcut.SetPixel(i, j, Color.FromArgb(161, 161, 161));                           
                            break;

                            case 8:
                                tmp[8]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Yellow);
                                bit70.SetPixel(i, j, Color.Yellow);
                                bit100.SetPixel(i, j, Color.Yellow);
                                bitcut.SetPixel(i, j, Color.FromArgb(184, 184, 184));
                            break;

                            case 9:
                            tmp[9]++;
                                bit10.SetPixel(i, j, Color.Black);
                                bit50.SetPixel(i, j, Color.Orange);
                                bit70.SetPixel(i, j, Color.Orange);
                                bit100.SetPixel(i, j, Color.Orange);
                                bitcut.SetPixel(i, j, Color.FromArgb(207, 207, 207));
                            break;

                            case 10:
                                tmp[10]++;
                                bit10.SetPixel(i, j, Color.Red);
                                bit50.SetPixel(i, j, Color.Red);
                                bit70.SetPixel(i, j, Color.Red);
                                bit100.SetPixel(i, j, Color.Red);
                                bitcut.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                            break;
                        }
                        
                }

            }

            bitmap[0] = bitcut;
            bitmap[1] = bit10;
            bitmap[2] = bit50;
            bitmap[3] = bit70;
            bitmap[4] = bit100;


            int[] ttmp = new int[CLUSTER];
            for (int i = 0; i < CLUSTER; i++)
                ttmp[i] = tmp[i];
            
            int min_idx, a;

            for (int i = 0; i < CLUSTER - 1; i++)
            {
                min_idx = i;
                for (int j = i + 1; j < CLUSTER; j++)
                {
                    if (tmp[j] < tmp[min_idx])
                    {
                        min_idx = j;
                    }
                }
                a = tmp[min_idx];
                tmp[min_idx] = tmp[i];
                tmp[i] = a;
            }

            int x;
            for (x = 0; x < CLUSTER; x++)
                if (tmp[7] == ttmp[x])
                    break;

            int[] pixel = { 0, 23, 46, 69, 92, 115, 138, 161, 184, 207, 255 };
            CT = pixel[x];

            Console.WriteLine(CT);
            

            return bitmap;
        }

        
    }
}
