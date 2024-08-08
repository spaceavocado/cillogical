namespace Cillogical.Kernel.Expression.Comparison;
using Cillogical.Kernel;

public class Prefix : ComparisonExpression
{
    public Prefix(IEvaluable left, IEvaluable right, string symbol = "PREFIX") :
        base(
            "<prefixes>",
            symbol,
            (object?[] operands) => ComparisonExpression.IsText(operands[0]) && ComparisonExpression.IsText(operands[1])
                ? ((dynamic)operands[1].ToString()).StartsWith((dynamic)operands[0].ToString())
                : false,
            left,
            right
        ) { }
}