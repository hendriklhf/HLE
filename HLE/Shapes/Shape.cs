namespace HLE.Shapes
{
    /// <summary>
    /// An abstract class for shape objects.
    /// </summary>
    public abstract class Shape
    {
        /// <summary>
        /// An abstract property for the area of a shape.
        /// </summary>
        public abstract double Area { get; }

        /// <summary>
        /// An abstract property for the circumference of a shape.
        /// </summary>
        public abstract double Circumference { get; }
    }
}
