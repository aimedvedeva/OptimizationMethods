using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Lab1_2TransportationProblem
{
   public static class ExtensionMethods
   {
      private static Action EmptyDelegate = delegate() { };

      public static void Refresh(this UIElement uiElement)
      {
         uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
      }
   }

   public class TPClient : FrameworkElement
   {
      private readonly Pen _blackPen = new Pen(Brushes.Black, 1);
      private readonly float _requirements;

      public TPClient(float requirements)
      {
         Height = 40;
         Width = 60;

         _requirements = requirements;
      }

      public void Render(DrawingContext drawingContext)
      {
         drawingContext.DrawRectangle(Brushes.Transparent, _blackPen, new Rect(0, 0, Width, Height));

         FormattedText _B = new FormattedText(
            "B",
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_B, new Point(25, 5));

         FormattedText _R = new FormattedText(
            String.Format("{0}", _requirements),
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_R, new Point(5, 20));
      }
   }

   public class TPProvider : FrameworkElement
   {
      private readonly Pen _blackPen = new Pen(Brushes.Black, 1);
      private readonly float _provided;

      public TPProvider(float provided)
      {
         Height = 40;
         Width = 60;
         _provided = provided;
      }

      public void Render(DrawingContext drawingContext)
      {
         drawingContext.DrawRectangle(Brushes.Transparent, _blackPen, new Rect(0, 0, Width, Height));

         FormattedText _A = new FormattedText(
            "A",
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_A, new Point(25, 5));

         FormattedText _P = new FormattedText(
            String.Format("{0}", _provided),
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_P, new Point(5, 20));
      }
   }

   public class TPElement : FrameworkElement
   {
      private readonly Pen _blackPen = new Pen(Brushes.Black, 1);

      public float Value { get; set; }

      public TPElement()
      {
         Height = 40;
         Width = 60;
      }

      public void Render(DrawingContext drawingContext)
      {
         drawingContext.DrawRectangle(Brushes.Transparent, _blackPen, new Rect(0, 0, Width, Height));

         if (Value == 0)
            return;

         FormattedText _V = new FormattedText(
            String.Format("{0}", Value),
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_V, new Point(5, 20));
      }
   }

   public class TPPrice : FrameworkElement
   {
      private readonly Pen _blackPen = new Pen(Brushes.Black, 1);

      public float Value { get; set; }

      public TPPrice()
      {
         Height = 40;
         Width = 60;
      }

      public void Render(DrawingContext drawingContext)
      {
         drawingContext.DrawRectangle(Brushes.Transparent, _blackPen, new Rect(0, 0, Width, Height));

         if (Value == float.NaN)
            return;

         FormattedText _A = new FormattedText(
            "Price",
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_A, new Point(5, 5));

         FormattedText _P = new FormattedText(
            String.Format("{0}", Value),
            CultureInfo.GetCultureInfo("en-us"),
            FlowDirection.LeftToRight,
            new Typeface("Courier New"),
            14,
            Brushes.Black);
         drawingContext.DrawText(_P, new Point(5, 20));
      }
   }

   public class TPTable : FrameworkElement
   {
      private TPElement[,] elements;
      private TPClient[] clients;
      private TPProvider[] providers;
      private TPPrice price;
      private Point[] cycle;

      public TPTable()
      {
         MainPlagin.Instance.PropertyChanged += OnTransoprtProblemChanged;
      }

      private void OnTransoprtProblemChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "TransportProblem")
            OnTransoprtProblemLoad();
         else if (e.PropertyName == "Plan")
            OnPlanChanged();
         else if (e.PropertyName == "Cycle")
            OnCycleChanged();
      }

      private void OnPlanChanged()
      {
         var plan = MainPlagin.Instance.Plan;

         for (int j = 0; j < elements.GetLength(0); ++j)
         {
            for (int i = 0; i < elements.GetLength(1); ++i)
            {
               if (float.IsNaN(plan[j, i]))
                  elements[j, i].Value = 0;
               else
                  elements[j, i].Value = plan[j, i];
            }
         }

         price.Value = MainPlagin.Instance.Price;

         Refresh();
      }

      private void OnCycleChanged()
      {
         cycle = MainPlagin.Instance.Cycle;

         Refresh();
      }

      private void OnTransoprtProblemLoad()
      {
         var tp = MainPlagin.Instance.TransportProblem;

         providers = new TPProvider[tp.ASize];
         clients = new TPClient[tp.BSize];
         elements = new TPElement[tp.ASize, tp.BSize];
         price = new TPPrice();

         for (int i = 0; i < tp.ASize; ++i)
         {
            providers[i] = new TPProvider(tp.mA[i]);
         }

         for (int i = 0; i < tp.BSize; ++i)
         {
            clients[i] = new TPClient(tp.mB[i]);
         }

         for (int j = 0; j < tp.ASize; ++j)
         {
            for (int i = 0; i < tp.BSize; ++i)
               elements[j, i] = new TPElement();
         }
      }

      private void DrawArrowDown(DrawingContext drawingContext)
      {
         Point z = new Point(0, 0);
         Point pLeft = new Point(-3, -12);
         Point pRight = new Point(3, -12);

         drawingContext.DrawLine(_redPen, z, pLeft);
         drawingContext.DrawLine(_redPen, z, pRight);
      }

      private void DrawArrow(DrawingContext drawingContext, Point p1, Point p2)
      {
         Point del = new Point(p1.X - p2.X, p1.Y - p2.Y);

         drawingContext.PushTransform(new TranslateTransform(p2.X, p2.Y));
         if (del.X == 0 && del.Y < 0) // down
         {
            DrawArrowDown(drawingContext);
         }
         else if (del.X == 0 && del.Y > 0) // up
         {
            drawingContext.PushTransform(new RotateTransform(180));
            DrawArrowDown(drawingContext);
            drawingContext.PushTransform(new RotateTransform(-180));
         }
         else if (del.Y == 0 && del.X > 0)
         {
            drawingContext.PushTransform(new RotateTransform(90));
            DrawArrowDown(drawingContext);
            drawingContext.PushTransform(new RotateTransform(-90));
         }
         else if (del.Y == 0 && del.X < 0)
         {
            drawingContext.PushTransform(new RotateTransform(-90));
            DrawArrowDown(drawingContext);
            drawingContext.PushTransform(new RotateTransform(90));
         }

         drawingContext.PushTransform(new TranslateTransform(-p2.X, -p2.Y));
      }

      private readonly Pen _redPen = new Pen(Brushes.Red, 1);
      private void DrawCycle(DrawingContext drawingContext)
      {
         if (cycle == null)
            return;

         for (int i = 0; i < cycle.Length - 1; ++i)
         {
            Point p1 = new Point(60 * cycle[i].Y + 30, 40 * cycle[i].X + 20);
            Point p2 = new Point(60 * cycle[i + 1].Y + 30, 40 * cycle[i + 1].X + 20);

            drawingContext.DrawLine(_redPen, p1, p2);
            DrawArrow(drawingContext, p2, p1);
         }

         Point p1_ = new Point(60 * cycle[cycle.Length - 1].Y + 30, 40 * cycle[cycle.Length - 1].X + 20);
         Point p2_ = new Point(60 * cycle[0].Y + 30, 40 * cycle[0].X + 20);

         drawingContext.DrawLine(_redPen, p1_, p2_);
         DrawArrow(drawingContext, p2_, p1_);

         Rect mainElem = new Rect(60 * cycle[cycle.Length - 1].Y + 30 - 3, 40 * cycle[cycle.Length - 1].X + 20 - 3, 6, 6);
         drawingContext.DrawRectangle(Brushes.Red, _redPen, mainElem);
      }

      private readonly Pen _blackPen = new Pen(Brushes.Black, 1);
      protected override void OnRender(DrawingContext drawingContext)
      {
         base.OnRender(drawingContext);

         if (providers == null)
            return;

         price.Render(drawingContext);

         drawingContext.PushTransform(new TranslateTransform(0, 40));
         for (int i = 0; i < providers.Length; ++i)
         {
            providers[i].Render(drawingContext);
            drawingContext.PushTransform(new TranslateTransform(0, 40));
         }
         drawingContext.PushTransform(new TranslateTransform(0, -40 * (providers.Length + 1)));

         drawingContext.PushTransform(new TranslateTransform(60, 0));
         for (int i = 0; i < clients.Length; ++i)
         {
            clients[i].Render(drawingContext);
            drawingContext.PushTransform(new TranslateTransform(60, 0));
         }
         drawingContext.PushTransform(new TranslateTransform(-60 * (clients.Length + 1), 0));

         drawingContext.PushTransform(new TranslateTransform(0, 40));
         for (int j = 0; j < elements.GetLength(0); ++j)
         {
            drawingContext.PushTransform(new TranslateTransform(60, 0));
            for (int i = 0; i < elements.GetLength(1); ++i)
            {
               elements[j, i].Render(drawingContext);
               drawingContext.PushTransform(new TranslateTransform(60, 0));
            }
            drawingContext.PushTransform(new TranslateTransform(-60 * (clients.Length + 1), 0));
            drawingContext.PushTransform(new TranslateTransform(0, 40));
         }
         drawingContext.PushTransform(new TranslateTransform(0, -40 * (providers.Length + 1)));

         drawingContext.PushTransform(new TranslateTransform(60, 40));
         DrawCycle(drawingContext);
         drawingContext.PushTransform(new TranslateTransform(-60, -40));

         drawingContext.DrawRectangle(Brushes.Transparent, _blackPen, new Rect(0, 0, Width, Height));
      }

      private static readonly DependencyProperty UpdaterProp =
         DependencyProperty.Register("Updater", typeof(bool), typeof(TPTable),
         new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

      private bool Updater
      {
         get { return (bool)base.GetValue(UpdaterProp); }
         set { base.SetValue(UpdaterProp, value); }
      }

      private void Refresh()
      {
         Updater = !Updater;
      }
   }
}
