using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class NotTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Value(false), true];
        // Falsy
        yield return [new Value(true), false];
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable operand, bool expected)
    {
        var expression = new Not(operand);
        Assert.Equal(expected, expression.Evaluate(null));
    }

    public static IEnumerable<object[]> EvaluateInvalidOperandTestData()
    {
        yield return [new Value(1)];
        yield return [new Value("bogus")];
    }

    [Theory]
    [MemberData(nameof(EvaluateInvalidOperandTestData))]
    public void EvaluateInvalidOperand(IEvaluable operand)
    {
        var expression = new Not(operand);
        Assert.Throws<InvalidExpressionException>(() => expression.Evaluate(null));
    }

    public static IEnumerable<object[]> SimplifyTestData()
    {
        yield return [new Value(false), true];
        yield return [new Value(true), false];
        yield return [new Reference("RefA"), false];
        yield return [new Reference("Missing"), new Not(new Reference("Missing"))];
        yield return [new Reference("invalid"), new Not(new Reference("invalid"))];
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable operand, object expected)
    {
        var expression = new Not(operand);
        var simplified = expression.Simplify(new Dictionary<string, object?> { { "RefA", true }, { "invalid", 1 } });

        if (expected is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        } 
    }
}