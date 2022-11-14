using System;

namespace HLE.Maths;

/// <summary>
/// A class to do calculations with a square.
/// </summary>
public readonly struct Square : IShape
{
    /// <summary>
    /// The side length of the square.
    /// </summary>
    public double SideLength { get; }

    /// <summary>
    /// The area of the square.
    /// </summary>
    public double Area { get; }

    /// <summary>
    /// The circumference of the square.
    /// </summary>
    public double Circumference { get; }

    /// <summary>
    /// The length of the diagonal line of the square.
    /// </summary>
    public double Diagonal { get; }

    /// <summary>
    /// The default constructor of <see cref="Square"/>.
    /// </summary>
    /// <param name="sideLength">The side length.</param>
    public Square(double sideLength)
    {
        SideLength = sideLength;
        Area = SideLength * SideLength;
        Circumference = 4 * SideLength;
        Diagonal = Math.Sqrt(2 * Math.Pow(SideLength, 2));
    }
}
