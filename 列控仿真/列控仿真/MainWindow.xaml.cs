using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Drawing;

namespace 列控仿真
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer TM = new DispatcherTimer();
        int selectedtrain=0;
        Polyline line = new Polyline();
        int [] stop = new int[1];
        double dis_dif= 0;
        double tk_c = 9.7;//空走时间
        double tk_j = 3.5;
        double b = 0.07295;                                                //
        double c = 0.00112;                                               //
        double v_s = 1;
        double v_li = 300/3.6;
        double timeshow = 0;
        double[] safe_dis_c = new double[1];
        double[] safe_dis_j = new double[1];
        double[] v = new double[1];                              //速度（m/s）
        double[] a = new double[1];                               //加速度（m/s^2）
        int train_no=0;                                         //列车数量
        int [] section_no = new int[1];                         //所在闭塞分区
        double[] distance = new double[1];                        //与起点的距离（0.1*M）
        double[] f = new double[1];                               //牵引力（N）
        double[] w = new double[1];                               //总阻力（N）
        double b_j = 600447.6;                                  //紧急制动力（N）
        double b_c = 333582;                                    //常规制动力（N）
        double m = 419.6;                                       //列车重量（吨）
        double[] w0 = new double[1];                              //基本阻力（N/t）
        RectangleGeometry[] train = new RectangleGeometry[1];   //矩形列车类数组
        int numOfSection = 10;                                  //闭塞分区数
        int?[] sec_occ = new int?[1];                           //分区占用情况
        RectangleGeometry[] light = new RectangleGeometry[1];  //信号机数组
        Path[] pl = new Path[1];
        Path[] pt = new Path[1];
        public MainWindow()
        {
            InitializeComponent();
            
            //制定Timer定时器的中断事件
            TM.Tick += new EventHandler(tm_tick);
            TM.Interval = TimeSpan.FromSeconds(0.1*(1/v_s));
            timelabel.Content="0";
            v_s = v_s_c.Value;
            v_s_l.Content = v_s.ToString()+"X" ;
            reset.IsEnabled = false;
            
            
        }
        void tm_tick(object sender, EventArgs e)
        {
            int i_1;
            int temp;
            double dis_to_z;
            timeshow += 0.1 * (v_s);
            timelabel.Content =timeshow.ToString()+"s";
            
            for (i_1 = 0; i_1 < train_no; i_1++)
            {
                temp = section_no[i_1];
                int numOfSafeSec = 0;
                section_no[i_1] = (int)((float)train[i_1].Bounds.Left / (1450d / (double)numOfSection));                         //计算当前分区号
                if (sec_occ[section_no[i_1]] != null && sec_occ[section_no[i_1]] != i_1)
                {
                    stop[i_1] = 2;
                }
                else
                {
                    if (stop[i_1] == 2)
                    {
                        stop[i_1] = 0;
                    }
                    if (section_no[i_1] != numOfSection)
                    {
                        while (sec_occ[section_no[i_1] + numOfSafeSec + 1] == null)
                        {
                            numOfSafeSec++;
                        }
                    }
                    sec_occ[temp] = null;
                    sec_occ[section_no[i_1]] = i_1;
                }
                dis_to_z = ((numOfSafeSec+1) * 1000) - (distance[i_1] % 1000);
                safe_dis_c[i_1] = ((4.17 * v[i_1] * v[i_1] * 3.6 * 3.6) / ((b_c / (m * 9.81)) + ((8.63 + c * (v[i_1] / 2) * (v[i_1] / 2) * 3.6 * 3.6 + b * (v[i_1] / 2) * 3.6) / 9.81))) + (tk_c * (v[i_1] ));
                safe_dis_j[i_1] = ((4.17 * v[i_1] * v[i_1] * 3.6 * 3.6) / ((b_j / (m * 9.81)) + ((8.63 + c * (v[i_1] / 2) * (v[i_1] / 2) * 3.6 * 3.6 + b * (v[i_1] / 2) * 3.6) / 9.81))) + (tk_j * (v[i_1] ));
                disshow.Content = "实时距离：" + distance[selectedtrain].ToString("0.000") + "M";
                w0[i_1] = 8.63 + c * v[i_1] * v[i_1]*3.6 * 3.6 + b * v[i_1]*3.6;    //计算基本阻力
                w[i_1] = w0[i_1] * m ;                                                                //计算总阻力 
                if (safe_dis_c[i_1] < dis_to_z && stop[i_1] == 0)
                {
                    if (v[i_1] <= v_li)
                    {
                        if (v[i_1] > 30.75)                                                                   //判断是否在恒功率运行阶段并且带入公式计算牵引力大小
                        {
                            f[i_1] = ((7346 * 3.6) / (v[i_1] * 3.6) * 1000);
                        }
                        else
                        {
                            f[i_1] = 238.8 * 1000;
                        }

                        a[i_1] = (f[i_1] - w[i_1]) / (m * 1000);                                                    //根据上述情况计算加速度
                        v[i_1] += a[i_1] * (v_s) * 0.1;                                                                       //根据加速度计算实时速度
                        status.Content = "实时状态：常规加速";
                        acc_l.Content = "实时加速度：" + a[selectedtrain].ToString("0.000") + "m/s^2";
                        speed.Content = "实时速度：" + ((v[selectedtrain] * 3.6)).ToString("0.000") + "KM/H";
                    }
                    else
                    {
                        status.Content = "实时状态：最高速度";
                        acc_l.Content = "实时加速度：0.00 m/s^2";
                        speed.Content = "实时速度：" + ((int)(v[selectedtrain] * 3.6)).ToString("0.000") + "KM/H";
                    }
                    if (v[i_1] > 0)
                    {
                        distance[i_1] += v[i_1] * 0.1 * (v_s);
                        train[i_1].Rect = new Rect(v[i_1] * (v_s) * 0.1 * (1450d / (numOfSection * 1000d)) + train[i_1].Bounds.Left, 145, 30, 20);               //列车位置发生变化
                    }
                }
                else if ((safe_dis_c[i_1] >= dis_to_z || stop[i_1] != 0) && v[i_1] >= 0)
                {
                    if (((safe_dis_j[i_1] >= dis_to_z || stop[i_1] == 2) && safe_dis_j[i_1] >= 10) || stop[i_1] == 4)
                    {
                        a[i_1] = -(b_j + w0[i_1]) / (m * 1000);
                        v[i_1] += a[i_1] * (v_s) * 0.1;
                        status.Content = "实时状态：紧急制动";
                        acc_l.Content = "实时加速度：" + a[selectedtrain].ToString("0.000") + "m/s^2";
                        speed.Content = "实时速度：" + ((v[selectedtrain] * 3.6)).ToString("0.000") + "KM/H";
                    }
                    else
                    {
                        a[i_1] = -(b_c + w0[i_1]) / (m * 1000);
                        v[i_1] += a[i_1] * (v_s) * 0.1;
                        status.Content = "实时状态：常规制动";
                        acc_l.Content = "实时加速度：" + a[selectedtrain].ToString("0.000") + "m/s^2";
                        speed.Content = "实时速度：" + ((v[selectedtrain] * 3.6)).ToString("0.000") + "KM/H";
                    }
                    if (v[i_1] > 0)
                    {
                        distance[i_1] += v[i_1] * 0.1 * (v_s);
                        train[i_1].Rect = new Rect(v[i_1] * (v_s) * 0.1 * (1450d / (numOfSection * 1000d)) + train[i_1].Bounds.Left, 145, 30, 20);               //列车位置发生变化
                    }
                }
                else
                {
                    status.Content = "实时状态：停止";
                    acc_l.Content = "实时加速度：0.00 m/s^2";
                    speed.Content = "实时速度：0.00 KM/H";
                }
                
            }
            try
            {
                drawline(v[0], distance[0]);
            }
            catch
            { 
            }
            int j;
            for (j = numOfSection; j >= 0; j--)                                                                                     //点亮信号灯
            {
                if (sec_occ[j] != null)
                {
                    int k;
                    for (k = 0; k <= j; k++)
                    {
                        switch (k)
                        {
                            case 0:
                                pl[j * 2 - 2 * k + 1].Fill = Brushes.Red;
                                pl[j * 2 - 2 * k + 1].Data = light[j * 2 - 2 * k + 1];
                                pl[j * 2 - 2 * k ].Fill = Brushes.Black;
                                pl[j * 2 - 2 * k ].Data = light[j * 2 - 2 * k];
                                break;
                            case 1:
                                pl[j * 2 - 2 * k + 1].Fill = Brushes.Yellow;
                                pl[j * 2 - 2 * k + 1].Data = light[j * 2 - k * 2 + 1];
                                pl[j * 2 - 2 * k ].Fill = Brushes.Black;
                                pl[j * 2 - 2 * k ].Data = light[j * 2 - k * 2];
                                break;
                            case 2:
                                pl[j * 2 - 2 * k + 1].Fill = Brushes.Yellow;
                                pl[j * 2 - 2 * k + 1].Data = light[j * 2 - k * 2 + 1];
                                pl[j * 2 - 2 * k ].Fill = Brushes.Green;
                                pl[j * 2 - 2 * k ].Data = light[j * 2 - k * 2];
                                break;
                            default:
                                pl[j * 2 - 2 * k + 1].Fill = Brushes.Green;
                                pl[j * 2 - 2 * k + 1].Data = light[j * 2 - k * 2 + 1];
                                pl[j * 2 - 2 * k ].Fill = Brushes.Black;
                                pl[j * 2 - 2 * k ].Data = light[j * 2 - k * 2];
                                break;
                        }
                    }

                }
            }

        }
        void drawtrack()
        {
            //绘制窗体
            Line track = new Line();                                //轨道
            track.Stroke = System.Windows.Media.Brushes.White;
            track.X1 = 10;
            track.Y1 = 165;
            track.X2 = canvas1.Width - 40;
            track.Y2 = 165;
            this.canvas1.Children.Add(track);

            Line[] section_line = new Line[numOfSection * 2 + 1];   //区间间断线
            int i_1;
            for (i_1 = 0; i_1 < numOfSection * 2 + 1; i_1++)
            {
                section_line[i_1] = new Line();
                section_line[i_1].Stroke = System.Windows.Media.Brushes.White;
                section_line[i_1].X1 = 10 + ((canvas1.Width - 50) / (numOfSection * 2)) * i_1;
                section_line[i_1].X2 = 10 + ((canvas1.Width - 50) / (numOfSection * 2)) * i_1;
                section_line[i_1].Y1 = 145 + 10 * (i_1 % 2);
                section_line[i_1].Y2 = 165;
                this.canvas1.Children.Add(section_line[i_1]);
            }
            Array.Resize(ref sec_occ,  0);
            Array.Resize(ref sec_occ,numOfSection + 1);
            sec_occ[numOfSection] = 99;
            //绘制信号机
            Array.Resize(ref light,2*(numOfSection+1));
            Array.Resize(ref pl, 2 * (numOfSection + 1));
            int i_2;
            for (i_2 = 0; i_2 <  numOfSection*2+2; i_2+=2)
            {
                light[i_2] = new RectangleGeometry();
                light[i_2].Rect = new Rect(5+((canvas1.Width - 50) / numOfSection) * (i_2/2), 110, 15, 15);
                light[i_2].RadiusX = 7.5;
                light[i_2].RadiusY = 7.5;
                light[i_2 +1] = new RectangleGeometry();
                light[i_2+1].Rect = new Rect(5+15 + ((canvas1.Width - 50) / numOfSection) * (i_2/2), 110, 15, 15);
                light[i_2+1].RadiusX = 7.5;
                light[i_2+1].RadiusY = 7.5;
                pl[i_2] = new Path();
                pl[i_2+1] = new Path();
                pl[i_2].Fill = Brushes.White;
                pl[i_2].Stroke = Brushes.Black;
                pl[i_2].StrokeThickness = 1;
                pl[i_2].Data = light[i_2];
                this.canvas1.Children.Add(pl[i_2]);
                pl[i_2 + 1].Fill = Brushes.White;
                pl[i_2 + 1].Stroke = Brushes.Black;
                pl[i_2 + 1].StrokeThickness = 1;
                pl[i_2 + 1].Data = light[i_2 + 1];
                this.canvas1.Children.Add(pl[i_2 + 1]);

            }
            Line l_y = new Line();
            Line l_x = new Line();
            Line l_x1 = new Line();
            Line l_x2 = new Line();
            Line l_y1 = new Line();
            Line l_y2 = new Line();
            l_x1.X1 = 1490;
            l_x1.Y1 = 190;
            l_x1.X2 = 1500;
            l_x1.Y2 = 200;
            l_x1.Stroke = Brushes.White;
            l_x1.StrokeThickness = 3;
            l_x2.X1 = 1490;
            l_x2.Y1 = 210;
            l_x2.X2 = 1500;
            l_x2.Y2 = 200;
            l_x2.Stroke = Brushes.White;
            l_x2.StrokeThickness = 3;
            l_y2.X1 = 0;
            l_y2.Y1 = 0;
            l_y2.X2 = 10;
            l_y2.Y2 = 10;
            l_y2.Stroke = Brushes.White;
            l_y2.StrokeThickness = 3;
            l_y1.X1 = 0;
            l_y1.Y1 = 0;
            l_y1.X2 = -10;
            l_y1.Y2 = 10;
            l_y1.Stroke = Brushes.White;
            l_y1.StrokeThickness = 3;
            l_x.X1 = 0;
            l_x.Y1 = 200;
            l_x.X2 = 1500;
            l_x.Y2 = 200;
            l_y.X1 = 0;
            l_y.Y1 = 200;
            l_y.X2 = 0;
            l_y.Y2 = 0;
            l_x.Stroke = Brushes.White;
            l_y.Stroke = Brushes.White;
            l_x.StrokeThickness = 3;
            l_y.StrokeThickness = 3;
            v_d_chart.Children.Add(l_y);
            v_d_chart.Children.Add(l_x);
            v_d_chart.Children.Add(l_x1);
            v_d_chart.Children.Add(l_x2);
            v_d_chart.Children.Add(l_y2);
            v_d_chart.Children.Add(l_y1);

        }
        void add_train() //加入一辆列车
        {
            
            Array.Resize(ref train,train_no+1 );
            train[train_no] = new RectangleGeometry();
            train[train_no].Rect = new Rect(0, 145, 30, 20);
            Array.Resize(ref pt, train_no + 1);
            pt[train_no] = new Path();
            pt[train_no].Fill = Brushes.LemonChiffon;
            pt[train_no].Stroke = Brushes.Red;
            pt[train_no].StrokeThickness = 1;
            pt[train_no].Data = train[train_no];
            pt[train_no].Name = "p" + Convert.ToString(train_no);
            pt[train_no].MouseLeftButtonDown += new MouseButtonEventHandler(leftclicktrain);
            pt[train_no].MouseRightButtonDown += new MouseButtonEventHandler(rightclicktrain);
            this.canvas1.Children.Add(pt[train_no]);
            Array.Resize(ref distance, train_no + 1);
            distance[train_no] = 0;
            Array.Resize(ref v, train_no + 1);
            v[train_no] = Convert.ToDouble(initial_v.Text)/3.6 ;
            Array.Resize(ref section_no, train_no + 1);
            Array.Resize(ref f, train_no + 1);
            f[train_no]=238.8*1000;
            Array.Resize(ref w0, train_no + 1);
            w0[train_no] = 8.63;
            Array.Resize(ref w, train_no + 1);
            w[train_no] = w0[train_no] * m * 0.001f;
            Array.Resize(ref a, train_no + 1);
            a[train_no] = 0;
            Array.Resize(ref section_no, train_no + 1);
            section_no[train_no] = 0;
            Array.Resize(ref safe_dis_c, train_no + 1);
            Array.Resize(ref safe_dis_j, train_no + 1);
            Array.Resize(ref stop, train_no + 1);
            stop[train_no] = 0;
            train_list.Items.Add("列车"+train_no);
            Canvas.SetLeft(line, 0);
            train_no += 1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            add_train();
        }
        private void leftclicktrain(object sender, MouseEventArgs e)
        {
            MouseEventArgs arg = (MouseButtonEventArgs)e;
            Path p = (Path)sender;
            if (stop[Convert.ToInt32(p.Name.Substring(1, 1))] == 0)
            {
                stop[Convert.ToInt32(p.Name.Substring(1, 1))] = 3;
            }
            else
            {
                stop[Convert.ToInt32(p.Name.Substring(1, 1))] = 0;
            }
        }
        private void rightclicktrain(object sender, MouseEventArgs e)
        {
            MouseEventArgs arg = (MouseButtonEventArgs)e;
            Path p = (Path)sender;
            if (stop[Convert.ToInt32(p.Name.Substring(1, 1))] == 0)
            {
                stop[Convert.ToInt32(p.Name.Substring(1, 1))] = 4;
            }
            else
            {
                stop[Convert.ToInt32(p.Name.Substring(1, 1))] = 0; 
            }
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            v_s = v_s_c.Value;
            v_s_l.Content = v_s.ToString() + "X";
        }

        private void train_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedtrain = train_list.SelectedIndex;
        }

        private void draw_track_Click(object sender, RoutedEventArgs e)
        {
            drawtrack();
            TM.Start();
            red_sec.IsEnabled = false;
            add_sec.IsEnabled = false;
            reset.IsEnabled = true;
            draw_track.IsEnabled = false;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            numOfSection++;
            section_setting.Content = "闭塞分区数：" + numOfSection.ToString();//
        }

        private void red_sec_Click(object sender, RoutedEventArgs e)
        {
            numOfSection--;
            section_setting.Content = "闭塞分区数：" + numOfSection.ToString();
        }


        private void reset_Click(object sender, RoutedEventArgs e)
        {
            int i;
            TM.Stop();
            canvas1.Children.Clear();
            red_sec.IsEnabled = true;
            add_sec.IsEnabled = true;
            train_no = 0;
            selectedtrain = 0;
            Array.Resize(ref train, 0);
            Array.Resize(ref pt, 0);
            Array.Resize(ref distance, 0);
            Array.Resize(ref v, 0);
            Array.Resize(ref section_no,  0);
            Array.Resize(ref f, 0);
            Array.Resize(ref w0,  0);
            Array.Resize(ref w,   0);
            Array.Resize(ref a,   0);
            Array.Resize(ref section_no,   0);
            Array.Resize(ref safe_dis_c,   0);
            Array.Resize(ref safe_dis_j,   0);
            Array.Resize(ref stop,   0);
            for (i = 0; i < numOfSection; i++)
            {
                sec_occ[i] = null;
            }
           selectedtrain = 0;
           train_list.Items.Clear();
           draw_track.IsEnabled = true;
           reset.IsEnabled = false;
           v_d_chart.Children.Clear();
           line.Points.Clear();
           
        }
        void drawline(double v_now,double dis_now)
        {
            line.Stroke = Brushes.White;
            line.StrokeThickness = 2;
            line.Points.Add(new Point(10+dis_now * (1450d / (numOfSection * 1000d)), 200 - v_now * 2));
            v_d_chart.Children.Remove(line);
            v_d_chart.Children.Add(line);
            if (distance[0] * (1450d / (numOfSection * 1000d)) >= 1506)
            {
                Canvas.SetLeft(line, Canvas.GetLeft(line) - (dis_now - dis_dif) * (1450d / (numOfSection * 1000d)));
            }
            dis_dif = dis_now;
            }
        }





    
}
