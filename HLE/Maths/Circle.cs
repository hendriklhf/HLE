using System;

namespace HLE.Maths;

/// <summary>
/// A class to do calculations with a circle.
/// </summary>
public readonly struct Circle : IShape
{
    /// <summary>
    /// The radius of the circle.
    /// </summary>
    public double Radius { get; }

    /// <summary>
    /// The diameter of the circle.
    /// </summary>
    public double Diameter { get; }

    /// <summary>
    /// The area of the circle.
    /// </summary>
    public double Area { get; }

    /// <summary>
    /// The circumference of the circle.
    /// </summary>
    public double Circumference { get; }

    /// <summary>
    /// The default constructor of <see cref="Circle"/>.
    /// </summary>
    /// <param name="radius">The radius.</param>
    public Circle(double radius)
    {
        Radius = radius;
        Diameter = 2 * radius;
        Area = Math.PI * Math.Pow(radius, 2);
        Circumference = Diameter * Math.PI;
    }
}
