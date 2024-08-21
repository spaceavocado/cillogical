namespace Cillogical.Kernel.Expression.Comparison;
public class Prefix : ComparisonExpression
{
    public Prefix(IEvaluable left, IEvaluable right, string symbol = "PREFIX") :
        base(
            "<prefixes>",
            symbol,
            (object?[] operands) => IsText(operands[0]) && IsText(operands[1])
                ? operands[1].ToString().StartsWith(operands[0].ToString())
                : false,
            left,
            right
        ) { }
}