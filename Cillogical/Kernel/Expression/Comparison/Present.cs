namespace Cillogical.Kernel.Expression.Comparison;

public class Present : ComparisonExpression
{
    public Present(IEvaluable operand, string symbol = "PRESENT") :
        base("<is present>", symbol, (object?[] operands) => operands[0] is not null, operand) { }
}