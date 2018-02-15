using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brightness
{
    class Program
    {
        static void Main(string[] args)
        {
            double A = AreaSinSquared(Math.PI);
                int pre = 0;
                double avg = 0;
            for (int pct = 2; pct <= 100; pct += 2)
            {
                double val = 0;
                double max = Math.PI;
                double min = avg;
                double tgt = pct * A / 100;
                do
                {
                    avg = (max + min) / 2;
                    val = AreaSinSquared(avg);
                    var diff = val - tgt;
                    if (Math.Abs(diff) < .000005) break;
                    if (max == min) break;
                    if (diff < 0)
                        min = avg;
                    else
                        max = avg;
                } while (true);
                int that = (int) Math.Floor(avg / Math.PI * 666666);
                //Console.WriteLine(pct.ToString() + "% at " + that.ToString() + " diff: " + (that-pre).ToString());
                Console.WriteLine((that - pre).ToString());
                pre = that;
            }
            Console.Read();
        }

        private static double AreaSinSquared(double angle)
        {
            return angle / 2 - Math.Sin(angle * 2) / 4;
        }
    }
}
