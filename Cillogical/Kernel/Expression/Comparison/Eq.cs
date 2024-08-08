namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Eq : ComparisonExpression
{
    public Eq(IEvaluable left, IEvaluable right, string symbol = "==") :
        base("==", symbol, (object?[] operands) => object.Equals(operands[0], operands[1]), left, right) { }
}