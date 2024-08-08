namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Ge : ComparisonExpression
{
    public Ge(IEvaluable left, IEvaluable right, string symbol = ">=") :
        base(
            ">=",
            symbol,
            (object?[] operands) => ComparisonExpression.IsNumber(operands[0]) && ComparisonExpression.IsNumber(operands[1])
                ? (dynamic)operands[0] >= (dynamic)operands[1]
                : false,
            left,
            right
        ) { }
}