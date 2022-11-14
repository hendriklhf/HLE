using System;

namespace HLE.Maths;

/// <summary>
/// A class to do calculations with a rectangle.
/// </summary>
public readonly struct Rectangle : IShape
{
    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// The length of the diagonal line of the rectangle.
    /// </summary>
    public double Diagonals { get; }

    /// <summary>
    /// The area of the rectangle.
    /// </summary>
    public double Area { get; }

    /// <summary>
    /// The circumference of the rectangle.
    /// </summary>
    public double Circumference { get; }

    /// <summary>
    /// The default constructor of <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
        Diagonals = Math.Sqrt(Math.Pow(Width, 2) + Math.Pow(Height, 2));
        Area = Width * Height;
        Circumference = 2 * (Width + Height);
    }
}
