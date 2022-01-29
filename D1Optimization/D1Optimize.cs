using Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D1Optimization
{
    public struct Segment
    {
        public Segment(double a, double b)
        {
            this.a = a;
            this.b = b;
        }

        public double Dist()
        {
            return b - a;
        }

        public double Middle()
        {
            return (a + b) / 2;
        }

        public override string ToString()
        {
            return String.Format("[{0:0.###}; {1:0.###}]", a, b);
        }

        public double a;
        public double b;
    }

    public class D1Optimize
    {
        private static double A = ((3.0 - Math.Sqrt(5.0)) / 2);

        public static int Iteration;

        public static bool GoldenSection(D1DFunction f, Segment seg, double eps, out Segment result)
        {
            Iteration = 0;

            double L = seg.a + A * seg.Dist();
            double M = seg.b - A * seg.Dist();
            double yL = f.Value(L);
            double yM = f.Value(M);

            while (true)
            {
                Iteration++;
                if (yL < yM)
                {
                    seg.b = M;
                    if (seg.Dist() <= eps)
                    {
                        result = seg;
                        return true;
                    }

                    M = L;
                    yM = yL;
                    L = seg.a + A * seg.Dist();
                    yL = f.Value(L);
                }
                else
                {
                    seg.a = L;
                    if (seg.Dist() <= eps)
                    {
                        result = seg;
                        return true;
                    }

                    L = M;
                    yL = yM;
                    M = seg.b - A * seg.Dist();
                    yM = f.Value(M);
                }
            }
        }
    }
}
