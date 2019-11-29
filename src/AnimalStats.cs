namespace SimEarth2020
{
    public class AnimalStats
    {
        public AnimalStats()
        {
            FoodSources = new FoodSource[] { };
            AverageLitterSize = 1;
            PregnancyProbability = 0.3;
        }
        public AnimalKind Kind { get; set; }
        public bool CanSwim { get; set; }
        public bool CanWalk { get; set; }
        public FoodSource[] FoodSources { get; set; }
        public int MaxFood { get; set; }
        public double FoodPerTurn { get; set; }
        public int ActionThreshold { get; set; }
        public int MaxHP { get; set; }
        public double Speed { get; set; }
        public int AverageLitterSize { get; set; }
        public double PregnancyProbability { get; set; }
    }

}
