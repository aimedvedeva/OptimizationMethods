using LinearProgrammingTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lab1_2TransportationProblem
{
   public class TransportProblem
   {
      class InvalidInpFormat : ApplicationException
      {
         public InvalidInpFormat() : base() { }
         public InvalidInpFormat(string str) : base(str) { }
         public override string ToString()
         {
            return Message;
         }
      }
      public float[] mA;
      public float[] mB;
      public float[,] mC;
      public int ASize;
      public int BSize;

      public TransportProblem(float[] nA, float[] nB, float[,] nC)
      {
         if ((nA.Length != nC.GetLength(0)) || (nB.Length != nC.GetLength(1)))
            throw new InvalidInpFormat("incorrect count of clients and costs");

         this.mA = nA;
         this.mB = nB;
         this.mC = nC;
         this.ASize = nA.Length;
         this.BSize = nB.Length;
      }
      public TransportProblem(int _Asize, int _Bsize, string sA, string sB, string[] sC)
      {
         ASize = _Asize;
         BSize = _Bsize;
         mA = new float[ASize];
         mB = new float[BSize];
         mC = new float[ASize, BSize];

         float tmpVal = 0;
         string[] strBuff;

         strBuff = sA.Split(' ');
         if (strBuff.Length != ASize)
            throw new InvalidInpFormat("incorrect count of providers and costs");

         for (int i = 0; i < mA.Length; i++)
         {
            if (float.TryParse(strBuff[i], out tmpVal))
               mA[i] = tmpVal;
         }

         strBuff = sB.Split(' ');
         if (strBuff.Length != BSize)
            throw new InvalidInpFormat("incorrect count of providers and costs");

         for (int i = 0; i < mB.Length; i++)
         {
            if (float.TryParse(strBuff[i], out tmpVal))
               mB[i] = tmpVal;
         }

         float sumA = 0;
         float sumB = 0;
         Array.ForEach(mA, f => sumA += f);
         Array.ForEach(mB, f => sumB += f);

         float dif = sumA - sumB;
         if (dif > 0)
         {
            float[] bufArr = mB;
            mB = new float[bufArr.Length + 1];
            bufArr.CopyTo(mB, 0);
            mB[mB.Length - 1] = Math.Abs(dif);
            BSize++;
            float[,] Ctmp = new float[ASize, BSize];
            for (int j = 0; j < BSize - 1; ++j)
            {
               for (int i = 0; i < ASize; ++i)
                  Ctmp[i, j] = mC[i, j];
            }
            mC = Ctmp;
         }
         else if (dif < 0)
         {
            float[] bufArr = mA;
            mA = new float[bufArr.Length + 1];
            bufArr.CopyTo(mA, 0);
            mA[mA.Length - 1] = Math.Abs(dif);
            ASize++;
            float[,] Ctmp = new float[ASize, BSize];
            for (int j = 0; j < ASize - 1; ++j)
            {
               for (int i = 0; i < BSize; ++i)
                  Ctmp[j, i] = mC[j, i];
            }
            mC = Ctmp;
            //for (int i = 0; i < BSize; ++i)
            //   Ctmp[ASize - 1, i] = 0;
         }

         for (int j = 0; j < sC.Length; j++)
         {
            strBuff = sC[j].Split(' ');
            if (strBuff.Length != _Bsize)
               throw new InvalidInpFormat("incorrect count of providers and costs");

            for (int i = 0; i < _Bsize; i++)
            {
               if (float.TryParse(strBuff[i], out tmpVal))
                  mC[j, i] = tmpVal;
            }
         }
      }

      private string Restrictions(int startIdx, int endIdx)
      {
         StringBuilder sb = new StringBuilder("x");
         sb.Append(startIdx);

         for (int i = startIdx + 1; i <= endIdx; ++i)
         {
            sb.Append("+x");
            sb.Append(i);
         }

         return sb.ToString();
      }

      public LPT ToLPT()
      {
         int newVCount = ASize * BSize;
         int newRCount = ASize + BSize;
         LPT lpt = new LPT(newVCount, newRCount);
         double[] buildRow;

         for (int i = 0; i < ASize; ++i)
         {
            buildRow = new double[ASize * BSize];
            for (int j = 0; j < BSize; ++j)
               buildRow[i * BSize + j] = 1;

            lpt.AddRestriction(buildRow, mA[i], LPT.Sign.Equality);
         }

         for (int i = 0; i < BSize; ++i)
         {
            buildRow = new double[ASize * BSize];
            for (int j = 0; j < ASize; ++j)
               buildRow[j * BSize + i] = 1;
            lpt.AddRestriction(buildRow, mB[i], LPT.Sign.Equality);
         }

         buildRow = new double[ASize * BSize];
         for (int j = 0; j < ASize; ++j)
         {
            for (int i = 0; i < BSize; ++i)
               buildRow[j * BSize + i] = mC[j, i];
         }
         lpt.SetTargetFunction(buildRow, LPT.Mode.MinimizeTargetFunc);

         for (int i = 0; i < ASize * BSize; ++i)
            lpt.SetSignRestriction(i);

         lpt.Realize();
         return lpt;
      }


      static readonly float INVALID_VAL = float.NaN;

      bool isEmpty(float[] arr)
      {
         return Array.TrueForAll(arr, delegate(float x) { return x == 0; });
      }

      private void NanToEmpty(float[,] outArr)
      {
         int i = 0, j = 0;
         for (i = 0; i < ASize; i++)
         {
            for (j = 0; j < BSize; j++)
            {
               if (outArr[i, j] == 0)
                  outArr[i, j] = INVALID_VAL;
            }
         }
      }

      float findMin(float[,] Arr, bool[,] pr, out int indi, out int indj)
      {
         indi = -1; indj = -1;
         float min = float.MaxValue;
         for (int i = 0; i < ASize; i++)
         {
            for (int j = 0; j < BSize; j++)
            {
               if ((pr[i, j]) && (Arr[i, j] < min))
               {
                  min = Arr[i, j];
                  indi = i; indj = j;
               }
            }
         }

         return min;
      }

      public float[,] NordWest()
      {
         float[] Ahelp = mA;
         float[] Bhelp = mB;
         int i = 0, j = 0;
         float[,] outArr = new float[ASize, BSize];
         NanToEmpty(outArr);

         while (!(isEmpty(Ahelp) && isEmpty(Bhelp)))
         {
            float Dif = Math.Min(Ahelp[i], Bhelp[j]);
            outArr[i, j] = Dif;
            Ahelp[i] -= Dif;
            Bhelp[j] -= Dif;
            if ((Ahelp[i] == 0) && (Bhelp[j] == 0) && (j + 1 < BSize))
               outArr[i, j + 1] = 0;
            if (Ahelp[i] == 0)
               i++;
            if (Bhelp[j] == 0)
               j++;
         }

         return outArr;
      }

      class FindWay
      {
         FindWay Father;
         Point Root;
         FindWay[] Childrens;
         Point[] mAllowed;
         Point Begining;
         bool flag;

         public FindWay(int x, int y, bool _flag, Point[] _mAllowed, Point _Beg, FindWay _Father)
         {
            Begining = _Beg;
            flag = _flag;
            Root = new Point(x, y);
            mAllowed = _mAllowed;
            Father = _Father;
         }
         public Boolean BuildTree()
         {
            Point[] ps = new Point[mAllowed.Length];
            int Count = 0;
            for (int i = 0; i < mAllowed.Length; i++)
            {
               if (flag)
               {
                  if (Root.Y == mAllowed[i].Y)
                  {
                     Count++;
                     ps[Count - 1] = mAllowed[i];
                  }

               }
               else
               {
                  if (Root.X == mAllowed[i].X)
                  {
                     Count++;
                     ps[Count - 1] = mAllowed[i];
                  }
               }
            }

            FindWay fwu = this;
            Childrens = new FindWay[Count];
            int k = 0;
            for (int i = 0; i < Count; i++)
            {
               if (ps[i] == Root)
                  continue;

               if (ps[i] == Begining)
               {
                  while (fwu != null)
                  {
                     mAllowed[k] = fwu.Root;
                     fwu = fwu.Father;
                     k++;
                  };
                  for (; k < mAllowed.Length; k++)
                     mAllowed[k] = new Point(-1, -1);
                  return true;
               }

               if (!Array.TrueForAll(ps, p => p.X == 0 && p.Y == 0))
               {
                  Childrens[i] = new FindWay((int)ps[i].X, (int)ps[i].Y, !flag, mAllowed, Begining, this);
                  bool result = Childrens[i].BuildTree();
                  if (result)
                     return true;
               }
            }
            return false;
         }

      }
      private void FindUV(float[] U, float[] V, float[,] HelpMatr)
      {
         bool[] U1 = new bool[ASize];
         bool[] U2 = new bool[ASize];
         bool[] V1 = new bool[BSize];
         bool[] V2 = new bool[BSize];

         while (!(AllTrue(V1) && AllTrue(U1)))
         {
            int i = -1;
            int j = -1;
            for (int i1 = BSize - 1; i1 >= 0; i1--)
               if (V1[i1] && !V2[i1])
                  i = i1;
            for (int j1 = ASize - 1; j1 >= 0; j1--)
               if (U1[j1] && !U2[j1])
                  j = j1;

            if (j == -1 && i == -1)
            {
               for (int i1 = BSize - 1; i1 >= 0; i1--)
               {
                  if (!V1[i1] && !V2[i1])
                  {
                     i = i1;
                     V[i] = 0;
                     V1[i] = true;
                     break;
                  }
               }
            }
            if (j == -1 && i == -1)
            {
               for (int j1 = ASize - 1; j1 >= 0; j1--)
               {
                  if (!U1[j1] && !U2[j1])
                  {
                     j = j1;
                     U[j] = 0;
                     U1[j] = true;
                     break;
                  }
               }
            }

            if (i != -1)
            {
               for (int j1 = 0; j1 < ASize; j1++)
               {
                  if (!U1[j1])
                     U[j1] = HelpMatr[j1, i] - V[i];
                  if (!float.IsNaN(U[j1]))
                     U1[j1] = true;
               }
               V2[i] = true;
            }

            if (j != -1)
            {
               for (int i1 = 0; i1 < BSize; i1++)
               {
                  if (!V1[i1])
                     V[i1] = HelpMatr[j, i1] - U[j];
                  if (!float.IsNaN(V[i1]))
                     V1[i1] = true;
               }
               U2[j] = true;
            }
         }
      }

      private Boolean AllPositive(float[,] m)
      {
         for (int i = 0; i < ASize; i++)
         {
            for (int j = 0; j < BSize; j++)
            {
               if (m[i, j] < 0)
                  return false;
            }
         }
         return true;
      }

      private bool AllTrue(bool[] arr)
      {
         return Array.TrueForAll(arr, x => x);
      }

      private float[,] MakeSMatr(float[,] M, float[] U, float[] V)
      {
         float[,] HM = new float[ASize, BSize];

         for (int i = 0; i < ASize; i++)
            for (int j = 0; j < BSize; j++)
            {
               HM[i, j] = M[i, j];
               if (float.IsNaN(HM[i, j]))
                  HM[i, j] = mC[i, j] - (U[i] + V[j]);
            }
         return HM;
      }

      private Point[] GetCycle(int x, int y, Point[] Allowed)
      {
         FindWay fw = new FindWay(x, y, true, Allowed, new Point(x, y), null);
         fw.BuildTree();
         Point[] Way = Array.FindAll(Allowed, p => p.X != -1 && p.Y != -1);
         return Way;
      }

      private void Roll(float[,] m, float[,] sm, Action<Point[]> CBnewCycle)
      {
         Point minInd = new Point();
         float min = float.MaxValue;
         int k = 0;
         Point[] allowed = new Point[ASize + BSize];
         for (int i = 0; i < ASize; i++)
         {
            for (int j = 0; j < BSize; j++)
            {
               if (!float.IsNaN(m[i, j]))
               {
                  allowed[k].X = i;
                  allowed[k].Y = j;
                  k++;
               }

               if (sm[i, j] < min)
               {
                  min = sm[i, j];
                  minInd.X = i;
                  minInd.Y = j;
               }
            }
         }

         allowed[allowed.Length - 1] = minInd;

         Point[] Cycle = GetCycle((int)minInd.X, (int)minInd.Y, allowed);

         CBnewCycle(Cycle);

         float[] Cycles = new float[Cycle.Length];

         min = float.MaxValue;
         for (int i = 0; i < Cycle.Length; i++)
         {
            int x = (int)Cycle[i].X;
            int y = (int)Cycle[i].Y;
            Cycles[i] = m[x, y];

            if (!float.IsNaN(Cycles[i]))
            {
               if (i % 2 == 0 && Cycles[i] < min)
               {
                  min = Cycles[i];
                  minInd = Cycle[i];
               }
            }
            else
               Cycles[i] = 0;
         }

         for (int i = 0; i < Cycle.Length; i++)
         {
            int x = (int)Cycle[i].X;
            int y = (int)Cycle[i].Y;

            if (i % 2 == 0)
            {
               Cycles[i] -= min;
               m[x, y] -= min;
            }
            else
            {
               Cycles[i] += min;

               if (float.IsNaN(m[x, y]))
                  m[x, y] = 0;

               m[x, y] += min;
            }
         }

         m[(int)minInd.X, (int)minInd.Y] = INVALID_VAL;
      }

      public float[,] PotenMeth(float[,] SupArr, Action<float[,]> CBnewPlan, Action<Point[]> CBnewCycle)
      {
         int i = 0, j = 0;
         float[,] HelpMatr = new float[ASize, BSize];
         for (i = 0; i < ASize; i++)
         {
            for (j = 0; j < BSize; j++)
            {
               if (!float.IsNaN(SupArr[i, j]))
                  HelpMatr[i, j] = mC[i, j];
               else
                  HelpMatr[i, j] = INVALID_VAL;
            }
         }

         float[] U = new float[ASize];
         float[] V = new float[BSize];
         FindUV(U, V, HelpMatr);
         float[,] SMatr = MakeSMatr(HelpMatr, U, V);

         while (!AllPositive(SMatr))
         {
            Roll(SupArr, SMatr, CBnewCycle);

            CBnewPlan(SupArr);

            for (i = 0; i < ASize; i++)
            {
               for (j = 0; j < BSize; j++)
               {
                  if (SupArr[i, j] == float.PositiveInfinity)
                  {
                     HelpMatr[i, j] = mC[i, j];
                     SupArr[i, j] = 0;
                     continue;
                  }

                  if (!float.IsNaN(SupArr[i, j]))
                     HelpMatr[i, j] = mC[i, j];
                  else
                     HelpMatr[i, j] = INVALID_VAL;
               }
            }

            FindUV(U, V, HelpMatr);
            SMatr = MakeSMatr(HelpMatr, U, V);
         }

         return SupArr;
      }
   }
}
