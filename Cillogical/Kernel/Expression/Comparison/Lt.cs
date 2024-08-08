namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Lt : ComparisonExpression
{
    public Lt(IEvaluable left, IEvaluable right, string symbol = "<") :
        base(
            "<",
            symbol,
            (object?[] operands) => ComparisonExpression.IsNumber(operands[0]) && ComparisonExpression.IsNumber(operands[1])
                ? (dynamic)operands[0] < (dynamic)operands[1]
                : false,
            left,
            right
        ) { }
}