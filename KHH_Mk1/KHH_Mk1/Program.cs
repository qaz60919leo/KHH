using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace KHH_Mk1
{
    class Program
    {
        static double limit = 10;//門檻值
        static string filePath = "音樂Node.csv";

        struct NODE
        {
            public double x;
            public double y;
            public int KGroup;
            public int LGroup;
            public double Distance;
        }

        struct SEED
        {
            public double x;
            public double y;
        }

        static void Main(string[] args)
        {
            ///////////////////
            //load data node
            NODE[] node = new NODE[DataCounter()];
            DataLoad(node);

            /*
            //data normalization
            dataNormal(node);
             */

            ////////////////////
            //math kMeans
            KMeans(node);

            //////////////////////
            //math level in group
            LinKG(node);


            ////////////////////
            //math level
            ReMathLG(node);
            Level(node);


            /////////////////
            //write to csv
            WriteToCsv(node);

                ////end
                System.Console.WriteLine("\nPress any key to continue.....");
            System.Console.ReadLine();
        }

        ////data number counter
        static int DataCounter()
        {
            int counter = 0;
            StreamReader sr = new StreamReader(filePath);

            while (sr.ReadLine() != null)
            {
                counter++;
            }
            sr.Close();

            System.Console.WriteLine("共有 {0} 筆資料", counter);
            return counter;
        }


        ////data load
        static void DataLoad(NODE[] n)
        {
            StreamReader sr = new StreamReader(filePath);
            string SData;//source data
            string[] CData;//cut data
            int counter = 0;

            while ((SData = sr.ReadLine()) != null)
            {
                CData = SData.Split(',', '\n');
                n[counter].x = Convert.ToDouble(CData[0]);
                n[counter].y = Convert.ToDouble(CData[1]);
                counter++;
            }

            System.Console.WriteLine("資料載入完畢....");
        }


        ////data normalization
        static void dataNormal(NODE[] n)
        {
            double maxX = 0, maxY = 0;

            for (int i = 0; i < n.Length; i++)
            {
                if (n[i].x > maxX)
                {
                    maxX = n[i].x;
                }

                if (n[i].y > maxY)
                {
                    maxY = n[i].y;
                }
            }

            for (int i = 0; i < n.Length; i++)
            {
                n[i].x = n[i].x / maxX * 100;
                n[i].y = n[i].y / maxY * 100;
            }

            System.Console.WriteLine("資料正規完成....");
        }

        ////write to csv
        static void WriteToCsv(NODE[] n)
        {
            FileStream fs = new FileStream("result.csv", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);

            sw.WriteLine("Node,X,Y,Kgroup,LGroup,Distance");
            for (int i = 0; i < n.Length; i++)
            {
                sw.WriteLine("{0},{1},{2},{3},{4},{5}", i, n[i].x, n[i].y, n[i].KGroup, n[i].LGroup, n[i].Distance);
            }

            sw.WriteLine("\n\nLGroup,Counter,AvgDistance,標準差");
            int maxLG = 0;

            for (int i = 0; i < n.Length; i++)
            {
                if (n[i].LGroup > maxLG)
                {
                    maxLG = n[i].LGroup;
                }
            }


            for (int i = 0; i <= maxLG; i++)
            {
                double sumDis = 0;
                double avgDis = 0;
                int nodeCounter = 0;

                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].LGroup == i)
                    {
                        sumDis += n[j].Distance;
                        nodeCounter++;
                    }
                }

                avgDis = sumDis / nodeCounter;

                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].LGroup == i)
                    {
                        sumDis += (Math.Pow(n[j].Distance - avgDis, 2));
                    }
                }
                double SD = Math.Sqrt(sumDis / nodeCounter);

                sw.WriteLine("{0},{1},{2},{3}", i, nodeCounter, avgDis, SD);
            }


                sw.Flush();
            fs.Flush();
            fs.Close();

            System.Console.WriteLine("寫入完畢...");
        }


        ////math distance
        static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }


        ////KMean
        static void KMeans(NODE[] n)
        {
            //craet seed
            SEED[] s = new SEED[(int)(n.Length / 10)];
            for (int i = 0; i < s.Length; i++)
            {
                Random rand = new Random(Guid.NewGuid().GetHashCode());
                int randIdex = rand.Next(0, n.Length - 1);

                s[i].x = n[randIdex].x;
                s[i].y = n[randIdex].y;
            }

            //check seed
            Boolean conti = false;//繼續否?
            int falseSeed = 0;
            do
            {
                conti = false;

                for (int i = 0; i < s.Length - 1; i++)
                {
                    if (s[i].x == -1) continue;

                    for (int j = i + 1; j < s.Length; j++)
                    {
                        if (s[j].x == -1) continue;

                        double dis = Distance(s[i].x, s[i].y, s[j].x, s[j].y);
                        if (dis < limit)
                        {
                            falseSeed++;
                            conti = true;
                            s[i].x = (s[i].x + s[j].x) / 2;
                            s[i].y = (s[i].y + s[j].y) / 2;
                            s[j].x = -1;
                        }
                    }
                }
            }
            while (conti == true);

            if (falseSeed > 0)
            {
                SEED[] tempS = new SEED[s.Length - falseSeed];
                int counter = 0;
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i].x == -1) continue;
                    else
                    {
                        tempS[counter].x = s[i].x;
                        tempS[counter].y = s[i].y;
                        counter++;
                    }
                }

                s = new SEED[tempS.Length];
                for (int i = 0; i < s.Length; i++)
                {
                    s[i].x = tempS[i].x;
                    s[i].y = tempS[i].y;
                }
            }

            //show seed
            System.Console.WriteLine("\nKM 種子:");
            for (int i = 0; i < s.Length; i++)
            {
                System.Console.WriteLine("Seed[{0}]=[{1},{2}]", i, s[i].x, s[i].y);
            }


                //math KM
                do
                {
                    conti = false;
                    SEED[] tempS = new SEED[s.Length];
                    for (int i = 0; i < tempS.Length; i++)
                    {
                        tempS[i].x = s[i].x;
                        tempS[i].y = s[i].y;
                    }

                    //math
                    for (int i = 0; i < n.Length; i++)
                    {
                        double minDis = double.MaxValue;
                        int minS =0;

                        for (int j = 0; j < s.Length; j++)
                        {
                            double dis = Distance(n[i].x, n[i].y, s[j].x, s[j].y);
                            if (dis < minDis)
                            {
                                minDis = dis;
                                minS = j;
                            }
                        }

                        n[i].KGroup = minS;
                        n[i].Distance = minDis;
                    }

                    //remath seed
                    for (int i = 0; i < s.Length; i++)
                    {
                        double sumX = 0, sumY = 0;
                        int nodeCounter = 0;

                        for (int j = 0; j < n.Length; j++)
                        {
                            if (n[j].KGroup == i)
                            {
                                sumX += n[j].x;
                                sumY += n[j].y;
                                nodeCounter++;
                            }
                        }

                        if (nodeCounter == 0) continue;
                        else
                        {
                            s[i].x = sumX / nodeCounter;
                            s[i].y = sumY / nodeCounter;
                        }
                    }

                    //check seed
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (s[i].x != tempS[i].x || s[i].y != tempS[i].y)
                        {
                            conti = true;
                        }
                    }

                }
                while (conti == true);

                System.Console.WriteLine("KMeans 分類結束\n");
        }


        ////Level in KG
        static void LinKG(NODE[] n)
        {
            int maxKG = 0;
            for (int i = 0; i < n.Length; i++)
            {
                if (n[i].KGroup > maxKG)
                {
                    maxKG = n[i].KGroup;
                }
            }

            //math
            for (int i = 0; i <= maxKG; i++)
            {
                //creat seed
                int NodeCounter = 0;
                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].KGroup == i)
                    {
                        NodeCounter++;
                    }
                }
                if (NodeCounter == 0) continue;

                NODE[] tempN = new NODE[NodeCounter];
                int counter = 0;
                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].KGroup == i)
                    {
                        tempN[counter].x = n[j].x;
                        tempN[counter].y = n[j].y;
                        tempN[counter].LGroup = counter;
                        counter++;
                    }
                }

                Level(tempN);

                //reWrite n
                counter = 0;
                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].KGroup == i)
                    {
                        n[j].LGroup = tempN[counter].LGroup;
                        n[j].Distance = tempN[counter].Distance;
                        counter++;
                    }
                }
            }
        }


        ////Level
        static void Level(NODE[] n)
        {
            int maxLG = 0;
            Boolean conti = false;

            do
            {
                //creat seed
                for (int i = 0; i < n.Length; i++)
                {
                    if (n[i].LGroup > maxLG)
                    {
                        maxLG = n[i].LGroup;
                    }
                }
                if ((maxLG + 1) == 1) break;

                SEED[] tempS = new SEED[maxLG + 1];
                for (int i = 0; i < tempS.Length; i++)
                {
                    double sumX = 0, sumY = 0;
                    int nodeCounter = 0;

                    for (int j = 0; j < n.Length; j++)
                    {
                        if (n[j].LGroup == i)
                        {
                            nodeCounter++;
                            sumX += n[j].x;
                            sumY += n[j].y;
                        }
                    }

                    tempS[i].x = sumX / nodeCounter;
                    tempS[i].y = sumY / nodeCounter;

                    //write dis
                    for (int j = 0; j < n.Length; j++)
                    {
                        if (n[j].LGroup == i)
                        {
                            n[j].Distance = Distance(n[j].x, n[j].y, tempS[i].x, tempS[i].y);
                        }
                    }
                }

                //math distance
                int minS1 = 0, minS2 = 0;
                double minDis = double.MaxValue;
                for (int i = 0; i < tempS.Length - 1; i++)
                {
                    for (int j = i + 1; j < tempS.Length; j++)
                    {
                        double tempDis=Distance(tempS[i].x,tempS[i].y,tempS[j].x,tempS[j].y);
                        if (tempDis < minDis)
                        {
                            minDis = tempDis;
                            minS1 = i;
                            minS2 = j;
                        }
                    }
                }

                if (minDis < limit)
                {
                    conti = true;

                    for (int i = 0; i < n.Length; i++)
                    {
                        if (n[i].LGroup == minS2)
                        {
                            n[i].LGroup = minS1;
                        }

                        else if (n[i].LGroup > minS2)
                        {
                            n[i].LGroup--;
                        }
                    }
                }
                else break;

                
            }
            while (conti == true);
        }

        ////remath LG
        static void ReMathLG(NODE[] n)
        {
            int maxKG = 0;
            for (int i = 0; i < n.Length; i++)
            {
                if (n[i].KGroup > maxKG)
                {
                    maxKG = n[i].KGroup;
                }
            }

            int maxLG = 0, tempLG = 0;
            for (int i = 0; i <= maxKG; i++)
            {
                for (int j = 0; j < n.Length; j++)
                {
                    if (n[j].KGroup == i)
                    {
                        n[j].LGroup += maxLG;

                        if (n[j].LGroup > tempLG)
                        {
                            tempLG = n[j].LGroup;
                        }
                    }
                }
                maxLG = tempLG + 1;
            }
        }
    }
}
