using LinearProgrammingTask;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplexMethod
{
   public static class H
   {
      public static void Substract(this Vector<double> a, Vector<double> b)
      {
         for (int i = 0; i < a.Count; ++i)
         {
            double t = a.At(i) - b.At(i);
            a.At(i, t);
         }
      }
   }

   public class LPTSolution
   {
      public Vector<double> vVal;
      public double fVal;

      public LPTSolution(int vCount)
      {
         vVal = Vector<double>.Build.Dense(vCount, 0);
      }

      public LPTSolution(LPT task, LPTSolution canonical)
         : this(task.VCount)
      {
         int I = 0;
         for (int i = 0; i < task.VCount; ++i)
         {
            double d = canonical.vVal.At(I++);
            vVal.At(i, d);

            if (!task.isVarRestric(i))
            {
               double D = vVal.At(i) - canonical.vVal.At(I++);
               vVal.At(i, D);
            }
         }
         if (task.Mode_ == LPT.Mode.MinimizeTargetFunc)
            fVal = canonical.fVal;
         else
            fVal = -canonical.fVal;
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder(String.Format("f(x) = {0}\n", fVal));
         for (int i = 0; i < vVal.Count; ++i)
         {
            sb.Append(String.Format("x{0} = {1}\n", i + 1, vVal.At(i)));
         }

         return sb.ToString();
      }
   }

    public static class Simplex
    {
       public enum Result {
          Success,
          TargetFunctionNotLimited,
          RestrictionMultiplicityIsEmpty
       }

       public static Result Execute(LPT task, out LPTSolution solution)
       {
          LPT taskCopy = new LPT(task);
          solution = null;
          if (taskCopy.isCanonical())
             return ExecuteCanonical(taskCopy, out solution);

          LPT canonical = taskCopy.ToCanonical();
          LPTSolution cSolution;
          Result r = ExecuteCanonical(canonical, out cSolution);
          if (r != Result.Success)
             return r;

          solution = new LPTSolution(taskCopy, cSolution);
          return Result.Success;
       }

       private static Result ExecuteCanonical(LPT task, out LPTSolution solution)
       {
          solution = null;
          Helpers.MakePositiveFreeMembers(task);
          SimplexTable table;

          int[] basisVars = CaptureBasisVars(task);
          if (CompletedCapture(basisVars))
          {
             table = new SimplexTable(task, basisVars);
          }
          else
          {
             Result r = ExecuteArtificialBasis(task, basisVars, out table);
             if (r != Result.Success)
                return r;
          }

          Result R = Execute(table);
          if (R != Result.Success)
             return R;

          solution = table.ExtractSolution();
          return Result.Success;
       }

       private static Result ExecuteArtificialBasis(LPT task, int[] basisVars, out SimplexTable table)
       {
          table = null;
          int dummyV = Array.FindAll(basisVars, v => v == -1).Length;

          Matrix<double>   A                 = Matrix<double>.Build.Dense(task.RCount, task.VCount + dummyV, 0);
          Vector<double>   B                 = task.B;
          Vector<double>   F                 = Vector<double>.Build.Dense(task.VCount + dummyV, 0);
          double           FVal              = 0;
          int[]            basisVarsIndex    = basisVars;
          LPT.Mode         mode              = LPT.Mode.MinimizeTargetFunc;
          int              vCount            = task.VCount + dummyV;
          int              rCount            = task.RCount;
          Vector<double>   addition          = Vector<double>.Build.Dense(task.VCount + dummyV, 0);
          double           additionVal       = 0;

          A.SetSubMatrix(0, 0, task.A);
          addition.SetSubVector(0, task.VCount, task.F);

          int dummyVI = task.VCount;
          for (int i = 0; i < task.RCount; ++i)
          {
             if (basisVarsIndex[i] != -1)
                continue;

             H.Substract(F, A.Row(i));
             FVal -= B[i];
             A.At(i, dummyVI, 1);

             basisVarsIndex[i] = dummyVI++;
          }

          SimplexTable tableAB = new SimplexTable(A, B, F, FVal,
             basisVarsIndex, mode, vCount, rCount, addition, additionVal);

          Result r = Execute(tableAB);
          if (r != Result.Success)
          {
             Helpers.Assert(r == Result.RestrictionMultiplicityIsEmpty);
             return r;
          }

          if (!Helpers.isZero(tableAB.FValue))
             return Result.RestrictionMultiplicityIsEmpty;

          tableAB.ReleaseDegenerateRestrictions(task);

          table = new SimplexTable(task, tableAB);
          return Result.Success;
       }

       private static Result Execute(SimplexTable table)
       {
          while (!table.isOptimal())
          {
             Result r = table.Iterate();
             if (r != Result.Success)
                return r;
          }
          return Result.Success;
       }

       private static int[] CaptureBasisVars(LPT task)
       {
          int[] basisVars = Enumerable.Repeat(-1, task.RCount).ToArray();

          for (int i = 0; i < task.VCount; ++i)
          {
             double[] a = task.A.Column(i).ToArray();

             if (a.Count(v => Helpers.isZero(v)) == task.RCount - 1 &&
                 a.Count(v => Helpers.isZero(v - 1)) == 1)
             {
                int row = Array.FindIndex(a, v => Helpers.isZero(v - 1));
                if (basisVars[row] == -1)
                   basisVars[row] = i;
             }
          }
          return basisVars;
       }

       private static bool CompletedCapture(int[] basisVars)
       {
          return Array.FindIndex(basisVars, v => v == -1) == -1;
       }
    }
}
