using System;

namespace SimEarth2020
{
    public struct Angle
    {
        public double Radians { get; set; }
        public Angle(double radians)
        {
            Radians = radians;
        }
        public double Degrees { get { return Radians * 180 / Math.PI; } }
    }
}
