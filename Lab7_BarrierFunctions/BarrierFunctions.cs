using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab7_BarrierFunctions
{
    class BarrierFunctions
    {
        private BarrierFunctions()
        {
        }

        static double g1(Vector<double> x)
        {
            return x[0] * x[0] + x[1] * x[1] + x[2] + x[2] - 8;
        }

        static double g2(Vector<double> x)
        {
            return 2 * x[0] * x[0] + x[1] * x[1] - 6;
        }

        static double f(Vector<double> x)
        {
            return (x[0] - 0.1) * (x[0] - 0.1) + (x[1] - 0.2) * (x[1] - 0.2) + (x[2] + 0.3) * (x[2] + 0.3);
        }

        static double F(Vector<double> x, double k)
        {
            return f(x) - k * (1 / g1(x) + 1 / g2(x));
        }

        static Vector<double> grad_F(Vector<double> x, double k)
        {
            Vector<double> grad = DenseVector.OfArray(new double[3]);
            grad[0] = 2 * (x[0] - 0.1) - k * (-2 * x[0] / g1(x) / g1(x) - 4 * x[0] / g2(x) / g2(x));
            grad[1] = 2 * (x[1] - 0.2) - k * (-2 * x[1] / g1(x) / g1(x) - 2 * x[1] / g2(x) / g2(x));
            grad[2] = 2 * (x[2] + 0.3) - k * (-2 * x[2] / g1(x) / g1(x));
            return grad;
        }

        static double findAlpha(Vector<double> x, double k)
        {
            double alpha = 1;
            double lambda = 0.5;
            double eps = 0.5;
            double n2_f = grad_F(x, k).Norm(2) * grad_F(x, k).Norm(2);

            Vector<double> x1 = x - alpha * grad_F(x, k);
            double f_x = F(x, k);
            double f_x1 = F(x1, k);

            for (; f_x1 - f_x > -eps * alpha * n2_f; alpha *= lambda)
            {
                x1 = x - alpha * grad_F(x, k);
                f_x1 = F(x1, k);
            }
            return alpha;
        }

        static Vector<double> Grad_method(Vector<double> x0, double eps, double K)
        {
            Vector<double> x = x0;
            double alpha = 1;

            int k = 0;

            for (; k < 30 && grad_F(x, K).Norm(2) > eps; k++)
            {
                alpha = findAlpha(x, K);
                x = x - alpha * grad_F(x, K);
            }
            return x;
        }

        public static void Minimize(double eps)
        {
            Vector<double> solution = DenseVector.OfArray(new double[] { 0.1, 0.2, -0.3 });

            Vector<double> x = DenseVector.OfArray(new double[] { 0, 0, 0 });
            double lambda = 0.5;
            double k = 1;
            int i = 0;

            for (; i < 100; i++)
            {
                x = Grad_method(x, eps, k);
                if (-k * (1 / g1(x) + 1 / g2(x)) < eps)
                    break;
                k *= lambda;
            }

            Console.WriteLine("solution: {0}", x.ToString());
            Console.WriteLine("iteration count: {0}", i);
            Console.WriteLine("dx: {0}", (x - solution).L2Norm());
            Console.WriteLine("f(x): {0}", f(x));
        }
    }
}
