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

        internal static AnimalStats GetStats(AnimalKind kind)
        {
            return new AnimalStats()
            {
                Kind = kind,
                MaxHP = 100,
                FoodPerTurn = 3.5,
                MaxFood = 100,
                ActionThreshold = 45,
                Speed = 0.4,
                FoodSources = new FoodSource[] {
                    new FoodSource() {
                        AnimalKind = AnimalKind.Prokaryote, Sun = true, Vegetation = true
                    }
                },
                CanSwim = true,
                CanWalk = false
            };
        }
    }
}
