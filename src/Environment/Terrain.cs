using System;

namespace Environment
{
    public sealed class Terrain
    {
        private static Random rand = new Random();
        public TerrainStats Stats { get; set; }

        public double Elevation { get; set; }
        public TerrainKind Kind { get => Stats.Kind; }
        public int RemainingFood { get; set; }

        public Terrain(TerrainKind kind)
        {
            Stats = TerrainStats.Get(kind);
            RemainingFood = (int)(Stats.MaxFood * 0.5);
        }

        private static double GetMaxSaturationPressure(Temperature T)
        {
            // https://www.engineeringtoolbox.com/water-vapor-saturation-pressure-air-d_689.html
            return Math.Exp(77.345 + .0057 * T.Kelvin - 7235 / T.Kelvin) / Math.Pow(T.Kelvin, 8.2);
            // Could also use the https://en.wikipedia.org/wiki/Clausius%E2%80%93Clapeyron_relation#Meteorology_and_climatology
            // return 6.1094 * Math.Exp((17.625 * T.Celsius) / (T.Celsius + 243.04));
        }

        private double GetVaporPressure(float CosLatitude)
        {
            return Stats.Humidity * GetMaxSaturationPressure(Stats.GetTemperature(CosLatitude));
        }

        private double GetEvaporationRate(float CosLatitude)
        {
            Temperature T = Stats.GetTemperature(CosLatitude);
            // https://en.wikipedia.org/wiki/Penman_equation
            double m_mmHg = 5336 / T.Kelvin.Squared() * Math.Exp(21.07 - 5336 / T.Kelvin);
            double m_kPa = m_mmHg / 7.50062;

            double Rn = 4;
            double pressure_kPa = 100;
            double pressure = pressure_kPa * 1000;
            double lambda_v = 2457; // Enthalpy of vaporization of water (kJ/kg) around 18 Celsius
            // See https://www.engineeringtoolbox.com/water-properties-d_1573.html for more values
            double gamma = 0.0016286 * pressure_kPa / lambda_v;
            double cp = 1.0035; // Heat capacity of air [kJ/kg]
            double M = 28.96e-3; // molar mass of air [kg/mol]
            double R = 8.314; // Ideal gas constant [J/mol/K]
            double densityAir = pressure * M / (R * T.Kelvin);
            // [densityAir] = Pa . kg . mol^-1 . J^-1 . mol . K . K^-1 = kg Pa / J = kg Nm^-2 N^-1 m^-1 = kg . m^-3

            double delta_e = (1 - Stats.Humidity) * GetMaxSaturationPressure(T);


            double U2 = .5; // Wind Speed [m s^-1]
            double E_mm_per_day = (m_kPa * Rn + gamma * 6.43 * (1 + .536 * U2) * delta_e) / (lambda_v * (m_kPa + gamma));
            return E_mm_per_day;
        }

        public void Tick(float CosLatitude)
        {
            int growth = (int)(Stats.GrowthPerTurn * CosLatitude);
            RemainingFood = Math.Min(RemainingFood + growth, Stats.MaxFood);
            double temperature = Stats.GetTemperature(CosLatitude).Celsius;
            // TODO: State machine for terrain to become a different terrain 
            // based on temperature, proximity to water, etc.
            if (temperature <= 0 &&
                Kind != TerrainKind.Tundra &&
                Kind != TerrainKind.Ocean)
            {
                if (rand.NextDouble() < .1) // slowly transition
                {
                    Become(TerrainKind.Tundra);
                }
            }
            if (temperature >= 40)
            {
                if (rand.NextDouble() < .8)
                {
                    if (Kind == TerrainKind.Ocean)
                    {
                        Elevation += temperature;
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
            // Debug.WriteLine($"{Kind} has become {kind}");
            Stats = TerrainStats.Get(kind);
        }
    }
}
