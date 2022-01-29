using Functions;
using Functions2;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab3_D2Optimization
{
    class Program
    {
        class Function : DDerDerFunction
        {
            public Function()
            {
                a = -2;
                b = 16;
                c = 25;
            }

            public override object Value(object param)
            {
                Vector<double> x = param as Vector<double>;
                Debug.Assert(x != null && x.Count == 2);

                return a * x[0] + x[1] + 4 * Math.Sqrt(1 + b * x[0] * x[0] + c * x[1] * x[1]);
            }

            public override object Grad(object param)
            {
                Vector<double> x = param as Vector<double>;
                Debug.Assert(x != null && x.Count == 2);

                double[] v = new double[2];
                v[0] = 4 * b * x[0] / Math.Sqrt(1 + b * x[0] * x[0] + c * x[1] * x[1]) + a;
                v[1] = 4 * c * x[1] / Math.Sqrt(1 + b * x[0] * x[0] + c * x[1] * x[1]) + 1;

                return Vector<double>.Build.DenseOfArray(v);
            }

            public override object Gesse(object param)
            {
                Vector<double> x = param as Vector<double>;
                Debug.Assert(x != null && x.Count == 2);

                double sqr = 1 + b * x[0] * x[0] + c * x[1] * x[1];
                double sqrt = Math.Sqrt(sqr);

                double[,] h = {
                    {(4 * b * sqrt - 4 * b * b * x[0] * x[0] / sqrt) / sqr,     -4 * b * c * x[0] * x[1] / (sqr * sqrt)},
                    {-4 * b * c * x[0] * x[1] / (sqr * sqrt),                   (4 * c * sqrt -  4 * c * c * x[1] * x[1] / sqrt) / sqr}
                };

                return Matrix<double>.Build.DenseOfArray(h);
            }

            private double a;
            private double b;
            private double c;
        }

        private static readonly Function f = new Function();


        private class FunctionOld : D2DDerFunction
        {
            public override object Value(object[] param)
            {
                Debug.Assert(param.Length == 2);
                double[] x = new double[] {(double)param[0], (double)param[1]};

                return f.Value(Vector<double>.Build.DenseOfArray(x));
            }

            public override object Grad(object[] param)
            {
                Debug.Assert(param.Length == 2);
                double[] x = new double[] { (double)param[0], (double)param[1] };

                return f.Grad(Vector<double>.Build.DenseOfArray(x)).ToArray();
            }
        }

        static void Main(string[] args)
        {
            Control.UseManaged();

            FunctionOld fOld = new FunctionOld();

            double x1, x2;
            int iter = GradMethod.Exec(fOld, 1e-7, out x1, out x2);
            double dxOld = dX(Vector<double>.Build.DenseOfArray(new double[]{x1, x2}));

            Vector<double> r;
            GradMethod.Descent2(f, 2, 1e-7, out r);
            double dx = dX(r);
            int iter2 = GradMethod.Iteration;
            int subIter = GradMethod.SubIteration;
        }

        private static double dX(Vector<double> r)
        {
            Vector<double> solution = Vector<double>.Build.DenseOfArray(new double[] { 0.03153711198875353, -0.01009187584072779 });
            return solution.Subtract(r).L2Norm();
        }
    }
}
