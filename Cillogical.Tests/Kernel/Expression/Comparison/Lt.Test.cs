using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class LtTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Value(1), new Value(2), true];
        yield return [new Value(1.2), new Value(1.3), true];
        yield return [new Value(1.2f), new Value(1.3f), true];
        // Falsy
        yield return [new Value(1), new Value(1), false];
        yield return [new Value(1.1), new Value(1.1), false];
        yield return [new Value(1.1f), new Value(1.1f), false];
        yield return [new Value(1), new Value(0), false];
        yield return [new Value(1.0f), new Value(0.9f), false];
        yield return [new Value(1), new Value("1"), false];
        yield return [new Value(1), new Value(true), false];
        yield return [new Value(1.1), new Value("1"), false];
        yield return [new Value(1.1), new Value(true), false];
        yield return [new Value("1"), new Value(true), false];
        yield return [new Value(null), new Value(1), false];
        yield return [new Value(1), new Value(null), false];
        yield return [new Collection([new Value(1)]), new Collection([new Value(1)]), false];
        yield return [new Value(1), new Collection([new Value(1)]), false];
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable left, IEvaluable right, bool expected)
    {
        var expression = new Lt(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}