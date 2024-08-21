using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class PrefixTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return [new Value("bo"), new Value("bogus"), true];
        yield return [new Value('b'), new Value("bogus"), true];
        yield return [new Value("b"), new Value('b'), true];
        // Falsy
        yield return [new Value("ogus"), new Value("bogus"), false];
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
        var expression = new Prefix(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}