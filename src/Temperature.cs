namespace SimEarth2020
{
    public class Temperature
    {
        public double Kelvin { get; set; }
        public double Celsius { get => Kelvin - 273.15; }
        public double Fahrenheit { get => Celsius * 1.8 + 32; }
        private Temperature()
        { }
        public static Temperature FromKelvin(double k)
        {
            return new Temperature() { Kelvin = k };
        }
    }
}
