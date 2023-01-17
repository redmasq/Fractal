using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractal
{
    public struct Complex
    {
        private static NameValueCollection AppSettings = System.Configuration.ConfigurationManager.AppSettings;
        private static int _RootSolverMode = 0;
        private static int _ChoseRoot = 0;

        static Complex() {
            if (!string.IsNullOrEmpty(AppSettings["rootSolver.mode"]))
            {
                int.TryParse(AppSettings["rootSolver.mode"], out _RootSolverMode);
            }
            if (!string.IsNullOrEmpty(AppSettings["colorScheme.fixAt"]))
            {
                int.TryParse(AppSettings["rootSolver.fixAt"], out _ChoseRoot);
            }
        }
        public readonly double Real;
        public readonly double Imag;

        public Complex(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        public Complex Power(double power)
        {
            return Complex.Power(this, power);
        }

        public override string ToString()
        {
            return this.Real.ToString() + (this.Imag >= 0?" + ":" - ") + Math.Abs(this.Imag).ToString() + "i";
        }

        public override int GetHashCode()
        {
            return this.Real.GetHashCode() ^ this.Imag.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Complex))
            {
                return false;
            }
            return Complex.Equals(this, (Complex)obj);
        }

        public static bool Equals(Complex a, Complex b)
        {
            return a.Real == b.Real && a.Imag == b.Imag;
        }

        public Complex Clone()
        {
            return new Complex(this.Real, this.Imag);
        }

        public double Scalar()
        {
            return Math.Abs(this.Real + this.Imag);
        }

        public static Complex operator+(Complex a, Complex b)
        {
            return new Complex(a.Real + b.Real, a.Imag + b.Imag);
        }

        public static Complex operator -(Complex a)
        {
            return new Complex(-a.Real, -a.Imag);
        }

        public static Complex operator-(Complex a, Complex b)
        {
            return new Complex(a.Real - b.Real, a.Imag - b.Imag);
        }

        public static Complex operator -(Complex a, double b)
        {
            return new Complex(a.Real - b, a.Imag);
        }

        public static Complex operator -(Complex a, decimal b)
        {
            return new Complex(a.Real - (double)b, a.Imag);
        }

        public static Complex operator*(Complex a, Complex b)
        {
            return new Complex((a.Real * b.Real) - (a.Imag * b.Imag), (a.Real * b.Imag) + (a.Imag * b.Real));
        }

        public static Complex operator*(Complex a, double b)
        {
            return new Complex(a.Real * b, a.Imag * b);
        }

        public static Complex operator *(Complex a, decimal b)
        {
            return new Complex(a.Real * (double)b, a.Imag * (double)b);
        }

        public static Complex operator +(Complex a, double b)
        {
            return new Complex(a.Real + b, a.Imag);
        }

        public static Complex operator +(Complex a, decimal b)
        {
            return new Complex(a.Real + (double)b, a.Imag);
        }

        public static Complex Exp(Complex a)
        {
            double scalar = Math.Exp(a.Real);
            return new Complex(scalar * Math.Cos(a.Imag), scalar * Math.Sin(a.Imag));
        }

        public static double Modulus(Complex a)
        {
            return Math.Sqrt(a.Real * a.Real + a.Imag * a.Imag);
        }

        public static double Argument(Complex a)
        {
            return Math.Atan2(a.Imag, a.Real);
        }

        public static Complex Log(Complex a)
        {
            return new Complex(Math.Log(Modulus(a)), Argument(a));
        }

        private static Complex _WholePower(Complex a, int power)
        {
            Complex o = a.Clone();
            if (power == 1)
            {
                return o;
            }
            for (int i = 1; i < power; i++)
            {
                o = o * a;
            }
            return o;
        }

        static Random rnd = new Random();

        public static Complex[] PowerAll(Complex a, double power)
        {
            List<Complex> l = new List<Complex>();
            if (Math.Floor(power)==Math.Ceiling(power) && power > 0)
            {
                
                l.Add(_WholePower(a, (int)power));
            }
            else
            {
                int whole = (int)power;
                double  repcipical =1.0 / Math.Abs(power);
                int sign = Math.Sign(whole);
                whole = Math.Abs(whole);
                Complex temp = a.Clone();

                double m = Math.Exp(Math.Log(Modulus(temp))/repcipical);
                int k = 0;
                double arg = Argument(temp);
                int n = (int)Math.Ceiling(repcipical);
                while(k < n)
                {
                    double t = (arg / repcipical) + (2 * Math.PI * k) / repcipical;
                    Complex result = new Complex(m * Math.Cos(t), m * Math.Sin(t));
                    if (sign < 0)
                    {
                        double denom = result.Real * result.Real + result.Imag * result.Imag;
                        if (denom != 0)
                        {
                            result = new Complex(result.Real / denom, -(result.Imag / denom));
                        }
                    }           
                    if (!l.Contains(result))
                    {
                        l.Add(result);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return l.ToArray();
        }
        

        public static Complex Power(Complex a, double power)
        {
            if (power == 0)
            {
                return new Complex(1, 0);
            }
            if (power == 1)
            {
                return a.Clone();
            }
            if (power == 2)
            {
                return a * a;
            }
            if (Math.Floor(power) == Math.Ceiling(power) && power > 0)
            {

                return _WholePower(a, (int)power);
            }
            Complex[] results = PowerAll(a, power);
            Complex o = new Complex(double.NaN, double.NaN);
            if (results.Length > 0)
            {
                switch(_RootSolverMode)
                {
                    case 1:
                        o = results[results.Length - 1];
                        break;
                    case 2:
                        o = results[rnd.Next(results.Length - 1)];
                        break;
                    case 3:
                        o = results[_ChoseRoot < results.Length ? _ChoseRoot : 0];
                        break;
                    case 4:
                        o = results[_ChoseRoot < results.Length ? _ChoseRoot : results.Length - 1];
                        break;
                    case 5:
                        o = results[_ChoseRoot < results.Length ? _ChoseRoot : rnd.Next(results.Length - 1)];
                        break;
                    case 0:
                    default:
                        o = results[0];
                        break;
                }
            }
            return o;
        }
    }
}
