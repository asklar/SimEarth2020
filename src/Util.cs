using System;
using System.Windows;

namespace SimEarth2020
{
    public static class Util
    {
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            var p = element.Parent as FrameworkElement;
            while (p != null && !(p is T))
            {
                p = p.Parent as FrameworkElement;
            }
            return (T)p;
        }


        /// <summary>
        /// Returns a Gaussian value with mean 0 and stddev 1
        /// </summary>
        /// <returns></returns>
        public static double GetNormal(this Random rand)
        {
            double u1 = rand.NextDouble();
            double u2 = rand.NextDouble();
            return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }

        public static string MakeEnumName(string v)
        {
            string r = "";
            bool lastWasUppercase = true;
            foreach (var c in v)
            {
                if (char.IsUpper(c) && !lastWasUppercase)
                {
                    r += ' ';
                }
                if (char.IsDigit(c))
                {
                    r += (char)(c - '0' + '₀');
                }
                else
                {
                    r += c;
                }
                lastWasUppercase = char.IsUpper(c);
            }
            return r;
        }

    }
}
