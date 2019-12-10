using System;

namespace Environment
{
    public struct Temperature : IEquatable<Temperature>
    {
        public double Kelvin { get; set; }
        public double Celsius { get => Kelvin - 273.15; }
        public double Fahrenheit { get => Celsius * 1.8 + 32; }

        public static Temperature FromKelvin(double k)
        {
            return new Temperature() { Kelvin = k };
        }

        public override bool Equals(object obj)
        {
            return obj is Temperature && Equals((Temperature)obj);
        }

        public override int GetHashCode()
        {
            return Kelvin.GetHashCode();
        }

        public bool Equals(Temperature other)
        {
            const double epsilon = 0.1;
            return other != null &&
                Math.Abs(Kelvin - other.Kelvin) < epsilon;
        }

        public static bool operator ==(Temperature left, Temperature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Temperature left, Temperature right)
        {
            return !(left == right);
        }
    }
}
