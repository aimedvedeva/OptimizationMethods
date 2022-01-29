using LinearProgrammingTask;
using MathNet.Numerics.LinearAlgebra;
using SimplexMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6_Plane
{
    class PlaneMethod
    {
        private PlaneMethod()
        {
        }

        private static Vector<double> RestricFunc(Vector<double> x)
        {
            double[] v = new double[2];
            v[0] = x[0] * x[0] + x[1] * x[1] + x[2] * x[2] - 4;
            v[1] = 2 * x[0] * x[0] + x[1] * x[1] - 6;

            return Vector<double>.Build.DenseOfArray(v);
        }

        private static Vector<double> RestricFuncGrad(Vector<double> x, int idxFunc)
        {
            double[] v = new double[3];

            if (idxFunc == 0)
            {
                v[0] = 2 * x[0];
                v[1] = 2 * x[1];
                v[2] = 2 * x[2];
            }
            else
            {
                v[0] = 4 * x[0];
                v[1] = 2 * x[1];
                v[2] = 0;
            }
            return Vector<double>.Build.DenseOfArray(v);
        }

        public static int Iteration = 0;

        public static bool Exec(double eps)
        {
            Iteration = 0;
            Vector<double> solution = Vector<double>.Build.DenseOfArray(new double[] { -0.64888567, 1.6222142, -0.97332853 });

            LPT task = new LPT(3, 6);
            task.AddRestriction(new double[] { 1, 0, 0 }, -3, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 1, 0, 0 },  3, LPT.Sign.LessOrEqual);
            task.AddRestriction(new double[] { 0, 1, 0 }, -2, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 0, 1, 0 },  2, LPT.Sign.LessOrEqual);
            task.AddRestriction(new double[] { 0, 0, 1 }, -4, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 0, 0, 1 },  4, LPT.Sign.LessOrEqual);

            task.SetTargetFunction(new double[] { 2, -5, 3 }, LPT.Mode.MinimizeTargetFunc);
            task.Realize();

            LPTSolution s;
            if (Simplex.Execute(task, out s) != Simplex.Result.Success)
                return false;
            Vector<double> x = s.vVal, x1;

            do
            {
                Iteration++;

                Vector<double> fV = RestricFunc(x);
                int funcIdx = fV.MaximumIndex();
                double funcVal = fV.Maximum();
                if (funcVal < 0)
                    break;

                Vector<double> grad = RestricFuncGrad(x, funcIdx);
                double b = -funcVal + grad * x;

                task = task.NewWithRestriction(grad.ToArray(), b, LPT.Sign.LessOrEqual);

                if (Simplex.Execute(task, out s) != Simplex.Result.Success)
                    return false;
                x1 = x;
                x = s.vVal;
            } while (x.Subtract(x1).L2Norm() > eps);

            Console.WriteLine("exps: {0}", eps);
            Console.WriteLine("solution: {0}", x.ToString());
            Console.WriteLine("dX: {0}", (solution - x).L2Norm());
            Console.WriteLine("solution value: {0}", (task.F * x).ToString());
            Console.WriteLine("iteration count: {0}", Iteration);
            return true;
        }
    }
}


/*
 
             LPT task = new LPT(3, 6);
            task.AddRestriction(new double[] { 1, 0, 0 }, -2, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 1, 0, 0 },  2, LPT.Sign.LessOrEqual);
            task.AddRestriction(new double[] { 0, 1, 0 }, -2, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 0, 1, 0 },  2, LPT.Sign.LessOrEqual);
            task.AddRestriction(new double[] { 0, 0, 1 }, -2, LPT.Sign.MoreOrEqual);
            task.AddRestriction(new double[] { 0, 0, 1 },  2, LPT.Sign.LessOrEqual);

            task.SetTargetFunction(new double[] { 1, 1, 1 }, LPT.Mode.MinimizeTargetFunc);
            task.Realize();
 
 */