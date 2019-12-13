namespace Environment
{
    public sealed class AnimalStats
    {
        public AnimalStats()
        {
            FoodSources = new FoodSources();
            AverageLitterSize = 1;
            PregnancyProbability = 0.03;
        }
        public AnimalKind Kind { get; set; }

        internal static AnimalStats Get(AnimalKind kind)
        {
            return new AnimalStats()
            {
                Kind = kind,
                MaxHP = 100,
                FoodPerTurn = 45,
                MaxFood = 100,
                ActionThreshold = 45,
                Speed = 0.4,
                FoodSources = new FoodSources(new AnimalKind[] { AnimalKind.Prokaryote, AnimalKind.Eukaryote }) { Sun = true, Vegetation = true },
                CanSwim = true,
                CanWalk = false
            };
        }

        public bool CanSwim { get; set; }
        public bool CanWalk { get; set; }
        public FoodSources FoodSources { get; set; }
        public int MaxFood { get; set; }
        public double FoodPerTurn { get; set; }
        public int ActionThreshold { get; set; }
        public int MaxHP { get; set; }
        public double Speed { get; set; }
        public int AverageLitterSize { get; set; }
        public double PregnancyProbability { get; set; }
    }

}
