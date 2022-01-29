using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Functions
{
    public abstract class Function
    {
        public abstract object Value(object[] param);
    }

    public abstract class D1DFunction : Function
    {
        public double Value(double x)
        {
            return (double)Value(new object[] { x });
        }
    }

    public abstract class DerFunction
    {
        public abstract object Value(object[] param);
        public abstract object Grad(object[] param);
    }

    public abstract class D2DDerFunction : DerFunction
    {
        public double Value(double x1, double x2)
        {
            return (double)Value(new object[] { x1, x2 });
        }

        public double[] Grad(double x1, double x2)
        {
            return (double[])Grad(new object[] { x1, x2 });
        }
    }
}
