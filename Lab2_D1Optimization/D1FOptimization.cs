using Functions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab2_D1Optimization
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

        public override string ToString()
        {
            return String.Format("[{0:0.###}; {1:0.###}]", a, b);
        }

        public double a;
        public double b;
    }

    class D1FOptimization
    {
        private D1FOptimization()
        {
        }

        private static double A = ((3.0 - Math.Sqrt(5.0)) / 2);

        public static bool GoldenSection(D1DFunction f, Segment seg, double eps, out Segment result, out int iters)
        {
            double L = seg.a + A * seg.Dist();
            double M = seg.b - A * seg.Dist();
            double yL = f.Value(L);
            double yM = f.Value(M);

            iters = 0;
            while (true)
            {
                iters++;
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

        private static class FibonacciNumber
        {
            private static int[] Init(int N)
            {
                int[] buffer = new int[N + 1];
                buffer[0] = 1;
                buffer[1] = 1;

                for (int i = 2; i <= N; ++i)
                    buffer[i] = buffer[i - 1] + buffer[i - 2];

                return buffer;
            }

            public static int Get(int n)
            {
                Debug.Assert(n <= 40);
                return buffer[n];
            }

            private static readonly int N = 40;
            private static int[] buffer = Init(N);
        }

        public static bool Fibonacci(D1DFunction f, Segment seg, double eps, out Segment result, out int iters)
        {
            int n = 0;
            while (FibonacciNumber.Get(n) <= 2 * seg.Dist() / eps)
                n++;

            double L = seg.a + seg.Dist() * FibonacciNumber.Get(n - 2) / FibonacciNumber.Get(n);
            double M = seg.a + seg.Dist() * FibonacciNumber.Get(n - 1) / FibonacciNumber.Get(n);
            double yL = f.Value(L);
            double yM = f.Value(M);

            iters = 0;
            while (--n > 1)
            {
                iters++;
                if (yL < yM)
                {
                    seg.b = M;

                    M = L;
                    yM = yL;

                    L = seg.a + seg.Dist() * FibonacciNumber.Get(n - 2) / FibonacciNumber.Get(n);
                    yL = f.Value(L);
                }
                else
                {
                    seg.a = L;

                    L = M;
                    yL = yM;

                    M = seg.a + seg.Dist() * FibonacciNumber.Get(n - 1) / FibonacciNumber.Get(n);
                    yM = f.Value(M);
                }
            }

            result = seg;
            return true;
        }
    }
}
