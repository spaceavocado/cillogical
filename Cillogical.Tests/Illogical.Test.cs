using Cillogical.Kernel;
using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using System.Text.RegularExpressions;

namespace Cillogical.UnitTests;


class MockSerializeOptions : ISerializeOptions
{
    public string? From(string operand) =>
        operand.Length > 2 && operand.StartsWith("__")
            ? operand.Substring(2)
            : null;

    public string To(string operand) => $"__{operand}";
}

public class MockSimplifyOptions : ISimplifyOptions
{
    public string[]? IgnoredPaths { get; }

    public Regex[]? IgnoredPathsRx { get; }

    public MockSimplifyOptions(string[]? ignoredPaths, Regex[]? ignoredPathsRx)
    {
        IgnoredPaths = ignoredPaths;
        IgnoredPathsRx = ignoredPathsRx;
    }
}

public class IllogicalTest
{
    private static ISerializeOptions serializeOptions = new DefaultSerializeOptions();
    private static string MockAddress(string value) => serializeOptions.To(value);

    public static IEnumerable<object[]> ParseTestData()
    {
        yield return new object[] { 1, new Value(1) };
        yield return new object[] { MockAddress("path"), new Reference("path") };
        yield return new object[] { new object[] { 1 }, new Collection(new IEvaluable[] { new Value(1) } ) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.EQ], 1, 1 }, new Eq(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND], true, true }, new And(new IEvaluable[] { new Value(true), new Value(true) }) };
    }

    [Theory]
    [MemberData(nameof(ParseTestData))]
    public void Parse(object input, IEvaluable expected)
    {
        var illogical = new Illogical();
        var evaluable = illogical.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    public static IEnumerable<object[]> EvaluateTestData()
    {
        yield return new object[] { 1, 1 };
        yield return new object[] { MockAddress("path"), "value" };
        yield return new object[] { new object[] { 1 }, new object[] { 1 } };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.EQ], 1, 1 }, true };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND], true, false }, false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(object input, object expected)
    {
        var illogical = new Illogical();
        var evaluated = illogical.Evaluate(input, new Dictionary<string, object?> { { "path", "value" } });

        Assert.Equal(expected, evaluated);
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return new object[] { 1, 1 };
        yield return new object[] { MockAddress("path"), "value" };
        yield return new object[] { MockAddress("nested.inner"), 2 };
        yield return new object[] { MockAddress("list[1]"), 3 };
        yield return new object[] { MockAddress("missing"), new Reference("missing") };
        yield return new object[] { new object[] { 1 }, new object[] { 1 } };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.EQ], 1, 1 }, true };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND], true, true }, true };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND], true, MockAddress("missing") }, new Reference("missing") };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(object input, object expected)
    {
        var illogical = new Illogical();
        var simplified = illogical.Simplify(input, new Dictionary<string, object?>
        {
            { "path", "value" },
            { "nested", new Dictionary<string, object?> { { "inner", 2 } } },
            { "list", new object[] { 1, 3 } }
        });

        if (simplified is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
    }

    public static IEnumerable<object[]> StatementTestData()
    {
        yield return new object[] { 1, "1" };
        yield return new object[] { true, "true" };
        yield return new object[] { "val", "\"val\"" };
        yield return new object[] { "$refA", "{refA}" };
        yield return new object[] { new object[] { "==", "$refA", "resolvedA" }, "({refA} == \"resolvedA\")" };
        yield return new object[] { new object[] { "AND", new object[] { "==", 1, 1 }, new object[] { "!=", 2, 1 } }, "((1 == 1) AND (2 != 1))" };
    }

    [Theory]
    [MemberData(nameof(StatementTestData))]
    public void Statement(object input, object expected)
    {
        var illogical = new Illogical();
        var statement = illogical.Statement(input);

        Assert.Equal(expected, statement);
    }

    public static IEnumerable<object[]> OperatorMappingTestData()
    {
        yield return new object[] { new object[] { "IS", 1, 1 }, true };
        yield return new object[] { new object[] { "IS", 1, 2 }, false };
    }

    [Theory]
    [MemberData(nameof(OperatorMappingTestData))]
    public void OperatorMapping(object input, object expected)
    {
        var operatorMapping = new Dictionary<Operator, string>(Parser.DEFAULT_OPERATOR_MAPPING);
        operatorMapping[Operator.EQ] = "IS";

        var illogical = new Illogical(operatorMapping: operatorMapping);
        var evaluated = illogical.Evaluate(input, null);

        Assert.Equal(expected, evaluated);
    }

    public static IEnumerable<object[]> SerializeOptionsTestData()
    {
        yield return new object[] { "__ref", new Reference("ref") };
        yield return new object[] { "$ref", new Value("$ref") };
    }

    [Theory]
    [MemberData(nameof(SerializeOptionsTestData))]
    public void SerializeOptions(object input, object expected)
    {
        var illogical = new Illogical(serializeOptions: new MockSerializeOptions());
        var parsed = illogical.Parse(input);
        var serialized = parsed.Serialize();

        Assert.Equal($"{expected}", $"{parsed}");
        Assert.Equal(input, serialized);
    }

    public static IEnumerable<object?[]> SimplifyOptionsTestData()
    {
        yield return new object[] { "$refA", 1 };
        yield return new object[] { "$refB", new Reference("refB") };
        yield return new object?[] { "$ignored", new Reference("ignored") };
    }

    [Theory]
    [MemberData(nameof(SimplifyOptionsTestData))]
    public void SimplifyOptions(object input, object? expected)
    {
        var illogical = new Illogical(simplifyOptions: new MockSimplifyOptions(
            new string[] { "ignored" }, new Regex[] { new Regex(@"^refB") }
        ));
        var simplified = illogical.Simplify(input, new Dictionary<string, object?> { { "refA", 1 }, { "refB", 2 }, { "ignored", 3 } });

        if (simplified is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
    }

    public static IEnumerable<object[]> EscapeCharacterTestData()
    {
        yield return new object[] { new object[] { "*AND", 1, 1 }, new Collection(new IEvaluable[] { new Value("AND"), new Value(1), new Value(1) }) };
    }

    [Theory]
    [MemberData(nameof(EscapeCharacterTestData))]
    public void EscapeCharacter(object input, object expected)
    {
        var illogical = new Illogical(escapeCharacter: '*');
        var parsed = illogical.Parse(input);

        Assert.Equal($"{expected}", $"{parsed}");
    }
}