namespace Cillogical.Kernel.Expression.Comparison;

public class Null : ComparisonExpression
{
    public Null(IEvaluable operand, string symbol = "NULL") :
        base("<is null>", symbol, (object?[] operands) => operands[0] is null, operand) { }
}