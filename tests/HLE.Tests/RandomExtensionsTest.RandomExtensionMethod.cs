using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace HLE.Tests;

public sealed partial class RandomExtensionsTest
{
    private sealed class RandomExtensionMethod(MethodInfo methodInfo) : IEquatable<RandomExtensionMethod>
    {
        private readonly string _name = methodInfo.Name;
        private readonly Type _returnType = methodInfo.ReturnType;
        private readonly Type[] _parameterTypes = methodInfo.GetParameters().Skip(1).Select(static p => p.ParameterType).ToArray();
        private readonly Type[] _genericParameters = methodInfo.GetGenericArguments();

        [Pure]
        public override string ToString()
        {
            string genericParameters = _genericParameters.Length == 0 ? string.Empty : $"<{string.Join(", ", _genericParameters.Select(static g => g.ToString()))}>";
            string parameters = _parameterTypes.Length == 0 ? string.Empty : string.Join(", ", _parameterTypes.Select(static p => p.ToString()));
            return $"{_returnType} {_name}{genericParameters}({parameters})";
        }

        [Pure]
        public bool Equals(RandomExtensionMethod? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _returnType == other._returnType && _parameterTypes.AsSpan().SequenceEqual(other._parameterTypes) &&
                   _genericParameters.AsSpan().SequenceEqual(other._genericParameters);
        }

        [Pure]
        public override bool Equals(object? obj) => obj is RandomExtensionMethod other && Equals(other);

        [Pure]
        public override int GetHashCode() => HashCode.Combine(_name, _returnType, _parameterTypes, _genericParameters);

        public static bool operator ==(RandomExtensionMethod? left, RandomExtensionMethod? right) => Equals(left, right);

        public static bool operator !=(RandomExtensionMethod? left, RandomExtensionMethod? right) => !(left == right);
    }
}
