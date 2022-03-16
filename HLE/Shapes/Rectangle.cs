using System;

namespace HLE.Shapes
{
    /// <summary>
    /// A class to do calculations with a rectangle.
    /// </summary>
    public class Rectangle : Shape
    {
        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The length of the diagonal line of the rectangle.
        /// </summary>
        public double Diagonals => Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2));

        /// <summary>
        /// The area of the rectangle.
        /// </summary>
        public override double Area => Width * Height;

        /// <summary>
        /// The circumference of the rectangle.
        /// </summary>
        public override double Circumference => 2 * (Width + Height);

        /// <summary>
        /// The basic constructor for <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Rectangle(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}
