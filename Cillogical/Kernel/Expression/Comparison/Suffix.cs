namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Suffix : ComparisonExpression
{
    public Suffix(IEvaluable left, IEvaluable right, string symbol = "SUFFIX") :
        base(
            "<with suffix>",
            symbol,
            (object?[] operands) => ComparisonExpression.IsText(operands[0]) && ComparisonExpression.IsText(operands[1])
                ? ((dynamic)operands[0].ToString()).EndsWith((dynamic)operands[1].ToString())
                : false,
            left,
            right
        ) { }
}