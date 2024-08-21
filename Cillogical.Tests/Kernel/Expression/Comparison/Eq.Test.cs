using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class EqTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Value(1), new Value(1), true];
        yield return [new Value(1.1), new Value(1.1), true];
        yield return [new Value(1.1f), new Value(1.1f), true];
        yield return [new Value("1"), new Value("1"), true];
        yield return [new Value(true), new Value(true), true];
        yield return [new Value(false), new Value(false), true];
        yield return [new Value(null), new Value(null), true];
        // Falsy
        yield return [new Value(1), new Value(1.1), false];
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
        var expression = new Eq(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}