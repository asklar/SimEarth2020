namespace SimEarth2020
{
    public class TerrainStats
    { 
        public int GrowthPerTurn { get; set; }
        public TerrainKind Kind { get; set; }
        public int MaxFood { get; set; }

        public static TerrainStats Get(TerrainKind kind)
        {
            int gpt = 0;
            int mf = 0;
            switch (kind)
            {
                case TerrainKind.Forest:
                    gpt = 30;
                    mf = 1000;
                    break;
                case TerrainKind.Grass:
                    gpt = 25;
                    mf = 2000;
                    break;
                case TerrainKind.Jungle:
                    gpt = 35;
                    mf = 1800;
                    break;
                case TerrainKind.Ocean:
                    gpt = 20;
                    mf = 4000;
                    break;
                case TerrainKind.Swamp:
                    gpt = 40;
                    mf = 800;
                    break;
                case TerrainKind.Taiga:
                    gpt = 20;
                    mf = 800;
                    break;
                case TerrainKind.Tundra:
                    gpt = 20;
                    mf = 300;
                    break;
            }
            return new TerrainStats()
            {
                Kind = kind,
                MaxFood = mf,
                GrowthPerTurn = gpt
            };
        }
    }
}
