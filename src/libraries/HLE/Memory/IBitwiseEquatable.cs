using System;
using System.Diagnostics.CodeAnalysis;

namespace HLE.Memory;

[SuppressMessage("Minor Code Smell", "S4023:Interfaces should not be empty", Justification = "marker interface")]
public interface IBitwiseEquatable<T> : IEquatable<T>;
