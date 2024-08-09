using Cillogical.Kernel.Expression.Logical;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Logical;

public class NotTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new Value(false), true };
        // Falsy
        yield return new object[] { new Value(true), false };
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
        yield return new object[] { new Value(1) };
        yield return new object[] { new Value("bogus") };
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
        yield return new object[] { new Value(false), true } ;
        yield return new object[] { new Value(true), false };
        yield return new object[] { new Reference("RefA"), false };
        yield return new object[] { new Reference("Missing"), new Not(new Reference("Missing")) };
    }

    [Theory]
    [MemberData(nameof(SimplifyTestData))]
    public void Simplify(IEvaluable operand, object expected)
    {
        var expression = new Not(operand);
        var simplified = expression.Simplify(new Dictionary<string, object> { { "RefA", true } });

        if (expected is IEvaluable) {
            Assert.Equal($"{expected}", $"{simplified}");
        } else {
            Assert.Equal(expected, simplified);
        } 
    }

    public static IEnumerable<object[]> SimplifyInvalidOperandTestData()
    {
        yield return new object[] { new Value(1) };
        yield return new object[] {  new Value("bogus") };
    }

    [Theory]
    [MemberData(nameof(SimplifyInvalidOperandTestData))]
    public void SimplifyInvalidOperand(IEvaluable operand)
    {
        var expression = new Not(operand);

        Assert.Throws<InvalidExpressionException>(() => expression.Simplify(null));
    }
}