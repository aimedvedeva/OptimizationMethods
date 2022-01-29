using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab6_Plane
{
    class Program
    {
        static void Main(string[] args)
        {
            Control.UseManaged();

            PlaneMethod.Exec(1e-11);
        }
    }
}
