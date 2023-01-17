using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Fractal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NameValueCollection AppSettings = System.Configuration.ConfigurationManager.AppSettings;
        Task mandlebrot = null;
        decimal inc = 0.01M;
        int dir = 0;
        
        decimal startZr = -1.5M;
        decimal startZi = -1.5M;
        decimal endZr = 1.5M;
        decimal endZi = 1.5M;
        decimal sZr = -1.5M;
        decimal sZi = -1.5M;
        int width = 1920;
        int height = 1080;
        decimal power = 2;
        decimal offsetR = 0;
        decimal offsetI = 0;
        decimal minCr = -2.5M;
        decimal minCi = -1.5M;
        decimal maxCr = 2.0M;
        decimal maxCi = 1.5M;
        decimal zoomLevel = 1;
        int iterationBase = 256;
        string outPath = @"C:\";

        public MainWindow()
        {
            InitializeComponent();
            decimal.TryParse(AppSettings["power"], out power);
            decimal.TryParse(AppSettings["startZr"], out startZr);
            decimal.TryParse(AppSettings["startZi"], out startZi);
            decimal.TryParse(AppSettings["endZi"], out endZi);
            decimal.TryParse(AppSettings["endZr"], out endZr);
            decimal.TryParse(AppSettings["inc"], out inc);
            decimal.TryParse(AppSettings["offsetR"], out offsetR);
            decimal.TryParse(AppSettings["offsetI"], out offsetI);
            decimal.TryParse(AppSettings["minCr"], out minCr);
            decimal.TryParse(AppSettings["minCi"], out minCi);
            decimal.TryParse(AppSettings["maxCr"], out maxCr);
            decimal.TryParse(AppSettings["maxCi"], out maxCi);
            decimal.TryParse(AppSettings["zoomLevel"], out zoomLevel);
            int.TryParse(AppSettings["iterationBase"], out iterationBase);
            int.TryParse(AppSettings["width"], out width);
            int.TryParse(AppSettings["height"], out height);
            if (!string.IsNullOrEmpty(AppSettings["outPath"]))
            {
                outPath = AppSettings["outPath"];
            }
            /**/
        }

        private void image1_Initialized(object sender, EventArgs e)
        {


        }

        private void runMandel(int counter)
        {
            int minWorker, minIOC;
            ThreadPool.GetMinThreads(out minWorker, out minIOC);
            minWorker = (int)(Environment.ProcessorCount * .8) - 1;
            minWorker = minWorker < 1? 1 : minWorker;
            ThreadPool.SetMinThreads(minWorker, minIOC);
            bool first = false;


            if (dir == 0)
            {
                first = true;
                dir = 1;
                
                sZr = startZr;
                sZi = startZi;
                
            }
            else if (startZr != endZr &&sZr >= endZr)
            {
                Application.Current.Shutdown();
                return;
            }
            if (!first)
            {
                if (dir == 1 && sZi >= endZi)
                {
                    if (startZr == endZr)
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                    dir = -1;
                    sZi = endZi;
                    sZr += inc;
                }
                else if (dir == -1 && sZi <= startZi)
                {
                    dir = 1;
                    sZi = startZi;
                    sZr += inc;
                }
                else
                {
                    sZi += inc * dir;
                }
            }

            string outFile = System.IO.Path.Combine(outPath, "m" + counter.ToString("0000") + ".png");
            Complex min = new Complex((double)minCr, (double)minCi);
            Complex max = new Complex((double)maxCr, (double)maxCi);
            Complex offset = new Complex((double)offsetR, (double)offsetI);
            Complex curZ = new Complex((double)sZr, (double)sZi);
            long iterations = (long)(1.0M * iterationBase * zoomLevel / 2.0M);

            min = (min * (1.0M / zoomLevel) + offset);
            max = (max * (1.0M / zoomLevel) + offset);
            label1.Content = string.Format("File: {0} CurZ: {1} Power: {8} MinC: {2} MaxC: {3} MinZ: {4} MaxZ: {5} Width: {6} Height: {7} MaxIter: {9}", outFile, curZ, min, max, new Complex((double)startZr, (double)startZi), new Complex((double)endZr, (double)endZi), width, height, power, iterations);
            WriteableBitmap wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
            image1.Source = wb;
            mandlebrot = Mandelbrot.RunMandlebrot(wb, width, height, (double)power, curZ, min, max, iterations, 16).ContinueWith(
            (task) =>
            {
                task.Wait();
                System.Threading.Thread.Sleep(500);
                task.Result.Dispatcher.Invoke(
                    () =>
                    {
                        System.Threading.Thread.Sleep(500);
                        WriteableBitmap bmp = task.Result.Clone();
                        string filename = outFile;
                        using (FileStream stream5 = new FileStream(filename, FileMode.Create))
                        {
                            PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                            encoder5.Frames.Add(BitmapFrame.Create(bmp));
                            encoder5.Save(stream5);
                        }
                        runMandel(counter + 1);
                    });

            });
        }

        private void image1_Loaded(object sender, RoutedEventArgs e)
        {
            runMandel(0);
        }
    }
}
