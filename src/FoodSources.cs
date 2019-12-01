using System;
using System.Collections.Generic;

namespace SimEarth2020
{
    public class FoodSources
    {
        private int storage = 0;
        public bool Vegetation
        {
            get { return (storage & 2) == 2; }
            set { if (value) storage |= 2; else storage &= ~2; }
        }

        public bool Sun
        {
            get { return (storage & 1) == 1; }
            set { if (value) storage |= 1; else storage &= ~1; }
        }

        public FoodSources() { }
        public FoodSources(AnimalKind[] kinds)
        {
            foreach (var kind in kinds)
            {
                this[kind] = true;
            }
        }

        public bool this[AnimalKind kind]
        {
            get
            {
                return (storage & (1 << ((int)kind + 2))) != 0;
            }
            set
            {
                if (value)
                {
                    storage |= (1 << ((int)kind + 2));
                }
                else
                {
                    storage &= ~(1 << ((int)kind + 2));
                }
            }
        }

        static string[] indicators;
        static FoodSources()
        {
            indicators = new string[2 + Enum.GetNames(typeof(AnimalKind)).Length];
            indicators[0] = "☀";
            indicators[1] = "🌱";
            int i = 0;
            foreach (var kind in Enum.GetNames(typeof(AnimalKind)))
            {
                indicators[2 + (i++)] = "" + kind[0];
            }
        }
        public override string ToString()
        {
            string r = "";
            var foods = storage;
            int index = 0;
            while (foods != 0)
            {
                r += ((foods & 1) == 0) ? " " : indicators[index];
                index++;
                foods >>= 1;
            }
            return r;
        }
    }
}
