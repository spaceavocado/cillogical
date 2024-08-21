namespace Cillogical.Kernel.Expression.Comparison;

public class Ne : ComparisonExpression
{
    public Ne(IEvaluable left, IEvaluable right, string symbol = "!=") :
        base("!=", symbol, (object?[] operands) => !Equals(operands[0], operands[1]), left, right) { }
}