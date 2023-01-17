using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Fractal
{
    public static class Mandelbrot
    {
        static NameValueCollection AppSettings = System.Configuration.ConfigurationManager.AppSettings;

        static bool pFlatDistance = true;
        static bool fixPBlue = false;
        static bool fixPGreen = false;
        static bool fixPRed = false;
        static bool invertPBlue = false;
        static bool invertPGreen = false;
        static bool invertPRed = false;

        static bool eAddDistance = true;
        static bool fixEBlue = false;
        static bool fixEGreen = false;
        static bool fixERed = false;
        static bool invertEBlue = false;
        static bool invertEGreen = false;
        static bool invertERed = false;

        static byte pBlue = 255;
        static byte pGreen = 255;
        static byte pRed = 255;

        static byte eBlue = 0;
        static byte eGreen = 0;
        static byte eRed = 0;

        static Mandelbrot()
        {
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.pFlatDistance"]))
            {
                bool.TryParse(AppSettings["colorScheme.pFlatDistance"], out pFlatDistance);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixPBlue"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixPBlue"], out fixPBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixPGreen"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixPGreen"], out fixPGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixPRed"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixPRed"], out fixPRed);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertPBlue"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertPBlue"], out invertPBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertPGreen"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertPGreen"], out invertPGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertPRed"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertPRed"], out invertPRed);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.eAddDistance"]))
            {
                bool.TryParse(AppSettings["colorScheme.eAddDistance"], out eAddDistance);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixEBlue"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixEBlue"], out fixEBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixPGreen"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixEGreen"], out fixEGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixERed"]))
            {
                bool.TryParse(AppSettings["colorScheme.fixERed"], out fixERed);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertEBlue"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertEBlue"], out invertEBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertEGreen"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertEGreen"], out invertEGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.invertERed"]))
            {
                bool.TryParse(AppSettings["colorScheme.invertERed"], out invertERed);
            }

            if (!string.IsNullOrEmpty(AppSettings["colorScheme.pBlue"]))
            {
                byte.TryParse(AppSettings["colorScheme.pBlue"], out pBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.pGreen"]))
            {
                byte.TryParse(AppSettings["colorScheme.pGreen"], out pGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.pRed"]))
            {
                byte.TryParse(AppSettings["colorScheme.pRed"], out pRed);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.eBlue"]))
            {
                byte.TryParse(AppSettings["colorScheme.eBlue"], out eBlue);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.eGreen"]))
            {
                byte.TryParse(AppSettings["colorScheme.eGreen"], out eGreen);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.eRed"]))
            {
                byte.TryParse(AppSettings["colorScheme.eRed"], out eRed);
            }
        }

        public struct MandelbrotResult
        {
            public readonly Complex Z;
            public readonly Complex C;
            public readonly double Power;
            public readonly long Iterations;
            public readonly bool Escaped;
            public readonly long MaxIterations;
            public readonly double EscapeLimit;
            public readonly object Data;

            public MandelbrotResult(Complex z, Complex c, double power, long iter, bool escaped, long maxIterations, double escapeLimit, object data)
            {
                Z = z;
                C = c;
                Power = power;
                Iterations = iter;
                Escaped = escaped;
                MaxIterations = maxIterations;
                EscapeLimit = escapeLimit;
                Data = data;
            }
        }

        public static MandelbrotResult CalculateMandelbrot(double power, Complex z, Complex c, long maxIterations, double escapeLimit, object data)
        {
            Complex localZ = z.Clone();
            long iter = 0;
            bool escaped = false;
            for(iter = 0; iter < maxIterations; iter++)
            {
                localZ = localZ.Power(power) + c;
                escaped = escaped || (localZ.Scalar() > escapeLimit);
                if (escaped)
                {
                    break;
                }
            }

            return new MandelbrotResult(z, c, power, iter, escaped, maxIterations, escapeLimit, data);
        }

        private static MandelbrotResult[] _RunRealLine(int y, int width, int height, double power, Complex startZ, Complex min, Complex max, long maxIterations, double escapeLimit)
        {
            MandelbrotResult[] mr = new MandelbrotResult[width];
            for (int x = 0; x < width; x++)
            {
                Complex c = _CalculatePos(x, y, width, height, min, max);
                mr[x] = CalculateMandelbrot(power, startZ, c, maxIterations, escapeLimit, new int[] { x, y });
            }
            return mr;
        }

        private static Complex _CalculatePos(int x, int y, int width, int height, Complex min, Complex max)
        {
            Complex range = max - min;
            double real = range.Real * ((double)x) / ((double)width);
            double imag = range.Imag * ((double)y) / ((double)height);
            Complex intermediate = new Complex(real + min.Real, imag + min.Imag);
            return intermediate;
        }

        private static Task<MandelbrotResult[]> _Run(int y, int width, int height, double power, Complex startZ, Complex minC, Complex maxC, long maxIterations, double escapeLimit)
        {
            Task<MandelbrotResult[]> t = Task.Run<MandelbrotResult[]>(() =>
            {
                return _RunRealLine(y, width, height, power, startZ, minC, maxC, maxIterations, escapeLimit);
            });
            
            return t;
        }

        public static Task<WriteableBitmap> RunMandlebrot(WriteableBitmap wb, int width, int height, double power, Complex startZ, Complex minC, Complex maxC, long maxIterations, double escapeLimit)
        {

            List<Task> jobs = new List<Task>();
            
            // Monte carlo rendering, a personal preference
            Random r = new Random();
            int[] ys = Enumerable.Range(0, height).OrderBy(x => r.Next(16)).ToArray();

            for(int y = 0; y < height; y++)
            {

                
                Task<MandelbrotResult[]> t = _Run(ys[y], width, height, power, startZ, minC, maxC, maxIterations, escapeLimit);
                Task temp = t.ContinueWith((t1) =>
                {
                    Complex range = maxC - minC;
                    double l255 = Math.Log(255);
                    double e255 = Math.Exp(255);
                    double size = Math.Sqrt(Math.Pow(range.Real, 2) + Math.Pow(range.Imag, 2));
                    MandelbrotResult[] mr = t1.Result;
                    byte[] colors = new byte[mr.Length * 3];
                    int[] coords = (int[])mr[0].Data;
                    Int32Rect ir = new Int32Rect(coords[0], coords[1], mr.Length, 1);
                    for (int x = 0; x < mr.Length; x++)
                    {

                        
                        double n = ((double)mr[x].Iterations);
                        double d = ((double)mr[x].MaxIterations);
                        double val = Math.Abs(255.0 * Math.Log(n) / Math.Log(d));

                        byte shadeA = (byte)val;
                        byte shadeB = (byte)Math.Abs(255.0 - 255 * (Math.Sqrt(Math.Pow(mr[x].C.Real, 2) + Math.Pow(mr[x].C.Imag, 2) * 1.9) / size));

                        if (!mr[x].Escaped)
                        {
                            byte useBlue = pBlue;
                            byte useGreen = pGreen;
                            byte useRed = pRed;
                            if (pFlatDistance)
                            {
                                useBlue = fixPBlue ? useBlue : shadeB;
                                useGreen = fixPGreen ? useGreen : shadeB;
                                useRed = fixPRed ? useRed : shadeB;
                            }

                            colors[x * 3 + 0] = invertPBlue? (byte)(255 - useBlue): useBlue;
                            colors[x * 3 + 1] = invertPGreen ? (byte)(255 - useGreen) : useGreen;
                            colors[x * 3 + 2] = invertPRed ? (byte)(255 - useRed) : useRed;
                        }
                        else
                        {

                            byte useBlue = eBlue;
                            byte useGreen = eGreen;
                            byte useRed = eRed;

                            useBlue = fixEBlue ? useBlue : shadeA;
                            useGreen = fixEGreen ? useGreen : shadeA;
                            useRed = fixERed ? useRed : shadeA;

                            if (eAddDistance)
                            {
                                useBlue = (byte)Math.Exp((Math.Log(useBlue) + Math.Log(shadeB)) / 2);
                                useGreen = (byte)Math.Exp((Math.Log(useGreen) + Math.Log(shadeB)) / 2);
                                useRed = (byte)Math.Exp((Math.Log(useRed) + Math.Log(shadeB)) / 2);
                            }

                            colors[x * 3 + 0] = invertEBlue ? (byte)(255 - useBlue) : useBlue;
                            colors[x * 3 + 1] = invertEGreen ? (byte)(255 - useGreen) : useGreen;
                            colors[x * 3 + 2] = invertERed ? (byte)(255 - useRed) : useRed;
                        }

                    }

                    DispatcherOperation waitMe = wb.Dispatcher.InvokeAsync(() => wb.WritePixels(ir, colors, colors.Length, 0));
                    
                    waitMe.Wait();
                });
                jobs.Add(temp);
            }

            Thread.Sleep(100);
            return Task.WhenAll(jobs.ToArray()).ContinueWith<WriteableBitmap>((t2) => { return wb; });
        }
    }
}
