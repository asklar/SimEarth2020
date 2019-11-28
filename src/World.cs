using System;

namespace SimEarth2020
{
    public partial class MainWindow
    {
        public class World
        {
            public World(MainWindow m)
            {
                Controller = m;
            }
            private double age;
            private int energy;

            private MainWindow Controller { get; set; }
            public string Name { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public double Age
            {
                get => age;
                set
                {
                    age = value; Controller.RaisePropertyChanged("TitleString");
                }
            }
            public double Radius { get; set; }
            private string GetAge()
            {
                string[] prefixes = new string[] { "", "k", "M", "G", "T" };
                int index = 0;
                double age = Age;
                while (age >= 1000 && index < prefixes.Length - 1)
                {
                    index++;
                    age /= 1000;
                }
                return $"{age} {prefixes[index]}";
            }
            public override string ToString()
            {
                return $"{Name}: {GetAge()}yr";
            }

            internal void Tick()
            {
                Age += Controller.Speed;
                Energy = Math.Min(MaxEnergy, Energy + GetProducedEnergy());
            }

            public int GetProducedEnergy()
            {
                // TODO: Calculate produced energy
                return 5;
            }

            public const int MaxEnergy = 5000;
            public int Energy
            {
                get => energy; 
                set { energy = value; Controller.RaisePropertyChanged("Energy"); }
            }
        }
    }
}
