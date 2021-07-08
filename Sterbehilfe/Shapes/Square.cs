using System;

namespace Sterbehilfe.Shapes
{
    public class Square : Shape
    {
        public double SideLength { get; set; }

        public override double Area => SideLength * SideLength;

        public override double Circumference => 4 * SideLength;

        public double Diagonal => Math.Sqrt(2 * Math.Pow(SideLength, 2));

        public Square(double sideLength)
        {
            SideLength = sideLength;
        }
    }
}
