namespace SimEarth2020
{
    public class AnimalPack
    {
        public AnimalPack(AnimalStats stats)
        {
            Stats = stats;
        }
        public int Food { get; set; }
        public int HP { get; set; }
        public AnimalKind Kind { get => Stats.Kind; }
        public long LastTick { get; set; }
        public int Population { get; set; } = 10;

        public AnimalStats Stats { get; set; }
    }

}
