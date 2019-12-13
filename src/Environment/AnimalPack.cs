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
        public int Population { get => TotalHP / Stats.MaxHP; }

        public AnimalStats Stats { get; set; }
    }

}
