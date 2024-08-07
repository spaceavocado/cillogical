using Xunit;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Cillogical.UnitTests.Kernel.Operand;

public class ReferenceTest
{
    private Dictionary<string, object> EXAMPLE_CONTEXT() {
        return new Dictionary<string, object> {
            { "refA", 1 },
            {
                "refB",
                new Dictionary<string, object> {
                    {"refB1", 2 },
                    { "refB2", "refB1" },
                    { "refB3", true },
                }
            },
            {"refC", "refB1"},
            {"refD", "refB2"},
            {"refE", new object[]{1, new object[] { 2, 3, 4 } } },
            {"refF", "A"},
            {"refG", "1"},
            {"refH", "1.1"},
        };
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("ref", null)]
    [InlineData("$ref", "ref")]
    public void DefaultSerializeOptionsFrom(string input, object expected)
    {
        Assert.Equal(new DefaultSerializeOptions().From(input), expected);
    }

    [Theory]
    [InlineData("ref", "$ref")]
    public void DefaultSerializeOptionsTo(string input, object expected)
    {
        Assert.Equal(new DefaultSerializeOptions().To(input), expected);
    }

    [Theory]
    [InlineData("path", null, null, false)]
    [InlineData("ignored", new string[]{ "bogus", "ignored" }, null, true)]
    [InlineData("not", new string[] { "ignored" }, null, false)]
    [InlineData("refC", null, new string[] { @"^ref" }, true)]
    public void IsIgnoredPath(string path, string[]? ignoredPaths, string[]? ignoredPathsRx, object expected)
    {
        Assert.Equal(
            Reference.IsIgnoredPath(
                path,
                ignoredPaths,
                ignoredPathsRx?.Select((patern) => new Regex(patern)).ToArray()
            ),
        expected);
    }

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
    [InlineData("UNDEFINED", "UNDEFINED", null)]
    [InlineData("refA", "refA", 1)]
    [InlineData("refB.refB1", "refB.refB1", 2)]
    [InlineData("refB.{refC}", "refB.refB1", 2)]
    [InlineData("refB.{UNDEFINED}", "refB.{UNDEFINED}", null)]
    [InlineData("refB.{refB.refB2}", "refB.refB1", 2)]
    [InlineData("refB.{refB.{refD}}", "refB.refB1", 2)]
    [InlineData("refE[0]", "refE[0]", 1)]
    [InlineData("refE[2]", "refE[2]", null)]
    [InlineData("refE[1][0]", "refE[1][0]", 2)]
    [InlineData("refE[1][3]", "refE[1][3]", null)]
    [InlineData("refE[{refA}][0]", "refE[1][0]", 2)]
    [InlineData("refE[{refA}][{refB.refB1}]", "refE[1][2]", 4)]
    [InlineData("ref{refF}", "refA", 1)]
    [InlineData("ref{UNDEFINED}", "ref{UNDEFINED}", null)]
    public void ContextLookup(string path, string? expectedPath, object? expectedValue)
    {
        var (resolvedPath, value) = Reference.ContextLookup(EXAMPLE_CONTEXT(), path);
        Assert.Equal(resolvedPath, expectedPath);
        Assert.Equal(value, expectedValue);
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