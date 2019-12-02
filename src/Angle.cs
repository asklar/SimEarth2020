using System;

namespace SimEarth2020
{
    public struct Angle : IEquatable<Angle>
    {
        public static Angle operator -(Angle angle)
        {
            return new Angle(-angle.Radians);
        }
        public double Radians { get; set; }
        public Angle(double radians)
        {
            Radians = radians;
        }
        public double Degrees { get { return Radians * 180 / Math.PI; } }

        public override bool Equals(object obj)
        {
            if (obj is Angle)
            {
                return this.Equals((Angle)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Radians.GetHashCode();
        }

        public static bool operator ==(Angle left, Angle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Angle left, Angle right)
        {
            return !(left == right);
        }

        public bool Equals(Angle other)
        {
            return Radians == other.Radians;
        }

        public static Angle FromDegrees(int v)
        {
            return new Angle(v * Math.PI / 180.0);
        }
    }
}
