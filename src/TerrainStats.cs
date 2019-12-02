using System;

namespace SimEarth2020
{
    public class TerrainStats
    { 
        public int GrowthPerTurn { get; set; }
        public TerrainKind Kind { get; set; }
        public int MaxFood { get; set; }
        public double Albedo { get; set; }
        public double Emissivity { get; set; }

        public Temperature GetTemperature(Angle latitude)
        {
            // Intensity radiated = Intensity absorbed
            // Iabs = (1-α) . L / 4piD^2 . cos(Latitude)
            // Irad = ε . σ . T^4
            const double StefanBoltzmannSigma = 5.670374e-8;
            const double S = World.SolarLuminosity / (World.RotationalFactor * Math.PI * World.DistanceToTheSun * World.DistanceToTheSun);
            double AverageAlbedo = 0.3;
            double Trad = Math.Pow(
                (1 - AverageAlbedo) * S / 4.0  * Math.Cos(latitude.Radians) 
                / Emissivity / StefanBoltzmannSigma, 
                1 / 4.0);
            // The factor of 2^(1/4) below comes from 
            // https://en.wikipedia.org/wiki/Idealized_greenhouse_model
            return Temperature.FromKelvin(Trad * Math.Pow(2, 1/4.0));
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
                double alb = 0;
                double em = 1;
                switch (kind)
                {
                    case TerrainKind.Forest:
                        gpt = 30;
                        mf = 1000;
                        alb = .15;
                        em = .98;
                        break;
                    case TerrainKind.Grass:
                        gpt = 25;
                        mf = 2000;
                        alb = .25;
                        em = .98;
                        break;
                    case TerrainKind.Jungle:
                        gpt = 35;
                        mf = 1800;
                        alb = (.15 + .18) / 2;
                        em = .98;
                        break;
                    case TerrainKind.Ocean:
                        gpt = 20;
                        mf = 4000;
                        alb = .06;
                        em = .95;
                        break;
                    case TerrainKind.Swamp:
                        gpt = 40;
                        mf = 800;
                        alb = .17;
                        em = .97;
                        break;
                    case TerrainKind.Taiga:
                        gpt = 20;
                        mf = 800;
                        alb = .6;
                        em = .98;
                        break;
                    case TerrainKind.Tundra:
                        gpt = 20;
                        mf = 300;
                        alb = .8;
                        em = .98;
                        break;
                    case TerrainKind.Desert:
                        gpt = 0;
                        mf = 50;
                        alb = .4;
                        em = .65;
                        break;
                    case TerrainKind.Rock:
                        gpt = 1;
                        mf = 150;
                        alb = .3;
                        em = .98;
                        break;
                }
                stats[(int)kind] = new TerrainStats()
                {
                    Kind = kind,
                    MaxFood = mf,
                    GrowthPerTurn = gpt,
                    Albedo = alb,
                    Emissivity = em
                };
            }
            return stats[(int)kind];
        }
    }
}
