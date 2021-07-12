using System;

namespace HLE.Shapes
{
    public class Rectangle : Shape
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public double Diagonals => Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2));

        public override double Area => Width * Height;

        public override double Circumference => 2 * (Width + Height);

        public Rectangle(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}
