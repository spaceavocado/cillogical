using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class LeTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new Value(1), new Value(1), true };
        yield return new object[] { new Value(1.1), new Value(1.1), true };
        yield return new object[] { new Value(1.1f), new Value(1.1f), true };
        yield return new object[] { new Value(2), new Value(2), true };
        yield return new object[] { new Value(1.2), new Value(1.3), true };
        yield return new object[] { new Value(1.2f), new Value(1.3f), true };
        // Falsy
        yield return new object[] { new Value(1), new Value(0), false };
        yield return new object[] { new Value(1.0f), new Value(0.9f), false };
        yield return new object[] { new Value(1), new Value("1"), false };
        yield return new object[] { new Value(1), new Value(true), false };
        yield return new object[] { new Value(1.1), new Value("1"), false };
        yield return new object[] { new Value(1.1), new Value(true), false };
        yield return new object[] { new Value("1"), new Value(true), false };
        yield return new object[] { new Value(null), new Value(1), false };
        yield return new object[] { new Value(1), new Value(null), false };
        yield return new object[] { new Collection(new IEvaluable[] { new Value(1) }), new Collection(new IEvaluable[] { new Value(1) }), false };
        yield return new object[] { new Value(1), new Collection(new IEvaluable[] { new Value(1) }), false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable left, IEvaluable right, bool expected)
    {
        var expression = new Le(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}