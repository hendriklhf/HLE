using System;

namespace HLE.Maths;

/// <summary>
/// A class to do calculations with a square.
/// </summary>
public class Square : IShape
{
    /// <summary>
    /// The side length of the square.
    /// </summary>
    public double SideLength { get; set; }

    /// <summary>
    /// The area of the square.
    /// </summary>
    public double Area => SideLength * SideLength;

    /// <summary>
    /// The circumference of the square.
    /// </summary>
    public double Circumference => 4 * SideLength;

    /// <summary>
    /// The length of the diagonal line of the square.
    /// </summary>
    public double Diagonal => Math.Sqrt(2 * Math.Pow(SideLength, 2));

    /// <summary>
    /// The default constructor of <see cref="Square"/>.
    /// </summary>
    /// <param name="sideLength">The side length.</param>
    public Square(double sideLength)
    {
        SideLength = sideLength;
    }
}
