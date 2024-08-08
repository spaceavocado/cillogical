using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class NullTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new Value(null), true };
        // Falsy
        yield return new object[] { new Value(1), false };
        yield return new object[] { new Value(1.1), false };
        yield return new object[] { new Value(1.1f), false };
        yield return new object[] { new Value("1"), false };
        yield return new object[] { new Value('c'), false };
        yield return new object[] { new Value(true), false };
        yield return new object[] { new Value(false), false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable operand, bool expected)
    {
        var expression = new Null(operand);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}