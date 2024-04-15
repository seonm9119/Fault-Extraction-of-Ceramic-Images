using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceramic1._0
{
    class PCM2
    {
        Bitmap Original;
        private int WIDTH, HEIGHT;

        private const int CLUSTER = 11;


        //데이터
        private int DATA;
        private double[] data_set;

        //전형성
        private double[,] pretypicity_set;
        private double[,] typicity_set;

        //중심벡터
        public double[] center;

        //클러스트 부피값
        private double[] volume;

        private double m;

        private int[,] tmp;

        public PCM2(Bitmap Original)
        {
            this.Original = Original;
            WIDTH = Original.Width;
            HEIGHT = Original.Height;

            DATA = WIDTH * HEIGHT;
            data_set = new double[DATA];
            pretypicity_set = new double[CLUSTER, DATA];
            typicity_set = new double[CLUSTER, DATA];
            center = new double[CLUSTER];

            //중심센터에 문제가 있음
            //center[0] = 255;
            //center[1] = 125;
            //center[2] = 0;
            //center[3] = 40;
            //center[4] = 45;
            //center[5] = 50;


            volume = new double[CLUSTER];
            m = 2;

            tmp = new int[WIDTH, HEIGHT];


        }


        public Bitmap PCMprocessing()
        {
            init();
            compute_center();


            //center[0] = center[2] - (center[2] / CLUSTER * 2);
            //center[1] = center[2] - (center[2] / CLUSTER * 1);
            //center[2] = center[2];
            //center[3] = center[2] + (center[2] / CLUSTER * 1);
            //center[4] = center[2] + (center[2] / CLUSTER * 2);

            //center[0] = center[1] - (center[1] / CLUSTER * 1);
            //center[1] = center[1];


            //center[0] = 255;
            //center[1] = 125;
            //center[2] = 60;
            //center[3] = 100;
            //center[4] = 255;
            //center[5] = 50;

            //center[0] = center[3] - (center[3] / CLUSTER * 3);
            //center[1] = center[3] - (center[3] / CLUSTER * 2);
            //center[2] = center[3] - (center[3] / CLUSTER * 1);
            //center[3] = center[3];
            //center[4] = center[3] + (center[3] / CLUSTER * 1);
            //center[5] = center[3] + (center[3] / CLUSTER * 2);
            //center[6] = center[3] + (center[3] / CLUSTER * 3);






            for (int i = 0; i < CLUSTER; i++)
                Console.Write(center[i] + "  ");

            for (int i = 0; i < 5; i++)
            {
                //compute_center2();
                compute_typicity();
                if (!compute_error())
                    break;


                // Console.WriteLine(pretypicity_set[1, 10000] + " " + typicity_set[1, 10000]);
            }

            return processing();

        }

        public void init()
        {
            // step 1. 데이터셋 초기화
            int index = 0;
            for (int x = 0; x < WIDTH; x++)
                for (int y = 0; y < HEIGHT; y++)
                {
                    //if (Original.GetPixel(x, y).R != 0)
                    //    data_set[index++] = Original.GetPixel(x, y).R;
                    //else
                    //{
                    //    tmp[x, y] = 1;
                    //}

              
                        data_set[index++] = Original.GetPixel(x, y).R;
                 
                }

            DATA = index;


            // step 2. 전형성 초기화
            Random random = new Random();
            for (int i = 0; i < CLUSTER; i++)
                for (int j = 0; j < DATA; j++)
                {
                    pretypicity_set[i, j] = random.NextDouble();

                }

            //center[0] = 200;
            //center[1] = 75;
            //center[2] = 125;
            //center[3] = 100;
            //center[4] = 255;
            ////center[5] = 50;



        }

        public void compute_typicity()
        {
            double distance = 0;

            for (int i = 0; i < CLUSTER; i++)
            {
                compute_volume(i);
                for (int j = 0; j < DATA; j++)
                {
                    //m = 255 / data_set[j];
                    //m = data_set[j] / 255;
                    //m = 2;
                    //distance = Math.Abs(data_set[j] - center[i]);
                    distance = Math.Pow(Math.Abs(data_set[j] - center[i]), 2);
                    //Console.WriteLine(i+","+j+": 이전전형성 : " + pretypicity_set[i, j]);
                    typicity_set[i, j] = 1 / (1 + Math.Pow((distance / volume[i]), 1 / (m - 1)));
                    //Console.WriteLine(i + "," + j + ": 현재전형성: " + typicity_set[i, j]);
                    //pretypicity_set[i, j] = typicity_set[i, j];
                }


            }

        }


        public void compute_volume(int i)
        {
            double distance, tsum;

            double sum1 = 0, sum2 = 0;
            for (int j = 0; j < DATA; j++)
            {
                //m = 255 / data_set[j];
                //m = data_set[j] / 255;
                //m = 2;
                //distance = Math.Abs(data_set[j] - center[i]);
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
                Console.WriteLine(i + "클러스트" + center[i]);

            }


        }

        public void compute_center2()
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


                center[i] = Math.Abs(sum1 / sum2 - (255 / CLUSTER * i));
                Console.WriteLine(i + "클러스트" + center[i]);

            }


        }

        //public Boolean compute_error()
        //{
        //    double error = 0;
        //    for(int i=0; i<CLUSTER; i++)
        //        for(int j=0; j<DATA; j++)
        //        {
        //            error += Math.Abs(typicity_set[i, j] - pretypicity_set[i, j]);
        //        }
        //    Console.WriteLine("error : "+error);
        //    Console.WriteLine("error/DATA : "+error / DATA);
        //    if (error / DATA > 0.5)
        //    {
        //        for (int i = 0; i < CLUSTER; i++)
        //            for (int j = 0; j < DATA; j++)
        //            {
        //                pretypicity_set[i, j] = typicity_set[i, j];
        //            }
        //        return false;
        //    }


        //    return true;
        //}

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

        public Bitmap processing() // 계산완료 후 이미지처리
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
                //Console.WriteLine("0" + " " + i + " = " + typicity_set[0, i]);

                ct[i] = count;
            }



            int p = 0;  // 픽셀.
            Bitmap bitmap = new Bitmap(WIDTH, HEIGHT);
            for (int i = 0; i < WIDTH; i++)
            {
                for (int j = 0; j < HEIGHT; j++)
                {
                    if (tmp[i, j] == 0)
                    {
                        switch (ct[p++]) // bitmap 픽셀마다 클러스터 0에 속하면 빨간색 픽셀 1에속하면 주황색픽셀 이런식으로 설정
                        {
                            case 0:
                                bitmap.SetPixel(i, j, Color.Aqua);
                                //bitmap.SetPixel(i, j, Color.A);
                                break;
                            case 1:
                                bitmap.SetPixel(i, j, Color.GreenYellow);
                                ////bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;
                            case 2:
                                bitmap.SetPixel(i, j, Color.Gold);
                                //bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;
                            case 3:
                                bitmap.SetPixel(i, j, Color.OrangeRed);
                                //bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;
                            case 4:
                                bitmap.SetPixel(i, j, Color.HotPink);
                                break;
                            case 5:
                                bitmap.SetPixel(i, j, Color.Purple);
                                break;
                            case 6:
                                bitmap.SetPixel(i, j, Color.Blue);
                                //bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;
                            case 7:
                                bitmap.SetPixel(i, j, Color.Green);
                                //bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;
                            case 8:
                                bitmap.SetPixel(i, j, Color.Yellow);
                                break;
                            case 9:
                                bitmap.SetPixel(i, j, Color.Orange);
                                break;
                            case 10:
                                bitmap.SetPixel(i, j, Color.Red);
                                //bitmap.SetPixel(i, j, oriBitmap.GetPixel(i, j));
                                break;


                        }
                    }
                    else
                        bitmap.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                }

            }

            return bitmap;
        }
    }
}
