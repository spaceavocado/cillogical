namespace Cillogical.Kernel.Expression.Comparison;

public class Suffix : ComparisonExpression
{
    public Suffix(IEvaluable left, IEvaluable right, string symbol = "SUFFIX") :
        base(
            "<with suffix>",
            symbol,
            (object?[] operands) => IsText(operands[0]) && IsText(operands[1])
                ? operands[0].ToString().EndsWith(operands[1].ToString())
                : false,
            left,
            right
        ) { }
}