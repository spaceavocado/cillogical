﻿using Cillogical.Kernel.Operand;
using Cillogical.Kernel;
using System.Text.RegularExpressions;

namespace Cillogical.UnitTests.Kernel.Operand;

public class SimplifyOptions : ISimplifyOptions
{
    public string[]? IgnoredPaths { get; }

    public Regex[]? IgnoredPathsRx { get; }

    public SimplifyOptions(string[]? ignoredPaths, Regex[]? ignoredPathsRx)
    {
        IgnoredPaths = ignoredPaths;
        IgnoredPathsRx = ignoredPathsRx;
    }
}

public class ReferenceTest
{
    private Dictionary<string, object> EXAMPLE_CONTEXT() {
        return ContextUtils.FlattenContext(
            new Dictionary<string, object> {
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
            }
        );
    }

    [Theory]
    [InlineData("", null)]
    [InlineData("ref", null)]
    [InlineData("$ref", "ref")]
    public void DefaultSerializeOptionsFrom(string input, object expected)
    {
        Assert.Equal(expected, new DefaultSerializeOptions().From(input));
    }

    [Theory]
    [InlineData("ref", "$ref")]
    public void DefaultSerializeOptionsTo(string input, object expected)
    {
        Assert.Equal(expected, new DefaultSerializeOptions().To(input));
    }

    [Theory]
    [InlineData("path", null, null, false)]
    [InlineData("ignored", new string[]{ "bogus", "ignored" }, null, true)]
    [InlineData("not", new string[] { "ignored" }, null, false)]
    [InlineData("refC", null, new string[] { @"^ref" }, true)]
    public void IsIgnoredPath(string path, string[]? ignoredPaths, string[]? ignoredPathsRx, object expected)
    {
        Assert.Equal(
            expected,
            Reference.IsIgnoredPath(
                path,
                ignoredPaths,
                ignoredPathsRx?.Select((patern) => new Regex(patern)).ToArray()
            )
        );
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
        Assert.Equal(expected,Reference.GetDataType(input));
    }

    [Theory]
    [InlineData("ref", "ref")]
    [InlineData("ref.(X)", "ref.(X)")]
    [InlineData("ref.(String)", "ref")]
    public void TrimDataType(string input, string expected)
    {
        Assert.Equal(expected, Reference.TrimDataType(input));
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
        Assert.Equal(expectedPath, resolvedPath);
        Assert.Equal(expectedValue, value);
    }

    [Theory]
    [InlineData("refA", DataType.Integer, 1)]
    [InlineData("refA", DataType.String, "1")]
    [InlineData("refG", DataType.Number, 1)]
    [InlineData("refH", DataType.Float, 1.1f)]
    [InlineData("refB.refB3", DataType.String, "true")]
    [InlineData("refB.refB3", DataType.Boolean, true)]
    [InlineData("refB.refB3", DataType.Number, 1)]
    [InlineData("refJ", DataType.Undefined, null)]
    public void Evaluate(string path, DataType dataType, object expected)
    {
        var (_, value) = Reference.Evaluate(EXAMPLE_CONTEXT(), path, dataType);
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("refA", 1)]
    [InlineData("refB.refB3", true)]
    [InlineData("refE[1][2]", 4)]
    [InlineData("refJ", null)]
    public void EvaluateOperand(string path, object expected)
    {
        var value = new Reference(path).Evaluate(EXAMPLE_CONTEXT());
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("ref", "$ref")]
    [InlineData("ref.(Number)", "$ref.(Number)")]
    public void Serialize(string input, string expected)
    {
        Assert.Equal(expected, new Reference(input).Serialize());
    }

    public static IEnumerable<object?[]> SimplifyTestData()
    {
        yield return new object[] { "refJ", new Reference("refJ") };
        yield return new object?[] { "ignored", null };
        yield return new object?[] { "refA", 1 };
        yield return new object[] { "refB.{refJ}", new Reference("refB.{refJ}") };
        yield return new object?[] { "refC.{refJ}", null };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(string address, object? expected)
    {
        var simplifyOptions = new SimplifyOptions(new string[] { "ignored" }, new Regex[] { new Regex(@"^refC") });
        var operand = new Reference(address, simplifyOptions: simplifyOptions);
        var simplified = operand.Simplify(EXAMPLE_CONTEXT());

        if (simplified is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(simplified, expected);
        }
    }

    [Theory]
    [InlineData("ref", "{ref}")]
    [InlineData("ref.(Number)", "{ref.(Number)}")]
    public void Stringify(string input, string expected)
    {
        Assert.Equal(expected, new Reference(input).ToString());
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
        Assert.Equal(expected, Reference.ToNumber(input));
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
        Assert.Equal(expected, Reference.ToInteger(input));
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
        Assert.Equal(expected, Reference.ToFloat(input));
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
        Assert.Equal(expected, Reference.ToString(input));
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
        Assert.Equal(expected, Reference.ToBoolean(input));
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