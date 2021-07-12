using System;

namespace HLE.Shapes
{
    public class Circle : Shape
    {
        public double Radius { get; set; }

        public double Diameter => 2 * Radius;

        public override double Area => Math.PI * Math.Pow(Radius, 2);

        public override double Circumference => Diameter * Math.PI;

        public Circle(double radius)
        {
            Radius = radius;
        }

        public static bool operator ==(Circle left, Circle right)
        {
            return left.Radius == right.Radius;
        }

        public static bool operator !=(Circle left, Circle right)
        {
            return !(left == right);
        }
    }
}
