namespace HLE.Maths
{
    /// <summary>
    /// An abstract class for shape objects.
    /// </summary>
    public interface IShape
    {
        /// <summary>
        /// An abstract property for the area of a shape.
        /// </summary>
        public double Area { get; }

        /// <summary>
        /// An abstract property for the circumference of a shape.
        /// </summary>
        public double Circumference { get; }
    }
}
