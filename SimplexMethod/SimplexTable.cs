using LinearProgrammingTask;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplexMethod
{
   internal class SimplexTable
   {
      private readonly Matrix<double> _A;
      private readonly Vector<double> _B;
      private readonly Vector<double> _F;
      private double _FVal;

      private readonly Vector<double> _addition;
      private double _additionVal;

      private readonly LPT.Mode _mode;
      private readonly int _vCount;
      private readonly int _rCount;
      private readonly int[] _basisVarsIndex;

      public SimplexTable(LPT task, int[] basisVars)
         : this(task.A, task.B, task.F, 0, basisVars, task.Mode_, task.VCount, task.RCount)
      {
         Helpers.Assert(Array.Exists(basisVars, v => v < 0 || v >= task.VCount) == false);
      }

      public SimplexTable(LPT task, SimplexTable completedAB)
         : this(completedAB._A.SubMatrix(0, task.RCount, 0, task.VCount),
                completedAB._B,
                completedAB._addition.SubVector(0, task.VCount),
                completedAB._additionVal,
                completedAB._basisVarsIndex,
                task.Mode_,
                task.VCount,
                task.RCount)
      {
         Helpers.Assert(completedAB.isOptimal());
         Helpers.Assert(completedAB._addition != null);
         Helpers.Assert(completedAB._mode == LPT.Mode.MinimizeTargetFunc);
         Helpers.Assert(Helpers.isZero(completedAB._FVal));
      }

      public SimplexTable(Matrix<double> A, Vector<double> B, Vector<double> F, double FVal,
         int[] basisVars, LPT.Mode mode, int vCount, int rCount, Vector<double> addition = null, double additionVal = 0)
      {
         _A = Matrix<double>.Build.DenseOfMatrix(A);
         _B = Vector<double>.Build.DenseOfVector(B);

         _F = Vector<double>.Build.DenseOfVector(F);
         _FVal = FVal;

         _addition = addition;
         _additionVal = additionVal;

         _mode = mode;
         _vCount = vCount;
         _rCount = rCount;
         _basisVarsIndex = basisVars;
      }

      public void ReleaseDegenerateRestrictions(LPT task)
      {
         for (int i = 0; i < _rCount; ++i)
         {
            int basisVar = _basisVarsIndex[i];
            Helpers.Assert(basisVar >= 0 && basisVar < _vCount);

            if (basisVar >= task.VCount)
            {
               // detected degenerate restriction
               Helpers.Assert(_A.Row(i).Find(v => !Helpers.isZero(v)).Item1 >= task.VCount);
               Helpers.Assert(Helpers.isZero(_B.At(i)));

               _basisVarsIndex[i] = -1;
            }
         }
      }

      public Simplex.Result Iterate()
      {
         int permRow, permColumn;
         Simplex.Result r = SelectPermisive(out permRow, out permColumn);
         if (r != Simplex.Result.Success)
            return r;

         Helpers.Assert(_basisVarsIndex[permRow] != permColumn);
         _basisVarsIndex[permRow] = permColumn;

         double permElem = _A.At(permRow, permColumn);
         Helpers.Assert(permElem > 0);

         ProcessAddition   (permRow, permColumn, permElem);
         ProcessF          (permRow, permColumn, permElem);

         ProcessB          (permRow, permColumn, permElem);
         ProcessA          (permRow, permColumn, permElem);

         return Simplex.Result.Success;
      }

      private Simplex.Result SelectPermisive(out int permRow, out int permColumn)
      {
         permRow = -1;
         permColumn = -1;
         int[] candidate = null;

         switch (_mode)
         {
            case LPT.Mode.MaximizeTargetFunc:
               Tuple<int, double>[] T = _F.
                  ToArray().
                  Select((v, i) => Tuple.Create(i, v)).
                  Where(t => t.Item2 > 0 && !Helpers.isZero(t.Item2)).
                  ToArray();
               Array.Sort(T, (x, y) => -x.Item2.CompareTo(y.Item2));
               candidate = T.Select(t => t.Item1).ToArray();
               break;
            case LPT.Mode.MinimizeTargetFunc:
               Tuple<int, double>[] T_ = _F.
                  ToArray().
                  Select((v, i) => Tuple.Create(i, v)).
                  Where(t => t.Item2 < 0 && !Helpers.isZero(t.Item2)).
                  ToArray();
               Array.Sort(T_, (x, y) => x.Item2.CompareTo(y.Item2));
               candidate = T_.Select(t => t.Item1).ToArray();
               break;
         }
         if (candidate.Length == 0)
            return Simplex.Result.Success;

         foreach (var permC in candidate)
         {
            if (TrySelectPermisive(permC, ref permRow))
            {
               permColumn = permC;
               break;
            }
            // may be not!
            return Simplex.Result.TargetFunctionNotLimited;
         }
         if (permRow == -1)
            return Simplex.Result.TargetFunctionNotLimited;
         return Simplex.Result.Success;
      }

      private bool TrySelectPermisive(int permColumn, ref int permRow)
      {
         Vector<double> column = _A.Column(permColumn);
         Vector<double> devision = Vector<double>.Build.Dense(_rCount);

         for (int i = 0; i < _rCount; ++i)
         {
            double d = column.At(i);
            if (d < 0 || Helpers.isZero(d))
               devision[i] = Double.MaxValue;
            else
               devision[i] = _B.At(i) / d;
         }

         if (devision.Minimum() == Double.MaxValue)
            return false;

         permRow = devision.MinimumIndex();
         return true;
      }

      public bool isOptimal()
      {
         switch (_mode)
         {
            case LPT.Mode.MaximizeTargetFunc:
               double max = _F.Maximum();
               return max <= 0 || Helpers.isZero(max);
            case LPT.Mode.MinimizeTargetFunc:
               double min = _F.Minimum();
               return min >= 0 || Helpers.isZero(min);
            default:
               return false;
         }
      }

      public LPTSolution ExtractSolution()
      {
         Helpers.Assert(isOptimal());

         LPTSolution s = new LPTSolution(_vCount);
         for (int i = 0; i < _rCount; ++i)
         {
            int var = _basisVarsIndex[i];
            if (var >= 0 && var < _vCount)
            {
               Helpers.Assert(_basisVarsIndex.Count(v => v == var) == 1);
               s.vVal.At(var, _B[i]);
            } else
               Helpers.Assert(var == -1);
         }
         s.fVal = -_FVal;

         return s;
      }

      private void ProcessA(int permRow, int permColumn, double permElem)
      {
         for (int j = 0; j < _rCount; ++j)
         {
            for (int i = 0; i < _vCount; ++i)
            {
               if (j == permRow || i == permColumn)
                  continue;

               double d = _A.At(j, i) - _A.At(permRow, i) * _A.At(j, permColumn) / permElem;
               _A.At(j, i, d);
            }
         }

         for (int i = 0; i < _vCount; ++i)
         {
            double d = _A.At(permRow, i) / permElem;
            _A.At(permRow, i, d);
         }
         for (int i = 0; i < _rCount; ++i)
         {
            if (i == permRow)
               _A.At(i, permColumn, 1);
            else
               _A.At(i, permColumn, 0);
         }
      }

      private void ProcessB(int permRow, int permColumn, double permElem)
      {
         for (int i = 0; i < _rCount; ++i)
         {
            if (i == permRow)
               continue;

            double d = _B.At(i) - _B.At(permRow) * _A.At(i, permColumn) / permElem;
            _B.At(i, d);
         }
         _B.At(permRow, _B.At(permRow) / permElem);
      }

      private void ProcessAddition(int permRow, int permColumn, double permElem)
      {
         if (_addition == null)
            return;

         _additionVal -= _addition.At(permColumn) * _B.At(permRow) / permElem;

         for (int i = 0; i < _vCount; ++i)
         {
            if (i == permColumn)
               continue;

            double d = _addition.At(i) - _A.At(permRow, i) * _addition.At(permColumn) / permElem;
            _addition.At(i, d);
         }
         _addition.At(permColumn, 0);
      }

      private void ProcessF(int permRow, int permColumn, double permElem)
      {
         _FVal -= _F.At(permColumn) * _B.At(permRow) / permElem;

         for (int i = 0; i < _vCount; ++i)
         {
            if (i == permColumn)
               continue;

            double d = _F.At(i) - _A.At(permRow, i) * _F.At(permColumn) / permElem;
            _F.At(i, d);
         }
         _F.At(permColumn, 0);
      }

      public double FValue
      {
         get { return _FVal; }
      }
   }
}
