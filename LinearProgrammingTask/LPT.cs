using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace LinearProgrammingTask
{
   public static class Helpers
   {
      public static void AssertMsg(bool condition, string msg)
      {
#if DEBUG
         if (!condition)
            throw new SystemException("LPT Assertion failed! " + msg);
#endif
      }

      public static void Assert(bool condition)
      {
#if DEBUG
         if (!condition)
            throw new SystemException("Assertion failed!");
#endif
      }

      private readonly static double _epsilon = 0.001;

      public static bool isZero(double v)
      {
         return Math.Abs(v) <= _epsilon;
      }

      public static void SwitchSign(LPT.Sign[] signs, int index)
      {
         switch (signs[index])
         {
            case LPT.Sign.MoreOrEqual:
               signs[index] = LPT.Sign.LessOrEqual;
               break;
            case LPT.Sign.LessOrEqual:
               signs[index] = LPT.Sign.MoreOrEqual;
               break;
         }
      }

      private static Vector<double> VecMultiply(Vector<double> v, double d)
      {
         for (int i = 0; i < v.Count; ++i)
         {
            double t = v.At(i) * d;
            v.At(i, t);
         }

         return v;
      }

      public static void MakePositiveFreeMembers(LPT task)
      {
         for (int i = 0; i < task.RCount; ++i)
         {
            if (task.B.At(i) < 0)
            {
               task.B.At(i, -task.B.At(i));
               task.A.SetRow(i, VecMultiply(task.A.Row(i), -1));
               //task.A.SetRow(i, task.A.Row(i).Multiply(-1));
               SwitchSign(task.Signs, i);
            }
         }
      }

      public static void MakeInequalityOfType(LPT task, LPT.Sign sign)
      {
         switch (sign)
         {
            case LPT.Sign.MoreOrEqual:
               for (int i = 0; i < task.RCount; ++i)
               {
                  if (task.Signs[i] == LPT.Sign.LessOrEqual)
                  {
                     task.B.At(i, -task.B.At(i));
                     task.A.SetRow(i, task.A.Row(i).Multiply(-1));
                     SwitchSign(task.Signs, i);
                  }
               }
               break;
            case LPT.Sign.LessOrEqual:
               for (int i = 0; i < task.RCount; ++i)
               {
                  if (task.Signs[i] == LPT.Sign.MoreOrEqual)
                  {
                     task.B.At(i, -task.B.At(i));
                     task.A.SetRow(i, task.A.Row(i).Multiply(-1));
                     SwitchSign(task.Signs, i);
                  }
               }
               break;
            default:
               Helpers.Assert(false);
               break;
         }
      }
   }

   public class LPT
   {
      private readonly int _vCount;
      private readonly int _rCount;
      private readonly Vector<double> _CtargetFunc;
      private readonly Matrix<double> _A;
      private readonly Vector<double> _B;

      private readonly Sign[] _sign;
      private readonly bool[] _signRestric;

      private bool _building           = true;
      private bool _targetFuncIsSet    = false;
      private int _rIdx                = 0;

      public enum Sign
      {
         Equality,
         MoreOrEqual,
         LessOrEqual
      }

      public enum Mode
      {
         MinimizeTargetFunc,
         MaximizeTargetFunc
      }

      private Mode _mode;

      public LPT(int vCount, int rCount)
      {
         Helpers.AssertMsg(vCount > 0 && rCount > 0,
            "LPT parameters is incorrect!");

         _vCount        = vCount;
         _rCount        = rCount;
         _A             = Matrix<double>.Build.Dense(rCount, vCount);
         _B             = Vector<double>.Build.Dense(rCount);
         _CtargetFunc   = Vector<double>.Build.Dense(vCount);

         _sign          = new Sign[rCount];
         _signRestric   = new bool[vCount];
      }

      public LPT(LPT task)
      {
         _vCount = task.VCount;
         _rCount = task.RCount;
         _A = Matrix<double>.Build.DenseOfMatrix(task.A);
         _B = Vector<double>.Build.DenseOfVector(task.B);
         _CtargetFunc = Vector<double>.Build.DenseOfVector(task.F);

         _sign = task.Signs.Clone() as Sign[];
         _signRestric = task._signRestric.Clone() as bool[];
         _mode = task.Mode_;

         _rIdx = _rCount;
         _targetFuncIsSet = true;

         Realize();
      }

      #region Building

      public void AddRestriction(double[] a, double b, Sign s)
      {
         Helpers.AssertMsg(_building, "LPT must be in building stage!");
         Helpers.AssertMsg(a != null && a.Length == _vCount, "Row array is incorrect!");
         Helpers.AssertMsg(_rIdx < _rCount, "Restriction overflow!");

         _A.SetRow(_rIdx, a);
         _B.At(_rIdx, b);
         _sign[_rIdx] = s;

         _rIdx++;
      }

      public void SetSignRestriction(int vIdx)
      {
         Helpers.AssertMsg(_building, "LPT must be in building stage!");
         Helpers.AssertMsg(vIdx >= 0 && vIdx < _vCount, "Variable index is incorrect!");

         _signRestric[vIdx] = true;
      }

      public void SetTargetFunction(double[] c, Mode mode)
      {
         Helpers.AssertMsg(_building, "LPT must be in building stage!");
         Helpers.AssertMsg(!_targetFuncIsSet, "Target function is already set!");
         Helpers.AssertMsg(c != null && c.Length == _vCount, "Target function array is incorrect!");

         _targetFuncIsSet = true;
         _CtargetFunc.SetValues(c);
         _mode = mode;
      }

      public void Realize()
      {
         Helpers.AssertMsg(_building, "LPT was realized!");
         Helpers.AssertMsg(_rIdx == _rCount, String.Format("{0} restriction is not set!", _rCount - _rIdx));
         Helpers.AssertMsg(_targetFuncIsSet, "Target function is not set!");

         _building = false;
      }

      #endregion

      #region Methods

      public bool isCanonical()
      {
         Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");

         bool onlyRestrictVar       = !Array.Exists(_signRestric, r => !r);
         bool onlyEquality          = !Array.Exists(_sign, s => s != Sign.Equality);
         bool minimizeTargetFunc    = _mode == Mode.MinimizeTargetFunc;

         return onlyRestrictVar && onlyEquality && minimizeTargetFunc;
      }

      public LPT ToCanonical()
      {
         Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");

         int nonRestrictVar         = _signRestric.Count(r => !r);
         int inequalityRestrict     = _sign.Count(s => s != Sign.Equality);

         int restrictVar            = _vCount - nonRestrictVar;
         int equalityRestrict       = _rCount - inequalityRestrict;

         int newVCount              = nonRestrictVar * 2 + restrictVar + inequalityRestrict;
         int newRCount              = _rCount;

         LPT canonical              = new LPT(newVCount, newRCount);
         double[] buildingRow       = new double[newVCount];

         // set restriction
         int inequalityIdx = 0;
         for (int r = 0; r < _rCount; ++r)
         {
            switch (_sign[r])
            {
               case Sign.Equality:
                  {
                     int newV = 0;
                     for (int v = 0; v < _vCount; ++v)
                     {
                        buildingRow[newV++] = _A.At(r, v);
                        if (!isVarRestric(v))
                           buildingRow[newV++] = -_A.At(r, v);
                     }
                     while (newV < newVCount)
                        buildingRow[newV++] = 0;

                     canonical.AddRestriction(buildingRow, _B.At(r), Sign.Equality);
                  }
                  break;
               case Sign.MoreOrEqual:
                  {
                     int newV = 0;
                     for (int v = 0; v < _vCount; ++v)
                     {
                        buildingRow[newV++] = _A.At(r, v);
                        if (!isVarRestric(v))
                           buildingRow[newV++] = -_A.At(r, v);
                     }
                     for (int i = newV; i < newVCount; ++i)
                        buildingRow[i] = 0;
                     buildingRow[newV + inequalityIdx] = -1;

                     canonical.AddRestriction(buildingRow, _B.At(r), Sign.Equality);
                     inequalityIdx++;
                  }
                  break;
               case Sign.LessOrEqual:
                  {
                     int newV = 0;
                     for (int v = 0; v < _vCount; ++v)
                     {
                        buildingRow[newV++] = _A.At(r, v);
                        if (!isVarRestric(v))
                           buildingRow[newV++] = -_A.At(r, v);
                     }
                     for (int i = newV; i < newVCount; ++i)
                        buildingRow[i] = 0;
                     buildingRow[newV + inequalityIdx] = 1;

                     canonical.AddRestriction(buildingRow, _B.At(r), Sign.Equality);
                     inequalityIdx++;
                  }
                  break;
            }
         }
         Helpers.AssertMsg(inequalityIdx == inequalityRestrict, "Not all inequality are processed!");

         // set target function
         switch (_mode)
         {
            case Mode.MinimizeTargetFunc:
               {
                  int newV = 0;
                  for (int v = 0; v < _vCount; ++v)
                  {
                     buildingRow[newV++] = _CtargetFunc.At(v);
                     if (!isVarRestric(v))
                        buildingRow[newV++] = -_CtargetFunc.At(v);
                  }
                  while (newV < newVCount)
                     buildingRow[newV++] = 0;
               }
               break;
            case Mode.MaximizeTargetFunc:
               {
                  int newV = 0;
                  for (int v = 0; v < _vCount; ++v)
                  {
                     buildingRow[newV++] = -_CtargetFunc.At(v);
                     if (!isVarRestric(v))
                        buildingRow[newV++] = -(-_CtargetFunc.At(v));
                  }
                  while (newV < newVCount)
                     buildingRow[newV++] = 0;
               }
               break;
         }
         canonical.SetTargetFunction(buildingRow, Mode.MinimizeTargetFunc);

         // set sign restriction
         for (int i = 0; i < newVCount; ++i)
         {
            canonical.SetSignRestriction(i);
         }

         canonical.Realize();
         Helpers.AssertMsg(canonical.isCanonical(), "LPT is not canonical!");

         return canonical;
      }

      public LPT ToDual()
      {
         Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");

         int newVCount = _rCount;
         int newRCount = _vCount;
         LPT dual = new LPT(newVCount, newRCount);

         LPT copy = new LPT(this);
         switch (_mode)
         {
            case Mode.MinimizeTargetFunc:
               Helpers.MakeInequalityOfType(copy, Sign.MoreOrEqual);
               for (int i = 0; i < copy.RCount; ++i)
               {
                  if (copy.Signs[i] == Sign.MoreOrEqual)
                     dual.SetSignRestriction(i);
                  else
                     Helpers.Assert(copy.Signs[i] == Sign.Equality);
               }
               for (int i = 0; i < copy.VCount; ++i)
               {
                  Sign s;
                  if (copy.isVarRestric(i))
                     s = Sign.LessOrEqual;
                  else
                     s = Sign.Equality;

                  dual.AddRestriction(copy.A.Column(i).ToArray(), copy.F.At(i), s);
               }
               dual.SetTargetFunction(copy.B.ToArray(), Mode.MaximizeTargetFunc);
               break;
            case Mode.MaximizeTargetFunc:
               Helpers.MakeInequalityOfType(copy, Sign.LessOrEqual);
               for (int i = 0; i < copy.RCount; ++i)
               {
                  if (copy.Signs[i] == Sign.LessOrEqual)
                     dual.SetSignRestriction(i);
                  else
                     Helpers.Assert(copy.Signs[i] == Sign.Equality);
               }
               for (int i = 0; i < copy.VCount; ++i)
               {
                  Sign s;
                  if (copy.isVarRestric(i))
                     s = Sign.MoreOrEqual;
                  else
                     s = Sign.Equality;

                  dual.AddRestriction(copy.A.Column(i).ToArray(), copy.F.At(i), s);
               }
               dual.SetTargetFunction(copy.B.ToArray(), Mode.MinimizeTargetFunc);
               break;
         }

         dual.Realize();
         return dual;
      }

      public LPT NewWithRestriction(double[] a, double b, Sign s)
      {
          Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");

          LPT res = new LPT(VCount, RCount + 1);
          res._A.SetSubMatrix(0, 0, _A);
          res._A.SetRow(RCount, a);
          res._B.SetSubVector(0, RCount, _B);
          res._B.At(RCount, b);

          for (int i = 0; i < RCount; ++i)
              res._sign[i] = _sign[i];
          res._sign[RCount] = s;

          for (int i = 0; i < VCount; ++i)
              res._signRestric[i] = _signRestric[i];

          res._mode = Mode_;
          _CtargetFunc.CopyTo(res._CtargetFunc);

          res._building = false;
          return res;
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();

         string mode = "";
         switch (_mode)
         {
            case Mode.MinimizeTargetFunc:
               mode = "min";
               break;
            case Mode.MaximizeTargetFunc:
               mode = "max";
               break;
         }
         sb.Append(String.Format("f(x) = {0} -> {1}\n", RowToString(_CtargetFunc.ToArray()), mode));

         sb.Append("Restrictions:\n");
         for (int i = 0; i < _rCount; ++i)
         {
            string sign = "";
            switch (_sign[i])
            {
               case Sign.Equality:
                  sign = "=";
                  break;
               case Sign.MoreOrEqual:
                  sign = ">=";
                  break;
               case Sign.LessOrEqual:
                  sign = "<=";
                  break;
            }

            sb.Append(String.Format("{0} {1} {2}\n",
               RowToString(_A.Row(i).ToArray()), sign, Helpers.isZero(_B.At(i)) ? 0 : _B.At(i)));
         }

         bool restrict = false;
         int idx = 0;
         while (idx < _vCount && !_signRestric[idx])
            idx++;
         if (idx < _vCount)
         {
            restrict = true;
            sb.Append(String.Format("Variable restrictions:\nx{0}", idx + 1));
         }
         idx++;
         while (idx < _vCount)
         {
            if (_signRestric[idx])
               sb.Append(String.Format(", x{0}", idx + 1));
            idx++;
         }
         if (restrict)
            sb.Append(" >= 0\n");
         else
            sb.Append("Variable restrictions: non.");

         return sb.ToString();
      }

      private string RowToString(double[] row)
      {
         StringBuilder sb = new StringBuilder();

         int i = 0;
         while (Helpers.isZero(row[i]))
         {
            i++;
            if (i == row.Length)
               return "0";
         }

         if (Helpers.isZero(row[i] - 1))
            sb.Append(String.Format("x{0}", i + 1));
         else if (Helpers.isZero(row[i] + 1))
            sb.Append(String.Format("-x{0}", i + 1));
         else
            sb.Append(String.Format("{0}x{1}", row[i], i + 1));
         i++;

         for (; i < row.Length; ++i)
         {
            if (Helpers.isZero(row[i]))
               continue;

            if (Helpers.isZero(row[i] - 1))
               sb.Append(String.Format("+x{0}", i + 1));
            else if (Helpers.isZero(row[i] + 1))
               sb.Append(String.Format("-x{0}", i + 1));
            else if (row[i] > 0)
               sb.Append(String.Format("+{0}x{1}", row[i], i + 1));
            else
               sb.Append(String.Format("{0}x{1}", row[i], i + 1));
         }

         return sb.ToString();
      }

      #endregion

      #region Getters

      public Matrix<double> A
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _A;
         }
      }

      public Vector<double> B
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _B;
         }
      }

      public Vector<double> F
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _CtargetFunc;
         }
      }

      public int VCount
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _vCount;
         }
      }

      public int RCount
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _rCount;
         }
      }

      public Mode Mode_
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _mode;
         }
      }

      public Sign[] Signs
      {
         get
         {
            Helpers.AssertMsg(!_building, "LPT mast be in not building stage!");
            return _sign;
         }
      }

      #endregion

      #region Helpers


      public bool isVarRestric(int varIdx)
      {
         Helpers.AssertMsg(varIdx >= 0 && varIdx < _vCount, "Variable index is incorrect!");
         return _signRestric[varIdx];
      }

      #endregion
   }
}
