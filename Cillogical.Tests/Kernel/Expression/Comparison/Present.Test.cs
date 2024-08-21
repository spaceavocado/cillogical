using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class PresentTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Value(1), true];
        yield return [new Value(1.1), true];
        yield return [new Value(1.1f), true];
        yield return [new Value("1"), true];
        yield return [new Value('c'), true];
        yield return [new Value(true), true];
        yield return [new Value(false), true];
        // Falsy
        yield return [new Value(null), false];
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable operand, bool expected)
    {
        var expression = new Present(operand);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}