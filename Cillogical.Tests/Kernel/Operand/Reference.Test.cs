using Xunit;
using Cillogical.Kernel.Operand;

namespace Cillogical.UnitTests.Kernel.Operand;

public class ReferenceTest
{
    [Theory]
    [InlineData("ref", DataType.Undefined)]
    [InlineData("ref.(X)", DataType.Undefined)]
    [InlineData("ref.(Bogus)", DataType.Unsupported)]
    [InlineData("ref.(String)", DataType.String)]
    [InlineData("ref.(Number)", DataType.Number)]
    [InlineData("ref.(Integer)", DataType.Integer)]
    [InlineData("ref.(Float)", DataType.Float)]
    [InlineData("ref.(Boolean)", DataType.Boolean)]
    public void GetDataType(string input, DataType expected)
    {
        Assert.Equal(Reference.GetDataType(input), expected);
    }

    [Theory]
    [InlineData("ref", "ref")]
    [InlineData("ref.(X)", "ref.(X)")]
    [InlineData("ref.(String)", "ref")]
    public void TrimDataType(string input, string expected)
    {
        Assert.Equal(Reference.TrimDataType(input), expected);
    }

    [Theory]
    [InlineData("ref", "$ref")]
    [InlineData("ref.(Number)", "$ref.(Number)")]
    public void Serialize(string input, string expected)
    {
        Assert.Equal(new Reference(input).Serialize(), expected);
    }

    [Theory]
    [InlineData("ref", "{ref}")]
    [InlineData("ref.(Number)", "{ref.(Number)}")]
    public void stringify(string input, string expected)
    {
        Assert.Equal(new Reference(input).ToString(), expected);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1.1, 1.1)]
    [InlineData(1.1f, 1.1f)]
    [InlineData("1", 1)]
    [InlineData("1.1", 1.1f)]
    [InlineData("1.9", 1.9f)]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ToNumber(object input, object expected)
    {
        Assert.Equal(Reference.ToNumber(input), expected);
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData(new int[] { })]
    public void ToNumberException(object input)
    {
        Assert.Throws<InvalidCastException>(() => Reference.ToNumber(input));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1.1, 1)]
    [InlineData(1.1f, 1)]
    [InlineData("1", 1)]
    [InlineData("1.1", 1)]
    [InlineData("1.9", 1)]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ToInteger(object input, int expected)
    {
        Assert.Equal(Reference.ToInteger(input), expected);
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData(new int[] {})]
    public void ToIntegerException(object input)
    {
        Assert.Throws<InvalidCastException>(() => Reference.ToInteger(input));
    }

    [Theory]
    [InlineData(1, 1.0f)]
    [InlineData(1.1, 1.1f)]
    [InlineData(1.1f, 1.1f)]
    [InlineData("1", 1.0f)]
    [InlineData("1.1", 1.1f)]
    [InlineData("1.9", 1.9f)]
    public void ToFloat(object input, float expected)
    {
        Assert.Equal(Reference.ToFloat(input), expected);
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData(true)]
    public void ToFloatException(object input)
    {
        Assert.Throws<InvalidCastException>(() => Reference.ToFloat(input));
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(1f, "1")]
    [InlineData(1d, "1")]
    [InlineData(1.1, "1.1")]
    [InlineData("1", "1")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void toString(object input, string expected)
    {
        Assert.Equal(Reference.ToString(input), expected);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("True", true)]
    [InlineData("False", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    public void ToBoolean(object input, bool expected)
    {
        Assert.Equal(Reference.ToBoolean(input), expected);
    }

    [Theory]
    [InlineData(1.1f)]
    [InlineData("bogus")]
    [InlineData(3)]
    public void ToBooleanException(object input)
    {
        Assert.Throws<InvalidCastException>(() => Reference.ToBoolean(input));
    }
}