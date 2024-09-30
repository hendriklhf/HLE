using System.Diagnostics.CodeAnalysis;

namespace HLE;

internal sealed unsafe partial class UnixEnvironmentVariableProvider
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Minor Code Smell", "S3459:Unassigned members should be removed")]
    [SuppressMessage("Major Code Smell", "S3898:Value types should implement \"IEquatable<T>\"")]
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types")]
    private readonly struct Variable
    {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        public readonly byte* Value;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    }
}
