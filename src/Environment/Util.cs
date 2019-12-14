using System;
using System.Collections.Generic;

namespace Environment
{
    public static class Util
    {
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

        public static double Squared(this double v)
        {
            return v * v;
        }
        public static float Squared(this float v)
        {
            return v * v;
        }

        public static IEnumerable<T> Where<T>(this T[,] ts, Func<T, bool> predicate)
        {
            for (int x = 0; x < ts.GetLength(0); x++)
            {
                for (int y = 0; y < ts.GetLength(1); y++)
                {
                    if (predicate(ts[x, y]))
                    {
                        yield return ts[x, y];
                    }
                }
            }
        }


        public static bool DebugOn { get; set; } = false;
        public static void Debug(string s)
        {
            if (DebugOn)
            {
                System.Diagnostics.Debug.WriteLine(s);
            }
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
