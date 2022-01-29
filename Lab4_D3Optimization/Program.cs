using Functions2;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab4_D3Optimization
{
    class QFFuntion : DDerDerFunction
    {
        public QFFuntion()
        {
            double[,] H_ = new double[,] {
                { 3,    -2, 0 },
                { -2,  4,   0 },
                { 0,    0,   5 }
            };
            
            double[] b_ = new double[] {
                -2, 4, 7
            };

            H = Matrix<double>.Build.DenseOfArray(H_);
            b = Vector<double>.Build.DenseOfArray(b_);
        }

        public override object Value(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            Matrix<double> xT = x.ToColumnMatrix().Transpose();
            Matrix<double> xTH = xT.Multiply(H);
            Vector<double> xTHx = xTH.Multiply(x);

            Matrix<double> bT = b.ToColumnMatrix().Transpose();
            Vector<double> bTx = bT.Multiply(x);

            return 0.5 * xTHx.At(0) + bTx.At(0);
        }

        public override object Grad(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            Vector<double> v = Vector<double>.Build.Dense(3);

            v.At(0, H.At(0, 0) * x[0] + H.At(0, 1) * x[1] + H.At(0, 2) * x[2] + b[0]);
            v.At(1, H.At(1, 0) * x[0] + H.At(1, 1) * x[1] + H.At(1, 2) * x[2] + b[1]);
            v.At(2, H.At(2, 0) * x[0] + H.At(2, 1) * x[1] + H.At(2, 2) * x[2] + b[2]);
            return v;
        }

        public override object Gesse(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            return H;
        }

        private Matrix<double> H;
        private Vector<double> b;
    }

    class ExpFuntion : DDerDerFunction
    {
        public ExpFuntion()
        {
            a = 0;
            b = -1;
            c = -1.4;
        }

        public override object Value(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            double exp = -0.5 * (
                (x.At(0) - a) * (x.At(0) - a) +
                (x.At(1) - b) * (x.At(1) - b) +
                (x.At(2) - c) * (x.At(2) - c));

            return Math.Exp(exp);
        }

        public override object Grad(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            Vector<double> v = Vector<double>.Build.Dense(3);
            double fV = Value(x);

            v.At(0, fV * (-a + x[0]));
            v.At(1, fV * (-b + x[1]));
            v.At(2, fV * (-c + x[2]));

            return v;
        }

        public override object Gesse(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            double fV = Value(x);
            double[,] H_ = new double[,] {
                { fV * ((-a + x.At(0)) * (-a + x.At(0)) + 1),  fV *  (-b + x.At(1)) * (-a + x.At(0)),       fV *  (-c + x.At(2)) * (-a + x.At(0)) },
                { fV *  (-a + x.At(0)) * (-b + x.At(1)),       fV * ((-b + x.At(1)) * (-b + x.At(1)) + 1),  fV *  (-c + x.At(2)) * (-b + x.At(1)) },
                { fV *  (-a + x.At(0)) * (-c + x.At(2)),       fV *  (-b + x.At(1)) * (-c + x.At(2)),       fV * ((-c + x.At(2)) * (-c + x.At(2)) + 1) }
            };

            return Matrix<double>.Build.DenseOfArray(H_);
        }

        private double a;
        private double b;
        private double c;
    }

    class SumFuntion : DDerDerFunction
    {
        public SumFuntion(DDerDerFunction f1, DDerDerFunction f2)
        {
            _f1 = f1;
            _f2 = f2;
        }

        public override object Value(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            return _f1.Value(x) + _f2.Value(x);
        }

        public override object Grad(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            return _f1.Grad(x) + _f2.Grad(x);
        }

        public override object Gesse(object param)
        {
            Vector<double> x = param as Vector<double>;
            Debug.Assert(x != null && x.Count == 3);

            return _f1.Gesse(x) + _f2.Gesse(x);
        }

        private readonly DDerDerFunction _f1;
        private readonly DDerDerFunction _f2;
    }

    class Program
    {

        static void Main(string[] args)
        {
            Control.UseManaged();

            QFFuntion qf = new QFFuntion();
            ExpFuntion exp = new ExpFuntion();
            SumFuntion sum = new SumFuntion(qf, exp);

            double eps = 1e-12, dx;
            int iterN = 20;
            int iter, subIter;
            Vector<double> r, R;
/*
            Console.WriteLine("epsilon: {0}", eps);
            Console.WriteLine("n = {0}", 3);
            Optimization.Descent2(sum, eps, iterN, out r);
            Console.WriteLine("Descent method iteration: {0}", Optimization.Iteration);

            dx = dX(r);
            iter = Optimization.Iteration;
            subIter = Optimization.SubIteration;

            Optimization.FR(sum, 3, eps, iterN * 3, out R);
            Console.WriteLine("Fletcher-Rivs method iteration: {0}", Optimization.Iteration);
            dx = dX(r);
            iter = Optimization.Iteration;
            subIter = Optimization.SubIteration;
            */
            //SDirection();


            Console.WriteLine("epsilon: {0}", eps);
            Console.WriteLine("n = {0}", 3);

            for (int i = 1; i < iterN; i += 2)
            {
                Optimization.Descent2(sum, eps, i, out r);
                Console.WriteLine("Descent method iteration: {0}", Optimization.Iteration);
                //Console.WriteLine("Descent X: {0}", r.ToString());

                Optimization.FR(sum, 3, eps, i * 3, out R);
                Console.WriteLine("Fletcher-Rivs method iteration: {0}", Optimization.Iteration);
                //Console.WriteLine("Fletcher-Rivs X: {0}", R.ToString());

                Console.WriteLine("Descent       dX: {0}", dX(r));
                Console.WriteLine("Fletcher-Rivs dX: {0}", dX(R));
            }
        }

        private static void SDirection()
        {
            QFFuntion qf = new QFFuntion();

            Vector<double>[] sD1 = new Vector<double>[3], sD2 = new Vector<double>[3];
            sD1[0] = Vector<double>.Build.DenseOfArray(new double[] { 1, 2, 3 });
            sD1[1] = Vector<double>.Build.DenseOfArray(new double[] { -3, 2, -1 });
            sD1[2] = Vector<double>.Build.DenseOfArray(new double[] { 6, 5, -1.6 });

            sD2[0] = Vector<double>.Build.DenseOfArray(new double[] { 1.2, 1, -0.32 });
            sD2[1] = Vector<double>.Build.DenseOfArray(new double[] { -3, 2, -1 });
            sD2[2] = Vector<double>.Build.DenseOfArray(new double[] { 1, 2, 3 });


            Vector<double> Rd;
            Optimization.SDirection(qf, sD2, out Rd);
        }

        private static double dX(Vector<double> r)
        {
            Vector<double> solution = Vector<double>.Build.DenseOfArray(new double[]{0, -1, -1.4});
            return solution.Subtract(r).L2Norm();
        }
    }
}
