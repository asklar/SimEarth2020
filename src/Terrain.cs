using System;
using System.Diagnostics;

namespace SimEarth2020
{
    public class Terrain
    {
        private static Random rand = new Random();
        public TerrainStats Stats { get; set; }

        public TerrainKind Kind { get => Stats.Kind; }
        public int RemainingFood { get; set; }

        public Terrain(TerrainKind kind)
        {
            Stats = TerrainStats.Get(kind);
            RemainingFood = (int)(Stats.MaxFood * 0.5);
        }
        internal void Tick(Angle lat)
        {
            int growth = (int)(Stats.GrowthPerTurn * Math.Cos(lat.Radians));
            RemainingFood = Math.Min(RemainingFood + growth, Stats.MaxFood);
            // TODO: State machine for terrain to become a different terrain 
            // based on temperature, proximity to water, etc.
            if (Stats.GetTemperature(lat).Celsius <= 2 &&
                Kind != TerrainKind.Tundra &&
                Kind != TerrainKind.Ocean &&
                Kind != TerrainKind.Rock)
            {
                if (rand.NextDouble() < .3)
                {
                    Become(TerrainKind.Tundra);
                }
            }
            if (Stats.GetTemperature(lat).Celsius >= 40)
            {
                if (rand.NextDouble() < .12)
                {
                    if (Kind == TerrainKind.Ocean)
                    {
                        Become(TerrainKind.Rock);
                    }
                    else if (Kind != TerrainKind.Desert)
                    {
                        Become(TerrainKind.Desert);
                    }
                }
            }
        }

        private void Become(TerrainKind kind)
        {
            Debug.WriteLine($"{Kind} has become {kind}");
            Stats = TerrainStats.Get(kind);
        }
    }
}
