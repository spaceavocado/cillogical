using Cillogical.Kernel;
using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;

namespace Cillogical.UnitTests.Kernel;


public class ParserTest
{
    private static ISerializeOptions serializeOptions = new DefaultSerializeOptions();
    private static string MockAddress(string value) => serializeOptions.To(value);

    [Theory]
    [InlineData("\\expected", true)]
    [InlineData("unexpected", false)]
    public void IsEspaced(string input, bool expected)
    {
        var parser = new Parser(serializeOptions: new DefaultSerializeOptions());
        Assert.Equal(expected, parser.IsEspaced(input));
    }

    [Theory]
    [InlineData("$expected", "expected")]
    [InlineData(null, null)]
    [InlineData(1, null)]
    public void ToReferenceAddress(object? reference, string? expected)
    {
        var parser = new Parser(serializeOptions: new DefaultSerializeOptions());
        Assert.Equal(expected, parser.ToReferenceAddress(reference));
    }

    public static IEnumerable<object[]> ParseValueTestData()
    {
        yield return new object[] { 1, new Value(1) };
        yield return new object[] { 1.1, new Value(1.1) };
        yield return new object[] { "val", new Value("val") };
        yield return new object[] { true, new Value(true) };
    }

    [Theory]
    [MemberData(nameof(ParseValueTestData))]
    public void ParseValue(object input, IEvaluable expected)
    {
        var parser = new Parser();
        var evaluable = parser.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    public static IEnumerable<object[]> ParseReferenceTestData()
    {
        yield return new object[] { MockAddress("address"), new Reference("address") };
    }

    [Theory]
    [MemberData(nameof(ParseReferenceTestData))]
    public void ParseReference(object input, IEvaluable expected)
    {
        var parser = new Parser();
        var evaluable = parser.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    public static IEnumerable<object[]> ParseCollectionTestData()
    {
        yield return new object[] {
            new object[] { 1 },
            new Collection(new IEvaluable[] { new Value(1) })
        };
        yield return new object[] {
            new object[] { MockAddress("address") },
            new Collection(new IEvaluable[] { new Reference("address") })
        };
        yield return new object[] {
            new object[] { "value", true },
            new Collection(new IEvaluable[] { new Value("value"), new Value(true) })
        };
        yield return new object[] {
            new object[] { 1, "value", true, MockAddress("address") },
            new Collection(new IEvaluable[] { new Value(1), new Value("value"), new Value(true), new Reference("address") })
        };
        yield return new object[] {
            new object[] { $"{Parser.DEFAULT_ESCAPE_CHARACTER}{Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND]}", 1 },
            new Collection(new IEvaluable[] { new Value($"{Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND]}"), new Value(1) })
        };
    }

    [Theory]
    [MemberData(nameof(ParseCollectionTestData))]
    public void ParseCollection(object input, IEvaluable expected)
    {
        var parser = new Parser();
        var evaluable = parser.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    public static IEnumerable<object[]> ParseComparisonTestData()
    {
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.EQ], 1, 1 }, new Eq(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.NE], 1, 1 }, new Ne(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.GT], 1, 1 }, new Gt(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.GE], 1, 1 }, new Ge(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.LT], 1, 1 }, new Lt(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.LE], 1, 1 }, new Le(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.IN], 1, 1 }, new In(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.NOTIN], 1, 1 }, new NotIn(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.NONE], 1 }, new Null(new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.PRESENT], 1 }, new Present(new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.PREFIX], 1, 1 }, new Prefix(new Value(1), new Value(1)) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.SUFFIX], 1, 1 }, new Suffix(new Value(1), new Value(1)) };
    }

    [Theory]
    [MemberData(nameof(ParseComparisonTestData))]
    public void ParseComparison(object input, IEvaluable expected)
    {
        var parser = new Parser();
        var evaluable = parser.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    public static IEnumerable<object[]> ParseLogicalTestData()
    {
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.AND], true, true }, new And(new IEvaluable[] { new Value(true), new Value(true) }) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.OR], true, true, false }, new Or(new IEvaluable[] { new Value(true), new Value(true), new Value(false) }) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.NOR], true, true }, new Nor(new IEvaluable[] { new Value(true), new Value(true) }) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.XOR], true, true }, new Xor(new IEvaluable[] { new Value(true), new Value(true) }) };
        yield return new object[] { new object[] { Parser.DEFAULT_OPERATOR_MAPPING[Operator.NOT], true }, new Not(new Value(true)) };
    }

    [Theory]
    [MemberData(nameof(ParseComparisonTestData))]
    public void ParseLogical(object input, IEvaluable expected)
    {
        var parser = new Parser();
        var evaluable = parser.Parse(input);

        Assert.IsAssignableFrom<IEvaluable>(evaluable);
        Assert.Equal($"{expected}", $"{evaluable}");
    }

    [Theory]
    [InlineData(null)]
    public void UnexpectedExpressionInputException(object input)
    {
        var parser = new Parser();
        Assert.Throws<UnexpectedExpressionInputException>(() => parser.Parse(input));
    }

    public static IEnumerable<object[]> UnexpectedOperandExceptionTestData()
    {
        yield return new object[] { new object[] { } };
    }

    [Theory]
    [MemberData(nameof(UnexpectedOperandExceptionTestData))]
    public void UnexpectedOperandException(object input)
    {
        var parser = new Parser();
        Assert.Throws<UnexpectedOperandException>(() => parser.Parse(input));
    }

    public static IEnumerable<object[]> UnexpectedExpressionExceptionTestData()
    {
        yield return new object[] { new object[] { "X", 1, 1 } };
        yield return new object[] { new object[] { 1, 1, 1 } };
    }

    [Theory]
    [MemberData(nameof(UnexpectedExpressionExceptionTestData))]
    public void UnexpectedExpressionException(object[] input)
    {
        var parser = new Parser();
        Assert.Throws<UnexpectedExpressionException>(() => parser.CreateExpression(input));
    }
}