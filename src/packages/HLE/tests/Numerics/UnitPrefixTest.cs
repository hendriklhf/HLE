using System.Linq;
using System.Reflection;
using HLE.Numerics;

namespace HLE.UnitTests.Numerics;

public sealed class UnitPrefixTest
{
    public static TheoryData<UnitPrefix> UnitPrefixes { get; } =
    [
        .. typeof(UnitPrefix)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(static p => p.PropertyType == typeof(UnitPrefix))
            .Select(static p => (UnitPrefix)p.GetValue(null)!)
    ];

    [Theory]
    [MemberData(nameof(UnitPrefixes))]
    public void UnitPrefix_Values_Test(UnitPrefix unitPrefix)
    {
        Assert.NotNull(unitPrefix.Name);
        Assert.NotNull(unitPrefix.Symbol);
        Assert.NotEqual(0, unitPrefix.Value);
        Assert.NotEqual(0, unitPrefix._dValue);
        Assert.NotEqual(0, unitPrefix._fValue);
    }
}
