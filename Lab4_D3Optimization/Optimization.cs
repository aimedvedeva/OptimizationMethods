using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Functions2;
using MathNet.Numerics.LinearAlgebra;
using Functions;
using System.Diagnostics;
using D1Optimization;

namespace Lab4_D3Optimization
{
    class Optimization
    {
        private Optimization()
        {
        }

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

        public static int Iteration;
        public static int SubIteration;

        public static bool Descent2(DDerDerFunction f, double eps, int iter, out Vector<double> x)
        {
            Iteration = 0;
            SubIteration = 0;
            x = Vector<double>.Build.Dense(3, 0);

            while (true)
            {
                if (Iteration >= iter)
                    return true;
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

        public static bool FR(DDerDerFunction f, int dim, double eps, int iter, out Vector<double> x)
        {
            Iteration = 0;
            SubIteration = 0;

            Vector<double> xPrev = Vector<double>.Build.Dense(dim, 0);
            Vector<double> grad = f.Grad(xPrev);
            Vector<double> S = -grad;
            SubOptimization hf = new SubOptimization(f, xPrev, S);
            double A = hf.Minimize();
            x = xPrev + A * S;

            while (true)
            {
                if (grad.L2Norm() < eps || Iteration >= iter)
                    return true;
                Iteration++;

                Vector<double> gradPrev = f.Grad(xPrev);
                double B;
                if (Iteration % dim != 0)
                    B = grad * (grad - gradPrev) / (gradPrev.L2Norm() * gradPrev.L2Norm());
                else
                    B = 0;

                S = -grad + B * S;

                hf = new SubOptimization(f, x, S);
                A = hf.Minimize();

                xPrev = x;
                x = x + A * S;
                grad = f.Grad(x);
            }
        }

        public static bool SDirection(DDerDerFunction f, Vector<double>[] directDirection, out Vector<double> x)
        {
            Iteration = 0;
            SubIteration = 0;

            x = Vector<double>.Build.Dense(directDirection.Length, 0);
            for (int i = 0; i < directDirection.Length; ++i)
            {
                Vector<double> S = directDirection[i];
                double A = -f.Grad(x) * S / (S * f.Gesse(x) * S);

                x = x.Add(S.Multiply(A));
            }

            return true;
        }
    }
}
