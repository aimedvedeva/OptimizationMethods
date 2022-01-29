using LinearProgrammingTask;
using SimplexMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Lab1_1Simplex
{
   /// <summary>
   /// Логика взаимодействия для MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private int _vCount = 5;
      private int _rCount = 5;

      private TextRange _logTextRange;

      public MainWindow()
      {
         InitializeComponent();

         _logTextRange = new TextRange(
             Log.Document.ContentStart, Log.Document.ContentEnd);
      }

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      private void Button_Click_2(object sender, RoutedEventArgs e)
      {
         _logTextRange.Text = "";

         LPT task;
         task = CreateTaskUI();
         //task = Task4();
         if (task == null)
            return;

         _logTextRange.Text = String.Format("Parsing success!\n" +
            "Linear Programming Task:\n{0}\n", task.ToString());

         LPT canonicalTask = task.ToCanonical();

         _logTextRange.Text += String.Format("Linear Programming Task (canonical):\n{0}\n",
            canonicalTask.ToString());

         LPTSolution s;
         Simplex.Result r;

         _logTextRange.Text += "Solution of main task:\n";
         r = Simplex.Execute(task, out s);
         if (r != Simplex.Result.Success)
         {
            PrintError(r);
         }
         else
            _logTextRange.Text += String.Format("{0}\n", s.ToString());


         LPT dual = task.ToDual();
         _logTextRange.Text += String.Format("Linear Programming Task (dual):\n{0}\n",
            dual.ToString());

         LPT dualCanonic = dual.ToCanonical();
         //_logTextRange.Text += String.Format("Linear Programming Task (dual canonic):\n{0}\n",
         //   dualCanonic.ToString());

         _logTextRange.Text += "Solution of dual task:\n";
         r = Simplex.Execute(dual, out s);
         if (r != Simplex.Result.Success)
         {
            PrintError(r);
         }
         else
            _logTextRange.Text += String.Format("{0}\n", s.ToString());


         LPT dualdual = dual.ToDual();
         //_logTextRange.Text += String.Format("Linear Programming Task (dual dual):\n{0}\n",
         //   dualdual.ToString());

         _logTextRange.Text += "Solution of dual dual task:\n";
         r = Simplex.Execute(dualdual, out s);
         if (r != Simplex.Result.Success)
         {
            PrintError(r);
         }
         else
            _logTextRange.Text += String.Format("{0}\n", s.ToString());
      }

      private void PrintError(Simplex.Result r)
      {
         switch (r)
         {
            case Simplex.Result.TargetFunctionNotLimited:
               _logTextRange.Text += "Solution Not Exists: Target function not limited!\n";
               break;
            case Simplex.Result.RestrictionMultiplicityIsEmpty:
               _logTextRange.Text += "Solution Not Exists: Restriction multiplicity is empty!\n";
               break;
         }
      }

      private LPT Task1()
      {
         int vCount = 6;
         int rCount = 3;
         _vCount = vCount;
         _rCount = rCount;

         LPT task = new LPT(vCount, rCount);
         double[] buildingRow = new double[vCount];
         double b;
         LPT.Sign sign;

         Helpers.Assert(ParseRestriction("x1+3x2-x3+2x5", "=", "7", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("-2x2+4x3+x4", "=", "12", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("-4x2+3x3+8x5+x6", "=", "10", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);

         Helpers.Assert(ParseRow("x2-3x3+2x5", buildingRow));
         task.SetTargetFunction(buildingRow, LPT.Mode.MaximizeTargetFunc);

         for (int i = 0; i < vCount; ++i)
            task.SetSignRestriction(i);

         task.Realize();
         return task;
      }

      private LPT Task2()
      {
         int vCount = 7;
         int rCount = 3;
         _vCount = vCount;
         _rCount = rCount;

         LPT task = new LPT(vCount, rCount);
         double[] buildingRow = new double[vCount];
         double b;
         LPT.Sign sign;

         Helpers.Assert(ParseRestriction("0,25x1-60x2-0,04x3+9x4+x5", "=", "1", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("0,5x1-90x2-0,02x3+3x4+x6", "=", "1", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("x3+x7", "=", "1", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);

         Helpers.Assert(ParseRow("-0,75x1+150x2-0,02x3+6x4", buildingRow));
         task.SetTargetFunction(buildingRow, LPT.Mode.MaximizeTargetFunc);

         for (int i = 0; i < vCount; ++i)
            task.SetSignRestriction(i);

         task.Realize();
         return task;
      }

      private LPT Task3()
      {
         int vCount = 5;
         int rCount = 3;
         _vCount = vCount;
         _rCount = rCount;

         LPT task = new LPT(vCount, rCount);
         double[] buildingRow = new double[vCount];
         double b;
         LPT.Sign sign;

         Helpers.Assert(ParseRestriction("3x1+2x2-x3+2x4-2x5", "=", "1", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("x1+x2-x3+3x4-2x5", "=", "0", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("4x1-x2+3x3-x4-7x5", "=", "2", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);

         Helpers.Assert(ParseRow("-3x2+4x3-4x4-6x5", buildingRow));
         task.SetTargetFunction(buildingRow, LPT.Mode.MaximizeTargetFunc);

         for (int i = 0; i < vCount; ++i)
            task.SetSignRestriction(i);

         task.Realize();
         return task;
      }

      private LPT Task4()
      {
         int vCount = 3;
         int rCount = 3;
         _vCount = vCount;
         _rCount = rCount;

         LPT task = new LPT(vCount, rCount);
         double[] buildingRow = new double[vCount];
         double b;
         LPT.Sign sign;

         Helpers.Assert(ParseRestriction("x1", "=", "-1", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("x2", "=", "-2", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);
         Helpers.Assert(ParseRestriction("x3", "=", "-3", buildingRow, out sign, out b));
         task.AddRestriction(buildingRow, b, sign);

         Helpers.Assert(ParseRow("x1+x2+x3", buildingRow));
         task.SetTargetFunction(buildingRow, LPT.Mode.MinimizeTargetFunc);

         for (int i = 0; i < vCount; ++i)
            task.SetSignRestriction(i);

         task.Realize();
         return task;
      }

      private LPT CreateTaskUI()
      {
         LPT task = new LPT(_vCount, _rCount);
         double[] buildingRow = new double[_vCount];

         for (int i = 1; i <= 5; ++i)
         {
            string restrictExprTextBox    = String.Format("A{0}", i);
            string restrictSignComboBox   = String.Format("AS{0}", i);
            string restrictNumTextBox     = String.Format("B{0}", i);
            string varRestrictCheckBox    = String.Format("S{0}", i);

            FieldInfo restrictExprField   = GetType().GetField(restrictExprTextBox, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo restrictSignField   = GetType().GetField(restrictSignComboBox, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo restrictNumField    = GetType().GetField(restrictNumTextBox, BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo signField           = GetType().GetField(varRestrictCheckBox, BindingFlags.NonPublic | BindingFlags.Instance);

            string restrictExpr           = ((TextBox)restrictExprField.GetValue(this)).Text;
            string restrictSign           = ((ComboBoxItem)((ComboBox)restrictSignField.GetValue(this)).SelectedItem).Content as string;
            string restrictNum            = ((TextBox)restrictNumField.GetValue(this)).Text;
            bool varRestrict              = ((CheckBox)signField.GetValue(this)).IsChecked.Value;

            double b;
            LPT.Sign sign;
            if (!ParseRestriction(restrictExpr, restrictSign, restrictNum,
               buildingRow, out sign, out b))
            {
               _logTextRange.Text = String.Format("Parsing error at {0} restriction!", i);
               return null;
            }
            task.AddRestriction(buildingRow, b, sign);

            if (varRestrict)
               task.SetSignRestriction(i - 1);
         }

         if (!ParseRow(F.Text, buildingRow))
         {
            _logTextRange.Text = String.Format("Parsing error at target function!");
            return null;
         }

         LPT.Mode mode = LPT.Mode.MinimizeTargetFunc;
         switch (((ComboBoxItem)FS.SelectedItem).Content as string)
         {
            case "min":
               mode = LPT.Mode.MinimizeTargetFunc;
               break;
            case "max":
               mode = LPT.Mode.MaximizeTargetFunc;
               break;
         }

         task.SetTargetFunction(buildingRow, mode);
         task.Realize();
         return task;
      }

      private bool ParseRestriction(string restrictExpr, string restrictSign, string restrictNum,
         double[] row, out LPT.Sign sign, out double b)
      {
         b = 0;
         sign = LPT.Sign.Equality;

         switch (restrictSign)
         {
            case "=":
               sign = LPT.Sign.Equality;
               break;
            case ">=":
               sign = LPT.Sign.MoreOrEqual;
               break;
            case "(=":
               sign = LPT.Sign.LessOrEqual;
               break;
            default:
               return false;
         }

         if (restrictNum.StartsWith("-"))
         {
            if (!Double.TryParse(restrictNum.Substring(1), out b))
               return false;
            b *= -1;
         }
         else
         {
            if (!Double.TryParse(restrictNum, out b))
               return false;
         }

         return ParseRow(restrictExpr, row);
      }

      private bool ParseRow(string restrictExpr, double[] row)
      {
         for (int idx = 0; idx < _vCount; ++idx)
            row[idx] = 0;

         string[] parth = restrictExpr.Split(new char[] { 'x' });
         string begin = parth[0];

         double num;
         if (restrictExpr.StartsWith("x"))
            num = 1;
         else
         {
            if (begin.StartsWith("-"))
            {
               if (Double.TryParse(begin.Substring(1), out num))
                  num *= -1;
               else
               {
                  if (begin != "-")
                     return false;
                  num = -1;
               }
            }
            else
            {
               if (!Double.TryParse(begin, out num))
                  return false;
            }
         }

         for (int i = 1; ; ++i)
         {
            int plusIdx = parth[i].LastIndexOf('+');
            if (plusIdx != -1)
            {
               int v;
               if (!Int32.TryParse(parth[i].Substring(0, plusIdx), out v))
                  return false;
               if (v <= 0 || v > _vCount)
                  return false;
               row[v - 1] = num;

               if (!Double.TryParse(parth[i].Substring(plusIdx + 1), out num))
               {
                  if (parth[i].Substring(plusIdx + 1) != "")
                     return false;
                  num = 1;
               }

               continue;
            }

            int minusIdx = parth[i].LastIndexOf('-');
            if (minusIdx != -1)
            {
               int v;
               if (!Int32.TryParse(parth[i].Substring(0, minusIdx), out v))
                  return false;
               if (v <= 0 || v > _vCount)
                  return false;
               row[v - 1] = num;

               if (!Double.TryParse(parth[i].Substring(minusIdx + 1), out num))
               {
                  if (parth[i].Substring(minusIdx + 1) != "")
                     return false;
                  num = 1;
               }
               num *= -1;

               continue;
            }

            int v1;
            if (!Int32.TryParse(parth[i], out v1))
               return false;
            if (v1 <= 0 || v1 > _vCount)
               return false;

            row[v1 - 1] = num;
            return true;
         }
      }

      #region Helpers

      private void Assert(bool condition)
      {
#if DEBUG
         if (!condition)
         {
            throw new SystemException("Assertion failed!");
         }
#endif
      }

      #endregion
   }
}
