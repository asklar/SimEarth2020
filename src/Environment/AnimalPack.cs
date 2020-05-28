using Windows.Foundation;

namespace Environment
{
    public sealed class AnimalPack
    {
        public override string ToString()
        {
            return $"{Population} {Kind}";
        }
        public AnimalPack(AnimalKind kind, int packPopulation)
        {
            Stats = AnimalStats.Get(kind);
            TotalHP = Stats.MaxHP * packPopulation;
        }
        public int Food { get; set; }
        public int TotalHP { get; set; }
        public AnimalKind Kind { get => Stats.Kind; }
        public long LastTick { get; set; }
        public int Population { get => TotalHP >= 0 ? TotalHP / Stats.MaxHP : 0; }

        public Point Location;
        public AnimalStats Stats { get; set; }

        internal void MicroMove(double r)
        {
            if ((r * 10) % 10 > 5)
            {
                Location.X -= (Location.X > 0 ? 1 : -1) * (Stats.Speed * (1 + r));
                Location.Y -= (Location.Y > 0 ? 1 : -1) * (Stats.Speed * (1 + r));
            }
            else
            {
                Location.X *= (1 - Stats.Speed);
                Location.Y *= (1 - Stats.Speed);
            }
        }
    }

}
