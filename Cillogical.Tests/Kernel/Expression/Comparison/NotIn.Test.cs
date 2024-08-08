using Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel.Operand;
using Cillogical.Kernel;

namespace Cillogical.UnitTests.Kernel.Expression.Comparison;

public class NotInTest
{
    public static IEnumerable<object[]> EvaluateTestData()
    {
        // Truthy
        yield return new object[] { new Value(2), new Collection(new IEvaluable[] { new Value(0), new Value(1) }), true };
        yield return new object[] { new Collection(new IEvaluable[] { new Value(2) }), new Value(1), true };
        yield return new object[] { new Value("2"), new Collection(new IEvaluable[] { new Value("1") }), true };
        yield return new object[] { new Collection(new IEvaluable[] { new Value(true) }), new Value(false), true };
        yield return new object[] { new Value(null), new Collection(new IEvaluable[] { new Value(1) }), true };
        yield return new object[] { new Value(1), new Value(1), true };
        yield return new object[] { new Value(null), new Value(null), true };
        // Falsy
        yield return new object[] { new Value(1), new Collection(new IEvaluable[] { new Value(1) }), false };
        yield return new object[] { new Collection(new IEvaluable[] { new Value(1) }), new Value(1), false };
        yield return new object[] { new Value("bogus"), new Collection(new IEvaluable[] { new Value("bogus") }), false };
        yield return new object[] { new Collection(new IEvaluable[] { new Value(true) }), new Value(true), false };
        yield return new object[] { new Value(null), new Collection(new IEvaluable[] { new Value(null) }), false };
    }

    [Theory]
    [MemberData(nameof(EvaluateTestData))]
    public void Evaluate(IEvaluable left, IEvaluable right, bool expected)
    {
        var expression = new NotIn(left, right);
        Assert.Equal(expected, expression.Evaluate(null));
    }
}