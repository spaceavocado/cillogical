using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class OverlapTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Collection([new Value(0)]), new Collection([new Value(0), new Value(1)]), true];
        yield return [new Collection([new Value(1)]), new Collection([new Value(1)]), true];
        yield return [new Collection([new Value(3), new Value(2)]), new Collection([new Value(1), new Value(2), new Value(3)]), true];
        yield return [new Collection([new Value("1")]), new Collection([new Value("1")]), true];
        yield return [new Collection([new Value(true)]), new Collection([new Value(true)]), true];
        yield return [new Collection([new Value(null)]), new Collection([new Value(null)]), true];
        yield return [new Value(1), new Value(1), true];
        yield return [new Collection([new Value(1)]), new Value(1), true];
        yield return [new Value(1), new Collection([new Value(1)]), true];
        // Falsy
        yield return [new Collection([new Value(0)]), new Collection([new Value(1)]), false];
        yield return [new Collection([new Value(1)]), new Collection([new Value(0)]), false];
        yield return [new Collection([new Value(false)]), new Value(true), false];
        yield return [new Value(null), new Collection([new Value(1)]), false];
        yield return [new Value(1), new Value(2), false];
        yield return [new Value(true), new Value(null), false];
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable left, IEvaluable right, bool expected)
    {
        var expression = new Overlap(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}