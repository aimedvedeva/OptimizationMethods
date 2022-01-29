using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functions;
using System.Diagnostics;

namespace Lab2_D1Optimization
{
    class Program
    {
        private class SmoothFunction : D1DFunction
        {
            public override object Value(object[] param)
            {
                Debug.Assert(param.Length == 1);

                double x = (double)param[0];
                double v = x * x / 2 + x / 2 + 1;

                var h = OnCalculate;
                if (h != null)
                    h();

                return v;
            }

            public event Action OnCalculate;
        }

        private class NonSmoothFunction : D1DFunction
        {
            public override object Value(object[] param)
            {
                Debug.Assert(param.Length == 1);

                double x = (double)param[0];
                double v;

                if (x <= -1)
                    v = -Math.Cos((x + 1) / 2);
                else if (x >= 1)
                    v = Math.Cos((x - 1) / 2);
                else
                    v = Math.Sin(x * Math.PI / 2);

                var h = OnCalculate;
                if (h != null)
                    h();

                return v;
            }

            public event Action OnCalculate;
        }

        static void Main(string[] args)
        {
            double[] epsArray = new double[3] { 1e-1, 1e-3, 1e-6 };
            Segment[] segArray = new Segment[3] { new Segment(-1.5, -0.25), new Segment(-2, 0), new Segment(-4, 0.5) };
            //Segment[] segArray = new Segment[3] { new Segment(-4, -3 ), new Segment(-1, 1), new Segment(3, 4) };
            
            int callCountSmooth = 0;
            Action callCounterSmooth = () => callCountSmooth++;
            int callCountNonSmooth = 0;
            Action callCounterNonSmooth = () => callCountNonSmooth++;

            SmoothFunction smoothF = new SmoothFunction();
            NonSmoothFunction nonSmoothF = new NonSmoothFunction();

            smoothF.OnCalculate += callCounterSmooth;
            nonSmoothF.OnCalculate += callCounterNonSmooth;

            for (int epsIdx = 0; epsIdx < epsArray.Length; ++epsIdx)
            {
                double eps = epsArray[epsIdx];
                Console.WriteLine("Epsilon {0}:", eps);
                for (int segIdx = 0; segIdx < segArray.Length; ++segIdx)
                {
                    Segment seg = segArray[segIdx];
                    Console.WriteLine("Segment {0}:", seg.ToString());

                    int iters;
                    Segment result;

                    callCountSmooth = callCountNonSmooth = 0;
                    D1FOptimization.GoldenSection(smoothF, seg, eps, out result, out iters);
                    Console.WriteLine("GS,     smooth F: Call {0}, Iteration {1}, {2}, f() = {3:0.###}",
                        callCountSmooth, iters, result.ToString(), smoothF.Value((result.a + result.b) / 2));

                    D1FOptimization.GoldenSection(nonSmoothF, seg, eps, out result, out iters);
                    Console.WriteLine("GS, non smooth F: Call {0}, Iteration {1}, {2}, f() = {3:0.###}",
                        callCountNonSmooth, iters, result.ToString(), nonSmoothF.Value((result.a + result.b) / 2));

                    callCountSmooth = callCountNonSmooth = 0;
                    D1FOptimization.Fibonacci(smoothF, seg, eps, out result, out iters);
                    Console.WriteLine("F,      smooth F: Call {0}, Iteration {1}, {2}, f() = {3:0.###}",
                        callCountSmooth, iters, result.ToString(), smoothF.Value((result.a + result.b) / 2));

                    D1FOptimization.Fibonacci(nonSmoothF, seg, eps, out result, out iters);
                    Console.WriteLine("F,  non smooth F: Call {0}, Iteration {1}, {2}, f() = {3:0.###}",
                        callCountNonSmooth, iters, result.ToString(), nonSmoothF.Value((result.a + result.b) / 2));

                    Console.WriteLine("");
                }

                Console.WriteLine("\n");
            }

            smoothF.OnCalculate -= callCounterSmooth;
            nonSmoothF.OnCalculate -= callCounterNonSmooth;
        }
    }
}
