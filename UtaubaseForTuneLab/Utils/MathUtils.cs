using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtaubaseForTuneLab.Utils
{
    public class MathUtils
    {
        public static double Limit(double value,double min,double max)
        {
            return Math.Min(max,Math.Min(max,value));
        }
        public static int Limit(int value, int min, int max)
        {
            return Math.Min(max, Math.Min(max, value));
        }
        public static int RoundLimit(double value,int min,int max)
        {
            return (int)Math.Round(Limit(value, min, max));
        }
    }
}
