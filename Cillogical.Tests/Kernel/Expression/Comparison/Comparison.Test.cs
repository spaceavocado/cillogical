using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

class ComparisionMock : ComparisonExpression
{
    public ComparisionMock(string op = "==", params IEvaluable[] operators) :
        base(op, op, (object?[] operands) => (operands[0] ?? new object { }).Equals(operands[1]), operators)
    { }
}

class RogueOperand : IEvaluable
{
    public object? Evaluate(Dictionary<string, object>? context) => throw new Exception();
    public object? Serialize() => throw new Exception();
    public object? Simplify(Dictionary<string, object>? context) => throw new Exception();
    public override string ToString() => throw new Exception();
}

public class ComparisionTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        yield return new object[] { new Value(1), new Value(1), true };
        yield return new object[] { new Value(1), new Value("1"), false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable left, IEvaluable right, bool expected)
    {
        var expression = new ComparisionMock("==", left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> EvaluateExceptionTestData()
    {
        yield return new object[] { new RogueOperand(), new RogueOperand() };
    }

    [Theory]
    [MemberData(nameof(EvaluateExceptionTestData))]
    public void EvaluateException(IEvaluable left, IEvaluable right)
    {
        var expression = new ComparisionMock("==", left, right);
        Assert.Equal(false, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> SerializeTestData()
    {
        yield return new object[] { "->", new object[] { "->", 1, 2 }, new Value(1), new Value(2) };
        yield return new object[] { "X", new object[] { "X", 1 }, new Value(1) };
    }

    [Theory]
    [MemberData(nameof(SerializeTestData))]
    public void Serialize(string op, object[] expected, params IEvaluable[] operators)
    {
        var expression = new ComparisionMock(op, operators);
        Assert.Equal(expected, expression.Serialize());
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return new object[] {
            new Value(0),
            new Reference("Missing"),
            new ComparisionMock("==", new Value(0), new Reference("Missing"))
        };
        yield return new object[] {
            new Reference("Missing"),
            new Value(0),
            new ComparisionMock("==", new Reference("Missing"), new Value(0))
        };
        yield return new object[] {
            new Reference("Missing"),
            new Reference("Missing"),
            new ComparisionMock("==", new Reference("Missing"), new Reference("Missing"))
        };
        yield return new object[] { new Value(0), new Value(0), true };
        yield return new object[] { new Value(0), new Value(1), false };
        yield return new object[] { new Value("A"), new Reference("RefA"), true };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable left, IEvaluable right, object expected)
    {
        var expression = new ComparisionMock("==", left, right);
        var simplified = expression.Simplify(new Dictionary<string, object> { { "RefA", "A"} });

        if (simplified is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        }
        
    }

    public static IEnumerable<object[]> StringifyTestData()
    {
        yield return new object[] { "==", "(1 == 2)", new Value(1), new Value(2), };
        yield return new object[] { "<null>", "(1 <null>)", new Value(1) };
    }

    [Theory]
    [MemberData(nameof(StringifyTestData))]
    public void Stringify(string op, string expected, params IEvaluable[] operators)
    {
        var expression = new ComparisionMock(op, operators);
        Assert.Equal(expected, $"{expression}");
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(1f, true)]
    [InlineData(1d, true)]
    [InlineData("1", false)]
    [InlineData('c', false)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    [InlineData(new object[] {}, false)]

    public void IsNumber(object? value, bool expected)
    {
        Assert.Equal(expected, ComparisonExpression.IsNumber(value));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData('c', true)]
    [InlineData(1, false)]
    [InlineData(1f, false)]
    [InlineData(1d, false)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(null, false)]
    [InlineData(new object[] { }, false)]

    public void IsText(object? value, bool expected)
    {
        Assert.Equal(expected, ComparisonExpression.IsText(value));
    }
}