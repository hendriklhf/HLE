using System.Reflection;
using HLE.Text;
using Xunit;

namespace HLE.UnitTests.Text;

public sealed class EmojiFileTest
{
    [Fact]
    public void HasFieldsAndAllFieldHaveAValidValue()
    {
        FieldInfo[] fields = typeof(Emoji).GetFields(BindingFlags.Public | BindingFlags.Static);
        Assert.NotEmpty(fields);
        foreach (FieldInfo field in fields)
        {
            object? value = field.GetValue(null);
            Assert.True(value is string { Length: not 0 });
        }
    }
}
