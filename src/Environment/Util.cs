using System;
using System.Collections.Generic;

namespace Environment
{
    static class Util
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
    }
}
