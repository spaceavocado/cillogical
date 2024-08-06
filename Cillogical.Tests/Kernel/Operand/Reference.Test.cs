using Xunit;
using Cillogical.Kernel.Operand;

namespace Cillogical.UnitTests.Kernel.Operand;

public class ReferenceTest
{
    [Theory]
    [InlineData("ref", DataType.Undefined)]
    [InlineData("ref.(X)", DataType.Unsupported)]
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
    public void toString(string input, string expected)
    {
        Assert.Equal(new Reference(input).ToString(), expected);
    }
}