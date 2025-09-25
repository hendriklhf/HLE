using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using HLE.Collections;
using HLE.Memory;
using HLE.TestUtilities;
using HLE.Text;

namespace HLE.UnitTests.Text;

public sealed partial class LazyStringTest
{
    private const string TestString = StringConstants.AlphaNumerics;

    public static TheoryData<Parameter> NonEmptyStringParameters { get; } =
    [
        new Parameter(TestString, static () => new(TestString)),
        new Parameter(TestString, static () => LazyString.FromString(TestString))
    ];

    public static TheoryData<Parameter> EmptyStringParameters { get; } =
    [
        new Parameter(string.Empty, static () => LazyString.Empty),
        new Parameter(string.Empty, static () => new(string.Empty)),
        new Parameter(string.Empty, static () => LazyString.FromString(string.Empty))
    ];

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void Indexer_Int_RefReadOnlyChar(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        for (int i = 0; i < str.Length; i++)
        {
            ref readonly char c = ref lazy[i];
            Assert.Equal(str[i], c);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IndexableChar_Indexer_Int_Char(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IIndexable<char> indexable = TestHelpers.Cast<LazyString, IIndexable<char>>(lazy);

        for (int i = 0; i < str.Length; i++)
        {
            char c = indexable[i];
            Assert.Equal(str[i], c);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IndexableChar_Indexer_Index_Char(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IIndexable<char> indexable = TestHelpers.Cast<LazyString, IIndexable<char>>(lazy);

        for (int i = 0; i < str.Length; i++)
        {
            char c = indexable[new Index(i)];
            Assert.Equal(str[i], c);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void Indexer_Index_RefReadOnlyChar(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        for (int i = 0; i < str.Length; i++)
        {
            ref readonly char c = ref lazy[new Index(i)];
            Assert.Equal(str[i], c);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void Indexer_Range_ReadOnlySpan_NonEmpty(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        ReadOnlySpan<char> lazySpan = lazy[..];
        ReadOnlySpan<char> strSpan = str.AsSpan(..);

        Assert.True(lazySpan.SequenceEqual(strSpan));

        if (str.Length >= 2)
        {
            lazySpan = lazy[2..];
            strSpan = str.AsSpan(2..);

            Assert.True(lazySpan.SequenceEqual(strSpan));

            lazySpan = lazy[..3];
            strSpan = str.AsSpan(..3);

            Assert.True(lazySpan.SequenceEqual(strSpan));
        }
    }

    [Theory]
    [MemberData(nameof(EmptyStringParameters))]
    public void Indexer_Range_ReadOnlySpan_Empty(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        ReadOnlySpan<char> lazySpan = lazy[..];
        ReadOnlySpan<char> strSpan = str.AsSpan(..);

        Assert.True(lazySpan.SequenceEqual(strSpan));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void Length(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        Assert.Equal(str.Length, lazy.Length);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICollection_Count(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        ICollection<char> collection = TestHelpers.Cast<LazyString, ICollection<char>>(lazy);
        Assert.Equal(str.Length, collection.Count);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICollection_IsReadOnly(Parameter parameter)
    {
        using LazyString lazy = parameter.CreateLazy();
        ICollection<char> collection = TestHelpers.Cast<LazyString, ICollection<char>>(lazy);
        Assert.True(collection.IsReadOnly);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IReadOnlyCollection_Count(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IReadOnlyCollection<char> collection = TestHelpers.Cast<LazyString, IReadOnlyCollection<char>>(lazy);
        Assert.Equal(str.Length, collection.Count);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICountable_Count(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        ICountable countable = TestHelpers.Cast<LazyString, ICountable>(lazy);
        Assert.Equal(str.Length, countable.Count);
    }

    [Fact]
    public void StaticEmptyIsEmpty()
    {
        Assert.Equal(0, LazyString.Empty.Length);
        Assert.Same(string.Empty, LazyString.Empty.ToString());
    }

    [Fact]
    public void Ctor_PooledInterpolatedStringHandler()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        using LazyString lazy = new($"{TestString}{123}");
        Assert.Equal($"{TestString}{123}", lazy.ToString());
    }

    [Fact]
    public void Ctor_ReadOnlySpan()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        using LazyString lazy = new(TestString.AsSpan());
        Assert.Equal(TestString, lazy.ToString());
    }

    [Fact]
    public void Ctor_ArrayLength()
    {
        using LazyString lazy = new(TestString.ToCharArray(), TestString.Length);
        Assert.Equal(TestString, lazy.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(TestString)]
    public void FromString(string str)
    {
        using LazyString lazy = LazyString.FromString(str);
        if (str.Length == 0)
        {
            Assert.Same(LazyString.Empty, lazy);
        }

        Assert.Same(str, lazy.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData(TestString)]
    public void Dispose_IsPossibleMultipleTimes(string str)
    {
        LazyString lazy = LazyString.FromString(str);
        lazy.Dispose();
        lazy.Dispose();
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void AsSpan(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Assert.True(str.AsSpan().SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void AsMemory(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Assert.True(lazy.AsMemory().Span.SequenceEqual(str));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ToArray(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char[] lazyArray = lazy.ToArray();
        Assert.True(lazyArray.AsSpan().SequenceEqual(str));
    }

    [Fact]
    public void ToArray_ReturnsEmptyArray()
        => Assert.Same(Array.Empty<char>(), LazyString.Empty.ToArray());

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToArray_Start(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

#pragma warning disable IDE0057
        char[] lazyArray = lazy.ToArray(2);
#pragma warning restore IDE0057
        Assert.True(lazyArray.AsSpan().SequenceEqual(str.AsSpan(2)));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToArray_StartLength(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char[] lazyArray = lazy.ToArray(2, 3);
        Assert.True(lazyArray.AsSpan().SequenceEqual(str.AsSpan(2, 3)));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToArray_Range(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char[] lazyArray = lazy.ToArray(2..);
        Assert.True(lazyArray.AsSpan().SequenceEqual(str.AsSpan(2..)));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ToList(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        List<char> lazyList = lazy.ToList();
        Assert.True(CollectionsMarshal.AsSpan(lazyList).SequenceEqual(str));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToList_Start(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

#pragma warning disable IDE0057
        List<char> lazyList = lazy.ToList(2);
#pragma warning restore IDE0057
        Assert.True(CollectionsMarshal.AsSpan(lazyList).SequenceEqual(str.AsSpan(2)));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToList_StartLength(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        List<char> lazyList = lazy.ToList(2, 3);
        Assert.True(CollectionsMarshal.AsSpan(lazyList).SequenceEqual(str.AsSpan(2, 3)));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void ToList_Range(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        List<char> lazyList = lazy.ToList(2..);
        Assert.True(CollectionsMarshal.AsSpan(lazyList).SequenceEqual(str.AsSpan(2..)));
    }

    [Fact]
    public void TryGetString_ReturnsFalse()
    {
        using LazyString lazy = new(TestString.AsSpan());
        Assert.False(lazy.TryGetString(out string? str));
        Assert.Null(str);
    }

    [Fact]
    public void TryGetString_ReturnsTrue()
    {
        LazyString lazy = LazyString.FromString(TestString);
        Assert.True(lazy.TryGetString(out string? str));
        Assert.Same(TestString, str);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ToString_Test(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Assert.Equal(str, lazy.ToString());
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void CopyTo_List(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        List<char> result = [];
        lazy.CopyTo(result);

        Assert.True(CollectionsMarshal.AsSpan(result).SequenceEqual(str));
        Assert.True(CollectionsMarshal.AsSpan(result).SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    public void CopyTo_List_Offset(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        List<char> result = [];
        lazy.CopyTo(result, 2);

        Assert.True(CollectionsMarshal.AsSpan(result)[2..].SequenceEqual(str));
        Assert.True(CollectionsMarshal.AsSpan(result)[2..].SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void CopyTo_Array(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char[] result = new char[str.Length];
        lazy.CopyTo(result);

        Assert.True(result.AsSpan().SequenceEqual(str));
        Assert.True(result.AsSpan().SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void CopyTo_Memory(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Memory<char> result = new char[str.Length];
        lazy.CopyTo(result);

        Assert.True(result.Span.SequenceEqual(str));
        Assert.True(result.Span.SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void CopyTo_Span(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Span<char> result = new char[str.Length];
        lazy.CopyTo(result);

        Assert.True(result.SequenceEqual(str));
        Assert.True(result.SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void CopyTo_RefChar(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Span<char> result = new char[str.Length];
        lazy.CopyTo(ref MemoryMarshal.GetReference(result));

        Assert.True(result.SequenceEqual(str));
        Assert.True(result.SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public unsafe void CopyTo_PointerChar(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char* result = stackalloc char[str.Length];
        lazy.CopyTo(result);

        Assert.True(new Span<char>(result, str.Length).SequenceEqual(str));
        Assert.True(new Span<char>(result, str.Length).SequenceEqual(lazy.AsSpan()));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICollection_Add_ThrowsNoSupportedException(Parameter parameter)
    {
        using LazyString lazy = parameter.CreateLazy();
        ICollection<char> collection = TestHelpers.Cast<LazyString, ICollection<char>>(lazy);

        Assert.Throws<NotSupportedException>(() => collection.Add('a'));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICollection_Clear_ThrowsNoSupportedException(Parameter parameter)
    {
        using LazyString lazy = parameter.CreateLazy();
        ICollection<char> collection = TestHelpers.Cast<LazyString, ICollection<char>>(lazy);

        Assert.Throws<NotSupportedException>(collection.Clear);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void ICollection_Remove_ThrowsNoSupportedException(Parameter parameter)
    {
        using LazyString lazy = parameter.CreateLazy();
        ICollection<char> collection = TestHelpers.Cast<LazyString, ICollection<char>>(lazy);

        Assert.Throws<NotSupportedException>(() => collection.Remove('a'));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IndexOf(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        foreach (char c in TestString)
        {
            Assert.Equal(str.IndexOf(c), lazy.IndexOf(c));
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void Contains(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        foreach (char c in TestString)
        {
            Assert.Equal(str.Contains(c), lazy.Contains(c));
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IReadOnlySpanProvider_GetReadOnlySpan(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IReadOnlySpanProvider<char> provider = TestHelpers.Cast<LazyString, IReadOnlySpanProvider<char>>(lazy);

        Assert.True(provider.AsSpan().SequenceEqual(str));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IReadOnlyMemoryProvider_GetReadOnlyMemory(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IReadOnlyMemoryProvider<char> provider = TestHelpers.Cast<LazyString, IReadOnlyMemoryProvider<char>>(lazy);

        Assert.True(provider.AsMemory().Span.SequenceEqual(str));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void GetEnumerator(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        using CharEnumerator stringEnumerator = str.GetEnumerator();
        using ReadOnlyMemoryEnumerator<char> lazyEnumerator = lazy.GetEnumerator();

        while (lazyEnumerator.MoveNext())
        {
            Assert.True(stringEnumerator.MoveNext());
            Assert.Equal(stringEnumerator.Current, lazyEnumerator.Current);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IEnumerableChar_GetEnumerator(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        IEnumerable<char> enumerable = TestHelpers.Cast<LazyString, IEnumerable<char>>(lazy);

        using CharEnumerator stringEnumerator = str.GetEnumerator();
        using IEnumerator<char> lazyEnumerator = enumerable.GetEnumerator();

        while (lazyEnumerator.MoveNext())
        {
            Assert.True(stringEnumerator.MoveNext());
            Assert.Equal(stringEnumerator.Current, lazyEnumerator.Current);
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IEnumerable_GetEnumerator(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        System.Collections.IEnumerable enumerable = TestHelpers.Cast<LazyString, System.Collections.IEnumerable>(lazy);

        using CharEnumerator stringEnumerator = str.GetEnumerator();
        // ReSharper disable once GenericEnumeratorNotDisposed
        System.Collections.IEnumerator lazyEnumerator = enumerable.GetEnumerator();

        while (lazyEnumerator.MoveNext())
        {
            Assert.True(stringEnumerator.MoveNext());
            Assert.Equal(stringEnumerator.Current, lazyEnumerator.Current);
        }
    }

    [Fact]
    public void Equals_LazyString_ReturnsTrue()
    {
        LazyString a = LazyString.FromString(TestString);
        LazyString b = TestHelpers.Return(LazyString.FromString(TestString));
        Assert.True(a.Equals(b));

        b = TestHelpers.Return(a);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_LazyString_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        LazyString b = TestHelpers.Return(LazyString.FromString(string.Empty));
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_LazyString_StringComparision_ReturnsTrue()
    {
        LazyString a = LazyString.FromString(TestString);
        LazyString b = TestHelpers.Return(LazyString.FromString(TestString.ToUpperInvariant()));
        Assert.True(a.Equals(b, StringComparison.OrdinalIgnoreCase));

        b = TestHelpers.Return(a);
        Assert.True(a.Equals(b, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Equals_LazyString_StringComparision_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        LazyString b = TestHelpers.Return(LazyString.FromString(TestString.ToUpperInvariant()));
        Assert.False(a.Equals(b, StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_string_ReturnsTrue()
    {
        LazyString a = LazyString.FromString(TestString);
        string b = TestHelpers.Return(TestString);
        Assert.True(a.Equals(b));

        b = TestHelpers.Return(a.ToString());
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_string_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        string b = TestHelpers.Return(string.Empty);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_string_StringComparision_ReturnsTrue()
    {
        LazyString a = LazyString.FromString(TestString);
        string b = TestHelpers.Return(TestString.ToUpperInvariant());
        Assert.True(a.Equals(b, StringComparison.OrdinalIgnoreCase));

        b = TestHelpers.Return(a.ToString());
        Assert.True(a.Equals(b, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Equals_string_StringComparision_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        string b = TestHelpers.Return(TestString.ToUpperInvariant());
        Assert.False(a.Equals(b, StringComparison.Ordinal));
    }

    [Fact]
    public void Equals_object_LazyString_ReturnsTrue()
    {
        LazyString a = LazyString.FromString(TestString);
        object b = TestHelpers.Cast<LazyString, object>(LazyString.FromString(TestString));
        Assert.True(a.Equals(b));

        b = TestHelpers.Cast<LazyString, object>(a);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_object_LazyString_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        object b = TestHelpers.Cast<LazyString, object>(LazyString.FromString(string.Empty));
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_object_string_ReturnsTrue()
    {
        using LazyString a = LazyString.FromString(TestString);
        object b = TestHelpers.Cast<string, object>(TestString);
        Assert.True(a.Equals(b));

        b = TestHelpers.Cast<LazyString, object>(a);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_object_string_ReturnsFalse()
    {
        LazyString a = LazyString.FromString(TestString);
        object b = TestHelpers.Cast<string, object>(string.Empty);
        Assert.False(a.Equals(b));
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void IFormattable_Format_FormatsCorrectly(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        IFormattable formattable = TestHelpers.Cast<LazyString, IFormattable>(lazy);

        string formattedString = formattable.ToString(null, null);

        Assert.Equal(str, formattedString);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void TryFormat_FormatsCorrectly(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        char[] buffer = ArrayPool<char>.Shared.Rent(lazy.Length);
        bool success = lazy.TryFormat(buffer, out int charsWritten);
        Assert.True(success);
        Assert.Equal(str.Length, charsWritten);
        Assert.Equal(lazy.Length, charsWritten);

        Assert.True(buffer.AsSpan(0, charsWritten).SequenceEqual(str));
        Assert.True(buffer.AsSpan(0, charsWritten).SequenceEqual(lazy.AsSpan()));

        ArrayPool<char>.Shared.Return(buffer);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void GetHashCode_ReturnsSameHashCode(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        Assert.Equal(str.GetHashCode(), lazy.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void GetHashCode_StringComparison_ReturnsSameHashCode(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        ReadOnlySpan<StringComparison> comparisons = EnumValues<StringComparison>.AsSpan();
        foreach (StringComparison comparison in comparisons)
        {
            Assert.Equal(str.GetHashCode(comparison), lazy.GetHashCode(comparison));
        }
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void EqualityOperator_String_ReturnsTrue(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();
        Assert.True(lazy == str);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void EqualityOperator_LazyString_ReturnsTrue(Parameter parameter)
    {
        using LazyString lazy1 = parameter.CreateLazy();
        using LazyString lazy2 = parameter.CreateLazy();
        Assert.True(lazy1 == lazy2);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void JsonSerialization(Parameter parameter)
    {
        string str = parameter.Value;
        using LazyString lazy = parameter.CreateLazy();

        object lazyObject = new
        {
            Value = lazy
        };

        object stringObject = new
        {
            Value = lazy
        };

        string lazyJson = JsonSerializer.Serialize(lazyObject);
        string stringJson = JsonSerializer.Serialize(stringObject);

        string expectedJson = $"{{\"Value\":\"{str}\"}}";

        Assert.Equal(expectedJson, lazyJson);
        Assert.Equal(expectedJson, stringJson);
    }

    [Theory]
    [MemberData(nameof(NonEmptyStringParameters))]
    [MemberData(nameof(EmptyStringParameters))]
    public void JsonDeserialization(Parameter parameter)
    {
        string str = parameter.Value;
        string json = $"{{\"Value\":\"{str}\"}}";

        Wrapper<LazyString>? lazyObject = JsonSerializer.Deserialize<Wrapper<LazyString>>(json);
        Wrapper<string>? stringObject = JsonSerializer.Deserialize<Wrapper<string>>(json);

        Assert.NotNull(lazyObject);
        Assert.NotNull(stringObject);

        Assert.Equal(str, lazyObject.Value.ToString());
        Assert.Equal(str, stringObject.Value);
    }

#pragma warning disable CA1812
    private sealed record Wrapper<T>(T Value);
#pragma warning restore CA1812
}
