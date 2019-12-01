using System;

namespace SimEarth2020
{
    public class Terrain
    {
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
        }
    }
}
