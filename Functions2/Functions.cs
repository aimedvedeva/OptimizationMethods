using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace Functions2
{
    public abstract class Function
    {
        public abstract object Value(object param);
    }

    public abstract class DerFunction : Function
    {
        public abstract object Grad(object param);
    }

    public abstract class DerDerFunction : DerFunction
    {
        public abstract object Gesse(object param);
    }

    public abstract class DDerDerFunction : DerDerFunction
    {
        public double Value(Vector<double> x)
        {
            return (double)Value((object)x);
        }

        public Vector<double> Grad(Vector<double> x)
        {
            return (Vector<double>)Grad((object)x);
        }

        public Matrix<double> Gesse(Vector<double> x)
        {
            return (Matrix<double>)Gesse((object)x);
        }
    }
}
