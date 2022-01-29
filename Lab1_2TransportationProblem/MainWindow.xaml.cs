using LinearProgrammingTask;
using SimplexMethod;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

namespace Lab1_2TransportationProblem
{
   public class MainPlagin : INotifyPropertyChanged
   {
      public static MainPlagin Instance { get; set; }
      public static void InitPlagin()
      {
         Instance = new MainPlagin();
      }

      private TransportProblem _transportProblem;
      public TransportProblem TransportProblem
      {
         get { return _transportProblem; }
         set
         {
            _transportProblem = value;

            RaisePropertyChanged("TransportProblem");
         }
      }

      private float[,] _plan;
      public float[,] Plan
      {
         get { return _plan; }
         set
         {
            _plan = value;

            RaisePropertyChanged("Plan");
         }
      }

      private float _price = float.NaN;
      public float Price
      {
         get { return _price; }
         set { _price = value; }
      }

      private Point[] _cycle;
      public Point[] Cycle
      {
         get { return _cycle; }
         set
         {
            _cycle = value;

            RaisePropertyChanged("Cycle");
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void RaisePropertyChanged(string property)
      {
         var h = PropertyChanged;
         if (h != null)
            h(this, new PropertyChangedEventArgs(property));
      }
   }
   /// <summary>
   /// Логика взаимодействия для MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window, INotifyPropertyChanged
   {
      private TransportProblem _tp;
      private LPT _lpt;
      private float[,] preparation;

      private int _clientCount = 0;
      public int ClientCount
      {
         get { return _clientCount; }
         private set
         {
            _clientCount = value;

            RaisePropertyChanged("ClientCount");
         }
      }

      private int _providerCount = 0;
      public int ProviderCount
      {
         get { return _providerCount; }
         private set
         {
            _providerCount = value;

            RaisePropertyChanged("ClientCount");
         }
      }

      public MainWindow()
      {
         MainPlagin.InitPlagin();

         InitializeComponent();
         InputText.Text = "320 280 250\n320 140 110 230 50\n20 23 20 15 24\n29 15 16 19 29\n6 11 10 9 8";
      }

      private void RaisePropertyChanged(string property)
      {
         var h = PropertyChanged;
         if (h != null)
            h(this, new PropertyChangedEventArgs(property));
      }

      public event PropertyChangedEventHandler PropertyChanged;

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      private void Button_Click_2(object sender, RoutedEventArgs e)
      {
         String[] sArray = InputText.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
         String A = sArray[0];
         String B = sArray[1];
         int Asize = A.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
         int Bsize = B.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
         String[] C = new String[Asize];
         for (int i = 0; i < Asize; i++)
            C[i] = sArray[2 + i];

         _tp = new TransportProblem(Asize, Bsize, A, B, C);
         MainPlagin.Instance.TransportProblem = _tp;

         _lpt = _tp.ToLPT();

         preparation = _tp.NordWest();
         MainPlagin.Instance.Price = CalcPrice(preparation);
         MainPlagin.Instance.Plan = preparation;

         SimplexSolutionText.Text = "";
      }

      private void OnTemporaryPlan(float[,] temporaryPlan)
      {
         Delegate refresh = new Action(() => {
            MainPlagin.Instance.Price = CalcPrice(temporaryPlan);
            MainPlagin.Instance.Plan = temporaryPlan;
         });

         Dispatcher.Invoke(refresh, null);

         Thread.Sleep(1000);
      }

      private void OnCycle(Point[] cycle)
      {
         Delegate onCycle = new Action(() =>
         {
            MainPlagin.Instance.Cycle = cycle;
         });

         Dispatcher.Invoke(onCycle, null);

         Thread.Sleep(10000);

         onCycle = new Action(() =>
         {
            MainPlagin.Instance.Cycle = null;
         });

         Dispatcher.Invoke(onCycle, null);
      }

      private void Button_Click_3(object sender, RoutedEventArgs e)
      {
         if (preparation == null)
            return;

         float[,] optimal;
         Action proc = () =>
         {
            optimal = _tp.PotenMeth(preparation, OnTemporaryPlan, OnCycle);
            OnTemporaryPlan(optimal);
         };

         Thread t = new Thread(new ThreadStart(proc));
         t.Start();

         LPTSolution s;
         Simplex.Result r = Simplex.Execute(_lpt, out s);
         SimplexSolutionText.Text = SimplexSolutionToString(s);
      }

      private float CalcPrice(float[,] plan)
      {
         float Sum = 0;
         for (int i = 0; i < plan.Length; i++)
         {
            int j = (i - i % _tp.BSize) / _tp.BSize;
            int k = i % _tp.BSize;
            if (!float.IsNaN(plan[j, k]))
               Sum += plan[j, k] * _tp.mC[j, k];
         }

         return Sum;
      }

      private string SimplexSolutionToString(LPTSolution s)
      {
         StringBuilder sb = new StringBuilder();
         float[,] simplexPlan = new float[_tp.ASize, _tp.BSize];

         for (int j = 0; j < _tp.ASize; ++j)
         {
            for (int i = 0; i < _tp.BSize; ++i)
            {
               sb.Append(String.Format("{0}      ", (int)s.vVal.At(j * _tp.BSize + i)));
               simplexPlan[j, i] = (float)s.vVal.At(j * _tp.BSize + i);
            }

            sb.Append("\n");
         }

         sb.Append(String.Format("Price: {0}", CalcPrice(simplexPlan)));
         return sb.ToString();
      }
   }
}
