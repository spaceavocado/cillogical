using Xunit;
using Cillogical.Kernel.Operand;

namespace Cillogical.UnitTests.Kernel.Operand;

public class ValueTest
{
    [Theory]
    [InlineData(new int[] { 1 })]
    public void InvalidType(object input)
    {
        Assert.Throws<ArgumentException>(() => new Value(input));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.1d)]
    [InlineData(1.1f)]
    [InlineData("val")]
    [InlineData('c')]
    [InlineData(true)]
    [InlineData(false)]
    public void Evaluate(object input)
    {
        var value = new Value(input);
        Assert.Equal(input, value.Evaluate());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.1d)]
    [InlineData(1.1f)]
    [InlineData("val")]
    [InlineData('c')]
    [InlineData(true)]
    [InlineData(false)]
    public void Serialize(object input)
    {
        var value = new Value(input);
        Assert.Equal(input, value.Serialize());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.1d)]
    [InlineData(1.1f)]
    [InlineData("val")]
    [InlineData('c')]
    [InlineData(true)]
    [InlineData(false)]
    public void Simplify(object input)
    {
        var value = new Value(input).Simplify();
        Assert.Equal(input, value);
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(1.1d, "1.1")]
    [InlineData(1.1f, "1.1")]
    [InlineData("val", "\"val\"")]
    [InlineData('c', "\"c\"")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void toString(object input, string expected)
    {
        var value = new Value(input);
        Assert.Equal(expected, value.ToString());
    }
}