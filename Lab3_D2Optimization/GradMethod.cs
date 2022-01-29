using D1Optimization;
using Functions;
using Functions2;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3_D2Optimization
{
    class GradMethod
    {
        private class SubOptimization : D1DFunction
        {
            public SubOptimization(DDerDerFunction f, Vector<double> x, Vector<double> r)
            {
                this.f = f;
                this.x = x;
                this.r = r;
            }

            public double Minimize()
            {
                Segment seg = new Segment(0, 500), result;
                D1Optimize.GoldenSection(this, seg, 0.01, out result);
                SubIteration += D1Optimize.Iteration;

                return result.Middle();
            }

            public override object Value(object[] param)
            {
                Debug.Assert(param.Length == 1);

                double A = (double)param[0];
                double v = f.Value(x + A * r);
                return v;
            }

            private DDerDerFunction f;
            private Vector<double> x;
            private Vector<double> r;
        }

        private GradMethod()
        {
        }

        private static double Norm2(double[] vec)
        {
            return Math.Sqrt(vec[0] * vec[0] + vec[1] * vec[1]);
        }

        private static double Lip(D2DDerFunction f,
            double x1min, double x1max, double x2min, double x2max,
            double y1min, double y1max, double y2min, double y2max)
        {
            double step = 0.1;
            double Rmin = double.MaxValue;

            for (double x1 = x1min; x1 < x1max; x1 += step)
            {
                for (double x2 = x2min; x2 < x2max; x2 += step)
                {
                    for (double y1 = y1min; y1 < y1max; y1 += step)
                    {
                        for (double y2 = y2min; y2 < y2max; y2 += step)
                        {
                            double[] gradX = f.Grad(x1, x2);
                            double[] gradY = f.Grad(y1, y2);
                            gradX[0] -= gradY[0];
                            gradX[1] -= gradY[1];

                            double gradNorm = Norm2(gradX);
                            double varsNorm = Norm2(new double[] { x1 - y1, x2 - y2 });

                            double R = gradNorm / varsNorm;
                            if (R < Rmin)
                                Rmin = R;
                        }
                    }
                }
            }

            return Rmin;
        }

        public static int Exec(D2DDerFunction f, double eps, out double x1, out double x2, double x10 = 0, double x20 = 0)
        {
            x1 = x10;
            x2 = x20;

            double R = Lip(f, -1, 1, -1, 1, -1, 1, -1, 1);
            double A = (1 - eps) / (100);
            int k = 0;

            while (true)
            {
                double[] grad = f.Grad(x1, x2);
                double gradNorm = Norm2(grad);

                if (gradNorm < eps)
                    break;
                k++;
                double Fxk = f.Value(x1, x2);

                x1 = x1 - A * grad[0];
                x2 = x2 - A * grad[1];

                double Fxk_1 = f.Value(x1, x2);

                if (Fxk_1 - Fxk <= -eps * A * gradNorm * gradNorm)
                    A *= 0.983;
            }
            return k;
        }

        public static int Iteration;
        public static int SubIteration;

        public static bool Descent2(DDerDerFunction f, int dim, double eps, out Vector<double> x)
        {
            Iteration = 0;
            SubIteration = 0;
            x = Vector<double>.Build.Dense(dim, 0);

            while (true)
            {
                Iteration++;
                Vector<double> grad = f.Grad(x);
                if (grad.L2Norm() < eps)
                    return true;

                Vector<double> r = -f.Gesse(x).Inverse() * grad;

                SubOptimization hf = new SubOptimization(f, x, r);
                double A = hf.Minimize();

                x = x + A * r;
            }
        }
    }
}
