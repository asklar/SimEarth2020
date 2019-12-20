using System;

namespace Environment
{
    public sealed class TerrainStats
    {
        public int GrowthPerTurn { get; set; }
        public TerrainKind Kind { get; set; }
        public int MaxFood { get; set; }
        public float Albedo { get; set; }
        public float Emissivity { get; set; }

        public double Humidity { get; set; }
        public Temperature GetTemperature(float CosLatitude)
        {
            // Intensity radiated = Intensity absorbed
            // Iabs = (1-α) . L / 4piD^2 . cos(Latitude)
            // Irad = ε . σ . T^4
            const float StefanBoltzmannSigma = 5.670374e-8f;
            const float S = World.SolarLuminosity / (World.RotationalFactor * MathF.PI * World.DistanceToTheSun * World.DistanceToTheSun);
            float AverageAlbedo = 0.3f;
            /// TODO: We should use the terrain's Albedo instead of average, but the resulting temps are unrealistic
            var Trad4 = (1 - AverageAlbedo) * S / 4.0f * CosLatitude
                / Emissivity / StefanBoltzmannSigma;
            var Trad = MathF.Sqrt(MathF.Sqrt(Trad4));
            // The factor of 2^(1/4) below comes from 
            // https://en.wikipedia.org/wiki/Idealized_greenhouse_model
            const float FourthRootOf2 = 1.1892071150027210667174999705605f;
            return Temperature.FromKelvin(Trad * FourthRootOf2);
        }
        private static TerrainStats[] stats;
        static TerrainStats()
        {
            stats = new TerrainStats[Enum.GetValues(typeof(TerrainKind)).Length];
        }
        public static TerrainStats Get(TerrainKind kind)
        {
            if (stats[(int)kind] == null)
            {
                int gpt = 0;
                int mf = 0;
                float alb = 0f;
                float em = 1f;
                float h = .56f;
                switch (kind)
                {
                    case TerrainKind.Forest:
                        gpt = 30;
                        mf = 1000;
                        alb = .15f;
                        em = .98f;
                        h = .6f;
                        break;
                    case TerrainKind.Grass:
                        gpt = 25;
                        mf = 2000;
                        alb = .25f;
                        em = .98f;
                        break;
                    case TerrainKind.Jungle:
                        gpt = 35;
                        mf = 1800;
                        alb = (.15f + .18f) / 2;
                        em = .98f;
                        h = .97f;
                        break;
                    case TerrainKind.Ocean:
                        gpt = 20;
                        mf = 4000;
                        alb = .06f;
                        em = .95f;
                        h = 1f;
                        break;
                    case TerrainKind.Swamp:
                        gpt = 40;
                        mf = 800;
                        alb = .17f;
                        em = .97f;
                        break;
                    case TerrainKind.Taiga:
                        gpt = 20;
                        mf = 800;
                        alb = .6f;
                        em = .98f;
                        break;
                    case TerrainKind.Tundra:
                        gpt = 20;
                        mf = 300;
                        alb = .8f;
                        em = .98f;
                        h = .15f;
                        break;
                    case TerrainKind.Desert:
                        gpt = 0;
                        mf = 50;
                        alb = .4f;
                        em = .65f;
                        h = .05f;
                        break;
                    case TerrainKind.Rock:
                        gpt = 1;
                        mf = 150;
                        alb = .3f;
                        em = .98f;
                        h = .1f;
                        break;
                }
                stats[(int)kind] = new TerrainStats()
                {
                    Kind = kind,
                    MaxFood = mf,
                    GrowthPerTurn = gpt,
                    Albedo = alb,
                    Emissivity = em,
                    Humidity = h,
                };
            }
            return stats[(int)kind];
        }
    }
}
